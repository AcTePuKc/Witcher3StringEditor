using System.Threading;
using System.Threading.Tasks;

namespace Witcher3StringEditor.Common.QualityAssurance;

public interface IQAStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<QualityAssuranceEntry?> FindAsync(QualityAssuranceQuery query, CancellationToken cancellationToken = default);

    Task SaveAsync(QualityAssuranceEntry entry, CancellationToken cancellationToken = default);
}
