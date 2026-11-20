using System.Threading;
using System.Threading.Tasks;

namespace AnalisisOpiniones.Core.Interfaces;

public interface IExtractor
{
    string Nombre { get; }

    Task<int> ExtraerAsync(CancellationToken cancellationToken);
}
