namespace salesngin.Services.Implementations;

public sealed class PhotoStorageService : IPhotoStorage
{
    private readonly IWebHostEnvironment _env;

    public PhotoStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public (string FilePath, string StoredName, string PublicUrl) Process(
        IFormFile photo, string entityName, string directory, string directoryName, string key, string existingUrl = null)
    {
        if (photo is null) return (string.Empty, null, existingUrl);
        //string directoryPath = $"/{directoryName}/";
        //var uploadsFolder = Path.Combine(_env.WebRootPath, directoryPath);
        var uploadsFolder = Path.Combine(_env.WebRootPath, directoryName);
        //var uploadsFolder = Path.Combine(_env.WebRootPath, directory);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(photo.FileName);
        //var storedName = $"{entityName}_{key}_{Guid.NewGuid():N}{extension}";
        var storedName = $"{key}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, storedName);

        //var publicUrl = $"/{directoryName}/{storedName}"; // e.g. /itemsphotos/file.jpg
        var publicUrl = $"{directory}/{storedName}"; // e.g. /itemsphotos/file.jpg

        return (filePath, storedName, publicUrl);
    }

    public async Task SaveAsync(IFormFile photo, string filePath, CancellationToken ct = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Create);
        await photo.CopyToAsync(stream, ct);
    }

    public List<(string FilePath, string StoredName, string PublicUrl)> ProcessMultiple(
        IEnumerable<IFormFile> photos, string entityName, string directory, string directoryName, string key, int maxCount = 5)
    {
        if (photos is null) return [];

        var limited = photos.Take(maxCount);
        var results = new List<(string, string, string)>();

        int index = 1;
        foreach (var photo in limited)
        {
            var ext = Path.GetExtension(photo.FileName);
            var storedName = $"{entityName}_{key}_{index}_{Guid.NewGuid():N}{ext}";
            var uploadsFolder = Path.Combine(_env.WebRootPath, directory);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, storedName);
            var publicUrl = $"/{directoryName}/{storedName}";

            results.Add((filePath, storedName, publicUrl));
            index++;
        }

        return results;
    }

    public async Task SaveMultipleAsync(IEnumerable<(IFormFile Photo, string FilePath)> files, CancellationToken ct = default)
    {
        foreach (var (photo, filePath) in files)
        {
            await using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream, ct);
        }
    }

}

