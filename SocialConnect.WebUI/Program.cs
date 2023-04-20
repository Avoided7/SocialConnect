using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Infrastructure.Interfaces;
using SocialConnect.Infrastructure.Data;
using SocialConnect.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using SocialConnect.Infrastructure.Repositories;
using SocialConnect.Infrastructure.Services;
using SocialConnect.WebUI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
string connectionString = builder.Configuration.GetConnectionString("SocialDbContext");
builder.Services.AddDbContext<SocialDbContext>(context => context.UseSqlServer(connectionString));

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SocialDbContext>();

// Authentication & Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

// Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IBlobService, BlobService>();

// Azure Blobs
string blobConnectionString = builder.Configuration.GetConnectionString("BlobConnectionString");
builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));

// Email service
builder.Services.AddSingleton<IMailKitEmailService, MailKitEmailService>();

// Automapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// SignalR
builder.Services.AddSignalR();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=News}/{action=All}");

app.MapHub<ChatHub>("/chat");
app.MapHub<NotificationHub>("/notification");

app.SeedRoles();

app.Run();
