using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace webscraping
{
    internal class WebScraper
    {
        public string URL { get; set; }
        public WebScraper(string URL)
        {
            this.URL = URL;
        }     
        
        static void cookieBtnClick(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            if (driver.FindElement(By.Id("onetrust-accept-btn-handler")) != null)
            driver.FindElement(By.Id("onetrust-accept-btn-handler")).Click();
        }
       
        
        public List<FlatDTO> scrape()
        {
            DatabaseManager database = new DatabaseManager("localhost", 5432, "postgres", "1365", "webscraper");

            List<FlatDTO> flatDTOList = new List<FlatDTO>();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            //chromeOptions.AddArguments("headless");

            // Set up ChromeDriver
            IWebDriver mainDriver = new ChromeDriver(chromeOptions);
            IWebDriver secondaryDriver = new ChromeDriver(chromeOptions);

            mainDriver.Navigate().GoToUrl(URL);

            System.Threading.Thread.Sleep(100);
            cookieBtnClick(mainDriver);
            System.Threading.Thread.Sleep(100);


            while (mainDriver.FindElement(By.XPath("//a[text()='»']")) != null)
            {


                IWebElement element = mainDriver.FindElement(By.TagName("div"));


                foreach (IWebElement Listing in element.FindElements(By.ClassName("list-photo-v2")))
                {
                    //Setting up FlatDTO
                    FlatDTO flatDTO = new FlatDTO();

                    IWebElement elements = Listing.FindElement(By.TagName("a"));

                    //getting the link of listing
                    flatDTO.link = elements.GetAttribute("href");

                    IWebElement postingData = null;

                    secondaryDriver.Navigate().GoToUrl(flatDTO.link);
                    try
                    {
                        postingData = secondaryDriver.FindElement(By.ClassName("main-content"));
                    }
                    catch (Exception e)
                    {
                        postingData = secondaryDriver.FindElement(By.CssSelector("div.main.project-in"));
                    }

                    string? coordinates = null;
                    string? flatPrice = null;

                    string? buildingData = null;

                    //Getting googleMapLocation
                    if (postingData.FindElements(By.CssSelector("a.link-obj-thumb.vector-thumb-map[data-type='map']")).Count > 0)
                    {
                        flatDTO.googleMapsLocation = postingData.FindElement(By.CssSelector("a.link-obj-thumb.vector-thumb-map[data-type='map']")).GetAttribute("href");
                        coordinates = flatDTO.googleMapsLocation.Replace("https://www.google.com/maps/search/?api=1&query=", "");
                        //West or East
                        flatDTO.latitude = coordinates.Split("%2C")[1];
                        //North or South
                        flatDTO.longitude = coordinates.Split("%2C")[0];
                    }

                    //Reservation and auction Check
                    if (postingData.FindElements(By.ClassName("special-comma")).Count > 0)
                    {
                        if (postingData.FindElements(By.ClassName("special-comma"))[0].Text == "Varžytynės/aukcionas")
                            flatDTO.isAuction = true;
                    }

                    if (postingData.FindElements(By.ClassName("reservation-strip")).Count > 0)
                        flatDTO.isReserved = true;

                    //Price of flat
                    flatPrice = postingData.FindElement(By.ClassName("price-eur")).Text.Replace("€", "").Replace(" ", "").Replace(",", ".");
                    flatDTO.doubleFlatPrice = double.Parse(flatPrice, CultureInfo.InvariantCulture);

                    //Unique Listing ID
                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Nuoroda')]/following-sibling::dd")).Count > 0)
                        flatDTO.uniqueID = postingData.FindElement(By.XPath("//*[contains(text(), 'Nuoroda')]/following-sibling::dd")).Text.Replace("www.aruodas.lt/", "");

                    if (postingData.FindElements(By.ClassName("project__advert-info__value")).Count > 0)
                        flatDTO.uniqueID = postingData.FindElement(By.ClassName("project__advert-info__value")).Text.Replace("www.aruodas.lt/", "");

                    //Other details of Flat like listing name, flat number, flatarea, etc.
                    //If checking whether they are provided.
                    if (postingData.FindElements(By.ClassName("obj-header-text")).Count > 0)
                        flatDTO.listingName = postingData.FindElements(By.ClassName("obj-header-text"))[0].Text;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Buto numeris:')]/following-sibling::dd")).Count > 0)
                        flatDTO.flatNumber = postingData.FindElements(By.XPath("//*[contains(text(), 'Buto numeris:')]/following-sibling::dd"))[0].Text;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Plotas:')]/following-sibling::dd")).Count > 0)
                        flatDTO.flatArea = double.Parse((postingData.FindElements(By.XPath("//*[contains(text(), 'Plotas:')]/following-sibling::dd"))[0].Text.Replace("m²", "").Replace(" ", "").Replace(",", ".")), CultureInfo.InvariantCulture);
                    
                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Kambarių sk.:')]/following-sibling::dd")).Count > 0)
                        flatDTO.numberOfRooms = Convert.ToInt16(postingData.FindElements(By.XPath("//*[contains(text(), 'Kambarių sk.:')]/following-sibling::dd"))[0].Text);

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Aukštų sk.:')]/following-sibling::dd")).Count > 0)
                    {
                        int tempNumberOfFloors = Convert.ToInt32(postingData.FindElements(By.XPath("//*[contains(text(), 'Aukštų sk.:')]/following-sibling::dd"))[0].Text);
                        if (tempNumberOfFloors < 130)
                            flatDTO.numberOfFloors = tempNumberOfFloors;
                        else
                            flatDTO.numberOfFloors = 0;
                    }        
                    else
                        flatDTO.numberOfFloors = 0;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Aukštas:')]/following-sibling::dd")).Count > 0)
                        flatDTO.whichFloor = Convert.ToInt16(postingData.FindElements(By.XPath("//*[contains(text(), 'Aukštas:')]/following-sibling::dd"))[0].Text);



                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Metai:')]/following-sibling::dd")).Count > 0)
                    {
                            buildingData = postingData.FindElements(By.XPath("//*[contains(text(), 'Metai:')]/following-sibling::dd"))[0].Text;
                        if (buildingData.Contains("renovacija") == false)
                            flatDTO.buildDate = Convert.ToInt16(postingData.FindElements(By.XPath("//*[contains(text(), 'Metai:')]/following-sibling::dd"))[0].Text);
                        else
                        {
                            string? tempRenovationDate = buildingData.Split(",")[1];
                            flatDTO.renovationDate = Convert.ToInt16($"{tempRenovationDate[1]}{tempRenovationDate[2]}{tempRenovationDate[3]}{tempRenovationDate[4]}");
                            flatDTO.buildDate = Convert.ToInt16($"{buildingData[0]}{buildingData[1]}{buildingData[2]}{buildingData[3]}");
                        }
                    }



                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Pastato tipas:')]/following-sibling::dd")).Count > 0)
                        flatDTO.buildingType = postingData.FindElements(By.XPath("//*[contains(text(), 'Pastato tipas:')]/following-sibling::dd"))[0].Text;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Šildymas:')]/following-sibling::dd")).Count > 0)
                        flatDTO.heatingType = postingData.FindElements(By.XPath("//*[contains(text(), 'Šildymas:')]/following-sibling::dd"))[0].Text;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Namo numeris:')]/following-sibling::dd")).Count > 0)
                        flatDTO.houseNumber = postingData.FindElements(By.XPath("//*[contains(text(), 'Namo numeris:')]/following-sibling::dd"))[0].Text;

                    if (postingData.FindElements(By.XPath("//*[contains(text(), 'Pastato energijos suvartojimo klasė:')]/following-sibling::dd")).Count > 0)
                        flatDTO.energyUsageClass = postingData.FindElements(By.XPath("//*[contains(text(), 'Pastato energijos suvartojimo klasė:')]/following-sibling::dd"))[0].Text;

                   
                    //??TODO await database to finish
                    flatDTOList.Add(flatDTO);
                    database.ListingToDatabase(flatDTO);

                }
                //If page button disabled, no more pages
                if (mainDriver.FindElement(By.XPath("//a[text()='»']")).GetAttribute("class") == "page-bt-disabled")
                    break;
                mainDriver.FindElement(By.XPath("//a[text()='»']")).Click();

            }

            secondaryDriver.Quit();
            mainDriver.Quit(); 

            database.Connection.Close();

            return flatDTOList;
        }
    }
}
