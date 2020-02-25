using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace ClientApi.Authorization
{
    public class RbacAuthorizationHandler : AuthorizationHandler<RbacRequirement>
    {
        private readonly ILogger<RbacAuthorizationHandler> _logger;

        public RbacAuthorizationHandler(ILogger<RbacAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RbacRequirement requirement)
        {
            _logger.LogWarning($"Evaluating RBAC authorization requirement for {requirement?.Setting}");

            var requirementPresentInJwt = (
                from c in context.User.Claims 
                where c.Type == "rbac" 
                    && c.Value == requirement.Setting select 1
            ).Any();
            
            if (requirementPresentInJwt)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
