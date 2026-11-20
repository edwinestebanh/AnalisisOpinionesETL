using AnalisisOpiniones.Core.Interfaces;
using CsvHelper;
using System.Globalization;
using System.Text.Json;

namespace AnalisisOpiniones.Infrastructure.Extractores;

public class CsvExtractor : IExtractor
{
    private readonly string _rutaCsv;
    private readonly string _rutaSalida;

    public string Nombre => "CSV";

    public CsvExtractor(string rutaCsv, string rutaSalida)
    {
        _rutaCsv = rutaCsv;
        _rutaSalida = rutaSalida;
    }

    public async Task<int> ExtraerAsync(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(_rutaCsv);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var registros = new List<dynamic>();

        await foreach (var registro in csv.GetRecordsAsync<dynamic>(cancellationToken))
        {
            registros.Add(registro);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_rutaSalida)!);

        await File.WriteAllTextAsync(
            _rutaSalida,
            JsonSerializer.Serialize(registros),
            cancellationToken);

        return registros.Count;
    }
}
