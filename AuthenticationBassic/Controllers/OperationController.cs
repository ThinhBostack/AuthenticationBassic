using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationBassic.Controllers
{
    public class OperationController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        public OperationController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        public async Task<IActionResult> Open()
        {
            var cookieJar = new CookieJar();
            var requirement = new OperationAuthorizationRequirement()
            {
                Name = CookieJarOperations.Open
            };
            //Authorize user based on authorization requirement() for a specific resource
            await authorizationService.AuthorizeAsync(User, cookieJar, requirement);
            return View();
        }
    }

    //This class is to config authorization, WHO is allowed to do THINGS ?   
    //OperationAuthorizationRequirement: requirement
    //CookieJar: resource
    public class CookieJarAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, CookieJar>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            CookieJar cookieJar)
        {
            //If u want to look for CookieJar
            if (requirement.Name == CookieJarOperations.Look)
            {
                //If user is authenticated
                if (context.User.Identity.IsAuthenticated)
                {
                    //Mark the requirement is successfully
                    //allowed to look in to CookieJar                    
                    context.Succeed(requirement);
                }
            }
            //If u want to ComeNear CookieJar
            //ComeNear is more than Look
            else if (requirement.Name == CookieJarOperations.ComeNear)
            {
                if (context.User.HasClaim("Friend", "Good")) //Type-Value
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
    public static class CookieJarOperations
    {
        public static string Open = "Open";
        public static string TakeCookie = "TakeCookie";
        public static string ComeNear = "ComeNear";
        public static string Look = "Look";
    }

    //This class is to demo 'object resource'(null) of line 27 
    //line: authorizationService.AuthorizeAsync(User, null, requirement);
    //'object resource' is likely resource that we want to access, but in this demo, we call it CookieJar
    //Each resource is corresponding to a service
    public class CookieJar
    {
        public string Name { get; set; }
    }
}
