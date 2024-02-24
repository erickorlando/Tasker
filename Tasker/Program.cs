using Hangfire;
using Microsoft.EntityFrameworkCore;
using Tasker;
using Tasker.DataAccess;
using Tasker.Dto;
using Tasker.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("TaskerDb"));
});

// Add services to the container.

builder.Services.AddDbContext<TaskerDbContext>(config =>
{
    config.UseSqlServer(builder.Configuration.GetConnectionString("TaskerDb"));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IFacturaService, FacturaService>();

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("api/Tarea", (IBackgroundJobClient backgroundJob, string nombre) =>
{
    backgroundJob.Enqueue(() => Console.WriteLine($"Se crea la tarea programada para {nombre}"));
});

app.MapPost("api/TareaProgramada", (IBackgroundJobClient backgroundJob) =>
{
    backgroundJob.Schedule(() => Console.WriteLine($"Tarea programada a las {DateTime.Now}"),
        TimeSpan.FromSeconds(20));
});

app.MapPost("api/TareaConReintentos", (IBackgroundJobClient backgroundJob) =>
{
    backgroundJob.Schedule(() => TareaProgramada.Ejecutar(), TimeSpan.FromSeconds(30));
});

using (var scope = app.Services.CreateScope())
{
    var recurringJob = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJob.AddOrUpdate<IFacturaService>("Crear_Facturas",
        service => service.AddFacturaAsync(new FacturaDto("FacturaProgramada", 100), default),
        Cron.Daily(13));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

app.Run();
