#if NETFRAMEWORK

using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;

namespace CLMS.Framework.Identity
{
    public class ClaimsTransformationModule : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (incomingPrincipal != null && incomingPrincipal.Identity.IsAuthenticated)
            {
                var localUserForWindowsIdentity = IdentityHelper.CreateLocalUserForWindowsIdentity(incomingPrincipal);
                if(localUserForWindowsIdentity == null)
                {
                    var logger = log4net.LogManager.GetLogger(typeof(ClaimsTransformationModule));
                    logger.Debug($"Local User for Windows Identity is Null for user: {incomingPrincipal?.Identity?.Name}");
                    return null;
                }
                CreateSession(localUserForWindowsIdentity);
                return new ClaimsPrincipal(localUserForWindowsIdentity);
            }
            else
            {
                var logger = log4net.LogManager.GetLogger(typeof(ClaimsTransformationModule));
                logger.Debug($"User {incomingPrincipal?.Identity?.Name} is not authenticated or principal is null");
            }
            return incomingPrincipal;
        }

        private void CreateSession(ClaimsPrincipal transformedPrincipal)
        {
            var sessionSecurityToken = new SessionSecurityToken(transformedPrincipal, TimeSpan.FromHours(8));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
        }
    }
}

#endif