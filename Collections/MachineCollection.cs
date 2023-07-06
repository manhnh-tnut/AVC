using System.Threading.Tasks;
using AVC.DatabaseModels;
using AVC.Models;
using MongoDB.Driver;

namespace AVC.Collections
{
    public interface IMachineCollection : Interfaces.ICollection<Machine>
    {
    }
    public class MachineCollection : BaseCollection<Machine>, IMachineCollection
    {
        public MachineCollection(IMongoContext context) : base(context)
        {
        }
    }
}