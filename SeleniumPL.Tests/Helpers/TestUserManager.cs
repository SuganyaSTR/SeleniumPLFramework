using System;
using System.Collections.Generic;
using System.Threading;

namespace SeleniumPL.Tests.Helpers
{
    /// <summary>
    /// Manages test user accounts for parallel test execution
    /// Ensures each test uses a different user to avoid login conflicts
    /// </summary>
    public static class TestUserManager
    {
        /// <summary>
        /// Available test user credentials
        /// </summary>
        private static readonly List<(string Username, string Password)> TestUsers = new()
        {
            ("WnIndigoTestUser1@mailinator.com", "WestlawNext1234"),   // User 1 (existing)
            ("WnIndigoTestUser16@mailinator.com", "WestlawNext1234"),  // User 2 (new)
            ("WnIndigoTestUser17@mailinator.com", "WestlawNext1234"),  // User 3 (new)
            ("WnIndigoTestUser18@mailinator.com", "WestlawNext1234")   // User 4 (new)
        };

        /// <summary>
        /// Thread-safe counter to track user assignments
        /// </summary>
        private static int _userCounter = 0;

        /// <summary>
        /// Lock object for thread safety
        /// </summary>
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Gets the next available test user credentials in a round-robin fashion
        /// Thread-safe for parallel test execution
        /// </summary>
        /// <returns>Tuple containing username and password</returns>
        public static (string Username, string Password) GetNextTestUser()
        {
            lock (_lockObject)
            {
                int userIndex = _userCounter % TestUsers.Count;
                _userCounter++;
                
                var selectedUser = TestUsers[userIndex];
                
                // Log which user is being assigned (could be enhanced with proper logging)
                Console.WriteLine($"[TestUserManager] Assigned User {userIndex + 1}: {selectedUser.Username}");
                
                return selectedUser;
            }
        }

        /// <summary>
        /// Gets a specific test user by index (0-based)
        /// Useful for tests that need to use a specific user
        /// </summary>
        /// <param name="userIndex">Index of the user (0-3)</param>
        /// <returns>Tuple containing username and password</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when userIndex is invalid</exception>
        public static (string Username, string Password) GetTestUserByIndex(int userIndex)
        {
            if (userIndex < 0 || userIndex >= TestUsers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(userIndex), 
                    $"User index must be between 0 and {TestUsers.Count - 1}");
            }

            var selectedUser = TestUsers[userIndex];
            Console.WriteLine($"[TestUserManager] Assigned User {userIndex + 1} by index: {selectedUser.Username}");
            
            return selectedUser;
        }

        /// <summary>
        /// Gets the total number of available test users
        /// </summary>
        /// <returns>Number of available test users</returns>
        public static int GetAvailableUserCount()
        {
            return TestUsers.Count;
        }

        /// <summary>
        /// Resets the user counter - useful for test initialization
        /// </summary>
        public static void ResetUserCounter()
        {
            lock (_lockObject)
            {
                _userCounter = 0;
                Console.WriteLine("[TestUserManager] User counter reset to 0");
            }
        }

        /// <summary>
        /// Gets user credentials based on test name hash
        /// This ensures consistent user assignment for the same test
        /// </summary>
        /// <param name="testName">Name of the test method</param>
        /// <returns>Tuple containing username and password</returns>
        public static (string Username, string Password) GetUserForTest(string testName)
        {
            // Use hash of test name to determine user index for consistency
            int hash = Math.Abs(testName.GetHashCode());
            int userIndex = hash % TestUsers.Count;
            
            var selectedUser = TestUsers[userIndex];
            Console.WriteLine($"[TestUserManager] Assigned User {userIndex + 1} for test '{testName}': {selectedUser.Username}");
            
            return selectedUser;
        }
    }
}
