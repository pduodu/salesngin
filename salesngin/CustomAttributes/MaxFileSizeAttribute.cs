namespace salesngin.CustomAttributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        //usage
        //[Required(ErrorMessage = "Please select a file.")]
        //[DataType(DataType.Upload)]
        //[MaxFileSize(5 * 1024 * 1024)]
        //[AllowedExtensions(new string[] { ".jpg", ".png" })]
        //public IFormFile Photo { get; set; }


        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is {_maxFileSize} bytes.";
        }
    }
}
