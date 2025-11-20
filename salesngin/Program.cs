
var builder = WebApplication.CreateBuilder(args);
string webRootPath = builder.Environment.WebRootPath;
string contentRootPath = builder.Environment.ContentRootPath;
// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
string databaseProvider = builder.Configuration.GetSection("DatabaseProvider").Value;
string connectionType = builder.Configuration.GetSection("ConnectionType").Value;
string primaryDatabase = builder.Configuration.GetConnectionString("PrimaryDatabase") ?? throw new InvalidOperationException("Connection string [ PrimaryDatabase ] not found.");
int sessionTimeoutMinutes = builder.Configuration.GetValue<int>("Session:TimeoutMinutes");
if (connectionType == "encoded" || connectionType == "secured" || connectionType == "1")
{
    primaryDatabase = StringEncryptionExtensions.DecryptText(primaryDatabase, Constants.KeyPhrase, Constants.IVPhrase);
}
// Add services to the container.
string connectionString = primaryDatabase;

switch (databaseProvider)
{
    //case "Sqlite":
    //    builder.services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
    //    break;
    case "MSSQL":
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString)
       .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug())));
        break;
    case "MySQL":
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySQL(connectionString)
        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug())));
        break;
    default:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySQL(connectionString)
        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug())));
        break;
}
// connect to Pomelo MySQL with connection string from app settings
//builder.Services.AddDbContext<ApplicationDbContext>(dbContextOptions => dbContextOptions.UseMySQL(connectionString, ServerVersion.AutoDetect(connectionString)));
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySQL(connectionString)
//        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug())));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmationTP")
                .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.Configure<IdentityOptions>(options =>
{
    //Set custom Email Confirmation Token lifespan
    options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmationTP";
    //User needs to confirm their email before they can use the system
    options.SignIn.RequireConfirmedEmail = true;
    //options.SignIn.RequireConfirmedAccount = true;
    //options.SignIn.RequireConfirmedPhoneNumber = false;
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    // Lockout settings.
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //options.Lockout.MaxFailedAccessAttempts = 5;
    //options.Lockout.AllowedForNewUsers = true;
    // User settings.
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
//Setting all tokens (password reset and change email confirmation tokens to 1 hour default is 1 day for all 
//builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(1));
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(30));
//Setting email confirmation token to 3 day (expiration period) 
builder.Services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(30));
//Configure the app's cookie in Startup.ConfigureServices. 
//ConfigureApplicationCookie must be called after calling AddIdentity or AddDefaultIdentity.
builder.Services.AddAuthentication(Constants.CookieScheme) // Use the constant variable for the scheme name
    .AddCookie(Constants.CookieScheme, options =>
    {
        // cookie authentication options 
        options.Cookie.Name = Constants.CookieScheme;
        //options.Cookie.SameSite = SameSiteMode.Lax;
        //options.Cookie.SecurePolicy = _environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
    });
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IMailService, MailService>();
// Register HttpClient
builder.Services.AddHttpClient();
//Custom Services
//builder.Services.AddTransient<IDataControllerService, DataControllerService>();
builder.Services.AddScoped<IDataControllerService, DataControllerService>();
builder.Services.AddSingleton<IUserConnectionService, UserConnectionService>();
builder.Services.AddSingleton<ISmsProviderService, SmsProviderService>();
builder.Services.AddScoped<IPhotoStorage, PhotoStorageService>();
builder.Services.AddScoped<IItemsService, ItemsService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRequestService, RequestService>();

IHtmlSanitizer sanitizer = new HtmlSanitizer();
builder.Services.AddSingleton(sanitizer);

//DNTCaptcha
builder.Services.AddDNTCaptcha(options =>
{
    options.UseSessionStorageProvider() // -> It doesn't rely on the server or client's times. Also it's the safest one.
                                        // options.UseMemoryCacheStorageProvider() // -> It relies on the server's times. It's safer than the CookieStorageProvider.
                                        //options.UseCookieStorageProvider() // -> It relies on the server and client's times. It's ideal for scalability, because it doesn't save anything in the server's memory.
                                        // .UseDistributedCacheStorageProvider() // --> It's ideal for scalability using `services.AddStackExchangeRedisCache()` for instance.
                                        // .UseDistributedSerializationProvider()

    // Don't set this line (remove it) to use the installed system's fonts (FontName = "Tahoma").
    // Or if you want to use a custom font, make sure that font is present in the wwwroot/fonts folder and also use a good and complete font!
    //.UseCustomFont(Path.Combine(Assembly.GetEntryAssembly().Location, "Fonts", "IRANSans(FaNum)_Bold.ttf"))
    .UseCustomFont(Path.Combine(webRootPath, "fonts", "GeBody-x0zj.ttf"))
    .AbsoluteExpiration(minutes: 7)
    .ShowThousandsSeparators(false)
    .WithEncryptionKey("This is my secure key!")
    .InputNames(// This is optional. Change it if you don't like the default names.
        new DNTCaptchaComponent
        {
            CaptchaHiddenInputName = "DNTCaptchaText",
            CaptchaHiddenTokenName = "DNTCaptchaToken",
            CaptchaInputName = "DNTCaptchaInputText"
        })
    .Identifier("dntCaptcha")// This is optional. Change it if you don't like its default name.
    ;
});
//HangFire
builder.Services.AddHangfire(options =>
{
    options.UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        //TransactionIsolationLevel = IsolationLevel.ReadCommitted,
        QueuePollInterval = TimeSpan.FromSeconds(15),
        JobExpirationCheckInterval = TimeSpan.FromHours(1),
        CountersAggregateInterval = TimeSpan.FromMinutes(5),
        PrepareSchemaIfNecessary = true,
        DashboardJobListLimit = 50000,
        TransactionTimeout = TimeSpan.FromMinutes(1),
        TablesPrefix = "Hangfire"
    }));

});
builder.Services.AddHangfireServer();
builder.Services.AddSignalR()
    .AddHubOptions<NotificationHub>(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(sessionTimeoutMinutes);
        //options.ClientTimeoutInterval = TimeSpan.FromMinutes(30); // Adjust this timeout as needed
        options.EnableDetailedErrors = true; // Enable detailed errors
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutMinutes);
    //options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
//builder.Services.AddHealthChecks();
//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.AddFile("Logs/applicationLogs.txt"); // Specify the path where logs will be stored
//});
//builder.Services.WebHost.UseKestrel(options => options.AddServerHeader = false);

var app = builder.Build();
// Inject IWebHostEnvironment into Program.cs
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await DbInitializer.Initialize(app);
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.MapHub<NotificationHub>("/notificationHub");
var hangFireOptions = new DashboardOptions
{
    //AppPath = "http://your-app.net" ,
    //AppPath = "/Account",
    AppPath = "/",
    Authorization = [new HangfireAuthorizationFilter()]
};
app.UseHangfireDashboard("/hangfire", hangFireOptions);

//app.UseHangfireDashboard("/hangfire", new DashboardOptions
//{
//    AppPath = "~/Account",
//    // Make `Back to site` link working for subfolder applications
//    Authorization = new[] { new HangfireAuthorizationFilter() }
//});

app.Run();

// Reset the auto-increment counter for a specific table
// _context.Database.ExecuteSqlRaw("ALTER TABLE TableName AUTO_INCREMENT = 1");
