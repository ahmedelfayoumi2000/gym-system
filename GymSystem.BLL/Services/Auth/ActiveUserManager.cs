using System;
using System.Collections.Concurrent;

namespace GymSystem.BLL.Services.Auth
{
    public class ActiveUserManager
    {
        private readonly ConcurrentDictionary<string, DateTime> _activeUsers;

        public ActiveUserManager()
        {
            _activeUsers = new ConcurrentDictionary<string, DateTime>();
        }

        public bool IsUserLoggedIn(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            return _activeUsers.ContainsKey(userId);
        }

        public void AddUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            if (!_activeUsers.TryAdd(userId, DateTime.UtcNow))
                throw new InvalidOperationException($"User with ID {userId} is already active.");
        }

        public void RemoveUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            if (!_activeUsers.TryRemove(userId, out _))
                throw new InvalidOperationException($"Failed to remove user with ID {userId} from active users list.");
        }
    }
}