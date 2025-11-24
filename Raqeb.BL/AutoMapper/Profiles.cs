using AutoMapper;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using Raqeb.Shared.ViewModels;

namespace Raqeb.AutoMapper
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {

            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserWithImageDto>().ReverseMap();
          
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Localization, LocalizationDto>().ReverseMap();
            CreateMap<Localization, LocalizationCrudDto>().ReverseMap();
            CreateMap<LanguageLocalization, LanguageLocalizationDto>().ReverseMap();
            CreateMap<LanguageLocalization, LanguageLocalizationListDto>().ReverseMap();
            CreateMap<Language, LanguageDto>().ReverseMap();
            CreateMap<Language, LanguageCrudDto>().ReverseMap();
            CreateMap<Language, LanguageDtoForHeader>()
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
                .ReverseMap();
            CreateMap<Customer, CustomerDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();
         

            // CreateMap<Site, SiteDto>()
            //.ForMember(dest => dest.SiteRole, opt => opt.Ignore())
            //.ForMember(dest => dest._CMP_CSL_PM, opt => opt.Ignore())
            //.ForMember(dest => dest._FC_EHS_Manager, opt => opt.Ignore())
            //.ForMember(dest => dest._ServiceManager_OutageManager, opt => opt.Ignore())
            //.ForMember(dest => dest._P_L_Manager, opt => opt.Ignore());

        }
    }
}
