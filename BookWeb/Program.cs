using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Models.Momo;
using BookWeb.Repositories;
using BookWeb.Services.Mail;
using BookWeb.Services.Momo;
using BookWeb.Services.Vnpay;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 30/11 - Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Add services to the container.
// Shopping Cart------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DB----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ID----------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();


//Đoàn duy đẹp trai  iệt hả ?


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    //options.CallbackPath = "/signin-google";
});

// MAIL SERVER ------------------
// Cấu hình dịch vụ gửi mail, giá trị Inject từ appsettings.json
builder.Services.AddOptions();                                              // Kích hoạt Options
var mailSettingsSection = builder.Configuration.GetSection("MailSettings"); // đọc config
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));    // đăng ký để Inject
builder.Services.AddTransient<IEmailSender, SendMailService>();             // Đăng ký dịch vụ Mail

// ------------------------------
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IBookRepo, EFBookRepo>();
builder.Services.AddScoped<ICategoryRepo, EFCategoryRepo>();
builder.Services.AddScoped<IFavoriteBookRepository, EFFavoriteBookRepository>();
builder.Services.AddScoped<ICommentRepository, EFCommentRepository>();
builder.Services.AddScoped<IRatingRepository, EFRatingRepository>();
builder.Services.AddScoped<IOrderRepo, EFOrderRepo>();

// 01/12 - Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // Không giới hạn kích thước tải lên
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

//Use session before routing ---------------
app.UseSession();

// Routing and Authentication--------------
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// -----------------
app.MapRazorPages();

// ID---------------
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
