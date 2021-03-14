using Common;
using Common.Exceptions;
using Common.Utilities;
using Core.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>, IScopedDependency
        where TEntity : class, IEntity
    {
        protected readonly ApplicationDbContext DbContext;
        public DbSet<TEntity> Entities { get; }
        public virtual IQueryable<TEntity> Table => Entities;
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

        public Repository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            Entities = DbContext.Set<TEntity>(); // City => Cities
        }

        #region Async Method
        public virtual Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            //return Entities.FindAsync(ids, cancellationToken);
            return Entities.FindAsync(ids, cancellationToken).AsTask();
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            await Entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            await Entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken);
        }

        private List<IProperty> GetPrimaryKey(TEntity entity)
        {
            return DbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties.ToList();
        }

        public virtual async Task UpdateCustomPropertiesAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true, params string[] properties)
        {
            Assert.NotNull(entity, nameof(entity));
            if (properties == null || properties.Length == 0)
                throw new LogicException("'properties' can't be NULL or Empty");

            if (DbContext.Entry(entity).State != EntityState.Detached)
                DbContext.Entry(entity).State = EntityState.Detached;

            var primaryKey = GetPrimaryKey(entity).First();
            TEntity localEntity = null;// default(TEntity);
            if (primaryKey.PropertyInfo.PropertyType == typeof(Guid))
            {
                localEntity = DbContext.Set<TEntity>().Local.FirstOrDefault(p => new Guid(p.GetType().GetProperty(primaryKey.Name).GetValue(p).ToString()) ==
                    new Guid(entity.GetType().GetProperty(primaryKey.Name).GetValue(entity).ToString()));
            }
            else
            {
                localEntity = DbContext.Set<TEntity>().Local
                    .FirstOrDefault(p => Convert.ChangeType(p.GetType().GetProperty(primaryKey.Name).GetValue(p), primaryKey.PropertyInfo.PropertyType, CultureInfo.InvariantCulture) ==
                     Convert.ChangeType(entity.GetType().GetProperty(primaryKey.Name).GetValue(entity), primaryKey.PropertyInfo.PropertyType, CultureInfo.InvariantCulture));
            }
            if (localEntity != null)
                DbContext.Entry(localEntity).State = EntityState.Detached;

            Entities.Attach(entity);

            var entityProperties = entity.GetType().GetProperties();
            foreach (var propertyName in properties)
            {
                if (!entityProperties.Any(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)))
                    throw new MissingFieldException(entity.GetType().Name, propertyName);
                else
                {
                    if (GetPrimaryKey(entity).Any(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))) continue;

                    DbContext.Entry(entity)
                        .Property(entityProperties
                        .Single(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).Name)
                        .IsModified = true;
                }
            }

            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken);
        }

        //my custom method
        public virtual async Task DeleteByIdAsync(object id, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(id, nameof(id));
            var entity = await GetByIdAsync(cancellationToken, id);

            if (entity == null)
                throw new NotFoundException($"not found {typeof(TEntity).Name} entity to PKey(Id) : '{id}'");

            await DeleteAsync(entity, cancellationToken, saveNow);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Sync Methods
        public virtual TEntity GetById(params object[] ids)
        {
            return Entities.Find(ids);
        }

        public virtual void Add(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual void AddRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.AddRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            DbContext.SaveChanges();
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual void Delete(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }

        //my custom method
        public virtual void DeleteById(object id, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(id, nameof(id));
            var entity = GetById(id);
            Delete(entity, saveNow);
        }

        public virtual int SaveChanges()
        {
            return DbContext.SaveChanges();
        }

        #endregion

        #region Attach & Detach
        public virtual void Detach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = DbContext.Entry(entity);
            if (entry != null)
                entry.State = EntityState.Detached;
        }

        public virtual void Attach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            if (DbContext.Entry(entity).State == EntityState.Detached)
                Entities.Attach(entity);
        }
        #endregion

        #region Explicit Loading
        public virtual async Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Assert.NotNull(entity, nameof(entity));
            Attach(entity);

            var collection = DbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            Assert.NotNull(entity, nameof(entity));
            Attach(entity);
            var collection = DbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                collection.Load();
        }

        public virtual async Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Assert.NotNull(entity, nameof(entity));
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty)
            where TProperty : class
        {
            Assert.NotNull(entity, nameof(entity));
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                reference.Load();
        }
        #endregion

        #region Select Methods

        /// <summary>
        /// این متد یک موجودیت را با استفاده از شناسه آن پیدا کرده و اجازه نمی دهد که کانتکست انتیتی فریمورک آن را تراک کند-NoTracking
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual TEntity GetByCondition(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true)
        {
            var entities = asNoTracking ? TableNoTracking : Table;

            TEntity entity = entities.FirstOrDefault(predicate);
            return entity;
        }

        /// <summary>
        /// این متد یک موجودیت را با استفاده از شناسه آن پیدا کرده و اجازه نمی دهد که کانتکست انتیتی فریمورک آن را تراک کند-NoTracking
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async virtual Task<TEntity> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool asNoTracking = true)
        {
            var entities = asNoTracking ? TableNoTracking : Table;

            TEntity entity = await entities.FirstOrDefaultAsync(predicate, cancellationToken);
            return entity;
        }

        public async Task<ColumnType> GetColumnValueAsync<ColumnType>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, string columnName = "Id")
        {
            Type entityType = typeof(TEntity);
            if (entityType.GetProperty(columnName) != null)
            {
                TEntity entity = await TableNoTracking.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
                if (entity != null && entityType.GetProperties().Any(p => p.Name == columnName))
                {
                    var proprtyInfo = entityType.GetProperty(columnName);
                    return (ColumnType)proprtyInfo.GetValue(entity, null);
                }
            }
            return default;
        }

        public ColumnType GetColumnValue<ColumnType>(Expression<Func<TEntity, bool>> predicate, string columnName = "Id")
        {
            Type entityType = typeof(TEntity);
            if (entityType.GetProperty(columnName) != null)
            {
                TEntity entity = TableNoTracking.Where(predicate).FirstOrDefault();
                if (entity != null && entityType.GetProperties().Any(p => p.Name == columnName))
                {
                    var proprtyInfo = entityType.GetProperty(columnName);
                    return (ColumnType)proprtyInfo.GetValue(entity, null);
                }
            }
            return default;
        }

        public IQueryable<TResult> CreatePage<TResult>(IQueryable<TResult> query, int pageNumber, int pageSize)
        {
            Assert.NotNull(query, nameof(query));
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
        public async Task<List<TEntity>> SelectByAsync(Expression<Func<TEntity, bool>> predicate,
                                                       CancellationToken cancellationToken,
                                                       bool asNoTracking = true,
                                                       int? pageNumber = null,
                                                       int? pageSize = null,
                                                       params Expression<Func<TEntity, object>>[] navigationPropertyPaths)
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            foreach (var navigationProperty in navigationPropertyPaths)
                query = query.Include(navigationProperty);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;

                return await CreatePage(query.Where(predicate), pageNumber.Value, pageSize.Value).ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            else
                return await query.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public IQueryable<TEntity> SelectBy(Expression<Func<TEntity, bool>> predicate,
                                            bool asNoTracking = true,
                                            int? pageNumber = null,
                                            int? pageSize = null,
                                            params Expression<Func<TEntity, object>>[] navigationPropertyPaths)
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            foreach (var navigationProperty in navigationPropertyPaths)
                query = query.Include(navigationProperty);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;

                return CreatePage(query.Where(predicate), pageNumber.Value, pageSize.Value);
            }
            else
                return query.Where(predicate);
        }

        public async Task<List<TResult>> SelectByAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                                CancellationToken cancellationToken,
                                                                bool asNoTracking = true,
                                                                int? pageNumber = null,
                                                                int? pageSize = null)
            where TResult : class
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;

                return await CreatePage(query.Select(selector), pageNumber.Value, pageSize.Value).ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            else
                return await query.Select(selector).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public IQueryable<TResult> SelectBy<TResult>(Expression<Func<TEntity, TResult>> selector,
                                                                bool asNoTracking = true,
                                                                int? pageNumber = null,
                                                                int? pageSize = null)
            where TResult : class
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;

                return CreatePage(query.Select(selector), pageNumber.Value, pageSize.Value);
            }
            else
                return query.Select(selector);
        }

        public async Task<List<TResult>> SelectByAsync<TResult>(Expression<Func<TEntity, bool>> predicate,
                                                                Expression<Func<TEntity, TResult>> selector,
                                                                CancellationToken cancellationToken,
                                                                bool asNoTracking = true,
                                                                int? pageNumber = null,
                                                                int? pageSize = null)
            where TResult : class
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;
                return await CreatePage(query.Where(predicate).Select(selector), pageNumber.Value, pageSize.Value)
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
            }
            else
                return await query.Where(predicate).Select(selector).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public IQueryable<TResult> SelectBy<TResult>(Expression<Func<TEntity, bool>> predicate,
                                                     Expression<Func<TEntity, TResult>> selector,
                                                     CancellationToken cancellationToken,
                                                     bool asNoTracking = true,
                                                     int? pageNumber = null,
                                                     int? pageSize = null) where TResult : class
        {
            IQueryable<TEntity> query = asNoTracking ? TableNoTracking : Table;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;
                return CreatePage(query.Where(predicate).Select(selector), pageNumber.Value, pageSize.Value);
            }
            else
                return query.Where(predicate).Select(selector);
        }

        #endregion       
    }
}
