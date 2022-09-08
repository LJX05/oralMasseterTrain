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
    options.Password.RequireDigit = true; //Ҫ�������ֽ���0-9 ֮��,  Ĭ��true
    options.Password.RequireLowercase = true;//Ҫ��Сд��ĸ,  Ĭ��true
    options.Password.RequireNonAlphanumeric = true;//Ҫ�������ַ�,  Ĭ��true
    options.Password.RequireUppercase = true;// Ҫ���д��ĸ ��Ĭ��true
    options.Password.RequiredLength = 6;//Ҫ��������С���ȣ�   Ĭ���� 6 ���ַ�
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);//����ʱ����Ĭ���� 5 ����
    options.Lockout.MaxFailedAccessAttempts = 5; //��¼��������Դ�����Ĭ�� 5 ��
    options.Lockout.AllowedForNewUsers = true;// ���û������˻�, Ĭ��true

    // User settings.
    //������û����ַ���Ĭ���� abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;//Ҫ��EmailΨһ

    options.SignIn.RequireConfirmedEmail = false;//Ҫ�󼤻�����., Ĭ��false
    options.SignIn.RequireConfirmedPhoneNumber = false;//Ҫ�󼤻��ֻ��Ų��ܵ�¼��Ĭ��false

});
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
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

#region ��ʼ�������ݿ�
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<AuthorityMiddleware>();
app.UseRouting();
app.UseAuthentication(); //UseAuthentication �������֤ �м�� ��ӵ�����ܵ���
app.UseAuthorization();
app.MapControllers();

app.Run();
