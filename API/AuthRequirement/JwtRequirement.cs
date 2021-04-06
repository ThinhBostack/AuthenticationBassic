using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.AuthRequirement
{
    public class JwtRequirement : IAuthorizationRequirement
    {

    }

    public class JwtRequirementHandler : AuthorizationHandler<JwtRequirement>
    {
        private readonly HttpClient _client;
        private readonly HttpContext _httpContext;

        //We need to comunicate to the server, to take the access_token
        //The API does not know anything about signing token, just receive
        //We need to have access to the access_token, from httpContext
        public JwtRequirementHandler(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _httpContext = httpContextAccessor.HttpContext; //Get HttpContext
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            JwtRequirement requirement)
        {
            if (_httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                //Extract token
                var accessToken = authHeader.ToString().Split(' ')[1];

                //Send token to server /oauth/validate through query
                var response = await _client
                    .GetAsync($"https://localhost:5001/oauth/validate?access_token={accessToken}");
                
                //If result is 200Ok
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Pass through the requirement
                    context.Succeed(requirement);
                }
            }
        }
    }
}
