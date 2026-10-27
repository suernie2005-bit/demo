using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Authorization;

public sealed class BearerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        bool allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any()
            || HasAttribute<AllowAnonymousAttribute>(context);
        bool requiresAuthorization = endpointMetadata.OfType<IAuthorizeData>().Any()
            || HasAttribute<AuthorizeAttribute>(context)
            || IsCatalogWriteOperation(context.ApiDescription);

        if (allowsAnonymous || !requiresAuthorization)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = new List<string>()
        });
    }

    private static bool HasAttribute<TAttribute>(OperationFilterContext context)
        where TAttribute : Attribute
    {
        return context.MethodInfo.IsDefined(typeof(TAttribute), inherit: true)
            || context.MethodInfo.DeclaringType?.IsDefined(typeof(TAttribute), inherit: true) == true;
    }

    private static bool IsCatalogWriteOperation(ApiDescription description)
    {
        string? path = description.RelativePath;
        string? method = description.HttpMethod;

        return method is not null
            && (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
                || method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
            && path is not null
            && (path.StartsWith("api/loai", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("api/sanpham", StringComparison.OrdinalIgnoreCase));
    }
}
