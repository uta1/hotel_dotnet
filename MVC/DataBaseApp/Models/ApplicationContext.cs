using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBaseApp.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<RoomMeta> RoomsMeta { get; set; }
        public DbSet<RoomInfo> RoomsInfo { get; set; }
        public DbSet<Order> Orders { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) 
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
