using System;
using Newtonsoft.Json;

namespace AmazEats.Entities
{
	public class OrderEntity
	{
        [JsonProperty("id")]
        public string Id { get; set; }
        public long Number { get; set; }
        public String CafeId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

