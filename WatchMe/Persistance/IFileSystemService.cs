namespace WatchMe.Repository
{
    public interface IFileSystemService
    {
        bool SaveVideoToFileSystem(byte[] videoBytes, string filePath);
        FileStream GetFileStreamOfFile(string filePath);
        Task<byte[]?> GetVideoBytesByFile(string filePath);

        byte[]? GetVideoBytesByFile(string filePath, int byteOffset);

        byte[]? GetVideoBytesByFile(string filePath, int byteOffset, int numBytes);
        string BuildCacheFileDirectory(string fileName);
    }
}
