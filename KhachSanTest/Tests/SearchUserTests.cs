using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using System.Threading;

namespace KhachSanTest.Tests
{
    public class SearchUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        SearchUserPage searchPage;
        WebDriverWait wait;

        private string currentTestName = "SearchUser";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Password")));

            loginPage = new LoginPage(driver);
            searchPage = new SearchUserPage(driver);
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetSearchUserTestCases))]
        public void Test_SearchUser(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";
            string screenshotPath = "";
            string lastKeyword = "";

            currentTestName = $"TC_{tc.TestCaseId}_{tc.SheetName}";

            try
            {
                foreach (var step in tc.Steps)
                {
                    // Ghi lại keyword đang tìm để kiểm tra kết quả sau
                    if (step.Action != null && step.Action.ToLower().Contains("nhập"))
                        lastKeyword = step.Data ?? "";

                    ExecuteStep(step);
                }

                string expected = tc.Steps
                    .LastOrDefault(s => !string.IsNullOrEmpty(s.Expected))?.Expected ?? "";

                actual = searchPage.GetSearchResult(lastKeyword);

                bool isMatch = CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                if (!isMatch)
                {
                    screenshotPath = ScreenshotHelper.TakeScreenshot(driver, currentTestName + "_FAILED");
                    TestContext.WriteLine($"[Screenshot - FAILED] {screenshotPath}");
                }

                Assert.That(isMatch, Is.True,
                    $"Kết quả không khớp.\nExpected: '{expected}'\nActual:   '{actual}'");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = "Lỗi: " + ex.Message;

                if (driver != null)
                {
                    screenshotPath = ScreenshotHelper.TakeScreenshot(driver, currentTestName + "_EXCEPTION");
                    TestContext.WriteLine($"[Screenshot - EXCEPTION] {screenshotPath}");
                }

                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, screenshotPath);
            }
        }

        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;

            string action = step.Action.ToLower().Trim();
            string data = step.Data ?? "";

            if (action.Contains("username"))
                loginPage.EnterUsername(data);

            else if (action.Contains("password"))
                loginPage.EnterPassword(data);

            else if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1500);
            }

            else if (action.Contains("vào trang"))
                searchPage.GoToUsers();

            // "nhập tên cần tìm", "nhập email cần tìm", "nhập quyền", "nhập thông tin tìm kiếm"
            else if (action.Contains("nhập") && !action.Contains("username") && !action.Contains("password"))
                searchPage.EnterSearch(data);

            // "để trống ô tìm kiếm" → tìm với chuỗi rỗng
            else if (action.Contains("để trống"))
                searchPage.EnterSearch("");

            else if (action.Contains("tìm kiếm"))
                searchPage.ClickSearch();

            // "xem kết quả" → không làm gì, kết quả lấy sau vòng lặp
        }

        /// <summary>
        /// So sánh NGỮ NGHĨA giữa expected (mô tả trong Excel) và actual (kết quả từ UI).
        ///
        /// Các nhóm expected từ Excel:
        ///   - "phù hợp chính xác" / "hiển thị tài khoản" / "có vai trò" / "toàn bộ danh sách"
        ///     → kỳ vọng TÌM THẤY ≥ 1 kết quả
        ///     → actual phải chứa "thành công" (= "Tìm kiếm thành công: N kết quả")
        ///
        ///   - "không có kết quả" / "danh sách trống"
        ///     → kỳ vọng KHÔNG TÌM THẤY
        ///     → actual phải chứa "không tìm thấy"
        /// </summary>
        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return false;

            string e = expected.ToLower().Trim();
            string a = actual.ToLower().Trim();

            // ── Case 1: Kỳ vọng TÌM THẤY kết quả ──
            bool expectedFound =
                e.Contains("phù hợp") ||
                e.Contains("chính xác") ||
                e.Contains("có vai trò") ||
                e.Contains("toàn bộ danh sách") ||
                e.Contains("hiển thị tài khoản");

            if (expectedFound)
                return a.Contains("thành công");

            // ── Case 2: Kỳ vọng KHÔNG TÌM THẤY ──
            bool expectedNotFound =
                e.Contains("không có kết quả") ||
                e.Contains("danh sách trống") ||
                e.Contains("không tìm thấy");

            if (expectedNotFound)
                return a.Contains("không tìm thấy");

            // ── Fallback: so sánh trực tiếp ──
            return a.Contains(e) || e.Contains(a);
        }

        [TearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}