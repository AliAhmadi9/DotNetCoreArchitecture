using Core.Entities.Base;
using Services.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public interface IServiceRepository<TDto, TSelectDto, TEntity, TKey>
         where TDto : BaseDto<TDto, TEntity, TKey>, new()
         where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()
         where TEntity : BaseEntity<TKey>, new()
    {
        Task<List<TSelectDto>> Get(CancellationToken cancellationToken = default);

        Task<PagingDto<TSelectDto>> Select(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<TSelectDto> Get(TKey id, CancellationToken cancellationToken = default);

        Task<TSelectDto> Create(TDto dto, CancellationToken cancellationToken = default);

        Task<TSelectDto> Update(TKey id, TDto dto, CancellationToken cancellationToken = default);

        Task<TSelectDto> Update(TDto dto, CancellationToken cancellationToken = default);

        Task<TSelectDto> UpdateCustomProperties(TDto dto, CancellationToken cancellationToken = default);

        Task<TSelectDto> UpdateProperty(TKey id, PropertyValueDto dto, CancellationToken cancellationToken = default);

        Task Delete(TKey id, CancellationToken cancellationToken = default);
    }

    public interface IServiceRepository<TDto, TEntity> : IServiceRepository<TDto, TDto, TEntity, int>
        where TDto : BaseDto<TDto, TEntity, int>, new()
         where TEntity : BaseEntity<int>, new()
    {

    }

    public interface IServiceRepository<TDto, TSelectDto, TEntity> : IServiceRepository<TDto, TSelectDto, TEntity, int>
          where TDto : BaseDto<TDto, TEntity, int>, new()
         where TSelectDto : BaseDto<TSelectDto, TEntity, int>, new()
         where TEntity : BaseEntity<int>, new()
    {

    }
}
