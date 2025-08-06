using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace TestServer.Data;

public class GoceGidContext : IdentityDbContext<IdentityUser>
{
    public GoceGidContext(DbContextOptions<GoceGidContext> options) : base(options)
    {
    }

    public DbSet<StoneColor> StoneColors { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<StoneType> StoneTypes { get; set; }
    public DbSet<OrderPallet> OrderPallets { get; set; }
    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Pallet> Pallets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OrderPallet>()
        .HasKey(op => new { op.OrderId, op.PalletId });
    }
}

