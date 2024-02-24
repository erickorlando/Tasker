using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Tasker.DataAccess;
using Tasker.Dto;
using Tasker.Entities;

namespace Tasker.Services
{
    public class FacturaService : IFacturaService
    {
        private readonly TaskerDbContext _context;
        private readonly ILogger<FacturaService> _logger;
        private readonly AsyncRetryPolicy<ICollection<Factura>> _policy;
        private readonly AsyncRetryPolicy _politicaPausada;

        public FacturaService(TaskerDbContext context, ILogger<FacturaService> logger)
        {
            _context = context;
            _logger = logger;

            // Politica de reintentos con Polly
            _policy = Policy<ICollection<Factura>>.Handle<InvalidOperationException>()
                .RetryAsync(3, onRetry: (exception, retryCount, _) =>
                {
                    _logger.LogWarning("---Intentando de nuevo, intento N° {retryCount} ---", retryCount);
                    _logger.LogWarning("Error capturado: {Message}", exception.Exception.Message);
                });

            _politicaPausada = Policy.Handle<InvalidOperationException>()
                .WaitAndRetryAsync(retryCount: 5,
                    intento =>
                    {
                        _logger.LogWarning("--Intentando de nuevo, intento N° {intento}", intento);
                        return TimeSpan.FromSeconds(5 * intento);
                    }, onRetry: (exception, tiempo, _) =>
                    {
                        _logger.LogWarning("Error capturado: {Message}", exception.Message);
                        _logger.LogWarning("Duracion del intento: {tiempo}", tiempo);
                    });
        }

        public async Task<ICollection<Factura>> ListAsync(CancellationToken cancellationToken = default)
        {
            var rnd = new Random();
            var data = await _policy.ExecuteAsync(async () =>
            {
                if (rnd.Next(1, 3) == 1) throw new InvalidOperationException("Error en la base de datos");

                var query = await _context.Set<Factura>()
                    .OrderByDescending(p => p.NumeroFactura)
                    .ToListAsync(cancellationToken);

                return query;
            });

            return data;
        }

        public async Task AddFacturaAsync(FacturaDto request, CancellationToken cancellationToken = default)
        {
            var rnd = new Random();
            await _politicaPausada.ExecuteAsync(async () =>
            {
                if (rnd.Next(1, 3) == 1) throw new InvalidOperationException("Error en la base de datos");

                var factura = new Factura
                {
                    NombreCliente = request.Nombre,
                    TotalVenta = request.Total
                };

                var numerador = await _context.Set<Numerador>()
                    .FirstOrDefaultAsync(p => p.Tabla == nameof(Factura), cancellationToken);

                var numeroFactura = numerador is not null
                    ? (numerador.UltimoNumero + 1).ToString("000000")
                    : 1.ToString("000000");

                factura.NumeroFactura = $"FAAA-{numeroFactura}";

                await _context.Set<Factura>().AddAsync(factura, cancellationToken);

                if (numerador is not null)
                {
                    // esto va a obligar al EF Core que haga un UPDATE a la tabla
                    numerador.UltimoNumero++;
                }
                else
                {
                    // aca hace un INSERT en la tabla numerador
                    await _context.Set<Numerador>().AddAsync(new Numerador()
                    {
                        Tabla = nameof(Factura),
                        UltimoNumero = 7800
                    }, cancellationToken);
                }

                // Estamos procesando mas de un registro al guardar
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Todo salió bien :-)");
            });

        }


    }
}
