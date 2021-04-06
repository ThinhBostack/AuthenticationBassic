using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationBassic.CustomPolicyProvider
{
    public class SecurityLevelAttriute : AuthorizeAttribute
    {
        public SecurityLevelAttriute(int level)
        {
            Policy = $"{DynamicPolicies.SecurityLevel}.{level}";
        }
    }

    //{type}
    public static class DynamicPolicies
    {
        public static IEnumerable<string> Get()
        {
            //about yield: https://viblo.asia/p/tu-khoa-yield-trong-c-va-cong-dung-tuyet-voi-cua-no-eW65GRXjlDO
            yield return SecurityLevel;
            yield return Rank;
        }
        //SecurityLevel = "SecurityLevel" ==== {type}.{value}
        public const string SecurityLevel = "SecurityLevel";
        public const string Rank = "Rank";
    }

    public static class DynamicAuthorizationPolicyFactory
    {
        public static AuthorizationPolicy Create(string policyName)
        {
            //Split {type}.{value}
            var parts = policyName.Split('.');
            var type = parts.First();
            var value = parts.Last();

            switch (type)
            {
                case DynamicPolicies.Rank:
                    return new AuthorizationPolicyBuilder()
                        .RequireClaim("Rank", value)
                        .Build();

                case DynamicPolicies.SecurityLevel:
                    return new AuthorizationPolicyBuilder()
                        .AddRequirements(new SecurityLevelRequirement(Convert.ToInt32(value)))
                        .Build();

                default:
                    return null;
            }
        }
    }

    public class SecurityLevelRequirement : IAuthorizationRequirement
    {
        public int Level { get; }
        public SecurityLevelRequirement(int level)
        {
            Level = level;
        }
    }

    //Handler for SecurityLevelRequirement
    public class SecurityLevelHandler : AuthorizationHandler<SecurityLevelRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            SecurityLevelRequirement requirement)
        {
            var claimValue = Convert.ToInt32(context.User.Claims
                .FirstOrDefault(x => x.Type == DynamicPolicies.SecurityLevel)?
                .Value?? "0"); //If there is no value, then Value = 0
            //If user has claim "SecurityLevel" > requirement
            if (claimValue >= requirement.Level)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;            
        }
    }

    public class CustomAuthorizationPolicyProvider
    : DefaultAuthorizationPolicyProvider
    {
        //IOptions<AuthorizationOptions> options: all the policy that being configed in Start up
        //Based on them, we have a ctor for CustomAuthorizationPolicyProvider
        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {

        }

        //Is called when resulting a policy
        //In Start up, after add policy in config authorization, this method will get the policy name
        //to return the 'AuthorizationPolicy', to take the requirement
        //Based on requirement, then go to HandleRequirementAsync

        //Override this method in order to make it dynamically return AuthorizationPolicy based on policyName 
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            //{type}.{value}
            foreach (var customPolicy in DynamicPolicies.Get())
            {
                //If all the suitable policies are found out
                if (policyName.StartsWith(customPolicy))
                {
                    //Build AuthorizationPolicyBuilder
                    var policy = DynamicAuthorizationPolicyFactory.Create(policyName);
                    return Task.FromResult(policy);
                }
            }
            return base.GetPolicyAsync(policyName);
        }
    }
}
