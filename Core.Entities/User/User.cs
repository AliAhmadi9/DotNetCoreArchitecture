using Core.Entities.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class User : IdentityUser<Guid>, IEntity // BaseEntity
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public Guid? DefaultWorkspace { get; set; }
        public GenderType Gender { get; set; }
        public DateTimeOffset LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(p => p.UserName).IsUnique();
            builder.Property(p => p.UserName).IsRequired().HasMaxLength(100);
            builder.HasMany<UserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            builder.Property(p => p.CreatedDate).HasDefaultValueSql("GetDate()");
        }
    }

    public enum GenderType
    {
        [Display(Name = "مرد")]
        Male = 1,

        [Display(Name = "زن")]
        Female = 2
    }
}
