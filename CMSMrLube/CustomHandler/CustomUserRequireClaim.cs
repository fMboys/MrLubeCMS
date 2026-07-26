using Microsoft.AspNetCore.Authorization;

namespace MrLubeCMS.CustomHandler
{
    public class CustomUserRequireClaim : IAuthorizationRequirement
    {
        public string CalimType { get; }
        public CustomUserRequireClaim(string claimType)
        {
            CalimType = claimType;  
        }
    }
}
