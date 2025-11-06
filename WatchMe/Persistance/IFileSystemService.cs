namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        Task<byte[]> MoveVideoToGallery(string fileName);
        FileStream GetFileStreamOfFile(string filePath);
        Task<byte[]?> GetAllFileBytesFromCacheDirectory(string filePath);
        string BuildCacheFileDirectory(string fileName);
    }
}
