using AnalisisOpiniones.Worker;
using AnalisisOpiniones.Infrastructure.Cargas;

var cargarDimensiones = args.Contains("--dimensiones");

var builder = Host.CreateApplicationBuilder(
    args.Where(a => a != "--dimensiones").ToArray());

if (cargarDimensiones)
{
    var conexion = builder.Configuration.GetConnectionString("AnalisisOpiniones")
        ?? throw new Exception("No se encontro la conexion MySQL");

    await new CargaDimensiones().EjecutarAsync(conexion);
    return;
}

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
