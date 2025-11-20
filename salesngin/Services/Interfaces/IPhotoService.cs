namespace salesngin.Services.Interfaces;

public interface IPhotoStorage
{
    // /// Processes the inbound photo and returns (filePathToWrite, storedFileName, publicUrlIfAny)
    // (string FilePath, string StoredName, string PublicUrl) Process(
    //     IFormFile photo, string entityName, string directory, string directoryName, string key, string existingUrl = null);

    // Task SaveAsync(IFormFile photo, string filePath, CancellationToken ct = default);

    /// Process a single inbound photo and return info (file path to write, stored file name, public URL if any).
    (string FilePath, string StoredName, string PublicUrl) Process(
        IFormFile photo, string entityName, string directory, string directoryName, string key, string existingUrl = null);

    /// Actually saves a single file to disk (or cloud).
    Task SaveAsync(IFormFile photo, string filePath, CancellationToken ct = default);

    /// Process multiple photos (up to maxCount) and return info for each.
    List<(string FilePath, string StoredName, string PublicUrl)> ProcessMultiple(
        IEnumerable<IFormFile> photos, string entityName, string directory, string directoryName, string key, int maxCount = 5);

    /// Save multiple photos asynchronously.
    Task SaveMultipleAsync(IEnumerable<(IFormFile Photo, string FilePath)> files, CancellationToken ct = default);

}

