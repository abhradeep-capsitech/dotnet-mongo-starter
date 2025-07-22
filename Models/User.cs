using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DotnetMongoStarter.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        public string? Password { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; } = UserRole.User;

        [BsonElement("refreshToken")]
        public string? RefreshToken { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
