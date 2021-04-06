using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthenticationBassic.Transformer
{
    //Support multiple claims-based identities
    public class ClaimTransformation : IClaimsTransformation
    {
        //This method is run whenever authorization is on the need   
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var hasFriend = principal.Claims.Any(x => x.Type == "Friend");

            if (!hasFriend)
            {
                //ClaimsIdentity is extended from IIdentity interface
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("Friend", "Bad"));
            }
            return Task.FromResult(principal);            
        }        
    }
}
