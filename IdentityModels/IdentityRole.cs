using AspNetCore.Identity.Mongo.Model;

namespace AVC.IdentityModels
{
    public class IdentityRole : MongoRole
    {
        public IdentityRole(string role) : base(role)
        {
        }
    }
}