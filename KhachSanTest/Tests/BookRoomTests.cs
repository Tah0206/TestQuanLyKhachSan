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
    public class BookRoomTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        BookRoomPage bookRoomPage;

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetBookRoomTestCases))]
        public void Test_BookRoom(ExcelDataProvider.TestCase tc)
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            bookRoomPage = new BookRoomPage(driver);

            string actual = "";
            string status = "Passed";
            string image = "";

            TestContext.WriteLine($"\n===== RUN TC: {tc.TestCaseId} =====");

            try
            {
                foreach (var step in tc.Steps)
                {
                    ExecuteStep(step);
                }

                string expected = GetExpected(tc);
                string actualResult = GetActual();

                actual = actualResult;

                TestContext.WriteLine($"Expected: {expected}");
                TestContext.WriteLine($"Actual: {actualResult}");

                bool isMatch = CompareResult(expected, actualResult);
                status = isMatch ? "Passed" : "Failed";

                Assert.That(isMatch, Is.True, $"Sai kết quả tại {tc.TestCaseId}");
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
            }
        }

        // ================= STEP =================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            string action = step.Action.ToLower().Trim();
            string data = step.Data ?? "";

            TestContext.WriteLine("STEP: " + action + " | DATA: " + data);

            // ===== LOGIN =====
            if (action.Contains("username"))
            {
                loginPage.EnterUsername(data);
            }
            else if (action.Contains("password"))
            {
                loginPage.EnterPassword(data);
            }
            else if (action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
                Thread.Sleep(1000);
            }

            // ===== NAVIGATE =====
            else if (action.Contains("vào trang đặt phòng"))
            {
                driver.Navigate().GoToUrl("http://localhost:58609/Bookings");
                Thread.Sleep(1000);
            }

            // ===== CLICK CREATE FORM =====
            else if (action.Contains("nhấn tạo đặt phòng") && string.IsNullOrEmpty(data))
            {
                bookRoomPage.ClickAddBooking();
                Thread.Sleep(1000);
            }

            // ===== DROPDOWN =====
            else if (action.Contains("chọn khách hàng"))
            {
                bookRoomPage.SelectCustomer(data);
                Thread.Sleep(500); // 🔥 tránh bị reset form
            }
            else if (action.Contains("chọn phòng"))
            {
                bookRoomPage.SelectRoom(data);
                Thread.Sleep(500);
            }
            else if (action.Contains("chọn trạng thái thanh toán"))
            {
                bookRoomPage.SelectPaymentStatus(data);
            }
            else if (action.Contains("chọn trạng thái phòng"))
            {
                bookRoomPage.SelectRoomStatus(data);
            }

            // ===== INPUT =====
            else if (action.Contains("ngày nhận"))
            {
                bookRoomPage.EnterCheckIn(data);
            }
            else if (action.Contains("ngày trả"))
            {
                bookRoomPage.EnterCheckOut(data);
            }
            else if (action.Contains("số người") && !action.Contains("không"))
            {
                bookRoomPage.EnterPeople(data);
            }

            // ===== EMPTY CASE =====
            else if (action.Contains("không chọn") || action.Contains("không nhập"))
            {
                // bỏ qua đúng testcase validation
            }

            // ===== SUBMIT =====
            else if (action.Contains("nhấn tạo đặt phòng") && action.Contains("tạo"))
            {
                bookRoomPage.ClickCreate();
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
                string url = driver.Url.ToLower();

                // SUCCESS
                if (url.Contains("bookings"))
                {
                    return "Tạo đặt phòng thành công";
                }

                // ERROR
                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0 && !string.IsNullOrEmpty(errors[0].Text))
                {
                    return errors[0].Text.Trim();
                }

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

            if (e.Contains("thành công"))
                return a.Contains("thành công");

            if (e.Contains("lỗi"))
                return a.Contains("lỗi") || a.Contains("error");

            if (e.Contains("yêu cầu"))
                return a.Contains("yêu cầu") || a.Contains("required");

            if (e.Contains("không hợp lệ"))
                return a.Contains("không hợp lệ") || a.Contains("invalid");

            return a.Contains(e);
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