using AVC.DatabaseModels;
using AVC.Models;

namespace AVC.Collections
{
    public interface ISummaryCollection : Interfaces.ICollection<Summary>
    {
    }
    public class SummaryCollection : BaseCollection<Summary>, ISummaryCollection
    {
        public SummaryCollection(IMongoContext context) : base(context)
        {
        }
    }
}