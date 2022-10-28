using System;
using System.Collections.Generic;
using EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;

namespace aspnetapp
{
    public partial class BusinessContext : DbContext
    {
        public BusinessContext()
        {
        }
        public DbSet<Counter> Counters { get; set; } = null!;

        public DbSet<ClockIn> ClockIns { get; set; } = null!;

        public DbSet<Video> Videos { get; set; } = null!;

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<PatientActivity> PatientActivitys { get; set; } = null!;
        public DbSet<PatientToTeachVideo> PatientToTeachVideos { get; set; } = null!;
        public DbSet<WeMessageTemplate> WeMessageTemplates { get; set; } = null!;
        public DbSet<WeMessageTemplateConfig> TemplateConfigs { get; set; } = null!;


        public BusinessContext(DbContextOptions<BusinessContext> options)
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
                optionsBuilder.UseLazyLoadingProxies().UseMySql(connstr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
            }
            #if DEBUG
            var connstr1 = $"server=sh-cynosdbmysql-grp-81t6sb3w.sql.tencentcdb.com;port=26148;user=root;password=pg8xZueU;database=aspnet_demo";
            optionsBuilder.UseLazyLoadingProxies()
                .UseMySql(connstr1, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
            #endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            modelBuilder.Entity<Counter>().ToTable("Counters");

            modelBuilder.Entity<Patient>(entityModel =>
            {
                entityModel.HasQueryFilter(p => !p.IsDeleted);
                entityModel.HasMany(pv => pv.PToVList).WithOne().HasForeignKey(o => o.PId);
                entityModel.HasOne(pv => pv.LastCheckIn).WithMany()
                .HasForeignKey(o => o.LastCheckInId);
            });
            modelBuilder.Entity<PatientToTeachVideo>()
                .HasOne(pv => pv.TVideo)
                .WithMany()
                .HasForeignKey(o => o.TVId);

            modelBuilder.Entity<ClockIn>()
                .HasMany(b => b.Videos)
                .WithOne()
                .HasForeignKey(v => v.ClockId);
            modelBuilder.Entity<Video>();
            modelBuilder.Entity<PatientActivity>();
            modelBuilder.Entity<WeMessageTemplate>();
            modelBuilder.Entity<WeMessageTemplateConfig>();
            #region 系统表
            //modelBuilder.Entity<User>();
            //modelBuilder.Entity<UserRole>();
            //modelBuilder.Entity<Role>();
            //modelBuilder.Entity<Function>();
            //modelBuilder.Entity<RoleFunc>();
            #endregion
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
