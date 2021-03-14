using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Utilities;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Services.Model
{
    [Serializable]
    public class UserEditModel
    {
        public Guid MembershipId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام و نام خانوادگی اجباری است!")]
        [StringLength(200)]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [StringLength(150)]
        [Display(Name = "عنوان شغلی")]
        public string JobTitle { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "کاری که من انجام میدهم؟")]
        public string WDYD { get; set; }

        [Display(Name = "ناظر")]
        public IEnumerable<SelectListItem> Users { get; set; }

        [Display(Name = "تیم(ها)")]
        public IEnumerable<SelectListItem> Teams { get; set; }
    }
}
