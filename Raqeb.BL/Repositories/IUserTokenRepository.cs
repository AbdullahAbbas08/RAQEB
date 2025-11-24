using Raqeb.Shared.Models;

namespace Raqeb.BL.Repositories
{
    public interface IUserTokenRepository : IRepository<UserToken>
    {
    }

    public class UserTokenRepository : Repository<UserToken>, IUserTokenRepository
    {
        public UserTokenRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}