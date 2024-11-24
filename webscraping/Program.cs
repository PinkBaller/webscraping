using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Text.Json;
using OpenQA.Selenium.Support.UI;
using webscraping;
using System.Text.RegularExpressions;
using Npgsql;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        WebScraper webscraper = new WebScraper("https://www.aruodas.lt/butai/puslapis/1/?FOrder=Price&detailed_search=1");

        List<FlatDTO> flatDTOs = webscraper.scrape();

        DatabaseManager database = new DatabaseManager("localhost", 5432, "postgres", "1365", "webscraper");

        database.isSold(flatDTOs);
    }
}