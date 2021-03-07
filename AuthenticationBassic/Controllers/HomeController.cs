using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace AuthenticationBassic.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IEmailService emailService;
        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }

        //Navigate to page Index.cshtml
        public IActionResult Index() => View();

        //Navigate to page Secret.cshtml with authorizing
        [Authorize]
        public IActionResult Secret() => View();

        [Authorize(Policy="Claim.DoB")]
        public IActionResult SecretPolicy() => View("Secret");

        [Authorize(Policy = "Admin")]
        public IActionResult SecretRole() => View("Secret");

        #region Create object for authorizing        
        /*public IActionResult Authenticate()
        {
            var grandClaims = new List<Claim>()
            { 
                new Claim(ClaimTypes.Name, "Bob"),
                new Claim(ClaimTypes.Email, "Bob@gmail.com"),
                new Claim(ClaimTypes.DateOfBirth, "11/11/2000"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("Grandma.Says", "Very nice boy"),
            };

            var licenseClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "Bob K Foo"),
                new Claim(ClaimTypes.Email, "Bob@gmail.com"),
                new Claim("Driving License", "A+"),
            };

            var grandmaIdentity = new ClaimsIdentity(grandClaims, "Grandma Indentity");
            var licenseIndentity = new ClaimsIdentity(licenseClaims, "License Indentity");

            var userPricipal = new ClaimsPrincipal(new[] { grandmaIdentity, licenseIndentity });

            //Sign in
            HttpContext.SignInAsync(userPricipal);

            //Redirect to Index after authorizing
            return RedirectToAction("Index");22
        }*/
        #endregion

        public IActionResult Login() => View();

        public IActionResult Error() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            //Login func           
            var user = await this.userManager.FindByNameAsync(userName);

            //If userName is existed
            if (user != null)
            {
                //Sign in
                var signInResult = await this.signInManager.PasswordSignInAsync(user, password, false, false);
                /*Cac parameter: 
                  user, 
                  password, 
                  isPersistent(bool): period of time that cookies exist, need to inform user when using
                  lockoutOnFailure(bool): counter in Db*/

                //If successful signing in
                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Error");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string password, string email)
        {
            //Register func
            //Create an identity user
            var user = new IdentityUser
            {
                UserName = userName,
                Email = email,
            };

            //Create a record in Db
            var result = await this.userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                #region Verify Email
                //Generate email token
                var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(RegisterVerifyEmail), "Home", new { userID = user.Id, code },
                    Request.Scheme, Request.Host.ToString());

                await this.emailService.SendAsync(email, "email verify",
                    $"<a href=\"{link}\">Verify Email</a>", true);

                return RedirectToAction("EmailVerification");
                #endregion

                /*
                //Register 
                var registerResult = await this.signInManager.PasswordSignInAsync(user, password, false, false);

                //If successful registering 
                if (registerResult.Succeeded)
                {
                    return RedirectToAction("Login");
                }*/
            }

            return RedirectToAction("Error");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await this.userManager.FindByEmailAsync(email);
            if (user != null)
            {
                #region Verify Email
                //Generate email token
                var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ForgotPasswordVerifyEmail), "Home", new { userID = user.Id, code },
                    Request.Scheme, Request.Host.ToString());

                await this.emailService.SendAsync(email, "email verify",
                    $"<a href=\"{link}\">Verify Email</a>", true);

                return RedirectToAction("EmailVerification");
                #endregion
            }

            return RedirectToAction("Error");
        }

        //After creating email token, sending noti to guest's email
        //==> guest confirm email
        //==> we take back the token, also redirect guest to next page
        //==> but how do we know the exactly guest that we sent token ??
        public async Task<IActionResult> RegisterVerifyEmail(string userID, string code)
        {
            //Find the user who verify email by ID
            var user = await this.userManager.FindByIdAsync(userID);
            if (user == null) return BadRequest();

            //Confirm whether user verify email or not
            var result = await this.userManager.ConfirmEmailAsync(user, code);
            //After success confirmation from user, take them back to Login Page
            if (result.Succeeded)
            {
                return View();
            }

            return BadRequest();
        }

        public async Task<IActionResult> ForgotPasswordVerifyEmail(string userID, string code)
        {
            //Find the user who verify email by ID
            var user = await this.userManager.FindByIdAsync(userID);
            if (user == null) return BadRequest();

            //Confirm whether user verify email or not
            var result = await this.userManager.ConfirmEmailAsync(user, code);
            //After success confirmation from user, take them back to Login Page
            if (result.Succeeded)
            {
                return View();
            }

            return BadRequest();
        }

        public IActionResult ResetPassword(string userID, string code)
        {
            //Reset password 
            return View("Login");
        }

        public IActionResult EmailVerification() => View();

        public async Task<IActionResult> LogOut()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

    }
}
