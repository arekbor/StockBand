﻿using Microsoft.EntityFrameworkCore;
using StockBand.Models;

namespace StockBand.Data
{
    public class ApplicationDbContext:DbContext
    {
        public DbSet<User> UserDbContext { get; set; }
        public DbSet<UserLog> UserLogDbContext { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions) : base(contextOptions)
        {

        }
    }
}
