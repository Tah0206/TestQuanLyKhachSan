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
    public class DeleteRoomTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        DeleteRoomPage deleteRoomPage;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            deleteRoomPage = new DeleteRoomPage(driver);
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

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetDeleteRoomTestCases))]
        public void Test_DeleteRoom(ExcelDataProvider.TestCase tc)
        {
            string actual = "";
            string status = "Passed";
            string image = "";

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = tc.Steps.LastOrDefault(s => !string.IsNullOrEmpty(s.Expected))?.Expected ?? "";
                actual = GetActualResult();

                bool isMatch = CompareResult(expected, actual);
                status = isMatch ? "Passed" : "Failed";

                // Chụp ảnh khi Failed
                if (status == "Failed")
                {
                    image = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                Assert.That(isMatch, Is.True, $"Expected: {expected} | Actual: {actual}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = "Lỗi: " + ex.Message;

                // Chụp ảnh khi exception (nếu chưa chụp)
                if (string.IsNullOrEmpty(image))
                {
                    image = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, image);
            }
        }

        // ===== STEP =====
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;

            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // LOGIN
            if (action.Contains("username"))
            {
                loginPage.EnterUsername(data);
                return;
            }

            if (action.Contains("password"))
            {
                loginPage.EnterPassword(data);
                return;
            }

            if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1000);
                return;
            }

            // NAVIGATION
            if (action.Contains("quản lý đặt phòng"))
            {
                deleteRoomPage.GoToPage();
                return;
            }

            // DELETE FLOW
            // "chọn phiếu" → click nút Xóa trên danh sách để vào trang Delete
            if (action.Contains("chọn phiếu"))
            {
                deleteRoomPage.ClickDelete();
                return;
            }

            if (action.Contains("nhấn nút xóa"))
            {
                deleteRoomPage.ClickDelete();
                return;
            }

            // "xác nhận xóa" → click nút submit Xóa trên trang Delete.cshtml
            if (action.Contains("xác nhận xóa"))
            {
                deleteRoomPage.ConfirmDelete();
                return;
            }

            // CASE MẤT MẠNG
            if (action.Contains("mất kết nối"))
            {
                driver.Navigate().GoToUrl("chrome://offline/");
                return;
            }
        }

        // ===== RESULT =====
        private string GetActualResult()
        {
            try
            {
                Thread.Sleep(2000);

                if (deleteRoomPage.IsDeletedSuccess())
                    return "Đã hủy";

                var error = deleteRoomPage.GetError();
                if (!string.IsNullOrEmpty(error))
                    return error;

                return "Không xác định";
            }
            catch
            {
                return "Lỗi hệ thống";
            }
        }

        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return false;

            string e = expected.ToLower();
            string a = actual.ToLower();

            // SUCCESS
            if (e.Contains("đã hủy") && a.Contains("hủy")) return true;

            // ERROR CASE
            if (e.Contains("lỗi"))
            {
                return a.Contains("lỗi") || a.Contains("không xác định");
            }

            return a.Contains(e) || e.Contains(a);
        }
    }
}