﻿using KidProjectServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidProjectServer.Config
{
    public class DBConnection : DbContext
    {
        public DBConnection(DbContextOptions<DBConnection> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Package> Packages { get; set; }
    }
}
