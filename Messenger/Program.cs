using Messenger;
using Microsoft.AspNetCore.Rewrite;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

builder.WebHost.UseKestrel(options =>
{
    var configuration = builder.Configuration;

    var mode = builder.Environment.IsDevelopment() ? "Private" : "Public";

    var cert = configuration![$"Kestrel:Certificates:{mode}:Path"];
    var certPassword = configuration![$"Kestrel:Certificates:{mode}:Password"];

    options.ListenAnyIP(443, listenOptions => listenOptions.UseHttps(cert, certPassword));
});

builder.Services.AddControllers();


builder.Services
    .AddEndpointsApiExplorer()
    .AddOptions()
    .AddDbContext<MessengerContext>(options => { })
    .AddMessenger()
    .AddRouting(options => options.LowercaseUrls = true)

    .AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "API ��� �����������", Version = "v1" });
    options.AddSecurityDefinition("Bearer", AddAuthHeaderOperationFilter.BearerScheme);

    options.OperationFilter<AddAuthHeaderOperationFilter>();
    
    
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
})

    .AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = JwtTokenStatics.Issuer,
        ValidAudience = JwtTokenStatics.Audience,
        ValidateAudience = true,
        ValidateLifetime = true,

        IssuerSigningKey = JwtTokenStatics.SecurityKey,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddLogging(options => options.AddConsole());

var app = builder.Build();
var options = new RewriteOptions();
options.AddRedirect("^$", "swagger");
app.UseRewriter(options);

app.UseSwagger();
app.UseSwaggerUI();

app.UseWebSockets(new WebSocketOptions() { KeepAliveInterval = TimeSpan.FromSeconds(10) });

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();
app.Run();
