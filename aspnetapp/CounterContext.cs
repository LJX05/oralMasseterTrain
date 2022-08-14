using System;
using System.Collections.Generic;
using entityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace aspnetapp
{
    public partial class CounterContext : DbContext
    {
        public CounterContext()
        {
        }
        public DbSet<Counter> Counters { get; set; } = null!;

        public DbSet<ClockIn> ClockIns { get; set; } = null!;

        public DbSet<Patient> Patients { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Role> Roles { get; set; } = null!;

        public DbSet<UserRole> UserRoles { get; set; } = null!;

        public DbSet<Function> Functions { get; set; } = null!;

        public DbSet<RoleFunc> RoleFuncs { get; set; } = null!;

        public CounterContext(DbContextOptions<CounterContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                var username = Environment.GetEnvironmentVariable("MYSQL_USERNAME");
                var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
                var addressParts = Environment.GetEnvironmentVariable("MYSQL_ADDRESS")?.Split(':');
                var host = addressParts?[0];
                var port = addressParts?[1];
                var connstr = $"server={host};port={port};user={username};password={password};database=aspnet_demo";
                optionsBuilder.UseMySql(connstr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
            }
#if DEBUG

            var connstr1 = $"server=sh-cynosdbmysql-grp-2c5br53o.sql.tencentcdb.com;port=21585;user=ljxroot;password=Lijinxuan123;database=aspnet_demo";
            optionsBuilder.UseMySql(connstr1, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            modelBuilder.Entity<Counter>().ToTable("Counters");
            modelBuilder.Entity<Patient>();
            modelBuilder.Entity<ClockIn>();
            modelBuilder.Entity<Video>();
            #region 系统表
            modelBuilder.Entity<User>();
            modelBuilder.Entity<UserRole>();
            modelBuilder.Entity<Role>();
            modelBuilder.Entity<Function>();
            modelBuilder.Entity<RoleFunc>();
            #endregion
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
