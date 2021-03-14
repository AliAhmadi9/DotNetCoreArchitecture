using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Services.Model
{
    public class ExternalLoginModel : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        [EmailAddress]
        public string Email { get; set; }

        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }
        public ClaimsPrincipal Principal { get; set; }

        //برای چک کردن منطق تجاری برنامه مناسب است نه مثلا چک ردن نام کاربری تکراری
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
