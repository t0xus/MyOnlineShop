using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyOnlineShop.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<myonlineshopContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

// 1) Distributed Memory Cache hinzufügen
builder.Services.AddDistributedMemoryCache();

// 2) Session aktivieren und konfigurieren
builder.Services.AddSession(options =>
{
    // Session-Timeout: z. B. 30 Minuten
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    // Optional: options.Cookie.IsEssential = true; 
});


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
