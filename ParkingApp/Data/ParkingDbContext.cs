using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Models;

namespace ParkingApp.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext (DbContextOptions<ParkingDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingApp.Models.ParkingVehicle> ParkingVehicle { get; set; } = default!;

        public DbSet<ParkingApp.Models.ParkingActivity>? ParkingActivity { get; set; }
    }
}
