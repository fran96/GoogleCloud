using Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PropertyRepository : ConnectionClass
    {
        public PropertyRepository() : base()
        {

        }

        public List<Property> GetProperty()
        {
            //DataAdapter
            //DataReader

            if (MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
            }

            string sql = "Select id, name, price, propertyPicture, location, username, description From property";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            List<Property> myResults = new List<Property>();

            while (reader.Read())
            {
                myResults.Add(new Property()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    PropertyPicture = reader.GetString(3),
                    Location = reader.GetString(4),
                    Username = reader.GetString(5),
                    Description = reader.GetString(6)
                });
            }

            return myResults;
        }

        public Property GetPropertyById(int id)
        {
            if (MyConnection.State == System.Data.ConnectionState.Closed)
            {
                MyConnection.Open();
            }

            string sql = "Select id, name, location, username, description From property where id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Property myResults = new Property();
            myResults=(new Property()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Location = reader.GetString(2),
                Username = reader.GetString(3),
                Description = reader.GetString(4)
            });
            return myResults;
        }

        

        public void AddProperty(Property p)
        {
            string sql = "insert into property (Name, Price, PropertyPicture, Location, Username, Description) Values (@name, @price, @propertyPicture, @location, @username, @description)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@propertyPicture", p.PropertyPicture);
            cmd.Parameters.AddWithValue("@location", p.Location);
            cmd.Parameters.AddWithValue("@username", p.Username);
            cmd.Parameters.AddWithValue("@description", p.Description);
            cmd.ExecuteNonQuery(); //executes command

        }
        

        public void DeleteProperty(int id)
        {
            string sql = "delete from property where id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@id", id);

            //the command has to participate in the transaction for the transaction to be effective
            cmd.Transaction = MyTransaction;

            cmd.ExecuteNonQuery(); //executes command

        }

        public void UpdateProperty(Property p)
        {
            string sql = "update property set Name = @name, Price = @price, PropertyPicture = @propertyPicture, Location = @location, Username = @username, Description = @description where id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, MyConnection);
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@propertyPicture", p.PropertyPicture);
            cmd.Parameters.AddWithValue("@location", p.Location);
            cmd.Parameters.AddWithValue("@username", p.Username);
            cmd.Parameters.AddWithValue("@description", p.Description);
            cmd.Parameters.AddWithValue("@id", p.Id);

            cmd.ExecuteNonQuery(); //executes command

        }
    }
}
