using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Tasker.Dto;
using Tasker.Services;

namespace Tasker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasController : ControllerBase
    {
        private readonly IFacturaService _service;
        private readonly IBackgroundJobClient _backgroundJob;

        public FacturasController(IFacturaService service, IBackgroundJobClient backgroundJob)
        {
            _service = service;
            _backgroundJob = backgroundJob;
        }

        [HttpPost]
        public async Task<IActionResult> Post(FacturaDto request, CancellationToken cancellationToken = default)
        {
            await _service.AddFacturaAsync(request, cancellationToken);

            return Ok();
        }

        [HttpPost("Programado")]
        public async Task<IActionResult> PostProgramador(FacturaDto request,
            CancellationToken cancellationToken = default)
        {
            _backgroundJob.Schedule(() => _service.AddFacturaAsync(request, cancellationToken),
                TimeSpan.FromSeconds(30));

            return Ok(await Task.FromResult(new
            {
                Respuesta = "Se creó la tarea programada"
            }));
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var data = await _service.ListAsync(cancellationToken);

            return Ok(data);
        }
    }
}
