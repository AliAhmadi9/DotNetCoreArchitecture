using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Utilities;

namespace Services.Model
{
    [Serializable]
    public class LoginModel : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        //[DataType(DataType.EmailAddress)]
        [RegularExpression(RegularExpression.Email, ErrorMessage = "ایمیل را با فرمت صحیح وارد کنید")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "رمز عبور اجباری است!")]
        [Display(Name = "رمز عبور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "مرا بخاطر بسپار")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //برای چک کردن منطق تجاری برنامه مناسب است نه مثلا چک ردن نام کاربری تکراری
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
