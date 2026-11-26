using MySqlConnector;

namespace AnalisisOpiniones.Infrastructure.Cargas;

public class CargaDimensiones
{
    public async Task EjecutarAsync(string cadena)
    {
        await using var conexion = new MySqlConnection(cadena);
        await conexion.OpenAsync();

        var consultas = new Dictionary<string, string>
        {
            ["clientes"] = """
                insert into opiniones_clientes.clientes (id_cliente, nombre)
                select idcliente, trim(nombre)
                from analisisopiniones.clientes
                on duplicate key update nombre = values(nombre)
                """,

            ["productos"] = """
                insert into opiniones_clientes.productos (id_producto, nombre, categoria)
                select idproducto, left(trim(nombre), 100), categoria
                from analisisopiniones.productos
                on duplicate key update
                nombre = values(nombre), categoria = values(categoria)
                """,

            ["fechas"] = """
                insert into opiniones_clientes.fechas (fecha, dia, mes, trimestre, anio)
                select fecha, day(fecha), month(fecha), quarter(fecha), year(fecha)
                from (
                    select fecha from analisisopiniones.opiniones_encuestas
                    union
                    select fecha from analisisopiniones.comentarios_redes
                    union
                    select fecha from analisisopiniones.resenas_web
                ) as datos
                on duplicate key update fecha = values(fecha)
                """,

            ["fuentes"] = """
                insert into opiniones_clientes.fuentes (nombre)
                select nombre from (
                    select trim(tipofuente) as nombre
                    from analisisopiniones.fuente_datos
                    union
                    select trim(fuente)
                    from analisisopiniones.opiniones_encuestas
                    union
                    select trim(fuente)
                    from analisisopiniones.comentarios_redes
                    union
                    select 'Resenas web'
                    from analisisopiniones.resenas_web
                ) as datos
                where nombre is not null and nombre <> ''
                on duplicate key update nombre = values(nombre)
                """,

            ["sentimientos"] = """
                insert into opiniones_clientes.sentimientos (nombre)
                select nombre from (
                    select distinct trim(clasificacion) as nombre
                    from analisisopiniones.opiniones_encuestas
                    union
                    select 'Sin clasificar'
                ) as datos
                where nombre is not null and nombre <> ''
                on duplicate key update nombre = values(nombre)
                """,

            ["eventos"] = """
                insert into opiniones_clientes.eventos (nombre, descripcion)
                select 'Sin evento', 'Registro sin evento asociado'
                where not exists (
                    select 1 from opiniones_clientes.eventos
                    where nombre = 'Sin evento'
                )
                """
        };

        await using var transaccion = await conexion.BeginTransactionAsync();

        foreach (var consulta in consultas)
        {
            await using var comando = new MySqlCommand(
                consulta.Value, conexion, transaccion);

            await comando.ExecuteNonQueryAsync();
        }

        await transaccion.CommitAsync();

        Console.WriteLine("====================================");
        Console.WriteLine("CARGA DE DIMENSIONES DATA WAREHOUSE");
        Console.WriteLine("====================================");

        foreach (var tabla in consultas.Keys)
        {
            await using var comando = new MySqlCommand(
                $"select count(*) from opiniones_clientes.{tabla}", conexion);

            var total = await comando.ExecuteScalarAsync();

            Console.WriteLine($"{tabla}: {total} registros");
        }

        Console.WriteLine("Carga completada correctamente");
    }
}
