namespace salesngin.ViewModels
{
    public class LoginViewModel
    {
        [TempData]
        public string ErrorMessage { get; set; }
        public string ReturnUrl { get; set; }

        public int Id { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public virtual Company Company { get; set; }
        public virtual ApplicationSetting ApplicationSettings { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

    }
}
