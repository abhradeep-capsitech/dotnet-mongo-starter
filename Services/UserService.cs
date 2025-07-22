// Services/UserService.cs
using DotnetMongoStarter.Db;
using DotnetMongoStarter.Models;
using DotnetMongoStarter.Utils;
using MongoDB.Driver;

namespace DotnetMongoStarter.Services
{
    public interface IUserService
    {
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(string userId);
        Task<User> CreateUser(User user);
        Task SaveUserToken(string userId, string refreshToken);
        Task DeleteToken(string userId);
    }

    public class UserService : IUserService
    {
        private readonly DbContext _dbContext;

        public UserService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                var user = await _dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new ApiException("Database error while fetching user by email.", 500, new List<string> { ex.Message });
            }
        }

        public async Task<User> GetUserById(string userId)
        {
            try
            {
                var user = await _dbContext.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new ApiException("Database error while fetching user by id.", 500, new List<string> { ex.Message });
            }
        }

        public async Task<User> CreateUser(User user)
        {
            try
            {
                await _dbContext.Users.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                throw new ApiException("Database error while creating user.", 500, new List<string> { ex.Message });
            }
        }

        public async Task SaveUserToken(string userId, string refreshToken)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update.Set(u => u.RefreshToken, refreshToken);
                await _dbContext.Users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new ApiException("Database error while saving user token.", 500, new List<string> { ex.Message });
            }
        }

        public async Task DeleteToken(string userId)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var update = Builders<User>.Update.Set(u => u.RefreshToken, null);
                await _dbContext.Users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new ApiException("Database error while deleting user token.", 500, new List<string> { ex.Message });
            }
        }
    }
}