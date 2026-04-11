using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace KhachSanTest.Tests
{
    public class DeleteUserTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        DeleteUserPage deletePage;
        WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            loginPage = new LoginPage(driver);
            deletePage = new DeleteUserPage(driver);
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetDeleteUserTestCases))]
        public void Test_DeleteUser(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";
            string screenshotPath = "";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                actual = GetActual();
                string expected = GetExpected(tc);

                bool isMatch = string.IsNullOrEmpty(expected) || CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                if (!isMatch)
                {
                    screenshotPath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                Assert.That(isMatch, Is.True, $"❌ Sai TC: {tc.TestCaseId} | Expected: {expected} | Actual: {actual}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;
                screenshotPath = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, screenshotPath);
            }
        }

        // ================= STEP =================

        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            string action = step.Action.ToLower();

            if (action.Contains("username") && step.Data == "admin")
                loginPage.EnterUsername(step.Data);

            else if (action.Contains("password"))
                loginPage.EnterPassword(step.Data);

            else if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                wait.Until(d => !d.Url.Contains("Login"));
            }

            else if (action.Contains("vào trang"))
                deletePage.GoToUsers();

            else if (action.Contains("chọn tài khoản"))
                deletePage.SelectUser(step.Data);

            else if (action.Contains("nhấn nút xóa") || action.Contains("click xóa"))
                deletePage.ClickDelete();

            else if (action.Contains("xác nhận"))
                deletePage.ConfirmDelete();

            else if (action.Contains("hủy"))
                deletePage.CancelDelete();
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
                if (driver.Url.ToLower().Contains("/users") &&
                    !driver.Url.ToLower().Contains("/delete"))
                    return "Xóa thành công";

                var alert = driver.FindElements(By.ClassName("alert"));
                if (alert.Count > 0 && !string.IsNullOrEmpty(alert[0].Text))
                    return alert[0].Text;

                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0 && !string.IsNullOrEmpty(errors[0].Text))
                    return errors[0].Text;

                return "";
            }
            catch
            {
                return "";
            }
        }

        // ================= COMPARE =================

        private bool CompareResult(string expected, string actual)
        {
            string e = expected.ToLower();
            string a = actual.ToLower();

            if (e.Contains("thành công")) return a.Contains("thành công");
            if (e.Contains("hủy")) return a.Contains("hủy") || driver.Url.Contains("/Users");

            return a.Contains(e);
        }

        // ================= TEARDOWN =================

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