using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webscraping
{

    internal class RemovedListingScrape
    {
        DatabaseManager database = new DatabaseManager("localhost", 5432, "postgres", "1365", "webscraper");

        public List<string> getRemovedListings(List<FlatDTO> flatDTOs)
        {
            List<string> flatDTOId = new List<string>();
            List<string> removedID = new List<string>();
            List<string> databaseID = database.getIdColumn();

            foreach(FlatDTO flatDTO in flatDTOs)
            {
                flatDTOId.Add(flatDTO.uniqueID);
            }


            foreach (string id in  databaseID) 
            {
                if (databaseID.Contains(id) && !flatDTOId.Contains(id))
                {
                    Console.WriteLine(id);
                    removedID.Add(id);
                }
            }

            return removedID;
        }

        static void cookieBtnClick(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            if (driver.FindElement(By.Id("onetrust-accept-btn-handler")) != null)
                driver.FindElement(By.Id("onetrust-accept-btn-handler")).Click();
        }

        public void updatedRemovedListingData(List<string> removedIds)
        {
            var dateAndTime = DateTime.Now;
            string today = Convert.ToString(dateAndTime.Date);

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            //chromeOptions.AddArguments("headless");

            // Set up ChromeDriver
            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Navigate().GoToUrl("https://www.aruodas.lt/");

            int i = 0;
            foreach (string id in removedIds)
            {
                bool isSold = Convert.ToBoolean(database.getIsSoldColumn(removedIds[i]));
                bool maybeSold = Convert.ToBoolean(database.getMaybeSoldColumn(removedIds[i])); ;
                bool delisted = Convert.ToBoolean(database.getDelistedColumn(removedIds[i])); ;
                string expiration_date = "none";
                string URL = "https://www.aruodas.lt/" + id;


                // Set up ChromeDriver
                driver.Navigate().GoToUrl(URL);

                System.Threading.Thread.Sleep(100);
                cookieBtnClick(driver);
                System.Threading.Thread.Sleep(100);

                IWebElement postingData = driver.FindElement(By.ClassName("obj-cont"));

                if (postingData.FindElements(By.ClassName("adv-sold1-lt")).Count > 0)
                    isSold = true;

                if (postingData.FindElements(By.XPath("//*[contains(text(), 'Aktyvus')]/following-sibling::dd")).Count > 0)
                    expiration_date = postingData.FindElement(By.XPath("//*[contains(text(), 'Aktyvus')]/following-sibling::dd")).Text;

                if (postingData.FindElements(By.TagName("b")).Count > 0 && !isSold && expiration_date == today && !delisted && !maybeSold)
                    delisted = true;

                if (!isSold && !delisted && !maybeSold && expiration_date != today)
                    maybeSold = true;

                database.UpdateListing(isSold, maybeSold, delisted, expiration_date, id);
                i += 1;
            }
            driver.Close();
        }


    }
}
