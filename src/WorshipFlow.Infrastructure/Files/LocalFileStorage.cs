using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WorshipFlow.Application.Abstractions;

namespace WorshipFlow.Infrastructure.Files;

public sealed class LocalFileStorage(IWebHostEnvironment environment) : IFileStorage
{
    public async Task<StoredFile> SaveAsync(IFormFile file, Guid tenantId, string category, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var relative = Path.Combine("uploads", tenantId.ToString("N"), category, storedName).Replace('\\', '/');
        var full = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var stream = File.Create(full);
        await file.CopyToAsync(stream, cancellationToken);
        return new StoredFile(file.FileName, storedName, file.ContentType, file.Length, "/" + relative);
    }
}
