using System;
using Newtonsoft.Json;

namespace AmazEats.Entities
{
    public class OrderNumberEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string CafeId { get; set; }
        public DateTime Date { get; set; }
        public long CurrentNumber { get; set; }
    }
}

