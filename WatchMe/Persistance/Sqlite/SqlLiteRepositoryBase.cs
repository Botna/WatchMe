using SQLite;

namespace WatchMe.Persistance.Sqlite
{
    public abstract class SqlLiteRepositoryBase<T>
    {
        public async Task<int> InsertItemsAsync(params T[] items)
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

        public async Task<int> UpdateItemsAsync(params T[] items)
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

        public async Task<int> DeleteItemsAsync(params T[] items)
        {
            var database = GetConnection();
            return await database.DeleteAsync(items);
        }

        public virtual ISQLiteAsyncConnection GetConnection() =>
            new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
    }
}
