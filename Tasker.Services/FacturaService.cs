using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasker.DataAccess;
using Tasker.Dto;
using Tasker.Entities;

namespace Tasker.Services
{
    public class FacturaService : IFacturaService
    {
        private readonly TaskerDbContext _context;
        private readonly ILogger<FacturaService> _logger;

        public FacturaService(TaskerDbContext context, ILogger<FacturaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddFacturaAsync(FacturaDto request, CancellationToken cancellationToken = default)
        {
            try
            {
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

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error al grabar {Message}", ex.Message);
            }
        }
    }
}
