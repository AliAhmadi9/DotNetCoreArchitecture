using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Common;
using Common.Utilities;
using Core.Entities;

namespace Services.Model
{
    [Serializable]
    public class CreateUserByEmailModel: IHaveCustomMapping
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        [RegularExpression(RegularExpression.Email, ErrorMessage = "ایمیل را با فرمت صحیح وارد کنید")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام و نام خانوادگی اجباری است!")]
        [StringLength(200)]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        public void CreateMappings(Profile profile)
        {
            profile.CreateMap<User, CreateUserByEmailModel>().ReverseMap();
        }
    }
}
