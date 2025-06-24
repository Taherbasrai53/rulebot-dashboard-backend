using rulebot_backend.DAL.Definition;
using rulebot_backend.Business.Implementation;
using System.Text.Json.Serialization;
using rulebot_backend.BLL.Definition;
using rulebot_backend.BLL.Implementation;
using rulebot_backend.DAL.Implementation;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddTransient<IProcessRepository, ProcessRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRuleRepository, RuleRepository>();

builder.Services.AddTransient<IProcessService, ProcessService>();
builder.Services.AddTransient<IRuleService, RuleService>();


builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); 


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowLocalhost4200");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
