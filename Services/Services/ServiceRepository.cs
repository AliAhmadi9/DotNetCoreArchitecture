using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Core.Data.Repositories;
using Core.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Services.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class ServiceRepository<TDto, TSelectDto, TEntity, TKey> : IServiceRepository<TDto, TSelectDto, TEntity, TKey>
         where TDto : BaseDto<TDto, TEntity, TKey>, new()
         where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()
         where TEntity : BaseEntity<TKey>, new()
    {
        private readonly IMapper mapper;
        private readonly IRepository<TEntity> repository;

        public ServiceRepository(IMapper mapper, IRepository<TEntity> repository)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        public virtual async Task<List<TSelectDto>> Get(CancellationToken cancellationToken = default)
        {
            var list = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return list;
        }

        public virtual async Task<PagingDto<TSelectDto>> Select(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0)
                throw new BadRequestException($"'{nameof(pageNumber)}' must be greater than or equal to 1, {nameof(pageNumber)} : {pageNumber}");

            if (pageSize <= 0)
                throw new BadRequestException($"'{nameof(pageSize)}' must be greater than or equal to 1, {nameof(pageSize)} : {pageSize}");

            var pagingDto = new PagingDto<TSelectDto>();
            pagingDto.Data = await repository.TableNoTracking
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            pagingDto.TotalRowCount = await repository.TableNoTracking.CountAsync(cancellationToken);
            pagingDto.TotalPages = (int)Math.Ceiling(pagingDto.TotalRowCount / (double)pageSize);
            pagingDto.CurrentPage = pageNumber;

            return pagingDto;
        }

        public virtual async Task<TSelectDto> Get(TKey id, CancellationToken cancellationToken = default)
        {
            var dto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(id), cancellationToken);

            return dto;
        }

        public virtual async Task<TSelectDto> Create(TDto dto, CancellationToken cancellationToken = default)
        {
            var model = dto.ToEntity(mapper);

            await repository.AddAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public virtual async Task<TSelectDto> Update(TKey id, TDto dto, CancellationToken cancellationToken = default)
        {
            dto.Id = id;
            var model = await repository.GetByIdAsync(cancellationToken, id);

            if (model == null)
                throw new NotFoundException($"not found {typeof(TEntity).Name} entity to PKey(Id) : '{dto.Id}'");

            model = dto.ToEntity(mapper, model);

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public virtual async Task<TSelectDto> Update(TDto dto, CancellationToken cancellationToken = default)
        {
            var model = await repository.GetByIdAsync(cancellationToken, dto.Id);

            if (model == null)
                throw new NotFoundException($"not found {typeof(TEntity).Name} entity to PKey(Id) : '{dto.Id}'");

            dto.CreatedDate = model.CreatedDate;
            model = dto.ToEntity(mapper, model);

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public async Task<TSelectDto> UpdateCustomProperties(TDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new NotFoundException("'dto' can not NULL");

            if (dto.UpdateProperties == null || !dto.UpdateProperties.Any())
                throw new NotFoundException("'dto.UpdateProperties' can't be NULL or Empty");

            var model = await repository.GetByIdAsync(cancellationToken, dto.Id);

            if (model == null)
                throw new NotFoundException($"not found {typeof(TEntity).Name} entity to PKey(Id) : '{dto.Id}'");        

            model = dto.ToEntity(mapper, model);

            await repository.UpdateCustomPropertiesAsync(model, cancellationToken, true, dto.UpdateProperties.ToArray());

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public virtual async Task<TSelectDto> UpdateProperty(TKey id, PropertyValueDto dto, CancellationToken cancellationToken = default)
        {
            var entityType = typeof(TEntity);
            var property = entityType.GetProperty(dto.PropertyName);
            if (property == null)
                throw new NotFoundException($"property {dto.PropertyName} not gound in entity : {typeof(TEntity).Name}");

            var model = await repository.GetByIdAsync(cancellationToken, id);

            if (model == null)
                throw new NotFoundException($"not found {typeof(TEntity).Name} entity to PKey(Id) : '{id}'");

            if (dto.PropertyValue != null)
            {
                if (entityType.GetProperty(dto.PropertyName).PropertyType == typeof(Guid))
                {
                    entityType.GetProperty(dto.PropertyName).SetValue(model, new Guid(dto.PropertyValue.ToString()));
                }
                else if (entityType.GetProperty(dto.PropertyName).PropertyType == typeof(Guid?))
                {
                    entityType.GetProperty(dto.PropertyName).SetValue(model, new Guid(dto.PropertyValue.ToString()));
                }
                else
                {
                    var value = Convert.ChangeType(dto.PropertyValue, entityType.GetProperty(dto.PropertyName).PropertyType, CultureInfo.InvariantCulture);
                    entityType.GetProperty(dto.PropertyName).SetValue(model, value);
                }
            }
            else
            {
                //todo check property support null value
                var propertyType = entityType.GetProperty(dto.PropertyName).PropertyType;
                bool canBeNull = !propertyType.IsValueType || (Nullable.GetUnderlyingType(propertyType) != null);
                if (canBeNull)
                    entityType.GetProperty(dto.PropertyName).SetValue(model, null);
                else
                {
                    //todo exception
                }
            }

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public virtual async Task Delete(TKey id, CancellationToken cancellationToken = default)
        {
            //var model = await repository.GetByIdAsync(cancellationToken, id);

            //await repository.DeleteAsync(model, cancellationToken);

            //OR
            await repository.DeleteByIdAsync(id, cancellationToken);
        }
    }

    public class ServiceRepository<TDto, TEntity> : ServiceRepository<TDto, TDto, TEntity, int>, IServiceRepository<TDto, TEntity>
      where TDto : BaseDto<TDto, TEntity, int>, new()
      where TEntity : BaseEntity<int>, new()
    {
        public ServiceRepository(IMapper mapper, IRepository<TEntity> repository) : base(mapper, repository)
        {
        }
    }

    public class ServiceRepository<TDto, TSelectDto, TEntity> : ServiceRepository<TDto, TSelectDto, TEntity, int>, IServiceRepository<TDto, TSelectDto, TEntity>
        where TDto : BaseDto<TDto, TEntity, int>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, int>, new()
        where TEntity : BaseEntity<int>, new()
    {
        public ServiceRepository(IMapper mapper, IRepository<TEntity> repository) : base(mapper, repository)
        {
        }
    }
}
