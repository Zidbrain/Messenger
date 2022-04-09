using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Messenger;

public class AddAuthHeaderOperationFilter : IOperationFilter
{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.ApiDescription.CustomAttributes().OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

           // operation.Security = new List<OpenApiSecurityRequirement>();
            //Add JWT bearer type
            operation.Security.Add(new OpenApiSecurityRequirement()
            {
                [OpenApiReference] = new List<string>()
            });
        }
    }

    public static OpenApiSecurityScheme BearerScheme { get; } =
        new()
        {
            In = ParameterLocation.Header,
            Description = "JWT-токен для авторизации",
            Name = "authorization",
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Flows = new OpenApiOAuthFlows
            {
                ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri("/auth"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "/", "Api" }
                    }
                }
            }
        };

    public static OpenApiSecurityScheme OpenApiReference { get; } =
        new()
        {
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Name = "Bearer",
            In = ParameterLocation.Header,
        };
}
