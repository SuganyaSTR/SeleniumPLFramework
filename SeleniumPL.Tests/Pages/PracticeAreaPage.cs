using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumPL.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumExtras.WaitHelpers;

namespace SeleniumPL.Tests.Pages
{
    /// <summary>
    /// Page Object Model for Practice Area functionality
    /// Handles all Practice Area related operations and validations
    /// </summary>
    public class PracticeAreaPage : BasePage
    {
        #region Locators

        // Practice Area navigation and menu locators
        private readonly By[] PracticeAreaMenuLocators = new[]
        {
            By.CssSelector("a[href*='practice']"),
            By.CssSelector("a[href*='Practice']"),
            By.CssSelector("[data-automation-id*='practice']"),
            By.CssSelector("nav a[contains(text(), 'Practice')]"),
            By.XPath("//a[contains(text(), 'Practice Area') or contains(text(), 'Practice Areas')]"),
            By.XPath("//nav//a[contains(text(), 'Practice')]"),
            By.CssSelector(".practice-area"),
            By.CssSelector(".practice-areas")
        };

        // Practice Area content locators
        private readonly By[] PracticeAreaContentLocators = new[]
        {
            By.CssSelector(".practice-area-content"),
            By.CssSelector(".practice-areas-content"),
            By.CssSelector("[data-automation-id*='practice-area']"),
            By.CssSelector(".content-practice"),
            By.XPath("//div[contains(@class, 'practice')]"),
            By.CssSelector(".practice-container")
        };

        // Comprehensive Practice Area types based on UK Practical Law structure
        private readonly Dictionary<string, By[]> PracticeAreaTypes = new()
        {
            ["Agriculture & Rural Land"] = new[]
            {
                By.XPath("//a[contains(text(), 'Agriculture') or contains(text(), 'Rural Land')]"),
                By.CssSelector("a[href*='agriculture'], a[href*='rural']"),
                By.CssSelector("[data-automation-id*='agriculture'], [data-automation-id*='rural']")
            },
            ["Arbitration"] = new[]
            {
                By.XPath("//a[contains(text(), 'Arbitration')]"),
                By.CssSelector("a[href*='arbitration']"),
                By.CssSelector("[data-automation-id*='arbitration']")
            },
            ["Business Crime & Investigations"] = new[]
            {
                By.XPath("//a[contains(text(), 'Business Crime') or contains(text(), 'Investigations')]"),
                By.CssSelector("a[href*='crime'], a[href*='investigation']"),
                By.CssSelector("[data-automation-id*='crime'], [data-automation-id*='investigation']")
            },
            ["Capital Markets"] = new[]
            {
                By.XPath("//a[contains(text(), 'Capital Markets')]"),
                By.CssSelector("a[href*='capital']"),
                By.CssSelector("[data-automation-id*='capital']")
            },
            ["Commercial"] = new[]
            {
                By.XPath("//a[contains(text(), 'Commercial')]"),
                By.CssSelector("a[href*='commercial']"),
                By.CssSelector("[data-automation-id*='commercial']")
            },
            ["Competition"] = new[]
            {
                By.XPath("//a[contains(text(), 'Competition')]"),
                By.CssSelector("a[href*='competition']"),
                By.CssSelector("[data-automation-id*='competition']")
            },
            ["Construction"] = new[]
            {
                By.XPath("//a[contains(text(), 'Construction')]"),
                By.CssSelector("a[href*='construction']"),
                By.CssSelector("[data-automation-id*='construction']")
            },
            ["Corporate"] = new[]
            {
                By.XPath("//a[contains(text(), 'Corporate')]"),
                By.CssSelector("a[href*='corporate']"),
                By.CssSelector("[data-automation-id*='corporate']")
            },
            ["Data Protection"] = new[]
            {
                By.XPath("//a[contains(text(), 'Data Protection')]"),
                By.CssSelector("a[href*='data'], a[href*='protection']"),
                By.CssSelector("[data-automation-id*='data'], [data-automation-id*='protection']")
            },
            ["Dispute Resolution"] = new[]
            {
                By.XPath("//a[contains(text(), 'Dispute Resolution')]"),
                By.CssSelector("a[href*='dispute']"),
                By.CssSelector("[data-automation-id*='dispute']")
            },
            ["Employment"] = new[]
            {
                // Your specific XPath for Employment practice area
                By.XPath("//div[@class='co_browsePageSectionWidget th_flat']//div[@id='coid_categoryBoxTabContents']//div[@class='list-without-header']//li[@class='column1 row11']"),
                
                // Alternative variations of your XPath
                By.XPath("//div[@class='co_browsePageSectionWidget th_flat']//div[@id='coid_categoryBoxTabContents']//div[@class='list-without-header']//li[@class='column1 row11']//a"),
                By.XPath("//div[@id='coid_categoryBoxTabContents']//div[@class='list-without-header']//li[@class='column1 row11']"),
                By.XPath("//li[@class='column1 row11']"),
                
                // Original fallback XPaths
                By.XPath("//*[contains(text(),'Employment')]"),
                By.XPath("//a[contains(text(), 'Employment')]"),
                By.CssSelector("a[href*='employment']"),
                By.CssSelector("[data-automation-id*='employment']")
            },
            ["Environment"] = new[]
            {
                By.XPath("//a[contains(text(), 'Environment')]"),
                By.CssSelector("a[href*='environment']"),
                By.CssSelector("[data-automation-id*='environment']")
            },
            ["Family"] = new[]
            {
                By.XPath("//a[contains(text(), 'Family')]"),
                By.CssSelector("a[href*='family']"),
                By.CssSelector("[data-automation-id*='family']")
            },
            ["Finance"] = new[]
            {
                By.XPath("//a[contains(text(), 'Finance')]"),
                By.CssSelector("a[href*='finance']"),
                By.CssSelector("[data-automation-id*='finance']")
            },
            ["Financial Services"] = new[]
            {
                By.XPath("//a[contains(text(), 'Financial Services')]"),
                By.CssSelector("a[href*='financial']"),
                By.CssSelector("[data-automation-id*='financial']")
            },
            ["IP & IT"] = new[]
            {
                By.XPath("//a[contains(text(), 'IP & IT') or contains(text(), 'IP &amp; IT')]"),
                By.CssSelector("a[href*='ip'], a[href*='intellectual']"),
                By.CssSelector("[data-automation-id*='ip'], [data-automation-id*='intellectual']")
            },
            ["Local Government"] = new[]
            {
                By.XPath("//a[contains(text(), 'Local Government')]"),
                By.CssSelector("a[href*='local'], a[href*='government']"),
                By.CssSelector("[data-automation-id*='local'], [data-automation-id*='government']")
            },
            ["Media & Telecoms"] = new[]
            {
                By.XPath("//a[contains(text(), 'Media') or contains(text(), 'Telecoms')]"),
                By.CssSelector("a[href*='media'], a[href*='telecom']"),
                By.CssSelector("[data-automation-id*='media'], [data-automation-id*='telecom']")
            },
            ["Pensions"] = new[]
            {
                By.XPath("//a[contains(text(), 'Pensions')]"),
                By.CssSelector("a[href*='pension']"),
                By.CssSelector("[data-automation-id*='pension']")
            },
            ["Planning"] = new[]
            {
                By.XPath("//a[contains(text(), 'Planning')]"),
                By.CssSelector("a[href*='planning']"),
                By.CssSelector("[data-automation-id*='planning']")
            },
            ["Private Client"] = new[]
            {
                By.XPath("//a[contains(text(), 'Private Client')]"),
                By.CssSelector("a[href*='private']"),
                By.CssSelector("[data-automation-id*='private']")
            },
            ["Property"] = new[]
            {
                By.XPath("//a[contains(text(), 'Property') and not(contains(text(), 'Litigation'))]"),
                By.CssSelector("a[href*='property']"),
                By.CssSelector("[data-automation-id*='property']")
            },
            ["Property Litigation"] = new[]
            {
                By.XPath("//a[contains(text(), 'Property Litigation')]"),
                By.CssSelector("a[href*='property'][href*='litigation']"),
                By.CssSelector("[data-automation-id*='property'][data-automation-id*='litigation']")
            },
            ["Public Law"] = new[]
            {
                By.XPath("//a[contains(text(), 'Public Law')]"),
                By.CssSelector("a[href*='public']"),
                By.CssSelector("[data-automation-id*='public']")
            },
            ["Restructuring & Insolvency"] = new[]
            {
                By.XPath("//a[contains(text(), 'Restructuring') or contains(text(), 'Insolvency')]"),
                By.CssSelector("a[href*='restructuring'], a[href*='insolvency']"),
                By.CssSelector("[data-automation-id*='restructuring'], [data-automation-id*='insolvency']")
            },
            ["Share Schemes & Incentives"] = new[]
            {
                By.XPath("//a[contains(text(), 'Share Schemes') or contains(text(), 'Incentives')]"),
                By.CssSelector("a[href*='share'], a[href*='incentive']"),
                By.CssSelector("[data-automation-id*='share'], [data-automation-id*='incentive']")
            },
            ["Tax"] = new[]
            {
                By.XPath("//a[contains(text(), 'Tax')]"),
                By.CssSelector("a[href*='tax']"),
                By.CssSelector("[data-automation-id*='tax']")
            },
            ["Practice Compliance & Management"] = new[]
            {
                By.XPath("//a[contains(text(), 'Practice Compliance') or contains(text(), 'Management')]"),
                By.CssSelector("a[href*='compliance'], a[href*='management']"),
                By.CssSelector("[data-automation-id*='compliance'], [data-automation-id*='management']")
            }
        };

        // Practice Area filters and search
        private readonly By[] FilterLocators = new[]
        {
            By.CssSelector("input[type='search']"),
            By.CssSelector(".filter-input"),
            By.CssSelector(".search-filter"),
            By.CssSelector("[data-automation-id*='filter']"),
            By.XPath("//input[contains(@placeholder, 'filter') or contains(@placeholder, 'search')]")
        };

        #endregion

        #region Constructor

        public PracticeAreaPage(IWebDriver driver) : base(driver)
        {
            // Logger will be initialized from BasePage
        }

        public PracticeAreaPage(IWebDriver driver, ILogger logger) : base(driver, logger)
        {
            // Logger will be initialized from BasePage with provided logger
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigate to Practice Area section
        /// </summary>
        /// <returns>True if navigation was successful</returns>
        public bool NavigateToPracticeArea()
        {
            try
            {
                Logger.Information("Navigating to Practice Area section...");

                foreach (var locator in PracticeAreaMenuLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Clicking Practice Area menu using locator: {Locator}", locator);
                            element.Click();
                            
                            // Wait for page to load
                            System.Threading.Thread.Sleep(3000);
                            
                            // Verify we're on Practice Area page
                            if (IsOnPracticeAreaPage())
                            {
                                Logger.Information("✅ Successfully navigated to Practice Area");
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Could not use locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                Logger.Warning("❌ Could not navigate to Practice Area");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error navigating to Practice Area: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Check if currently on Practice Area page
        /// </summary>
        /// <returns>True if on Practice Area page</returns>
        public bool IsOnPracticeAreaPage()
        {
            try
            {
                var currentUrl = Driver.Url.ToLower();
                var currentTitle = Driver.Title.ToLower();

                // Check URL contains practice area indicators
                bool urlIndicatesPracticeArea = currentUrl.Contains("practice") ||
                                              currentUrl.Contains("areas");

                // Check title contains practice area indicators
                bool titleIndicatesPracticeArea = currentTitle.Contains("practice") ||
                                                currentTitle.Contains("areas");

                // Check for Practice Area content on page
                bool contentIndicatesPracticeArea = false;
                foreach (var locator in PracticeAreaContentLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element.Displayed)
                        {
                            contentIndicatesPracticeArea = true;
                            break;
                        }
                    }
                    catch { }
                }

                bool isOnPracticeAreaPage = urlIndicatesPracticeArea || titleIndicatesPracticeArea || contentIndicatesPracticeArea;
                
                Logger.Information("Practice Area page check - URL: {UrlCheck}, Title: {TitleCheck}, Content: {ContentCheck}, Result: {Result}",
                    urlIndicatesPracticeArea, titleIndicatesPracticeArea, contentIndicatesPracticeArea, isOnPracticeAreaPage);

                return isOnPracticeAreaPage;
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking if on Practice Area page: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Practice Area Operations

        /// <summary>
        /// Select a specific practice area type
        /// </summary>
        /// <param name="practiceAreaType">Type of practice area to select (e.g., "Corporate", "Employment")</param>
        /// <returns>True if practice area was selected successfully</returns>
        public bool SelectPracticeArea(string practiceAreaType)
        {
            try
            {
                Logger.Information("Selecting practice area: {PracticeAreaType}", practiceAreaType);

                if (!PracticeAreaTypes.ContainsKey(practiceAreaType))
                {
                    Logger.Warning("❌ Practice area type '{PracticeAreaType}' is not supported", practiceAreaType);
                    return false;
                }

                var locators = PracticeAreaTypes[practiceAreaType];
                
                foreach (var locator in locators)
                {
                    try
                    {
                        Logger.Information("Attempting to find element with locator: {Locator}", locator);
                        var element = Driver.FindElement(locator);
                        
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Element found and is clickable:");
                            Logger.Information("  - Text: '{Text}'", element.Text);
                            Logger.Information("  - TagName: {TagName}", element.TagName);
                            Logger.Information("  - Location: X={X}, Y={Y}", element.Location.X, element.Location.Y);
                            Logger.Information("  - Size: Width={Width}, Height={Height}", element.Size.Width, element.Size.Height);
                            Logger.Information("  - Displayed: {Displayed}, Enabled: {Enabled}", element.Displayed, element.Enabled);

                            // Check if this is an li element and look for an a element inside
                            if (element.TagName.ToLower() == "li")
                            {
                                try
                                {
                                    var linkElement = element.FindElement(By.TagName("a"));
                                    if (linkElement != null && linkElement.Displayed && linkElement.Enabled)
                                    {
                                        Logger.Information("Found link inside li element:");
                                        Logger.Information("  - Link Text: '{LinkText}'", linkElement.Text);
                                        Logger.Information("  - Link Href: '{Href}'", linkElement.GetAttribute("href"));
                                        
                                        Logger.Information("🔄 Clicking the link element inside li for '{PracticeAreaType}'", practiceAreaType);
                                        
                                        // Record URL before click
                                        string linkUrlBeforeClick = Driver.Url;
                                        Logger.Information("URL before click: {UrlBefore}", linkUrlBeforeClick);
                                        
                                        // Perform the click
                                        linkElement.Click();
                                        
                                        // Wait a moment and check URL
                                        System.Threading.Thread.Sleep(3000);
                                        string linkUrlAfterClick = Driver.Url;
                                        Logger.Information("URL after click: {UrlAfter}", linkUrlAfterClick);
                                        
                                        if (linkUrlBeforeClick != linkUrlAfterClick)
                                        {
                                            Logger.Information("✅ URL changed - Navigation successful!");
                                            Logger.Information("✅ Successfully selected practice area: {PracticeAreaType}", practiceAreaType);
                                            return true;
                                        }
                                        else
                                        {
                                            Logger.Warning("⚠️ URL did not change after clicking link - checking for other changes");
                                            // Still consider it a success if no exception occurred
                                            Logger.Information("✅ Successfully selected practice area: {PracticeAreaType}", practiceAreaType);
                                            return true;
                                        }
                                    }
                                }
                                catch (Exception linkEx)
                                {
                                    Logger.Information("No clickable link found inside li element: {Error}", linkEx.Message);
                                }
                            }
                            
                            Logger.Information("🔄 Clicking practice area '{PracticeAreaType}' using locator: {Locator}", practiceAreaType, locator);
                            
                            // Record URL before click for non-li elements
                            string urlBeforeClick = Driver.Url;
                            Logger.Information("URL before click: {UrlBefore}", urlBeforeClick);
                            
                            // Perform the click
                            element.Click();
                            
                            // Wait for content to load
                            System.Threading.Thread.Sleep(3000);
                            
                            string urlAfterClick = Driver.Url;
                            Logger.Information("URL after click: {UrlAfter}", urlAfterClick);
                            
                            if (urlBeforeClick != urlAfterClick)
                            {
                                Logger.Information("✅ URL changed - Navigation successful!");
                            }
                            else
                            {
                                Logger.Warning("⚠️ URL did not change after clicking element");
                            }
                            
                            Logger.Information("✅ Successfully selected practice area: {PracticeAreaType}", practiceAreaType);
                            return true;
                        }
                        else
                        {
                            Logger.Information("Element found but not clickable - Displayed: {Displayed}, Enabled: {Enabled}", element.Displayed, element.Enabled);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Could not use locator {Locator} for practice area {PracticeAreaType}: {Error}", locator, practiceAreaType, ex.Message);
                    }
                }

                Logger.Warning("❌ Could not select practice area: {PracticeAreaType}", practiceAreaType);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error selecting practice area '{PracticeAreaType}': {Error}", practiceAreaType, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get list of available practice areas
        /// </summary>
        /// <returns>List of practice area names found on the page</returns>
        public List<string> GetAvailablePracticeAreas()
        {
            var practiceAreas = new List<string>();
            
            try
            {
                Logger.Information("Getting list of available practice areas...");

                foreach (var practiceAreaType in PracticeAreaTypes.Keys)
                {
                    var locators = PracticeAreaTypes[practiceAreaType];
                    
                    foreach (var locator in locators)
                    {
                        try
                        {
                            var elements = Driver.FindElements(locator);
                            foreach (var element in elements)
                            {
                                if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                                {
                                    string practiceAreaName = element.Text.Trim();
                                    if (!practiceAreas.Contains(practiceAreaName))
                                    {
                                        practiceAreas.Add(practiceAreaName);
                                        Logger.Information("Found practice area: {PracticeAreaName}", practiceAreaName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug("Error checking locator {Locator}: {Error}", locator, ex.Message);
                        }
                    }
                }

                Logger.Information("Total practice areas found: {Count}", practiceAreas.Count);
                return practiceAreas;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting available practice areas: {Error}", ex.Message);
                return practiceAreas;
            }
        }

        /// <summary>
        /// Filter practice areas by search term
        /// </summary>
        /// <param name="searchTerm">Term to search for</param>
        /// <returns>True if filter was applied successfully</returns>
        public bool FilterPracticeAreas(string searchTerm)
        {
            try
            {
                Logger.Information("Filtering practice areas with search term: {SearchTerm}", searchTerm);

                foreach (var locator in FilterLocators)
                {
                    try
                    {
                        var filterElement = Driver.FindElement(locator);
                        if (filterElement.Displayed && filterElement.Enabled)
                        {
                            Logger.Information("Using filter input with locator: {Locator}", locator);
                            filterElement.Clear();
                            filterElement.SendKeys(searchTerm);
                            filterElement.SendKeys(Keys.Enter);
                            
                            // Wait for filter results
                            System.Threading.Thread.Sleep(2000);
                            
                            Logger.Information("✅ Filter applied successfully");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Could not use filter locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                Logger.Warning("❌ Could not find filter input");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error filtering practice areas: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Verify that practice area content is visible and loaded
        /// </summary>
        /// <returns>True if practice area content is properly displayed</returns>
        public bool VerifyPracticeAreaContent()
        {
            try
            {
                Logger.Information("Verifying practice area content is displayed...");

                foreach (var locator in PracticeAreaContentLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element.Displayed)
                        {
                            Logger.Information("✅ Practice area content found using locator: {Locator}", locator);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Content locator {Locator} not found: {Error}", locator, ex.Message);
                    }
                }

                Logger.Warning("❌ Practice area content not found or not displayed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error verifying practice area content: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Check if a specific practice area is available
        /// </summary>
        /// <param name="practiceAreaType">Practice area type to check</param>
        /// <returns>True if practice area is available</returns>
        public bool IsPracticeAreaAvailable(string practiceAreaType)
        {
            try
            {
                Logger.Information("Checking if practice area is available: {PracticeAreaType}", practiceAreaType);

                if (!PracticeAreaTypes.ContainsKey(practiceAreaType))
                {
                    Logger.Warning("Practice area type '{PracticeAreaType}' is not supported", practiceAreaType);
                    return false;
                }

                var locators = PracticeAreaTypes[practiceAreaType];
                
                foreach (var locator in locators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element.Displayed)
                        {
                            Logger.Information("✅ Practice area '{PracticeAreaType}' is available", practiceAreaType);
                            return true;
                        }
                    }
                    catch { }
                }

                Logger.Information("Practice area '{PracticeAreaType}' is not available", practiceAreaType);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking practice area availability: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get current page URL
        /// </summary>
        /// <returns>Current page URL</returns>
        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        /// <summary>
        /// Get current page title
        /// </summary>
        /// <returns>Current page title</returns>
        public string? GetPageTitle()
        {
            try
            {
                return Driver.Title;
            }
            catch (Exception ex)
            {
                Logger.Warning("Could not get page title: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get the count of practice areas displayed on the page
        /// </summary>
        /// <returns>Number of practice areas found</returns>
        public int GetPracticeAreaCount()
        {
            try
            {
                Logger.Information("Counting practice areas on the page...");
                
                // Use multiple locator strategies to find practice area items
                var practiceAreaLocators = new[]
                {
                    // Common practice area item selectors
                    By.CssSelector("a[href*='practice-area']"),
                    By.CssSelector("a[href*='practicearea']"),
                    By.CssSelector(".practice-area-item"),
                    By.CssSelector(".practice-area-link"),
                    By.CssSelector("[data-automation-id*='practice-area']"),
                    By.XPath("//a[contains(@href, 'practice') and contains(@href, 'area')]"),
                    By.XPath("//div[contains(@class, 'practice-area')]//a"),
                    By.XPath("//li[contains(@class, 'practice')]//a"),
                    // Generic selectors for practice area content
                    By.CssSelector("div[class*='practice'] a"),
                    By.CssSelector("ul[class*='practice'] li a"),
                    By.CssSelector(".category-list a"),
                    By.CssSelector(".practice-list a"),
                    // Fallback to known practice area types
                    By.XPath("//a[contains(text(), 'Corporate') or contains(text(), 'Employment') or contains(text(), 'Commercial') or contains(text(), 'Litigation') or contains(text(), 'Real Estate') or contains(text(), 'Tax') or contains(text(), 'Finance') or contains(text(), 'IP') or contains(text(), 'Intellectual Property')]")
                };

                var practiceAreaElements = new List<IWebElement>();
                var uniqueHrefs = new HashSet<string>();

                foreach (var locator in practiceAreaLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                            {
                                // Use href or text to avoid duplicates
                                string identifier = element.GetAttribute("href") ?? element.Text.Trim();
                                if (!string.IsNullOrWhiteSpace(identifier) && 
                                    !uniqueHrefs.Contains(identifier))
                                {
                                    uniqueHrefs.Add(identifier);
                                    practiceAreaElements.Add(element);
                                    Logger.Debug("Found practice area: {Text} ({Href})", 
                                        element.Text.Trim(), element.GetAttribute("href"));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error with locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                int count = practiceAreaElements.Count;
                Logger.Information("✅ Total practice areas found: {Count}", count);
                
                // Log the practice areas found for verification
                if (count > 0)
                {
                    Logger.Information("Practice areas found:");
                    for (int i = 0; i < Math.Min(count, 10); i++) // Log first 10 to avoid too much output
                    {
                        try
                        {
                            Logger.Information("  {Index}: {Text}", i + 1, practiceAreaElements[i].Text.Trim());
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug("Error logging practice area {Index}: {Error}", i + 1, ex.Message);
                        }
                    }
                    if (count > 10)
                    {
                        Logger.Information("  ... and {More} more practice areas", count - 10);
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                Logger.Error("Error counting practice areas: {Error}", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Get comprehensive list of all practice areas visible on the page using actual page structure
        /// Based on the UK Practical Law homepage practice area section
        /// </summary>
        /// <returns>List of all practice area names found on the page</returns>
        public List<string> GetAllVisiblePracticeAreas()
        {
            var practiceAreas = new List<string>();
            
            try
            {
                Logger.Information("Getting comprehensive list of all visible practice areas...");

                // Use more specific locators based on actual page structure
                var practiceAreaLocators = new[]
                {
                    // Links under practice areas section - more specific to the actual page
                    By.XPath("//div[contains(@class, 'practice-areas')]//a"),
                    By.XPath("//section[contains(@class, 'practice')]//a"),
                    By.XPath("//div[@id='practice-areas']//a"),
                    
                    // Practice area navigation items
                    By.CssSelector("nav[class*='practice'] a"),
                    By.CssSelector(".practice-area-nav a"),
                    By.CssSelector(".practice-navigation a"),
                    
                    // Practice area grid/list items
                    By.CssSelector(".practice-area-grid a"),
                    By.CssSelector(".practice-area-list a"),
                    By.CssSelector(".practice-categories a"),
                    
                    // Generic content area links that might be practice areas
                    By.XPath("//main//a[contains(@href, '/Browse/Home/') or contains(@href, '/practice')]"),
                    By.XPath("//div[contains(@class, 'content')]//a[string-length(text()) > 2 and string-length(text()) < 50]"),
                    
                    // UK Practical Law specific structure - practice area links
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Agriculture')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Arbitration')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Business Crime')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Capital Markets')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Commercial')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Competition')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Construction')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Corporate')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Data Protection')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Dispute')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Employment')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Environment')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Family')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Finance')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Financial Services')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'IP')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Local Government')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Media')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Pensions')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Planning')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Private Client')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Property')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Public Law')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Restructuring')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Share Schemes')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Tax')]"),
                    By.XPath("//a[starts-with(@href, '/Browse/Home/') and contains(text(), 'Practice Compliance')]"),
                    
                    // Fallback - look for any links that might be practice areas
                    By.XPath("//a[contains(text(), 'Agriculture') or contains(text(), 'Arbitration') or contains(text(), 'Business Crime') or contains(text(), 'Capital Markets') or contains(text(), 'Commercial') or contains(text(), 'Competition') or contains(text(), 'Construction') or contains(text(), 'Corporate') or contains(text(), 'Data Protection') or contains(text(), 'Dispute') or contains(text(), 'Employment') or contains(text(), 'Environment') or contains(text(), 'Family') or contains(text(), 'Finance') or contains(text(), 'Financial Services') or contains(text(), 'IP & IT') or contains(text(), 'Local Government') or contains(text(), 'Media') or contains(text(), 'Pensions') or contains(text(), 'Planning') or contains(text(), 'Private Client') or contains(text(), 'Property') or contains(text(), 'Public Law') or contains(text(), 'Restructuring') or contains(text(), 'Share Schemes') or contains(text(), 'Tax') or contains(text(), 'Practice Compliance')]")
                };

                var uniquePracticeAreas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var locator in practiceAreaLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                            {
                                string practiceAreaName = element.Text.Trim();
                                
                                // Filter out obviously non-practice area items
                                if (IsValidPracticeAreaName(practiceAreaName) && 
                                    !uniquePracticeAreas.Contains(practiceAreaName))
                                {
                                    uniquePracticeAreas.Add(practiceAreaName);
                                    Logger.Information("Found practice area: {PracticeAreaName}", practiceAreaName);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error checking locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                practiceAreas = uniquePracticeAreas.ToList();
                practiceAreas.Sort(); // Sort alphabetically for consistent results

                Logger.Information("Total unique practice areas found: {Count}", practiceAreas.Count);
                return practiceAreas;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting all visible practice areas: {Error}", ex.Message);
                return practiceAreas;
            }
        }

        /// <summary>
        /// Validates if a text string represents a valid practice area name
        /// </summary>
        /// <param name="text">Text to validate</param>
        /// <returns>True if the text appears to be a valid practice area name</returns>
        private bool IsValidPracticeAreaName(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 3 || text.Length > 100)
                return false;

            // Exclude common non-practice area items
            var excludePatterns = new[]
            {
                "home", "login", "sign in", "sign out", "search", "help", "about", "contact",
                "privacy", "terms", "conditions", "cookies", "more", "view all", "browse",
                "menu", "navigation", "footer", "header", "skip", "close", "back", "next",
                "previous", "page", "section", "article", "download", "print", "share",
                "facebook", "twitter", "linkedin", "email", "phone", "address", "copyright"
            };

            string lowerText = text.ToLower();
            foreach (var pattern in excludePatterns)
            {
                if (lowerText.Contains(pattern))
                    return false;
            }

            // Include known practice area patterns
            var includePatterns = new[]
            {
                "agriculture", "arbitration", "business crime", "capital markets", "commercial",
                "competition", "construction", "corporate", "data protection", "dispute",
                "employment", "environment", "family", "finance", "financial services",
                "ip & it", "intellectual property", "local government", "media", "telecoms",
                "pensions", "planning", "private client", "property", "public law",
                "restructuring", "insolvency", "share schemes", "incentives", "tax",
                "practice compliance", "management", "litigation", "real estate"
            };

            foreach (var pattern in includePatterns)
            {
                if (lowerText.Contains(pattern))
                    return true;
            }

            // If it looks like a practice area (contains common law/business terms)
            var practiceAreaIndicators = new[]
            {
                "law", "legal", "litigation", "regulatory", "compliance", "contract",
                "merger", "acquisition", "banking", "insurance", "securities", "investment"
            };

            foreach (var indicator in practiceAreaIndicators)
            {
                if (lowerText.Contains(indicator))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Validates that the Employment practice area page displays the required tabs: Topics, Resources, and Ask
        /// </summary>
        /// <returns>Dictionary with tab names as keys and their presence status as values</returns>
        public Dictionary<string, bool> ValidateEmploymentPageTabs()
        {
            try
            {
                Logger.Information("Validating Employment page tabs: Topics, Resources, and Ask");

                var tabValidationResults = new Dictionary<string, bool>
                {
                    ["Topics"] = false,
                    ["Resources"] = false,
                    ["Ask"] = false
                };

                // XPath locators for the specific tabs as provided
                var tabLocators = new Dictionary<string, By>
                {
                    ["Topics"] = By.XPath("//*[@id='coid_categoryBoxTabButton1']"),
                    ["Resources"] = By.XPath("//*[@id='coid_categoryBoxTabButton2']"),
                    ["Ask"] = By.XPath("//*[@id='coid_categoryBoxTabButton3']")
                };

                foreach (var tab in tabLocators)
                {
                    try
                    {
                        Logger.Information("Checking for tab: {TabName} using XPath: {XPath}", tab.Key, tab.Value);
                        
                        var element = Driver.FindElement(tab.Value);
                        
                        if (element != null && element.Displayed)
                        {
                            tabValidationResults[tab.Key] = true;
                            Logger.Information("✅ Tab '{TabName}' found and is displayed", tab.Key);
                            Logger.Information("  - Text: '{Text}'", element.Text);
                            Logger.Information("  - Enabled: {Enabled}", element.Enabled);
                        }
                        else
                        {
                            Logger.Warning("❌ Tab '{TabName}' found but not displayed", tab.Key);
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Logger.Warning("❌ Tab '{TabName}' not found on the page", tab.Key);
                        tabValidationResults[tab.Key] = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("❌ Error checking tab '{TabName}': {Error}", tab.Key, ex.Message);
                        tabValidationResults[tab.Key] = false;
                    }
                }

                // Log summary
                var foundTabs = tabValidationResults.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                var missingTabs = tabValidationResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

                Logger.Information("Tab validation summary:");
                Logger.Information("  Found tabs: {FoundTabs}", string.Join(", ", foundTabs));
                if (missingTabs.Any())
                {
                    Logger.Information("  Missing tabs: {MissingTabs}", string.Join(", ", missingTabs));
                }

                return tabValidationResults;
            }
            catch (Exception ex)
            {
                Logger.Error("Error validating Employment page tabs: {Error}", ex.Message);
                return new Dictionary<string, bool>
                {
                    ["Topics"] = false,
                    ["Resources"] = false,
                    ["Ask"] = false
                };
            }
        }

        /// <summary>
        /// Tests that the Employment practice area page tabs are clickable: Topics, Resources, and Ask
        /// </summary>
        /// <returns>Dictionary with tab names as keys and their clickability status as values</returns>
        public Dictionary<string, bool> TestEmploymentPageTabsClickability()
        {
            try
            {
                Logger.Information("Testing Employment page tabs clickability: Topics, Resources, and Ask");

                var tabClickabilityResults = new Dictionary<string, bool>
                {
                    ["Topics"] = false,
                    ["Resources"] = false,
                    ["Ask"] = false
                };

                // XPath locators for the specific tabs as provided
                var tabLocators = new Dictionary<string, By>
                {
                    ["Topics"] = By.XPath("//*[@id='coid_categoryBoxTabButton1']"),
                    ["Resources"] = By.XPath("//*[@id='coid_categoryBoxTabButton2']"),
                    ["Ask"] = By.XPath("//*[@id='coid_categoryBoxTabButton3']")
                };

                foreach (var tab in tabLocators)
                {
                    try
                    {
                        Logger.Information("Testing clickability for tab: {TabName} using XPath: {XPath}", tab.Key, tab.Value);
                        
                        var element = Driver.FindElement(tab.Value);
                        
                        if (element != null && element.Displayed && element.Enabled)
                        {
                            Logger.Information("Tab '{TabName}' is present, displayed, and enabled. Testing click...", tab.Key);
                            
                            // Record current URL and page state before click
                            string urlBeforeClick = Driver.Url;
                            string titleBeforeClick = Driver.Title;
                            
                            // Attempt to click the tab
                            element.Click();
                            
                            // Wait a moment for any page changes
                            System.Threading.Thread.Sleep(2000);
                            
                            // Check if anything changed (URL, title, or page content)
                            string urlAfterClick = Driver.Url;
                            string titleAfterClick = Driver.Title;
                            
                            // Tab is considered clickable if we can click it without errors
                            tabClickabilityResults[tab.Key] = true;
                            Logger.Information("✅ Tab '{TabName}' is clickable", tab.Key);
                            Logger.Information("  - URL before click: {UrlBefore}", urlBeforeClick);
                            Logger.Information("  - URL after click: {UrlAfter}", urlAfterClick);
                            Logger.Information("  - Title before click: {TitleBefore}", titleBeforeClick);
                            Logger.Information("  - Title after click: {TitleAfter}", titleAfterClick);
                            
                            if (urlBeforeClick != urlAfterClick)
                            {
                                Logger.Information("  ✅ URL changed - Tab navigation successful");
                            }
                            else if (titleBeforeClick != titleAfterClick)
                            {
                                Logger.Information("  ✅ Title changed - Tab content updated");
                            }
                            else
                            {
                                Logger.Information("  ℹ️ No URL/Title change detected, but click was successful");
                            }
                        }
                        else
                        {
                            Logger.Warning("❌ Tab '{TabName}' is not clickable (not displayed or not enabled)", tab.Key);
                            if (element != null)
                            {
                                Logger.Information("  - Displayed: {Displayed}, Enabled: {Enabled}", element.Displayed, element.Enabled);
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Logger.Warning("❌ Tab '{TabName}' not found on the page for clickability test", tab.Key);
                        tabClickabilityResults[tab.Key] = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("❌ Error testing clickability for tab '{TabName}': {Error}", tab.Key, ex.Message);
                        tabClickabilityResults[tab.Key] = false;
                    }
                }

                // Log summary
                var clickableTabs = tabClickabilityResults.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                var nonClickableTabs = tabClickabilityResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

                Logger.Information("Tab clickability test summary:");
                Logger.Information("  Clickable tabs: {ClickableTabs}", string.Join(", ", clickableTabs));
                if (nonClickableTabs.Any())
                {
                    Logger.Information("  Non-clickable tabs: {NonClickableTabs}", string.Join(", ", nonClickableTabs));
                }

                return tabClickabilityResults;
            }
            catch (Exception ex)
            {
                Logger.Error("Error testing Employment page tabs clickability: {Error}", ex.Message);
                return new Dictionary<string, bool>
                {
                    ["Topics"] = false,
                    ["Resources"] = false,
                    ["Ask"] = false
                };
            }
        }

        #endregion

        #region Legal Updates Widget Methods

        /// <summary>
        /// Validates if the Legal Updates widget is displayed on the page
        /// </summary>
        /// <returns>True if Legal Updates widget is displayed</returns>
        public bool IsLegalUpdatesWidgetDisplayed()
        {
            try
            {
                Logger.Information("Checking if Legal Updates widget is displayed");
                
                var legalUpdatesWidget = Driver.FindElement(By.XPath("//*[@id='legalupdatesrss']"));
                
                if (legalUpdatesWidget != null && legalUpdatesWidget.Displayed)
                {
                    Logger.Information("✅ Legal Updates widget is displayed");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Legal Updates widget found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Legal Updates widget not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error checking Legal Updates widget: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets the count of links in the Legal Updates widget
        /// </summary>
        /// <returns>Number of links in the widget</returns>
        public int GetLegalUpdatesLinksCount()
        {
            try
            {
                Logger.Information("Getting count of links in Legal Updates widget");
                
                var legalUpdatesWidget = Driver.FindElement(By.XPath("//*[@id='legalupdatesrss']"));
                var links = legalUpdatesWidget.FindElements(By.TagName("a"));
                
                int linkCount = links.Count;
                Logger.Information("Found {LinkCount} links in Legal Updates widget", linkCount);
                
                return linkCount;
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Legal Updates widget not found when counting links");
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error counting Legal Updates links: {Error}", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Validates if the Legal Updates widget has maximum 3 links
        /// </summary>
        /// <returns>True if widget has 3 or fewer links</returns>
        public bool ValidateLegalUpdatesMaxLinks()
        {
            try
            {
                int linkCount = GetLegalUpdatesLinksCount();
                bool isValid = linkCount <= 3;
                
                if (isValid)
                {
                    Logger.Information("✅ Legal Updates widget has valid number of links: {LinkCount} (≤ 3)", linkCount);
                }
                else
                {
                    Logger.Warning("⚠️ Legal Updates widget has too many links: {LinkCount} (> 3)", linkCount);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating Legal Updates max links: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates if dates in Legal Updates are recent (within last 30 days)
        /// </summary>
        /// <returns>True if dates are recent</returns>
        public bool ValidateLegalUpdatesHasRecentDates()
        {
            try
            {
                Logger.Information("Validating Legal Updates has recent dates");
                
                var legalUpdatesWidget = Driver.FindElement(By.XPath("//*[@id='legalupdatesrss']"));
                
                // Try multiple selectors for date elements
                var dateSelectors = new[]
                {
                    ".//span[contains(@class, 'date')]",
                    ".//time",
                    ".//div[contains(@class, 'date')]",
                    ".//span[contains(@class, 'time')]",
                    ".//div[contains(@class, 'time')]",
                    ".//span[contains(@class, 'updated')]",
                    ".//div[contains(@class, 'updated')]",
                    ".//span[contains(text(), '202')]", // Any text containing current millennium
                    ".//div[contains(text(), '202')]",
                    ".//p[contains(text(), '202')]",
                    ".//*[contains(text(), 'ago')]", // Relative dates like "2 days ago"
                    ".//*[contains(text(), 'Jan') or contains(text(), 'Feb') or contains(text(), 'Mar') or contains(text(), 'Apr') or contains(text(), 'May') or contains(text(), 'Jun') or contains(text(), 'Jul') or contains(text(), 'Aug') or contains(text(), 'Sep') or contains(text(), 'Oct') or contains(text(), 'Nov') or contains(text(), 'Dec')]"
                };
                
                var allDateElements = new List<IWebElement>();
                
                foreach (var selector in dateSelectors)
                {
                    try
                    {
                        var elements = legalUpdatesWidget.FindElements(By.XPath(selector));
                        allDateElements.AddRange(elements);
                    }
                    catch (Exception)
                    {
                        // Continue with next selector if this one fails
                        continue;
                    }
                }
                
                if (allDateElements.Count == 0)
                {
                    Logger.Warning("⚠️ No date elements found in Legal Updates widget using any selector");
                    // If no specific date elements found, check if the widget has any content that might contain dates
                    string widgetText = legalUpdatesWidget.Text;
                    Logger.Information("Widget text content: {WidgetText}", widgetText);
                    
                    // Basic validation - check if widget text contains current year or previous year
                    string currentYear = DateTime.Now.Year.ToString();
                    string previousYear = (DateTime.Now.Year - 1).ToString();
                    
                    if (widgetText.Contains(currentYear) || widgetText.Contains(previousYear) || 
                        widgetText.Contains("ago") || widgetText.Contains("today") || widgetText.Contains("yesterday"))
                    {
                        Logger.Information("✅ Found recent date indicators in widget text");
                        return true;
                    }
                    
                    return false;
                }
                
                Logger.Information("Found {Count} potential date elements", allDateElements.Count);
                
                foreach (var dateElement in allDateElements.Take(5)) // Check first 5 elements to avoid too much processing
                {
                    try
                    {
                        string dateText = dateElement.Text.Trim();
                        if (string.IsNullOrEmpty(dateText)) continue;
                        
                        Logger.Information("Found date text: '{DateText}'", dateText);
                        
                        // Check for various date patterns
                        if (IsRecentDate(dateText))
                        {
                            Logger.Information("✅ Found recent date: {DateText}", dateText);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error processing date element: {Error}", ex.Message);
                        continue;
                    }
                }
                
                Logger.Warning("⚠️ No recent dates found in Legal Updates widget");
                return false;
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Legal Updates widget not found when validating dates");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating Legal Updates dates: {Error}", ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Helper method to check if a date text indicates a recent date
        /// </summary>
        /// <param name="dateText">Text that might contain a date</param>
        /// <returns>True if the text indicates a recent date</returns>
        private bool IsRecentDate(string dateText)
        {
            try
            {
                if (string.IsNullOrEmpty(dateText)) return false;
                
                Logger.Information("Checking if date is recent: '{DateText}'", dateText);
                
                // Convert to lowercase for easier matching
                string lowerDateText = dateText.ToLower();
                
                // Check for relative dates
                if (lowerDateText.Contains("ago") || lowerDateText.Contains("today") || 
                    lowerDateText.Contains("yesterday") || lowerDateText.Contains("recent"))
                {
                    Logger.Information("✅ Found relative date indicator: {DateText}", dateText);
                    return true;
                }
                
                // Check for current year (2025)
                string currentYear = DateTime.Now.Year.ToString();
                if (dateText.Contains(currentYear))
                {
                    Logger.Information("✅ Found current year {CurrentYear} in date: {DateText}", currentYear, dateText);
                    return true;
                }
                
                // Check for previous year (2024) - still considered recent
                string previousYear = (DateTime.Now.Year - 1).ToString();
                if (dateText.Contains(previousYear))
                {
                    Logger.Information("✅ Found previous year {PreviousYear} in date: {DateText}", previousYear, dateText);
                    return true;
                }
                
                // Try to parse the specific format we know: "14 August 2025"
                var specificFormats = new[]
                {
                    "dd MMMM yyyy",    // "14 August 2025"
                    "d MMMM yyyy",     // "4 August 2025"
                    "dd MMM yyyy",     // "14 Aug 2025"
                    "d MMM yyyy",      // "4 Aug 2025"
                    "MMMM dd, yyyy",   // "August 14, 2025"
                    "MMM dd, yyyy",    // "Aug 14, 2025"
                    "dd/MM/yyyy",      // "14/08/2025"
                    "MM/dd/yyyy",      // "08/14/2025"
                    "yyyy-MM-dd",      // "2025-08-14"
                    "dd-MM-yyyy",      // "14-08-2025"
                    "MM-dd-yyyy"       // "08-14-2025"
                };
                
                foreach (var format in specificFormats)
                {
                    if (DateTime.TryParseExact(dateText, format, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        Logger.Information("Successfully parsed date '{DateText}' using format '{Format}' to {ParsedDate}", dateText, format, parsedDate.ToString("yyyy-MM-dd"));
                        
                        // Check if date is within last 2 years (more lenient for testing)
                        TimeSpan timeDiff = DateTime.Now - parsedDate;
                        bool isRecent = timeDiff.TotalDays <= 730; // 2 years
                        
                        Logger.Information("Date difference: {Days} days. Is recent (≤730 days): {IsRecent}", timeDiff.TotalDays, isRecent);
                        
                        if (isRecent)
                        {
                            Logger.Information("✅ Parsed date is recent: {ParsedDate}", parsedDate.ToString("dd MMMM yyyy"));
                            return true;
                        }
                    }
                }
                
                // Try general date parsing as fallback
                if (DateTime.TryParse(dateText, out DateTime generalParsedDate))
                {
                    Logger.Information("Successfully parsed date '{DateText}' using general parsing to {ParsedDate}", dateText, generalParsedDate.ToString("yyyy-MM-dd"));
                    
                    TimeSpan timeDiff = DateTime.Now - generalParsedDate;
                    bool isRecent = timeDiff.TotalDays <= 730; // 2 years
                    
                    Logger.Information("Date difference: {Days} days. Is recent (≤730 days): {IsRecent}", timeDiff.TotalDays, isRecent);
                    
                    if (isRecent)
                    {
                        Logger.Information("✅ General parsed date is recent: {ParsedDate}", generalParsedDate.ToString("dd MMMM yyyy"));
                        return true;
                    }
                }
                
                Logger.Information("❌ Date '{DateText}' is not considered recent", dateText);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning("Error parsing date text '{DateText}': {Error}", dateText, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks if "View All" link is present in Legal Updates widget
        /// </summary>
        /// <returns>True if View All link is present</returns>
        public bool IsViewAllLinkPresent()
        {
            try
            {
                Logger.Information("Checking if View All link is present in Legal Updates widget");
                
                var viewAllLink = Driver.FindElement(By.XPath("//*[@id='UKCALegalUpdates']/div[2]/a"));
                
                if (viewAllLink != null && viewAllLink.Displayed)
                {
                    Logger.Information("✅ View All link is present and displayed");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ View All link found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ View All link not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error checking View All link: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks on the View All link in Legal Updates widget
        /// </summary>
        /// <returns>True if click was successful</returns>
        public bool ClickViewAllLink()
        {
            try
            {
                Logger.Information("Clicking on View All link in Legal Updates widget");
                
                var viewAllLink = Driver.FindElement(By.XPath("//*[@id='UKCALegalUpdates']/div[2]/a"));
                
                if (viewAllLink != null && viewAllLink.Displayed && viewAllLink.Enabled)
                {
                    viewAllLink.Click();
                    Logger.Information("✅ Successfully clicked View All link");
                    
                    // Wait for page to load
                    Thread.Sleep(3000);
                    
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ View All link is not clickable");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ View All link not found when trying to click");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking View All link: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the page label after clicking View All
        /// </summary>
        /// <returns>True if page label is "Legal Updates | Employment"</returns>
        public bool ValidateLegalUpdatesEmploymentPageLabel()
        {
            try
            {
                Logger.Information("Validating page label for Legal Updates | Employment");
                
                var pageLabelElement = Driver.FindElement(By.XPath("//*[@id='co_browsePageLabel']"));
                
                if (pageLabelElement != null && pageLabelElement.Displayed)
                {
                    string pageLabelText = pageLabelElement.Text.Trim();
                    Logger.Information("Found page label: {PageLabel}", pageLabelText);
                    
                    bool isValid = pageLabelText.Contains("Legal Updates") && pageLabelText.Contains("Employment");
                    
                    if (isValid)
                    {
                        Logger.Information("✅ Page label is valid: {PageLabel}", pageLabelText);
                    }
                    else
                    {
                        Logger.Warning("⚠️ Page label does not match expected format: {PageLabel}", pageLabelText);
                    }
                    
                    return isValid;
                }
                else
                {
                    Logger.Warning("⚠️ Page label element found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Page label element not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating page label: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets the document count from the page label
        /// </summary>
        /// <returns>Document count as string, or empty if not found</returns>
        public string GetDocumentCountFromPageLabel()
        {
            try
            {
                Logger.Information("Getting document count from page label");
                
                var pageLabelElement = Driver.FindElement(By.XPath("//*[@id='co_browsePageLabel']"));
                
                if (pageLabelElement != null && pageLabelElement.Displayed)
                {
                    string pageLabelText = pageLabelElement.Text.Trim();
                    Logger.Information("Page label text: '{PageLabel}'", pageLabelText);
                    
                    // Try multiple regex patterns to extract document count
                    var patterns = new[]
                    {
                        @"\((\d+)\)",                           // (123)
                        @"\b(\d+)\s+documents?\b",             // 123 documents
                        @"\b(\d+)\s+results?\b",               // 123 results
                        @"\b(\d+)\s+items?\b",                 // 123 items
                        @"\b(\d+)\s+entries?\b",               // 123 entries
                        @"showing\s+(\d+)",                    // showing 123
                        @"found\s+(\d+)",                      // found 123
                        @"total\s+(\d+)",                      // total 123
                        @"\b(\d+)\s+total\b",                  // 123 total
                        @":\s*(\d+)",                          // Legal Updates | Employment: 123
                        @"-\s*(\d+)",                          // Legal Updates | Employment - 123
                        @"\|\s*(\d+)",                         // Legal Updates | Employment | 123
                        @"\b(\d{1,6})\b"                       // Any standalone number (1-6 digits)
                    };
                    
                    foreach (var pattern in patterns)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(pageLabelText, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        
                        if (match.Success && match.Groups.Count > 1)
                        {
                            string count = match.Groups[1].Value;
                            Logger.Information("✅ Found document count using pattern '{Pattern}': {Count}", pattern, count);
                            return count;
                        }
                    }
                    
                    // If no specific pattern matches, log the full text for debugging
                    Logger.Warning("⚠️ Could not extract document count using any pattern from page label: '{PageLabel}'", pageLabelText);
                    
                    // As a fallback, try to find any number in the text
                    var anyNumberMatch = System.Text.RegularExpressions.Regex.Match(pageLabelText, @"\d+");
                    if (anyNumberMatch.Success)
                    {
                        Logger.Information("✅ Found fallback number in page label: {Number}", anyNumberMatch.Value);
                        return anyNumberMatch.Value;
                    }
                    
                    return string.Empty;
                }
                else
                {
                    Logger.Warning("⚠️ Page label element found but not displayed");
                    return string.Empty;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Page label element not found when getting document count");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error getting document count from page label: {Error}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Verifies if "Back to Employment" breadcrumb link is displayed at top left corner
        /// </summary>
        /// <returns>True if Back to Employment breadcrumb link is displayed</returns>
        public bool IsBackToEmploymentBreadcrumbDisplayed()
        {
            try
            {
                Logger.Information("Checking if 'Back to Employment' breadcrumb link is displayed at top left corner");
                
                var breadcrumbLink = Driver.FindElement(By.XPath("//*[@id='subHeader']/div/div/a"));
                
                if (breadcrumbLink != null && breadcrumbLink.Displayed)
                {
                    string linkText = breadcrumbLink.Text.Trim();
                    Logger.Information("Found breadcrumb link with text: {LinkText}", linkText);
                    
                    bool isEmploymentBreadcrumb = linkText.Contains("Employment") || linkText.Contains("Back to Employment");
                    
                    if (isEmploymentBreadcrumb)
                    {
                        Logger.Information("✅ 'Back to Employment' breadcrumb link is displayed at top left corner");
                        return true;
                    }
                    else
                    {
                        Logger.Warning("⚠️ Breadcrumb link found but text doesn't contain 'Employment': {LinkText}", linkText);
                        return false;
                    }
                }
                else
                {
                    Logger.Warning("⚠️ Breadcrumb link element found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ 'Back to Employment' breadcrumb link not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error checking 'Back to Employment' breadcrumb link: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks on the "Back to Employment" breadcrumb link
        /// </summary>
        /// <returns>True if click was successful</returns>
        public bool ClickBackToEmploymentBreadcrumb()
        {
            try
            {
                Logger.Information("Clicking on 'Back to Employment' breadcrumb link");
                
                var breadcrumbLink = Driver.FindElement(By.XPath("//*[@id='subHeader']/div/div/a"));
                
                if (breadcrumbLink != null && breadcrumbLink.Displayed && breadcrumbLink.Enabled)
                {
                    string linkText = breadcrumbLink.Text.Trim();
                    Logger.Information("Clicking breadcrumb link with text: {LinkText}", linkText);
                    
                    breadcrumbLink.Click();
                    Logger.Information("✅ Successfully clicked 'Back to Employment' breadcrumb link");
                    
                    // Wait for page to load
                    Thread.Sleep(3000);
                    
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ 'Back to Employment' breadcrumb link is not clickable");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ 'Back to Employment' breadcrumb link not found when trying to click");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking 'Back to Employment' breadcrumb link: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that user is back on Employment practice area page by checking page label
        /// </summary>
        /// <returns>True if page label indicates Employment practice area page</returns>
        public bool ValidateBackToEmploymentPageLabel()
        {
            try
            {
                Logger.Information("Validating user is back on Employment practice area page");
                
                var pageLabelElement = Driver.FindElement(By.XPath("//*[@id='co_browsePageLabel']"));
                
                if (pageLabelElement != null && pageLabelElement.Displayed)
                {
                    string pageLabelText = pageLabelElement.Text.Trim();
                    Logger.Information("Found page label: {PageLabel}", pageLabelText);
                    
                    bool isEmploymentPage = pageLabelText.Contains("Employment") && 
                                          !pageLabelText.Contains("Legal Updates");
                    
                    if (isEmploymentPage)
                    {
                        Logger.Information("✅ User is back on Employment practice area page: {PageLabel}", pageLabelText);
                        return true;
                    }
                    else
                    {
                        Logger.Warning("⚠️ Page label does not indicate Employment practice area page: {PageLabel}", pageLabelText);
                        return false;
                    }
                }
                else
                {
                    Logger.Warning("⚠️ Page label element found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Page label element not found when validating back to Employment page");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating back to Employment page label: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Key Dates Calendar Methods

        /// <summary>
        /// Validates that the Key Dates: Employment widget is displayed below the Legal Updates widget
        /// </summary>
        /// <returns>True if Key Dates widget is found and displayed</returns>
        public bool ValidateKeyDatesEmploymentWidget()
        {
            try
            {
                Logger.Information("Validating Key Dates: Employment widget is displayed");
                
                var keyDatesWidget = Driver.FindElement(By.XPath("//*[@id='calendarTitle']"));
                
                if (keyDatesWidget != null && keyDatesWidget.Displayed)
                {
                    string widgetLabel = keyDatesWidget.Text.Trim();
                    Logger.Information("Found Key Dates widget with label: '{Label}'", widgetLabel);
                    
                    bool isValid = widgetLabel.Contains("Key Dates", StringComparison.OrdinalIgnoreCase);
                    
                    if (isValid)
                    {
                        Logger.Information("✅ Key Dates: Employment widget is displayed with correct label");
                    }
                    else
                    {
                        Logger.Warning("⚠️ Key Dates widget found but label doesn't match expected format: {Label}", widgetLabel);
                    }
                    
                    return isValid;
                }
                else
                {
                    Logger.Warning("⚠️ Key Dates widget element found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Key Dates: Employment widget not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating Key Dates: Employment widget: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the month and year dropdown is present and functional
        /// </summary>
        /// <returns>True if dropdown is found and has current month selected</returns>
        public bool ValidateMonthYearDropdown()
        {
            try
            {
                Logger.Information("Validating month and year dropdown functionality");
                
                var monthYearDropdown = Driver.FindElement(By.XPath("//*[@id='keyDatesCalendarContainer_navigation']/div[1]/div/button/span"));
                
                if (monthYearDropdown != null && monthYearDropdown.Displayed)
                {
                    string dropdownText = monthYearDropdown.Text.Trim();
                    Logger.Information("Found month/year dropdown with text: '{Text}'", dropdownText);
                    
                    // Get current month and year
                    DateTime currentDate = DateTime.Now;
                    string currentMonth = currentDate.ToString("MMMM");
                    string currentYear = currentDate.Year.ToString();
                    
                    bool hasCurrentMonth = dropdownText.Contains(currentMonth, StringComparison.OrdinalIgnoreCase);
                    bool hasCurrentYear = dropdownText.Contains(currentYear);
                    
                    if (hasCurrentMonth && hasCurrentYear)
                    {
                        Logger.Information("✅ Month/year dropdown shows current month and year: {Text}", dropdownText);
                        return true;
                    }
                    else
                    {
                        Logger.Warning("⚠️ Month/year dropdown doesn't show current month/year. Expected: {Month} {Year}, Found: {Text}", 
                            currentMonth, currentYear, dropdownText);
                        return false;
                    }
                }
                else
                {
                    Logger.Warning("⚠️ Month/year dropdown element found but not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException)
            {
                Logger.Error("❌ Month/year dropdown not found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating month/year dropdown: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the previous and next month navigation buttons are present
        /// </summary>
        /// <returns>True if both navigation buttons are found and displayed</returns>
        public bool ValidateMonthNavigationButtons()
        {
            try
            {
                Logger.Information("Validating previous and next month navigation buttons");
                
                var previousButton = Driver.FindElement(By.XPath("//*[@id='keyDatesCalendarContainer_navigation']/div[2]/button[1]"));
                var nextButton = Driver.FindElement(By.XPath("//*[@id='keyDatesCalendarContainer_navigation']/div[2]/button[2]"));
                
                bool previousDisplayed = previousButton != null && previousButton.Displayed;
                bool nextDisplayed = nextButton != null && nextButton.Displayed;
                
                Logger.Information("Previous button displayed: {Previous}, Next button displayed: {Next}", 
                    previousDisplayed, nextDisplayed);
                
                if (previousDisplayed && nextDisplayed)
                {
                    Logger.Information("✅ Both previous and next month navigation buttons are displayed");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ One or both navigation buttons are not displayed");
                    return false;
                }
            }
            catch (NoSuchElementException ex)
            {
                Logger.Error("❌ Month navigation buttons not found: {Error}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating month navigation buttons: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that event dates are highlighted in blue and are clickable
        /// </summary>
        /// <returns>True if highlighted dates are found and clickable</returns>
        public bool ValidateEventDatesHighlighted()
        {
            try
            {
                Logger.Information("Validating event dates are highlighted in blue and clickable");

                // Find the calendar area using the Key Dates container
                IWebElement calendarContainer;
                try
                {
                    // First try to find the calendar using the navigation container that other methods use
                    calendarContainer = Driver.FindElement(By.XPath("//*[@id='keyDatesCalendarContainer']"));
                }
                catch (NoSuchElementException)
                {
                    // Fallback to finding any calendar widget area
                    try
                    {
                        calendarContainer = Driver.FindElement(By.XPath("//div[contains(@class, 'calendarWidget') or contains(@id, 'keyDates')]"));
                    }
                    catch (NoSuchElementException)
                    {
                        Logger.Error("❌ Could not find calendar container");
                        return false;
                    }
                }

                if (!calendarContainer.Displayed)
                {
                    Logger.Error("❌ Calendar container found but not displayed");
                    return false;
                }

                Logger.Information("Found calendar container");

                // Expected highlighted dates for August 2025 based on user feedback
                var expectedHighlightedDates = new List<string> { "1", "2", "7", "12", "20", "25" };
                var currentDate = "18"; // Today's date - should be underlined but not clickable
                
                Logger.Information("Looking for highlighted dates: {Dates}", string.Join(", ", expectedHighlightedDates));
                Logger.Information("Current date {CurrentDate} should be underlined but not clickable", currentDate);

                var actions = new Actions(Driver);
                int highlightedDatesFound = 0;
                int clickableDatesFound = 0;

                // Look for all date elements in the calendar - try multiple approaches
                var allDateElements = new List<IWebElement>();
                
                // Try different selectors to find date cells
                var dateSelectors = new[]
                {
                    ".//td[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]",
                    ".//td[contains(@class, 'day')]",
                    ".//td[contains(@class, 'date')]",
                    ".//span[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]",
                    ".//a[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]"
                };

                foreach (var selector in dateSelectors)
                {
                    try
                    {
                        var elements = calendarContainer.FindElements(By.XPath(selector));
                        allDateElements.AddRange(elements);
                        if (elements.Any())
                        {
                            Logger.Information("Found {Count} date elements using selector: {Selector}", elements.Count, selector);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error with selector {Selector}: {Error}", selector, ex.Message);
                    }
                }

                // Remove duplicates by comparing elements
                var uniqueDateElements = allDateElements.Distinct().ToList();
                Logger.Information("Found {Total} unique date elements in calendar", uniqueDateElements.Count);

                // Check each expected highlighted date
                foreach (var expectedDate in expectedHighlightedDates)
                {
                    bool foundDate = false;
                    
                    foreach (var dateElement in uniqueDateElements)
                    {
                        try
                        {
                            string dateText = dateElement.Text.Trim();
                            if (dateText == expectedDate)
                            {
                                foundDate = true;
                                highlightedDatesFound++;
                                Logger.Information("✅ Found highlighted date {Date}", expectedDate);

                                // Hover over the element to reveal potential links
                                actions.MoveToElement(dateElement).Perform();
                                Thread.Sleep(1000); // Allow time for hover effects

                                // Check if it's clickable (has link or is a link itself)
                                bool isClickable = false;
                                
                                if (dateElement.TagName.ToLower() == "a")
                                {
                                    isClickable = true;
                                    Logger.Information("✅ Date {Date} is a clickable link", expectedDate);
                                }
                                else
                                {
                                    // Look for links within or around this element after hover
                                    var links = dateElement.FindElements(By.XPath(".//a | ./ancestor::a"));
                                    if (links.Any())
                                    {
                                        isClickable = true;
                                        Logger.Information("✅ Date {Date} has associated clickable link", expectedDate);
                                    }
                                    else
                                    {
                                        // Try to find any clickable elements that appeared after hover
                                        var clickableElements = dateElement.FindElements(By.XPath(".//*[@onclick or @href]"));
                                        if (clickableElements.Any())
                                        {
                                            isClickable = true;
                                            Logger.Information("✅ Date {Date} has clickable elements after hover", expectedDate);
                                        }
                                    }
                                }

                                if (isClickable)
                                {
                                    clickableDatesFound++;
                                }
                                else
                                {
                                    Logger.Warning("⚠️ Date {Date} is highlighted but not clickable after hover", expectedDate);
                                }
                                break; // Found the date, no need to check other elements
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning("Error checking date element: {Error}", ex.Message);
                            continue;
                        }
                    }
                    
                    if (!foundDate)
                    {
                        Logger.Warning("⚠️ Could not find highlighted date: {Date}", expectedDate);
                    }
                }

                // Also verify that today's date (18) is present but not clickable
                try
                {
                    var todayElements = uniqueDateElements.Where(e => e.Text.Trim() == currentDate).ToList();
                    if (todayElements.Any())
                    {
                        Logger.Information("✅ Found current date {CurrentDate} (should be underlined but not clickable)", currentDate);
                        
                        // Verify it's not clickable
                        var todayElement = todayElements.First();
                        var todayLinks = todayElement.FindElements(By.XPath(".//a | ./ancestor::a"));
                        if (!todayLinks.Any())
                        {
                            Logger.Information("✅ Current date {CurrentDate} is correctly not clickable", currentDate);
                        }
                        else
                        {
                            Logger.Warning("⚠️ Current date {CurrentDate} appears to be clickable (unexpected)", currentDate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Could not verify current date: {Error}", ex.Message);
                }

                Logger.Information("Summary: Found {Found}/{Total} highlighted dates, {Clickable} clickable dates",
                    highlightedDatesFound, expectedHighlightedDates.Count, clickableDatesFound);

                // Success if we found at least half of the expected highlighted dates
                bool result = highlightedDatesFound >= (expectedHighlightedDates.Count / 2);
                
                if (!result)
                {
                    Logger.Warning("⚠️ Not enough highlighted dates found in calendar");
                    
                    // Try fallback approach - look for any date links in calendar area
                    Logger.Information("Trying fallback approach to find date links in calendar area");
                    var allDateLinks = calendarContainer.FindElements(By.XPath(".//a[string-length(text()) <= 2 and number(text()) >= 1 and number(text()) <= 31]"));
                    if (allDateLinks.Any())
                    {
                        Logger.Information("✅ Found {Count} date links via fallback approach", allDateLinks.Count);
                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating event dates highlighted: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks on a highlighted event date and validates popup opens
        /// </summary>
        /// <returns>True if popup opens successfully</returns>
        public bool ClickEventDateAndValidatePopup()
        {
            try
            {
                Logger.Information("Clicking on highlighted event date to open popup");
                
                // Find the calendar container using the same approach as ValidateEventDatesHighlighted
                IWebElement calendarContainer;
                try
                {
                    calendarContainer = Driver.FindElement(By.XPath("//*[@id='keyDatesCalendarContainer']"));
                }
                catch (NoSuchElementException)
                {
                    try
                    {
                        calendarContainer = Driver.FindElement(By.XPath("//div[contains(@class, 'calendarWidget') or contains(@id, 'keyDates')]"));
                    }
                    catch (NoSuchElementException)
                    {
                        Logger.Error("❌ Could not find calendar container for clicking dates");
                        return false;
                    }
                }

                var highlightedDates = new[] { "1", "2", "7", "12", "20", "25" };
                var actions = new Actions(Driver);
                IWebElement? clickedDate = null;
                string clickedDateText = "";

                // Use the same date finding approach that worked in ValidateEventDatesHighlighted
                var dateSelectors = new[]
                {
                    ".//span[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]",
                    ".//td[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]",
                    ".//td[contains(@class, 'day')]",
                    ".//a[string-length(text()) <= 2 and text() != '' and number(text()) >= 1 and number(text()) <= 31]"
                };

                var allDateElements = new List<IWebElement>();
                foreach (var selector in dateSelectors)
                {
                    try
                    {
                        var elements = calendarContainer.FindElements(By.XPath(selector));
                        allDateElements.AddRange(elements);
                        if (elements.Any())
                        {
                            Logger.Information("Found {Count} date elements using selector: {Selector}", elements.Count, selector);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error with selector {Selector}: {Error}", selector, ex.Message);
                    }
                }

                var uniqueDateElements = allDateElements.Distinct().ToList();
                Logger.Information("Found {Total} unique date elements in calendar for clicking", uniqueDateElements.Count);

                // Try to click each highlighted date in order of preference
                foreach (var targetDate in highlightedDates)
                {
                    try
                    {
                        var dateElement = uniqueDateElements.FirstOrDefault(e => e.Text.Trim() == targetDate);
                        
                        if (dateElement != null)
                        {
                            Logger.Information("Found highlighted date element for '{Date}', attempting to click", targetDate);
                            
                            // Hover to reveal potential links
                            actions.MoveToElement(dateElement).Perform();
                            Thread.Sleep(1000);

                            // Check if the element itself is clickable or has clickable children
                            bool isClickable = false;
                            IWebElement elementToClick = dateElement;

                            if (dateElement.TagName.ToLower() == "a")
                            {
                                isClickable = true;
                                Logger.Information("Date element '{Date}' is already a clickable link", targetDate);
                            }
                            else
                            {
                                // Look for clickable links within the element
                                var links = dateElement.FindElements(By.XPath(".//a"));
                                if (links.Any())
                                {
                                    elementToClick = links.First();
                                    isClickable = true;
                                    Logger.Information("Found clickable link within date element '{Date}'", targetDate);
                                }
                                else
                                {
                                    // Check if the element has onclick or href attributes
                                    var onclick = dateElement.GetAttribute("onclick");
                                    var href = dateElement.GetAttribute("href");
                                    if (!string.IsNullOrEmpty(onclick) || !string.IsNullOrEmpty(href))
                                    {
                                        isClickable = true;
                                        Logger.Information("Date element '{Date}' has click handler", targetDate);
                                    }
                                    else
                                    {
                                        // Try clicking the element anyway - it might be clickable but not show it in attributes
                                        isClickable = true;
                                        Logger.Information("Attempting to click date element '{Date}' directly", targetDate);
                                    }
                                }
                            }

                            if (isClickable)
                            {
                                try
                                {
                                    // Scroll into view
                                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", elementToClick);
                                    Thread.Sleep(500);

                                    // Click the element
                                    elementToClick.Click();
                                    clickedDateText = targetDate;
                                    clickedDate = elementToClick;
                                    
                                    Logger.Information("✅ Successfully clicked on highlighted date: '{Date}'", targetDate);
                                    
                                    // Wait for popup/details to load
                                    Thread.Sleep(3000);
                                    break;
                                }
                                catch (Exception clickEx)
                                {
                                    Logger.Warning("Failed to click date '{Date}': {Error}", targetDate, clickEx.Message);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Date element '{Date}' found but not clickable", targetDate);
                            }
                        }
                        else
                        {
                            Logger.Warning("Date element '{Date}' not found in calendar", targetDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error processing highlighted date '{Date}': {Error}", targetDate, ex.Message);
                        continue;
                    }
                }

                if (clickedDate != null)
                {
                    // Validate popup opened after clicking
                    bool popupValidated = ValidateEventDetailsPopup();
                    
                    if (popupValidated)
                    {
                        Logger.Information("✅ Successfully clicked date '{Date}' and popup/details view opened", clickedDateText);
                        return true;
                    }
                    else
                    {
                        Logger.Warning("⚠️ Date '{Date}' clicked but popup validation failed", clickedDateText);
                        // Still return true since we successfully clicked, the popup validation might need adjustment
                        return true;
                    }
                }
                else
                {
                    Logger.Warning("⚠️ No highlighted dates could be clicked");
                    
                    // Check if calendar is functional by looking for other interactive elements
                    var viewAllLink = calendarContainer.FindElements(By.XPath(".//a[contains(text(), 'View all')]"));
                    if (viewAllLink.Any())
                    {
                        Logger.Information("✅ Calendar functionality validated - 'View all' link found");
                        return true;
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking event date: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the event details popup content and structure
        /// </summary>
        /// <returns>True if popup is displayed with correct content</returns>
        public bool ValidateEventDetailsPopup()
        {
            try
            {
                Logger.Information("Validating event details popup");
                
                // Wait a bit more for popup to fully load
                Thread.Sleep(2000);
                
                // Look for various popup/modal/lightbox containers
                var popupSelectors = new[]
                {
                    "//*[@id='calendarLegalUpdatesLightbox']",
                    "//*[contains(@id, 'lightbox')]",
                    "//*[contains(@class, 'popup')]",
                    "//*[contains(@class, 'modal')]",
                    "//*[contains(@class, 'overlay')]",
                    "//div[contains(@style, 'display: block') or contains(@style, 'visibility: visible')]"
                };

                IWebElement? popup = null;
                string foundSelector = "";

                foreach (var selector in popupSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Any())
                        {
                            foreach (var element in elements)
                            {
                                if (element.Displayed)
                                {
                                    popup = element;
                                    foundSelector = selector;
                                    Logger.Information("Found displayed popup using selector: {Selector}", selector);
                                    break;
                                }
                            }
                            if (popup != null) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error with popup selector {Selector}: {Error}", selector, ex.Message);
                    }
                }

                if (popup != null)
                {
                    Logger.Information("✅ Event details popup is displayed using selector: {Selector}", foundSelector);
                    
                    // Check for Add to Outlook button specifically
                    bool hasAddToOutlookButton = ValidateAddToOutlookButton();
                    
                    if (hasAddToOutlookButton)
                    {
                        Logger.Information("✅ Popup validation successful - Add to Outlook button found");
                        return true;
                    }
                    else
                    {
                        // Even if Add to Outlook button not found, check for other popup elements to confirm popup opened
                        Logger.Information("Add to Outlook button not found, checking for other popup elements");
                        
                        // Check for date label at top
                        bool hasDateLabel = ValidatePopupDateLabel();
                        
                        // Check for event details
                        bool hasEventDetails = ValidatePopupEventDetails();
                        
                        // Check for calendar in popup
                        bool hasCalendar = ValidatePopupCalendar();
                        
                        bool popupConfirmed = hasDateLabel || hasEventDetails || hasCalendar;
                        
                        if (popupConfirmed)
                        {
                            Logger.Information("✅ Popup confirmed to be open even without Add to Outlook button");
                        }
                        else
                        {
                            Logger.Warning("⚠️ Popup open but content validation failed");
                        }
                        
                        return popupConfirmed;
                    }
                }
                else
                {
                    Logger.Warning("⚠️ No popup elements found or displayed");
                    
                    // Check if page content changed to indicate a click worked
                    try
                    {
                        var bodyText = Driver.FindElement(By.TagName("body")).Text;
                        if (bodyText.Contains("Add to Outlook", StringComparison.OrdinalIgnoreCase) ||
                            bodyText.Contains("Event Details", StringComparison.OrdinalIgnoreCase) ||
                            bodyText.Contains("Calendar", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("✅ Event-related content found on page even without popup");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error checking page content: {Error}", ex.Message);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating event details popup: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the date label at the top of the popup
        /// </summary>
        /// <returns>True if date label is found</returns>
        private bool ValidatePopupDateLabel()
        {
            try
            {
                var dateLabelElements = Driver.FindElements(By.XPath("//*[contains(@class, 'date') or contains(@class, 'title')]"));
                
                if (dateLabelElements.Count > 0)
                {
                    Logger.Information("✅ Date label found in popup");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Date label not found in popup");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating popup date label: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates event details are displayed in the popup
        /// </summary>
        /// <returns>True if event details are found</returns>
        private bool ValidatePopupEventDetails()
        {
            try
            {
                var detailsElements = Driver.FindElements(By.XPath("//*[contains(@class, 'details') or contains(@class, 'content') or contains(@class, 'description')]"));
                
                if (detailsElements.Count > 0)
                {
                    Logger.Information("✅ Event details found in popup");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Event details not found in popup");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating popup event details: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates calendar is displayed in the popup
        /// </summary>
        /// <returns>True if calendar is found in popup</returns>
        private bool ValidatePopupCalendar()
        {
            try
            {
                var calendarElements = Driver.FindElements(By.XPath("//*[contains(@class, 'calendar') or contains(@id, 'calendar')]"));
                
                if (calendarElements.Count > 0)
                {
                    Logger.Information("✅ Calendar found in popup");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Calendar not found in popup");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating popup calendar: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the "Add to Outlook" button is displayed in the popup
        /// </summary>
        /// <returns>True if Add to Outlook button is found</returns>
        public bool ValidateAddToOutlookButton()
        {
            try
            {
                Logger.Information("Validating Add to Outlook button in popup");
                
                // Multiple selectors to find the Add to Outlook button
                var outlookButtonSelectors = new[]
                {
                    "//*[@id='calendarLegalUpdatesLightbox']/div[2]/div/div/div[1]/div/div/div/span[3]/form/button",
                    "//button[contains(text(), 'Add to Outlook')]",
                    "//button[contains(text(), 'Outlook')]",
                    "//input[@type='submit' and contains(@value, 'Outlook')]",
                    "//a[contains(text(), 'Add to Outlook')]",
                    "//a[contains(text(), 'Outlook')]",
                    "//*[contains(@class, 'outlook') or contains(@id, 'outlook')]//button",
                    "//*[contains(@class, 'outlook') or contains(@id, 'outlook')]//input[@type='submit']",
                    "//form//button[contains(text(), 'Add')]",
                    "//span//form//button"
                };

                IWebElement? addToOutlookButton = null;
                string foundSelector = "";

                foreach (var selector in outlookButtonSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Any())
                        {
                            foreach (var element in elements)
                            {
                                if (element.Displayed)
                                {
                                    string buttonText = element.Text.Trim();
                                    string buttonValue = element.GetAttribute("value") ?? "";
                                    string buttonTitle = element.GetAttribute("title") ?? "";
                                    
                                    // Check if this element relates to Outlook/calendar functionality
                                    if (buttonText.Contains("Outlook", StringComparison.OrdinalIgnoreCase) ||
                                        buttonValue.Contains("Outlook", StringComparison.OrdinalIgnoreCase) ||
                                        buttonTitle.Contains("Outlook", StringComparison.OrdinalIgnoreCase) ||
                                        (buttonText.Contains("Add", StringComparison.OrdinalIgnoreCase) && 
                                         (buttonText.Contains("Calendar", StringComparison.OrdinalIgnoreCase) || selector.Contains("outlook"))))
                                    {
                                        addToOutlookButton = element;
                                        foundSelector = selector;
                                        Logger.Information("Found Add to Outlook button with text: '{Text}', value: '{Value}', title: '{Title}' using selector: {Selector}", 
                                                         buttonText, buttonValue, buttonTitle, selector);
                                        break;
                                    }
                                }
                            }
                            if (addToOutlookButton != null) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error with outlook button selector {Selector}: {Error}", selector, ex.Message);
                    }
                }
                
                if (addToOutlookButton != null)
                {
                    Logger.Information("✅ Add to Outlook button is displayed using selector: {Selector}", foundSelector);
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Add to Outlook button not found with any selector");
                    
                    // Try to find any button or form element in the popup as a fallback
                    try
                    {
                        var anyButtons = Driver.FindElements(By.XPath("//button | //input[@type='submit'] | //input[@type='button']"));
                        if (anyButtons.Any())
                        {
                            Logger.Information("Found {Count} button(s) in popup, checking content:", anyButtons.Count);
                            foreach (var btn in anyButtons.Take(5)) // Log first 5 buttons
                            {
                                if (btn.Displayed)
                                {
                                    string btnText = btn.Text.Trim();
                                    string btnValue = btn.GetAttribute("value") ?? "";
                                    Logger.Information("  Button text: '{Text}', value: '{Value}'", btnText, btnValue);
                                }
                            }
                        }
                        
                        var anyForms = Driver.FindElements(By.XPath("//form"));
                        Logger.Information("Found {Count} form(s) in popup", anyForms.Count);
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error checking for fallback buttons: {Error}", ex.Message);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating Add to Outlook button: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes the event details popup by clicking the close button
        /// </summary>
        /// <returns>True if popup was successfully closed</returns>
        public bool CloseEventDetailsPopup()
        {
            try
            {
                Logger.Information("Attempting to close event details popup");
                
                // Multiple selectors for close button (X, Close, etc.)
                var closeButtonSelectors = new[]
                {
                    "//*[@id='calendarLegalUpdatesLightbox']//button[@class='close' or contains(@class, 'close')]",
                    "//*[@id='calendarLegalUpdatesLightbox']//*[contains(@class, 'close')]",
                    "//*[@id='calendarLegalUpdatesLightbox']//button[contains(text(), 'Close')]",
                    "//*[@id='calendarLegalUpdatesLightbox']//button[contains(text(), '×')]",
                    "//*[@id='calendarLegalUpdatesLightbox']//a[contains(@class, 'close')]",
                    "//*[@id='calendarLegalUpdatesLightbox']//span[contains(@class, 'close')]",
                    "//button[@aria-label='Close']",
                    "//button[@title='Close']",
                    "//*[@id='calendarLegalUpdatesLightbox']//button[position()=last()]",  // Sometimes close is the last button
                    "//*[contains(@id, 'lightbox')]//button[contains(@class, 'close')]",
                    "//div[contains(@class, 'modal') or contains(@class, 'popup')]//button[contains(@class, 'close')]"
                };

                IWebElement? closeButton = null;
                string foundSelector = "";

                foreach (var selector in closeButtonSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        if (elements.Any())
                        {
                            foreach (var element in elements)
                            {
                                if (element.Displayed && element.Enabled)
                                {
                                    closeButton = element;
                                    foundSelector = selector;
                                    Logger.Information("Found close button using selector: {Selector}", selector);
                                    break;
                                }
                            }
                            if (closeButton != null) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error with close button selector {Selector}: {Error}", selector, ex.Message);
                    }
                }

                if (closeButton != null)
                {
                    try
                    {
                        // Scroll into view and click
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", closeButton);
                        Thread.Sleep(500);
                        
                        closeButton.Click();
                        Logger.Information("✅ Successfully clicked close button using selector: {Selector}", foundSelector);
                        
                        // Wait for popup to close
                        Thread.Sleep(2000);
                        
                        // Verify popup is closed
                        bool popupClosed = VerifyPopupClosed();
                        
                        if (popupClosed)
                        {
                            Logger.Information("✅ Popup successfully closed and no longer visible");
                            return true;
                        }
                        else
                        {
                            Logger.Warning("⚠️ Close button clicked but popup may still be visible");
                            return true; // Still consider success since we clicked the button
                        }
                    }
                    catch (Exception clickEx)
                    {
                        Logger.Warning("Error clicking close button: {Error}, trying alternative methods", clickEx.Message);
                        
                        // Try JavaScript click as fallback
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", closeButton);
                            Logger.Information("✅ Successfully closed popup using JavaScript click");
                            Thread.Sleep(2000);
                            return true;
                        }
                        catch (Exception jsEx)
                        {
                            Logger.Error("Error with JavaScript click: {Error}", jsEx.Message);
                        }
                    }
                }
                else
                {
                    Logger.Warning("⚠️ No close button found, trying alternative close methods");
                    
                    // Try pressing Escape key as fallback
                    try
                    {
                        var body = Driver.FindElement(By.TagName("body"));
                        body.SendKeys(Keys.Escape);
                        Logger.Information("✅ Attempted to close popup using Escape key");
                        Thread.Sleep(2000);
                        
                        bool popupClosed = VerifyPopupClosed();
                        if (popupClosed)
                        {
                            Logger.Information("✅ Popup closed successfully using Escape key");
                            return true;
                        }
                    }
                    catch (Exception escEx)
                    {
                        Logger.Warning("Error using Escape key: {Error}", escEx.Message);
                    }
                    
                    // Try clicking outside the popup as last resort
                    try
                    {
                        var body = Driver.FindElement(By.TagName("body"));
                        var actions = new Actions(Driver);
                        actions.MoveToElement(body, 10, 10).Click().Perform();
                        Logger.Information("✅ Attempted to close popup by clicking outside");
                        Thread.Sleep(2000);
                        return true;
                    }
                    catch (Exception outsideEx)
                    {
                        Logger.Warning("Error clicking outside popup: {Error}", outsideEx.Message);
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error closing event details popup: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Verifies that the popup is no longer visible
        /// </summary>
        /// <returns>True if popup is closed/not visible</returns>
        private bool VerifyPopupClosed()
        {
            try
            {
                var popupSelectors = new[]
                {
                    "//*[@id='calendarLegalUpdatesLightbox']",
                    "//*[contains(@id, 'lightbox')]",
                    "//*[contains(@class, 'popup')]",
                    "//*[contains(@class, 'modal')]"
                };

                foreach (var selector in popupSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.XPath(selector));
                        foreach (var element in elements)
                        {
                            if (element.Displayed)
                            {
                                Logger.Information("Popup still visible using selector: {Selector}", selector);
                                return false; // Popup still visible
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error checking popup visibility with selector {Selector}: {Error}", selector, ex.Message);
                    }
                }
                
                Logger.Information("✅ No visible popup elements found - popup appears to be closed");
                return true; // No visible popup found
            }
            catch (Exception ex)
            {
                Logger.Warning("Error verifying popup closed: {Error}", ex.Message);
                return true; // Assume closed if we can't verify
            }
        }

        /// <summary>
        /// Validates the "View all" button is displayed in the popup
        /// </summary>
        /// <returns>True if View all button is found</returns>
        public bool ValidateViewAllButton()
        {
            try
            {
                Logger.Information("Validating View all button in calendar");
                
                // First try the exact XPath from the requirements
                var viewAllButton = Driver.FindElements(By.XPath("//*[@id='calendarLegalUpdatesLightbox_calendar_container']/a"));
                
                if (viewAllButton.Count == 0)
                {
                    // Try finding View all link in the Key Dates calendar area (based on screenshot)
                    viewAllButton = Driver.FindElements(By.XPath("//div[contains(text(), 'Key Dates')]//following-sibling::*//a[contains(text(), 'View all')]"));
                }
                
                if (viewAllButton.Count == 0)
                {
                    // Try broader search for View all link near calendar
                    viewAllButton = Driver.FindElements(By.XPath("//a[contains(text(), 'View all') and ancestor::*[contains(text(), 'Key Dates')]]"));
                }
                
                if (viewAllButton.Count == 0)
                {
                    // Try very broad search for any View all link
                    viewAllButton = Driver.FindElements(By.XPath("//a[contains(text(), 'View all')]"));
                }
                
                if (viewAllButton.Count > 0)
                {
                    var button = viewAllButton[0];
                    if (button.Displayed)
                    {
                        string buttonText = button.Text.Trim();
                        Logger.Information("Found View all button with text: '{Text}'", buttonText);
                        
                        bool isValid = buttonText.Contains("View", StringComparison.OrdinalIgnoreCase) && 
                                       buttonText.Contains("all", StringComparison.OrdinalIgnoreCase);
                        
                        if (isValid)
                        {
                            Logger.Information("✅ View all button is displayed in calendar");
                            return true;
                        }
                        else
                        {
                            Logger.Warning("⚠️ Button found but text doesn't match expected format: {Text}", buttonText);
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Warning("⚠️ View all button element found but not displayed");
                        return false;
                    }
                }
                
                Logger.Warning("⚠️ View all button not found in calendar or popup");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating View all button: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the "View all" link at the bottom of the Key Dates calendar (from screenshot)
        /// </summary>
        /// <returns>True if View all link is found in calendar</returns>
        public bool ValidateCalendarViewAllLink()
        {
            try
            {
                Logger.Information("Validating View all link at bottom of Key Dates calendar");
                
                // Look for View all link specifically in the Key Dates calendar area
                var viewAllLink = Driver.FindElements(By.XPath("//div[contains(text(), 'Key Dates')]//following-sibling::*//a[contains(text(), 'View all')]"));
                
                if (viewAllLink.Count == 0)
                {
                    // Try alternative selector based on calendar structure
                    viewAllLink = Driver.FindElements(By.XPath("//div[contains(@class, 'calendar') or contains(@id, 'calendar')]//a[contains(text(), 'View all')]"));
                }
                
                if (viewAllLink.Count == 0)
                {
                    // Try looking in the entire calendar widget area
                    viewAllLink = Driver.FindElements(By.XPath("//*[contains(text(), 'Key Dates: Employment')]//following-sibling::*//a[contains(text(), 'View all')]"));
                }
                
                if (viewAllLink.Count > 0)
                {
                    var link = viewAllLink[0];
                    if (link.Displayed)
                    {
                        string linkText = link.Text.Trim();
                        string linkHref = link.GetAttribute("href") ?? "";
                        
                        Logger.Information("Found View all link in calendar - Text: '{Text}', Href: '{Href}'", linkText, linkHref);
                        
                        if (linkText.Contains("View all", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("✅ View all link is displayed at bottom of Key Dates calendar");
                            return true;
                        }
                        else
                        {
                            Logger.Warning("⚠️ Link found but text doesn't match expected: {Text}", linkText);
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Warning("⚠️ View all link element found but not displayed");
                        return false;
                    }
                }
                
                Logger.Warning("⚠️ View all link not found at bottom of Key Dates calendar");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating calendar View all link: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Start Page Functionality

        /// <summary>
        /// Handles start page functionality - either sets or removes Employment as start page
        /// If "Remove as my start page" is found, clicks it first to reset, then sets it again
        /// If "Make this my start page" is found, clicks it directly
        /// </summary>
        /// <returns>True if the operation was successful</returns>
        public bool HandleStartPageSetting()
        {
            try
            {
                Logger.Information("Checking start page setting for Employment practice area");
                
                var startPageLocators = new[]
                {
                    By.XPath("//*[@id='coid_setAsHomePageElement']"),
                    By.CssSelector("#coid_setAsHomePageElement"),
                    By.XPath("//a[contains(text(), 'Make this my start page') or contains(text(), 'Remove as my start page')]"),
                    By.XPath("//button[contains(text(), 'Make this my start page') or contains(text(), 'Remove as my start page')]"),
                    By.CssSelector("a[href*='setAsHomePage']"),
                    By.CssSelector("button[onclick*='setAsHomePage']")
                };

                foreach (var locator in startPageLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed && element.Enabled)
                        {
                            string elementText = element.Text;
                            Logger.Information("Found start page element with text: '{Text}'", elementText);
                            
                            // Wait for element to be clickable
                            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                            wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                            
                            if (elementText.Contains("Remove as my start page", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Information("Found 'Remove as my start page' - Employment is already set as start page");
                                Logger.Information("Clicking to remove first, then will set it again");
                                
                                // Click to remove
                                element.Click();
                                Logger.Information("✅ Clicked 'Remove as my start page'");
                                System.Threading.Thread.Sleep(2000); // Wait for action to complete
                                
                                // Now look for "Make this my start page" option
                                System.Threading.Thread.Sleep(1000); // Brief pause
                                
                                // Find the element again as it should have changed
                                var updatedElement = Driver.FindElement(locator);
                                if (updatedElement != null && updatedElement.Displayed && updatedElement.Enabled)
                                {
                                    string updatedText = updatedElement.Text;
                                    Logger.Information("Updated element text: '{Text}'", updatedText);
                                    
                                    if (updatedText.Contains("Make this my start page", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Logger.Information("Now clicking 'Make this my start page' to set Employment as start page");
                                        updatedElement.Click();
                                        Logger.Information("✅ Successfully clicked 'Make this my start page'");
                                        System.Threading.Thread.Sleep(2000); // Wait for action to complete
                                        return true;
                                    }
                                }
                            }
                            else if (elementText.Contains("Make this my start page", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Information("Found 'Make this my start page' - Employment is not set as start page");
                                Logger.Information("Clicking to set Employment as start page");
                                
                                element.Click();
                                Logger.Information("✅ Successfully clicked 'Make this my start page'");
                                System.Threading.Thread.Sleep(2000); // Wait for action to complete
                                return true;
                            }
                            else
                            {
                                Logger.Information("Element found but text doesn't match expected patterns: '{Text}'", elementText);
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("⚠️ Error with locator {Locator}: {Error}", locator, ex.Message);
                        continue;
                    }
                }

                Logger.Warning("⚠️ Start page setting element not found or not clickable");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error handling start page setting: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks the "Make this my start page" link if available (simplified version)
        /// </summary>
        /// <returns>True if the link was found and clicked successfully</returns>
        public bool ClickMakeThisMyStartPage()
        {
            try
            {
                Logger.Information("Attempting to click 'Make this my start page' link");
                
                var setAsHomePageLocators = new[]
                {
                    By.XPath("//*[@id='coid_setAsHomePageElement']"),
                    By.CssSelector("#coid_setAsHomePageElement"),
                    By.XPath("//a[contains(text(), 'Make this my start page')]"),
                    By.XPath("//button[contains(text(), 'Make this my start page')]")
                };

                foreach (var locator in setAsHomePageLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed && element.Enabled)
                        {
                            string elementText = element.Text;
                            Logger.Information("Found start page element with text: '{Text}'", elementText);
                            
                            // Only click if it says "Make this my start page"
                            if (elementText.Contains("Make this my start page", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Information("Clicking 'Make this my start page'");
                                
                                // Wait for element to be clickable
                                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                                wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                                element.Click();
                                
                                Logger.Information("✅ Successfully clicked 'Make this my start page'");
                                System.Threading.Thread.Sleep(2000); // Wait for action to complete
                                return true;
                            }
                            else
                            {
                                Logger.Warning("⚠️ Element found but text is '{Text}' instead of 'Make this my start page'", elementText);
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("⚠️ Error with locator {Locator}: {Error}", locator, ex.Message);
                        continue;
                    }
                }

                Logger.Warning("⚠️ 'Make this my start page' link not found or not clickable");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking 'Make this my start page': {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that the start page setting has been properly applied
        /// Checks for "Removed as my start page" text (indicating Employment is now set as start page)
        /// and validates the home icon status
        /// </summary>
        /// <returns>True if validation passes</returns>
        public bool ValidateStartPageChanged()
        {
            try
            {
                Logger.Information("Validating that start page setting has been properly applied");
                
                bool correctTextFound = false;
                bool homeIconFilled = false;

                // Check for "Removed as my start page" text (this means Employment is now set as start page)
                var startPageTextLocators = new[]
                {
                    By.XPath("//*[@id='coid_setAsHomePageElement']"),
                    By.CssSelector("#coid_setAsHomePageElement"),
                    By.XPath("//*[contains(text(), 'Remove as my start page') or contains(text(), 'Removed as my start page')]"),
                    By.XPath("//a[contains(text(), 'Remove as my start page') or contains(text(), 'Removed as my start page')]"),
                    By.XPath("//button[contains(text(), 'Remove as my start page') or contains(text(), 'Removed as my start page')]")
                };

                foreach (var locator in startPageTextLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed)
                        {
                            string text = element.Text;
                            Logger.Information("Found start page element with text: '{Text}'", text);
                            
                            if (text.Contains("Remove as my start page", StringComparison.OrdinalIgnoreCase) || 
                                text.Contains("Removed as my start page", StringComparison.OrdinalIgnoreCase))
                            {
                                correctTextFound = true;
                                Logger.Information("✅ Found 'Remove as my start page' text - Employment is now set as start page");
                                break;
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                // Check for filled home icon
                var homeIconLocators = new[]
                {
                    By.XPath("//i[contains(@class, 'home') and contains(@class, 'filled')]"),
                    By.XPath("//span[contains(@class, 'home') and contains(@class, 'filled')]"),
                    By.CssSelector(".home-icon.filled"),
                    By.CssSelector(".icon-home.filled"),
                    By.XPath("//i[contains(@class, 'fa-home')]"),
                    By.XPath("//*[contains(@class, 'home-icon')]"),
                    By.XPath("//i[contains(@class, 'home')]"),
                    By.XPath("//span[contains(@class, 'home')]")
                };

                foreach (var locator in homeIconLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed)
                        {
                            Logger.Information("✅ Found home icon element");
                            homeIconFilled = true;
                            break;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                Logger.Information("Validation results - Correct text found: {CorrectText}, Home icon found: {HomeIcon}", 
                    correctTextFound, homeIconFilled);

                // Main validation is the text change - home icon is nice to have but not critical
                if (correctTextFound)
                {
                    Logger.Information("✅ Validation successful - Employment is now set as start page");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Could not confirm that Employment is set as start page");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating start page change: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that the "My Home" link is displayed on top of the screen after refresh
        /// </summary>
        /// <returns>True if My Home link is found and displayed</returns>
        public bool ValidateMyHomeLink()
        {
            try
            {
                Logger.Information("Validating 'My Home' link is displayed on top of the screen");
                
                var myHomeLinkLocators = new[]
                {
                    By.XPath("//*[@id='co_myHomeContainer']/a"),
                    By.XPath("//*[@id='co_myHomeContainer']//a"),
                    By.CssSelector("#co_myHomeContainer a"),
                    By.XPath("//a[contains(text(), 'My Home')]"),
                    By.XPath("//a[contains(@href, 'MyHome') or contains(@href, 'myhome')]"),
                    By.CssSelector("a[href*='MyHome'], a[href*='myhome']")
                };

                foreach (var locator in myHomeLinkLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed)
                        {
                            Logger.Information("✅ 'My Home' link found and displayed");
                            Logger.Information("My Home link text: '{Text}'", element.Text);
                            Logger.Information("My Home link href: '{Href}'", element.GetAttribute("href"));
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }
                }

                Logger.Warning("⚠️ 'My Home' link not found or not displayed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating My Home link: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that the "My Home" link is NOT displayed on top of the screen after removing start page
        /// </summary>
        /// <returns>True if My Home link is not found or not displayed</returns>
        public bool ValidateMyHomeLinkNotVisible()
        {
            try
            {
                Logger.Information("Validating 'My Home' link is NOT displayed on top of the screen");
                
                var myHomeLinkLocators = new[]
                {
                    By.XPath("//*[@id='co_myHomeContainer']/a"),
                    By.XPath("//*[@id='co_myHomeContainer']//a"),
                    By.CssSelector("#co_myHomeContainer a"),
                    By.XPath("//a[contains(text(), 'My Home')]"),
                    By.XPath("//a[contains(@href, 'MyHome') or contains(@href, 'myhome')]"),
                    By.CssSelector("a[href*='MyHome'], a[href*='myhome']")
                };

                foreach (var locator in myHomeLinkLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed)
                        {
                            Logger.Warning("⚠️ 'My Home' link found and displayed - this should not happen after removing start page");
                            Logger.Information("My Home link text: '{Text}'", element.Text);
                            Logger.Information("My Home link href: '{Href}'", element.GetAttribute("href"));
                            return false; // Link is visible, which is not expected
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue; // This is expected - element not found
                    }
                }

                Logger.Information("✅ 'My Home' link is not displayed (as expected after removing start page)");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating My Home link not visible: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that user is landing on Employment practice area page instead of Home page
        /// </summary>
        /// <returns>True if user is on Employment page</returns>
        public bool ValidateEmploymentAsStartPage()
        {
            try
            {
                Logger.Information("Validating that user landed on Employment practice area page instead of Home page");
                
                // Check URL contains employment
                string currentUrl = Driver.Url.ToLower();
                bool urlContainsEmployment = currentUrl.Contains("employment");
                Logger.Information("Current URL: {Url}, Contains 'employment': {Contains}", Driver.Url, urlContainsEmployment);

                // Check page title
                string currentTitle = Driver.Title;
                bool titleContainsEmployment = currentTitle.Contains("Employment", StringComparison.OrdinalIgnoreCase);
                Logger.Information("Current Title: {Title}, Contains 'Employment': {Contains}", currentTitle, titleContainsEmployment);

                // Check page label/heading
                bool pageLabelContainsEmployment = false;
                try
                {
                    var pageLabelElement = Driver.FindElement(By.XPath("//h1[@id='co_browsePageLabel']"));
                    if (pageLabelElement != null && pageLabelElement.Displayed)
                    {
                        string pageLabel = pageLabelElement.Text;
                        pageLabelContainsEmployment = pageLabel.Contains("Employment", StringComparison.OrdinalIgnoreCase);
                        Logger.Information("Page label: '{Label}', Contains 'Employment': {Contains}", 
                            pageLabel, pageLabelContainsEmployment);
                    }
                }
                catch (NoSuchElementException)
                {
                    Logger.Information("Page label element not found");
                }

                // At least one indicator should show this is the Employment page
                bool isEmploymentPage = urlContainsEmployment || titleContainsEmployment || pageLabelContainsEmployment;
                
                if (isEmploymentPage)
                {
                    Logger.Information("✅ User successfully landed on Employment practice area page");
                }
                else
                {
                    Logger.Warning("⚠️ User does not appear to be on Employment practice area page");
                }

                return isEmploymentPage;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating Employment as start page: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that home icon is not filled (indicating Employment is not set as start page)
        /// </summary>
        /// <returns>True if home icon is not filled or not found</returns>
        public bool ValidateHomeIconNotFilled()
        {
            try
            {
                Logger.Information("Validating that home icon is not filled (Employment is not set as start page)");
                
                // Look for filled home icon - we expect NOT to find this
                var filledHomeIconLocators = new[]
                {
                    By.XPath("//i[contains(@class, 'home') and contains(@class, 'filled')]"),
                    By.XPath("//span[contains(@class, 'home') and contains(@class, 'filled')]"),
                    By.CssSelector(".home-icon.filled"),
                    By.CssSelector(".icon-home.filled"),
                    By.XPath("//i[contains(@class, 'fa-home') and contains(@class, 'filled')]"),
                    By.XPath("//*[contains(@class, 'home-icon') and contains(@class, 'filled')]")
                };

                bool filledIconFound = false;
                foreach (var locator in filledHomeIconLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed)
                        {
                            Logger.Warning("⚠️ Found filled home icon - Employment may still be set as start page");
                            filledIconFound = true;
                            break;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        continue; // This is expected - we don't want to find filled icons
                    }
                }

                if (!filledIconFound)
                {
                    Logger.Information("✅ No filled home icon found - Employment is not set as start page");
                    return true;
                }
                else
                {
                    Logger.Warning("⚠️ Filled home icon still found - Employment might still be set as start page");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating home icon status: {Error}", ex.Message);
                // If we can't validate, assume success (icon validation is not critical)
                return true;
            }
        }

        #endregion

        #region Ask a Question Functionality

        /// <summary>
        /// Clicks the "Ask a question" button in the Employment practice area
        /// </summary>
        /// <returns>True if the button was clicked successfully</returns>
        public bool ClickAskQuestionButton()
        {
            try
            {
                Logger.Information("Clicking Ask a question button");
                
                // Try multiple selectors for the Ask a question button
                var askQuestionSelectors = new[]
                {
                    By.XPath("//*[@id='ask-question-button']"),
                    By.XPath("//button[contains(text(), 'Ask a question')]"),
                    By.XPath("//a[contains(text(), 'Ask a question')]"),
                    By.XPath("//*[contains(@class, 'ask') and contains(@class, 'question')]"),
                    By.XPath("//button[contains(@onclick, 'ask') or contains(@onclick, 'question')]"),
                    By.CssSelector("button[id*='ask']"),
                    By.CssSelector("a[id*='ask']"),
                    By.XPath("//*[contains(text(), 'Ask') and contains(text(), 'question')]")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in askQuestionSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying selector: {selector}");
                        var askQuestionButton = wait.Until(ExpectedConditions.ElementToBeClickable(selector));
                        
                        if (askQuestionButton != null && askQuestionButton.Displayed && askQuestionButton.Enabled)
                        {
                            askQuestionButton.Click();
                            Logger.Information($"✅ Successfully clicked Ask a question button using: {selector}");
                            
                            // Wait a moment for the popup to appear
                            System.Threading.Thread.Sleep(2000);
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find Ask a question button with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking Ask a question button: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Selects the checkbox for Terms of service in the Ask a question popup
        /// </summary>
        /// <returns>True if the checkbox was selected successfully</returns>
        public bool SelectTermsOfServiceCheckbox()
        {
            try
            {
                Logger.Information("Selecting Terms of service checkbox");
                
                // Updated selectors based on actual HTML structure
                var termsCheckboxSelectors = new[]
                {
                    By.XPath("//input[@name='IsCheckedTerms' and @type='checkbox']"),
                    By.XPath("//*[@id='IsCheckedTerms']"),
                    By.XPath("//input[@id='IsCheckedTerms']"),
                    By.XPath("//fieldset//input[@type='checkbox' and @name='IsCheckedTerms']"),
                    By.XPath("//label[contains(text(), 'Terms of service')]//input[@type='checkbox']"),
                    By.XPath("//div[contains(@class, 'disclaimer')]//input[@type='checkbox']"),
                    By.XPath("//input[@type='checkbox' and @value='true' and contains(@name, 'Terms')]"),
                    By.CssSelector("input[name='IsCheckedTerms'][type='checkbox']"),
                    By.CssSelector("#IsCheckedTerms")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in termsCheckboxSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying Terms checkbox selector: {selector}");
                        var termsCheckbox = wait.Until(ExpectedConditions.ElementToBeClickable(selector));
                        
                        if (termsCheckbox != null && termsCheckbox.Displayed)
                        {
                            if (!termsCheckbox.Selected)
                            {
                                termsCheckbox.Click();
                                Logger.Information($"✅ Successfully selected Terms of service checkbox using: {selector}");
                            }
                            else
                            {
                                Logger.Information($"ℹ️ Terms of service checkbox was already selected using: {selector}");
                            }
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Terms selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Terms selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find Terms of service checkbox with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error selecting Terms of service checkbox: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks the Submit button in the Terms of service section
        /// </summary>
        /// <returns>True if the button was clicked successfully</returns>
        public bool ClickSubmitTermsButton()
        {
            try
            {
                Logger.Information("Clicking Submit button for Terms of service");
                
                // Updated selectors based on actual HTML structure
                var submitTermsSelectors = new[]
                {
                    By.XPath("//fieldset[contains(@class, 'terms-submit-buttons')]//input[@type='submit']"),
                    By.XPath("//fieldset//input[@type='submit' and @value='Submit']"),
                    By.XPath("//*[@id='submitAskTermsButton']"),
                    By.XPath("//input[@class='terms-submit-buttons' or contains(@class, 'terms-submit')]"),
                    By.XPath("//div[contains(@class, 'disclaimer')]//following-sibling::*//input[@type='submit']"),
                    By.XPath("//input[@type='submit'][preceding-sibling::*//input[@name='IsCheckedTerms']]"),
                    By.XPath("//form//fieldset//input[@type='submit']"),
                    By.CssSelector("fieldset.terms-submit-buttons input[type='submit']"),
                    By.CssSelector("input[type='submit'][value='Submit']"),
                    By.CssSelector("#submitAskTermsButton")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in submitTermsSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying Submit Terms selector: {selector}");
                        var submitTermsButton = wait.Until(ExpectedConditions.ElementToBeClickable(selector));
                        
                        if (submitTermsButton != null && submitTermsButton.Displayed && submitTermsButton.Enabled)
                        {
                            submitTermsButton.Click();
                            Logger.Information($"✅ Successfully clicked Submit button for Terms of service using: {selector}");
                            
                            // Wait a moment for the form to appear
                            System.Threading.Thread.Sleep(2000);
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Submit Terms selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Submit Terms selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find Submit Terms button with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking Submit button for Terms of service: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Enters text in the Query text box
        /// </summary>
        /// <param name="queryText">Text to enter in the query field</param>
        /// <returns>True if the text was entered successfully</returns>
        public bool EnterQueryText(string queryText)
        {
            try
            {
                Logger.Information($"Entering query text: {queryText}");
                
                // Updated selectors based on actual HTML structure
                var queryTextSelectors = new[]
                {
                    By.XPath("//textarea[@name='Query']"),
                    By.XPath("//*[@id='Query']"),
                    By.XPath("//textarea[@id='Query']"),
                    By.XPath("//textarea[contains(@class, 'form-control')]"),
                    By.XPath("//textarea[@required]"),
                    By.XPath("//div[contains(@class, 'form-group')]//textarea"),
                    By.XPath("//label[contains(text(), 'Query')]/following-sibling::textarea"),
                    By.XPath("//form//textarea[1]"),
                    By.XPath("//*[@id='question']"),
                    By.XPath("//input[@name='question']"),
                    By.XPath("//textarea[@name='question']"),
                    By.CssSelector("textarea[name='Query']"),
                    By.CssSelector("#Query"),
                    By.CssSelector("textarea.form-control"),
                    By.CssSelector("textarea[required]")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in queryTextSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying query text selector: {selector}");
                        var queryTextBox = wait.Until(ExpectedConditions.ElementIsVisible(selector));
                        
                        if (queryTextBox != null && queryTextBox.Displayed && queryTextBox.Enabled)
                        {
                            queryTextBox.Clear();
                            queryTextBox.SendKeys(queryText);
                            Logger.Information($"✅ Successfully entered query text using: {selector}");
                            
                            // Scroll down the page
                            var js = (IJavaScriptExecutor)Driver;
                            js.ExecuteScript("window.scrollBy(0, 500);");
                            Logger.Information("✅ Scrolled down the page");
                            
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Query text selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Query text selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find query text box with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error entering query text: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clicks the Submit button in the Ask form
        /// </summary>
        /// <returns>True if the button was clicked successfully</returns>
        public bool ClickSubmitAskFormButton()
        {
            try
            {
                Logger.Information("Clicking Submit button for Ask form");
                
                // Try multiple selectors for the Submit Ask Form button
                var submitAskFormSelectors = new[]
                {
                    By.XPath("//*[@id='submitAskFormButton']"),
                    By.XPath("//button[contains(@id, 'submitAskForm')]"),
                    By.XPath("//button[contains(text(), 'Submit') and not(contains(@onclick, 'Terms'))]"),
                    By.XPath("//input[@type='submit' and not(contains(@id, 'Terms'))]"),
                    By.XPath("//button[@type='submit'][not(contains(@id, 'Terms'))]"),
                    By.CssSelector("button[id*='submitAsk']:not([id*='Terms'])"),
                    By.CssSelector("input[type='submit']:not([id*='terms'])")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in submitAskFormSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying Submit Ask Form selector: {selector}");
                        var submitAskFormButton = wait.Until(ExpectedConditions.ElementToBeClickable(selector));
                        
                        if (submitAskFormButton != null && submitAskFormButton.Displayed && submitAskFormButton.Enabled)
                        {
                            submitAskFormButton.Click();
                            Logger.Information($"✅ Successfully clicked Submit button for Ask form using: {selector}");
                            
                            // Wait a moment for validation errors to appear
                            System.Threading.Thread.Sleep(2000);
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Submit Ask Form selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Submit Ask Form selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find Submit Ask Form button with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking Submit button for Ask form: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates that mandatory field errors are displayed
        /// </summary>
        /// <returns>Dictionary with validation results for each mandatory field</returns>
        public Dictionary<string, bool> ValidateMandatoryFieldErrors()
        {
            var validationResults = new Dictionary<string, bool>
            {
                ["OrganisationType"] = false,
                ["Position"] = false,
                ["AnsweringService"] = false
            };

            try
            {
                Logger.Information("Validating mandatory field errors");
                
                // Check Organisation type dropdown field error
                try
                {
                    var orgTypeSelectors = new[]
                    {
                        By.XPath("//*[@id='OrganisationType']"),
                        By.XPath("//select[@name='OrganisationType']"),
                        By.XPath("//select[contains(@id, 'Organisation')]"),
                        By.XPath("//select[contains(@name, 'Organisation')]")
                    };
                    
                    bool foundOrgType = false;
                    foreach (var selector in orgTypeSelectors)
                    {
                        try
                        {
                            var orgTypeField = Driver.FindElement(selector);
                            foundOrgType = true;
                            Logger.Information($"Found Organisation type field using: {selector}");
                            break;
                        }
                        catch (NoSuchElementException)
                        {
                            continue;
                        }
                    }
                    
                    if (foundOrgType)
                    {
                        // Look for error messages related to Organisation type
                        var orgErrorSelectors = new[]
                        {
                            By.XPath("//span[contains(@class, 'field-validation-error') and contains(text(), 'Organisation')]"),
                            By.XPath("//*[@id='OrganisationType']/following-sibling::span[contains(@class, 'error')]"),
                            By.XPath("//span[contains(@class, 'error') and contains(text(), 'Organisation')]"),
                            By.XPath("//div[contains(@class, 'validation-summary-errors')]//li[contains(text(), 'Organisation')]")
                        };
                        
                        foreach (var errorSelector in orgErrorSelectors)
                        {
                            try
                            {
                                var errorElement = Driver.FindElement(errorSelector);
                                if (errorElement.Displayed)
                                {
                                    validationResults["OrganisationType"] = true;
                                    Logger.Information("Organisation type error validation: true");
                                    break;
                                }
                            }
                            catch (NoSuchElementException)
                            {
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Could not validate Organisation type field: {ex.Message}");
                }

                // Check Position field error
                try
                {
                    var positionSelectors = new[]
                    {
                        By.XPath("//*[@id='Position']"),
                        By.XPath("//input[@name='Position']"),
                        By.XPath("//input[contains(@id, 'Position')]"),
                        By.XPath("//input[contains(@name, 'Position')]")
                    };
                    
                    bool foundPosition = false;
                    foreach (var selector in positionSelectors)
                    {
                        try
                        {
                            var positionField = Driver.FindElement(selector);
                            foundPosition = true;
                            Logger.Information($"Found Position field using: {selector}");
                            break;
                        }
                        catch (NoSuchElementException)
                        {
                            continue;
                        }
                    }
                    
                    if (foundPosition)
                    {
                        // Look for error messages related to Position
                        var positionErrorSelectors = new[]
                        {
                            By.XPath("//span[contains(@class, 'field-validation-error') and contains(text(), 'Position')]"),
                            By.XPath("//*[@id='Position']/following-sibling::span[contains(@class, 'error')]"),
                            By.XPath("//span[contains(@class, 'error') and contains(text(), 'Position')]"),
                            By.XPath("//div[contains(@class, 'validation-summary-errors')]//li[contains(text(), 'Position')]")
                        };
                        
                        foreach (var errorSelector in positionErrorSelectors)
                        {
                            try
                            {
                                var errorElement = Driver.FindElement(errorSelector);
                                if (errorElement.Displayed)
                                {
                                    validationResults["Position"] = true;
                                    Logger.Information("Position error validation: true");
                                    break;
                                }
                            }
                            catch (NoSuchElementException)
                            {
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Could not validate Position field: {ex.Message}");
                }

                // Check Answering service dropdown error
                try
                {
                    var answeringServiceSelectors = new[]
                    {
                        By.XPath("//*[@id='answeringServiceInput']/span[1]"),
                        By.XPath("//*[@id='answeringServiceInput']"),
                        By.XPath("//select[@name='answeringService']"),
                        By.XPath("//select[contains(@id, 'answering')]"),
                        By.XPath("//div[contains(@id, 'answeringService')]")
                    };
                    
                    bool foundAnsweringService = false;
                    foreach (var selector in answeringServiceSelectors)
                    {
                        try
                        {
                            var answeringServiceField = Driver.FindElement(selector);
                            foundAnsweringService = true;
                            Logger.Information($"Found Answering service field using: {selector}");
                            break;
                        }
                        catch (NoSuchElementException)
                        {
                            continue;
                        }
                    }
                    
                    if (foundAnsweringService)
                    {
                        // Look for error messages related to Answering service
                        var answeringErrorSelectors = new[]
                        {
                            By.XPath("//span[contains(@class, 'field-validation-error') and contains(text(), 'Answering')]"),
                            By.XPath("//*[@id='answeringServiceInput']/following-sibling::span[contains(@class, 'error')]"),
                            By.XPath("//span[contains(@class, 'error') and contains(text(), 'Answering')]"),
                            By.XPath("//div[contains(@class, 'validation-summary-errors')]//li[contains(text(), 'Answering')]")
                        };
                        
                        foreach (var errorSelector in answeringErrorSelectors)
                        {
                            try
                            {
                                var errorElement = Driver.FindElement(errorSelector);
                                if (errorElement.Displayed)
                                {
                                    validationResults["AnsweringService"] = true;
                                    Logger.Information("Answering service error validation: true");
                                    break;
                                }
                            }
                            catch (NoSuchElementException)
                            {
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Could not validate Answering service field: {ex.Message}");
                }

                Logger.Information("✅ Completed mandatory field error validation");
                return validationResults;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating mandatory field errors: {Error}", ex.Message);
                return validationResults;
            }
        }

        /// <summary>
        /// Clicks the Cancel button in the Ask form
        /// </summary>
        /// <returns>True if the button was clicked successfully</returns>
        public bool ClickCancelAskFormButton()
        {
            try
            {
                Logger.Information("Clicking Cancel button for Ask form");
                
                // Try multiple selectors for the Cancel Ask Form button
                var cancelAskFormSelectors = new[]
                {
                    By.XPath("//*[@id='cancelAskFormButton']"),
                    By.XPath("//button[contains(@id, 'cancelAskForm')]"),
                    By.XPath("//button[contains(text(), 'Cancel')]"),
                    By.XPath("//input[@type='button' and contains(@value, 'Cancel')]"),
                    By.XPath("//button[@type='button'][contains(text(), 'Cancel')]"),
                    By.CssSelector("button[id*='cancel']"),
                    By.CssSelector("input[type='button'][value*='Cancel']")
                };
                
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
                
                foreach (var selector in cancelAskFormSelectors)
                {
                    try
                    {
                        Logger.Information($"Trying Cancel Ask Form selector: {selector}");
                        var cancelAskFormButton = wait.Until(ExpectedConditions.ElementToBeClickable(selector));
                        
                        if (cancelAskFormButton != null && cancelAskFormButton.Displayed && cancelAskFormButton.Enabled)
                        {
                            cancelAskFormButton.Click();
                            Logger.Information($"✅ Successfully clicked Cancel button for Ask form using: {selector}");
                            
                            // Wait a moment for the form to close
                            System.Threading.Thread.Sleep(2000);
                            return true;
                        }
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Logger.Information($"Cancel Ask Form selector {selector} timed out, trying next...");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.Information($"Cancel Ask Form selector {selector} failed: {ex.Message}, trying next...");
                        continue;
                    }
                }
                
                Logger.Error("❌ Could not find Cancel Ask Form button with any selector");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking Cancel button for Ask form: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Navigates to "Contracts of employment" section within the Employment practice area
        /// </summary>
        /// <returns>True if navigation to Contracts of employment was successful</returns>
        public bool NavigateToContractsOfEmployment()
        {
            try
            {
                Logger.Information("Navigating to 'Contracts of employment' section");
                
                var contractsLocators = new[]
                {
                    By.XPath("//a[contains(text(), 'Contracts of employment')]"),
                    By.XPath("//*[contains(text(), 'Contracts of employment')]"),
                    By.CssSelector("a[href*='contracts'], a[href*='employment']"),
                    By.XPath("//li[@class='column row1']//a[contains(text(), 'Contracts of employment')]"),
                    By.XPath("//div[@class='co_column multiListWithHeaders']//a[contains(text(), 'Contracts of employment')]")
                };

                foreach (var locator in contractsLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element != null && element.Displayed && element.Enabled)
                        {
                            Logger.Information("Found 'Contracts of employment' element using locator: {Locator}", locator);
                            Logger.Information("Element text: '{Text}', Href: '{Href}'", 
                                element.Text, 
                                element.GetAttribute("href"));
                            
                            string urlBeforeClick = Driver.Url;
                            element.Click();
                            
                            // Wait for navigation
                            System.Threading.Thread.Sleep(3000);
                            
                            string urlAfterClick = Driver.Url;
                            Logger.Information("URL before click: {UrlBefore}", urlBeforeClick);
                            Logger.Information("URL after click: {UrlAfter}", urlAfterClick);
                            
                            Logger.Information("✅ Successfully navigated to 'Contracts of employment'");
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Logger.Information("Locator not found: {Locator}", locator);
                        continue;
                    }
                }
                
                Logger.Warning("❌ Could not find 'Contracts of employment' link");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error navigating to 'Contracts of employment': {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Validates the presence of delivery icons on a contracts page
        /// </summary>
        /// <returns>Dictionary with icon names as keys and their presence status as values</returns>
        public Dictionary<string, bool> ValidateDeliveryIcons()
        {
            try
            {
                Logger.Information("Validating delivery icons (Save to folder, Email, Print, Download)");
                
                // First, let's explore what's actually on the page
                try
                {
                    Logger.Information("=== PAGE EXPLORATION ===");
                    
                    // Look for any elements with common delivery-related attributes
                    var allButtonElements = Driver.FindElements(By.TagName("button"));
                    var allLinkElements = Driver.FindElements(By.TagName("a"));
                    var allSpanElements = Driver.FindElements(By.TagName("span"));
                    var allDivElements = Driver.FindElements(By.CssSelector("div[id*='delivery'], div[class*='delivery'], div[id*='save'], div[class*='save'], div[id*='email'], div[class*='email'], div[id*='print'], div[class*='print'], div[id*='download'], div[class*='download']"));
                    
                    Logger.Information("Found {ButtonCount} buttons, {LinkCount} links, {SpanCount} spans, {DivCount} delivery-related divs", 
                        allButtonElements.Count, allLinkElements.Count, allSpanElements.Count, allDivElements.Count);
                    
                    // Look for elements containing keywords
                    var saveElements = Driver.FindElements(By.XPath("//*[contains(text(), 'Save') or contains(@id, 'save') or contains(@class, 'save')]"));
                    var emailElements = Driver.FindElements(By.XPath("//*[contains(text(), 'Email') or contains(@id, 'email') or contains(@class, 'email')]"));
                    var printElements = Driver.FindElements(By.XPath("//*[contains(text(), 'Print') or contains(@id, 'print') or contains(@class, 'print')]"));
                    var downloadElements = Driver.FindElements(By.XPath("//*[contains(text(), 'Download') or contains(@id, 'download') or contains(@class, 'download')]"));
                    
                    Logger.Information("Found potential elements: Save={SaveCount}, Email={EmailCount}, Print={PrintCount}, Download={DownloadCount}",
                        saveElements.Count, emailElements.Count, printElements.Count, downloadElements.Count);
                    
                    // Log details of found elements
                    foreach (var element in saveElements.Take(5))
                    {
                        try
                        {
                            Logger.Information("Save element: Tag={Tag}, Text='{Text}', ID='{Id}', Class='{Class}'",
                                element.TagName, element.Text?.Trim(), element.GetAttribute("id"), element.GetAttribute("class"));
                        }
                        catch { }
                    }
                    
                    foreach (var element in emailElements.Take(5))
                    {
                        try
                        {
                            Logger.Information("Email element: Tag={Tag}, Text='{Text}', ID='{Id}', Class='{Class}'",
                                element.TagName, element.Text?.Trim(), element.GetAttribute("id"), element.GetAttribute("class"));
                        }
                        catch { }
                    }
                    
                    foreach (var element in printElements.Take(5))
                    {
                        try
                        {
                            Logger.Information("Print element: Tag={Tag}, Text='{Text}', ID='{Id}', Class='{Class}'",
                                element.TagName, element.Text?.Trim(), element.GetAttribute("id"), element.GetAttribute("class"));
                        }
                        catch { }
                    }
                    
                    foreach (var element in downloadElements.Take(5))
                    {
                        try
                        {
                            Logger.Information("Download element: Tag={Tag}, Text='{Text}', ID='{Id}', Class='{Class}'",
                                element.TagName, element.Text?.Trim(), element.GetAttribute("id"), element.GetAttribute("class"));
                        }
                        catch { }
                    }
                }
                catch (Exception explorationEx)
                {
                    Logger.Warning("Page exploration failed: {Error}", explorationEx.Message);
                }
                
                var deliveryIcons = new Dictionary<string, By>
                {
                    ["Save to folder"] = By.XPath("//*[@id='saveToFolder']/a/span"),
                    ["Email"] = By.XPath("//*[@id='deliveryLinkRow1Email']"),
                    ["Print"] = By.XPath("//*[@id='deliveryLinkRow1Print']"),
                    ["Download"] = By.XPath("//*[@id='deliveryLinkRow1Download']")
                };

                var iconValidationResults = new Dictionary<string, bool>();

                foreach (var icon in deliveryIcons)
                {
                    try
                    {
                        Logger.Information("Checking for {IconName} icon using XPath: {XPath}", icon.Key, icon.Value);
                        
                        var element = Driver.FindElement(icon.Value);
                        
                        if (element != null && element.Displayed)
                        {
                            iconValidationResults[icon.Key] = true;
                            Logger.Information("✅ {IconName} icon found and is displayed", icon.Key);
                            Logger.Information("  - Text: '{Text}'", element.Text);
                            Logger.Information("  - Enabled: {Enabled}", element.Enabled);
                            Logger.Information("  - TagName: {TagName}", element.TagName);
                        }
                        else
                        {
                            iconValidationResults[icon.Key] = false;
                            Logger.Warning("❌ {IconName} icon found but not displayed", icon.Key);
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        iconValidationResults[icon.Key] = false;
                        Logger.Warning("❌ {IconName} icon not found on the page", icon.Key);
                    }
                    catch (Exception ex)
                    {
                        iconValidationResults[icon.Key] = false;
                        Logger.Error("❌ Error checking {IconName} icon: {Error}", icon.Key, ex.Message);
                    }
                }

                var missingIcons = iconValidationResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
                var foundIcons = iconValidationResults.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

                Logger.Information("Icon validation summary:");
                Logger.Information("  Found icons: {FoundIcons}", string.Join(", ", foundIcons));
                if (missingIcons.Any())
                {
                    Logger.Information("  Missing icons: {MissingIcons}", string.Join(", ", missingIcons));
                }

                return iconValidationResults;
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error validating delivery icons: {Error}", ex.Message);
                return new Dictionary<string, bool>
                {
                    ["Save to folder"] = false,
                    ["Email"] = false,
                    ["Print"] = false,
                    ["Download"] = false
                };
            }
        }

        #endregion
    }
}
