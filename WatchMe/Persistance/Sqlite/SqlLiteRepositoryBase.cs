using SQLite;
using WatchMe.Persistance.Sqlite.Tables;

namespace WatchMe.Persistance.Sqlite
{
    public abstract class SqlLiteRepositoryBase<T>
    {
        protected SQLiteAsyncConnection database;

        public async Task Init()
        {
            //Todo move into its own separate initialization that we can fine tune later.
            if (database is not null)
                return;
            database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            var tableTypes = new List<Type>()
            {
                typeof(VideoChunks),
                typeof(Videos)
            };

            var result = await database.CreateTablesAsync(CreateFlags.ImplicitIndex, tableTypes.ToArray());
        }

        //public async Task<int> InsertItemAsync(T item)
        //{
        //    await Init();
        //    return await database.InsertAsync(item);
        //}

        public async Task<int> InsertItemsAsync(params T[] items)
        {
            await Init();
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
            await Init();
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
            await Init();
            return await database.DeleteAsync(items);
        }
    }
}
