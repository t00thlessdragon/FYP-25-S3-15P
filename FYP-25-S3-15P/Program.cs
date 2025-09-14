using FYP_25_S3_15P.Data;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<SmartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Smart")));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();