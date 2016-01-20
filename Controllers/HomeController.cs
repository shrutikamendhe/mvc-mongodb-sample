using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MvcSample.Web
{
    public class HomeController : Controller
    {
        private static string _mongoDbConnectionString = string.Empty;
        private static string _databaseName = string.Empty;
        private static string _collectionName = "restaurants";

        private static IMongoClient _client;
        private static IMongoDatabase _database;

        private static List<Restaurants> restCollection = null;

        public IActionResult Index()
        {
            restCollection = new List<Restaurants>();

            try
            {
                string userName = Environment.GetEnvironmentVariable("MONGODB_USER");
                string password = Environment.GetEnvironmentVariable("MONGODB_PASSWORD");
                string server = Environment.GetEnvironmentVariable("DATABASE_SERVICE_NAME");
                _databaseName = "sampledb";

                if (!(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(server)))
                {
                    _mongoDbConnectionString = "mongodb://" + userName + ":" + password + "@" + server + ":27017/" + _databaseName;
                    restCollection = ListRestaurants();
                }
            }
            catch (Exception) { }

            return View(restCollection);
        }

        public List<Restaurants> ListRestaurants()
        {
            List<Restaurants> restaurant = new List<Restaurants>();

            if (_client == null && !string.IsNullOrEmpty(_mongoDbConnectionString) ) { _client = new MongoClient(_mongoDbConnectionString); }
            if (_database == null) { _database = _client.GetDatabase(_databaseName); }

            var collection = _database.GetCollection<BsonDocument>(_collectionName);

            try
            {
                var filter = new BsonDocument();
                using (var cursor = collection.FindAsync(filter).Result)
                {
                    while (cursor.MoveNextAsync().Result)
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            restaurant.Add(new Restaurants()
                            {
                                Name = document["Name"].ToString(),
                                Address = document["Address"].ToString(),
                                RestaurantId = int.Parse(document["RestaurantId"].ToString()),
                                Cuisine = document["Cuisine"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception) { }

            //Restaurants restaurant = new Restaurants()
            //{
            //    Name = "Haldiram",
            //    Address = "2 Avenue",
            //    RestaurantId = 41704621,
            //    Cuisine = "Indian"
            //};

            ////BsonDocument doc = new BsonDocument();
            ////doc.Add(new BsonElement("Name", "test"));
            ////collection.InsertOne(doc);

            //collection.InsertOneAsync(restaurant.ToBsonDocument()).Wait();

            return restaurant;
        }
    }
}