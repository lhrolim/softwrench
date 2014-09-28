using System.Net;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Metadata;

namespace softWrench.Mobile.Persistence
{
    internal class UserRepository
    {
        public Task<User> LoadAsync()
        {
            return Database
                .GetConnection(new CancellationToken())
                .Table<User>()
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(User user)
        {
            var connection = Database.GetConnection(new CancellationToken());

            var exists = null != await connection
                .FindAsync<User>(user.UserName);

            if (exists)
            {
                return await connection.UpdateAsync(user);
            }

            //TODO: for now we're a single-user app.
            await connection.DeleteAsync(user);
            return await connection.InsertAsync(user);
        }

        public async Task<CookieCollection> LoadCookieCollectionAsync()
        {
            var result = await Database
                .GetConnection(new CancellationToken())
                .Table<PersistableCookieCollection>()
                .FirstOrDefaultAsync();

            return null == result
                ? null
                : result.ToCookieCollection();
        }

        public async Task<int> SaveAsync(CookieCollection cookieCollection)
        {
            var connection = Database.GetConnection(new CancellationToken());

            //TODO: for now we're a single-user app.
            await connection.ExecuteAsync("delete from CookieCollection");

            // Serializes the cookie collection so
            // we can store it in the database.
            var persistableCookieCollection = PersistableCookieCollection
                .FromCookieCollection(cookieCollection);

            return await connection.InsertAsync(persistableCookieCollection);
        }

        public async Task<Settings> LoadSettingsAsync()
        {
            return await Database
                .GetConnection(new CancellationToken())
                .Table<Settings>()
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAndApplySettingsAsync(Settings settings)
        {
            var connection = Database.GetConnection(new CancellationToken());

            await connection.DeleteAsync(settings);
            var result = await connection.InsertAsync(settings);

            // Applies the settings.
            User.Settings = settings;

            return result;
        }
    }
}