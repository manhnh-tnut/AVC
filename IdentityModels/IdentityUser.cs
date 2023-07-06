using AspNetCore.Identity.Mongo.Model;

namespace AVC.IdentityModels
{
    public class IdentityUser : MongoUser
    {
        public string Gen { get => UserName; set { UserName = value; Email = UserName + "@samsung.com"; } }
        public string Group { get; set; }
    }
}