using System;
using System.ComponentModel.DataAnnotations;
using Common.Utilities;

namespace Services.Model
{
    [Serializable]
    public class ResendEmailModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        [RegularExpression(RegularExpression.Email, ErrorMessage = "ایمیل را با فرمت صحیح وارد کنید")]
        public string Email { get; set; }
    }
}
