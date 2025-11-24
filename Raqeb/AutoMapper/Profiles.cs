//using AutoMapper;
//using Raqeb.Shared.Models;
//using Raqeb.Shared.ViewModels.DTOs;

//namespace Raqeb.BL.AutoMapper
//{
//    public class ProfileMapping : Profile
//    {
//        public ProfileMapping()
//        {
//            CreateMap<User, UserDto>().ReverseMap();
//            CreateMap<Country, CountryDto>().ReverseMap();
//            CreateMap<Segment, SegmentDto>().ReverseMap();
//            CreateMap<Localization, LocalizationDto>().ReverseMap();
//            CreateMap<LanguageLocalization, LanguageLocalizationDto>().ReverseMap();
//            CreateMap<Language, LanguageDto>().ReverseMap();
//            CreateMap<Section, SectionDto>().ReverseMap();
//            CreateMap<Question, QuestionDto>().ForMember(dest => dest.Record, opt => opt.MapFrom(src => src.Records.FirstOrDefault())).ReverseMap();
//            CreateMap<QuestionType, QuestionTypeDto>().ReverseMap();
//            CreateMap<QuestionTypeValue, QuestionTypeValueDto>().ReverseMap();
//            CreateMap<Record, RecordDto>().ReverseMap();
//        }
//    }
//}
