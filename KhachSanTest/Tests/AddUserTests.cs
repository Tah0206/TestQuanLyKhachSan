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
    public class AddUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        AddUserPage addUserPage;

        // Lưu tên test case hiện tại để đặt tên ảnh
        private string currentTestName = "Test";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            new WebDriverWait(driver, TimeSpan.FromSeconds(15))
                .Until(ExpectedConditions.ElementIsVisible(By.Id("Password")));

            loginPage = new LoginPage(driver);
            addUserPage = new AddUserPage(driver);
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

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetAddUserTestCases))]
        public void Test_AddUser(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";
            string screenshotPath = "";

            // Đặt tên test để ảnh dễ nhận biết
            currentTestName = $"TC_{tc.TestCaseId}_{tc.SheetName}";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = tc.Steps.LastOrDefault(s => !string.IsNullOrEmpty(s.Expected))?.Expected ?? "";
                actual = SafeGetActualResult();

                bool isMatch = CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                // ── CHỤP ẢNH KHI FAILED ──
                if (!isMatch)
                {
                    screenshotPath = ScreenshotHelper.TakeScreenshot(driver, currentTestName + "_FAILED");
                    TestContext.WriteLine($"[Screenshot - FAILED] {screenshotPath}");
                }

                Assert.That(isMatch, Is.True, $"Kết quả không khớp: Expected '{expected}', Actual '{actual}'");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = "Lỗi: " + ex.Message;

                // ── CHỤP ẢNH KHI CÓ EXCEPTION ──
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

        private bool IsOnLoginPage()
        {
            string url = driver.Url.ToLower();
            return url.Contains("/auth/login") || url.Contains("/login")
                   || url.EndsWith(":58609/");
        }

        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;

            string action = step.Action.ToLower().Trim();
            string data = step.Data ?? "";

            if (IsOnLoginPage())
            {
                if (action.Contains("nhập username")) { loginPage.EnterUsername(data); return; }
                if (action.Contains("nhập password")) { loginPage.EnterPassword(data); return; }
                if (action.Contains("nhấn đăng nhập")) { loginPage.ClickLogin(); Thread.Sleep(1500); return; }
            }

            if (action.Contains("vào trang quản trị"))
            { driver.Navigate().GoToUrl("http://localhost:58609/Users"); return; }

            if (action.Contains("thêm tài khoản"))
            { addUserPage.ClickAddUser(); return; }

            if (action.Contains("username") && !IsOnLoginPage())
                addUserPage.EnterUsername(action.Contains("để trống") ? "" : data);

            if ((action.Contains("password") || action.Contains("mật khẩu") || action.Contains("passwordhash"))
                && !IsOnLoginPage())
                addUserPage.EnterPasswordHash(action.Contains("để trống") ? "" : data);

            if (action.Contains("họ") || action.Contains("fullname"))
                addUserPage.EnterFullName(action.Contains("để trống") ? "" : data);

            if (action.Contains("email"))
                addUserPage.EnterEmail(action.Contains("để trống") ? "" : data);

            if (action.Contains("vai trò"))
                addUserPage.SelectRole(data);

            if (action.Contains("tạo tài khoản"))
                addUserPage.ClickCreate();
        }

        private string SafeGetActualResult()
        {
            try
            {
                Thread.Sleep(3000);

                if (driver.Url.ToLower().Contains("/users") && !driver.Url.ToLower().Contains("/create"))
                {
                    return "Thêm tài khoản thành công";
                }

                var errorElements = driver.FindElements(By.CssSelector(".field-validation-error, .text-danger, .validation-summary-errors"));
                foreach (var error in errorElements)
                {
                    if (!string.IsNullOrEmpty(error.Text)) return error.Text.Trim();
                }

                return "Không xác định";
            }
            catch
            {
                return "Lỗi Driver";
            }
        }

        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return false;

            string e = expected.ToLower();
            string a = actual.ToLower();

            if (e.Contains("thành công") && a.Contains("thành công")) return true;

            if (e.Contains("lỗi") || e.Contains("trống"))
            {
                return !string.IsNullOrEmpty(a) && a != "không xác định";
            }

            return a.Contains(e) || e.Contains(a);
        }
    }
}