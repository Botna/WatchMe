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

        public Task UpdateBytesOffLoadedOfVideo(int id, long bytesOffloaded);
        public Task UpdateTotalBytesOfVideo(int id, long totalBytes);

        public Task UpdateStateOfVideos(VideoStates state, params int[] ids);
        public Task<int> DeleteVideosAsync(params Videos[] items);
    }

    public class VideosRepository : IVideosRepository
    {
        public async Task<List<Videos>> GetAllVideosAsync()
        {
            var database = GetConnection();
            var results = await database.Table<Videos>().ToListAsync();
            await database.CloseAsync();
            return results;
        }



        public async Task<Videos> GetVideosByVideoName(string videoName)
        {
            var database = GetConnection();
            var results = await database.Table<Videos>().Where(x => x.VideoName == videoName).FirstOrDefaultAsync();
            await database.CloseAsync();
            return results;
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
            await database.CloseAsync();
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
            await database.CloseAsync();
            return count;
        }

        public async Task<int> DeleteVideosAsync(params Videos[] items)
        {
            var database = GetConnection();
            var count = 0;
            foreach (var item in items)
            {
                await database.DeleteAsync(item);
                count++;
            }
            await database.CloseAsync();
            return count;
        }

        public virtual ISQLiteAsyncConnection GetConnection() =>
             new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        public async Task UpdateBytesOffLoadedOfVideo(int id, long bytesOffloaded)
        {
            var database = GetConnection();
            var record = await database.Table<Videos>().Where(x => x.Id == id).FirstOrDefaultAsync();
            record.BytesOffloaded = bytesOffloaded;
            await UpdateVideosAsync(record);
            await database.CloseAsync();
        }

        public async Task UpdateTotalBytesOfVideo(int id, long totalBytes)
        {
            var database = GetConnection();
            var record = await database.Table<Videos>().Where(x => x.Id == id).FirstOrDefaultAsync();
            record.TotalBytes = totalBytes;
            await UpdateVideosAsync(record);
            await database.CloseAsync();
        }

        public async Task UpdateStateOfVideos(VideoStates state, params int[] ids)
        {
            var database = GetConnection();

            var records = await GetVideosByIds(database, ids);
            foreach (var record in records)
            {
                record.VideoState = state.ToString();
            }
            await UpdateVideosAsync(records.ToArray());
            await database.CloseAsync();
        }

        private async Task<List<Videos>> GetVideosByIds(ISQLiteAsyncConnection database, int[] ids)
        {
            var results = await database.Table<Videos>().Where(x => ids.Contains(x.Id)).ToListAsync();
            return results;
        }
    }
}
