using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SignInPage.Models
{
    public class SignInDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        //public byte[] image { get; set; } = null!;


    }
}
