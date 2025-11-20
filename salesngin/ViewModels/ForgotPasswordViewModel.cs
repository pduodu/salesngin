namespace salesngin.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Message { get; set; } = null;
    }
}
