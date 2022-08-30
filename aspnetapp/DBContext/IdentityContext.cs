using System;
using System.Collections.Generic;
using entityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace aspnetapp
{
    public class NoteUser : IdentityUser
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? _ct_ { get; set; } = DateTime.Now;
        /// <summary>
        /// 创建者
        /// </summary>
        public string _cuid_ { get; set; } = string.Empty;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? _ut_ { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改人
        /// </summary>
        public string _uuid_ { get; set; } = string.Empty;
    }
    public class NoteRole : IdentityRole
    {
        public bool IsPreset { get; set; }

        public string Mark { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? _ct_ { get; set; } = DateTime.Now;
        /// <summary>
        /// 创建者
        /// </summary>
        public string _cuid_ { get; set; } = string.Empty;
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? _ut_ { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改人
        /// </summary>
        public string _uuid_ { get; set; } = string.Empty;
    }


    public partial class IdentityContext : IdentityDbContext<NoteUser, NoteRole, string>
    {
        public IdentityContext()
        {
        }


        public IdentityContext(DbContextOptions<IdentityContext> options)
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
            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class InitIdentityDB
    {

        public static async Task Initialize(IdentityContext context, UserManager<NoteUser> userManager, RoleManager<NoteRole> roleManager)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;   // DB has been seeded  
            }

            await CreateDefaultUserAndRole(userManager, roleManager);

        }

        private static async Task CreateDefaultUserAndRole(UserManager<NoteUser> userManager, RoleManager<NoteRole> roleManager)
        {
            string roleAdmin = "系统管理员";
            await CreateDefaultRole(roleManager, roleAdmin);
            var user = await CreateDefaultUser(userManager);
            await AddDefaultRoleToDefaultUser(userManager, roleAdmin, user);
        }

        private static async Task CreateDefaultRole(RoleManager<NoteRole> roleManager, string roleName)
        {
            await roleManager.CreateAsync(new NoteRole() { IsPreset = true , Name = roleName,_ut_ =DateTime.Now,_ct_=DateTime.Now });
        }

        private static async Task<NoteUser> CreateDefaultUser(UserManager<NoteUser> userManager)
        {
            var user = new NoteUser { UserName = "admin",_ct_ = DateTime.Now,_ut_=DateTime.Now };

            var  result = await userManager.CreateAsync(user, "Abc@123");

            var createdUser = await userManager.FindByNameAsync("admin");
            return createdUser;
        }

        private static async Task AddDefaultRoleToDefaultUser(UserManager<NoteUser> userManager, string role, NoteUser user)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }


}
