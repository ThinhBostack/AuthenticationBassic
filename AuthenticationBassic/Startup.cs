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
            //Make it available for methods to be injected everywhere within the application             
            services.AddDbContext<AppDbContext>(config =>
            {
                config.UseInMemoryDatabase("Memory"); //Memory is name of Db
            });
            #endregion                      

            //Register identity
            //Add identity registers the services(infrastructure alllow to communicate with Db)
            services.AddIdentity<IdentityUser, IdentityRole>(config =>
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
                .AddDefaultTokenProviders(); //Provide token

            #region Create cookies to authenticate bonus identity
            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.LoginPath = "/Home/Login";
            });
            #endregion

            #region Create cookies to authenticate
            /*Create cookies to authenticate
            services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", config =>
                {
                    config.Cookie.Name = "Grandmas.Cookie";
                    config.LoginPath = "/Home/Authenticate";
               });*/
            #endregion

            #region Build DefaultPolicy
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
            services.AddAuthorization(config =>
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

                });
            //up to this line: 'app.UseAuthorization();'
            //=> bring up all the authorization policy that configured above
            //=> find requirement claim
            //=> find service to process the requirement
            services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();
            #endregion

            #region MailKit
            //'GetSection("Email")': config in appsettings.json
            //'.Get<MailKitOptions>()': get MailKitOptions based on info configured(F12 for more)            
            var mailKitOptions = _config.GetSection("Email").Get<MailKitOptions>(); 
            //Add MailKit into app as an emailSender
            services.AddMailKit(config => config.UseMailKit(mailKitOptions)); 
            #endregion

            services.AddControllersWithViews();
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
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                /*endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });*/
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
