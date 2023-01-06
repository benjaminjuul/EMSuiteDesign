using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Metadata.Export;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Xml;
using EMSuite.ViewModels;
using EMSuite.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMSuite.Controllers
{
    [Route("[controller]/[action]")]
    public class SamlController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly ISamlServiceProvider _samlServiceProvider;
        private readonly IConfigurationToMetadata _configurationToMetadata;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public SamlController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserStore<ApplicationUser> userStore,
            ISamlServiceProvider samlServiceProvider,
            IConfigurationToMetadata configurationToMetadata,
            IConfiguration configuration,
            ApplicationDbContext context)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _samlServiceProvider = samlServiceProvider;
            _configurationToMetadata = configurationToMetadata;
            _configuration = configuration;
            _context = context;

        }


        public async Task<IActionResult> Index()
        {
            return View();
        }

        
        public async Task<IActionResult> InitiateSingleSignOn(ApplicationUser model, string? returnUrl = null)
        {
            //var partnerAdfs = _configuration["Adfs"];
            //var partnerAzure = _configuration["Azure"];
            //var partnerCentrify = _configuration["Centrify"];
            //var partnerGoogle = _configuration["Google"];
            var partnerOkta = _configuration["PartnerOkta"];
            var partnerOneLogin = _configuration["PartnerOneLogin"];
            // var partnerPingOne = _configuration["PingOne"];
            //var partnerSalesforce = _configuration["PartnerSalesforce"];
            // var partnerShibboleth = _configuration["Shibboleth"];



            // Retrieve the value from the input field
            string inputValue = model.Company;
            var user = _userManager.Users.FirstOrDefault(u => u.Company == inputValue);
            //var user = _context.Users.FirstOrDefault(u => u.Email == inputValue);

            if (user != null)
            {
                // The value exists in the custom column
                switch (user.IdentityProvider)
                {

                    //case "Adfs":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerAdfs, returnUrl);
                    //    break;
                    //case "Azure":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerAzure, returnUrl);
                    //    break;
                    //case "Centrify":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerCentrify, returnUrl);
                    //    break;
                    //case "Google":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerGoogle, returnUrl);
                    //    break;
                    case "Okta":
                        await _samlServiceProvider.InitiateSsoAsync(partnerOkta, returnUrl);
                        break;
                    case "OneLogin":
                        await _samlServiceProvider.InitiateSsoAsync(partnerOneLogin, returnUrl);
                        break;
                    //case "PingOne":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerPingOne, returnUrl);
                    //    break;
                    //case "Salesforce":
                    //await _samlServiceProvider.InitiateSsoAsync(partnerSalesforce, returnUrl);
                    //break;
                    //case "Shibboleth":
                    //    await _samlServiceProvider.InitiateSsoAsync(partnerShibboleth, returnUrl);
                    //    break;
                    default:
                        ViewData["Title"] = "Company Domain does not exist";
                        break;

                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "The company name or the username does not exist.");
                return new EmptyResult();
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> InitiateSingleLogout(string? returnUrl = null)
        {
            //Request logout at the identity provider.
            await _samlServiceProvider.InitiateSloAsync(relayState: returnUrl);

            return new EmptyResult();
        }

        public async Task<IActionResult> AssertionConsumerService(ApplicationUser model)
        {

            //Receive and process the SAML assertion contained in the SAML response.
            //The SAML response is received either as part of IdP-initiated or SP-initiated SSO
            var ssoResult = await _samlServiceProvider.ReceiveSsoAsync();


          
            
        
            //Automatically provision the user.
            //If the user doesn't exist locally then create the user.
            //Automatic provisioning is an optional step.

            //var user1 = await _userManager.GetUserName(ssoResult.UserID)

           var user = await _userManager.FindByNameAsync(ssoResult.UserID);
            //var user = await _userManager.FindByNameAsync(ssoResult.UserID);

           
            if (user == null)
            {
                //user = { model.Company, model.UserName};
                user = new ApplicationUser { UserName = ssoResult.UserID, Email = ssoResult.UserID };

                var result = await _userManager.CreateAsync(user);
                //user.Company = model.Company;

                if (!result.Succeeded)
                {
                    throw new Exception($"The user {ssoResult.UserID} couldn't be created - {result}");
                }

                //For demonstration purposes, create some additional claims.
                if (ssoResult.Attributes != null)
                {
                    var samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.Email);

                    if (samlAttribute != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, samlAttribute.ToString()));
                    }

                    samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.GivenName);

                    if (samlAttribute != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, samlAttribute.ToString()));
                    }

                    samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.Surname);

                    if (samlAttribute != null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, samlAttribute.ToString()));
                    }
                }
            }

            //Automatically login using the asserted identity.
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Redirect to the target URL if specified.
            if (!string.IsNullOrEmpty(ssoResult.RelayState))
            {
                return LocalRedirect(ssoResult.RelayState);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SingleLogoutService()
        {
            //Receive the single logout request or response.
            //If a request is received then single logout is being initiated by the identity provider.
            //If a response is received then this is in response to single logout having been initiated by the service provider.
            var sloResult = await _samlServiceProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                //SP - initiated SLO has completed.
                if (!string.IsNullOrEmpty(sloResult.RelayState))
                {
                    return LocalRedirect(sloResult.RelayState);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                //Logout locally.
                await _signInManager.SignOutAsync();

                //Respond to the IdP-initiated SLO request indicating successful logout.
                await _samlServiceProvider.SendSloAsync();
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> ArtifactResolutionService()
        {
            //Resolve the HTTP artifact.
            //This is only required if supporting the HTTP-Artifact binding.
            await _samlServiceProvider.ResolveArtifactAsync();

            return new EmptyResult();
        }

        public async Task<IActionResult> ExportMetadata()
        {
            var entityDescriptor = await _configurationToMetadata.ExportAsync();
            var xmlElement = entityDescriptor.ToXml();

            Response.ContentType = "text/xml";
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"metadata.xml\"");

            var xmlWriterSettings = new XmlWriterSettings()
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var xmlWriter = XmlWriter.Create(Response.Body, xmlWriterSettings))
            {
                xmlElement.WriteTo(xmlWriter);
                await xmlWriter.FlushAsync();
            }

            return new EmptyResult();
        }
    }
}
