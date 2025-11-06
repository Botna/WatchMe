using WatchMe.Persistance.Sqlite.Tables;

namespace WatchMe.Persistance.Sqlite
{

    public class VideosRepository : SqlLiteRepositoryBase<Videos>
    {
        public async Task<List<Videos>> GetItemsAsync()
        {
            var database = GetConnection();
            return await database.Table<Videos>().ToListAsync();
        }

        public async Task<Videos> GetVideosByVideoName(string videoName)
        {
            var database = GetConnection();
            return await database.Table<Videos>().Where(x => x.VideoName == videoName).FirstOrDefaultAsync();
        }
    }
}
