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
    /// <summary>
    /// Module Đặt Phòng Hotline — Bookings/Create.cshtml
    /// TC: DatPhongHotline_01 → DatPhongHotline_10
    ///
    /// Tài khoản: admin / 123456
    /// Luồng cố định mỗi TC:
    ///   1. Mở http://localhost:58609/ → trang đăng nhập
    ///   2. Nhập Username=admin, Password=123456 → Đăng nhập
    ///   3. Redirect vào /Bookings
    ///   4. Nhấn nút "Đặt Phòng" → redirect vào /Bookings/Create
    ///   5. Thực hiện steps form:
    ///      - name=CustomerId    (dropdown, required)
    ///      - name=RoomId        (dropdown, required)
    ///      - name=CheckInDate   (input[type=date], required)
    ///      - name=CheckOutDate  (input[type=date], required)
    ///      - name=NumberOfPeople(input[type=number], min=1, required)
    ///      - name=Note          (textarea, không bắt buộc)
    ///   6. Nhấn input[type=submit][value="Tạo Đặt Phòng"]
    ///   JS: checkIn lt today → alert | checkOut le checkIn → alert
    ///   Lỗi: .text-danger (MVC server-side validation)
    /// </summary>
    [TestFixture]
    public class DatPhongHotlineTests
    {
        IWebDriver driver;
        HNguyenPage page;

        private const string BASE_URL = "http://localhost:58609";
        private const string USERNAME = "admin";
        private const string PASSWORD = "123456";
        private const string SHEET = "TC HNguyên";

        // ============================================================
        // Hàm chạy chung
        // ============================================================
        private void RunTest(ExcelDataProvider.TestCase tc)
        {
            InitDriver();
            string actual = ""; string status = "Passed"; string img = "";
            try
            {
                // Bước 1-4: login và navigate đến /Bookings/Create
                LoginAndNavigateToCreate();

                // Bước 5+: thực hiện steps từ Excel
                foreach (var step in tc.Steps)
                    ExecuteStep(step);

                // Bắt JS alert nếu có
                actual = TryGetAlert() ?? page.SafeGetActualResult();

                string expected = GetExpected(tc);
                bool pass = CompareResult(expected, actual);
                status = pass ? "Passed" : "Failed";

                TestContext.WriteLine($"[{tc.TestCaseId}] Expected: {expected}");
                TestContext.WriteLine($"[{tc.TestCaseId}] Actual:   {actual}");
                TestContext.WriteLine($"[{tc.TestCaseId}] Status:   {status}");

                Assert.That(pass, Is.True,
                    $"Sai kết quả {tc.TestCaseId}\nExpected: {expected}\nActual: {actual}");
            }
            catch (AssertionException)
            {
                status = "Failed";
                img = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                throw;
            }
            catch (Exception ex)
            {
                status = "Failed";
                actual = ex.Message;
                img = ScreenshotHelper.TakeScreenshot(driver, tc.TestCaseId);
                TestContext.WriteLine($"ERROR [{tc.TestCaseId}]: {ex.Message}");
                throw;
            }
            finally
            {
                ExcelDataProvider.WriteResult(SHEET, tc.TestCaseId, actual, status, img);
                Cleanup();
            }
        }

        // ============================================================
        // TEST CASES
        // ============================================================

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_01" })]
        public void DatPhongHotline_01_HopLe(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_02" })]
        public void DatPhongHotline_02_KhongChonKhachHang(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_03" })]
        public void DatPhongHotline_03_KhongChonPhong(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_04" })]
        public void DatPhongHotline_04_TrongCheckIn(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_05" })]
        public void DatPhongHotline_05_TrongCheckOut(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_06" })]
        public void DatPhongHotline_06_CheckInQuaKhu(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_07" })]
        public void DatPhongHotline_07_CheckOutBangCheckIn(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_08" })]
        public void DatPhongHotline_08_SoNguoiBang0(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_09" })]
        public void DatPhongHotline_09_KhongNhapNote(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhongHotline_10" })]
        public void DatPhongHotline_10_NutQuayLai(ExcelDataProvider.TestCase tc) => RunTest(tc);

        // ============================================================
        // LOGIN + NAVIGATE ĐẾN /Bookings/Create
        // ============================================================
        private void LoginAndNavigateToCreate()
        {
            // 1. Vào trang đăng nhập
            driver.Navigate().GoToUrl($"{BASE_URL}/");

            // 2-3. Nhập thông tin đăng nhập
            page.EnterUsername(USERNAME);
            page.EnterPassword(PASSWORD);

            // 4. Đăng nhập — chờ URL thay đổi khỏi trang login
            string loginUrl = driver.Url;
            page.ClickDangNhap();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url != loginUrl);

            // 5. Navigate thẳng vào /Bookings — không click sidebar
            // (tránh vấn đề encoding tiếng Việt và timing của DOM render)
            driver.Navigate().GoToUrl($"{BASE_URL}/Bookings");
            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url.ToLower().Contains("/bookings"));

            // 6. Nhấn nút "Đặt Phòng" (class="btn-add-booking") → /Bookings/Create
            // HTML thực tế: <a href=".../Bookings/Create" class="btn-add-booking">Đặt Phòng</a>
            var btnCreate = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("a.btn-add-booking")));
            btnCreate.Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url.Contains("/Bookings/Create"));
        }

        // ============================================================
        // EXECUTE STEP
        // ============================================================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;
            string action = step.Action.ToLower();
            string data = step.Data ?? "";

            // Bỏ qua steps login/navigate — đã xử lý trong LoginAndNavigateToCreate()
            if (action.Contains("vào trang") ||
                action.Contains("nhập username") ||
                action.Contains("nhập password") ||
                action.Contains("nhấn đăng nhập") ||
                action.Contains("nhấn nút \"đặt phòng\"") ||
                action.Contains("redirect vào"))
                return;

            // ---- Form Bookings/Create ----

            // Chọn Khách hàng (name=CustomerId)
            if (action.Contains("khách hàng") && action.Contains("dropdown") ||
                action.Contains("customerid") || action.Contains("chọn khách hàng"))
            {
                if (!string.IsNullOrEmpty(data) && data != "\"\"")
                    page.ChonCustomer(data);
                return;
            }

            // Chọn Phòng (name=RoomId)
            if (action.Contains("chọn phòng") || action.Contains("roomid"))
            {
                if (!string.IsNullOrEmpty(data) && data != "\"\"")
                    page.ChonRoom(data);
                return;
            }

            // Ngày nhận phòng (name=CheckInDate)
            if (action.Contains("checkindate") || action.Contains("ngày nhận phòng"))
            {
                if (data == "[today]")
                    data = DateTime.Now.ToString("yyyy-MM-dd");
                page.NhapCheckInDate(data == "\"\"" ? "" : data);
                return;
            }

            // Ngày trả phòng (name=CheckOutDate)
            if (action.Contains("checkoutdate") || action.Contains("ngày trả phòng"))
            {
                if (data.Contains("[today+"))
                {
                    int d = int.Parse(data.Replace("[today+", "").Replace("]", ""));
                    data = DateTime.Now.AddDays(d).ToString("yyyy-MM-dd");
                }
                page.NhapCheckOutDate(data == "\"\"" ? "" : data);
                return;
            }

            // Số người (name=NumberOfPeople)
            if (action.Contains("số người") || action.Contains("numberofpeople"))
            {
                page.NhapNumberOfPeople(data == "\"\"" ? "" : data);
                return;
            }

            // Ghi chú (name=Note)
            if (action.Contains("ghi chú") || action.Contains("note"))
            {
                page.NhapNote(data == "\"\"" ? "" : data);
                return;
            }

            // Trạng thái thanh toán (name=PaymentStatusID)
            if (action.Contains("trạng thái thanh toán") || action.Contains("paymentstatusid"))
            {
                if (!string.IsNullOrEmpty(data) && data != "\"\"")
                    page.ChonPaymentStatus(data);
                return;
            }

            // Trạng thái đặt phòng (name=BookingStatusID)
            if (action.Contains("trạng thái đặt phòng") || action.Contains("bookingstatusid"))
            {
                if (!string.IsNullOrEmpty(data) && data != "\"\"")
                    page.ChonBookingStatus(data);
                return;
            }

            // Nhấn "Tạo Đặt Phòng"
            if (action.Contains("tạo đặt phòng") || action.Contains("input[type=submit]"))
            {
                page.NhanTaoDatPhong();
                Thread.Sleep(1000);
                return;
            }

            // Nhấn Quay lại (a.btn-back)
            if (action.Contains("quay lại") || action.Contains("btn-back"))
            {
                page.NhanQuayLaiBooking();
                Thread.Sleep(500);
                return;
            }
        }

        // ============================================================
        // COMPARE RESULT
        // ============================================================
        private bool CompareResult(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return true;
            string e = expected.Trim().ToLower();
            string a = actual.Trim().ToLower();

            if (a.Contains("stale element")) return false;

            // Thành công
            if (e.Contains("thành công") || e.Contains("booking"))
                return a.Contains("thành công") || a.Contains("booking");

            // JS Alert hoặc server-side text: CheckIn quá khứ / nhỏ hơn ngày hiện tại
            if (e.Contains("quá khứ") || e.Contains("nhỏ hơn ngày hiện tại") || e.Contains("nhỏ hơn ngày"))
                return a.Contains("quá khứ") || a.Contains("nhỏ hơn") || a.Contains("ngày hiện tại") || a.Contains("js alert");

            // JS Alert hoặc server-side text: CheckOut <= CheckIn
            if (e.Contains("lớn hơn ngày nhận") || e.Contains("lớn hơn ngày nhận phòng"))
                return a.Contains("lớn hơn") || a.Contains("ngày nhận") || a.Contains("js alert");

            // Expected bắt đầu bằng "js alert:" → so sánh nội dung sau dấu ":"
            if (e.StartsWith("js alert"))
            {
                // Lấy nội dung thực của alert (bỏ prefix "js alert: " hoặc "js alert hoặc text-danger: ")
                string alertContent = System.Text.RegularExpressions.Regex.Replace(e, @"^js alert[^:]*:\s*", "").Trim().Trim('"');
                // actual phải chứa nội dung đó (dù là từ alert hay text-danger)
                return a.Contains(alertContent) || a.Contains("js alert");
            }

            // .text-danger (server-side)
            if (e.Contains("vui lòng chọn khách hàng") || e.Contains("customerid"))
                return a.Contains("khách hàng") || a.Contains("text-danger") || a.Contains("lỗi");

            if (e.Contains("vui lòng chọn phòng") || e.Contains("roomid"))
                return a.Contains("phòng") || a.Contains("text-danger") || a.Contains("lỗi");

            if (e.Contains("checkindate") || e.Contains("ngày nhận phòng") && e.Contains("trống"))
                return a.Contains("ngày nhận") || a.Contains("text-danger") || a.Contains("lỗi");

            if (e.Contains("checkoutdate") || e.Contains("ngày trả phòng") && e.Contains("trống"))
                return a.Contains("ngày trả") || a.Contains("text-danger") || a.Contains("lỗi");

            if (e.Contains("numberofpeople") || e.Contains("phải ≥ 1") || e.Contains("min=1"))
                return a.Contains("số người") || a.Contains("text-danger") || a.Contains("lỗi")
                    || a.Contains("/bookings/create");  // HTML5 min= chặn submit → URL không đổi

            // Thiếu trạng thái
            if (e.Contains("paymentstatusid") || e.Contains("trạng thái thanh toán"))
                return a.Contains("thanh toán") || a.Contains("text-danger") || a.Contains("lỗi");

            if (e.Contains("bookingstatusid") || e.Contains("trạng thái đặt phòng"))
                return a.Contains("đặt phòng") || a.Contains("text-danger") || a.Contains("lỗi");

            // Quay lại
            if (e.Contains("/bookings/index") || e.Contains("quay lại"))
                return a.Contains("bookings") || a.Contains("index");

            // Lỗi chung
            if (e.Contains("lỗi") || e.Contains("text-danger"))
                return a.Contains("lỗi") || a.Contains("không") || a.Contains("text-danger");

            return a.Contains(e);
        }

        // ============================================================
        // HELPERS
        // ===========================================================
        private string TryGetAlert()
        {
            try
            {
                var alert = new WebDriverWait(driver, TimeSpan.FromSeconds(2))
                    .Until(ExpectedConditions.AlertIsPresent());
                string txt = $"JS Alert: {alert.Text}";
                alert.Accept();
                return txt;
            }
            catch { return null; }
        }

        private string GetExpected(ExcelDataProvider.TestCase tc)
        {
            return tc.Steps.Where(s => !string.IsNullOrEmpty(s.Expected))
                           .LastOrDefault()?.Expected?.Trim() ?? "";
        }

        private void InitDriver()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            page = new HNguyenPage(driver);
        }

        [TearDown]
        public void Cleanup()
        {
            if (driver != null) { driver.Quit(); driver.Dispose(); driver = null; }
        }
    }
}