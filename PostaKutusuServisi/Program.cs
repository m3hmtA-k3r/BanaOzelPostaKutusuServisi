using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.CustomValidation;
using PostaKutusuServisi.Entities;
using Microsoft.AspNetCore.Identity;
using PostaKutusuServisi.Services;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;

    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    opt.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<AppDbContext>()
        .AddErrorDescriber<CustomErrorDescriber>()
        .AddDefaultTokenProviders();


builder.Services.AddAuthentication();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Kullanıcı giriş yapmamışsa yönlendirilecek sayfa
    options.LogoutPath = "/Auth/Logout"; // Kullanıcı çıkış yaparken yönlendirilecek sayfa
    options.Cookie.Name = "PostaKutusuServisiCookie"; // Çerezin adi
});

//if (builder.Environment.IsDevelopment())
//{
//    builder.Services.AddScoped<IMailService, FileMailService>();
//}
//else  //bunu production ortamında kullanmak için açabilirsin
//{
//    builder.Services.AddScoped<IMailService, SmtpMailService>();
//}

builder.Services.AddScoped<IMailService, SmtpMailService>();


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//  ROL tanımlama => CreateScope() neden gerekli? RoleManager ve UserManager scoped servisleri oldugu için
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string[] roles = { "Admin", "User" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = roleName });
        }
    }

    var adminEmail = app.Configuration["AdminEmail"];

    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
