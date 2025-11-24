using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;

namespace Raqeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : Controller
    {
        private readonly IUnitOfWork uow;
        public LanguageController(IUnitOfWork _uow)
        {
            uow = _uow;
        }

        [HttpGet("getAll")]
        public List<NameIdForDropDown> GetAll()
        {
            try
            {
                var res = uow.DbContext.Languages.Select(x => new NameIdForDropDown
                {
                    ID = x.ID,
                    NameAr = x.Name
                }).ToList();
                return res;
            }
            catch
            {
                return null;
            }
        }
        
        [HttpGet("getAllLocalizationLangValue")]
        public List<LanguageLocalizationListDto> LocalizationLangValue()
        {
            try
            {
                var res = uow.DbContext.Languages.Where(x=> !x.IsDeleted).Select(x => new LanguageLocalizationListDto
                {
                   LanguageId = x.ID,
                   LanguageName = x.Name, 
                   LocalizationId = 0,
                   Value = ""
                }).ToList();
                return res;
            }
            catch
            {
                return null;
            }
        }


        [HttpGet]
        [Route("getLanguageWithPaginate")]
        [ProducesResponseType(typeof(ViewerPagination<LanguageCrudDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult getLanguageWithPaginate(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var myList = uow.Localization.getLanguageWithPaginate(page, pageSize, searchTerm);
                return Ok(myList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById")]
        public virtual async Task<LanguageCrudDto> GetById(int id)
        {
            var res = await uow.DbContext.Languages.FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted);
            LanguageCrudDto ReturnedRes = uow.Mapper.Map<LanguageCrudDto>(res);
            return ReturnedRes;
        }


        [HttpPost("SaveData")]
        public virtual async Task<bool> SaveData([FromForm] LanguageCrudDto entity)
        {
            try
            {
                if (uow.DbContext.Languages.AsNoTracking().Any(x => x.Code.ToLower() == entity.Code.ToLower()) && entity.ID ==0)
                    return false;

                var ExistItem = await uow.DbContext.Languages.FirstOrDefaultAsync(x => x.ID == entity.ID);
                if (ExistItem == null)
                {
                    //entity.Icon =  $"data:image/png;base64,{Convert.ToBase64String(uow.ConvertIFormFileToByteArray(entity.LogoForm).Result)}";
                    Language model = uow.Mapper.Map<Language>(entity);
                    model.Icon = entity.Icon;
                    await uow.DbContext.Languages.AddAsync(model);
                    await uow.SaveChangesAsync();
                    return true;
                }
                else
                {
                    ExistItem.Name = entity.Name;
                    ExistItem.Code = entity.Code;
                    ExistItem.Direction = entity.Direction;
                    ExistItem.Icon = entity.Icon;

                    //if (entity.LogoForm != null)
                    //    ExistItem.Icon = $"data:image/png;base64,{Convert.ToBase64String(uow.ConvertIFormFileToByteArray(entity.LogoForm).Result)}";
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
                var res = await uow.DbContext.Languages.FindAsync(id);
                if (res != null)
                {
                    res.IsDeleted = true;
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
