using GameMaker.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.DataStorageImplementation
{
    public class GameContext : DbContext
    {


        public DbSet<Game> Games { get; set; }
       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connstring = @"Server=tcp:mymailarjun.database.windows.net,1433;Initial Catalog=MyAzureData;Persist Security Info=False;User ID=applicationuser;Password=Password@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;";

            optionsBuilder.UseSqlServer(connstring);



        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>().HasKey(p => p.SessonId);
           
        }


    }
}
