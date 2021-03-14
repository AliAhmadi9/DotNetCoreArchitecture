using AutoMapper;
using Common;
using Common.Utilities;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Services.Dto
{
    public class UserDto : IHaveCustomMapping, IBaseDTO, IValidatableObject //<=> User
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "شناسه اجباری است!")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام کاربری اجباری است!")]
        [StringLength(100)]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        [RegularExpression(RegularExpression.Email, ErrorMessage = "ایمیل را با فرمت صحیح وارد کنید")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "کلمه عبور اجباری است!")]
        [StringLength(100, ErrorMessage = "{0} باید حداقل  {2} کاراکتر و حداکثر {1} کاراکتر باشد.", MinimumLength = 6)]
        [Display(Name = "کلمه عبور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(100)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "تکرار کلمه عبور اجباری است!")]
        [Compare(nameof(Password), ErrorMessage = "کلمه عبور و تکرار کلمه عبور باید یکسان باشند")]
        [Display(Name = "تکرار کلمه عبور")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام و نام خانوادگی اجباری است!")]
        [StringLength(200)]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        //[StringLength(200)]
        //[Display(Name = "شماره تلفن")]
        //public string PhoneNumber { get; set; }

        //public int Age { get; set; }

        [NotMapped] 
        public string ReturnUrl { get; set; }

        public GenderType Gender { get; set; }

        /// <summary>
        /// add property name to list to update just column in database
        /// </summary>
        public List<string> UpdateProperties { get; set; }

        public void CreateMappings(Profile profile)
        {
            profile.CreateMap<User, UserDto>()
                .ReverseMap();
            //    .ForMember(p => p.Posts, opt => opt.Ignore());
        }

        //برای چک کردن منطق تجاری برنامه مناسب است نه مثلا چک ردن نام کاربری تکراری
        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            if (UserName.Equals("test", StringComparison.OrdinalIgnoreCase))
                yield return new ValidationResult("نام کاربری نمیتواند Test باشد", new[] { nameof(UserName) });
            //if (Password.Equals("123456"))
            //    yield return new ValidationResult("رمز عبور نمیتواند 123456 باشد", new[] { nameof(Password) });
            //if (Gender == GenderType.Male && Age > 30)
            //    yield return new ValidationResult("آقایان بیشتر از 30 سال معتبر نیستند", new[] { nameof(Gender), nameof(Age) });

            //yield return ValidationResult.Success;
        }
    }

    public class UserSelectDto : IHaveCustomMapping //<=> User
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "شناسه اجباری است!")]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام کاربری اجباری است!")]
        [StringLength(100)]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "ایمیل اجباری است!")]
        [Display(Name = "ایمیل")]
        [RegularExpression(RegularExpression.Email, ErrorMessage = "ایمیل را با فرمت صحیح وارد کنید")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "نام و نام خانوادگی اجباری است!")]
        [StringLength(200)]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [StringLength(200)]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        public int Age { get; set; }

        public GenderType Gender { get; set; }

        public List<string> Roles { get; set; }

        public string ReturnUrl { get; set; }

        /// <summary>
        /// add property name to list to update just column in database
        /// </summary>
        public List<string> UpdateProperties { get; set; }

        public void CreateMappings(Profile profile)
        {
            profile.CreateMap<User, UserSelectDto>()
                .ForMember(p => p.Roles, opt => opt.MapFrom(p => p.UserRoles.Select(x => x.Role.Name)))
                .ReverseMap();
            //    .ForMember(p => p.Posts, opt => opt.Ignore());
        }
    }
}
