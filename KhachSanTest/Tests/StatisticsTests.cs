using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using System.Threading;

namespace KhachSanTest.Tests
{
    public class StatisticsTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        StatisticsPage statisticsPage;

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetStatisticsTestCases))]
        public void Test_Statistics(ExcelDataProvider.TestCase tc)
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            statisticsPage = new StatisticsPage(driver);

            string actual = "";
            string status = "Passed";
            string image = "";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = GetExpected(tc);
                string actualResult = GetActual();

                actual = actualResult;

                bool isMatch = actualResult.ToLower().Contains(expected.ToLower());
                status = isMatch ? "Passed" : "Failed";

                Assert.That(isMatch, Is.True, $"Sai TC: {tc.TestCaseId}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;
                image = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, image);
                driver.Quit();
            }
        }

        // ================= STEP =================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // LOGIN
            if (action.Contains("username"))
                loginPage.EnterUsername(data);

            else if (action.Contains("password"))
                loginPage.EnterPassword(data);

            else if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1000);
            }

            // VÀO TRANG THỐNG KÊ
            else if (action.Contains("thống kê"))
            {
                driver.Navigate().GoToUrl("http://localhost:58609/Statistics");
                Thread.Sleep(1000);
            }

            // NHẬP NGÀY
            else if (action.Contains("từ ngày"))
                statisticsPage.EnterFromDate(data);

            else if (action.Contains("đến ngày"))
                statisticsPage.EnterToDate(data);

            // CLICK XEM
            else if (action.Contains("xem"))
            {
                statisticsPage.ClickView();
                Thread.Sleep(1500);
            }
        }

        // ================= EXPECTED =================
        private string GetExpected(ExcelDataProvider.TestCase tc)
        {
            return tc.Steps
                .Where(s => !string.IsNullOrEmpty(s.Expected))
                .LastOrDefault()?.Expected ?? "";
        }

        // ================= ACTUAL =================
        private string GetActual()
        {
            try
            {
                return driver.PageSource; // check text toàn trang
            }
            catch
            {
                return "";
            }
        }
        [TearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                // 1. Đóng trình duyệt và kết thúc session
                driver.Quit();

                // 2. Giải phóng tài nguyên bộ nhớ (Sửa lỗi bạn đang gặp)
                driver.Dispose();

                // 3. Đưa về null để tránh dùng nhầm ở TC sau
                driver = null;
            }
        }
    }
}