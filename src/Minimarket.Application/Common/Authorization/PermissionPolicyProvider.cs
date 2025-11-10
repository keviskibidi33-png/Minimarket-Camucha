using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Minimarket.Application.Common.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Si la política comienza con "Permission:", crear una política dinámica
        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName.Substring("Permission:".Length).Split(':');
            if (parts.Length == 2)
            {
                var moduleSlug = parts[0];
                var permission = parts[1];

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(moduleSlug, permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // De lo contrario, usar el provider por defecto
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}

