using MongoDB.Driver;

namespace AVC.Models
{
    public interface IMongoContext
    {
        IMongoDatabase Database { get; }
    }
    public class MongoContext : IMongoContext
    {
        public IMongoDatabase Database { get; }
        public MongoContext(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            Database = client.GetDatabase(settings.DatabaseName);
        }

    }
}