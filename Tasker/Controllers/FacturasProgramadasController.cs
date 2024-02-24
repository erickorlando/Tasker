using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Tasker.Dto;
using Tasker.Services;

namespace Tasker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasProgramadasController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly IRecurringJobManager _recurringJob;

        public FacturasProgramadasController(IBackgroundJobClient backgroundJob, IRecurringJobManager recurringJob)
        {
            _backgroundJob = backgroundJob;
            _recurringJob = recurringJob;
        }

        [HttpPost]
        public IActionResult Post(FacturaDto request, CancellationToken cancellationToken = default)
        {
            _backgroundJob.Schedule<IFacturaService>((service) => service.AddFacturaAsync(request, cancellationToken),
                TimeSpan.FromMinutes(1));

            return Ok(new { Mensaje = "Proceso correcto"});
        }

        [HttpPost("recurrente")]
        public IActionResult PostRecurring(FacturaDto request, CancellationToken cancellationToken = default)
        {
            _recurringJob.AddOrUpdate<IFacturaService>("Crear_Facturas", 
                service => service.AddFacturaAsync(request, cancellationToken),
                Cron.Daily(12));

            return Ok(new
            {
                Mensaje = "Tarea programada diariamente a las 11AM"
            });
        }
    }
}
