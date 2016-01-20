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
        private static IMongoDatabase MONGO_DATABASE = null;
        private static string COLLECTION_NAME = "restaurants";

        private void InitializeMongoDatabase()
        {
            //Retrive Parameters from Environment Variables
            string userName = Environment.GetEnvironmentVariable("MONGODB_USER");
            string password = Environment.GetEnvironmentVariable("MONGODB_PASSWORD");
            string server = Environment.GetEnvironmentVariable("DATABASE_SERVICE_NAME");
            string databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");

            if (!(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(databaseName)))
            {
                //Create Connection String for MongoDB
                string mongoDbConnectionString = string.Format("mongodb://{0}:{1}@{2}:{3}/{4}", userName, password,
                    server, "27017", databaseName);

                try
                {
                    var client = new MongoClient(mongoDbConnectionString);
                    MONGO_DATABASE = client.GetDatabase(databaseName);
                }
                catch (Exception) { }
            }
            else { MONGO_DATABASE = null; }
        }

        public IActionResult Index()
        {
            InitializeMongoDatabase();
            List<Restaurants> restCollection = ListRestaurants();

            restCollection = new List<Restaurants>();
            return View(restCollection);
        }

        [HttpPost]
        public IActionResult Index(Microsoft.AspNet.Http.Internal.FormCollection formCollection)
        {
            try
            {
                BsonDocument doc = new BsonDocument();
                doc.Add(new BsonElement("Name", Request.Form["restname"].ToString()));
                doc.Add(new BsonElement("Address", Request.Form["address"].ToString()));
                doc.Add(new BsonElement("RestaurantId", Request.Form["restaurantId"].ToString()));
                doc.Add(new BsonElement("Cuisine", Request.Form["cuisine"].ToString()));

                var collection = MONGO_DATABASE.GetCollection<BsonDocument>(COLLECTION_NAME);
                collection.InsertOne(doc);

                //Restaurants restaurant = new Restaurants()
                //{
                //    Name = Request.Form["restname"],
                //    Address = Request.Form["address"],
                //    RestaurantId = int.Parse(Request.Form["restaurantId"]),
                //    Cuisine = Request.Form["cuisine"]
                //};
                //collection.InsertOneAsync(restaurant.ToBsonDocument()).Wait();
            }
            catch (Exception) { }

            List<Restaurants> restCollection = ListRestaurants();
            return View(restCollection);
        }

        public List<Restaurants> ListRestaurants()
        {
            List<Restaurants> restaurant = new List<Restaurants>();
            var collection = MONGO_DATABASE.GetCollection<BsonDocument>(COLLECTION_NAME);
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
            catch (Exception ex) { }
            return restaurant;
        }
    }
}