using WatchMe.Repository;

namespace WatchMe.Persistance.Implementations
{
    public abstract class BaseFileSystemService : IFileSystemService
    {
        public abstract FileStream GetFileStreamOfFile(string filename);

        public abstract bool SaveVideoToFileSystem(byte[] videoBytes, string fileName);

        public async Task<byte[]?> GetVideoBytesByFile(string filePath) =>
            await File.ReadAllBytesAsync(filePath);
    }
}
