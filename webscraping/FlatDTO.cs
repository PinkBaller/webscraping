using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webscraping
{
    internal class FlatDTO
    {
        public string? link = null;
        public string? flatNumber = null;
        public string? houseNumber = null;
        public double flatArea;
        public int numberOfRooms;
        public int numberOfFloors;
        public int whichFloor;

        public int buildDate;
        public int renovationDate;

        public string? buildingType = null;
        public string? heatingType = null;
        public string? listingName = null;
        public string? energyUsageClass = null; 


        public double doubleFlatPrice;

        public string? googleMapsLocation = null;
        public string? latitude = null;
        public string? longitude = null;

        public bool isReserved;
        public bool isAuction;

        public string? uniqueID = null;
    }
}
    