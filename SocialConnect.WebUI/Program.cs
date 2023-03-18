using Microsoft.EntityFrameworkCore;
using SocialConnect.Infrastructure.Interfaces;
using SocialConnect.Infrastructure.Data;
using SocialConnect.Domain.Services;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Domain.Entities;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
string connectionString = builder.Configuration.GetConnectionString("SocialDbContext");
builder.Services.AddDbContext<SocialDbContext>(context => context.UseSqlServer(connectionString));

// Identity
builder.Services.AddDefaultIdentity<User>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<SocialDbContext>();

// Authentication & Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IAccountService, AccountRepository>();
builder.Services.AddScoped<IEmailService, EmailRepository>();

// Automapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
