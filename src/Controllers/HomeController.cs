#region Copyright ©2016, Click2Cloud Inc. - All Rights Reserved
/* ------------------------------------------------------------------- *
*                            Click2Cloud Inc.                          *
*                  Copyright ©2016 - All Rights reserved               *
*                                                                      *
* Apache 2.0 License                                                   *
* You may obtain a copy of the License at                              * 
* http://www.apache.org/licenses/LICENSE-2.0                           *
* Unless required by applicable law or agreed to in writing,           *
* software distributed under the License is distributed on an "AS IS"  *
* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express  *
* or implied. See the License for the specific language governing      *
* permissions and limitations under the License.                       *
*                                                                      *
* -------------------------------------------------------------------  */
#endregion Copyright ©2016, Click2Cloud Inc. - All Rights Reserved

using Microsoft.AspNet.Mvc;
using Click2Cloud.Samples.AspNetCore.MvcMongoDb.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Click2Cloud.Samples.AspNetCore.MvcMongoDb.Web
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
            //string server = Environment.GetEnvironmentVariable("DATABASE_SERVICE_NAME");
            //TODO: Provide ip of mongo db server 
            string server = "172.30.193.8"; 
            string databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE");


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
                catch (Exception ex) { Logger.Error(ex, "InitializeMongoDatabase"); }
            }
            else { MONGO_DATABASE = null; }
        }

        public IActionResult Index()
        {
            if (MONGO_DATABASE == null) { InitializeMongoDatabase(); }
            List<Restaurants> restCollection = ListRestaurants();
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
            }
            catch (Exception ex) { Logger.Error(ex, "Index"); }

            List<Restaurants> restCollection = ListRestaurants();
            return View(restCollection);
        }

        public List<Restaurants> ListRestaurants()
        {
            List<Restaurants> restaurant = new List<Restaurants>();
            try
            {
                var collection = MONGO_DATABASE.GetCollection<BsonDocument>(COLLECTION_NAME);
                var filter = new BsonDocument();
                using (var cursor = collection.FindAsync(filter).Result)
                {
                    while (cursor.MoveNextAsync().Result)
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            int id = 0;
                            int.TryParse(document["RestaurantId"].ToString(), out id);
                            restaurant.Add(new Restaurants()
                            {
                                Name = document["Name"].ToString(),
                                Address = document["Address"].ToString(),
                                RestaurantId = id,
                                Cuisine = document["Cuisine"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Error(ex, "ListRestaurants"); }
            return restaurant;
        }
    }
}
