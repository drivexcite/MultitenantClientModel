using Microsoft.AspNetCore.Authorization;

namespace ClientApi.Authorization
{
    public class AuthorizeRbacAttribute : AuthorizeAttribute
    {
        public AuthorizeRbacAttribute(string setting)
        {
            Policy = setting;
        }
    }
}
