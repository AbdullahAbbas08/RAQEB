using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;

namespace Raqeb.BL.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        public ViewerPagination<CustomerDTO> getWithPaginate(int page, int pageSize, string searchTerm);
    }
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly IUnitOfWork uow;

        public CustomerRepository(IUnitOfWork _uow) : base(_uow)
        {
            uow = _uow;
        }

        public ViewerPagination<CustomerDTO> getWithPaginate(int page, int pageSize, string searchTerm)
        {
            try
            {
                var UserId = uow.SessionServices.UserId;
                var CustomerId = uow.SessionServices.CustomerID;

                if (!string.IsNullOrEmpty(searchTerm)) searchTerm = searchTerm.ToLower();

                IQueryable<Customer> myData;
                myData = uow.DbContext
                                .Customers
                                .AsNoTracking()
                                .Where(x=>x.ID == CustomerId)
                                .Where(c => searchTerm == null || c.NameAr.ToLower().Contains(searchTerm) ||
                                             c.NameEn.ToLower().Contains(searchTerm) ||
                                             c.Phone.ToLower().Contains(searchTerm) ||
                                             c.Email.ToLower().Contains(searchTerm));

                int myDataCount = 0;
                myDataCount = myData.Count();
                ViewerPagination<CustomerDTO> viewerPagination = new ViewerPagination<CustomerDTO>();

                List<CustomerDTO> ReturnData = myData.OrderBy(a => a.ID).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new CustomerDTO
                {
                    ID = x.ID,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Email = x.Email,
                    Phone = x.Phone,
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
