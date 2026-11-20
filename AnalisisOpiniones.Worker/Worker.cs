using AnalisisOpiniones.Infrastructure.Extractores;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace AnalisisOpiniones.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IHostApplicationLifetime lifetime,
        IConfiguration configuration)
    {
        _logger = logger;
        _lifetime = lifetime;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reloj = Stopwatch.StartNew();

        try
        {
            var carpeta = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            var salida = Path.Combine(carpeta, "DatosExtraidos");

            var rutaCsv = Directory.GetFiles(
                Path.Combine(carpeta, "Datos"),
                "surveys_part1.csv",
                SearchOption.AllDirectories).First();

            var conexion = _configuration.GetConnectionString("AnalisisOpiniones")
                ?? throw new Exception("No se encontro la conexion MySQL");

            var url = _configuration["Fuentes:ApiUrl"]
                ?? throw new Exception("No se encontro la URL de la API");

            var csv = new CsvExtractor(
                rutaCsv, Path.Combine(salida, "encuestas.json"));

            var mysql = new DatabaseExtractor(
                conexion, Path.Combine(salida, "resenas.json"));

            var api = new ApiExtractor(
                url, Path.Combine(salida, "comentarios_api.json"));

            var resultados = await Task.WhenAll(
                csv.ExtraerAsync(stoppingToken),
                mysql.ExtraerAsync(stoppingToken),
                api.ExtraerAsync(stoppingToken));

            _logger.LogInformation("CSV: {Total} registros", resultados[0]);
            _logger.LogInformation("MySQL: {Total} registros", resultados[1]);
            _logger.LogInformation("API: {Total} registros", resultados[2]);
            _logger.LogInformation("Total: {Total} registros", resultados.Sum());
            _logger.LogInformation("Tiempo: {Tiempo} segundos",
                reloj.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la extraccion");
        }
        finally
        {
            reloj.Stop();
            _lifetime.StopApplication();
        }
    }
}
