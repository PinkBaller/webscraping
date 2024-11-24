using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V128.Page;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace webscraping
{
    internal class DatabaseManager
    {

        public NpgsqlConnection Connection { get; set; }
        public DatabaseManager(string server, int port, string userID, string password, string database)
        {
            this.Connection = new NpgsqlConnection(connectionString: $"Server={server};Port={port};User Id={userID};Password={password};Database={database}");
            this.Connection.Open();
        }

        public async void ListingToDatabase(FlatDTO flatDTO)
        {
            using var cmd = new NpgsqlCommand();
            cmd.Connection = Connection;
            cmd.CommandText = "INSERT INTO flats" +
                "(id, url, flat_number, house_number, flat_area, number_of_rooms, number_of_floors, which_floor, build_date, renovation_date, building_type," +
                "heating_type, listing_name, energy_usage_class, double_flat_price, google_maps_location, latitude, longitude, is_reserved, is_auction, is_sold, when_sold)" +
                "VALUES" +
                $"('{flatDTO.uniqueID}', '{flatDTO.link}', '{flatDTO.flatNumber}', '{flatDTO.houseNumber}', '{flatDTO.flatArea}', '{flatDTO.numberOfRooms}', " +
                $"'{flatDTO.numberOfFloors}', '{flatDTO.whichFloor}', '{flatDTO.buildDate}', '{flatDTO.renovationDate}', '{flatDTO.buildingType}', '{flatDTO.heatingType}', " +
                $"'{flatDTO.listingName}', '{flatDTO.energyUsageClass}', '{flatDTO.doubleFlatPrice}', '{flatDTO.googleMapsLocation}', '{flatDTO.longitude}'," +
                $"'{flatDTO.latitude}', '{flatDTO.isReserved}', '{flatDTO.isAuction}', '{false}', '{null}')" +
                "ON CONFLICT(id)" +
                $"DO UPDATE SET" +
                $" double_flat_price = EXCLUDED.double_flat_price";

                await cmd.ExecuteNonQueryAsync();   
        }

        public async void isSold(List<FlatDTO> flatDTOs)
        {
            List<string> dataBaseId = new List<string>();
            List<string> soldID = new List<string>();
            List<string> flatDTOId = new List<string>();

            foreach(FlatDTO flatDTO in flatDTOs)
            {
                flatDTOId.Add(flatDTO.uniqueID);
            }


            using NpgsqlCommand cmd = new NpgsqlCommand("SELECT id FROM flats", Connection);

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string id = (string)reader["id"];
                dataBaseId.Add(id);
            }
            reader.Close();

            foreach(string id in dataBaseId)
            {
                if (dataBaseId.Contains(id) && !flatDTOId.Contains(id))
                    soldID.Add(id);
            }


            foreach (string id in soldID)
            {
                using var sql = new NpgsqlCommand();
                sql.Connection = Connection;
                sql.CommandText = $"UPDATE flats SET is_sold = 'TRUE', when_sold = '{DateTime.Today}' WHERE id = '{id}'";
                sql.ExecuteNonQuery();
            }

            await cmd.ExecuteNonQueryAsync();
        }
    }
}