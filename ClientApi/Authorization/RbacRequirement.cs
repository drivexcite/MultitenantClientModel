using Microsoft.AspNetCore.Authorization;

namespace ClientApi.Authorization
{
    public class RbacRequirement : IAuthorizationRequirement
    {
        public RbacRequirement(string setting)
        {
            Setting = setting;
        }

        public string Setting { get; set; }
    }
}
