using LeaveManagementSystem.Web.Data;
using LeaveManagementSystem.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure EF Core to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; 
    })
    .AddRoles<IdentityRole>()                         
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    var db = sp.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await SeedDataAsync(sp);
}

static async Task SeedDataAsync(IServiceProvider sp)
{
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Employee", "Supervisor", "Administrator" };

    foreach (var name in roles)
    {
        if (!await roleMgr.RoleExistsAsync(name))
        {
            var result = await roleMgr.CreateAsync(new IdentityRole(name));
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new Exception($"Role seed failed for '{name}': {msg}");
            }
        }
    }

    var config = sp.GetRequiredService<IConfiguration>();

    var email = config["AdminUser:Email"];
    var password = config["AdminUser:Password"];

    var admin = await userMgr.FindByEmailAsync(email);
    if (admin is null)
    {
        admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var createUser = await userMgr.CreateAsync(admin, password);
        if (!createUser.Succeeded)
        {
            var msg = string.Join("; ", createUser.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new Exception($"Admin user seed failed: {msg}");
        }
    }

    if (!await userMgr.IsInRoleAsync(admin, "Administrator"))
    {
        var addToRole = await userMgr.AddToRoleAsync(admin, "Administrator");
        if (!addToRole.Succeeded)
        {
            var msg = string.Join("; ", addToRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new Exception($"AddToRole failed: {msg}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
