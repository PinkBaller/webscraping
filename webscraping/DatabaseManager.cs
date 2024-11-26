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
                "heating_type, listing_name, energy_usage_class, double_flat_price, google_maps_location, latitude, longitude, is_reserved, is_auction, is_sold, when_sold," +
                "maybe_sold, delisted, expiration_date)" +
                "VALUES" +
                $"('{flatDTO.uniqueID}', '{flatDTO.link}', '{flatDTO.flatNumber}', '{flatDTO.houseNumber}', '{flatDTO.flatArea}', '{flatDTO.numberOfRooms}', " +
                $"'{flatDTO.numberOfFloors}', '{flatDTO.whichFloor}', '{flatDTO.buildDate}', '{flatDTO.renovationDate}', '{flatDTO.buildingType}', '{flatDTO.heatingType}', " +
                $"'{flatDTO.listingName}', '{flatDTO.energyUsageClass}', '{flatDTO.doubleFlatPrice}', '{flatDTO.googleMapsLocation}', '{flatDTO.longitude}'," +
                $"'{flatDTO.latitude}', '{flatDTO.isReserved}', '{flatDTO.isAuction}', '{false}', '{null}', '{false}', '{false}', 'none')" +
                "ON CONFLICT(id)" +
                $"DO UPDATE SET" +
                $" double_flat_price = EXCLUDED.double_flat_price";
            
                await cmd.ExecuteNonQueryAsync();   
        }

        public async void UpdateListing(bool isSold, bool maybeSold, bool delisted, string expirationDate, string uniqueId)
        {
            using var cmd = new NpgsqlCommand();
            cmd.Connection = Connection;
            cmd.CommandText = "UPDATE flats " +
                "SET is_sold = @isSold, maybe_sold = @maybeSold, delisted = @delisted, expiration_date = @expiration, when_sold = @today " +
                $"WHERE id = @uniqueId";
            cmd.Parameters.AddWithValue("@isSold", isSold);
            cmd.Parameters.AddWithValue("@maybeSold", maybeSold);
            cmd.Parameters.AddWithValue("@delisted", delisted);
            cmd.Parameters.AddWithValue("@expiration", expirationDate);
            cmd.Parameters.AddWithValue("@today", DateTime.Today);
            cmd.Parameters.AddWithValue("@uniqueId", uniqueId);
            await cmd.ExecuteNonQueryAsync();
        }

        public List<string> getIdColumn()
        {
            using NpgsqlCommand cmd = new NpgsqlCommand("SELECT id FROM flats", Connection);
            List<string> dataBaseId = new List<string>();

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string id = (string)reader["id"];
                dataBaseId.Add(id);
            }
            reader.Close();

            Task.Delay(1000);
            return dataBaseId;
        }

        public bool getMaybeSoldColumn(string id)
        {
            using NpgsqlCommand cmd = new NpgsqlCommand($"SELECT maybe_sold FROM flats WHERE id = '{id}'", Connection);
            bool dataBaseMaybeSold = false;

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bool maybe_sold = (bool)reader["maybe_sold"];
                dataBaseMaybeSold = maybe_sold;
            }
            reader.Close();

            return dataBaseMaybeSold;
        }

        public bool getIsSoldColumn(string id)
        {
            using NpgsqlCommand cmd = new NpgsqlCommand($"SELECT is_sold FROM flats WHERE id = '{id}'", Connection);
            bool databaseIsSold = false;

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bool is_sold = (bool)reader["is_sold"];
                databaseIsSold = is_sold;
            }
            reader.Close();

            return databaseIsSold;
        }

        public bool getDelistedColumn(string id)
        {
            using NpgsqlCommand cmd = new NpgsqlCommand($"SELECT delisted FROM flats WHERE id = '{id}'", Connection);
            bool databaseDelistedList = false;

            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bool delisted = (bool)reader["delisted"];
                databaseDelistedList = delisted;
            }
            reader.Close();

            return databaseDelistedList;
        }
    }
}