using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Raqeb.BL;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;

namespace Raqeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationController : Controller
    {
        private readonly IUnitOfWork uow;
        public LocalizationController(IUnitOfWork _uow)
        {
            uow = _uow;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("json/{culture}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Json(string culture)
        {
            try
            {
                var myList = await uow.Localization.getLocalizationLanguage(culture);
                var res = uow.Localization.GetJson(myList);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("getAll")]
        public List<NameIdForDropDown> GetAll()
        {
            try
            {
                var res = uow.DbContext.Localizations.Select(x => new NameIdForDropDown
                {
                    ID = x.ID,
                    NameAr = x.Code
                }).ToList();
                return res;
            }
            catch
            {
                return null;
            }
        }


        [HttpGet]
        [Route("getLocalizationWithPaginate")]
        [ProducesResponseType(typeof(ViewerPagination<LocalizationCrudDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult getLocalizationWithPaginate(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var myList = uow.Localization.getLocalizationWithPaginate(page, pageSize, searchTerm);
                return Ok(myList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet]
        [Route("getLocalizationLanguageWithPaginate")]
        [ProducesResponseType(typeof(ViewerPagination<LanguageLocalizationListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult getLocalizationLanguageWithPaginate(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var myList = uow.Localization.getLocalizationLanguageWithPaginate(page, pageSize, searchTerm);
                return Ok(myList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById")]
        public virtual async Task<LocalizationCrudDto> GetById(int id)
        {
            var res = await uow.DbContext.Localizations.FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted);
            LocalizationCrudDto ReturnedRes = uow.Mapper.Map<LocalizationCrudDto>(res);
            return ReturnedRes;
        }
        
        [HttpGet("GetLocalLangById")]
        public virtual async Task<LanguageLocalizationListDto> GetLocalLangById(int id)
        {
            var res = await uow.DbContext.LanguageLocalizations.FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted);
            LanguageLocalizationListDto ReturnedRes = uow.Mapper.Map<LanguageLocalizationListDto>(res);
            return ReturnedRes;
        }
        
        [HttpGet("GetLocalizationData")]
        public List<LanguageLocalizationListDto> GetLocalizationData(int id)
        {
            List<LanguageLocalizationListDto> languageLocalizationListDtos = new List<LanguageLocalizationListDto>();
            var res = uow.DbContext.Localizations.AsNoTracking().Include(x=>x.LanguageLocalization).ThenInclude(x=>x.Language).FirstOrDefault(x => x.ID == id && !x.IsDeleted);
            foreach (var item in res?.LanguageLocalization)
            {
                languageLocalizationListDtos.Add(new LanguageLocalizationListDto
                {
                    ID = item.ID,
                    LanguageId = item.LanguageId,
                    LanguageName = item.Language !=null? item.Language.Name:null,
                    LocalizationCode = item.Language!=null?item.Language.Code:null,
                    LocalizationId = item.LocalizationId,
                    Value = item.Value
                });
            }

            var langs = uow.DbContext.Languages.Where(x=>!x.IsDeleted).AsNoTracking().ToList();
            foreach (var item in langs)
            {
                if (!languageLocalizationListDtos.Select(x => x.LanguageId).Any(x => x == item.ID))
                {
                    languageLocalizationListDtos.Add(new LanguageLocalizationListDto
                    {
                        LanguageId = item.ID,
                        LanguageName = item.Name
                    });
                }
            }
            return languageLocalizationListDtos;
        }


        [HttpPost("SaveData")]
        public virtual async Task<bool> SaveData(LocalizationCrudDto entity)
        {
            try
            {
                if (uow.DbContext.Localizations.AsNoTracking().Any(x => x.Code.ToLower() == entity.Code.ToLower()))
                    return false;

                var ExistItem = await uow.DbContext.Localizations.FirstOrDefaultAsync(x => x.ID == entity.ID);
                if (ExistItem == null)
                {
        
                    Localization model = uow.Mapper.Map<Localization>(entity);
                    await uow.DbContext.Localizations.AddAsync(model);
                    await uow.SaveChangesAsync();
                    return true;
                }
                else
                {
                    ExistItem.Code = entity.Code;
                    await uow.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        
        [HttpPost("SaveDataLanguageLocalization")]
        public virtual async Task<bool> SaveDataLanguageLocalization(LanguageLocalizationListDto entity)
        {
            try
            {
                var ExistItem = await uow.DbContext.LanguageLocalizations.FirstOrDefaultAsync(x => x.ID == entity.ID);
                if (ExistItem == null)
                {

                    LanguageLocalization model = new LanguageLocalization
                    {
                        LocalizationId = entity.LocalizationId,
                        LanguageId = entity.LanguageId,
                        Value = entity.Value
                    };
                    await uow.DbContext.LanguageLocalizations.AddAsync(model);
                    await uow.SaveChangesAsync();
                    return true;
                }
                else
                {
                    ExistItem.Value = entity.Value;
                    ExistItem.LanguageId = entity.LanguageId;
                    ExistItem.LocalizationId = entity.LocalizationId;
                    await uow.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
