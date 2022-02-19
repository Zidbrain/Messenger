using Microsoft.AspNetCore.Authentication.JwtBearer;
using Messenger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOptions();
builder.Services.AddDbContext<MessengerContext>(options =>
{
});
builder.Services.AddMessenger();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets(new WebSocketOptions() { KeepAliveInterval = TimeSpan.FromSeconds(10) });

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
