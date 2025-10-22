using LeaveManagementSystem.Web.Data;
using LeaveManagementSystem.Web.Services.LeaveAllocations;
using LeaveManagementSystem.Web.Services.LeaveTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure EF Core to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveAllocationsService, LeaveAllocationsService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
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
    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var config = sp.GetRequiredService<IConfiguration>();

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

    
    var email = config["AdminUser:Email"];
    var password = config["AdminUser:Password"];

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        throw new Exception("Missing AdminUser:Email or AdminUser:Password in configuration/UserSecrets.");

    var admin = await userMgr.FindByEmailAsync(email);

    if (admin is null)
    {
        admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = "Default", LastName = "Admin", DateOfBirth = new DateOnly(1990, 1, 1)
        };
        var createUser = await userMgr.CreateAsync(admin, password);
        if (!createUser.Succeeded)
        {
            var msg = string.Join("; ", createUser.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new Exception($"Admin user seed failed: {msg}");
        }
    }

    else
    {
        bool changed = false;
        if (string.IsNullOrWhiteSpace(admin.FirstName)) { admin.FirstName = "Default"; changed = true; }
        if (string.IsNullOrWhiteSpace(admin.LastName)) { admin.LastName = "Admin"; changed = true; }
        if (admin.DateOfBirth is null) { admin.DateOfBirth = new DateOnly(1990, 1, 1); changed = true; }

        if (changed)
        {
            var upd = await userMgr.UpdateAsync(admin);
            if (!upd.Succeeded)
                throw new Exception("Admin user update failed: " +
                    string.Join("; ", upd.Errors.Select(e => $"{e.Code}:{e.Description}")));
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
