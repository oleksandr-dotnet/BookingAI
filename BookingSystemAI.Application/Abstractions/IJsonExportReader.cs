using BookingSystemAI.Application.Migration;

namespace BookingSystemAI.Application.Abstractions;

public interface IJsonExportReader
{
    Task<CompanyExportDto> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
