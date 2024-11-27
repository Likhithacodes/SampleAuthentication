using Authentication.Modals;
using MongoDB.Driver;

namespace Authentication.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<User> _users;
        public MongoDBService(IConfiguration config)
        {
            var client = new MongoClient(config["Database:ConnectionString"]);
            var database = client.GetDatabase(config["Database:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
        }
        public async Task<User> GetUserByUsername(string username) =>
           await _users.Find(user => user.Username == username).FirstOrDefaultAsync();

        public async Task CreateUser(User user) =>
            await _users.InsertOneAsync(user);
    }
}
