using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MvcSample.Web.Models
{
    [BsonIgnoreExtraElements]
    public class Restaurants
    {
        [Required]
        [MinLength(4)]
        public string Name { get; set; }
        public string Address { get; set; }
        public int RestaurantId { get; set; }
        public string Cuisine { get; set; }

        public ObjectId _id { get; set; }
    }
}