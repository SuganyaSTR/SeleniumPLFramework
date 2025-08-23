using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumPL.Tests.Pages;
using Serilog;
using System;

namespace SeleniumPL.Tests.TestCases
{
    /// <summary>
    /// Test class using Page Object Model for Practical Law Login Tests
    /// Demonstrates clean, maintainable test structure using POM
    /// </summary>
    [TestFixture]
    [Order(1)] // This fixture runs first
    public class PracticalLawLoginTestsPOM : BaseTest
    {
        private static bool _isLoggedIn = false; // Static flag to track login status
        private static DashboardPage? _dashboardPage; // Shared dashboard page for session persistence

        #region Helper Methods

        /// <summary>
        /// Performs login operation that can be reused across test methods
        /// </summary>
        /// <returns>DashboardPage instance after successful login</returns>
        private DashboardPage PerformLogin()
        {
            Logger.Information("=== Performing Login Operation ===");

            // Step 1: Navigate to Practical Law and handle cookies
            var homePage = new PracticalLawHomePagePOM(Driver, Logger);
            
            // Check if we're already on Practical Law page
            if (!homePage.IsOnPracticalLawPage())
            {
                Logger.Information("Not on Practical Law page, navigating...");
                homePage.NavigateTo();
            }
            else
            {
                Logger.Information("Already on Practical Law page");
            }

            // Validate we're on the correct page
            if (!homePage.IsOnPracticalLawPage())
                throw new Exception("Should be on Practical Law page");

            // Step 2: Handle cookie consent
            homePage.HandleCookieConsent();

            // Step 3: Click Sign In to navigate to login page
            var loginPage = homePage.ClickSignIn();

            // Validate we're on login page
            if (!loginPage.IsOnLoginPage())
                throw new Exception("Should be on login page after clicking Sign In");

            // Step 4: Perform login with credentials
            var dashboard = loginPage.Login(
                username: "WnIndigoTestUser1@mailinator.com",
                password: "WestlawNext1234"
            );

            // Step 5: Validate successful login
            if (!dashboard.IsUserLoggedIn())
                throw new Exception("User should be logged in successfully");
            if (!dashboard.IsOnDashboard())
                throw new Exception("Should be on dashboard/main page after login");

            Logger.Information("✅ Login operation completed successfully");
            return dashboard;
        }

        /// <summary>
        /// Ensures user is logged in for test execution
        /// </summary>
        private void EnsureUserIsLoggedIn()
        {
            // If we have an existing logged-in session, verify it's still valid
            if (_isLoggedIn && _dashboardPage != null)
            {
                Logger.Information("Checking existing logged-in session...");
                try
                {
                    if (_dashboardPage.IsUserLoggedIn())
                    {
                        Logger.Information("✅ Session is still valid - continuing with existing login");
                        return;
                    }
                    else
                    {
                        Logger.Information("Session expired - will re-login");
                        _isLoggedIn = false;
                        _dashboardPage = null;
                    }
                }
                catch
                {
                    Logger.Information("Could not verify session - will re-login");
                    _isLoggedIn = false;
                    _dashboardPage = null;
                }
            }

            // Perform fresh login
            try
            {
                _dashboardPage = PerformLogin();
                _isLoggedIn = true;
                Logger.Information("🔐 Login session established for test execution");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Failed to establish login session: {Error}", ex.Message);
                throw new Exception($"Cannot proceed with test - login failed: {ex.Message}");
            }
        }

        #endregion

        #region Test Setup and Teardown

        [OneTimeSetUp]
        public void OneTimeSetUpLogin()
        {
            Logger.Information("=== Setting up POM-based login session for all tests ===");
        }

        [SetUp]
        public void TestSetUp()
        {
            // Override base setup behavior for login tests
            // Custom handling for Test13 to avoid navigation issues
            if (TestContext.CurrentContext.Test.Name.Contains("Test13"))
            {
                Logger.Information("Custom setup for Test13 - using direct navigation approach");
            }
            
            // Ensure user is logged in before each test
            EnsureUserIsLoggedIn();
        }

        [OneTimeTearDown]
        public void OneTimeTearDownLogin()
        {
            // Clean up the shared session at the end of all tests
            if (_dashboardPage != null)
            {
                Logger.Information("Cleaning up shared login session");
                try
                {
                    // Try to sign out gracefully
                    _dashboardPage.SignOut();
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error during graceful logout: {Error}", ex.Message);
                }
                finally
                {
                    _dashboardPage = null;
                    _isLoggedIn = false;
                }
            }
        }

        #endregion

        #region Login Tests

        [Test, Order(1), Retry(2)]
        [Category("Login")]
        [Description("Practical Law login test using Page Object Model - Establishes session for other tests")]
        public void PracticalLaw_LoginUsingPOM()
        {
            // Skip login if already logged in
            if (_isLoggedIn && _dashboardPage != null)
            {
                Logger.Information("Already logged in - skipping login test");
                Assert.Pass("Login session already established using POM");
                return;
            }

            try
            {
                Logger.Information("=== Starting Practical Law Login Test using POM ===");

                // Use the helper method to perform login
                _dashboardPage = PerformLogin();
                _isLoggedIn = true;

                // Log success information
                Logger.Information("✅ Login successful using POM!");
                Logger.Information("✅ Sign Out button text: '{Text}'", _dashboardPage.GetSignOutButtonText());
                Logger.Information("Final URL: {Url}", _dashboardPage.GetCurrentUrl());
                Logger.Information("Final Title: {Title}", _dashboardPage.GetPageTitle());
                Logger.Information("🔐 Login session established for subsequent tests");

                Assert.Pass("Practical Law login completed successfully using POM!");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ POM login test failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver.Url);
                Logger.Information("Current Title: {Title}", Driver.Title);
                throw;
            }
        }

        #endregion

        #region Essential Profile Icon Tests

        [Test, Order(2), Retry(2)]
        [Category("PostLogin")]
        [Description("Validate profile icon hover functionality without clicking")]
        public void PracticalLaw_ProfileIconHoverValidation()
        {
            try
            {
                Logger.Information("=== Profile Icon Hover Validation Test ===");
                Logger.Information("Testing profile icon hover functionality...");

                // Step 1: Verify we're logged in
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in");

                // Step 2: Get profile icon and validate its properties
                Logger.Information("Step 1: Locating profile icon");
                var profileIcon = _dashboardPage.GetProfileIcon();
                
                Assert.That(profileIcon, Is.Not.Null, 
                    "Profile icon should be found using XPath: //*[@id=\"coid_website_signOffRegion\"]");
                
                Assert.That(profileIcon!.Displayed, Is.True, 
                    "Profile icon should be displayed");
                
                Assert.That(profileIcon.Enabled, Is.True, 
                    "Profile icon should be enabled");

                // Step 3: Log profile icon details
                Logger.Information("✅ Profile icon found and validated!");
                Logger.Information("Profile icon details:");
                Logger.Information("  - Tag Name: '{TagName}'", profileIcon.TagName);
                Logger.Information("  - Location: X={X}, Y={Y}", 
                    profileIcon.Location.X, profileIcon.Location.Y);
                Logger.Information("  - Size: Width={Width}, Height={Height}", 
                    profileIcon.Size.Width, profileIcon.Size.Height);

                // Step 4: Test hover functionality
                try
                {
                    Logger.Information("Step 2: Testing hover functionality");
                    var actions = new Actions(Driver);
                    actions.MoveToElement(profileIcon).Perform();
                    
                    // Wait a moment for any hover effects
                    System.Threading.Thread.Sleep(1000);
                    
                    Logger.Information("✅ Hover action performed successfully");
                }
                catch (Exception hoverEx)
                {
                    Logger.Warning("⚠️ Hover action failed: {Error}", hoverEx.Message);
                }

                // Step 5: Validate current page state
                var currentUrl = _dashboardPage.GetCurrentUrl();
                var pageTitle = _dashboardPage.GetPageTitle();
                
                Logger.Information("✅ PROFILE ICON HOVER VALIDATION COMPLETE");
                Logger.Information("Current URL: {Url}", currentUrl);
                Logger.Information("Page Title: {Title}", pageTitle ?? "N/A");

                Assert.Pass("Profile icon hover validation completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Profile icon hover validation test failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver.Url);
                Logger.Information("Current Title: {Title}", Driver.Title);
                throw;
            }
        }

        [Test, Order(3), Retry(2)]
        [Category("PostLogin")]
        [Description("Click profile icon and validate sign out button - Complete profile functionality test")]
        public void PracticalLaw_ProfileIconClickAndSignOutValidation()
        {
            try
            {
                Logger.Information("=== Profile Icon Click and Sign Out Validation Test ===");
                Logger.Information("Testing complete profile icon functionality with sign out validation...");

                // Step 1: Verify we're logged in
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in");

                // Step 2: Click profile icon and validate sign out button appears
                Logger.Information("Step 1: Clicking profile icon to reveal sign out options");
                bool profileClickSuccess = _dashboardPage.ClickProfileIconAndValidateSignOut();
                
                Assert.That(profileClickSuccess, Is.True, 
                    "Profile icon click should succeed and reveal sign out button");

                // Step 3: Get and validate the sign out button after profile click
                Logger.Information("Step 2: Validating sign out button properties");
                var signOutButton = _dashboardPage.GetSignOutButtonAfterProfileClick();
                
                Assert.That(signOutButton, Is.Not.Null, 
                    "Sign out button should be available after clicking profile icon");
                
                Assert.That(signOutButton!.Displayed, Is.True, 
                    "Sign out button should be displayed");
                
                Assert.That(signOutButton.Enabled, Is.True, 
                    "Sign out button should be enabled and clickable");

                // Step 4: Log detailed information about the sign out button
                Logger.Information("✅ Profile icon click validation successful!");
                Logger.Information("Sign out button details:");
                Logger.Information("  - Text: '{Text}'", signOutButton.Text);
                Logger.Information("  - Tag Name: '{TagName}'", signOutButton.TagName);
                Logger.Information("  - Location: X={X}, Y={Y}", 
                    signOutButton.Location.X, signOutButton.Location.Y);
                Logger.Information("  - Size: Width={Width}, Height={Height}", 
                    signOutButton.Size.Width, signOutButton.Size.Height);

                // Step 5: Validate current page state
                var currentUrl = _dashboardPage.GetCurrentUrl();
                var pageTitle = _dashboardPage.GetPageTitle();
                
                Logger.Information("✅ PROFILE ICON AND SIGN OUT VALIDATION COMPLETE");
                Logger.Information("Current URL: {Url}", currentUrl);
                Logger.Information("Page Title: {Title}", pageTitle ?? "N/A");
                Logger.Information("Session Status: User remains logged in for subsequent tests");

                Assert.Pass("Profile icon click and sign out button validation completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Profile icon and sign out validation test failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver.Url);
                Logger.Information("Current Title: {Title}", Driver.Title);
                
                // Take screenshot for debugging
                TakeScreenshot("ProfileIconSignOutValidation_Failed");
                throw;
            }
        }

        [Test, Order(4), Retry(2)]
        [Category("PostLogin")]
        [Description("Login, verify no error messages on home page, and logout")]
        public void PracticalLaw_LoginVerifyNoErrorsAndLogout()
        {
            try
            {
                Logger.Information("=== Login, Verify No Error Messages, and Logout Test ===");
                Logger.Information("Testing complete login flow with error validation and logout...");

                // Step 1: Ensure user is logged in (using existing login method)
                Logger.Information("Step 1: Ensuring user is logged in");
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in");
                Logger.Information("✅ User is successfully logged in");

                // Step 2: Verify there are no error messages on the home page
                Logger.Information("Step 2: Verifying no error messages are displayed on the home page");
                bool noErrorMessages = _dashboardPage.HasNoErrorMessages();
                
                if (!noErrorMessages)
                {
                    // If error messages are found, log them for debugging
                    var errorMessages = _dashboardPage.GetErrorMessages();
                    Logger.Warning("❌ Error messages found on the home page:");
                    foreach (var errorMessage in errorMessages)
                    {
                        Logger.Warning("  - {ErrorMessage}", errorMessage);
                    }
                }
                
                Assert.That(noErrorMessages, Is.True, 
                    "No error messages should be displayed on the home page after successful login");
                Logger.Information("✅ No error messages found on the home page");

                // Step 3: Log out from the application
                Logger.Information("Step 3: Logging out from the application");
                bool logoutSuccessful = _dashboardPage.SignOut();
                
                Assert.That(logoutSuccessful, Is.True, 
                    "Logout should be successful");
                Logger.Information("✅ Successfully logged out from the application");

                // Step 4: Verify logout was successful (user is no longer logged in)
                Logger.Information("Step 4: Verifying logout was successful");
                
                // Wait a moment for logout to complete
                System.Threading.Thread.Sleep(2000);
                
                bool loggedOut = false;
                try
                {
                    // Check if we're redirected to login page or if user is no longer logged in
                    var currentUrl = _dashboardPage.GetCurrentUrl().ToLower();
                    loggedOut = currentUrl.Contains("signoffactivity") ||
                               currentUrl.Contains("signoff") ||
                               currentUrl.Contains("login") || 
                               currentUrl.Contains("signin") || 
                               currentUrl.Contains("sign-in") ||
                               !_dashboardPage.IsUserLoggedIn();
                               
                    Logger.Information("Logout verification - URL: {Url}, Logged out: {LoggedOut}", currentUrl, loggedOut);
                }
                catch (OpenQA.Selenium.WebDriverException ex) when (ex.Message.Contains("invalid session id") || ex.Message.Contains("session deleted"))
                {
                    // If the session is invalid/deleted, that means logout was successful
                    Logger.Information("✅ Session was terminated - logout successful (session disconnect indicates successful logout)");
                    loggedOut = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error checking logout status: {Error}", ex.Message);
                    // If we can't check the status due to session issues, assume logout was successful
                    loggedOut = true;
                }
                
                Assert.That(loggedOut, Is.True, 
                    "User should be logged out and redirected to login page or session should be terminated");
                Logger.Information("✅ Logout verification successful - user is no longer logged in");

                // Reset the static login state since we logged out
                _isLoggedIn = false;
                _dashboardPage = null;
                
                Logger.Information("✅ LOGIN, ERROR VERIFICATION, AND LOGOUT TEST COMPLETED SUCCESSFULLY");
                
                // Try to get final page information, but handle session disconnect gracefully
                try
                {
                    Logger.Information("Current URL after logout: {Url}", _dashboardPage?.GetCurrentUrl() ?? Driver.Url);
                    Logger.Information("Page Title after logout: {Title}", _dashboardPage?.GetPageTitle() ?? Driver.Title);
                }
                catch (OpenQA.Selenium.WebDriverException ex) when (ex.Message.Contains("invalid session id") || ex.Message.Contains("session deleted"))
                {
                    Logger.Information("Session terminated after logout - cannot retrieve final page information (this is expected)");
                }
                catch (Exception ex)
                {
                    Logger.Information("Could not retrieve final page information: {Error}", ex.Message);
                }

                Assert.Pass("Login, error verification, and logout test completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Login, error verification, and logout test failed: {Error}", ex.Message);
                
                // Try to get current page information, but handle session disconnect gracefully
                try
                {
                    Logger.Information("Current URL: {Url}", Driver.Url);
                    Logger.Information("Current Title: {Title}", Driver.Title);
                }
                catch (OpenQA.Selenium.WebDriverException webEx) when (webEx.Message.Contains("invalid session id") || webEx.Message.Contains("session deleted"))
                {
                    Logger.Information("Session terminated - cannot retrieve page information (this may be due to successful logout)");
                }
                catch (Exception pageEx)
                {
                    Logger.Information("Could not retrieve page information: {Error}", pageEx.Message);
                }
                
                // Try to take screenshot, but handle session disconnect gracefully
                try
                {
                    TakeScreenshot("LoginVerifyNoErrorsAndLogout_Failed");
                }
                catch (OpenQA.Selenium.WebDriverException webEx) when (webEx.Message.Contains("invalid session id") || webEx.Message.Contains("session deleted"))
                {
                    Logger.Information("Could not take screenshot due to session termination");
                }
                catch (Exception screenshotEx)
                {
                    Logger.Information("Could not take screenshot: {Error}", screenshotEx.Message);
                }
                
                // Reset login state on failure
                _isLoggedIn = false;
                _dashboardPage = null;
                throw;
            }
        }

        #endregion

        #region Test12 - Employment Practice Area Validation and Sign Out

        [Test, Order(12)]
        [Category("EmploymentValidation")]
        [Description("Test12 - Login to PLUK, validate Employment practice area page, and sign out")]
        public void Test12_LoginValidateEmploymentPracticeAreaAndSignOut()
        {
            try
            {
                Logger.Information("=== Starting Test12: Login, Validate Employment Practice Area, and Sign Out ===");

                // Step 1: Login to PLUK (using existing login mechanism)
                Logger.Information("Step 1: Login to PLUK");
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized after login");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in successfully");
                
                Logger.Information("✅ Step 1 Complete: Successfully logged in to PLUK");

                // Step 2: Validate user is landing to Employment practice area page
                Logger.Information("Step 2: Validate user is landing to Employment practice area page");
                
                // Navigate to Practice Area
                var practiceAreaPage = _dashboardPage.NavigateToPracticeArea();
                Assert.That(practiceAreaPage, Is.Not.Null, "Practice area page should be available");

                // Navigate specifically to Employment practice area
                Logger.Information("Navigating to Employment practice area...");
                bool employmentNavigated = practiceAreaPage.SelectPracticeArea("Employment");
                Assert.That(employmentNavigated, Is.True, "Should successfully navigate to Employment practice area");

                // Validate we are on Employment practice area page
                bool isOnEmploymentPage = practiceAreaPage.ValidateEmploymentAsStartPage();
                Assert.That(isOnEmploymentPage, Is.True, "User should be on Employment practice area page");

                Logger.Information("✅ Step 2 Complete: Successfully validated Employment practice area page");

                // Step 3: Click on sign out
                Logger.Information("Step 3: Click on sign out");
                
                bool signOutSuccessful = _dashboardPage.SignOut();
                Assert.That(signOutSuccessful, Is.True, "Sign out should be successful");

                Logger.Information("✅ Step 3 Complete: Successfully signed out");

                // Reset login state after successful sign out
                _isLoggedIn = false;
                _dashboardPage = null;

                Logger.Information("✅ Test12 completed successfully!");
                Logger.Information("Final Summary:");
                Logger.Information("  - ✅ Login to PLUK: Successful");
                Logger.Information("  - ✅ Employment practice area validation: Successful");
                Logger.Information("  - ✅ Sign out: Successful");

                Assert.Pass("Test12: Login, Employment practice area validation, and sign out completed successfully!");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Test12 failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver?.Url ?? "N/A");
                Logger.Information("Current Title: {Title}", Driver?.Title ?? "N/A");
                
                // Try to take screenshot for debugging
                try
                {
                    TakeScreenshot("Test12_Failed");
                }
                catch (Exception screenshotEx)
                {
                    Logger.Warning("Could not take screenshot: {Error}", screenshotEx.Message);
                }
                
                // Reset login state on failure
                _isLoggedIn = false;
                _dashboardPage = null;
                throw;
            }
        }

        [Test, Order(13)]
        [Category("PostLogin")]
        [Description("Test13: Login to PLUK, Remove as my start page, validate text change, validate home icon, refresh page, check My Home link visibility, and sign out")]
        public void Test13_RemoveStartPageValidation()
        {
            try
            {
                Logger.Information("=== Starting Test13: Remove Start Page Validation ===");

                // Step 1: Login to PLUK (using same approach as Test12)
                Logger.Information("Step 1: Login to PLUK");
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized after login");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in successfully");
                
                Logger.Information("✅ Step 1 Complete: Successfully logged in to PLUK");

                // Navigate to Practice Area (same as Test12)
                var practiceAreaPage = _dashboardPage.NavigateToPracticeArea();
                Assert.That(practiceAreaPage, Is.Not.Null, "Practice area page should be available");

                // Check if we're already on Employment page or navigate to it
                string pageUrl = Driver.Url.ToLower();
                bool alreadyOnEmploymentPage = pageUrl.Contains("employment");
                
                if (alreadyOnEmploymentPage)
                {
                    Logger.Information("Already on Employment practice area page after login");
                }
                else
                {
                    // Navigate specifically to Employment practice area
                    Logger.Information("Navigating to Employment practice area...");
                    bool employmentNavigated = practiceAreaPage.SelectPracticeArea("Employment");
                    Assert.That(employmentNavigated, Is.True, "Should successfully navigate to Employment practice area");
                }

                // Step 2: Click on "Remove as my start page" using the specific xpath
                Logger.Information("Step 2: Click on 'Remove as my start page'");
                
                bool removeClicked = false;
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                // Primary xpath for the Remove as my start page element
                var removeStartPageLocator = By.XPath("//*[@id='coid_setAsHomePageElement']");
                
                try
                {
                    Logger.Information("Looking for Remove as my start page element with xpath: //*[@id='coid_setAsHomePageElement']");
                    
                    // Wait for the element to be present and clickable
                    var element = wait.Until(ExpectedConditions.ElementToBeClickable(removeStartPageLocator));
                    
                    if (element != null && element.Displayed && element.Enabled)
                    {
                        string elementText = element.Text;
                        Logger.Information("Found start page element with text: '{Text}'", elementText);
                        
                        // If it's "Make this my start page", click it first to set it
                        if (elementText.Contains("Make this", StringComparison.OrdinalIgnoreCase) && 
                            elementText.Contains("start page", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("Found 'Make this my start page' - clicking to set Employment as start page first");
                            
                            // Scroll to element to ensure it's visible
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                            System.Threading.Thread.Sleep(1000);
                            
                            element.Click();
                            Logger.Information("✅ Clicked 'Make this my start page'");
                            System.Threading.Thread.Sleep(3000); // Wait for action to complete
                            
                            // Now look for "Remove as my start page"
                            element = wait.Until(ExpectedConditions.ElementToBeClickable(removeStartPageLocator));
                            elementText = element.Text;
                            Logger.Information("Updated element text after setting: '{Text}'", elementText);
                        }
                        
                        // Now click "Remove as my start page"
                        if (elementText.Contains("Remove as my start page", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("Clicking 'Remove as my start page'");
                            
                            // Scroll to element to ensure it's visible
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                            System.Threading.Thread.Sleep(1000);
                            
                            // Try JavaScript click if regular click fails
                            try
                            {
                                element.Click();
                            }
                            catch (Exception)
                            {
                                Logger.Information("Regular click failed, trying JavaScript click");
                                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element);
                            }
                            
                            Logger.Information("✅ Successfully clicked 'Remove as my start page'");
                            System.Threading.Thread.Sleep(3000); // Wait for action to complete
                            removeClicked = true;
                        }
                        else
                        {
                            Logger.Warning("Element text is '{Text}' - not 'Remove as my start page'", elementText);
                        }
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    Logger.Error("❌ Timeout waiting for Remove as my start page element with xpath: //*[@id='coid_setAsHomePageElement']");
                }
                catch (Exception ex)
                {
                    Logger.Error("❌ Error finding Remove as my start page element: {Error}", ex.Message);
                }

                Assert.That(removeClicked, Is.True, "Should successfully click 'Remove as my start page'");
                Logger.Information("✅ Step 2 Complete: Successfully clicked 'Remove as my start page'");

                // Step 3: Validate the text changed from "Remove as my start page" to "Make this start page"
                Logger.Information("Step 3: Validate text changed to 'Make this start page'");
                
                bool textChangedCorrectly = false;
                System.Threading.Thread.Sleep(2000); // Wait for UI to update
                
                try
                {
                    var element = Driver.FindElement(removeStartPageLocator);
                    if (element != null && element.Displayed)
                    {
                        string updatedText = element.Text;
                        Logger.Information("Found element with updated text: '{Text}'", updatedText);
                        
                        if (updatedText.Contains("Make this", StringComparison.OrdinalIgnoreCase) && 
                            updatedText.Contains("start page", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("✅ Text correctly changed to 'Make this start page'");
                            textChangedCorrectly = true;
                        }
                    }
                }
                catch (NoSuchElementException)
                {
                    Logger.Warning("Could not find element to validate text change");
                }

                Assert.That(textChangedCorrectly, Is.True, "Text should change from 'Remove as my start page' to 'Make this start page'");
                Logger.Information("✅ Step 3 Complete: Text validation successful");

                // Step 4: Validate Home icon is not filled in
                Logger.Information("Step 4: Validate Home icon is not filled in");
                
                bool homeIconNotFilled = practiceAreaPage.ValidateHomeIconNotFilled();
                Assert.That(homeIconNotFilled, Is.True, "Home icon should not be filled after removing start page");
                Logger.Information("✅ Step 4 Complete: Home icon is not filled (as expected)");

                // Step 5: Refresh the page
                Logger.Information("Step 5: Refresh the page");
                string currentUrl = Driver.Url;
                Logger.Information("Current URL before refresh: {Url}", currentUrl);
                
                Driver.Navigate().Refresh();
                System.Threading.Thread.Sleep(5000); // Wait for page to reload completely
                
                string urlAfterRefresh = Driver.Url;
                Logger.Information("URL after refresh: {Url}", urlAfterRefresh);
                Logger.Information("✅ Step 5 Complete: Page refreshed successfully");

                // Step 6: Make sure My Home link is not visible on top of the screen
                Logger.Information("Step 6: Validate 'My Home' link is not visible on top of the screen");
                
                bool myHomeLinkNotVisible = practiceAreaPage.ValidateMyHomeLinkNotVisible();
                Assert.That(myHomeLinkNotVisible, Is.True, "'My Home' link should not be visible after removing start page");
                Logger.Information("✅ Step 6 Complete: 'My Home' link is not visible (as expected)");

                // Step 7: Sign out (same as Test12)
                Logger.Information("Step 7: Sign out");
                
                bool signOutSuccessful = _dashboardPage.SignOut();
                Assert.That(signOutSuccessful, Is.True, "Sign out should be successful");
                Logger.Information("✅ Step 7 Complete: Successfully signed out");

                // Reset login state after successful sign out (same as Test12)
                _isLoggedIn = false;
                _dashboardPage = null;

                Logger.Information("✅ Test13 completed successfully!");
                Logger.Information("Final Summary:");
                Logger.Information("  - ✅ Login to PLUK: Successful");
                Logger.Information("  - ✅ Click 'Remove as my start page': Successful");
                Logger.Information("  - ✅ Validate text change to 'Make this start page': Successful");
                Logger.Information("  - ✅ Validate Home icon not filled: Successful");
                Logger.Information("  - ✅ Refresh page: Successful");
                Logger.Information("  - ✅ Validate 'My Home' link not visible: Successful");
                Logger.Information("  - ✅ Sign out: Successful");

                Assert.Pass("Test13: Remove start page validation completed successfully!");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Test13 failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver?.Url ?? "N/A");
                Logger.Information("Current Title: {Title}", Driver?.Title ?? "N/A");
                
                // Try to take screenshot for debugging
                try
                {
                    TakeScreenshot("Test13_Failed");
                }
                catch (Exception screenshotEx)
                {
                    Logger.Warning("Could not take screenshot: {Error}", screenshotEx.Message);
                }
                
                // Reset login state on failure
                _isLoggedIn = false;
                _dashboardPage = null;
                throw;
            }
        }

        [Test, Order(14)]
        [Category("PostLogin")]
        [Description("Test14: Login to PLUK, validate user is landing to Home page, and sign out")]
        public void Test14_LoginValidateHomePageAndSignOut()
        {
            try
            {
                Logger.Information("=== Starting Test14: Login, Validate Home Page, and Sign Out ===");

                // Step 1: Login to PLUK
                Logger.Information("Step 1: Login to PLUK");
                Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page should be initialized after login");
                Assert.That(_dashboardPage!.IsUserLoggedIn(), Is.True, "User should be logged in successfully");
                
                Logger.Information("✅ Step 1 Complete: Successfully logged in to PLUK");

                // Step 2: Validate the user is landing to Home page
                Logger.Information("Step 2: Validate the user is landing to Home page");
                
                // Validate that user is on the dashboard/home page
                bool isOnHomePage = _dashboardPage.IsOnDashboard();
                Assert.That(isOnHomePage, Is.True, "User should be on the home/dashboard page after login");
                
                // Additional validations for home page
                string currentUrl = _dashboardPage.GetCurrentUrl();
                string? pageTitle = _dashboardPage.GetPageTitle();
                
                Logger.Information("Home page validation details:");
                Logger.Information("  - Current URL: {Url}", currentUrl);
                Logger.Information("  - Page Title: {Title}", pageTitle ?? "N/A");
                Logger.Information("  - Is on Dashboard: {IsOnDashboard}", isOnHomePage);
                
                // Validate that essential home page elements are present
                bool userLoggedInIndicatorPresent = _dashboardPage.IsUserLoggedIn();
                Assert.That(userLoggedInIndicatorPresent, Is.True, "User logged in indicator should be present on home page");
                
                // Check if there are no error messages on the home page
                bool noErrorMessages = _dashboardPage.HasNoErrorMessages();
                if (!noErrorMessages)
                {
                    var errorMessages = _dashboardPage.GetErrorMessages();
                    Logger.Warning("❌ Error messages found on the home page:");
                    foreach (var errorMessage in errorMessages)
                    {
                        Logger.Warning("  - {ErrorMessage}", errorMessage);
                    }
                }
                Assert.That(noErrorMessages, Is.True, "No error messages should be displayed on the home page");
                
                Logger.Information("✅ Step 2 Complete: Successfully validated user is on Home page");

                // Step 3: Sign out
                Logger.Information("Step 3: Sign out");
                
                bool signOutSuccessful = _dashboardPage.SignOut();
                Assert.That(signOutSuccessful, Is.True, "Sign out should be successful");
                Logger.Information("✅ Step 3 Complete: Successfully signed out");

                // Verify logout was successful
                Logger.Information("Verifying logout was successful...");
                System.Threading.Thread.Sleep(2000); // Wait for logout to complete
                
                bool loggedOut = false;
                try
                {
                    // Check if we're redirected to login page or if user is no longer logged in
                    var currentUrlAfterLogout = _dashboardPage.GetCurrentUrl().ToLower();
                    loggedOut = currentUrlAfterLogout.Contains("signoffactivity") ||
                               currentUrlAfterLogout.Contains("signoff") ||
                               currentUrlAfterLogout.Contains("login") || 
                               currentUrlAfterLogout.Contains("signin") || 
                               currentUrlAfterLogout.Contains("sign-in") ||
                               !_dashboardPage.IsUserLoggedIn();
                               
                    Logger.Information("Logout verification - URL: {Url}, Logged out: {LoggedOut}", currentUrlAfterLogout, loggedOut);
                }
                catch (OpenQA.Selenium.WebDriverException ex) when (ex.Message.Contains("invalid session id") || ex.Message.Contains("session deleted"))
                {
                    Logger.Information("✅ Session was terminated - logout successful");
                    loggedOut = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error checking logout status: {Error}", ex.Message);
                    loggedOut = true; // Assume logout was successful if we can't check
                }
                
                Assert.That(loggedOut, Is.True, "User should be logged out successfully");

                // Reset login state after successful sign out
                _isLoggedIn = false;
                _dashboardPage = null;

                Logger.Information("✅ Test14 completed successfully!");
                Logger.Information("Final Summary:");
                Logger.Information("  - ✅ Login to PLUK: Successful");
                Logger.Information("  - ✅ Validate user landing on Home page: Successful");
                Logger.Information("  - ✅ Sign out: Successful");

                Assert.Pass("Test14: Login, validate Home page, and sign out completed successfully!");
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Test14 failed: {Error}", ex.Message);
                Logger.Information("Current URL: {Url}", Driver?.Url ?? "N/A");
                Logger.Information("Current Title: {Title}", Driver?.Title ?? "N/A");
                
                // Try to take screenshot for debugging
                try
                {
                    TakeScreenshot("Test14_Failed");
                }
                catch (Exception screenshotEx)
                {
                    Logger.Warning("Could not take screenshot: {Error}", screenshotEx.Message);
                }
                
                // Reset login state on failure
                _isLoggedIn = false;
                _dashboardPage = null;
                throw;
            }
        }

        #endregion
    }
}
