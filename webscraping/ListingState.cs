using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webscraping
{
    internal class ListingState
    {
        public async void isSold(List<FlatDTO> flatDTOs)
        {
            List<string> dataBaseId = new List<string>();
            List<string> soldID = new List<string>();
            List<string> flatDTOId = new List<string>();

            foreach (FlatDTO flatDTO in flatDTOs)
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

            foreach (string id in dataBaseId)
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
