using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Manages test user allocation to prevent concurrent login conflicts
    /// </summary>
    public class TestUserManager
    {
        private readonly IConfiguration _config;
        private readonly ConcurrentDictionary<string, bool> _userAvailability;
        private readonly Dictionary<string, TestUser> _testUsers;
        private static readonly object _lock = new object();

        public TestUserManager(IConfiguration config)
        {
            _config = config;
            _userAvailability = new ConcurrentDictionary<string, bool>();
            _testUsers = new Dictionary<string, TestUser>();
            InitializeUsers();
        }

        private void InitializeUsers()
        {
            var usersSection = _config.GetSection("TestUsers");
            foreach (var userConfig in usersSection.GetChildren())
            {
                var testUser = new TestUser
                {
                    UserId = userConfig.Key,
                    Username = userConfig["Username"],
                    Password = userConfig["Password"]
                };
                
                _testUsers[userConfig.Key] = testUser;
                _userAvailability[userConfig.Key] = true; // Mark as available
            }
        }

        /// <summary>
        /// Gets an available test user for the current test
        /// </summary>
        /// <param name="testName">Name of the test requesting the user</param>
        /// <returns>TestUser object with credentials</returns>
        public TestUser GetAvailableUser(string testName)
        {
            lock (_lock)
            {
                // Find the first available user
                foreach (var kvp in _userAvailability)
                {
                    if (kvp.Value) // User is available
                    {
                        _userAvailability[kvp.Key] = false; // Mark as in use
                        var user = _testUsers[kvp.Key];
                        user.AssignedTest = testName;
                        return user;
                    }
                }

                // If no users are available, wait and retry (with timeout)
                throw new InvalidOperationException($"No test users available for test: {testName}. All users are currently in use.");
            }
        }

        /// <summary>
        /// Releases a user back to the available pool
        /// </summary>
        /// <param name="userId">The user ID to release</param>
        public void ReleaseUser(string userId)
        {
            lock (_lock)
            {
                if (_userAvailability.ContainsKey(userId))
                {
                    _userAvailability[userId] = true; // Mark as available
                    if (_testUsers.ContainsKey(userId))
                    {
                        _testUsers[userId].AssignedTest = null;
                    }
                }
            }
        }

        /// <summary>
        /// Gets user by specific user ID (for tests that need a specific user)
        /// </summary>
        /// <param name="userId">The specific user ID to get</param>
        /// <param name="testName">Name of the test requesting the user</param>
        /// <returns>TestUser object if available</returns>
        public TestUser GetSpecificUser(string userId, string testName)
        {
            lock (_lock)
            {
                if (!_testUsers.ContainsKey(userId))
                {
                    throw new ArgumentException($"User {userId} not found in configuration");
                }

                if (!_userAvailability[userId])
                {
                    throw new InvalidOperationException($"User {userId} is currently in use by another test");
                }

                _userAvailability[userId] = false; // Mark as in use
                var user = _testUsers[userId];
                user.AssignedTest = testName;
                return user;
            }
        }

        /// <summary>
        /// Gets all available users (for debugging)
        /// </summary>
        /// <returns>List of available user IDs</returns>
        public List<string> GetAvailableUserIds()
        {
            var availableUsers = new List<string>();
            foreach (var kvp in _userAvailability)
            {
                if (kvp.Value)
                {
                    availableUsers.Add(kvp.Key);
                }
            }
            return availableUsers;
        }
    }

    /// <summary>
    /// Represents a test user with credentials
    /// </summary>
    public class TestUser
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AssignedTest { get; set; }

        public override string ToString()
        {
            return $"User: {UserId} ({Username}) - Assigned to: {AssignedTest ?? "None"}";
        }
    }
}
