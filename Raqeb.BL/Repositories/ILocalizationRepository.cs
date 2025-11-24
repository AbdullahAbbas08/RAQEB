using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using System.Drawing;
using System.Text.Json;

namespace Raqeb.BL.Repositories
{
    public interface ILocalizationRepository : IRepository<Localization>
    {
        Task<List<LanguageLocalizationDto>> getLocalizationLanguage(string lang);
        string GetJson(List<LanguageLocalizationDto> myList);
        Task<int> AddLanguageAsync(LanguageDtoForHeader lang);
        //Task<int> AddLocalizationAsync(LanguageDtoForHeader lang);
        public ViewerPagination<LanguageCrudDto> getLanguageWithPaginate(int page, int pageSize, string searchTerm);
        public ViewerPagination<LocalizationCrudDto> getLocalizationWithPaginate(int page, int pageSize, string searchTerm);
        public ViewerPagination<LanguageLocalizationListDto> getLocalizationLanguageWithPaginate(int page, int pageSize, string searchTerm);

    }

    public class LocalizationRepository : Repository<Localization>, ILocalizationRepository
    {
        private readonly IUnitOfWork uow;

        public LocalizationRepository(IUnitOfWork _uow) : base(_uow)
        {
            uow = _uow;
        }

        public async Task<List<LanguageLocalizationDto>> getLocalizationLanguage(string lang)
        {
            var Language = await uow.DbContext.Languages.Include(x => x.LanguageLocalization).ThenInclude(x => x.Localization).FirstOrDefaultAsync(x => x.Code.ToLower() == lang);
            if (Language == null) return new List<LanguageLocalizationDto>();
            var LanguageLocalization = uow.Mapper.Map<List<LanguageLocalizationDto>>(Language.LanguageLocalization);
            return LanguageLocalization;
        }

        public string GetJson(List<LanguageLocalizationDto> myList)
        {
            var dictionary = myList.ToDictionary(
                x => x.Localization.Code,
                x => x.Value
            );

            var jsonString = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null // or JsonNamingPolicy.Default
            });

            return jsonString;
        }

        public async Task<int> AddLanguageAsync(LanguageDtoForHeader model)
        {
            try
            {
                var lang = uow.DbContext.Languages.FirstOrDefault(x => x.ID == model.ID);
                if (lang == null)
                {
                    await uow.DbContext.Languages.AddAsync(new Language
                    {
                        Name = model.Name,
                        Direction = model.Direction,
                        Code = model.Code,
                        Icon = model.Icon
                    });

                }
                else
                {
                    lang.Name = model.Name;
                    lang.Direction = model.Direction;
                    lang.Code = model.Code;
                    lang.Icon = model.Icon;
                }
                await uow.SaveChangesAsync();
                return lang.ID;

            }
            catch (Exception ex)
            {

            }
            return 0;

        }

        public ViewerPagination<LanguageCrudDto> getLanguageWithPaginate(int page, int pageSize, string searchTerm)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchTerm)) searchTerm = searchTerm.ToLower();

                IQueryable<Language> myData;
                myData = uow.DbContext
                                .Languages
                                .AsNoTracking()
                                .Where(x => !x.IsDeleted)
                                .Where(c => searchTerm == null || c.Name.ToLower().Contains(searchTerm) ||
                                             c.Code.ToLower().Contains(searchTerm));

                int myDataCount = 0;
                myDataCount = myData.Count();
                ViewerPagination<LanguageCrudDto> viewerPagination = new ViewerPagination<LanguageCrudDto>();

                List<LanguageCrudDto> ReturnData = myData.OrderBy(a => a.ID).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new LanguageCrudDto
                {
                    ID = x.ID,
                    Code = x.Code,
                    Direction = x.Direction,
                    Icon = x.Icon,
                    Name = x.Name
                }).ToList();

                viewerPagination.PaginationList = ReturnData;

                viewerPagination.OriginalListListCount = myDataCount;
                return viewerPagination;
            }
            catch (Exception ex) { }
            return null;
        }

        public ViewerPagination<LocalizationCrudDto> getLocalizationWithPaginate(int page, int pageSize, string searchTerm)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchTerm)) searchTerm = searchTerm.ToLower();

                IQueryable<Localization> myData;
                myData = uow.DbContext
                                .Localizations
                                .AsNoTracking()
                                .Where(x => !x.IsDeleted)
                                .Where(c => searchTerm == null || c.Code.ToLower().Contains(searchTerm) ||
                                             c.Code.ToLower().Contains(searchTerm));

                int myDataCount = 0;
                myDataCount = myData.Count();
                ViewerPagination<LocalizationCrudDto> viewerPagination = new ViewerPagination<LocalizationCrudDto>();

                List<LocalizationCrudDto> ReturnData = myData.OrderBy(a => a.ID).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new LocalizationCrudDto
                {
                    ID = x.ID,
                    Code = x.Code
                }).ToList();

                viewerPagination.PaginationList = ReturnData;

                viewerPagination.OriginalListListCount = myDataCount;
                return viewerPagination;
            }
            catch (Exception ex) { }
            return null;
        }

        public ViewerPagination<LanguageLocalizationListDto> getLocalizationLanguageWithPaginate(int page, int pageSize, string searchTerm)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchTerm)) searchTerm = searchTerm.ToLower();

                IQueryable<Localization> myData;
                myData = uow.DbContext
                                .Localizations
                                .Include(x => x.LanguageLocalization)
                                .ThenInclude(x=>x.Language)
                                .AsNoTracking()
                                .Where(x => !x.IsDeleted)
                                .Where(c => searchTerm == null || c.Code.ToLower().Contains(searchTerm));

                int myDataCount = 0;
                myDataCount = myData.Count();
                ViewerPagination<LanguageLocalizationListDto> viewerPagination = new ViewerPagination<LanguageLocalizationListDto>();

                List<LanguageLocalizationListDto> ReturnData = myData.OrderBy(a => a.ID).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new LanguageLocalizationListDto
                {
                    ID = x.ID,
                    LocalizationCode = x.Code,
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
