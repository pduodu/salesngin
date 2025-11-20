namespace salesngin.Controllers
{
    [Authorize]
    public class AccountController(
        ApplicationDbContext context,
        IMailService mailService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IDataControllerService dataService
            ) : BaseController(context, mailService, signInManager, userManager, roleManager, webHostEnvironment, dataService)
    {

        [HttpGet]
        [TypeFilter(typeof(PasswordFilter))]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            //Get the current Logged-in user 
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            var currentYear = DateTime.UtcNow.Date.Year;
            int currentMonth = DateTime.UtcNow.Date.Month;

            year ??= currentYear;
            month ??= currentMonth;

            List<int> years = Enumerable.Range(2022, (currentYear + 3) - 2022).ToList();

            List<(int MonthNumber, string MonthName)> months =
            [
                (1, "January"),
                (2, "February"),
                (3, "March"),
                (4, "April"),
                (5, "May"),
                (6, "June"),
                (7, "July"),
                (8, "August"),
                (9, "September"),
                (10, "October"),
                (11, "November"),
                (12, "December")
            ];

            //Fetch all orders from DB
            var requestQuery = _databaseContext.Requests.AsNoTracking().AsQueryable();
            var storeItemQuery = _databaseContext.Inventory.Include(x => x.Item).AsNoTracking().AsQueryable();
            var requestedItemQuery = _databaseContext.RequestItems.Include(x => x.Request).Include(x => x.Item).AsNoTracking().Where(x => x.Request.Status == RequestStatus.Delivered).AsQueryable();

            //Filter by year
            if (year != 0)
            {
                requestQuery = requestQuery.Where(x => x.RequestDate.HasValue && x.RequestDate.Value.Year == year);
                requestedItemQuery = requestedItemQuery.Where(x => x.Request.RequestDate.HasValue && x.Request.RequestDate.Value.Year == year);
            }

            //Filter by month
            if (month != 0)
            {
                requestQuery = requestQuery.Where(x => x.RequestDate.HasValue && x.RequestDate.Value.Month == month);
                requestedItemQuery = requestedItemQuery.Where(x => x.Request.RequestDate.HasValue && x.Request.RequestDate.Value.Month == month);
            }

            requestQuery = requestQuery.OrderByDescending(x => x.RequestDate);
            requestedItemQuery = requestedItemQuery.OrderByDescending(x => x.Request.RequestDate);

            var requests = requestQuery.ToList();
            var storeItems = storeItemQuery.Where(x => x.Status == StockStatus.Available || x.Status == StockStatus.OutOfStock).ToList();
            var topRequestedItems = requestedItemQuery
                .GroupBy(ri => ri.ItemId)
                .Select(g => new RequestSummaryViewModel
                {
                    RequestId = g.Key,
                    RequestItem = g.First(),
                    TotalRequested = g.Sum(ri => ri.Quantity)
                })
                .OrderByDescending(x => x.TotalRequested)
                .Take(20)
                .ToList();

            /*int currentYear = DateTime.UtcNow.Year;

            return await _context.OrderItems
                .Where(oi => oi.Order.PaymentDate >= firstDayOfMonth && oi.Order.PaymentDate <= lastDayOfMonth)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ProductViewModel
                {
                    Id = (int)g.Key,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    TotalSold = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(20)
                .ToListAsync();

             // Generate a list of years starting from 2020 to the current year
             List<int> years = [];
             for (int year = 2020; year <= currentYear; year++)
             {
                 years.Add(year);
             }
             DashboardViewModel model = new();
             model.ActiveYear = currentYear;
             model.ActiveYears = years;

             */

            var model = new DashboardViewModel
            {
                AllRequests = requests,
                RequestsCount = requests.Count(),
                InventoryItems = storeItems,
                RequestedItems = topRequestedItems,
                //AllRequests = [.. requestQuery],
                //RequestsCount = _context.Requests.Count(),
                ItemsCount = _databaseContext.Items.Count(),
                InventoryCount = _databaseContext.Inventory.Count(),
                UsersCount = _databaseContext.Users.Count(),
                ActiveMonth = month,
                ActiveMonths = months,
                ActiveYear = year,
                ActiveYears = [.. years.OrderByDescending(n => n)],
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.Inventory_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser,
                Requests = _databaseContext.Requests
                          .OrderByDescending(r => r.RequestDate)
                          .Take(5)  // Get the latest 5 requests
                          .Select(r => new RequestViewModel
                          {
                              Id = r.Id,
                              RequestCode = r.RequestCode,
                              Purpose = r.Purpose,
                              RequestDate = r.RequestDate,
                              Status = r.Status,
                              RequestItems = r.RequestItems.ToList(),
                              RequestedBy = r.RequestedBy
                          }).ToList()
            };

            return View(model);

        }

        #region Login, Logout and Account management
        /// <summary>
        /// Login, Logout and account reset
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="email"></param>
        /// <param name="res"></param>
        /// <returns></returns>

        [AllowAnonymous]
        [HttpGet, ActionName("Login")]
        public async Task<IActionResult> Login(string returnUrl, string email)
        {

            //await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            //await _signInManager.SignOutAsync();

            LoginViewModel model = new();
            var applicationSettings = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
            var company = await _databaseContext.Company.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
            model.Company = company;
            model.ApplicationSettings = applicationSettings;

            if (email != null)
            {
                model.Email = email;
            }
            else { }

            returnUrl ??= Url.Content("~/");
            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                //Clear session on Login
                HttpContext.Session.Clear();
                return View(model);
            }


            if (model.Email == null || string.IsNullOrEmpty(model.Email))
            {
                Notify(Constants.toastr, "Required!", "Valid email required.", notificationType: NotificationType.error);
                return View(model);
            }

            if (model.Password == null || string.IsNullOrEmpty(model.Password))
            {
                Notify(Constants.toastr, "Required!", "Valid password required.", notificationType: NotificationType.error);
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    //ViewBag.errorMessage = "You must have a confirmed email to log on.";
                    Notify(Constants.toastr, "Required!", "Confirmed email required.", notificationType: NotificationType.error);
                    return View(model);
                }
                else
                {
                    ////User is in reset mode
                    //if (user.IsResetMode == true)
                    //{
                    //    ResetPasswordViewModel resetModel = new()
                    //    {
                    //        Email = model.Email,
                    //        ResetType = "A",
                    //    };
                    //    //return RedirectToAction(nameof(ResetPass), new { email = user.Email });
                    //    return RedirectToAction(nameof(ResetPassword), new { email = user.Email, type = "A" });
                    //}
                    if (user.IsActive == false)
                    {
                        Notify(Constants.toastr, "Failed!", $"Invalid login attempt. Contact Administrator.", notificationType: NotificationType.error);
                        return View();
                    }
                    else
                    {
                        // To enable password failures to trigger account lockout, change to shouldLockout: true
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {

                            //User is in reset mode started by the administrator
                            if (user.IsResetMode == true)
                            {
                                TempData["ResetObj"] = model.Password;
                                return RedirectToAction(nameof(ResetPassword), new { email = user.Email, type = "A" });
                            }
                            else
                            {
                                //Add claims
                                var claims = new List<Claim>{
                                        new Claim(ClaimTypes.Name, user.Email),
                                        new Claim(ClaimTypes.Email, user.Email),
                                        new Claim(ClaimTypes.Role, user.Email),
                                    };

                                var claimsIdentity = new ClaimsIdentity(claims, Constants.CookieScheme);
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                                await HttpContext.SignInAsync(Constants.CookieScheme, claimsPrincipal);

                                user.LastLogin = DateTime.UtcNow;
                                await _userManager.UpdateAsync(user);
                                _databaseContext.SaveChanges();

                                return RedirectToLocal(returnUrl);
                            }
                        }
                        else if (result.RequiresTwoFactor)
                        {
                            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, model.RememberMe });
                        }
                        else if (result.IsNotAllowed)
                        {
                            Notify(Constants.toastr, "Failed!", "User not allowed to login.", notificationType: NotificationType.error);
                            return View();
                        }
                        else if (result.IsLockedOut)
                        {
                            Notify(Constants.toastr, "Failed!", "User locked out.", notificationType: NotificationType.error);
                            return View();
                        }
                        else
                        {
                            Notify(Constants.toastr, "Failed!", "Invalid login attempt.", notificationType: NotificationType.error);
                            return View();
                        }
                    }
                }
            }


            return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction(nameof(Index), "Home");
        //}

        //[HttpGet]
        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction(nameof(Login), "Account");
        //}

        [HttpGet, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login), "Account");
        }

        [AllowAnonymous]
        [HttpGet, ActionName("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            ForgotPasswordViewModel model = new();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost, ActionName("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email == null)
                {
                    Notify(Constants.toastr, "Failed!", "Email cannot be null.", notificationType: NotificationType.error);
                    return View(model);
                }
                else
                {
                    // Require the user to have a confirmed email before they can log on.
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                    {
                        Notify(Constants.toastr, "Failed!", "Confirmed Email required!", notificationType: NotificationType.error);
                        // Don't reveal that the user does not exist or is not confirmed
                        return View(model);
                    }
                    else
                    {
                        //Generate Email Confirmation Token
                        var passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                        passwordResetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetCode));
                        //Generate Password Reset Token
                        List<EmailAddress> emailRecipients = new() { new EmailAddress() { Name = user.FirstName, Address = user.Email } };
                        var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code = passwordResetCode, type = "S" }, protocol: Request.Scheme);
                        EmailMessage mailMessage = new();
                        mailMessage.Subject = "Password Reset - salesngin";
                        mailMessage.Body = $"You are receiving this email because we received a password reset request for your account. To proceed with the password reset please click on the button below:";
                        mailMessage.BodyB = $"This password reset link will expire in 30 minutes. If you did not request a password reset, no further action is required and kindly inform the administrator about it. Thank You";
                        mailMessage.EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.PassResetEmailTemplateFile;
                        //mailMessage.EmailLink = $"<a style='color:white;' class='button' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Set a new password</a>.";
                        //string link = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' rel='noopener' style='text-decoration:none;display:inline-block;text-align:center;padding:0.75575rem 1.3rem;font-size:0.925rem;line-height:1.5;border-radius:0.35rem;color:#ffffff;background-color:#009EF7;border:0px;margin-right:0.75rem!important;font-weight:600!important;outline:none!important;vertical-align:middle'> Reset Password </a>";
                        string link = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' rel='noopener' class='linkButton'> Reset Password </a>";

                        mailMessage.EmailLink = link;
                        mailMessage.Company = "Boheneko Catering Services";
                        mailMessage.App = "salesngin";
                        //mailMessage.EmailLink = $"<a style='color:white;' class='button' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Set a new password</a>.";
                        mailMessage.ToAddresses = emailRecipients;
                        BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(mailMessage));

                        Notify(Constants.sweetAlert, "Success!", "Password reset link sent successfully. Check your email and continue process.", notificationType: NotificationType.success);
                        return View(nameof(Login));
                    }
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet, ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string email = null, string code = null, string type = null, bool res = false)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                ResetPasswordViewModel model = new()
                {
                    Email = email,
                    UserId = user.Id
                };
                //Administrator started reset process
                if (type == "A" && email != null)
                {
                    string currentPassword = (string)TempData["ResetObj"];
                    model.Name = user.FullName;
                    model.CurrentPassword = currentPassword ?? string.Empty;
                    model.ResetType = "A";
                    return View(model);
                }
                else if (type == "S" && (email != null || code != null))
                {
                    //User started reset process
                    model.Name = user.Email;
                    model.OldCode = code;
                    model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    model.ResetType = "S";
                    return View(model);
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Verification failed. Restart process or contact administrator.", notificationType: NotificationType.error);
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                Notify(Constants.toastr, "Failed!", "Verification failed. Restart process or contact administrator.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Login));
            }
        }

        [AllowAnonymous]
        [HttpPost, ActionName("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ResetObj"] = model.CurrentPassword;
                return View(model);
                //return RedirectToAction("ResetPassword", "Account", new { email = model.Email, code = model.OldCode, type = model.ResetType });
            }

            if (model != null)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (model.ResetType == "A")
                    {
                        if (await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            var result = await _userManager.ResetPasswordAsync(user, token, model.ConfirmPassword);
                            if (result.Succeeded)
                            {
                                user.IsResetMode = false;
                                _databaseContext.Users.Update(user);

                                model.Code = string.Empty;
                                var routeValues = new RouteValueDictionary { { "email", user.Email }, { "returnUrl", null } };

                                if (await _databaseContext.SaveChangesAsync() > 0)
                                {
                                    Notify(Constants.toastr, "Success!", "Password updated successfully.", notificationType: NotificationType.success);
                                }
                                else
                                {
                                    Notify(Constants.toastr, "Success!", "Password changed successfully.", notificationType: NotificationType.success);
                                }
                                return RedirectToAction("Login", "Account", routeValues);
                            }
                            else
                            {
                                Notify(Constants.toastr, "Failed!", "Unable to reset user account. Check inputs and try again.", notificationType: NotificationType.info);
                            }
                        }
                        else
                        {
                            Notify(Constants.toastr, "Failed!", "Current Password did not match, Check inputs and try again.", notificationType: NotificationType.info);
                        }
                    }
                    else if (model.ResetType == "S")
                    {

                        model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
                        //verify the reset code
                        if (await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", model.Code))
                        {
                            //Valid reset code
                            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                            if (result.Succeeded)
                            {
                                model.Code = string.Empty;
                                var routeValues = new RouteValueDictionary{
                               {"email", user.Email },
                               {"returnUrl", null },
                               {"res", false }
                            };

                                Notify(Constants.toastr, "Success!", "Your password has been reset. Login to proceed.", notificationType: NotificationType.success);
                                return RedirectToAction(actionName: "Login", controllerName: "Account", routeValues);
                            }
                            else
                            {
                                Notify(Constants.toastr, "Failed!", "Password reset failed. Check your email and click reset link to try again.", notificationType: NotificationType.success);
                                //return RedirectToAction("ResetPassword", "Account", new { email = model.Email, code = model.OldCode,});
                                return RedirectToAction("Login", "Account");
                            }
                        }
                        else
                        {
                            //invalid reset code
                            Notify(Constants.toastr, "Failed!", "Unable to reset user account. Check inputs and try again.", notificationType: NotificationType.error);
                            return RedirectToAction("Login", "Account");
                        }


                    }
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "User Verification failed. Check inputs and try again.", notificationType: NotificationType.error);
                }

            }

            return RedirectToAction(actionName: "Login", controllerName: "Account");
        }

        [AllowAnonymous]
        [HttpPost, ActionName("UpdatePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(ApplicationUserViewModel model)
        {
            ApplicationUser selectedEmployee = await _databaseContext.Users.FirstOrDefaultAsync(e => e.Id == model.Id);
            if (selectedEmployee is null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    Notify(Constants.toastr, "Failed!", "Password is required!", notificationType: NotificationType.warning);
                }
                else
                {
                    var loggedIn = await GetCurrentUserAsync();
                    var token = await _userManager.GeneratePasswordResetTokenAsync(selectedEmployee);
                    var result = await _userManager.ResetPasswordAsync(selectedEmployee, token, model.CurrentPassword);

                    if (result.Succeeded)
                    {
                        selectedEmployee.IsResetMode = true;
                        _databaseContext.Update(selectedEmployee);
                        if (await _databaseContext.SaveChangesAsync() > 0)
                        {
                            Notify(Constants.toastr, "Success!", "Reset successful.Reset mode activated!", notificationType: NotificationType.success);
                        }
                        else
                        {
                            Notify(Constants.toastr, "Success!", "Reset successful.", notificationType: NotificationType.success);
                        }
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Password Reset Failed. Try again.", notificationType: NotificationType.info);
                    }
                }
            }
            return RedirectToAction(nameof(Profile), new { id = model.Id });

        }

        [AllowAnonymous]
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(UserViewModel model)
        {
            if (model.UserId == null)
            {
                Notify(Constants.toastr, "Failed!", "User is required!", notificationType: NotificationType.warning);
                return RedirectToAction(nameof(Profile), new { id = model.UserId });
            }


            if (model.Password == null)
            {
                Notify(Constants.toastr, "Failed!", "Password is required!", notificationType: NotificationType.warning);
                return RedirectToAction(nameof(Profile), new { id = model.UserId });
            }

            if (model.ConfirmPassword == null)
            {
                Notify(Constants.toastr, "Failed!", "Confirm Password is required!", notificationType: NotificationType.warning);
                return RedirectToAction(nameof(Profile), new { id = model.UserId });
            }


            if (!model.ConfirmPassword.Equals(model.Password) || !model.Password.Equals(model.ConfirmPassword))
            {
                Notify(Constants.toastr, "Failed!", $"Password & Confirm Password do not match!", notificationType: NotificationType.warning);
                return RedirectToAction(nameof(Profile), new { id = model.UserId });
            }

            var user = await _dataService.GetUserById(model.UserId.ToString());
            if (user != null)
            {
                var result = await _dataService.ResetUserPassword(user.Id, model.ConfirmPassword);
                if (result == true)
                {
                    Notify(Constants.toastr, "Success!", $"{user.FirstName}'s Password reset was successful.", notificationType: NotificationType.success);
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", $"{user.FirstName}'s Password Reset was unsuccessful.", notificationType: NotificationType.warning);
                }
            }
            else
            {
                Notify(Constants.toastr, "Failed!", $"User not found.", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Profile), new { id = model.UserId });

        }

        //Works
        [AllowAnonymous]
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(UserViewModel model)
        {
            if (model.UserId == null)
            {
                Notify(Constants.toastr, "Failed!", "User is required!", notificationType: NotificationType.warning);
                return RedirectToAction(nameof(Profile), new { id = model.UserId });
            }

            string phoneNumber = string.Empty;
            string emailAddress = string.Empty;

            //Get the selected user
            var user = await _dataService.GetUserById(model.UserId.ToString());
            if (user != null)
            {
                if (model.PhoneNumber != null)
                {
                    if (!model.PhoneNumber.Equals(user.PhoneNumber))
                    {
                        //check if phone number exists in db

                        if (await _databaseContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber.Equals(model.PhoneNumber)) == null)
                        {
                            phoneNumber = model.PhoneNumber;
                        }

                    }
                }

                if (model.Email != null)
                {
                    if (!model.Email.Equals(user.Email))
                    {
                        //check if phone number exists in db
                        if (await _databaseContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(model.Email)) == null)
                        {
                            emailAddress = model.Email;
                        }

                    }
                }

                using var transaction = _databaseContext.Database.BeginTransaction();
                try
                {
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        //user.PhoneNumber = phoneNumber;
                        var phoneToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
                        await _userManager.ChangePhoneNumberAsync(user, phoneNumber, phoneToken);
                        _databaseContext.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(emailAddress))
                    {
                        //user.Email = emailAddress;
                        var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, emailAddress);
                        await _userManager.ChangeEmailAsync(user, emailAddress, emailToken);
                        _databaseContext.SaveChanges();
                    }

                    transaction.Commit();
                    Notify(Constants.toastr, "Success!", $"{user.FirstName}'s contact info updated successfully.", notificationType: NotificationType.success);
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    transaction.Rollback();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                }

            }
            else
            {
                Notify(Constants.toastr, "Failed!", $"User not found.", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Profile), new { id = model.UserId });

        }

        [AllowAnonymous]
        [HttpGet, ActionName("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string ecode)
        {
            if (email == null || ecode == null)
            {
                return RedirectToAction(nameof(Login), "Account");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction(actionName: "Login", controllerName: "Account");
            }

            ecode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(ecode));
            var result = await _userManager.ConfirmEmailAsync(user, ecode);
            if (!result.Succeeded)
            {
                return RedirectToAction(actionName: "Login", controllerName: "Account");
            }

            //Generate Password Reset Token
            var passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            passwordResetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetCode));

            var routeValues = new RouteValueDictionary
            {
                {"email", user.Email },
                {"code", passwordResetCode },
                {"type", "S" }
            };

            //return RedirectToAction("ResetPassword", "Account", routeValues);
            return RedirectToAction(actionName: "ResetPassword", controllerName: "Account", routeValues);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (email == null)
            {
                Notify(Constants.toastr, "Failed!", "Email cannot be null.", notificationType: NotificationType.error);
            }
            else
            {
                // Require the user to have a confirmed email || !(await _userManager.IsEmailConfirmedAsync(user)).
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    Notify(Constants.toastr, "Not Found!", "User not found!", notificationType: NotificationType.error);
                    // Don't reveal that the user does not exist or is not confirmed
                }
                else
                {
                    //Generate Email Confirmation Token
                    var passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                    passwordResetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetCode));
                    //Generate Password Reset Token
                    List<EmailAddress> emailRecipients = new() { new EmailAddress() { Name = user.FirstName, Address = user.Email } };
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code = passwordResetCode, type = "S" }, protocol: Request.Scheme);
                    EmailMessage mailMessage = new();
                    mailMessage.Subject = "Password Reset - salesngin";
                    mailMessage.Body = $"You are receiving this email because we received a password reset request for your account. To proceed with the password reset please click on the button below:";
                    mailMessage.BodyB = $"This password reset link will expire in 30 minutes. If you did not request a password reset, no further action is required and kindly inform the administrator about it. Thank You";
                    mailMessage.EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.PassResetEmailTemplateFile;
                    //mailMessage.EmailLink = $"<a style='color:white;' class='button' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Set a new password</a>.";
                    //string link = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' rel='noopener' style='text-decoration:none;display:inline-block;text-align:center;padding:0.75575rem 1.3rem;font-size:0.925rem;line-height:1.5;border-radius:0.35rem;color:#ffffff;background-color:#009EF7;border:0px;margin-right:0.75rem!important;font-weight:600!important;outline:none!important;vertical-align:middle'> Reset Password </a>";
                    string link = $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' rel='noopener' class='linkButton'> Reset Password </a>";

                    mailMessage.EmailLink = link;
                    mailMessage.Company = "Ghana Civil Aviation Training Academy";
                    mailMessage.App = "salesngin";
                    //mailMessage.EmailLink = $"<a style='color:white;' class='button' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Set a new password</a>.";
                    mailMessage.ToAddresses = emailRecipients;
                    BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(mailMessage));

                    Notify(Constants.sweetAlert, "Success!", "Password reset link sent successfully. Check your email and continue process.", notificationType: NotificationType.success);
                    return RedirectToAction("Profile", "Account", new { id = user.Id });
                }
            }
            return View(nameof(Index));
        }

        #endregion


        #region Roles and Modules
        /// <summary>
        /// Roles and Modules
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> Roles()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            RoleViewModel model = new()
            {
                Roles = await _roleManager.Roles.ToListAsync(),
                UserLoggedIn = loggedInUser
            };

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.UserStatuses;  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> Permissions(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            if (id == null)
            {
                Notify(Constants.toastr, "Failed!", "User not Found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Users), "Account");
            }

            //Get the selected user
            ApplicationUser user = new();
            user = await _userManager.FindByIdAsync(id.ToString());
            //Get Users Role
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count <= 0)
            {
                Notify(Constants.toastr, "Failed!", "No Roles assigned!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Roles), "Account");
            }

            var selectedRoleName = roles.FirstOrDefault();
            ApplicationRole userRole = await _roleManager.FindByNameAsync(selectedRoleName);

            List<RoleModule> roleModules = new();
            roleModules = await _databaseContext.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(r => r.RoleId == userRole.Id).ToListAsync();

            //Get Users Permissions
            List<ModulePermission> modulePermissions = new();
            modulePermissions = await _databaseContext.ModulePermissions.Where(m => m.UserId == user.Id).ToListAsync();

            UserViewModel model = new()
            {
                UserRole = userRole,
                RoleModules = roleModules,
                RoleModulePermissions = modulePermissions,
                UserLoggedIn = loggedInUser
            };

            return View(model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> RoleModules(int? id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            if (id == null)
            {
                Notify(Constants.toastr, "Failed!", "Role not Found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Roles), "Account");
            }

            ApplicationRole role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                Notify(Constants.toastr, "Failed!", "Role not Found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Roles), "Account");
            }

            List<RoleModule> roleModules = new();
            roleModules = await _databaseContext.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(r => r.RoleId == role.Id).ToListAsync();

            var modules = await _databaseContext.Modules.ToListAsync();

            RoleModuleViewModel rmvm = new()
            {
                Role = role,
                RoleModules = roleModules,
                Modules = modules,
                UserLoggedIn = loggedInUser
            };

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.UserStatuses;  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(rmvm);
        }

        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> RoleModules(RoleModuleViewModel model)
        {
            if (model.ModuleId == 0)
            {
                Notify(Constants.toastr, "Failed!", "Module is required!", NotificationType.error);
            }
            else if (model.RoleId == 0)
            {
                Notify(Constants.toastr, "Failed!", "Role is required!", NotificationType.error);
            }
            else if (await ModuleAvailability(model.RoleId, model.ModuleId) == true)
            {
                Notify(Constants.toastr, "Failed!", "Module already added to selected Role!", NotificationType.error);
            }
            else
            {
                RoleModule roleModule = new()
                {
                    RoleId = (int)model.RoleId,
                    ModuleId = model.ModuleId
                };
                _databaseContext.RoleModules.Add(roleModule);
                var result = _databaseContext.SaveChanges();
                if (result > 0)
                {
                    Notify(Constants.toastr, "Success!", "Module added to selected Role", NotificationType.success);
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Unsuccessful. Module not added.", NotificationType.error);
                }
            }
            return RedirectToAction(nameof(RoleModules), "Account", new { id = model.RoleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> DeleteRoleModule(int? id, int? rid)
        {
            //id = Role Module Id,  rid = Role Id (for redirection to role modules page)
            if (id == null)
            {
                Notify(Constants.toastr, "Failed!", "Module not available!");
            }
            //Get the role module
            var roleModule = await _databaseContext.RoleModules.Include(m => m.Module).Include(r => r.Role).FirstOrDefaultAsync(r => r.Id == id);
            if (roleModule != null)
            {
                //store the roleId and ModuleId
                var roleId = roleModule.RoleId;
                var moduleId = roleModule.ModuleId;
                //Get all role permissions with the roleId and ModuleId
                var modulePermissions = await _databaseContext.ModulePermissions.Where(m => m.ModuleId == moduleId && m.RoleId == roleId).ToListAsync();

                using var transaction = _databaseContext.Database.BeginTransaction();
                try
                {
                    if (modulePermissions.Any())
                    {
                        //remove the module permissions
                        _databaseContext.ModulePermissions.RemoveRange(modulePermissions);
                    }
                    //remove the module from the role 
                    _databaseContext.RoleModules.Remove(roleModule);
                    //Save the changes
                    var result = _databaseContext.SaveChanges();

                    transaction.Commit();
                    if (result > 0)
                    {
                        Notify(Constants.toastr, "Success!", "Module removed from Role.", notificationType: NotificationType.success);
                    }
                    else
                    {
                        Notify(Constants.toastr, "Failed!", "Unsuccessful. Module not removed.", notificationType: NotificationType.error);
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    transaction.Rollback();
                    Notify(Constants.toastr, "Failed!", "Something went wrong.", notificationType: NotificationType.error);
                }


            }
            else
            {
                Notify(Constants.toastr, "Failed!", "Module not available!");
            }


            return RedirectToAction(nameof(RoleModules), "Account", new { id = rid });

        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> RoleUsers(int? id)
        {
            //id = Role Id
            if (id == null)
            {
                Notify(Constants.toastr, "Failed!", "Role not Found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Roles), "Account");
            }

            ApplicationRole role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                Notify(Constants.toastr, "Failed!", "Role not Found!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Roles), "Account");
            }

            //List<RoleModule> roleModules = new();
            //roleModules = await _context.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(r => r.RoleId == role.Id).ToListAsync();

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            RoleModuleViewModel roleModuleViewModel = new()
            {
                Role = role,
                //RoleModules = roleModules,
                Users = (List<ApplicationUser>)usersInRole,
                UserList = await _databaseContext.Users.ToListAsync(),
            };

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = true;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.UserStatuses;  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(roleModuleViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApplicationRoles.SuperAdministrator},{ApplicationRoles.Staff}")]
        public async Task<IActionResult> AddUserToRole(int? id, [Bind("UserId,RoleId")] RoleModuleViewModel model)
        {

            if (model == null)
            {
                return RedirectToAction(nameof(RoleUsers), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.UserId == null || model.UserId == 0)
                    {
                        Notify(Constants.toastr, "Empty Field!", "Select a User. ", NotificationType.info);
                    }
                    else if (model.RoleId == null || model.RoleId == 0)
                    {
                        Notify(Constants.toastr, "Empty Field!", "Select a Role. ", NotificationType.info);
                    }
                    else
                    {
                        //Check if record exists
                        var roleChange = string.Empty;
                        var role = await _roleManager.FindByIdAsync(model.RoleId.ToString());
                        if (role == null)
                        {
                            Notify(Constants.toastr, "Not Found!", "Role not Found. ", NotificationType.info);
                        }
                        else
                        {
                            ApplicationUser user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                            if (user != null)
                            {
                                //Assign Selected User to Selected Role 
                                if (await _userManager.IsInRoleAsync(user, role.Name) == true)
                                {
                                    Notify(Constants.toastr, "Failed!", $"{user.FullName} is already assigned to {role.Name} role ", NotificationType.info);
                                }
                                else
                                {
                                    using var transaction = _databaseContext.Database.BeginTransaction();
                                    try
                                    {
                                        //For Single Role, Remove User from any role already assigned
                                        var assignedRoleNames = await _userManager.GetRolesAsync(user);
                                        List<ApplicationRole> assignedRoles = await _dataService.GetUserRoles(user.Id);
                                        if (assignedRoles.Count > 0)
                                        {
                                            foreach (var currentRole in assignedRoles)
                                            {
                                                //get user permissions
                                                var currentPermissions = await _databaseContext.ModulePermissions
                                                    .Where(m => m.UserId == user.Id && m.RoleId == currentRole.Id).ToListAsync();
                                                if (currentPermissions.Count > 0)
                                                {
                                                    //remove permissions  
                                                    _databaseContext.ModulePermissions.RemoveRange(currentPermissions);
                                                    _databaseContext.SaveChanges();
                                                }

                                                await _userManager.RemoveFromRoleAsync(user, currentRole.Name);
                                            }
                                        }

                                        // Add the user to the newly selected Role
                                        await _userManager.AddToRoleAsync(user, role.Name);
                                        transaction.Commit();
                                        Notify(Constants.toastr, "Success!", $"{user.FirstName} , has been added to {role.Name} role.", NotificationType.success);

                                    }
                                    catch (Exception ex)
                                    {
                                        // Commit transaction if all commands succeed, transaction will auto-rollback
                                        _ = ex.Message;
                                        transaction.Rollback();
                                        //if the operation wasnt successful, add error message to model state and return the view 
                                        Notify(Constants.toastr, "Failed!", "Something went wrong adding the role to this user account.", NotificationType.info);
                                    }
                                }
                            }
                            else
                            {
                                Notify(Constants.toastr, "Failed!", "Something Went wrong. ", NotificationType.info);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Notify(Constants.toastr, "Error!", "Something Went wrong. " + ex.Message, NotificationType.error);
                }
            }

            return RedirectToAction(nameof(RoleUsers), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> DeleteUserFromRole(int? id, int? rid)
        {
            //id = User Id,  rid = Role Id (for redirection to role modules page)
            if (id == null)
            {
                Notify(Constants.toastr, "Failed!", "User not available!", notificationType: NotificationType.error);
            }

            if (rid == null)
            {
                Notify(Constants.toastr, "Failed!", "Role not available!", notificationType: NotificationType.error);
            }

            var role = await _databaseContext.Roles.FirstOrDefaultAsync(r => r.Id == rid);
            var user = await _dataService.GetUserById(id.ToString());

            //var role = await _context.RoleModules.FirstOrDefaultAsync(r => r.Id == id);
            if (role != null && user != null)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                if (result.Succeeded)
                {
                    Notify(Constants.toastr, "Success!", "User removed from Role.", notificationType: NotificationType.success);
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Unsuccessful. User not removed.", notificationType: NotificationType.error);
                }

                //_context.UserRoles.Remove(roleModule);
                //var result = _context.SaveChanges();
                //if (result > 0)
                //{
                //    Notify(Constants.toastr, "Success!", "User removed from Role.", notificationType: NotificationType.success);
                //}
                //else
                //{
                //    Notify(Constants.toastr, "Failed!", "Unsuccessful. User not removed.", notificationType: NotificationType.warning);
                //}
            }
            else
            {
                Notify(Constants.toastr, "Failed!", "User / Role not available!");
            }


            return RedirectToAction(nameof(RoleUsers), "Account", new { id = rid });

        }


        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Configure)]
        public async Task<IActionResult> Modules()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            ModuleViewModel model = new()
            {
                Modules = await _databaseContext.Modules.AsNoTracking().OrderBy(m => m.ModuleDisplay).ToListAsync(),
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.User_Module, loggedInUser.Id),
                UserLoggedIn = loggedInUser
            };

            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = false;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = "";  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(model);
        }

        public async Task<bool> ModuleAvailability(int? roleId, int? moduleId)
        {
            if (roleId == null || moduleId == null)
            {
                return true;
            }
            else
            {
                return await _databaseContext.RoleModules.AsNoTracking().AnyAsync(m => m.RoleId == roleId && m.ModuleId == moduleId);
            }
        }

        #endregion 


        #region User Create, Update, Delete

        [HttpPost, ActionName("DeleteUser")]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            //id = User Id
            ApplicationUser userToDelete = new();
            userToDelete = await _userManager.FindByIdAsync(id.ToString());
            if (userToDelete != null)
            {
                List<string> usersRoles = new();
                var userRoles = await _userManager.GetRolesAsync(userToDelete);
                if (userRoles != null)
                {
                    usersRoles = [.. userRoles];
                }
                using var transaction = _databaseContext.Database.BeginTransaction();
                try
                {
                    if (usersRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(userToDelete, usersRoles);
                    }
                    await _userManager.DeleteAsync(userToDelete);

                    await _databaseContext.SaveChangesAsync();

                    transaction.Commit();

                    Notify(Constants.toastr, "Success!", "User has been deleted.", notificationType: NotificationType.success);

                }
                catch (Exception ex)
                {
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    _ = ex.Message;
                    transaction.Rollback();
                    Notify(Constants.toastr, "Success!", "User has been deleted.", notificationType: NotificationType.success);
                }
            }
            else
            {
                Notify(Constants.toastr, "Record Exists!", "User does not exist. ", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Users));
        }


        [HttpGet, ActionName("UserRole")]
        public async Task<IActionResult> UserRole(int? id)
        {
            var roles = await _roleManager.Roles.ToListAsync();

            //var user = await _context.Users.AsNoTracking().Include(d => d.Department).FirstOrDefaultAsync(e => e.Id == id);
            var user = await _databaseContext.Users.FirstOrDefaultAsync(e => e.Id == id);

            if (user == null)
            {
                Notify(Constants.toastr, "Failed!", $"User not found", notificationType: NotificationType.error);
                ViewBag.RequestedItem = id;
                ViewBag.ReferrerUrl = "/Account/Users";
                return View("NotFound");
            }

            List<ApplicationRole> usersRoles = new();
            string SelectedRoleName = "0";
            //Get a list of role names that the specified user has been assigned to 
            var userRoleNames = await _userManager.GetRolesAsync(user);
            if (userRoleNames.Count > 0)
            {
                List<ApplicationRole> listOfUserRoles = new();
                foreach (string roleName in userRoleNames)
                {
                    var assignedRole = await _roleManager.FindByNameAsync(roleName);
                    ApplicationRole r = new()
                    {
                        Id = assignedRole.Id,
                        Name = assignedRole.Name
                    };
                    listOfUserRoles.Add(r);
                    usersRoles.Add(r);
                }
                SelectedRoleName = listOfUserRoles.First().Name;
                //SelectedRoleId = listOfUserRoles.First().Id;
            }

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                //UserFullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OtherName = user.OtherName,
                UserPhoto = user.PhotoPath,
                //StaffNumber = user.StaffNumber,
                PhoneNumber = user.PhoneNumber,
                //RoleName = SelectedRoleName,
                Email = user.Email,
                ApplicationRoles = roles,
                Roles = usersRoles

            };
            return View(model);
        }

        [HttpPost, ActionName("UpdateUserRole")]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(ManageUserRolesViewModel model)
        {
            //Get the list of system roles 
            var systemRoles = await _roleManager.Roles.ToListAsync();
            //Get an object of the user with the specified id 
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                //if no user if found 
                Notify(Constants.sweetAlert, "Info!", "User was not found.", notificationType: NotificationType.info);
                return RedirectToAction("Users", "Account");
            }
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

            //Create a list to hold the selected role for the user 
            List<string> roleList = new();


            if (model.RoleName == null || model.RoleName == "0")
            {
                ModelState.AddModelError("", "Select a Role");
                Notify(Constants.sweetAlert, "Info!", "Select a valid role.", notificationType: NotificationType.info);
                return View(nameof(UserRole), new { id = model.UserId });
            }

            int selectedRoleId;
            var selectedRole = await _roleManager.FindByNameAsync(model.RoleName);
            selectedRoleId = selectedRole != null ? selectedRole.Id : 0;

            //Get an object of the role selected with the role id  
            //var assignedRole = await _roleManager.FindByIdAsync(model.RoleId);
            if (selectedRole == null || selectedRoleId == 0)
            {
                //if the object is null / empty, pass error message to view and reload the form 
                ModelState.AddModelError("", "Selected Role doesn't exist");
                Notify(Constants.sweetAlert, "Info!", "Selected Role doesn't exist", notificationType: NotificationType.info);
                return View(nameof(UserRole), new { id = model.UserId });
            }
            //if the object is not empty add it to the list
            //this would be used in the AddToRolesAsync method as a parameter
            roleList.Add(selectedRole.Name);

            //Get a list of role names that the specified user has been assigned to 
            IList<string> roles = await _userManager.GetRolesAsync(user);
            IdentityResult result = new();



            //var selectedUserRoleLevel = await _context.UserRoleLevels.Include(u => u.User).Include(r => r.Role).FirstOrDefaultAsync(r => r.RoleId == selectedRoleId && r.UserId == user.Id);

            using var transaction = _databaseContext.Database.BeginTransaction();
            try
            {
                await _userManager.AddToRoleAsync(user, selectedRole.Name);
                //if (selectedUserRoleLevel is not null)
                //{
                //    selectedUserRoleLevel.IsMaster = model.IsMaster;
                //}
                //else
                //{
                //    UserRoleLevel userRoleLevel = new()
                //    {
                //        UserId = user.Id,
                //        RoleId = selectedRoleId,
                //        IsMaster = model.IsMaster,
                //        DateAdded = DateTime.UtcNow
                //    };
                //    await _context.AddAsync(userRoleLevel);

                //}
                await _databaseContext.SaveChangesAsync();

                transaction.Commit();

                Notify(Constants.sweetAlert, "Success!", model.FirstName + ", has been assigned " + selectedRole.Name + " role.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(UserRole), new { id = model.UserId });

            }
            catch (Exception ex)
            {
                // Commit transaction if all commands succeed, transaction will auto-rollback
                _ = ex.Message;
                transaction.Rollback();
                //if the operation wasnt successful, add error message to model state and return the view 
                Notify(Constants.sweetAlert, "Failed!", "Something went wrong adding the role to this user account.", notificationType: NotificationType.info);
                return View(nameof(UserRole), new { id = model.UserId });
            }
        }

        // POST: AccountController/DeleteUserRole/5
        [HttpPost, ActionName("DeleteUserRole")]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserRole(ManageUserRolesViewModel model)
        {
            if (model == null) { return RedirectToAction(nameof(UserRole)); }

            try
            {

                ApplicationRole roleToDelete = new();
                roleToDelete = await _roleManager.FindByIdAsync(model.RoleId.ToString());
                var userToRemove = await _databaseContext.Users.FindAsync(model.UserId.ToString());
                //var employeeToRemove = await _context.Employees.FindAsync(model.UserId);

                if (roleToDelete != null && userToRemove != null)
                {
                    IdentityResult result = new();
                    //UserRoleLevel roleLevel = new();
                    //roleLevel = await _context.UserRoleLevels.FirstOrDefaultAsync(x => x.UserId == userToRemove.Id && x.RoleId == roleToDelete.Id);

                    using var transaction = _databaseContext.Database.BeginTransaction();
                    try
                    {
                        await _userManager.RemoveFromRoleAsync(userToRemove, roleToDelete.Name);
                        //if (roleLevel is not null)
                        //{
                        //    _context.Remove(roleLevel);
                        //}
                        await _databaseContext.SaveChangesAsync();

                        transaction.Commit();

                        Notify(Constants.sweetAlert, "Success!", "User has been removed from Role.", notificationType: NotificationType.success);
                        return RedirectToAction(nameof(UserRole), new { id = model.UserId });



                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                        // Commit transaction if all commands succeed, transaction will auto-rollback
                        transaction.Rollback();
                        //if the operation wasnt successful, add error message to model state and return the view 
                        Notify(Constants.sweetAlert, "Failed!", "Something went wrong. role not removed.", notificationType: NotificationType.info);
                        ////_logger.LogError("Something went wrong creating user account. Error : " + ex.Message);
                        return View(nameof(UserRole), new { id = model.UserId });
                    }
                }
                else
                {
                    Notify(Constants.sweetAlert, "Failed!", "Record does not exist. ", notificationType: NotificationType.info);
                }
            }
            catch (Exception ex)
            {
                //_ = ex.Message;
                Notify(Constants.sweetAlert, "Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.info);
            }
            return RedirectToAction(nameof(UserRole), new { id = model.UserId });
        }


        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangeUserRole([Bind("UserId,RoleId")] UserViewModel model)
        public async Task<IActionResult> ChangeUserRole(int? id, UserViewModel model)
        {
            if (id == null)
            {
                Notify(Constants.toastr, "Info!", "User was not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Index));

            }

            if (model.RoleId == null)
            {
                Notify(Constants.toastr, "Info!", "Role is required!.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { id = id });
            }

            //Get the User 
            var user = await _dataService.GetUserById(id.ToString());
            if (user == null)
            {
                Notify(Constants.toastr, "Info!", "User was not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { id = id });
            }

            //Change role
            var selectedRole = await _dataService.GetRoleById((int)model.RoleId);
            if (selectedRole == null)
            {
                Notify(Constants.toastr, "Info!", "Role was not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { id = id });
            }

            bool result = await _dataService.ChangeUserRole(user, selectedRole);
            if (result == true)
            {
                Notify(Constants.toastr, "Success!", $"{user.FirstName}'s role has been changed to: {selectedRole.Name}. Update Permissions to grant access.", notificationType: NotificationType.success);
            }
            else
            {
                Notify(Constants.toastr, "Info!", $"{user.FirstName}'s role was not changed.", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Profile), new { id = id });
        }


        /// <summary>
        /// Change User Role Permissions (ChangeRolePermissions)  
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserPermission(UserViewModel model)
        {
            if (model.UserId == null)
            {
                Notify(Constants.toastr, "Info!", "User was not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile));
            }

            if (model.RoleId == null)
            {
                Notify(Constants.toastr, "Info!", "Role was not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile));
            }
            //Get the User 
            var user = await _dataService.GetUserById(model.UserId.ToString());
            //Current role
            var role = await _dataService.GetRoleById((int)model.RoleId);

            List<ModulePermission> permissions = new();

            //Get User role module permissions 
            if (model.RoleModulePermissions.Count > 0)
            {
                foreach (var permission in model.RoleModulePermissions)
                {
                    ModulePermission singlePermission = new()
                    {
                        Create = permission.Create,
                        Read = permission.Read,
                        Update = permission.Update,
                        Delete = permission.Delete,
                        Configure = permission.Configure,
                        Report = permission.Report,
                        Approve = permission.Approve,
                        UserId = user.Id,
                        RoleId = role.Id,
                        ModuleId = permission.ModuleId,
                        CreatedBy = user?.Id,
                        DateCreated = DateTime.UtcNow,

                    };
                    permissions.Add(singlePermission);
                }
            }

            bool result = await _dataService.ChangeUserModulePermissions(user.Id, role.Id, permissions);
            if (result == true)
            {
                Notify(Constants.toastr, "Success!", $"{user.FirstName}'s permissions has changed.", notificationType: NotificationType.success);
            }
            else
            {
                Notify(Constants.toastr, "Info!", $"{user.FirstName}'s permission was not changed.", notificationType: NotificationType.error);
            }

            return RedirectToAction(nameof(Profile), new { id = model.UserId });

        }



        #endregion

        // GET: AccountController
        public IActionResult HangFireDash()
        {
            return View("/hangFire");
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                MailAddress m = new(email);
                return true;
            }
            catch (FormatException ex)
            {
                _ = ex.Message;
                //_ = ex.Message;
                return false;

            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                if (returnUrl == null)
                {
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
        }

        //-------------------------------------------------------NEW------------------------
        [HttpPost]
        public async Task<IActionResult> ImportUserData(RegisterViewModel model)
        {
            //using StreamReader reader = new(Request.Body, Encoding.UTF8);
            //string content = reader.ReadToEndAsync().Result;
            //Get the current Logged-in user 
            ApplicationUser usr = await GetCurrentUserAsync();
            if (model.Photo == null || model.Photo.Length == 0)
            {
                return Content("file not selected");
            }

            string fileContent = null;
            using (var reader = new StreamReader(model.Photo.OpenReadStream()))
            {
                fileContent = reader.ReadToEnd();
            }
            List<UserViewModel> importedList = new();
            List<ApplicationUser> newStaffList = new();
            //var result = JsonConvert.DeserializeObject<List<UserViewModel>>(fileContent);
            var result = JsonSerializer.Deserialize<List<UserViewModel>>(fileContent);

            if (result.Count > 0)
            {
                foreach (UserViewModel staff in result)
                {
                    importedList.Add(staff);
                }
            }

            //Check if user already exists using staff Number and Email
            //Save Staff to DB and display staff records 

            //model.EmployeeList = newList;

            #region Save Employee List

            if (importedList.Count > 0)
            {
                foreach (UserViewModel user in importedList)
                {
                    if (!await _databaseContext.Users.AnyAsync(x => x.Email == user.Email))
                    {
                        //Add the  User to the list 
                        ApplicationUser newUser = new()
                        {
                            UserName = user.Email,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            OtherName = user.OtherName,
                            PhoneNumber = user.PhoneNumber,
                            PhotoPath = user.StaffNumber + ".jpg",
                            DateCreated = DateTime.UtcNow,
                            JobTitle = user.JobTitle,
                            //StaffNumber = user.StaffNumber,
                            //DepartmentId = user.DepartmentId == 0 ? null : user.DepartmentId,
                            //SectionId = user.SectionId == 0 ? null : user.SectionId,
                            //UnitId = user.UnitId == 0 ? null : user.UnitId,
                            //JobCategoryId = user.JobCategoryId == 0 ? null : user.JobCategoryId,
                            //SupervisorId = user.SupervisorId ?? usr?.Id,
                            EmailConfirmed = true,
                            CreatedBy = usr?.Id
                        };

                        ApplicationRole roleToAssign = await _roleManager.FindByNameAsync(ApplicationRoles.Staff);


                        using var transaction = _databaseContext.Database.BeginTransaction();
                        try
                        {
                            await _userManager.CreateAsync(newUser, "password123");
                            await _userManager.AddToRoleAsync(newUser, roleToAssign.Name);
                            //Add user role level
                            //await _context.AddAsync(roleLevel);
                            await _databaseContext.SaveChangesAsync();
                            transaction.Commit();

                            Notify(Constants.sweetAlert, "Success!", "Data imported successfully.", notificationType: NotificationType.success);
                            //return RedirectToAction(nameof(Index));
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Message;
                            transaction.Rollback();
                            Notify(Constants.sweetAlert, "Failed!", "Something went wrong. data not imported.", notificationType: NotificationType.info);
                            //_logger.LogError("Something went wrong data not imported. Error : " + ex.Message);
                        }

                    }
                }
            }

            #endregion
            //model.Users = await GetUserList();
            return RedirectToAction(nameof(Index));
        }

        // GET: Users
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Users()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            //var employees = await _context.Employees.AsNoTracking().Include(u => u.User).OrderBy(x => x.StaffNumber).ToListAsync();
            var employees = await _databaseContext.Users.AsNoTracking()
                .Include(u => u.Unit)
                .Include(u => u.EmployeeType)
                .Include(u => u.Country)
                .OrderBy(x => x.StaffNumber)
                .ToListAsync();
            UserViewModel model = new()
            {
                Employees = employees,
                UserLoggedIn = loggedInUser,
                ModulePermission = await _dataService.GetModulePermission(ConstantModules.User_Module, loggedInUser.Id)
            };

            //Use this to configure the datatables toolbar
            ViewBag.ToolBarDateFilter = false; //Table has a date column
            ViewBag.ToolBarStatusFilter = true;//Table has a status filter column
            ViewBag.ToolBarStatusFilterOptions = GlobalConstants.UserStatuses;  //Status filter option items
            ViewBag.ToolBarExportOptions = true;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesPartial()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            //var employees = await _context.Employees.AsNoTracking().Include(u => u.User).OrderBy(x => x.StaffNumber).ToListAsync();
            var employees = await _databaseContext.Users.AsNoTracking()
               .Include(u => u.Unit)
               .Include(u => u.EmployeeType)
               .Include(u => u.Country)
               .OrderBy(x => x.StaffNumber)
               .ToListAsync();
            UserViewModel model = new()
            {
                Employees = employees,
                UserLoggedIn = loggedInUser,
            };
            return PartialView("_UsersTable", model);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeesJson()
        {
            //var employees = await _context.Employees.Include(u => u.User).OrderBy(x => x.StaffNumber).ToListAsync();
            var employees = await _databaseContext.Users.AsNoTracking()
               .Include(u => u.Unit)
               .Include(u => u.EmployeeType)
               .Include(u => u.Country)
               .OrderBy(x => x.StaffNumber)
               .ToListAsync();

            return Json(new { Data = employees });
        }

        /// <summary>
        /// Goto User Profile Page (ProfilePage)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.Profile_Module, ConstantPermissions.Read)]
        public async Task<IActionResult> Profile(int id)
        {

            //if (object.Equals(id, 0) || object.Equals(id, null))
            if (id.Equals(0) || object.Equals(id, null))
            {
                return NotFound();
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            UserViewModel searchUser;
            if (await _userManager.IsInRoleAsync(loggedInUser, ApplicationRoles.SuperAdministrator) || await _userManager.IsInRoleAsync(loggedInUser, ApplicationRoles.Administrator))
            {
                searchUser = await _dataService.GetUserProfile(id);
            }
            else
            {
                searchUser = await _dataService.GetUserProfile(loggedInUser.Id);
            }

            UserViewModel model = searchUser;
            model.Roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.AsNoTracking().ToListAsync();
            model.Countries = await _databaseContext.Countries.AsNoTracking().ToListAsync();
            model.Titles = await _databaseContext.Titles.AsNoTracking().ToListAsync();
            //model.Sections = await _databaseContext.Sections.AsNoTracking().ToListAsync();
            model.Units = await _databaseContext.Units.AsNoTracking().ToListAsync();
            model.UserLoggedIn = loggedInUser;
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.User_Module, loggedInUser.Id);

            return View(model);
        }

        //Update Corporate Info 
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserCorporate([Bind("UserId,EmployeeTypeId,DateStarted,DateEnded,StaffNumber,UnitId,JobTitle")] UserViewModel model)
        {
            model.Roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.AsNoTracking().ToListAsync();
            model.Countries = await _databaseContext.Countries.AsNoTracking().ToListAsync();
            model.Titles = await _databaseContext.Titles.AsNoTracking().ToListAsync();
            //model.Sections = await _databaseContext.Sections.AsNoTracking().ToListAsync();
            model.Units = await _databaseContext.Units.AsNoTracking().ToListAsync();
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.UserLoggedIn = loggedInUser;

            if (string.IsNullOrEmpty(model.StaffNumber))
            {
                Notify(Constants.toastr, "Info!", "Staff Number Required!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { Id = model.UserId });
            }

            if (!ModelState.IsValid)
            {
                Notify(Constants.toastr, "Info!", "Certain Fields are required!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { Id = model.UserId });
            }
            //Get the User
            var user = await _dataService.GetUserById(model.UserId.ToString());
            if (user != null)
            {
                //Get the User's employee details 
                //var employee = await _context.Employees.FirstOrDefaultAsync(x => x.UserId == user.Id);
                var employee = await _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
                if (employee != null)
                {
                    using var transaction = _databaseContext.Database.BeginTransaction();
                    try
                    {
                        //Employee Details
                        employee.StartDate = model.DateStarted;
                        employee.EndDate = model.DateEnded;
                        employee.EmployeeTypeId = model.EmployeeTypeId;
                        employee.StaffNumber = model.StaffNumber;
                        employee.UnitId = model.UnitId;
                        //employee.SectionId = model.SectionId;
                        employee.StaffNumber = model.StaffNumber;
                        user.JobTitle = model.JobTitle;
                        //user.Company = model.Company is null ? user.Company : model.Company;
                        user.Company = "POS";
                        _databaseContext.Users.Update(user);
                        await _databaseContext.SaveChangesAsync();
                        transaction.Commit();
                        Notify(Constants.toastr, "Success!", "User details Updated.", notificationType: NotificationType.success);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                        transaction.Rollback();
                        Notify(Constants.sweetAlert, "Failed!", "Something went wrong. action rolled back.", notificationType: NotificationType.info);
                    }
                }
                else
                {
                    Notify(Constants.toastr, "Info!", "Employee not found!", notificationType: NotificationType.info);
                }
            }
            else
            {
                Notify(Constants.toastr, "Info!", "User not found!", notificationType: NotificationType.info);
            }
            return RedirectToAction(nameof(Profile), new { Id = model.UserId });
        }

        //Update Personal Info 
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPersonal([Bind("UserId,Title,FirstName,LastName,OtherName,DOB,Gender,CountryId,PostalAddress")] UserViewModel model)
        {
            model.Roles = await _roleManager.Roles.ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.ToListAsync();
            model.Countries = await _databaseContext.Countries.ToListAsync();
            model.Titles = await _databaseContext.Titles.ToListAsync();
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.UserLoggedIn = loggedInUser;

            if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                Notify(Constants.toastr, "Info!", "First name or last name Required!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { Id = model.UserId });
            }

            if (model.CountryId is null)
            {
                Notify(Constants.toastr, "Info!", "Country is Required!", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { Id = model.UserId });
            }

            ModelState.Remove("Email");
            ModelState.Remove("EmployeeTypeId");
            ModelState.Remove("StaffNumber");
            ModelState.Remove("JobTitle");
            ModelState.Remove("Company");
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            if (ModelState.IsValid)
            {
                //Get the User
                var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                //var user = await _dataService.GetUserById(model.UserId.ToString());
                if (user != null)
                {

                    using var transaction = _databaseContext.Database.BeginTransaction();
                    try
                    {
                        //Application User Details
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.OtherName = model.OtherName;
                        user.DOB = model.DOB;
                        user.Title = model.Title;
                        user.Gender = model.Gender;
                        user.CountryId = model.CountryId;
                        user.PostalAddress = model.PostalAddress;

                        //if (user.PhoneNumber != model.PhoneNumber)
                        //{
                        //    //var changePhoneNumberToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                        //    //await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, changePhoneNumberToken);
                        //    user.PhoneNumber = model.PhoneNumber;

                        //}

                        //await _userManager.UpdateAsync(user);
                        //if (!changePhoneResult.Succeeded)
                        //{
                        //    return StatusCode(StatusCodes.Status500InternalServerError, changePhoneResult.Errors);
                        //}

                        var updateResult = await _userManager.UpdateAsync(user);
                        _databaseContext.SaveChanges();
                        transaction.Commit();

                        if (updateResult.Succeeded)
                        {
                            Notify(Constants.toastr, "Success!", "User details updated.", notificationType: NotificationType.success);
                            //return StatusCode(StatusCodes.Status500InternalServerError);
                        }
                        else
                        {
                            Notify(Constants.toastr, "Failed!", "Something went wrong. update unsuccessful.", notificationType: NotificationType.error);
                        }
                        //return Ok("User updated");

                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                        // Commit transaction if all commands succeed, transaction will auto-rollback
                        transaction.Rollback();
                        Notify(Constants.toastr, "Failed!", "Something went wrong. action rolled back.", notificationType: NotificationType.error);
                    }
                }
                else
                {
                    Notify(Constants.toastr, "Info!", "User not found!", notificationType: NotificationType.info);
                }
            }
            else
            {
                Notify(Constants.toastr, "Info!", "Certain Fields are required!", notificationType: NotificationType.info);
            }
            return RedirectToAction(nameof(Profile), new { Id = model.UserId });
        }
        //Update Email Address 

        //Update Photo 
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPhoto([Bind("UserId,Photo")] UserViewModel model)
        {
            model.Roles = await _roleManager.Roles.ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.ToListAsync();
            model.Countries = await _databaseContext.Countries.ToListAsync();
            model.Titles = await _databaseContext.Titles.ToListAsync();
            // model.Sections = await _databaseContext.Sections.ToListAsync();
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            model.UserLoggedIn = loggedInUser;

            if (model.UserId == null)
            {
                Notify(Constants.toastr, "Not Found!", "User not found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Index));
            }

            //ModelState.Remove("FirstName");
            //ModelState.Remove("LastName");
            //ModelState.Remove("Email");
            //ModelState.Remove("EmployeeTypeId");
            //ModelState.Remove("StaffNumber");
            //ModelState.Remove("JobTitle");
            //ModelState.Remove("Company");
            if (!ModelState.IsValid)
            {
                Notify(Constants.toastr, "Invalid Fields!", "Check input fields and try again.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Profile), new { Id = model.UserId });
            }

            //Get the User
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var employee = await _databaseContext.Users.FirstOrDefaultAsync(x => x.Id.Equals(user.Id));

            string fileExtension;
            string storeFileName = user.PhotoPath;
            string storePath = string.Empty;
            string filePath = FileStorePath.noUserPhotoPath;
            string fileUrl = FileStorePath.noUserPhotoPath;
            if (model.Photo != null)
            {
                var uniqueId = Guid.NewGuid();
                string newPictureName = !string.IsNullOrEmpty(employee.StaffNumber) ? employee.StaffNumber : uniqueId.ToString();
                fileExtension = Path.GetExtension(model.Photo.FileName);
                string newFileName = $"{newPictureName}{fileExtension}";

                fileUrl = FileStorePath.UserPhotoDirectory + newFileName;
                storeFileName = newFileName;
                storePath = Path.Combine(_webHostEnvironment.WebRootPath, FileStorePath.UserPhotoDirectoryName);
                if (!Directory.Exists(storePath))
                {
                    Directory.CreateDirectory(storePath);
                }

                filePath = Path.Combine(storePath, newFileName);

                user.PhotoPath = storeFileName;
                var updateResult = await _userManager.UpdateAsync(user);
                _databaseContext.SaveChanges();
                if (updateResult.Succeeded)
                {
                    Notify(Constants.toastr, "Success!", "User Photo Updated.", notificationType: NotificationType.success);
                    // Save Picture
                    if (model.Photo != null && !string.IsNullOrEmpty(filePath))
                    {
                        try
                        {
                            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                            await model.Photo.CopyToAsync(fileStream);

                        }
                        catch (Exception)
                        {

                            Notify(Constants.toastr, "Failed!", "Something went wrong. update unsuccessful.", notificationType: NotificationType.error);
                            ///throw;
                        }

                        //await model.Photo.CopyToAsync(new FileStream(filePath, FileMode.Create)); //Save image
                    }
                }
                else
                {
                    Notify(Constants.toastr, "Failed!", "Something went wrong. update unsuccessful.", notificationType: NotificationType.error);
                }
            }
            return RedirectToAction(nameof(Profile), new { Id = model.UserId });

        }

        //Reset Password 
        [HttpPost]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPassword([Bind("UserId,Password,ConfirmPassword")] UserViewModel model)
        {
            if (model.UserId == null)
            {
                return RedirectToAction(nameof(Users));
            }

            ApplicationUser loggedInUser = await GetCurrentUserAsync();

            model.Roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.AsNoTracking().ToListAsync();
            model.Countries = await _databaseContext.Countries.AsNoTracking().ToListAsync();
            model.Titles = await _databaseContext.Titles.AsNoTracking().ToListAsync();
            // model.Sections = await _databaseContext.Sections.AsNoTracking().ToListAsync();
            model.UserLoggedIn = loggedInUser;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    Notify(Constants.toastr, "Failed!", "Password &Confirm Password required.", notificationType: NotificationType.error);
                }
                else
                {
                    var selectedUser = await _userManager.FindByIdAsync(model.UserId.ToString());
                    if (selectedUser != null)
                    {


                        using var transaction = _databaseContext.Database.BeginTransaction();
                        try
                        {

                            var token = await _userManager.GeneratePasswordResetTokenAsync(selectedUser);
                            var result = await _userManager.ResetPasswordAsync(selectedUser, token, model.ConfirmPassword);
                            _databaseContext.SaveChanges();
                            selectedUser.IsResetMode = true;
                            await _userManager.UpdateAsync(selectedUser);
                            _databaseContext.SaveChanges();
                            transaction.Commit();

                            if (result.Succeeded)
                            {
                                Notify(Constants.toastr, "Success!", "Reset successful.Reset mode activated!", notificationType: NotificationType.success);
                                //selectedUser.IsResetMode = true;
                                //_context.Update(selectedUser);
                                //if (await _context.SaveChangesAsync() > 0)
                                //{
                                //    Notify(Constants.toastr, "Success!", "Reset successful.Reset mode activated!", notificationType: NotificationType.success);
                                //}
                                //else
                                //{
                                //    Notify(Constants.toastr, "Success!", "Reset successful.", notificationType: NotificationType.success);
                                //}
                            }
                            else
                            {
                                Notify(Constants.toastr, "Failed!", "Password Reset Failed. Try again.", notificationType: NotificationType.error);
                            }
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Message;
                            // Commit transaction if all commands succeed, transaction will auto-rollback
                            transaction.Rollback();
                            Notify(Constants.toastr, "Failed!", "Something went wrong. action rolled back.", notificationType: NotificationType.error);
                        }
                    }
                }
            }

            return RedirectToAction(nameof(Profile), new { id = model.UserId });

        }

        [HttpPost]
        //[Authorize(Roles = $"{ApplicationRoles.SuperAdministrator},{ApplicationRoles.Administrator}")]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(int? id, bool status)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            //model.Roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            //model.EmployeeTypes = await _context.EmployeeTypes.AsNoTracking().ToListAsync();
            //model.Countries = await _context.Countries.AsNoTracking().ToListAsync();
            //model.Titles = await _context.Titles.AsNoTracking().ToListAsync();
            //model.Sections = await _context.Sections.AsNoTracking().ToListAsync();
            //model.UserLoggedIn = loggedInUser;

            if (id == null)
            {
                Notify(Constants.toastr, "Not Found!", "User account not Found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Users), "Account");
            }

            //if (ModelState.IsValid)
            //{
            //var selectedUser = await _dataService.GetUserByIdInt((int)id);
            var selectedUser = await _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == (int)id);
            if (selectedUser == null)
            {
                Notify(Constants.toastr, "Not Found!", "User account not Found.", notificationType: NotificationType.error);
                return RedirectToAction(nameof(Users), "Account");
            }
            using var transaction = _databaseContext.Database.BeginTransaction();
            try
            {
                selectedUser.IsActive = status;
                selectedUser.DateModified = DateTime.UtcNow;
                selectedUser.ModifiedBy = loggedInUser?.Id;

                _databaseContext.Users.Update(selectedUser);
                _databaseContext.SaveChanges();


                transaction.Commit();
                Notify(Constants.toastr, "Success!", "User's status changed successfully.", notificationType: NotificationType.success);
                return RedirectToAction(nameof(Profile), new { Id = id });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                // Commit transaction if all commands succeed, transaction will auto-rollback
                transaction.Rollback();
                Notify(Constants.toastr, "Failed!", "Something went wrong. status not changed.", notificationType: NotificationType.error);
            }
            //}
            return RedirectToAction(nameof(Profile), new { Id = id });
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Create)]
        public async Task<IActionResult> Create()
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            //List<Unit> units = await _context.Units.ToListAsync();
            //ViewBag.Units = new SelectList(units, "Id", "UnitName");

            //var empTypes = await _context.EmployeeTypes.ToListAsync();

            UserViewModel model = new()
            {
                Roles = await _roleManager.Roles.ToListAsync(),
                EmployeeTypes = await _databaseContext.EmployeeTypes.ToListAsync(),
                Countries = await _databaseContext.Countries.ToListAsync(),
                Titles = await _databaseContext.Titles.ToListAsync(),
                // Departments = await _databaseContext.Departments.ToListAsync(),
                // Sections = await _databaseContext.Sections.ToListAsync(),
                Units = await _databaseContext.Units.ToListAsync(),
                UserLoggedIn = loggedInUser
            };

            return View(model);

        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Create)]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            model.Roles = await _roleManager.Roles.ToListAsync();
            model.EmployeeTypes = await _databaseContext.EmployeeTypes.ToListAsync();
            model.Countries = await _databaseContext.Countries.ToListAsync();
            model.Titles = await _databaseContext.Titles.ToListAsync();
            //model.Departments = await _databaseContext.Departments.ToListAsync();
            //model.Sections = await _databaseContext.Sections.ToListAsync();
            model.Units = await _databaseContext.Units.ToListAsync();
            model.RoleModulePermissions = model.RoleModulePermissions;
            //Get the current Logged-in user 

            ApplicationUser user = await GetCurrentUserAsync();

            if (ModelState.IsValid)
            {
                if (await UserEmailExists(model.Email))
                {
                    Notify(Constants.toastr, "Failed!", "Email already exists.", notificationType: NotificationType.warning);
                    //return View(nameof(Create), emptyEmployee);
                    return View(model);
                }

                if (await EmployeeStaffNumberExists(model.StaffNumber) || model.StaffNumber is null)
                {
                    Notify(Constants.toastr, "Failed!", "Staff number already exists.", notificationType: NotificationType.error);
                    //return View(nameof(Create), emptyEmployee);
                    return View(model);

                }

                if (model.RoleName == "0" || model.RoleName == string.Empty)
                {
                    Notify(Constants.toastr, "Failed!", "Role is required. Select a Role.", notificationType: NotificationType.error);
                    //return View(nameof(Create), emptyEmployee);
                    return View(model);

                }

                string fileExtension;
                string storeFileName = user.PhotoPath;
                string storePath = string.Empty;
                string filePath = FileStorePath.noUserPhotoPath;
                string fileUrl = FileStorePath.noUserPhotoPath;
                if (model.Photo != null)
                {
                    //Guid g = new();
                    //string NewFileName = g.ToString() + fileExtension;
                    fileExtension = Path.GetExtension(model.Photo.FileName);
                    string NewFileName = model.StaffNumber + fileExtension;
                    fileUrl = FileStorePath.UserPhotoDirectory + NewFileName;
                    storeFileName = NewFileName;
                    storePath = Path.Combine(_webHostEnvironment.WebRootPath, FileStorePath.UserPhotoDirectoryName);
                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }

                    filePath = Path.Combine(storePath, NewFileName);
                }

                var company = await _databaseContext.Company.FirstOrDefaultAsync(x => x.Id == 1);

                ApplicationUser applicationUser = new()
                {
                    Title = model.Title,
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    OtherName = model.OtherName,
                    PhotoPath = storeFileName,
                    PhoneNumber = model.PhoneNumber,
                    JobTitle = model.JobTitle,
                    Company = string.IsNullOrEmpty(model.Company) ? company.CompanyName : model.Company,
                    CountryId = model.CountryId,
                    Gender = model.Gender,
                    DOB = model.DOB,
                    PostalAddress = model.PostalAddress,
                    IsResetMode = false,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = user?.Id,
                    StaffNumber = model.StaffNumber,
                    UnitId = model.UnitId,
                    ////SectionId = model.SectionId,
                    StartDate = model.DateStarted,
                    EndDate = model.DateEnded,
                    EmployeeTypeId = model.EmployeeTypeId,


                };
                ApplicationRole selectedRole = await _roleManager.FindByIdAsync(model.RoleId.ToString());
                List<ModulePermission> permissions = [];


                using var transaction = _databaseContext.Database.BeginTransaction();
                try
                {
                    await _userManager.CreateAsync(applicationUser, model.Password);
                    await _userManager.AddToRoleAsync(applicationUser, selectedRole.Name);
                    _databaseContext.SaveChanges();



                    //Add User role module permissions 


                    foreach (var permission in model.RoleModulePermissions)
                    {
                        ModulePermission singlePermission = new()
                        {
                            Create = permission.Create,
                            Read = permission.Read,
                            Update = permission.Update,
                            Delete = permission.Delete,
                            Configure = permission.Configure,
                            Report = permission.Report,
                            Approve = permission.Approve,
                            UserId = applicationUser.Id,
                            RoleId = permission.RoleId,
                            ModuleId = permission.ModuleId,
                            CreatedBy = user?.Id,
                            DateCreated = DateTime.UtcNow,

                        };
                        permissions.Add(singlePermission);
                    }


                    //Log creator action in audit trail
                    AuditLog trail = new()
                    {
                        ActionType = "CREATE",
                        ActionDescription = $"Created new user: {applicationUser.FullName}. Id : {applicationUser.Id}",
                        ActionDate = DateTime.UtcNow,
                        ActionById = user.Id,
                        ActionByFullname = user.FullName,
                        //ActionInfo=$"Controller:"
                    };

                    _databaseContext.SaveChanges();

                    transaction.Commit();
                    // Save Picture
                    if (model.Photo != null && !string.IsNullOrEmpty(filePath))
                    {
                        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                        // Write to the file here
                        await model.Photo.CopyToAsync(stream); //Save image
                        //await model.Photo.CopyToAsync(new FileStream(filePath, FileMode.Create)); //Save image
                    }


                    //Generate Email Confirmation Token
                    var emailConfirmCode = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                    emailConfirmCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmCode));
                    //Generate Password Reset Token
                    List<EmailAddress> emailRecipients = new() { new EmailAddress() { Name = applicationUser.FirstName, Address = applicationUser.Email } };
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { email = applicationUser.Email, ecode = emailConfirmCode }, protocol: Request.Scheme);
                    EmailMessage mailMessage = new();
                    mailMessage.Subject = "Account Confirmation";
                    mailMessage.Body = $"Your account has been successfully created. To activate it, please click on the button below to verify your email address and reset your password. Once activated, you’ll have access to the application.";
                    mailMessage.BodyB = $"This link will expire in 30 minutes. If you did not request it, no further action is required.";
                    mailMessage.EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.ConfirmEmailTemplateFile;
                    mailMessage.EmailLink = $"<a class='linkButton' target='_blank' href='{HtmlEncoder.Default.Encode(callbackUrl)}'> Activate Account </a>.";
                    //< a href = "https://keenthemes.com/account/confirm/07579ae29b6?email=max%40kt.com" rel = "noopener" style = "text-decoration:none;display:inline-block;text-align:center;padding:0.75575rem 1.3rem;font-size:0.925rem;line-height:1.5;border-radius:0.35rem;color:#ffffff;background-color:#009EF7;border:0px;margin-right:0.75rem!important;font-weight:600!important;outline:none!important;vertical-align:middle" target = "_blank" > Activate Account </ a >
                    mailMessage.ToAddresses = emailRecipients;
                    mailMessage.Company = await GetCompanyDetailsHtml();
                    mailMessage.App = "salesngin";
                    BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(mailMessage));
                    //await _mailService.SendEmail(mailMessage);



                    Notify(Constants.sweetAlert, "Success!", "User account created successfully.", notificationType: NotificationType.success);
                    return RedirectToAction(nameof(Users), "Account");
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    transaction.Rollback();
                    Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
                }
            }
            return View(nameof(Create), model);
        }

        [HttpGet]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            IEnumerable<Unit> unitList = await _databaseContext.Units.ToListAsync();
            ViewBag.Units = new SelectList(unitList, "Id", "UnitName");

            //get data to bind to dropdown list
            var employeeList = await _databaseContext.Users.ToListAsync();

            //Get the section to be edited
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }


            //Create the Section View model object 
            ApplicationUserViewModel model = new()
            {
                //SectionName = selectedEmployee.Section?.SectionName,
                //DepartmentId = selectedEmployee.Section?.Department?.Id,
                //DepartmentName = selectedEmployee.Section?.Department?.DepartmentName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OtherName = user.OtherName,
                PhotoPath = user.PhotoPath,
                OldPhotoPath = user.PhotoPath,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserLoggedIn = loggedInUser
            };
            //pass the object to the view 
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeModuleAction(ConstantModules.User_Module, ConstantPermissions.Update)]
        public async Task<IActionResult> Edit(ApplicationUserViewModel Employee)
        {
            //IEnumerable<Department> departmentList = await _context.Departments.ToListAsync();
            //ViewBag.Departments = new SelectList(departmentList, "Id", "DepartmentName");
            //if (id != Employee.Id)
            //{
            //    return NotFound();
            //}

            //get data to bind to dropdown list
            var employeeList = await _databaseContext.Users.ToListAsync();
            //exclude password from the model state object 
            //ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                try
                {
                    //var employeeToUpdated = await _context.Users.FirstOrDefaultAsync(u => u.Id == Employee.Id);
                    //if (employeeToUpdated == null)
                    //{
                    //    Notify(Constants.sweetAlert,"Warning!", $"User { Employee.FullName } cannot be found", notificationType: NotificationType.warning);
                    //    return View(Employee);
                    //}
                    //var userToUpdated = await _userManager.FindByIdAsync(Employee.Id);
                    var userToUpdated = await _databaseContext.Users.FirstOrDefaultAsync(e => e.Id == Employee.Id);
                    if (userToUpdated == null)
                    {
                        Notify(Constants.sweetAlert, "Warning!", $"User {Employee.FullName} cannot be found", notificationType: NotificationType.warning);
                        return View(nameof(Edit), new { id = Employee.Id });
                    }
                    else
                    {
                        var employeeToUpdated = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == Employee.Id);
                        if (employeeToUpdated == null)
                        {
                            Notify(Constants.sweetAlert, "Warning!", $"employee details cannot be found", notificationType: NotificationType.warning);
                            return View(Employee);
                        }
                        bool isStaffNumberValid = employeeToUpdated.StaffNumber != Employee.StaffNumber && await EmployeeStaffNumberExists(Employee.StaffNumber);
                        bool isEmailValid = userToUpdated.Email != Employee.Email && await UserEmailExists(Employee.Email);
                        if (isStaffNumberValid == false && isEmailValid == false)
                        {
                            var usr = await GetCurrentUserAsync();

                            string fileExtension;
                            string storeFileName = Employee.PhotoPath;
                            string filePath = string.Empty;

                            if (Employee.EmployeePhoto != null)
                            {
                                //fileExtension = Path.GetExtension(Employee.EmployeePhoto.FileName);
                                //string NewFileName = Employee.StaffNumber + fileExtension;
                                //UserPhotoDirectory += NewFileName;
                                //storeFileName = NewFileName;
                                //filePath = Path.Combine(_webHostEnvironment.WebRootPath, UserPhotoDirectory);

                                Guid g = new();
                                fileExtension = Path.GetExtension(Employee.EmployeePhoto.FileName);
                                //string NewFileName = Employee.StaffNumber + fileExtension;
                                string NewFileName = g.ToString() + fileExtension;
                                string path = FileStorePath.UserPhotoDirectory + NewFileName;
                                storeFileName = NewFileName;
                                filePath = Path.Combine(_webHostEnvironment.WebRootPath, path);

                            }
                            else { }

                            Employee.OldPhotoPath = userToUpdated.PhotoPath;
                            userToUpdated.UserName = Employee.Email;
                            userToUpdated.Email = Employee.Email;
                            userToUpdated.FirstName = Employee.FirstName;
                            userToUpdated.LastName = Employee.LastName;
                            userToUpdated.OtherName = Employee.OtherName;
                            userToUpdated.PhotoPath = storeFileName;
                            userToUpdated.PhoneNumber = Employee.PhoneNumber;
                            //userToUpdated.StaffNumber = Employee.StaffNumber;
                            userToUpdated.JobTitle = Employee.JobTitle;
                            //userToUpdated.DepartmentId = Employee.DepartmentId == 0 ? null : Employee.DepartmentId;
                            //userToUpdated.SectionId = Employee.SectionId == 0 ? null : Employee.SectionId;
                            //userToUpdated.UnitId = Employee.UnitId == 0 ? null : Employee.UnitId;
                            userToUpdated.DateModified = DateTime.UtcNow;
                            userToUpdated.ModifiedBy = usr?.Id;
                            _databaseContext.Update(userToUpdated);
                            var result = _databaseContext.SaveChanges();
                            if (result > 0)
                            {
                                Notify(Constants.sweetAlert, "Success!", "User record has been updated.", notificationType: NotificationType.success);

                                if (Employee.EmployeePhoto != null && !string.IsNullOrEmpty(filePath))
                                {
                                    await Employee.EmployeePhoto.CopyToAsync(new FileStream(filePath, FileMode.Create)); //Save image
                                }
                                else { }

                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                Notify(Constants.sweetAlert, "Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.info);
                            }

                            //using var transaction = _context.Database.BeginTransaction();
                            //try
                            //{
                            //    await _userManager.UpdateAsync(userToUpdated);
                            //    await _context.SaveChangesAsync();

                            //    transaction.Commit();
                            //    Notify(Constants.sweetAlert,"Success!", "User record has been modified.", notificationType: NotificationType.success);
                            //    return RedirectToAction(nameof(Index));
                            //}
                            //catch (Exception ex)
                            //{
                            //    // Commit transaction if all commands succeed, transaction will auto-rollback
                            //    // when disposed if either commands fails
                            //    transaction.Rollback();

                            //    Notify(Constants.sweetAlert,"Failed!", "Something went wrong. record not updated.", notificationType: NotificationType.info);
                            //    //_logger.LogError("Something went wrong updating user record. Record not updated. Error : " + ex.Message);
                            //}
                        }
                        else
                        {
                            Notify(Constants.sweetAlert, "Record Exists!", "A Record with same Staff Number : " + Employee.StaffNumber + " or Email : " + Employee.Email + " already exists. ", notificationType: NotificationType.info);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (await _userManager.FindByIdAsync(Employee.Id.ToString()) == null)
                    {
                        _ = ex.Message;
                        Notify(Constants.sweetAlert, "Info!", "404. Record not Found. " + ex.Message, notificationType: NotificationType.info);
                    }
                    else
                    {
                        Notify(Constants.sweetAlert, "Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.error);
                        _ = ex.Message;
                    }
                }
                //return RedirectToAction(nameof(Index));
            }


            //IEnumerable<Section> sectionList = await _context.Sections.Where(d => d.DepartmentId == Employee.DepartmentId).ToListAsync();
            //ViewBag.Sections = new SelectList(sectionList, "Id", "SectionName");
            //if (Employee.UnitId != 0 || Employee.UnitId != null)
            //{
            //    IEnumerable<Unit> unitList = await _context.Units.Where(s => s.SectionId == Employee.SectionId).ToListAsync();
            //    ViewBag.Units = new SelectList(unitList, "Id", "UnitName");
            //}

            IEnumerable<Unit> unitList = await _databaseContext.Units.ToListAsync();
            ViewBag.Units = new SelectList(unitList, "Id", "UnitName");


            return View(nameof(Edit), new { id = Employee.Id });

            //return View(nameof(Index));
        }

        private async Task<bool> EmployeeStaffNumberExists(string staffNumber)
        {
            //return await _context.Users.AnyAsync(u => u.StaffNumber == staffNumber);
            return await _databaseContext.Users.AnyAsync(u => u.StaffNumber == staffNumber);
        }

        [HttpPost, ActionName("AssignToRole")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApplicationRoles.SuperAdministrator},{ApplicationRoles.Staff}")]
        public async Task<IActionResult> AssignToRole(ApplicationRoleViewModel model)
        {

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.UserId == null || model.UserId == 0)
                    {
                        Notify(Constants.sweetAlert, "Empty Field!", "Select a User. ", notificationType: NotificationType.info);
                    }
                    else if (string.IsNullOrEmpty(model.RoleName) || model.RoleName == "0")
                    {
                        Notify(Constants.sweetAlert, "Empty Field!", "Select a Role. ", notificationType: NotificationType.info);
                    }
                    else
                    {
                        //Check if record exists
                        var roleChange = string.Empty;
                        //pass selected role from view 
                        if (!string.IsNullOrEmpty(model.RoleName)) { roleChange = await _roleManager.RoleExistsAsync(model.RoleName) == true ? model.RoleName : string.Empty; }
                        if (!string.IsNullOrEmpty(roleChange))
                        {
                            //_roleManager.FindByNameAsync(model.RoleName)
                            //if not empty, get the user assigned to the role 
                            //var userAssignedToRole = await _modelQueryManager.GetUserAssignedTo(roleChange);
                            //Get Users asigned to that role 
                            //var usersInRole = await _modelQueryManager.GetUsersAssignedTo(roleChange);
                            //get the role id and details 
                            var role = await _roleManager.FindByNameAsync(roleChange);
                            if (role == null)
                            {
                                var roleChangeDescription = string.Empty;
                                //roleChangeDescription = roleChange switch
                                //{
                                //    ApplicationRoles.Staff => "Standards Safety Q.A Manager Role",
                                //    ApplicationRoles.Administrator => "Chief of Facility Role",
                                //    _ => string.Empty,
                                //};
                                //Dosent exist, so create new role using roleManager
                                ApplicationRole newRole = new() { Name = roleChange, RoleDescription = roleChangeDescription };
                                newRole.DateCreated = DateTime.UtcNow;
                                IdentityResult result = await _roleManager.CreateAsync(newRole);
                                role = newRole;
                            }
                            else { }


                            ApplicationUser user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
                            if (user != null)
                            {
                                //Assign Selected User to Selected Role 
                                if (await _userManager.IsInRoleAsync(user, roleChange) == true)
                                {
                                    Notify(Constants.sweetAlert, "Failed!", user.FullName + " is already assigned to " + roleChange + " role ", notificationType: NotificationType.info);
                                }
                                else
                                {
                                    //Get all users roles 
                                    //var userRoles = await _userManager.GetRolesAsync(user);
                                    //if (userRoles.Count > 0)
                                    //{
                                    //    foreach (var item in userRoles)
                                    //    {
                                    //        //remove user from role
                                    //        await _userManager.RemoveFromRoleAsync(user, item);
                                    //    }
                                    //}

                                    //remove user currently assigned to the Selected Role 
                                    //if (userAssignedToRole != null)
                                    //{
                                    //    await _userManager.RemoveFromRoleAsync(userAssignedToRole.User, roleChange);
                                    //    //Set the user role to default staff
                                    //    var userRoles = await _userManager.GetRolesAsync(userAssignedToRole.User);
                                    //    if (userRoles.Count <= 0)
                                    //    {
                                    //        await _userManager.AddToRoleAsync(userAssignedToRole.User, ApplicationRoles.Staff);
                                    //    }
                                    //}

                                    //Change existing users with master status to slave
                                    //List<UserRoleLevel> allUsersInSelectedRole = new();
                                    //var usersInRoleLevel = await _context.UserRoleLevels.Include(u => u.User).Include(r => r.Role).Where(r => r.RoleId == role.Id).ToListAsync();
                                    //if (usersInRoleLevel.Count > 0)
                                    //{
                                    //    foreach (var userRole in usersInRoleLevel)
                                    //    {
                                    //        if (model.IsMaster == true)
                                    //        {
                                    //            userRole.IsMaster = false;
                                    //            _context.SaveChanges();
                                    //        }
                                    //    }
                                    //}

                                    //var recordType = 0;
                                    //var selectedUserRoleLevel = await _context.UserRoleLevels.Include(u => u.User).Include(r => r.Role).FirstOrDefaultAsync(r => r.UserId == user.Id && r.RoleId == role.Id);
                                    //if (selectedUserRoleLevel != null)
                                    //{
                                    //    recordType = 1;
                                    //    selectedUserRoleLevel.IsMaster = model.IsMaster;
                                    //}
                                    //else
                                    //{
                                    //    recordType = 0;
                                    //    //selectedUserRoleLevel.UserId = user.Id;
                                    //    //selectedUserRoleLevel.RoleId = role.Id;
                                    //    //selectedUserRoleLevel.IsMaster = model.IsMaster;
                                    //    //selectedUserRoleLevel.DateAdded = DateTime.UtcNow;
                                    //}



                                    using var transaction = _databaseContext.Database.BeginTransaction();
                                    try
                                    {
                                        // Add the user to the Role
                                        await _userManager.AddToRoleAsync(user, roleChange);

                                        //Change the existing user's role level master to false if the new user's master level is true
                                        //if (usersInRoleLevel.Count > 0)
                                        //{
                                        //    foreach (var userRole in usersInRoleLevel)
                                        //    {
                                        //        if (model.IsMaster == true && userRole.IsMaster == true)
                                        //        {
                                        //            userRole.IsMaster = false;
                                        //            _context.SaveChanges();
                                        //        }
                                        //    }
                                        //}

                                        //Add the new user's master role level if it doesn't exist in the table
                                        //if (selectedUserRoleLevel is null)
                                        //{
                                        //    UserRoleLevel newLevel = new()
                                        //    {
                                        //        UserId = user.Id,
                                        //        RoleId = role.Id,
                                        //        IsMaster = model.IsMaster,
                                        //        DateAdded = DateTime.UtcNow
                                        //    };
                                        //    await _context.AddAsync(newLevel);
                                        //}
                                        //else
                                        //{
                                        //    selectedUserRoleLevel.IsMaster = model.IsMaster;
                                        //}
                                        //await _context.SaveChangesAsync();

                                        transaction.Commit();

                                        Notify(Constants.sweetAlert, "Success!", $"{user.FirstName} , has been assigned {roleChange} role.", notificationType: NotificationType.success);

                                    }
                                    catch (Exception ex)
                                    {
                                        // Commit transaction if all commands succeed, transaction will auto-rollback
                                        _ = ex.Message;
                                        transaction.Rollback();
                                        //if the operation wasnt successful, add error message to model state and return the view 
                                        Notify(Constants.sweetAlert, "Failed!", "Something went wrong adding the role to this user account.", notificationType: NotificationType.info);
                                    }




                                    //IdentityResult addResult = await _userManager.AddToRoleAsync(user, roleChange);
                                    //if (addResult.Succeeded)
                                    //{
                                    //    Notify(Constants.sweetAlert,"Success!", user.FullName + " has been assigned " + roleChange + " role", notificationType: NotificationType.success);
                                    //}
                                    //else { Notify(Constants.sweetAlert,"Failed!", "Something Went wrong. ", notificationType: NotificationType.info); }
                                }
                            }
                            else
                            {
                                Notify(Constants.sweetAlert, "Failed!", "Something Went wrong. ", notificationType: NotificationType.info);
                            }
                        }
                        else { Notify(Constants.sweetAlert, "Failed!", "Role cannot be empty. ", notificationType: NotificationType.info); }
                    }
                }
                catch (Exception ex)
                {
                    Notify(Constants.sweetAlert, "Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.error);
                }
            }
            //appRole.Roles = (IEnumerable<ApplicationRole>)_roleManager.Roles.ToList();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("RemoveUserFromRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(ApplicationRoleViewModel model)
        {
            if (model == null) { return RedirectToAction(nameof(Index)); }

            try
            {

                ApplicationRole roleToDelete = new();

                roleToDelete = await _roleManager.FindByNameAsync(model.RoleName);
                var userToRemove = await _databaseContext.Users.FindAsync(model.UserId);

                if (roleToDelete != null && userToRemove != null)
                {
                    IdentityResult result = new();
                    //var roleLevel = await _context.UserRoleLevels.FirstOrDefaultAsync(x => x.UserId == userToRemove.Id && x.RoleId == roleToDelete.Id);

                    using var transaction = _databaseContext.Database.BeginTransaction();
                    try
                    {
                        await _userManager.RemoveFromRoleAsync(userToRemove, roleToDelete.Name);
                        //if (roleLevel is not null)
                        //{
                        //    _context.Remove(roleLevel);
                        //}
                        await _databaseContext.SaveChangesAsync();

                        transaction.Commit();

                        Notify(Constants.sweetAlert, "Success!", "User has been removed from Role.", notificationType: NotificationType.success);
                        return RedirectToAction(nameof(Index));



                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                        // Commit transaction if all commands succeed, transaction will auto-rollback
                        transaction.Rollback();
                        //if the operation wasnt successful, add error message to model state and return the view 
                        Notify(Constants.sweetAlert, "Failed!", "Something went wrong. role not removed.", notificationType: NotificationType.info);
                        ////_logger.LogError("Something went wrong creating user account. Error : " + ex.Message);
                        return View(nameof(Index));
                    }


                    //if the returned list count is greater than 0, means there are other roles assigned
                    //remove this user from the named roles 
                    //result = await _userManager.RemoveFromRoleAsync(userToRemove, roleToDelete.Name);
                    //if (!result.Succeeded)
                    //{
                    //    //if the operation wasnt successful, add error message to model state and return the view 
                    //    Notify(Constants.sweetAlert,"Failed!", "Something Went wrong.Cannot remove user from role ", notificationType: NotificationType.info);
                    //    return View(nameof(SmsRoles));
                    //}

                    ////var employee = await _context.Employees.FirstOrDefaultAsync(u => u.UserId == userToRemove.Id);
                    //Notify(Constants.sweetAlert,"Success!", "User has been removed from Role.", notificationType: NotificationType.success);
                    //return RedirectToAction(nameof(SmsRoles));
                }
                else
                {
                    Notify(Constants.sweetAlert, "Failed!", "Record does not exist. ", notificationType: NotificationType.info);
                }
            }
            catch (Exception ex)
            {
                Notify(Constants.sweetAlert, "Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.info);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> UploadImage(string olImagePath, string uniqueNumber, IFormFile file)
        {
            string storeFileName = String.Empty;
            try
            {

                //you can replace the guid and filename with the user id etc
                //UserPhotoFolder += Guid.NewGuid().ToString() + "_" + file.FileName;
                string fileExtension = Path.GetExtension(file.FileName);
                string path = FileStorePath.UserPhotoDirectory + uniqueNumber + fileExtension;
                //UserPhotoDirectory += uniqueNumber + fileExtension;
                storeFileName = uniqueNumber + fileExtension;
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, path);

                if (!string.IsNullOrEmpty(olImagePath))
                {
                    //check if the old image exists
                    if (System.IO.File.Exists(olImagePath))
                    {
                        try
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            System.IO.File.Delete(olImagePath);
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Message;

                        }

                    }
                }

                if (System.IO.File.Exists(filePath))
                {
                    //check if the new image exists
                    try
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;

                    }
                }

                await file.CopyToAsync(new FileStream(filePath, FileMode.Create)); //Save image

            }
            catch (IOException ex)     // Should capture access exception
            {
                // Show error; do nothing; etc.
                _ = ex.Message;
            }
            //return "/" + UserPhotoDirectory;
            return storeFileName;
        }

        //public async Task<JsonResult> GetSections(int id)
        //{
        //    var sectionList = await _context.Sections.Where(d => d.DepartmentId == id).ToListAsync();
        //    return Json(new SelectList(sectionList, "Id", "SectionName"));
        //}

        public async Task<JsonResult> GetUnits(int id)
        {
            //var unitList = await _context.Units.Where(d => d.SectionId == id).ToListAsync();
            var unitList = await _databaseContext.Units.ToListAsync();
            return Json(new SelectList(unitList, "Id", "UnitName"));
        }


        [HttpGet]
        public async Task<PartialViewResult> AddUsersToRole()
        {
            UsersRoleViewModel model = new()
            {
                Units = await _databaseContext.Units.ToListAsync()
            };

            return PartialView("_UsersToRoleAdd", model);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetUserPermissionsModal(int id)
        {
            ApplicationUser loggedInUser = await GetCurrentUserAsync();
            UserViewModel model = await _dataService.GetUserProfile(id);
            model.ModulePermission = await _dataService.GetModulePermission(ConstantModules.User_Module, model.User.Id);
            model.UserLoggedIn = loggedInUser;

            return PartialView("_UserPermissionsInput", model);
        }


        [HttpGet]
        public async Task<PartialViewResult> GetRolePermissions(int id, int? uid)
        {
            ApplicationRole selectedRole = new();
            List<RoleModule> roleModules = new();
            List<ModulePermission> modulePermissions = new();
            selectedRole = await _roleManager.FindByIdAsync(id.ToString());
            if (selectedRole != null)
            {
                roleModules = await _databaseContext.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(r => r.RoleId == selectedRole.Id).ToListAsync();
                if (uid != null)
                {
                    modulePermissions = await _databaseContext.ModulePermissions.Include(r => r.Role).Include(m => m.Module).Include(u => u.User).Where(m => m.UserId == uid && m.RoleId == selectedRole.Id).ToListAsync();
                }
            }

            if (roleModules.Count > 0)
            {
                foreach (var module in roleModules)
                {

                    ModulePermission modper = new()
                    {
                        ModuleId = module.ModuleId,
                        Module = module.Module,
                        RoleId = module.RoleId,
                        Role = module.Role,
                    };

                    if (!modulePermissions.Contains(modper))
                    {
                        modulePermissions.Add(modper);
                    }
                }
            }

            UserViewModel model = new()
            {
                UserRole = selectedRole,
                RoleModules = roleModules,
                RoleModulePermissions = modulePermissions,
            };

            return PartialView("_AddModulePermission", model);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetUserPermissions(int id, int uid)
        {
            ApplicationRole selectedRole = new();
            List<RoleModule> roleModules = new();
            List<ModulePermission> userModulePermissions = new();

            //Get the selected user
            ApplicationUser selectedUser = await _userManager.FindByIdAsync(uid.ToString());
            //if(selectedUser != null)
            //{
            //    //get the users modulePermissions
            //    userModulePermissions = await _context.ModulePermissions.Include(m => m.Module).Include(r => r.Role).Include(u => u.User).Where(u => u.UserId == selectedUser.Id).ToListAsync();
            //}
            //Get the selected Role
            selectedRole = await _roleManager.FindByIdAsync(id.ToString());
            if (selectedRole != null)
            {
                //Get all the modules assigned to the selected role
                roleModules = await _databaseContext.RoleModules.Include(m => m.Module).Include(r => r.Role).Where(r => r.RoleId == selectedRole.Id).ToListAsync();
            }

            userModulePermissions = await _dataService.GetUserRoleModulePermissions(selectedUser.Id, selectedRole.Id);

            UserViewModel model = new()
            {
                UserRole = selectedRole,
                RoleModules = roleModules,
                //RoleModulePermissions = modulePermissions,
                RoleModulePermissions = userModulePermissions,
            };

            return PartialView("_UserPermissionsInput", model);
        }

        [HttpGet]
        public async Task<string> GetTemporalStaffNumber(int? id)
        {
            string result = string.Empty;
            if (id != null)
            {
                var employeeType = await _databaseContext.EmployeeTypes.FirstOrDefaultAsync(x => x.Id == id);
                if (employeeType != null)
                {
                    if (employeeType.Name == Constants.TemporalStaff)
                    {
                        //Generate
                        result = await GenerateTemporalStaffNumber(employeeType.Id);
                        //var temporalStaff = await _context.Employees.Where(e => e.EmployeeTypeId == employeeType.Id).ToListAsync();
                        //var lastRecordNumber = temporalStaff?.Count;
                        //do
                        //{
                        //    //Temporal Staff Code format TS-01
                        //    lastRecordNumber += 1;
                        //    result = $"TS{lastRecordNumber:D2}";

                        //} while (await _context.Employees.AnyAsync(e => e.StaffNumber == result) == true);
                    }
                }
            }

            return result;
        }

        [HttpGet]
        public async Task<string> GetStaffNumber(int? id, int? eid)
        {
            string result = string.Empty;

            if (eid != null)
            {
                var employee = await _databaseContext.Users.FirstOrDefaultAsync(e => e.Id.Equals(eid));
                if (employee != null)
                {
                    if (id != null)
                    {
                        var employeeType = await _databaseContext.EmployeeTypes.FirstOrDefaultAsync(x => x.Id == id);
                        if (employeeType != null)
                        {
                            if (employee.EmployeeTypeId != employeeType.Id)
                            {
                                if (employeeType.Name == Constants.TemporalStaff)
                                {
                                    result = await GenerateTemporalStaffNumber(eid);
                                }
                                else { result = employee.StaffNumber; }
                            }
                            else
                            {
                                result = employee.StaffNumber;
                            }

                        }
                    }
                }
            }


            return result;
        }

        private async Task<string> GenerateTemporalStaffNumber(int? id)
        {
            string result = string.Empty;
            //Generate
            var temporalStaff = await _databaseContext.Users.Where(e => e.EmployeeTypeId == id).ToListAsync();
            var lastRecordNumber = temporalStaff?.Count;
            do
            {
                //Temporal Staff Code format TS-01
                lastRecordNumber += 1;
                result = $"TS{lastRecordNumber:D2}";

            } while (await _databaseContext.Users.AnyAsync(e => e.StaffNumber == result) == true);

            return result;
        }

        private async Task<IActionResult> SendEmailConfirmationTokenAsync(string userEmail)
        {
            var setting = await _databaseContext.ApplicationSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1);
            //string code = await _userManager.GenerateEmailConfirmationTokenAsync(userID);
            //var callbackUrl = Url.Action("ConfirmEmail", "Account",
            //   new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            //await UserManager.SendEmailAsync(userID, subject,
            //   "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return RedirectToAction(actionName: "Profile", controllerName: "Account");
        }


        public IActionResult CheckSessionStatus()
        {
            //bool isSessionExpired = /* Logic to check session expiry */;
            bool isSessionExpired = true;

            // Check if the session exists and if it has expired
            if (HttpContext.Session.TryGetValue("POSCookie", out byte[] lastActivityBytes))
            {
                DateTime lastActivity = new DateTime(BitConverter.ToInt64(lastActivityBytes, 0));

                TimeSpan sessionTimeout = TimeSpan.FromMinutes(30); // Define your session timeout

                if (DateTime.UtcNow - lastActivity > sessionTimeout)
                {
                    // Session has expired
                    // You can redirect the user to a login page or take appropriate action
                    //return RedirectToAction("Login", "Account"); // Example redirect
                    isSessionExpired = true;
                }
                else
                {
                    isSessionExpired = false;
                    // Session is still valid, update the last activity timestamp
                    HttpContext.Session.Set("POSCookie", BitConverter.GetBytes(DateTime.UtcNow.Ticks));
                }
            }
            else
            {
                // Session doesn't exist or has expired, handle accordingly
                isSessionExpired = true;
                // Redirect to login, show error, etc.
                return RedirectToAction("Login", "Account"); // Example redirect
            }
            return Json(new { isSessionExpired });
        }

        [HttpPost]
        public IActionResult ExtendSession()
        {
            // Update the session expiration time to extend the session
            //HttpContext.Session.SetInt32("LastActivity", 1);
            if (HttpContext.Session.GetInt32("POSCookie") == 1)
            {
                HttpContext.Session.SetInt32("POSCookie", 0); // Reset the extension flag
                HttpContext.Session.SetInt32("POSCookie", (int)TimeSpan.FromMinutes(30).TotalSeconds); // Extend the session
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddUsersToRole(UsersRoleViewModel model)
        //{
        //    if (model == null) { return RedirectToAction(nameof(Users)); }
        //    try
        //    {
        //        var selectedRole = await _roleManager.FindByNameAsync(model.RoleName);
        //        var selectedSection = await _context.Sections.FirstOrDefaultAsync(s => s.Id == model.SectionId);
        //        if (selectedRole == null)
        //        {
        //            Notify(Constants.sweetAlert,"Failed!", "Selected Role dose not exist.", notificationType: NotificationType.info);
        //        }
        //        else if (selectedSection == null)
        //        {
        //            Notify(Constants.sweetAlert,"Failed!", "Selected section dose not exist.", notificationType: NotificationType.info);
        //        }
        //        else
        //        {


        //            var usersToAdd = await _context.Users.Where(u => u.SectionId == selectedSection.Id).ToListAsync();
        //            //var usersToAdd = await _userManager.GetUsersInRoleAsync(model.RoleName);
        //            if (usersToAdd.Count > 0)
        //            {
        //                string[] rolesToExclude = { ApplicationRoles.Administrator, ApplicationRoles.SuperAdministrator, ApplicationRoles.Staff };

        //                //Iterate through list
        //                //Todo : List Compare - if (nums1.Any(x => nums2.Any(y => y == x)))
        //                var usersAdded = 0;
        //                foreach (var user in usersToAdd)
        //                {
        //                    //get users roles
        //                    var userRoles = await _userManager.GetRolesAsync(user);

        //                    if (!userRoles.Contains(model.RoleName))
        //                    {
        //                        if (!rolesToExclude.Contains(model.RoleName))
        //                        {
        //                            //Add the user to the role 
        //                            await _userManager.AddToRoleAsync(user, model.RoleName);
        //                            usersAdded++;
        //                        }
        //                        else { }
        //                    }
        //                    else { }
        //                    //if (rolesToExclude.Any(x => userRoles.Any(y => y == x)))
        //                    //{
        //                    //    //Console.WriteLine("There are equal elements");
        //                    //}
        //                    //else
        //                    //{
        //                    //    //Console.WriteLine("No Match Found!");
        //                    //}
        //                }

        //                _context.SaveChanges();

        //                if (usersAdded > 0)
        //                {
        //                    Notify(Constants.sweetAlert,"Success!", $"{usersAdded} Users have been added to {model.RoleName} Role.", notificationType: NotificationType.success);
        //                }
        //                else
        //                {
        //                    Notify(Constants.sweetAlert,"Failed!", "No users added.", notificationType: NotificationType.info);

        //                }
        //            }
        //            else
        //            {
        //                Notify(Constants.sweetAlert,"Failed!", "No users to add.", notificationType: NotificationType.info);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Notify(Constants.sweetAlert,"Error!", "Something Went wrong. " + ex.Message, notificationType: NotificationType.info);
        //    }

        //    return RedirectToAction(nameof(Users));
        //}


        //TODO : Transaction Code
        //using var transaction = _context.Database.BeginTransaction();
        //try{
        //    transaction.Commit();
        //}
        //catch (Exception ex)
        //{
        //   _ = ex.Message;
        //   // Commit transaction if all commands succeed, transaction will auto-rollback
        //   transaction.Rollback();
        //   Notify(Constants.toastr, "Failed!", "Something went wrong. record not created.", notificationType: NotificationType.error);
        //}

    }
}
