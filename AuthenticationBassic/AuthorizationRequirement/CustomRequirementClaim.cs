using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationBassic.AuthorizationRequirement
{
    //This class is used for taking request claim
    //Answer the ques: Are you allowed to pass here ?
    public class CustomRequirementClaim : IAuthorizationRequirement
    {
        public CustomRequirementClaim(string claimType)
        {
            ClaimType = claimType;
        }
        public string ClaimType { get; }
    }

    //This class handles the authorization request  
    //Output the result of claim: true/false
    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequirementClaim>
    {
        public CustomRequireClaimHandler()
        {

        }
        //HandleRequirementAsync: make a decision if authorization is allowed based on requirement or not
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomRequirementClaim requirement)
        {
            //'context': contains authorization information
            //'requirement': ClaimType with which must be match
            //Return true/false whether user pass the claim or not
            var hasClaim = context.User.Claims.Any(x => x.Type == requirement.ClaimType);
            if (hasClaim)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireCustomClaim(
            this AuthorizationPolicyBuilder builder,
            string claimType)
        {
            builder.AddRequirements(new CustomRequirementClaim(claimType));
            return builder;
        }        
    }
}
