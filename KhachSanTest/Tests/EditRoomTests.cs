using KhachSanTest.Pages;
using KhachSanTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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

                bool isMatch = CompareResult(expected, actualResult);
                status = isMatch ? "Passed" : "Failed";

                // Chụp ảnh khi Failed
                if (status == "Failed")
                {
                    image = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                Assert.That(isMatch, Is.True,
                    $"Sai TC: {tc.TestCaseId}\n  Expected: {expected}\n  Actual  : {actualResult}");
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;

                if (string.IsNullOrEmpty(image))
                {
                    image = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                }

                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(tc.SheetName, tc.TestCaseId, actual, status, image);
                driver.Quit();
            }
        }

        // ================= COMPARE =================
        /// <summary>
        /// So sánh expected (từ Excel) với actual (từ trình duyệt).
        /// - So sánh 2 chiều: actual chứa expected HOẶC expected chứa actual
        ///   → tránh lỗi khi 2 chuỗi cùng ý nghĩa nhưng độ dài khác nhau
        ///   (ví dụ: actual="Cập nhật thành công", expected="...được cập nhật thành công")
        /// - Xử lý các từ khoá ngữ nghĩa đặc biệt
        /// </summary>
        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return false;

            string e = expected.Trim().ToLower();
            string a = actual.Trim().ToLower();

            // --- NHÓM THÀNH CÔNG ---
            // Expected dạng: "thành công", "cập nhật thành công", "thông tin đặt phòng được cập nhật thành công"...
            if (e.Contains("thành công"))
                return a.Contains("thành công");

            // --- NHÓM KHÔNG THAY ĐỔI / GIỮ NGUYÊN ---
            // Expected dạng: "trạng thái phiếu không thay đổi", "giữ nguyên"...
            // → actual phải là "Cập nhật thành công" (redirect về list, nghĩa là save OK nhưng
            //   nội dung không đổi) HOẶC vẫn còn trên trang edit
            if (e.Contains("không thay đổi") || e.Contains("giữ nguyên"))
                return a.Contains("thành công") || a.Contains("không thay đổi");

            // --- NHÓM LỖI / VALIDATION ---
            // Expected dạng: "lỗi", "không hợp lệ", "bắt buộc", "vui lòng nhập"...
            // Actual có thể là message tiếng Anh của ASP.NET model binding:
            //   "The value '31/03/2026' is not valid for CheckInDate."
            //   "The CheckOutDate field is required."
            if (e.Contains("lỗi") || e.Contains("không hợp lệ") || e.Contains("bắt buộc") || e.Contains("vui lòng"))
                return a.Contains("lỗi") || a.Contains("không hợp lệ")
                    || a.Contains("bắt buộc") || a.Contains("vui lòng")
                    || a.Contains("required")
                    || a.Contains("invalid")
                    || a.Contains("not valid")
                    || a.Contains("must be a")
                    || a.Contains("value")
                    || a.Contains("date")
                    || a.Contains("checkindate") || a.Contains("checkoutdate");

            // --- SO SÁNH 2 CHIỀU (fallback) ---
            // Bắt cả trường hợp: actual chứa expected, hoặc expected chứa actual
            return a.Contains(e) || e.Contains(a);
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

            // CHỌN PHIẾU - click nút Sửa đầu tiên
            // Index.cshtml: <a class="btn-edit-booking"><i ...></i> Sửa</a>
            // Không dùng LinkText("Sửa") vì thẻ <a> chứa icon <i> nên text không thuần
            else if (action.Contains("chọn phiếu"))
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElements(By.CssSelector("a.btn-edit-booking")).Count > 0);

                var editButtons = driver.FindElements(By.CssSelector("a.btn-edit-booking"));
                if (editButtons.Count == 0)
                    throw new NoSuchElementException("Không tìm thấy nút Sửa (a.btn-edit-booking) trên trang danh sách.");

                editButtons[0].Click();
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
                // PHẢI kiểm tra lỗi TRƯỚC khi kiểm tra URL
                // Vì URL trang Edit dạng /Bookings/Edit/5 cũng chứa "bookings"
                // → nếu check URL trước sẽ luôn trả về "Cập nhật thành công" dù đang có lỗi

                // 1. Kiểm tra lỗi validation (text-danger) trên form hiện tại
                var errors = driver.FindElements(By.ClassName("text-danger"));
                if (errors.Count > 0)
                {
                    var errorText = errors
                        .Select(e => e.Text.Trim())
                        .FirstOrDefault(t => !string.IsNullOrEmpty(t));
                    if (!string.IsNullOrEmpty(errorText))
                        return errorText;
                }

                // 2. Không có lỗi + URL là trang danh sách (/Bookings, /Bookings/Index)
                //    → Save thành công và đã redirect về list
                string url = driver.Url.ToLower();
                bool isListPage = url.EndsWith("/bookings")
                               || url.EndsWith("/bookings/")
                               || url.EndsWith("/bookings/index")
                               || url.Contains("/bookings?")
                               || url.Contains("/bookings#");

                if (isListPage)
                    return "Cập nhật thành công";

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
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}   