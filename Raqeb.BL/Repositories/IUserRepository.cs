using Raqeb.Shared.DTOs;
using Raqeb.Shared.Helpers;
using Raqeb.Shared.Models;

namespace Raqeb.BL.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<AuthModel> Login(UserLoginModel model);
        Task<User> FindUserPasswordAsync(string username, string password, bool isHashedPassword);
        public ViewerPagination<UserDTO> getWithPaginate(int page, int pageSize, string searchTerm);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IUnitOfWork uow;

        public UserRepository(IUnitOfWork _uow) : base(_uow)
        {
            uow = _uow;
        }

        public async Task<User> FindUserPasswordAsync(string username, string password, bool isHashedPassword)
        {
            string passwordHash = EncryptHelper.Encrypt(password);
            User result = await uow.User.DbSet.Where(x => (x.Email == username) && x.Password == passwordHash && !x.IsDeleted).Select(x=> new User
            {
                CustomerId = x.CustomerId,
                ID = x.ID,  
                Email = x.Email,
                Mobile = x.Mobile,
                NameEn = x.NameEn,
                NameAr = x.NameAr,                
            }).FirstOrDefaultAsync();
            return result;
        }
        public async Task<AuthModel> Login(UserLoginModel model)
        {
            var authModel = new AuthModel();
            try
            {
                User user = await FindUserPasswordAsync(model.Email, model.Password, false);
                if (user is null)
                {
                    authModel.Message = "InvalidUsernameOrPassword";
                    authModel.IsAuthenticated = false;
                    authModel.Status = 400;
                    return authModel;
                }

                var jwtSecurityToken = uow.TokenStoreRepository.CreateJwtTokens(user, 1, null);
                authModel.Message = null;
                authModel.IsAuthenticated = true;
                authModel.Status = 200;
                authModel.Token = jwtSecurityToken.Result.accessToken;
                authModel.refreshToken = jwtSecurityToken.Result.refreshToken;
                authModel.Count = jwtSecurityToken.Result.Count;
            }
            catch (Exception ex)
            {
                authModel.Message = ex.Message;
                authModel.IsAuthenticated = false;
                authModel.Status = 500;
            }
            return authModel;
        }

        public ViewerPagination<UserDTO> getWithPaginate(int page, int pageSize, string searchTerm)
        {
            try
            {
                var UserId = uow.SessionServices.UserId;
                var CustomerId = uow.SessionServices.CustomerID;

                if (!string.IsNullOrEmpty(searchTerm)) searchTerm = searchTerm.ToLower();

                IQueryable<User> myData;
                myData = uow.DbContext
                                .Users
                                .Include(x=>x.Customer)
                                .AsNoTracking()
                                .Where(x=>x.CustomerId == CustomerId)
                                .Where(x => !x.IsDeleted)
                                     .Where(c => searchTerm == null || c.NameAr.ToLower().Contains(searchTerm) ||
                                             c.NameEn.ToLower().Contains(searchTerm) ||
                                             c.Mobile.ToLower().Contains(searchTerm) ||
                                             c.Email.ToLower().Contains(searchTerm));

                int myDataCount = 0;
                myDataCount = myData.Count();
                ViewerPagination<UserDTO> viewerPagination = new ViewerPagination<UserDTO>();

                List<UserDTO> ReturnData = myData.OrderBy(a => a.ID).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new UserDTO
                {
                    ID = x.ID,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Email = x.Email,
                    Mobile = x.Mobile,
                    Image = x.Image,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.NameAr

                }).ToList();

                viewerPagination.PaginationList = ReturnData;

                viewerPagination.OriginalListListCount = myDataCount;
                return viewerPagination;
            }
            catch (Exception ex) { }
            return null;
        }

    }
}
