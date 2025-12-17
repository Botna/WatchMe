namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        Task<byte[]> MoveVideoToGallery(string fileName);
        FileStream GetFileStreamOfFile(string filePath);
        Task<byte[]?> GetAllFileBytesFromCacheDirectory(string fileName);
        string BuildCacheFileDirectory(string fileName);
        byte[]? GetFileBytesFromCacheDirectory(string fileName, long byteOffset);
    }
}
