using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumPL.Tests.Pages;
using System.Linq;

namespace SeleniumPL.Tests
{
    /// <summary>
    /// Base class for tests that require a logged-in session using Page Object Model.
    /// Ensures the login test has been executed before running dependent tests.
    /// </summary>
    public abstract class LoggedInBaseTest : BaseTest
    {
        protected DashboardPage? Dashboard;

        [SetUp]
        public void EnsureLoggedInPOM()
        {
            // Initialize dashboard page
            Dashboard = new DashboardPage(Driver);
            
            // Check if we're already logged in using POM
            try
            {
                if (Dashboard.IsUserLoggedIn())
                {
                    Logger.Information("✅ Session is already logged in (verified via POM)");
                    return;
                }
                else
                {
                    Logger.Warning("⚠️  Not logged in - this test requires a login session");
                    Logger.Information("Please ensure the POM login test 'PracticalLaw_LoginUsingPOM' has been executed first");
                    Assert.Inconclusive("This test requires a logged-in session. Please run the POM login test first.");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("Error checking login status via POM: {Error}", ex.Message);
                Logger.Information("Assuming login is required - please run POM login test first");
                Assert.Inconclusive("Unable to verify login status via POM. Please run the login test first.");
            }
        }

        /// <summary>
        /// Helper method to verify the user is still logged in using POM
        /// </summary>
        protected bool IsLoggedIn()
        {
            try
            {
                return Dashboard?.IsUserLoggedIn() ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to get sign out button using POM
        /// </summary>
        protected IWebElement? GetSignOutButton()
        {
            try
            {
                return Dashboard?.GetSignOutButton();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to perform logout using POM
        /// </summary>
        protected bool Logout()
        {
            try
            {
                if (Dashboard != null)
                {
                    return Dashboard.SignOut();
                }
                throw new InvalidOperationException("Dashboard not initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to logout using POM: {Error}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Helper method to perform logout and click signin for test continuity using POM
        /// </summary>
        protected bool LogoutAndClickSignIn()
        {
            try
            {
                if (Dashboard != null)
                {
                    return Dashboard.SignOutAndClickSignIn();
                }
                throw new InvalidOperationException("Dashboard not initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to logout and click signin using POM: {Error}", ex.Message);
                throw;
            }
        }
    }
}
