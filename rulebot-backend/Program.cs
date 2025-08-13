using rulebot_backend.DAL.Definition;
using rulebot_backend.Business.Implementation;
using System.Text.Json.Serialization;
using rulebot_backend.BLL.Definition;
using rulebot_backend.BLL.Implementation;
using rulebot_backend.DAL.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigins",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:4200",
                "https://www.rulebot-app.optrpa.com",
                "http://49.43.224.201"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

builder.Services.AddTransient<IProcessRepository, ProcessRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRuleRepository, RuleRepository>();
builder.Services.AddTransient<IConnectionRepository, ConnectionRepository>();

builder.Services.AddTransient<IProcessService, ProcessService>();
builder.Services.AddTransient<IRuleService, RuleService>();
builder.Services.AddTransient<IConnectionService, ConnectionService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddTransient<ISubscriptionService, SubscriptionService>();


builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AspNetCore.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ensure compatibility with HTTPS
    options.Cookie.SameSite = SameSiteMode.None; // allow cross-origin
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseCors("AllowFrontendOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Log cookies BEFORE session is processed
app.Use(async (context, next) =>
{
    Console.WriteLine($"[Before Session Middleware] Path: {context.Request.Path}");
    Console.WriteLine($"Incoming Cookies: {string.Join(", ", context.Request.Cookies.Select(c => $"{c.Key}={c.Value}"))}");
    await next.Invoke();
});

// Session middleware must be here
app.UseSession();

// Log session AFTER middleware has run
app.Use(async (context, next) =>
{
    Console.WriteLine($"[After Session Middleware] Session ID: {context.Session.Id}");
    await next.Invoke();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();