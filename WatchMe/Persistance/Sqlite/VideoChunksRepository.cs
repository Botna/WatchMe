using WatchMe.Persistance.Sqlite.Tables;

namespace WatchMe.Persistance.Sqlite
{
    public interface IVideoChunksRepository
    {
        public Task<List<VideoChunks>> GetVideoChunksAsync();
        public Task<int> InsertVideoChunksAsync(params VideoChunks[] items);

        public Task<int> UpdateVideoChunksAsync(params VideoChunks[] items);

        public Task<int> DeleteVideoChunksAsync(params VideoChunks[] items);
    }

    public class VideoChunksRepository : IVideoChunksRepository
    {
        public Task<int> DeleteVideoChunksAsync(params VideoChunks[] items)
        {
            throw new NotImplementedException();
        }

        public Task<List<VideoChunks>> GetVideoChunksAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertVideoChunksAsync(params VideoChunks[] items)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateVideoChunksAsync(params VideoChunks[] items)
        {
            throw new NotImplementedException();
        }
    }
}
