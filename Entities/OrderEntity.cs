using System;
using System.ComponentModel.DataAnnotations;


namespace AmazEats.Entities
{
	public class OrderEntity
	{
		public string Id { get; set; }
        public long Number { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

