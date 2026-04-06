using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace KhachSanTest.Pages
{
    public class StatisticsPage
    {
        private IWebDriver driver;

        public StatisticsPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ===== ELEMENT =====
        private IWebElement txtFromDate => driver.FindElement(By.Name("FromDate"));
        private IWebElement txtToDate => driver.FindElement(By.Name("ToDate"));
        private IWebElement btnView => driver.FindElement(By.CssSelector("button[type='submit']"));

        // kết quả (ví dụ label hoặc table)
        private IWebElement lblResult => driver.FindElement(By.Id("result"));

        // ===== ACTION =====
        public void EnterFromDate(string date)
        {
            txtFromDate.Clear();
            txtFromDate.SendKeys(date);
        }

        public void EnterToDate(string date)
        {
            txtToDate.Clear();
            txtToDate.SendKeys(date);
        }

        public void ClickView()
        {
            btnView.Click();
        }

        public string GetResult()
        {
            try
            {
                return lblResult.Text;
            }
            catch
            {
                return "";
            }
        }
    }
}