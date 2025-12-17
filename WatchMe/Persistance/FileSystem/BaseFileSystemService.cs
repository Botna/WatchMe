using WatchMe.Repository;

namespace WatchMe.Persistance.Implementations
{
    public abstract class BaseFileSystemService : IFileSystemService
    {
        public FileStream GetFileStreamOfFile(string fileName)
        {
            return new FileStream(BuildCacheFileDirectory(fileName), FileMode.Open);
        }

        public abstract Task<byte[]> MoveVideoToGallery(string fileName);

        public async Task<byte[]?> GetAllFileBytesFromCacheDirectory(string fileName)
        {
            return await File.ReadAllBytesAsync(BuildCacheFileDirectory(fileName));
        }

        public byte[]? GetFileBytesFromCacheDirectory(string fileName, long byteOffset)
        {
            var filePath = BuildCacheFileDirectory(fileName);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var currentMax = fileStream.Length;
                byte[] buffer = new byte[4096];

                using (MemoryStream ms = new MemoryStream())
                {
                    int bytesRead;
                    fileStream.Seek(byteOffset, SeekOrigin.Current);
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    return ms.ToArray();
                }
            }
        }

        public string BuildCacheFileDirectory(string fileName) =>
            Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        #region GetBytesByChunk
        //public byte[]? GetVideoBytesByFile(string filePath, int byteOffset)
        //{

        //    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //    {

        //        byte[] buffer = new byte[4096];

        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int bytesRead;
        //            fileStream.Seek(byteOffset, SeekOrigin.Current);
        //            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        //            {
        //                ms.Write(buffer, 0, bytesRead);
        //            }
        //            return ms.ToArray();
        //        }
        //    }
        //}

        //public byte[]? GetVideoBytesByFile(string filePath, int byteOffset, int numBytes)
        //{
        //    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //    {

        //        byte[] buffer = new byte[4096];

        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int bytesRead;
        //            var totalBytes = 0;
        //            fileStream.Seek(byteOffset, SeekOrigin.Current);

        //            while (totalBytes < numBytes)
        //            {
        //                bytesRead = fileStream.Read(buffer, 0, buffer.Length);

        //                ms.Write(buffer, 0, bytesRead);

        //                if (bytesRead < 4096)
        //                {
        //                    break;
        //                }
        //                totalBytes += bytesRead;
        //            }
        //            return ms.ToArray();
        //        }
        //    }
        //}

        #endregion
    }
}
