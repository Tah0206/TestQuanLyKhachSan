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
    /// Module Đổi Thông Tin — Customers/Edit.cshtml
    /// TC: DoiThongTin_01 → DoiThongTin_15
    ///
    /// Luồng cố định mỗi TC:
    ///   1. Mở http://localhost:58609/ → trang đăng nhập
    ///   2. Đăng nhập admin / 123456
    ///   3. Redirect xong → navigate vào /Customers
    ///   4. Nhấn nút Edit của khách hàng (ID=1) → /Customers/Edit/{id}
    ///   5. Thực hiện steps form từ Excel
    ///   6. Ghi kết quả vào Excel
    /// </summary>
    [TestFixture]
    public class DoiThongTinTests
    {
        IWebDriver driver;
        HNguyenPage page;

        private const string BASE_URL         = "http://localhost:58609";
        private const string USERNAME         = "admin";
        private const string PASSWORD         = "123456";
        private const string SHEET            = "TC HNguyên";
        private const string EDIT_CUSTOMER_ID = "1";   // ID khách hàng dùng để test Edit

        // ============================================================
        // Hàm chạy chung cho tất cả TC
        // ============================================================
        private void RunTest(ExcelDataProvider.TestCase tc)
        {
            InitDriver();
            string actual = ""; string status = "Passed"; string img = "";
            try
            {
                // Bước 1-4: login → /Customers → /Customers/Edit/{id}
                LoginAndNavigateToEdit();

                // Bước 5: thực hiện steps từ Excel
                foreach (var step in tc.Steps)
                    ExecuteStep(step);

                actual = page.SafeGetActualResult();
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
        // TEST CASES — DoiThongTin_01 → DoiThongTin_15
        // ============================================================

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_01" })]
        public void DoiThongTin_01_HopLe(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_02" })]
        public void DoiThongTin_02_TrongFullName(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_03" })]
        public void DoiThongTin_03_TrongEmail(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_04" })]
        public void DoiThongTin_04_TrongPhone(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_05" })]
        public void DoiThongTin_05_TrongIdentityNumber(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_06" })]
        public void DoiThongTin_06_TrongAddress(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_07" })]
        public void DoiThongTin_07_TrongTatCaTruong(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_08" })]
        public void DoiThongTin_08_EmailSaiDinhDang(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_09" })]
        public void DoiThongTin_09_HoTen1KyTu(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_10" })]
        public void DoiThongTin_10_ChiSuaAddress(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_11" })]
        public void DoiThongTin_11_ChiSuaPhone(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_12" })]
        public void DoiThongTin_12_ChiSuaEmail(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_13" })]
        public void DoiThongTin_13_AddressKyTuDacBiet(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_14" })]
        public void DoiThongTin_14_NutQuayLai(ExcelDataProvider.TestCase tc) => RunTest(tc);

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCasesByPrefix),
            new object[] { "DoiThongTin_15" })]
        public void DoiThongTin_15_DataKhacHopLe(ExcelDataProvider.TestCase tc) => RunTest(tc);

        // ============================================================
        // LOGIN + NAVIGATE: / → đăng nhập → /Customers → /Customers/Edit/{id}
        // ============================================================
        private void LoginAndNavigateToEdit()
        {
            // 1. Mở trang login
            driver.Navigate().GoToUrl($"{BASE_URL}/");

            // 2. Nhập thông tin đăng nhập
            page.EnterUsername(USERNAME);
            page.EnterPassword(PASSWORD);

            // 3. Nhấn Đăng nhập — chờ redirect khỏi trang login
            string loginUrl = driver.Url;
            page.ClickDangNhap();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url != loginUrl);

            // 4. Navigate vào /Customers
            driver.Navigate().GoToUrl($"{BASE_URL}/Customers");
            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url.ToLower().Contains("/customers"));

            // 5. Nhấn nút Edit của khách hàng cần test
            //    Thử selector chính xác theo ID trước, sau đó fallback
            IWebElement btnEdit = null;
            var selectors = new[]
            {
                $"a[href*='/Customers/Edit/{EDIT_CUSTOMER_ID}']",  // link chứa đúng ID
                "a.btn-edit",                                       // class phổ biến MVC scaffold
                "a.btn-edit-customer",
                "table tbody tr:first-child a[href*='Edit']",       // dòng đầu tiên trong bảng
                "a[href*='Customers/Edit']",                        // bất kỳ link Edit nào
            };

            foreach (var sel in selectors)
            {
                try
                {
                    btnEdit = new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                        .Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(sel)));
                    if (btnEdit != null) break;
                }
                catch { /* thử selector tiếp theo */ }
            }

            if (btnEdit != null)
            {
                btnEdit.Click();
                new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Url.ToLower().Contains("/customers/edit"));
            }
            else
            {
                // Fallback: navigate thẳng bằng ID nếu không tìm được nút trên trang
                driver.Navigate().GoToUrl($"{BASE_URL}/Customers/Edit/{EDIT_CUSTOMER_ID}");
                new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Url.ToLower().Contains("/customers/edit"));
            }
        }

        // ============================================================
        // EXECUTE STEP
        // ============================================================
        private void ExecuteStep(ExcelDataProvider.TestStep step)
        {
            if (step == null || string.IsNullOrEmpty(step.Action)) return;
            string action = step.Action.ToLower();
            string data   = step.Data ?? "";

            // Bỏ qua steps login/navigate — đã xử lý trong LoginAndNavigateToEdit()
            if (action.Contains("vào trang") ||
                action.Contains("nhập username") ||
                action.Contains("nhập password") ||
                action.Contains("nhấn đăng nhập") ||
                action.Contains("redirect vào") ||
                action.Contains("chỉnh sửa khách hàng") ||
                action.Contains("/customers/edit") ||
                action.Contains("/customers"))
                return;

            // ---- Form Customers/Edit ----

            if (action.Contains("họ và tên") || action.Contains("fullname") ||
                action.Contains("họ tên") || action.Contains("xóa trắng fullname"))
            {
                page.NhapFullName(data == "\"\"" ? "" : data);
            }
            else if (action.Contains("số điện thoại") || action.Contains("phone") ||
                     action.Contains("sđt") || action.Contains("xóa trắng phone"))
            {
                page.NhapPhone(data == "\"\"" ? "" : data);
            }
            else if (action.Contains("email"))
            {
                page.NhapEmail(data == "\"\"" ? "" : data);
            }
            else if (action.Contains("căn cước") || action.Contains("cccd") ||
                     action.Contains("identitynumber") || action.Contains("cmnd"))
            {
                page.NhapIdentityNumber(data == "\"\"" ? "" : data);
            }
            else if (action.Contains("địa chỉ") || action.Contains("address"))
            {
                page.NhapAddress(data == "\"\"" ? "" : data);
            }
            else if (action.Contains("xóa trắng tất cả") || action.Contains("xóa tất cả"))
            {
                page.NhapFullName("");
                page.NhapPhone("");
                page.NhapEmail("");
                page.NhapIdentityNumber("");
                page.NhapAddress("");
            }
            else if (action.Contains("lưu thay đổi") || action.Contains("nhấn lưu") ||
                     action.Contains("btn-save") || action.Contains("button.btn-save"))
            {
                page.NhanLuuThayDoi();
                Thread.Sleep(800);
            }
            else if (action.Contains("quay lại") || action.Contains("btn-cancel"))
            {
                page.NhanQuayLaiCustomer();
                Thread.Sleep(500);
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

            // Thành công → redirect về /Customers/Index
            if (e.Contains("thành công") || e.Contains("/customers/index") || e.Contains("lưu thành công"))
                return a.Contains("thành công") || a.Contains("/customers/index") || a.Contains("lưu thay đổi thành công");

            // Quay lại → cũng về /Customers/Index
            if (e.Contains("quay lại"))
                return a.Contains("/customers/index") || a.Contains("thành công");

            // Email sai định dạng — browser tooltip chặn submit → URL vẫn ở /Customers/Edit
            if (e.Contains("email") && (e.Contains("định dạng") || e.Contains("không hợp lệ") || e.Contains("sai")))
                return a.Contains("email") || a.Contains("lỗi") || a.Contains("/customers/edit");

            // Lỗi validation server-side (.text-danger)
            if (e.Contains("text-danger") || e.Contains("lỗi") ||
                e.Contains("không được trống") || e.Contains("bắt buộc") || e.Contains("required"))
                return a.Contains("lỗi") || a.Contains("không") || a.Contains("trống") || a.Contains("text-danger");

            return a.Contains(e);
        }

        // ============================================================
        // HELPERS
        // ============================================================
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
