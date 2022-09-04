using aspnetapp;
using aspnetapp.Common;
using aspnetapp.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BusinessContext>();
builder.Services.AddDbContext<IdentityContext>();
builder.Services.AddIdentity<NoteUser, NoteRole>()
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{

    // Password settings.
    options.Password.RequireDigit = true; //要求有数字介于0-9 之间,  默认true
    options.Password.RequireLowercase = true;//要求小写字母,  默认true
    options.Password.RequireNonAlphanumeric = true;//要求特殊字符,  默认true
    options.Password.RequireUppercase = true;// 要求大写字母 ，默认true
    options.Password.RequiredLength = 6;//要求密码最小长度，   默认是 6 个字符
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);//锁定时长，默认是 5 分钟
    options.Lockout.MaxFailedAccessAttempts = 5; //登录错误最大尝试次数，默认 5 次
    options.Lockout.AllowedForNewUsers = true;// 新用户锁定账户, 默认true

    // User settings.
    //允许的用户名字符，默认是 abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;//要求Email唯一

    options.SignIn.RequireConfirmedEmail = false;//要求激活邮箱., 默认false
    options.SignIn.RequireConfirmedPhoneNumber = false;//要求激活手机号才能登录，默认false

});
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("BadgeEntry", );
//});
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

#region 初始化话数据库
try
{
    WebAppContext.Init(app.Services);
    using var serviceScope = app.Services.CreateScope();
    var context = serviceScope.ServiceProvider.GetService<IdentityContext>();
    var userManager = serviceScope.ServiceProvider.GetService<UserManager<NoteUser>>();
    var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<NoteRole>>();
    InitIdentityDB.Initialize(context, userManager, roleManager).Wait();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message + "----An error occurred while seeding the database.");
}
#endregion
app.UseStaticFiles();
app.UseMiddleware<AuthorityMiddleware>();
app.UseRouting();
app.UseAuthentication(); //UseAuthentication 将身份验证 中间件 添加到请求管道。
app.UseAuthorization();
app.MapControllers();

app.Run();
