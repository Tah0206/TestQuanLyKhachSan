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
    /// Module Đặt Phòng — _BookingFormPartial.cshtml
    /// TC: DatPhong_01 → DatPhong_17
    ///
    /// Tài khoản: nguenne2078 / Linh121273
    /// Luồng cố định mỗi TC:
    ///   1. Mở http://localhost:58609/ → trang đăng nhập
    ///   2. Nhập Username=nguenne2078, Password=Linh121273 → Đăng nhập
    ///   3. Redirect vào /Home/TrangChuNguoiDung
    ///   4. Nhấn nút "ĐẶT PHÒNG NGAY" → hiện form id="bookingForm" (_BookingFormPartial)
    ///   5. Thực hiện steps form:
    ///      - name=RoomId        (dropdown, required)
    ///      - id=checkIn         (input[type=date], min=today, required)
    ///      - id=checkOut        (input[type=date], min=today, required)
    ///      - name=NumberOfPeople(input[type=number], min=1, required)
    ///      - name=Note          (textarea, không bắt buộc)
    ///   6. Nhấn button.btn-dark[type=submit] "Đặt phòng ngay"
    ///   JS: checkIn lt today → alert | checkOut le checkIn → alert
    ///   Lỗi: .invalid-feedback (HTML5 required) hoặc .text-danger (server)
    /// </summary>
    [TestFixture]
    public class DatPhongTests
    {
        IWebDriver driver;
        HNguyenPage page;

        private const string BASE_URL  = "http://localhost:58609";
        private const string USERNAME  = "nguenne2078";
        private const string PASSWORD  = "Linh121273";
        private const string SHEET     = "TC HNguyên";

        // ============================================================
        // Hàm chạy chung: khởi tạo → login → nhấn ĐẶT PHÒNG NGAY → steps → compare
        // ============================================================
        private void RunTest(ExcelDataProvider.TestCase tc)
        {
            InitDriver();
            string actual = ""; string status = "Passed"; string img = "";
            try
            {
                // Bước 1-4: login và navigate đến form
                LoginAndOpenBookingForm();

                // Bước 5+: thực hiện steps từ Excel (bỏ qua steps login đã làm ở trên)
                foreach (var step in tc.Steps)
                    ExecuteStep(step);

                // Bắt JS alert nếu có (validation ngày)
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
            new object[] { "DatPhong_01" })]
        public void DatPhong_01_HopLe(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_02" })]
        public void DatPhong_02_KhongChonPhong(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_03" })]
        public void DatPhong_03_TrongCheckIn(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_04" })]
        public void DatPhong_04_TrongCheckOut(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_05" })]
        public void DatPhong_05_CheckInQuaKhu(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_06" })]
        public void DatPhong_06_CheckOutBangCheckIn(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_07" })]
        public void DatPhong_07_CheckOutNhoHonCheckIn(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_08" })]
        public void DatPhong_08_SoNguoiBang0(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_09" })]
        public void DatPhong_09_SoNguoiAm(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_10" })]
        public void DatPhong_10_TrongSoNguoi(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_11" })]
        public void DatPhong_11_KhongNhapNote(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_12" })]
        public void DatPhong_12_CheckInHomNay(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_13" })]
        public void DatPhong_13_SoNguoi1(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_14" })]
        public void DatPhong_14_GhiChuTiengViet(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_15" })]
        public void DatPhong_15_SoNguoiLon(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_16" })]
        public void DatPhong_16_DataKhacHopLe(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DatPhong_17" })]
        public void DatPhong_17_KiemTraRedirectBooking(ExcelDataProvider.TestCase tc) => RunTest(tc);

        // ============================================================
        // LOGIN + NAVIGATE ĐẾN FORM _BookingFormPartial
        // ============================================================
        private void LoginAndOpenBookingForm()
        {
            // 1. Vào trang đăng nhập
            driver.Navigate().GoToUrl($"{BASE_URL}/");

            // 2-3. Nhập thông tin đăng nhập
            page.EnterUsername(USERNAME);
            page.EnterPassword(PASSWORD);

            // 4. Nhấn Đăng nhập → redirect TrangChuNguoiDung
            page.ClickDangNhap();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url.Contains("TrangChuNguoiDung"));

            // 5. Nhấn nút "ĐẶT PHÒNG NGAY" → form bookingForm xuất hiện
            var btnDatPhongNgay = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//a[contains(translate(normalize-space(.), 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'), 'ĐẶT PHÒNG NGAY')] | //button[contains(translate(normalize-space(.), 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'), 'ĐẶT PHÒNG NGAY')]")));
            btnDatPhongNgay.Click();

            // Chờ form bookingForm hiển thị
            new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(ExpectedConditions.ElementIsVisible(By.Id("bookingForm")));
        }

        // ============================================================
        // EXECUTE STEP (bỏ qua steps login/navigate đã xử lý trước)
        // ============================================================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;
            string action = step.Action.ToLower();
            string data   = step.Data ?? "";

            // Bỏ qua steps login/navigate — đã xử lý trong LoginAndOpenBookingForm()
            if (action.Contains("vào trang") ||
                action.Contains("nhập username") ||
                action.Contains("nhập password") ||
                action.Contains("nhấn đăng nhập") ||
                action.Contains("đặt phòng ngay") && action.Contains("nhấn nút"))
                return;

            // ---- Form _BookingFormPartial ----

            // Chọn Phòng (name=RoomId)
            if (action.Contains("chọn phòng") || action.Contains("roomid"))
            {
                if (!string.IsNullOrEmpty(data) && data != "\"\"")
                    page.ChonRoomFast(data);
                // nếu data rỗng → không chọn (để test required)
                return;
            }

            // Ngày nhận phòng (id=checkIn)
            if (action.Contains("ngày nhận phòng") && (action.Contains("id=checkin") || action.Contains("(id=checkin")))
            {
                if (data == "[today]")
                    data = DateTime.Now.ToString("yyyy-MM-dd");
                else if (data.Contains("[today+"))
                {
                    int days = int.Parse(data.Replace("[today+", "").Replace("]", ""));
                    data = DateTime.Now.AddDays(days).ToString("yyyy-MM-dd");
                }
                page.NhapCheckInFast(data == "\"\"" ? "" : data);
                return;
            }

            // Ngày trả phòng (id=checkOut)
            if (action.Contains("ngày trả phòng") && (action.Contains("id=checkout") || action.Contains("(id=checkout")))
            {
                if (data.Contains("[today+"))
                {
                    int days = int.Parse(data.Replace("[today+", "").Replace("]", ""));
                    data = DateTime.Now.AddDays(days).ToString("yyyy-MM-dd");
                }
                page.NhapCheckOutFast(data == "\"\"" ? "" : data);
                return;
            }

            // Số người (name=NumberOfPeople)
            if (action.Contains("số người"))
            {
                page.NhapSoNguoiFast(data == "\"\"" ? "" : data);
                return;
            }

            // Ghi chú (name=Note)
            if (action.Contains("ghi chú") || action.Contains("note"))
            {
                page.NhapNote(data == "\"\"" ? "" : data);
                return;
            }

            // Nhấn "Đặt phòng ngay" (button.btn-dark[type=submit] trong #bookingForm)
            if (action.Contains("đặt phòng ngay") || action.Contains("button.btn-dark"))
            {
                page.NhanDatPhongNgay();
                Thread.Sleep(1000);
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

            // Đặt phòng thành công → redirect sang Booking
            if (e.Contains("thành công") || e.Contains("booking"))
                return a.Contains("thành công") || a.Contains("booking");

            // JS Alert quá khứ
            if (e.Contains("quá khứ") || e.Contains("past"))
                return a.Contains("quá khứ") || a.Contains("js alert");

            // JS Alert ngày trả > ngày nhận
            if (e.Contains("lớn hơn ngày nhận"))
                return a.Contains("lớn hơn") || a.Contains("js alert");

            // .invalid-feedback
            if (e.Contains("vui lòng chọn phòng"))
                return a.Contains("vui lòng chọn phòng") || a.Contains("invalid");

            if (e.Contains("ngày nhận phòng phải từ hôm nay"))
                return a.Contains("hôm nay") || a.Contains("invalid");

            if (e.Contains("ngày trả phải lớn hơn"))
                return a.Contains("lớn hơn") || a.Contains("invalid");

            if (e.Contains("vui lòng nhập số người"))
                return a.Contains("số người") || a.Contains("invalid") || a.Contains("ít nhất");

            // Lỗi chung
            if (e.Contains("lỗi") || e.Contains("invalid") || e.Contains("text-danger"))
                return a.Contains("lỗi") || a.Contains("invalid") || a.Contains("không") || a.Contains("vui lòng");

            return a.Contains(e);
        }

        // ============================================================
        // HELPERS
        // ============================================================
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
