using ComponentSpace.Saml2.Configuration;
using ComponentSpace.Saml2.Configuration.Database;
using ComponentSpace.Saml2.Utility;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EMSuite.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EMSuite.Controllers
{
    public class SamlConfigurationController : Controller
    {
        private readonly SamlConfigurationContext _context;

        public SamlConfigurationViewModel model = new SamlConfigurationViewModel();


        public SamlConfigurationController(SamlConfigurationContext _context)
        {
            this._context = _context;
        }

       

        //public void OnGet()
        //{
        //    DatabaseSeeded = samlConfigurationContext.SamlConfigurations.Count() > 0;

        //    string licenseType;

        //    if (license.IsLicensed)
        //    {
        //        licenseType = "Licensed";
        //    }
        //    else
        //    {
        //        licenseType = $"Evaluation (Expires {license.Expires.ToShortDateString()})";
        //    }

        //    ProductInformation = $"ComponentSpace.Saml2, Version={license.Version}, {licenseType}";
        //}

        // GET: SamlConfigurations
        [HttpGet]
        public async Task<IActionResult> Index(SamlConfigurationViewModel model)
        {

            var data = _context.SamlConfigurations
                .Include(p => p.PartnerIdentityProviderConfigurations)
                .First().PartnerIdentityProviderConfigurations.ToList();

            return View(data);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SamlConfigurationViewModel model)
        {


            //if (_context.SamlConfigurations.Count() != 0)
            //{
            //try
            //{

            try
            {
                

                var data = _context.SamlConfigurations
                .Include(i => i.LocalServiceProviderConfiguration).Where(l => l.Id == 2027).First();


                if (data != null)
                {


                    var partnerIdentityProvider = new PartnerIdentityProviderConfiguration()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        SignAuthnRequest = true,
                        SignLogoutRequest = true,
                        SignLogoutResponse = true,
                        SingleSignOnServiceUrl = model.SingleSignOnServiceUrl,
                        SingleLogoutServiceUrl = model.SingleLogoutServiceUrl,
                        PartnerCertificates = new List<Certificate>()
                            {
                                new Certificate()
                                {
                                    FileName = model.FileName
                                }
                            }

                    };


                    data.PartnerIdentityProviderConfigurations.Add(partnerIdentityProvider);
                    _context.SaveChanges();
                    //return RedirectToAction("/Index");

                }
            }
            catch (NullReferenceException ex)
            {
                return new EmptyResult();
            }
            

            return View(model);
        }
 
     
            //samlConfiguration.LocalServiceProviderConfiguration = new LocalServiceProviderConfiguration()
            //{
            //    Name = "https://EMSuite",
            //    Description = "EMSuite",
            //    AssertionConsumerServiceUrl = "https://localhost:44306/SAML/AssertionConsumerService",
            //    SingleLogoutServiceUrl = "https://localhost:44306/SAML/SingleLogoutService",
            //    LocalCertificates = new List<Certificate>()
            //        {
            //            new Certificate()
            //            {
            //                FileName = "certificates/sp.pfx",
            //                Password = "password"
            //            }
            //        }
            //};

            //samlConfiguration.PartnerIdentityProviderConfigurations = new List<PartnerIdentityProviderConfiguration>()
            //{
            //        new PartnerIdentityProviderConfiguration()
            //        {
            //            Name = "http://www.okta.com/exk3n5fc0drZ7m57X697",
            //            Description = "Okta",
            //            SignAuthnRequest = true,
            //            SignLogoutRequest = true,
            //            SignLogoutResponse = true,
            //            SingleSignOnServiceUrl = "https://trial-4007665.okta.com/app/trial-4007665_emsuite_1/exk3n5fc0drZ7m57X697/sso/saml",
            //            SingleLogoutServiceUrl = "https://trial-4007665.okta.com/app/trial-4007665_emsuite_1/exk3n5fc0drZ7m57X697/slo/saml",
            //            PartnerCertificates = new List<Certificate>()
            //            {
            //                new Certificate()
            //                {
            //                    FileName = "certificates/okta.cer"
            //                }
            //            }
            //        },
            //        new PartnerIdentityProviderConfiguration()
            //        {
            //            Name = "https://app.onelogin.com/saml/metadata/a79374a7-67e0-4a7a-9abc-3ce9a38b4ab1",
            //            Description = "OneLogin",
            //            SignAuthnRequest = true,
            //            SignLogoutRequest = true,
            //            SignLogoutResponse = true,
            //            SingleSignOnServiceUrl = "https://ellab.onelogin.com/trust/saml11/http-post/sso/a79374a7-67e0-4a7a-9abc-3ce9a38b4ab1",
            //            SingleLogoutServiceUrl = "https://ellab.onelogin.com/trust/saml2/http-redirect/slo/1938103",
            //            PartnerCertificates = new List<Certificate>()
            //            {
            //                new Certificate()
            //                {
            //                    FileName = "certificates/onelogin.cer"
            //                }
            //            }
            //        }
            // };

            // _context.SamlConfigurations.Add(samlConfiguration);
            //_context.SaveChanges();

          
        

        // GET: SamlConfigurations/Edit/5
        //[HttpGet]
        //public async Task<IActionResult> Edit(int? id, SamlConfigurationViewModel model)
        //{
        //    if (id == null || _context.SamlConfigurations == null)
        //    {
        //        return NotFound();
        //    }

        //    var samlConfiguration = _context.SamlConfigurations.FindAsync(id);
        //    if (samlConfiguration == null)
        //    {
        //        return NotFound();
        //    }
        //    //ViewData["PartnerIdentityProviderConfigurationId"] = new SelectList(_context.PartnerIdentityProviderConfigurations, "Id", "Id", model.Id);
        //    //ViewData["LocalServiceProviderConfigurationId"] = new SelectList(_context.LocalServiceProviderConfigurations, "Id", "Id", samlConfiguration.LocalServiceProviderConfigurationId);
        //    return View(model);
        //}

        // POST: SamlConfigurations/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SamlConfigurationViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                var samlConfiguration = new SamlConfiguration()
                {
                    //LocalServiceProviderConfiguration = new LocalServiceProviderConfiguration()
                    //{
                    //    Name = "https://EMSuite",
                    //    Description = "",
                    //    AssertionConsumerServiceUrl = "https://localhost:44306/SAML/AssertionConsumerService",
                    //    SingleLogoutServiceUrl = "https://localhost:44306/SAML/SingleLogoutService",
                    //    ArtifactResolutionServiceUrl = "https://localhost:44306/SAML/ArtifactResolutionService",
                    //    LocalCertificates = new List<Certificate>()
                    //    {
                    //        new Certificate()
                    //        {
                    //            FileName = "certificates/sp.pfx",
                    //            Password = "password"
                    //        }
                    //    }
                    //},
                    PartnerIdentityProviderConfigurations = new List<PartnerIdentityProviderConfiguration>()
                        {
                            new PartnerIdentityProviderConfiguration()
                            {
                                Id = model.Id,
                                Name = model.Name,
                                Description = model.Description,
                                SignAuthnRequest = true,
                                SignLogoutRequest = true,
                                SignLogoutResponse = true,
                                SingleSignOnServiceUrl = model.SingleSignOnServiceUrl,
                                SingleLogoutServiceUrl = model.SingleLogoutServiceUrl,
                                PartnerCertificates = new List<Certificate>()
                                {
                                    new Certificate()
                                    {

                                        FileName = model.FileName
                                    }
                                }
                            }
                        }
                };

                _context.SamlConfigurations.Update(samlConfiguration);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "SamlConfiguration");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SamlConfigurationExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    ViewData["Title"] = "error";
                }
            }

            return View(model);
        }

        // GET: SamlConfigurations/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id, SamlConfigurationViewModel model)
        {
            if (id == null || _context.SamlConfigurations == null)
            {
                return NotFound();
            }


            await _context.SamlConfigurations
            .Include(p => p.PartnerIdentityProviderConfigurations)
            //.Include(p => p.PartnerIdentityProviderConfigurations)
            .FirstOrDefaultAsync(p => p.Id == id);


            return View(model);
        }

        //POST: SamlConfigurations/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, SamlConfigurationViewModel model, SamlConfiguration samlConfiguration)
        {

            var idp = _context.SamlConfigurations.Find(2027);

            _context.SamlConfigurations.Remove(idp);
            _context.SaveChanges();

            return View(model);
        }




        private bool SamlConfigurationExists(int id)
        {
            return _context.SamlConfigurations.Any(e => e.Id == id);
        }


    }
    }

