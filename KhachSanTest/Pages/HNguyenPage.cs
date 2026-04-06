using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KhachSanTest.Pages
{
    /// <summary>
    /// Page Object Model — sheet HNguyên
    /// Dựa trên HTML thực tế từ Views/:
    ///   Auth/Login.cshtml          → Id("Username"), Id("Password"), button[type='submit']
    ///   Customers/Edit.cshtml      → name="FullName/Phone/Email/IdentityNumber/Address", button.btn-save
    ///   Bookings/Create.cshtml     → name="CustomerId/RoomId/CheckInDate/CheckOutDate/NumberOfPeople/Note"
    ///   _BookingFormPartial.cshtml → id="checkIn", id="checkOut", #bookingForm button[type=submit]
    ///   Payment/Pay.cshtml         → button.btn-pay (tại quầy), a.btn-momo, a.btn-qr, span.amount
    ///   Payment/GenerateBankQr.cshtml → .qr-amount, .qr-image img, a.btn-success, a.btn-secondary
    ///   Bookings/Index.cshtml      → a.btn-edit-booking[href*='Payment/Pay'], span.status-badge
    /// </summary>
    public class HNguyenPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public HNguyenPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        }

        // =============================================
        // LOCATORS — Login (Views/Auth/Login.cshtml)
        // Html.TextBoxFor(m => m.Username) → Id("Username")
        // Html.PasswordFor(m => m.Password) → Id("Password")
        // button[type='submit'] → nút "Đăng nhập"
        // .text-danger → lỗi validation
        // =============================================
        private By txtUsername = By.Id("Username");
        private By txtPassword = By.Id("Password");
        private By btnDangNhap = By.CssSelector("button[type='submit']");
        private By msgTextDanger = By.CssSelector(".text-danger");

        // =============================================
        // LOCATORS — Customers/Edit.cshtml
        // Html.EditorFor(model => model.FullName)       → name="FullName"
        // Html.EditorFor(model => model.Phone)          → name="Phone"
        // Html.EditorFor(model => model.Email)          → name="Email"
        // Html.EditorFor(model => model.IdentityNumber) → name="IdentityNumber"
        // Html.EditorFor(model => model.Address)        → name="Address"
        // button.btn-save  = "Lưu thay đổi"
        // a.btn-cancel     = "Quay lại"
        // =============================================
        private By txtFullName = By.Name("FullName");
        private By txtPhone = By.Name("Phone");
        private By txtEmail = By.Name("Email");
        private By txtIdentityNumber = By.Name("IdentityNumber");
        private By txtAddress = By.Name("Address");
        private By btnLuuThayDoi = By.CssSelector("button.btn-save");
        private By lnkQuayLaiCustomer = By.CssSelector("a.btn-cancel");

        // =============================================
        // LOCATORS — Bookings/Create.cshtml
        // Html.DropDownList("CustomerId") → name="CustomerId"
        // Html.DropDownList("RoomId")     → name="RoomId"
        // Html.EditorFor(CheckInDate)     → name="CheckInDate" type="date"
        // Html.EditorFor(CheckOutDate)    → name="CheckOutDate" type="date"
        // Html.EditorFor(NumberOfPeople)  → name="NumberOfPeople" min="1"
        // Html.TextAreaFor(Note)          → name="Note"
        // input[type=submit][value="Tạo Đặt Phòng"]
        // a.btn-back = "Quay lại Danh sách"
        // =============================================
        private By ddlCustomerId = By.Name("CustomerId");
        private By ddlRoomId = By.Name("RoomId");
        private By txtCheckInDate = By.Name("CheckInDate");
        private By txtCheckOutDate = By.Name("CheckOutDate");
        private By txtNumPeople = By.Name("NumberOfPeople");
        private By txtNote = By.Name("Note");
        private By ddlPaymentStatusID = By.Name("PaymentStatusID");
        private By ddlBookingStatusID = By.Name("BookingStatusID");
        private By btnTaoDatPhong = By.CssSelector("input[type='submit'][value='Tạo Đặt Phòng']");
        private By lnkQuayLaiBooking = By.CssSelector("a.btn-back");

        // =============================================
        // LOCATORS — _BookingFormPartial.cshtml (form nhanh trang chủ)
        // form id="bookingForm"
        // id="checkIn"  → CheckInDate (type=date)
        // id="checkOut" → CheckOutDate (type=date)
        // #bookingForm button[type=submit] = "Đặt phòng ngay"
        // .invalid-feedback → lỗi HTML5 required
        // =============================================
        private By txtCheckInFast = By.Id("checkIn");
        private By txtCheckOutFast = By.Id("checkOut");
        private By btnDatPhongNgay = By.CssSelector("#bookingForm button[type='submit']");
        private By msgInvalidFeedback = By.CssSelector(".invalid-feedback");

        // =============================================
        // LOCATORS — Payment/Pay.cshtml
        // button.btn-pay (form POST /Payment/ConfirmPay) = "Thanh toán tại quầy"
        // a.btn-momo = "Thanh toán qua MoMo"
        // a.btn-qr   = "Chuyển khoản VietQR"
        // span.amount = số tiền (định dạng N0 VNĐ)
        // =============================================
        private By btnThanhToanTaiQuay = By.CssSelector("button.btn-pay");
        private By lnkThanhToanMoMo = By.CssSelector("a.btn-momo");
        private By lnkChuyenKhoanVietQR = By.CssSelector("a.btn-qr");
        private By spanAmount = By.CssSelector("span.amount");

        // =============================================
        // LOCATORS — Payment/GenerateBankQr.cshtml
        // .qr-amount    = số tiền (ViewBag.Amount)
        // .qr-image img = ảnh QR (src = ViewBag.QrUrl)
        // a.btn-success  = "Đã thanh toán" → /Home/TrangChuNguoiDung
        // a.btn-secondary= "Quay lại" → /Payment/Pay?bookingId=X
        // =============================================
        private By spanQrAmount = By.CssSelector(".qr-amount");
        private By imgQrCode = By.CssSelector(".qr-image img");
        private By btnDaThanhToan = By.CssSelector("a.btn-success");
        private By btnQuayLaiQR = By.CssSelector("a.btn-secondary");

        // =============================================
        // LOCATORS — Bookings/Index.cshtml
        // a.btn-edit-booking[href*="Payment/Pay"] = nút "Thanh toán"
        // span.status-badge.payment-paid / payment-pending / payment-failed
        // span.status-badge.status-confirmed / status-pending / status-cancelled
        // =============================================
        private By lnkThanhToanInList = By.CssSelector("a.btn-edit-booking[href*='Payment/Pay']");

        // ================================================================
        // ACTIONS — Login
        // ================================================================

        public void EnterUsername(string username)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
            el.Clear();
            el.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtPassword));
            el.Clear();
            el.SendKeys(password);
        }

        public void ClickDangNhap()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnDangNhap)).Click();
        }

        /// <summary>Đăng nhập nhanh — gọi ở đầu mỗi test case (mỗi lần browser reset)</summary>
        public void Login(string username, string password)
        {
            EnterUsername(username);
            EnterPassword(password);
            ClickDangNhap();
            // Chờ redirect xong mới tiếp tục
            wait.Until(d => !d.Url.ToLower().Contains("/auth/login"));
        }

        // ================================================================
        // ACTIONS — Đổi Thông Tin (Customers/Edit)
        // ================================================================

        public void NhapFullName(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtFullName));
            el.Clear(); el.SendKeys(value);
        }

        public void NhapPhone(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtPhone));
            el.Clear(); el.SendKeys(value);
        }

        public void NhapEmail(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtEmail));
            el.Clear(); el.SendKeys(value);
        }

        public void NhapIdentityNumber(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtIdentityNumber));
            el.Clear(); el.SendKeys(value);
        }

        public void NhapAddress(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtAddress));
            el.Clear(); el.SendKeys(value);
        }

        public void NhanLuuThayDoi()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnLuuThayDoi)).Click();
        }

        public void NhanQuayLaiCustomer()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(lnkQuayLaiCustomer)).Click();
        }

        // ================================================================
        // ACTIONS — Đặt Phòng (Bookings/Create)
        // ================================================================

        public void ChonCustomer(string tenKhachHang)
        {
            // partialMatch: true để tránh lỗi khi text option chứa thêm thông tin
            new SelectElement(wait.Until(ExpectedConditions.ElementIsVisible(ddlCustomerId)))
                .SelectByText(tenKhachHang, true);
        }

        public void ChonRoom(string tenPhong)
        {
            var sel = wait.Until(ExpectedConditions.ElementIsVisible(ddlRoomId));
            SelectRoomByPartialText(sel, tenPhong);
        }

        /// <summary>Set date input qua JS để tránh vấn đề locale/format</summary>
        public void NhapCheckInDate(string isoDate)
        {
            SetDateViaJs(wait.Until(ExpectedConditions.ElementIsVisible(txtCheckInDate)), isoDate);
        }

        public void NhapCheckOutDate(string isoDate)
        {
            SetDateViaJs(wait.Until(ExpectedConditions.ElementIsVisible(txtCheckOutDate)), isoDate);
        }

        public void NhapNumberOfPeople(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtNumPeople));
            el.Clear(); el.SendKeys(value);
        }

        public void NhapNote(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtNote));
            el.Clear(); el.SendKeys(value);
        }

        /// <summary>Chọn Trạng thái thanh toán (name=PaymentStatusID)</summary>
        public void ChonPaymentStatus(string trangThai)
        {
            new SelectElement(wait.Until(ExpectedConditions.ElementIsVisible(ddlPaymentStatusID)))
                .SelectByText(trangThai, true);
        }

        /// <summary>Chọn Trạng thái đặt phòng (name=BookingStatusID)</summary>
        public void ChonBookingStatus(string trangThai)
        {
            new SelectElement(wait.Until(ExpectedConditions.ElementIsVisible(ddlBookingStatusID)))
                .SelectByText(trangThai, true);
        }

        public void NhanTaoDatPhong()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnTaoDatPhong)).Click();
        }

        public void NhanQuayLaiBooking()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(lnkQuayLaiBooking)).Click();
        }

        // ================================================================
        // ACTIONS — (_BookingFormPartial)
        // ================================================================

        public void ChonRoomFast(string tenPhong)
        {
            var sel = wait.Until(ExpectedConditions.ElementIsVisible(ddlRoomId));
            SelectRoomByPartialText(sel, tenPhong);
        }

        public void NhapCheckInFast(string isoDate)
        {
            SetDateViaJs(wait.Until(ExpectedConditions.ElementIsVisible(txtCheckInFast)), isoDate);
        }

        public void NhapCheckOutFast(string isoDate)
        {
            SetDateViaJs(wait.Until(ExpectedConditions.ElementIsVisible(txtCheckOutFast)), isoDate);
        }

        public void NhapSoNguoiFast(string value)
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(txtNumPeople));
            el.Clear(); el.SendKeys(value);
        }

        public void NhanDatPhongNgay()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnDatPhongNgay)).Click();
        }

        // ================================================================
        // ACTIONS — Thanh Toán (Payment/Pay)
        // ================================================================

        public void NhanThanhToanTaiQuay()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnThanhToanTaiQuay)).Click();
        }

        public void NhanThanhToanMoMo()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(lnkThanhToanMoMo)).Click();
        }

        public void NhanChuyenKhoanVietQR()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(lnkChuyenKhoanVietQR)).Click();
        }

        public void NhanDaThanhToan()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnDaThanhToan)).Click();
        }

        public void NhanQuayLaiQR()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(btnQuayLaiQR)).Click();
        }

        public void NhanThanhToanTrongDanhSach(int index = 0)
        {
            var links = driver.FindElements(lnkThanhToanInList);
            if (links.Count > index)
                links[index].Click();
            else
                throw new Exception($"Không tìm thấy nút Thanh toán tại index {index}");
        }

        // ================================================================
        // VERIFY HELPERS
        // ================================================================

        /// <summary>Có ít nhất 1 .text-danger không rỗng (lỗi MVC validation)</summary>
        public bool HasValidationError()
        {
            try
            {
                foreach (var el in driver.FindElements(msgTextDanger))
                    if (el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                        return true;
                return false;
            }
            catch { return false; }
        }

        public List<string> GetValidationErrors()
        {
            var list = new List<string>();
            try
            {
                foreach (var el in driver.FindElements(msgTextDanger))
                    if (el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                        list.Add(el.Text.Trim());
            }
            catch { }
            return list;
        }

        public bool HasInvalidFeedback()
        {
            try
            {
                foreach (var f in driver.FindElements(msgInvalidFeedback))
                    if (f.Displayed && !string.IsNullOrWhiteSpace(f.Text))
                        return true;
                return false;
            }
            catch { return false; }
        }

        public string GetAmountText()
        {
            try { return wait.Until(ExpectedConditions.ElementIsVisible(spanAmount)).Text.Trim(); }
            catch { return ""; }
        }

        public string GetQrAmountText()
        {
            try { return wait.Until(ExpectedConditions.ElementIsVisible(spanQrAmount)).Text.Trim(); }
            catch { return ""; }
        }

        public bool IsQrImageVisible()
        {
            try
            {
                var img = wait.Until(ExpectedConditions.ElementIsVisible(imgQrCode));
                return img.Displayed && !string.IsNullOrEmpty(img.GetAttribute("src"));
            }
            catch { return false; }
        }

        public bool HasBadgeWithClass(string cssClass)
        {
            try { return driver.FindElements(By.CssSelector($"span.status-badge.{cssClass}")).Count > 0; }
            catch { return false; }
        }

        public bool IsLinkVisibleWithHref(By locator, string hrefContains)
        {
            try
            {
                var el = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                return el.Displayed && (el.GetAttribute("href") ?? "").Contains(hrefContains);
            }
            catch { return false; }
        }

        public string SafeGetActualResult()
        {
            try
            {
                string url = driver.Url.ToLower();

                // Đổi thông tin: thành công → redirect về /Customers/Index
                if (url.Contains("/customers/index"))
                    return "Lưu thay đổi thành công";

                // Đặt phòng: thành công → BookingSuccess
                if (url.Contains("bookingsuccess") || url.Contains("booking/success"))
                    return "Đặt phòng thành công";

                // Thanh toán: trang QR
                if (url.Contains("generatebankqr"))
                    return "Hiển thị mã QR thanh toán";

                // Về trang chủ người dùng
                if (url.Contains("trangchunguoidung"))
                    return "Chuyển về trang chủ người dùng";

                // Vẫn ở trang hiện tại → lấy lỗi
                var errors = GetValidationErrors();
                if (errors.Count > 0)
                    return string.Join("; ", errors);

                if (HasInvalidFeedback())
                    return "Hiển thị lỗi invalid-feedback";

                return driver.Url;
            }
            catch (StaleElementReferenceException)
            {
                return "Lỗi hệ thống: stale element";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ================================================================
        // PRIVATE UTILS
        // ================================================================

        /// <summary>
        /// Chọn option trong dropdown phòng theo partial text.
        /// Ưu tiên: exact match → contains match → JS select value của option đầu tiên chứa text.
        /// </summary>
        private void SelectRoomByPartialText(IWebElement selectEl, string tenPhong)
        {
            var se = new SelectElement(selectEl);
            string search = tenPhong.Trim().ToLower();

            // Thử exact match trước
            foreach (var opt in se.Options)
            {
                if (opt.Text.Trim().ToLower() == search)
                {
                    opt.Click();
                    return;
                }
            }

            // Thử partial match (text option chứa tenPhong)
            foreach (var opt in se.Options)
            {
                if (opt.Text.Trim().ToLower().Contains(search))
                {
                    opt.Click();
                    return;
                }
            }

            // Thử ngược lại: tenPhong chứa một phần text của option (ví dụ "304" trong "Phòng 304")
            foreach (var opt in se.Options)
            {
                string optText = opt.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(optText) && search.Contains(optText))
                {
                    opt.Click();
                    return;
                }
            }

            throw new NoSuchElementException(
                $"Không tìm thấy option phòng với text gần với: '{tenPhong}'. " +
                $"Các option hiện có: {string.Join(", ", se.Options.Select(o => o.Text))}");
        }

        private void SetDateViaJs(IWebElement el, string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate) || isoDate == "\"\"")
            {
                el.Clear();
                return;
            }
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = arguments[1];", el, isoDate);
        }
    }
}