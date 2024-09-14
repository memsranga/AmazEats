using System;
using Microsoft.EntityFrameworkCore;
using AmazEats.Entities;
using System.Xml;

namespace AmazEats
{
    public class AmazEatsDbContext : DbContext
    {
        public AmazEatsDbContext(DbContextOptions<AmazEatsDbContext> options) : base(options)
        {
        }

        public DbSet<OrderEntity> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ordersEntity = modelBuilder.Entity<OrderEntity>();
            ordersEntity.HasKey(o => o.Id);

            ordersEntity.ToContainer("Orders");
        }
    }
}