using System.Threading.Tasks;
using AVC.DatabaseModels;
using AVC.Models;
using MongoDB.Driver;

namespace AVC.Collections
{
    public interface ILogCollection : Interfaces.ICollection<Log>
    {
    }
    public class LogCollection : BaseCollection<Log>, ILogCollection
    {
        public LogCollection(IMongoContext context) : base(context)
        {
        }
    }
}