using Common;
using Common.Exceptions;
using Common.Utilities;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository, IScopedDependency
    {
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Task<User> GetByUserAndPass(string username, string password, CancellationToken cancellationToken)
        {
            var passwordHash = SecurityHelper.GetSha256Hash(password);
            return Table.SingleOrDefaultAsync(p => p.UserName == username && p.PasswordHash == passwordHash, cancellationToken);
        }

        public Task UpdateSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            //user.SecurityStamp = Guid.NewGuid();
            return UpdateAsync(user, cancellationToken);
        }
        public Task UpdateLastLoginDateAsync(User user, CancellationToken cancellationToken)
        {
            user.LastLoginDate = DateTimeOffset.Now;
            return UpdateAsync(user, cancellationToken);
        }

        //we can factor 'async' & 'await' 

        //public override void Update(User entity, bool saveNow = true)
        //{
        //    entity.SecurityStamp = Guid.NewGuid();
        //    base.Update(entity, saveNow);
        //}

        //public override Task UpdateAsync(User entity, CancellationToken cancellationToken, bool saveNow = true)
        //{
        //    entity.SecurityStamp = Guid.NewGuid();
        //    return base.UpdateAsync(entity, cancellationToken, saveNow);
        //}

        //public override void UpdateRange(IEnumerable<User> entities, bool saveNow = true)
        //{
        //    foreach (var user in entities)            
        //        user.SecurityStamp = Guid.NewGuid();

        //    base.UpdateRange(entities, saveNow);
        //}
        public async Task AddAsync(User user, string password, CancellationToken cancellationToken)
        {
            //todo در لایه سرویس باید انجام شود
            var exists = await TableNoTracking.AnyAsync(p => p.UserName == user.UserName);
            if (exists)
                throw new BadRequestException("نام کاربری تکراری است");

            var passwordHash = SecurityHelper.GetSha256Hash(password);
            user.PasswordHash = passwordHash;
            await base.AddAsync(user, cancellationToken);
        }
    }
}
