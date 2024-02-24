using Tasker.Dto;
using Tasker.Entities;

namespace Tasker.Services
{
    public interface IPastelesService
    {
        Task<BaseResponse> AddPasteles(string carpeta, CancellationToken cancellationToken = default);

        Task<ICollection<Pastel>> ListAsync(CancellationToken cancellationToken = default);
    }
}
