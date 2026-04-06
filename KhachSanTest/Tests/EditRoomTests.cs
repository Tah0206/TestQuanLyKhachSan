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
    public class EditRoomTests
    {
        IWebDriver driver;
        LoginPage loginPage;
        EditRoomPage editRoomPage;

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetEditRoomTestCases))]
        public void Test_EditRoom(ExcelDataProvider.TestCase tc)
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost:58609/");

            loginPage = new LoginPage(driver);
            editRoomPage = new EditRoomPage(driver);

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

            // VÀO BOOKING
            else if (action.Contains("quản lý đặt phòng"))
            {
                driver.Navigate().GoToUrl("http://localhost:58609/Bookings");
                Thread.Sleep(1000);
            }

            // CHỌN PHIẾU
            else if (action.Contains("chọn phiếu"))
            {
                driver.FindElements(By.LinkText("Sửa"))[0].Click();
                Thread.Sleep(1000);
            }

            // FORM
            else if (action.Contains("chọn phòng"))
                editRoomPage.SelectRoom(data);

            else if (action.Contains("ngày nhận"))
                editRoomPage.EnterCheckIn(data);

            else if (action.Contains("ngày trả"))
                editRoomPage.EnterCheckOut(data);

            else if (action.Contains("số người"))
                editRoomPage.EnterPeople(data);

            else if (action.Contains("thanh toán"))
                editRoomPage.SelectPayment(data);

            else if (action.Contains("trạng thái"))
                editRoomPage.SelectStatus(data);

            // SAVE
            else if (action.Contains("lưu"))
            {
                editRoomPage.ClickSave();
                Thread.Sleep(1500);
            }

            else if (action.Contains("hủy"))
            {
                editRoomPage.ClickCancel();
                Thread.Sleep(1000);
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
                if (driver.Url.Contains("Bookings"))
                    return "Cập nhật thành công";

                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0)
                    return errors[0].Text;

                return "";
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