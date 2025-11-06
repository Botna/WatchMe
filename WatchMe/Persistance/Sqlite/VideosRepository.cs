using SQLite;
using WatchMe.Persistance.Sqlite.Tables;

namespace WatchMe.Persistance.Sqlite
{
    public interface IVideosRepository
    {
        public Task<List<Videos>> GetAllVideosAsync();
        public Task<Videos> GetVideosByVideoName(string videoName);
        public Task<int> InsertVideosAsync(params Videos[] items);
        public Task<int> UpdateVideosAsync(params Videos[] items);
        public Task<int> DeleteVideosAsync(params Videos[] items);
    }

    public class VideosRepository : IVideosRepository
    {
        public async Task<List<Videos>> GetAllVideosAsync()
        {
            var database = GetConnection();
            return await database.Table<Videos>().ToListAsync();
        }

        public async Task<Videos> GetVideosByVideoName(string videoName)
        {
            var database = GetConnection();
            return await database.Table<Videos>().Where(x => x.VideoName == videoName).FirstOrDefaultAsync();
        }

        public async Task<int> InsertVideosAsync(params Videos[] items)
        {
            var database = GetConnection();
            var count = 0;
            foreach (var item in items)
            {
                await database.InsertAsync(item);
                count++;
            }
            return count;
        }

        public async Task<int> UpdateVideosAsync(params Videos[] items)
        {
            var database = GetConnection();
            var count = 0;
            foreach (var item in items)
            {
                await database.UpdateAsync(item);
                count++;
            }
            return count;
        }

        public async Task<int> DeleteVideosAsync(params Videos[] items)
        {
            var database = GetConnection();
            return await database.DeleteAsync(items);
        }

        public virtual ISQLiteAsyncConnection GetConnection() =>
             new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
    }
}
