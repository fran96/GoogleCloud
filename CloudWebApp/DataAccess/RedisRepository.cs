using Common;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class RedisRepository
    {
        public string ConnectionString = "redis-16371.c135.eu-central-1-1.ec2.cloud.redislabs.com:16371,password=xxxx";
        ConnectionMultiplexer connection;
        public RedisRepository()
        {
            connection = ConnectionMultiplexer.Connect(ConnectionString);
        }

        public string StoreItems(List<Property> items)
        {
            var db = connection.GetDatabase();
            string serializedItems = JsonConvert.SerializeObject(items);

            db.StringSet("prop", serializedItems);
            return HashValue(serializedItems);
        }

        public string StoreOneItem(Property p)
        {
            var db = connection.GetDatabase();
            string serializedItems = JsonConvert.SerializeObject(p);

            db.StringSet("prop", serializedItems);
            return HashValue(serializedItems);
        }


        public List<Property> LoadItems()
        {
            var db = connection.GetDatabase();
            if (String.IsNullOrEmpty(db.StringGet("prop"))) return null;
            List<Property> items = JsonConvert.DeserializeObject<List<Property>>(db.StringGet("prop"));

            return items;
        }

        public string HashValue(string v)
        {
            MD5 alg = MD5.Create();
            byte[] digest = alg.ComputeHash(Encoding.UTF32.GetBytes(v));
            return Convert.ToBase64String(digest);
        }
    }
}
