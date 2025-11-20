using System.Threading.Tasks;
using JT808.Gateway.Abstractions.Configurations;
using JT808.Gateway.Abstractions.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JT808.Gateway.Authorization;

/// <summary>
/// 鉴权过滤器
/// </summary>
internal class JT808TokenFilter : IEndpointFilter
{
    const string key = "token";

    /// <inheritdoc/>
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var anticipatedToken = context.HttpContext.RequestServices.GetRequiredService<IOptions<JT808Configuration>>().Value.WebApiToken;

        if ((context.HttpContext.Request.Query.TryGetValue(key, out var value) || context.HttpContext.Request.Headers.TryGetValue(key, out value)) && !string.IsNullOrEmpty(value) && value == anticipatedToken)
        {
            return await next(context);
        }

        return Results.Ok(new JT808ResultDto<string>
        {
            Code = 401,
            Message = "auth error",
            Data = "auth error"
        });
    }
}