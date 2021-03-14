using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities.Base
{
    public abstract class BaseEntity<TKey> : IEntity
    {
        public virtual TKey Id { get; set; }

        [DefaultValue(typeof(DateTime),"GetDate()")] 
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool Archived { get; set; }
    }

    public abstract class BaseEntityTypeConfiguration<TBase> : IEntityTypeConfiguration<TBase>
        where TBase : class, IEntity
    {
        public virtual void Configure(EntityTypeBuilder<TBase> builder)
        {
            //Base Configuration
            //builder.Property("CreatedDate").HasComputedColumnSql("GetDate()");
            //builder.Property("CreatedDate").HasDefaultValueSql("GetDate()");
        }
    }

    public abstract class BaseEntity : BaseEntity<int>
    {
    }
}