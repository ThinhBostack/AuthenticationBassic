using AuthenticationBassic.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using AuthenticationBassic.AuthorizationRequirement;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Authorization;
using AuthenticationBassic.Controllers;
using Microsoft.AspNetCore.Authentication;
using AuthenticationBassic.Transformer;
using AuthenticationBassic.CustomPolicyProvider;

namespace AuthenticationBassic
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Db available                      
            services.AddDbContext<AppDbContext>(config =>
            {
                config.UseInMemoryDatabase("Memory"); //Memory is name of Db
            });
            #endregion

            #region Config Password
            /*services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                #region Password config
                config.Password.RequiredLength = 3;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.SignIn.RequireConfirmedEmail = true;
                #endregion
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); //Provide token*/
            #endregion

            #region Create cookies to authenticate bonus identity
            /*services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.LoginPath = "/Home/Login";
            });*/
            #endregion

            #region Create cookies to authenticate
            //Create cookies to authenticate
            services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", config =>
                {
                    config.Cookie.Name = "Grandmas.Cookie";
                    config.LoginPath = "/Home/Authenticate";
               });
            #endregion

            #region Build Default Policy
            /*See if it is suitable to allow user to get in
            services.AddAuthorization(config =>
                {
                    var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                    var defaultAuthPolicy = defaultAuthBuilder //Build policy
                    .RequireAuthenticatedUser()
                    .RequireRole()
                    .RequireClaim(ClaimTypes.DateOfBirth)
                    //...Add more requirements here
                    .Build(); //Build

                    config.DefaultPolicy = defaultAuthPolicy;
                });*/
            #endregion

            #region Build Policy with own handler
            /*services.AddAuthorization(config =>
                {
                    config.AddPolicy("Claim.DoB", policyBuilder =>
                    {
                        //Add requirement
                        policyBuilder.RequireCustomClaim(ClaimTypes.DateOfBirth);
                    });
                    config.AddPolicy("Admin", policyBuilder =>
                    {
                        //Add requirement with exact parameter
                        policyBuilder.RequireClaim(ClaimTypes.Role,"Admin"); 
                    });

                });*/
            #endregion

            #region Dependency Injection
            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, SecurityLevelHandler>();
            //Override method of IAuthorizationHandler, config requirement
            services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();
            //Override method of IAuthorizationHandler, config authorization handler
            services.AddScoped<IAuthorizationHandler, CookieJarAuthorizationHandler>();
            ////Override method of IClaimsTransformation
            services.AddScoped<IClaimsTransformation, ClaimTransformation>();
            #endregion

            #region MailKit
            /*
            //'GetSection("Email")': config in appsettings.json
            //'.Get<MailKitOptions>()': get MailKitOptions based on info configured(F12 for more)            
            var mailKitOptions = _config.GetSection("Email").Get<MailKitOptions>(); 
            //Add MailKit into app as an emailSender
            services.AddMailKit(config => config.UseMailKit(mailKitOptions));*/
            #endregion

            #region Authorization Filter(build policy through filter)
            /*services.AddControllersWithViews(config =>
            {
                var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                var defaultAuthPolicy = defaultAuthBuilder //Build policy
                .RequireAuthenticatedUser()                
                .RequireClaim(ClaimTypes.DateOfBirth)
                //...Add more requirements here
                .Build(); //Build
                
                config.Filters.Add(new AuthorizeFilter(defaultAuthPolicy));
            });*/

            services.AddControllersWithViews();
            #endregion

            //Config Razor Page
            services.AddRazorPages()
                .AddRazorPagesOptions(config =>
                {
                    //Set authorized page
                    config.Conventions.AuthorizePage("/Razorz/Secured");
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //Who are you
            app.UseAuthentication();
            //What are you allowed to do 
            //up to this line: 'app.UseAuthorization();'
            //=> bring up all the authorization policy that configured above
            //=> find requirement claim
            //=> find service to process the requirement
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {                
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
