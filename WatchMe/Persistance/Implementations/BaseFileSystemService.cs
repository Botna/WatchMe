using WatchMe.Repository;

namespace WatchMe.Persistance.Implementations
{
    public abstract class BaseFileSystemService : IFileSystemService
    {
        public abstract FileStream GetFileStreamOfFile(string filename);

        public abstract bool SaveVideoToFileSystem(byte[] videoBytes, string fileName);

        public async Task<byte[]?> GetVideoBytesByFile(string filePath) =>
            await File.ReadAllBytesAsync(filePath);

        //FileSystem.Current.* doesnt work in unit tests, so must be mockable.
        public string BuildCacheFileDirectory(string fileName) =>
            Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        public async Task<byte[]?> GetVideoBytesByFile(string filePath, int byteOffset)
        {
            //Something is mad here, we aren't able to recombine these files together. 

            //Need to check hex values probably of very small broken apart files to make sure they are getting pushed backtogether correctly.


            var adjustedByteOffset = byteOffset;
            if (byteOffset > 0)
            {
                //we are on subsequent reads, meaning we have already read the byte we are seeking);
                adjustedByteOffset++;
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {

                byte[] buffer = new byte[4096];

                using (MemoryStream ms = new MemoryStream())
                {
                    int bytesRead;
                    fileStream.Seek(adjustedByteOffset, SeekOrigin.Begin);
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
