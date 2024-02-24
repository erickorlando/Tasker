using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Tasker.DataAccess;
using Tasker.Dto;
using Tasker.Entities;

namespace Tasker.Services
{
    public class PastelesService : IPastelesService
    {
        private readonly ILogger<PastelesService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TaskerDbContext _context;

        public PastelesService(ILogger<PastelesService> logger, IConfiguration configuration, TaskerDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<BaseResponse> AddPasteles(string carpeta, CancellationToken cancellationToken = default)
        {
            var policy = Policy<BaseResponse>.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: 3,
                    intento =>
                    {
                        _logger.LogWarning("--Intentando de nuevo, intento N° {intento}", intento);
                        return TimeSpan.FromSeconds(3 * intento);
                    }, onRetry: (exception, tiempo, _) =>
                    {
                        _logger.LogWarning("Error capturado: {Message}", exception.Exception.Message);
                        _logger.LogWarning("Duracion del intento: {tiempo}", tiempo);
                    });

            var policyResponse = new BaseResponse();

            policyResponse = await policy.ExecuteAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(carpeta))
                    {
                        throw new ApplicationException("No se especifico la carpeta");
                    }

                    if (!Directory.Exists(carpeta))
                        throw new ApplicationException($"No se encontró la carpeta {carpeta}");

                    var archivos = Directory.GetFiles(carpeta, "*.jpg");

                    // Nos conectamos a Azure Blob Storage
                    var client = new BlobServiceClient(_configuration["StorageConfiguration:AzureKey"]);

                    var contenedor = client.GetBlobContainerClient("pasteles");

                    _logger.LogInformation("Conectando a Azure");

                    foreach (var archivo in archivos)
                    {
                        var info = new FileInfo(archivo);

                        var pastel = new Pastel()
                        {
                            Nombre = info.Name.Replace(".jpg", string.Empty)
                        };

                        var bytesDelArchivo = await File.ReadAllBytesAsync(archivo, cancellationToken);

                        _logger.LogInformation("Lectura del archivo {archivo}", archivo);

                        var blob = contenedor.GetBlobClient(info.Name);

                        await using var stream = new MemoryStream(bytesDelArchivo);
                        await blob.UploadAsync(stream, overwrite: true, cancellationToken);

                        _logger.LogInformation("Se subio el archivo a Azure");

                        pastel.UrlImagen = $"{_configuration["StorageConfiguration:PublicUrl"]}/{info.Name}";

                        await _context.Pasteles.AddAsync(pastel, cancellationToken);
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Se proceso todo correctamente");

                    policyResponse.Success = true;

                }
                catch (ApplicationException ex)
                {
                    policyResponse.Message = ex.Message;
                }

                return policyResponse;

            });

            return policyResponse;
        }

        public async Task<ICollection<Pastel>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _context
                .Set<Pastel>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
