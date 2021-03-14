using Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Services.Model
{
    [Serializable]
    public class SetNewPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "کلمه عبور قدیمی اجباری است!")]
        [Display(Name = "کلمه عبور قدیمی")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "کلمه عبور اجباری است!")]
        [StringLength(100, ErrorMessage = "{0} باید حداقل  {2} کاراکتر و حداکثر {1} کاراکتر باشد.", MinimumLength = 6)]
        [Display(Name = "کلمه عبور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(100)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "تکرار کلمه عبور اجباری است!")]
        [Compare("Password", ErrorMessage = "کلمه عبور و تکرار کلمه عبور باید یکسان باشند")]
        [Display(Name = "تکرار کلمه عبور")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
