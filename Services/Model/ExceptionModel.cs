using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Services.Dto;

namespace Services.Model
{
    [Serializable]
    public class ExceptionModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "لطفا توضیحاتی بنویسید .")]
        [Display(Name = "متن خطا")]
        public string Exception { get; set; }

        public string UserEmail { get; set; }
    }
}
