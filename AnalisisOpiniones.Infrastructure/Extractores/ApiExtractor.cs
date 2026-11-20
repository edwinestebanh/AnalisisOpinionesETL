using AnalisisOpiniones.Core.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AnalisisOpiniones.Infrastructure.Extractores;

public class ApiExtractor : IExtractor
{
    private readonly string _url;
    private readonly string _rutaSalida;

    public string Nombre => "API REST";

    public ApiExtractor(string url, string rutaSalida)
    {
        _url = url;
        _rutaSalida = rutaSalida;
    }

    public async Task<int> ExtraerAsync(CancellationToken cancellationToken)
    {
        using var cliente = new HttpClient();

        var json = await cliente.GetStringAsync(_url, cancellationToken);

        using var documento = JsonDocument.Parse(json);

        var total = documento.RootElement.GetArrayLength();

        Directory.CreateDirectory(Path.GetDirectoryName(_rutaSalida)!);

        await File.WriteAllTextAsync(
            _rutaSalida,
            json,
            cancellationToken);

        return total;
    }
}
