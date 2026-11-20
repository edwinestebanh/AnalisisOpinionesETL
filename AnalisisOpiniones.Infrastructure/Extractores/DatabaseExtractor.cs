using AnalisisOpiniones.Core.Interfaces;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AnalisisOpiniones.Infrastructure.Extractores;

public class DatabaseExtractor : IExtractor
{
    private readonly string _conexion;
    private readonly string _rutaSalida;

    public string Nombre => "MySQL";

    public DatabaseExtractor(string conexion, string rutaSalida)
    {
        _conexion = conexion;
        _rutaSalida = rutaSalida;
    }

    public async Task<int> ExtraerAsync(CancellationToken cancellationToken)
    {
        var registros = new List<Dictionary<string, object?>>();

        await using var conexion = new MySqlConnection(_conexion);
        await conexion.OpenAsync(cancellationToken);

        await using var comando = new MySqlCommand(
            "SELECT * FROM resenas_web", conexion);

        await using var lector = await comando.ExecuteReaderAsync(cancellationToken);

        while (await lector.ReadAsync(cancellationToken))
        {
            var registro = new Dictionary<string, object?>();

            for (int i = 0; i < lector.FieldCount; i++)
            {
                registro[lector.GetName(i)] =
                    lector.IsDBNull(i) ? null : lector.GetValue(i);
            }

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
