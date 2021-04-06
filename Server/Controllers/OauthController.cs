using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class OauthController : Controller
    {
        [HttpGet]
        public IActionResult Authorize(
            string respone_type, // auth flow type
            string client_id, // client identify
            string redirect_uri, // redirect to only specific redirect uri
            string scope, // what info i want = email, tel,...
            string state) //random string generated to
                          //confirm that we are going to back to the same client

        {
            var query = new QueryBuilder();
            query.Add("redirect_uri", redirect_uri);
            query.Add("state", state);
            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string userName,
            string redirect_uri,
            string state)
        {
            const string code = "BSBSBS";

            var query = new QueryBuilder();
            query.Add("code", code);
            query.Add("state", state);

            return Redirect($"{redirect_uri}{query.ToString()}");
        }

        public async Task<IActionResult> Token(
            string grant_type, //flow of accessToken 
            string code, //confirmation for authentication process
            string redirect_uri,
            string client_id)
        {
            //Mechanism for validating the code

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_ID"),
                new Claim("granny", "cookie")
            };

            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                Constants.Issuer,
                Constants.Audience,
                claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(1),
                signingCredentials
                );

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                tokenType = "Bearer",
                raw_claim = "oauthTutorial"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            await Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);

            return Redirect(redirect_uri);
        }

        //Validate access_token from API\AuthRequirement\JwtRequirement.cs
        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                //Perform more check...
                return Ok();
            }
            return BadRequest();
        }
    }
}
