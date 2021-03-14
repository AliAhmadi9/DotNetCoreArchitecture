using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Core.Data.Repositories;
using Core.Entities.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebFramework.Api
{
    /// <summary>
    /// working to repository
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TSelectDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [ApiVersion("1")]
    public class OldCrudController<TDto, TSelectDto, TEntity, TKey> : BaseApiController
        where TDto : BaseDto<TDto, TEntity, TKey>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()//new() : type argument in a generic class declaration must have a public parameterless constructor.
        where TEntity : BaseEntity<TKey>, new()
    {
        private readonly IMapper mapper;
        private readonly IRepository<TEntity> repository;
        private readonly IServiceRepository<TDto, TSelectDto, TEntity, TKey> serviceRepository;

        public OldCrudController(IMapper mapper, IRepository<TEntity> repository, IServiceRepository<TDto, TSelectDto, TEntity, TKey> serviceRepository)
        {
            this.mapper = mapper;
            this.repository = repository;
            this.serviceRepository = serviceRepository;
        }

        [HttpGet]
        public virtual async Task<ActionResult<List<TSelectDto>>> Get(CancellationToken cancellationToken)
        {
            //var list = await serviceRepository.Get();
            //return Ok(list);
            var list = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Ok(list);
        }

        [HttpGet("[action]/{pageNumber:int}/{pageSize:int}")]
        public virtual async Task<ApiResult<PagingDto<TSelectDto>>> Select(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
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

        [HttpGet("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Get(TKey id, CancellationToken cancellationToken)
        {
            var dto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(id), cancellationToken);

            if (dto == null)
                return NotFound();

            return dto;
        }

        [HttpPost]
        public virtual async Task<ApiResult<TSelectDto>> Create(TDto dto, CancellationToken cancellationToken)
        {
            var model = dto.ToEntity(mapper);

            await repository.AddAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        [HttpPut("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Update(TKey id, TDto dto, CancellationToken cancellationToken)
        {
            dto.Id = id;
            var model = await repository.GetByIdAsync(cancellationToken, id);

            model = dto.ToEntity(mapper, model);

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        [HttpPut]
        public virtual async Task<ApiResult<TSelectDto>> Update(TDto dto, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(cancellationToken, dto.Id);

            model = dto.ToEntity(mapper, model);

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        public class PropertyValueDto
        {
            public string PropertyName { get; set; }
            public object PropertyValue { get; set; }
        }

        [HttpPut("[action]/{id}")]
        public virtual async Task<ApiResult<TSelectDto>> UpdateProperty(TKey id, PropertyValueDto dto, CancellationToken cancellationToken)
        {
            var entityType = typeof(TEntity);
            var property = entityType.GetProperty(dto.PropertyName);
            if (property == null)
                throw new NotFoundException($"property {dto.PropertyName} not gound in entity : {typeof(TEntity).Name}");

            var model = await repository.GetByIdAsync(cancellationToken, id);

            if (model == null)
                return NotFound();
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
                entityType.GetProperty(dto.PropertyName).SetValue(model, null);
            }

            await repository.UpdateAsync(model, cancellationToken);

            var resultDto = await repository.TableNoTracking.ProjectTo<TSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }

        [HttpDelete("{id}")]
        public virtual async Task<ApiResult> Delete(TKey id, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(cancellationToken, id);

            await repository.DeleteAsync(model, cancellationToken);

            return Ok();
        }
    }

    public class OldCrudController<TDto, TSelectDto, TEntity> : OldCrudController<TDto, TSelectDto, TEntity, int>
        where TDto : BaseDto<TDto, TEntity, int>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, int>, new()
        where TEntity : BaseEntity<int>, new()
    {
        public OldCrudController(IMapper mapper, IRepository<TEntity> repository, IServiceRepository<TDto, TSelectDto, TEntity, int> serviceRepository)
            : base(mapper, repository, serviceRepository)
        {

        }
    }

    public class OldCrudController<TDto, TEntity> : OldCrudController<TDto, TDto, TEntity, int>
    where TDto : BaseDto<TDto, TEntity, int>, new()
    where TEntity : BaseEntity<int>, new()
    {
        public OldCrudController(IMapper mapper, IRepository<TEntity> repository, IServiceRepository<TDto, TDto, TEntity, int> serviceRepository)
            : base(mapper, repository, serviceRepository)
        {

        }
    }
}
