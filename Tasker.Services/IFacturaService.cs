using Tasker.Dto;
using Tasker.Entities;

namespace Tasker.Services
{
    public interface IFacturaService
    {
        Task AddFacturaAsync(FacturaDto request, CancellationToken cancellationToken = default);

        Task<ICollection<Factura>> ListAsync(CancellationToken cancellationToken = default);
    }
}
