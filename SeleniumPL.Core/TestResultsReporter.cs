using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Serilog;
using NUnit.Framework;
using OpenQA.Selenium;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Enhanced test results reporter that captures comprehensive execution details
    /// </summary>
    public class TestResultsReporter
    {
        private readonly ILogger _logger;
        private readonly string _resultsPath;
        private readonly string _reportsPath;
        private readonly List<TestExecutionDetails> _testExecutions;

        public TestResultsReporter(ILogger logger, string resultsPath = "TestResults", string reportsPath = "Reports")
        {
            _logger = logger;
            _resultsPath = resultsPath;
            _reportsPath = reportsPath;
            _testExecutions = new List<TestExecutionDetails>();

            // Ensure directories exist
            Directory.CreateDirectory(_resultsPath);
            Directory.CreateDirectory(_reportsPath);
        }

        /// <summary>
        /// Capture detailed test execution information
        /// </summary>
        public void CaptureTestExecution(TestExecutionDetails details)
        {
            try
            {
                _testExecutions.Add(details);
                
                // Save individual test result file
                var testFileName = $"{details.TestName}_{details.StartTime:yyyyMMdd_HHmmss}.json";
                var testFilePath = Path.Combine(_resultsPath, testFileName);
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(details, options);
                File.WriteAllText(testFilePath, json, Encoding.UTF8);
                
                // Also create a detailed text report
                var textReportPath = Path.Combine(_resultsPath, $"{details.TestName}_{details.StartTime:yyyyMMdd_HHmmss}.txt");
                CreateDetailedTextReport(details, textReportPath);
                
                _logger.Information("Test execution captured: {TestName} - Results saved to {Path}", 
                    details.TestName, testFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to capture test execution details for {TestName}", details?.TestName);
            }
        }

        /// <summary>
        /// Generate comprehensive test session report
        /// </summary>
        public void GenerateSessionReport(string? sessionName = null)
        {
            try
            {
                var session = sessionName ?? $"TestSession_{DateTime.Now:yyyyMMdd_HHmmss}";
                var sessionReportPath = Path.Combine(_reportsPath, $"{session}_FullReport.html");
                var sessionJsonPath = Path.Combine(_reportsPath, $"{session}_Summary.json");
                
                // Create HTML report
                CreateHtmlReport(sessionReportPath, session);
                
                // Create JSON summary
                CreateJsonSummary(sessionJsonPath, session);
                
                _logger.Information("Session report generated: {Path}", sessionReportPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate session report");
            }
        }

        /// <summary>
        /// Create detailed text report for individual test
        /// </summary>
        private void CreateDetailedTextReport(TestExecutionDetails details, string filePath)
        {
            var report = new StringBuilder();
            
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine($"TEST EXECUTION REPORT: {details.TestName}");
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine();
            
            // Test Summary
            report.AppendLine("TEST SUMMARY");
            report.AppendLine("-".PadRight(40, '-'));
            report.AppendLine($"Test Name: {details.TestName}");
            report.AppendLine($"Test Class: {details.TestClass}");
            report.AppendLine($"Test Category: {details.TestCategory}");
            report.AppendLine($"Test Description: {details.TestDescription}");
            report.AppendLine($"Result: {details.Result}");
            report.AppendLine($"Start Time: {details.StartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"End Time: {details.EndTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Duration: {details.Duration.TotalSeconds:F2} seconds");
            report.AppendLine();

            // Environment Information
            report.AppendLine("ENVIRONMENT INFORMATION");
            report.AppendLine("-".PadRight(40, '-'));
            if (details.EnvironmentInfo != null)
            {
                foreach (var kvp in details.EnvironmentInfo)
                {
                    report.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }
            report.AppendLine();

            // Test Steps and Actions
            if (details.TestSteps?.Count > 0)
            {
                report.AppendLine("TEST EXECUTION STEPS");
                report.AppendLine("-".PadRight(40, '-'));
                for (int i = 0; i < details.TestSteps.Count; i++)
                {
                    var step = details.TestSteps[i];
                    report.AppendLine($"Step {i + 1}: [{step.Timestamp:HH:mm:ss.fff}] {step.Action}");
                    if (!string.IsNullOrEmpty(step.Details))
                    {
                        report.AppendLine($"         Details: {step.Details}");
                    }
                    if (!string.IsNullOrEmpty(step.Result))
                    {
                        report.AppendLine($"         Result: {step.Result}");
                    }
                    report.AppendLine();
                }
            }

            // Error Information
            if (!string.IsNullOrEmpty(details.ErrorMessage))
            {
                report.AppendLine("ERROR DETAILS");
                report.AppendLine("-".PadRight(40, '-'));
                report.AppendLine($"Error Message: {details.ErrorMessage}");
                report.AppendLine();
                
                if (!string.IsNullOrEmpty(details.StackTrace))
                {
                    report.AppendLine("Stack Trace:");
                    report.AppendLine(details.StackTrace);
                    report.AppendLine();
                }
            }

            // Console Output
            if (details.ConsoleOutput?.Count > 0)
            {
                report.AppendLine("CONSOLE OUTPUT");
                report.AppendLine("-".PadRight(40, '-'));
                foreach (var output in details.ConsoleOutput)
                {
                    report.AppendLine($"[{output.Timestamp:HH:mm:ss.fff}] {output.Level}: {output.Message}");
                }
                report.AppendLine();
            }

            // Attachments
            if (details.Attachments?.Count > 0)
            {
                report.AppendLine("ATTACHMENTS");
                report.AppendLine("-".PadRight(40, '-'));
                foreach (var attachment in details.Attachments)
                {
                    report.AppendLine($"Type: {attachment.Type} | Path: {attachment.FilePath} | Description: {attachment.Description}");
                }
                report.AppendLine();
            }

            // Performance Metrics
            if (details.PerformanceMetrics?.Count > 0)
            {
                report.AppendLine("PERFORMANCE METRICS");
                report.AppendLine("-".PadRight(40, '-'));
                foreach (var metric in details.PerformanceMetrics)
                {
                    report.AppendLine($"{metric.Key}: {metric.Value}");
                }
                report.AppendLine();
            }

            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine("=".PadRight(80, '='));

            File.WriteAllText(filePath, report.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Create HTML report for all tests in session
        /// </summary>
        private void CreateHtmlReport(string filePath, string sessionName)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine($"    <title>Test Session Report - {sessionName}</title>");
            html.AppendLine("    <style>");
            html.AppendLine(GetHtmlStyles());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine($"    <h1>Test Session Report: {sessionName}</h1>");
            html.AppendLine($"    <p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            
            // Summary
            var totalTests = _testExecutions.Count;
            var passedTests = _testExecutions.Count(t => t.Result == "Passed");
            var failedTests = _testExecutions.Count(t => t.Result == "Failed");
            var totalDuration = _testExecutions.Sum(t => t.Duration.TotalSeconds);
            
            html.AppendLine("    <div class='summary'>");
            html.AppendLine("        <h2>Summary</h2>");
            html.AppendLine($"        <p>Total Tests: {totalTests}</p>");
            html.AppendLine($"        <p>Passed: <span class='passed'>{passedTests}</span></p>");
            html.AppendLine($"        <p>Failed: <span class='failed'>{failedTests}</span></p>");
            html.AppendLine($"        <p>Success Rate: {(totalTests > 0 ? (passedTests * 100.0 / totalTests):0):F1}%</p>");
            html.AppendLine($"        <p>Total Duration: {totalDuration:F2} seconds</p>");
            html.AppendLine("    </div>");
            
            // Test Details
            html.AppendLine("    <h2>Test Details</h2>");
            html.AppendLine("    <table>");
            html.AppendLine("        <tr>");
            html.AppendLine("            <th>Test Name</th>");
            html.AppendLine("            <th>Result</th>");
            html.AppendLine("            <th>Duration</th>");
            html.AppendLine("            <th>Start Time</th>");
            html.AppendLine("            <th>Actions</th>");
            html.AppendLine("        </tr>");
            
            foreach (var test in _testExecutions.OrderBy(t => t.StartTime))
            {
                var resultClass = test.Result.ToLower();
                html.AppendLine("        <tr>");
                html.AppendLine($"            <td>{test.TestName}</td>");
                html.AppendLine($"            <td class='{resultClass}'>{test.Result}</td>");
                html.AppendLine($"            <td>{test.Duration.TotalSeconds:F2}s</td>");
                html.AppendLine($"            <td>{test.StartTime:HH:mm:ss}</td>");
                html.AppendLine("            <td>");
                
                // Add links to detailed reports
                var testFileName = $"{test.TestName}_{test.StartTime:yyyyMMdd_HHmmss}.txt";
                html.AppendLine($"                <a href='../TestResults/{testFileName}'>Details</a>");
                
                if (test.Attachments?.Any(a => a.Type == "Screenshot") == true)
                {
                    var screenshot = test.Attachments.First(a => a.Type == "Screenshot");
                    html.AppendLine($" | <a href='{screenshot.FilePath}'>Screenshot</a>");
                }
                
                html.AppendLine("            </td>");
                html.AppendLine("        </tr>");
            }
            
            html.AppendLine("    </table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Create JSON summary for programmatic access
        /// </summary>
        private void CreateJsonSummary(string filePath, string sessionName)
        {
            var summary = new
            {
                SessionName = sessionName,
                GeneratedAt = DateTime.Now,
                Summary = new
                {
                    TotalTests = _testExecutions.Count,
                    PassedTests = _testExecutions.Count(t => t.Result == "Passed"),
                    FailedTests = _testExecutions.Count(t => t.Result == "Failed"),
                    TotalDuration = _testExecutions.Sum(t => t.Duration.TotalSeconds),
                    SuccessRate = _testExecutions.Count > 0 ? (_testExecutions.Count(t => t.Result == "Passed") * 100.0 / _testExecutions.Count) : 0
                },
                Tests = _testExecutions.Select(t => new
                {
                    t.TestName,
                    t.TestClass,
                    t.Result,
                    t.StartTime,
                    t.EndTime,
                    Duration = t.Duration.TotalSeconds,
                    t.ErrorMessage,
                    StepCount = t.TestSteps?.Count ?? 0,
                    AttachmentCount = t.Attachments?.Count ?? 0
                }).OrderBy(t => t.StartTime)
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(summary, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Get CSS styles for HTML report
        /// </summary>
        private string GetHtmlStyles()
        {
            return @"
                body { font-family: Arial, sans-serif; margin: 20px; }
                h1 { color: #333; border-bottom: 2px solid #ddd; }
                h2 { color: #666; margin-top: 30px; }
                .summary { background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }
                table { width: 100%; border-collapse: collapse; margin-top: 10px; }
                th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
                th { background-color: #f2f2f2; font-weight: bold; }
                .passed { color: green; font-weight: bold; }
                .failed { color: red; font-weight: bold; }
                .error { color: orange; font-weight: bold; }
                a { color: #007bff; text-decoration: none; }
                a:hover { text-decoration: underline; }
            ";
        }

        /// <summary>
        /// Clear captured test executions (call this at the start of a new session)
        /// </summary>
        public void ClearSession()
        {
            _testExecutions.Clear();
            _logger.Information("Test execution session cleared");
        }
    }

    /// <summary>
    /// Detailed test execution information
    /// </summary>
    public class TestExecutionDetails
    {
        public string TestName { get; set; } = string.Empty;
        public string TestClass { get; set; } = string.Empty;
        public string TestCategory { get; set; } = string.Empty;
        public string TestDescription { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
        public Dictionary<string, object>? EnvironmentInfo { get; set; }
        public List<TestStep>? TestSteps { get; set; }
        public List<ConsoleLogEntry>? ConsoleOutput { get; set; }
        public List<TestAttachment>? Attachments { get; set; }
        public Dictionary<string, object>? PerformanceMetrics { get; set; }
    }

    /// <summary>
    /// Individual test step information
    /// </summary>
    public class TestStep
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? Result { get; set; }
    }

    /// <summary>
    /// Console log entry
    /// </summary>
    public class ConsoleLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test attachment information
    /// </summary>
    public class TestAttachment
    {
        public string Type { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
