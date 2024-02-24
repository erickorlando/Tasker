using Tasker.Dto;

namespace Tasker.Services
{
    public interface IFacturaService
    {
        Task AddFacturaAsync(FacturaDto request, CancellationToken cancellationToken = default);
    }
}
