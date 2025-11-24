using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Raqeb.BL;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using System.Globalization;
using System.Security.Claims;
namespace Raqeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IUnitOfWork uow;
        public CustomerController(IUnitOfWork unitOfWork)
        {
            this.uow = unitOfWork;
        }


        [HttpGet("getAllCustomerForDropdown")]
        public List<NameIdForDropDown> GetAllCustomerForDropdown()
        {
            try
            {
                var CustomerId = uow.SessionServices.CustomerID;

                var res = uow.DbContext.Customers.Where(x=>x.ID == CustomerId).Select(x => new NameIdForDropDown
                {
                    ID = x.ID,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn
                }).ToList();
                return res;
            }
            catch
            {
                return null;
            }
        }


        [HttpGet]
        [Route("getWithPaginate")]
        [ProducesResponseType(typeof(ViewerPagination<CustomerDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult getCustomerSites(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var myList = uow.Customer.getWithPaginate(page, pageSize, searchTerm);
                return Ok(myList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById")]
        public virtual async Task<CustomerDTO> GetById(int id)
        {
            var res = await uow.DbContext.Customers.FirstOrDefaultAsync(x => x.ID == id );
            CustomerDTO ReturnedRes = uow.Mapper.Map<CustomerDTO>(res);
            //if (res.Logo != null)
            //    ReturnedRes.LogoBase64 = res.Logo != null ? "data:image/png;base64," + Convert.ToBase64String(res.Logo) : null;
            return ReturnedRes;
        }


        [HttpPost("SaveData")]
        public virtual async Task<bool> SaveData([FromForm] CustomerDTO entity)
        {
            try
            {
                var ExistItem = await uow.Customer.DbSet.FirstOrDefaultAsync(x => x.ID == entity.ID);
                if (ExistItem == null)
                {
                    //entity.Logo = uow.ConvertIFormFileToByteArray(entity.LogoForm).Result;
                    Customer model = uow.Mapper.Map<Customer>(entity);
                    await uow.Customer.DbSet.AddAsync(model);
                    await uow.SaveChangesAsync();
                    return true;
                }
                else
                {
                    ExistItem.NameAr = entity.NameAr;
                    ExistItem.NameEn = entity.NameEn;
                    ExistItem.Email = entity.Email;
                    ExistItem.Phone = entity.Phone;
                    //ExistItem.Logo = uow.ConvertIFormFileToByteArray(entity.LogoForm).Result;
                    await uow.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost("Delete")]
        public virtual async Task<bool> Delete(int id)
        {
            try
            {
                var res = await uow.DbContext.Customers.FindAsync(id);
                if (res != null)
                {
                    uow.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}