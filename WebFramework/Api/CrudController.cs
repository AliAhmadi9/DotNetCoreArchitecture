using System;
using AutoMapper;
using Common.Exceptions;
using Core.Entities.Base;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebFramework.Api
{
    /// <summary>
    /// working base ServiceRepository
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TSelectDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [ApiVersion("1")]
    public class CrudController<TDto, TSelectDto, TEntity, TKey> : BaseApiController
        where TDto : BaseDto<TDto, TEntity, TKey>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()//new() : type argument in a generic class declaration must have a public parameterless constructor.
        where TEntity : BaseEntity<TKey>, new()
    {
        private readonly IMapper mapper;
        private readonly IServiceRepository<TDto, TSelectDto, TEntity, TKey> serviceRepository;

        public CrudController(IMapper mapper, IServiceRepository<TDto, TSelectDto, TEntity, TKey> serviceRepository)
        {
            this.mapper = mapper;
            this.serviceRepository = serviceRepository;
        }

        [HttpGet]
        public virtual async Task<ApiResult<List<TSelectDto>>> Get(CancellationToken cancellationToken)
        {
            //var list = await serviceRepository.Get();
            return Ok(await serviceRepository.Get());
        }

        [HttpGet("[action]/{pageNumber:int}/{pageSize:int}")]
        public virtual async Task<ApiResult<PagingDto<TSelectDto>>> Select(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            return await serviceRepository.Select(pageNumber, pageSize, cancellationToken);
        }

        [HttpGet("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Get(TKey id, CancellationToken cancellationToken)
        {
            var dto = await serviceRepository.Get(id, cancellationToken);

            if (dto == null)
                return NotFound();

            return dto;
        }

        [HttpPost]
        public virtual async Task<ApiResult<TSelectDto>> Create(TDto dto, CancellationToken cancellationToken)
        {
            dto.CreatedDate = DateTime.Now;
            return await serviceRepository.Create(dto, cancellationToken);
        }

        [HttpPut("{id}")]
        public virtual async Task<ApiResult<TSelectDto>> Update(TKey id, TDto dto, CancellationToken cancellationToken)
        {
            return await serviceRepository.Update(id, dto, cancellationToken);
        }

        [HttpPut]
        public virtual async Task<ApiResult<TSelectDto>> Update(TDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                throw new NotFoundException("'dto' can't be NULL");

            return await serviceRepository.Update(dto, cancellationToken);
        }

        [HttpPut("[action]/{id}")]
        public virtual async Task<ApiResult<TSelectDto>> UpdateProperty(TKey id, PropertyValueDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                throw new NotFoundException("'dto' can't be NULL");

            var resultDto = await serviceRepository.UpdateProperty(id, dto, cancellationToken);

            if (resultDto == null)
                return NotFound();

            return resultDto;
        }

        [HttpPut("[action]")]
        public virtual async Task<ApiResult<TSelectDto>> UpdateCustomProperties(TDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                throw new NotFoundException("'dto' can not NULL");

            if (dto.UpdateProperties == null || !dto.UpdateProperties.Any())
                throw new NotFoundException("'dto.UpdateProperties' can't be NULL or Empty");

            var resultDto = await serviceRepository.UpdateCustomProperties(dto, cancellationToken);

            return resultDto;
        }

        [HttpDelete("{id}")]
        public virtual async Task<ApiResult> Delete(TKey id, CancellationToken cancellationToken)
        {
            await serviceRepository.Delete(id, cancellationToken);

            return Ok();
        }
    }

    /// <summary>
    /// working base ServiceRepository
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TSelectDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class CrudController<TDto, TSelectDto, TEntity> : CrudController<TDto, TSelectDto, TEntity, int>
        where TDto : BaseDto<TDto, TEntity, int>, new()
        where TSelectDto : BaseDto<TSelectDto, TEntity, int>, new()
        where TEntity : BaseEntity<int>, new()
    {
        public CrudController(IMapper mapper, IServiceRepository<TDto, TSelectDto, TEntity, int> serviceRepository)
            : base(mapper, serviceRepository)
        {

        }
    }

    /// <summary>
    /// working base ServiceRepository
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class CrudController<TDto, TEntity> : CrudController<TDto, TDto, TEntity, int>
    where TDto : BaseDto<TDto, TEntity, int>, new()
    where TEntity : BaseEntity<int>, new()
    {
        public CrudController(IMapper mapper, IServiceRepository<TDto, TDto, TEntity, int> serviceRepository)
            : base(mapper, serviceRepository)
        {

        }
    }
}
