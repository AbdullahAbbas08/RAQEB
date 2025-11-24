using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raqeb.BL;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace Raqeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : Controller
    {
        private readonly IUnitOfWork uow;
        public LookupController(IUnitOfWork _uow)
        {
            uow = _uow;
        }

        //[AllowAnonymous]
        //[HttpPost("InsertLanguageIcon")]
        //public async Task InsertLanguageIcon([FromForm] InsertImage image)
        //{
        //    var lang = uow.DbContext.Languages.FirstOrDefault(x => x.ID == image.ID);
        //    if (lang != null)
        //    {
        //        using (var imageStream = image.image.OpenReadStream())
        //        {
        //            var resizedImageStream = new MemoryStream();
        //            using (var imageSharp = Image.Load(imageStream))
        //            {
        //                imageSharp.Mutate(x => x.Resize(new ResizeOptions
        //                {
        //                    Size = new Size(30, 30),
        //                    Mode = ResizeMode.Max
        //                }));
        //                imageSharp.Mutate(x => x.BackgroundColor(Color.White));


        //                imageSharp.Save(resizedImageStream, new JpegEncoder());
        //            }
        //            byte[] resizedImageBytes = resizedImageStream.ToArray();
        //            string base64Image = $"data:image/png;base64,{Convert.ToBase64String(resizedImageBytes)}";


        //            lang.Icon = base64Image;
        //            uow.SaveChanges();
        //        }
        //    }
        //}

        [AllowAnonymous]
        [HttpPost("InsertLanguageIcon")]
        public async Task InsertLanguageIcon([FromForm] InsertImage image)
        {
            var lang = uow.DbContext.Languages.FirstOrDefault(x => x.ID == image.ID);
            if (lang != null)
            {
                using (var imageStream = image.image.OpenReadStream())
                {
                    var originalImageStream = new MemoryStream();
                    await imageStream.CopyToAsync(originalImageStream);

                    byte[] originalImageBytes = originalImageStream.ToArray();
                    string base64Image = $"data:image/png;base64,{Convert.ToBase64String(originalImageBytes)}";

                    lang.Icon = base64Image;
                    uow.SaveChanges();
                }
            }
        }


        [AllowAnonymous]
        [HttpPost("GetLanguage")]
        public List<LanguageDtoForHeader> GetLanguage()
        {
            return uow.Mapper.Map<List<LanguageDtoForHeader>>(uow.DbContext.Languages.Where(x=> !x.IsDeleted).ToList());
        }


        [HttpGet("getRegions")]
        public List<RegionDto> getRegions()
        {
            try
            {
                List<RegionDto> items = uow.DbContext.Regions.AsNoTracking().Select(x => new RegionDto
                {
                    ID = x.ID,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn
                }).ToList();
                return items;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("getCountries")]
        public List<CountryDto> getCountries()
        {
            try
            {
                List<CountryDto> items = uow.DbContext.Countries.AsNoTracking().Select(x => new CountryDto
                {
                    ID = x.ID,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn
                }).ToList();
                return items;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("getLocalizations")]
        public List<LocalizationDto> getLocalizations(string Lang)
        {
            try
            {
                List<LocalizationDto> items = uow.DbContext.Languages.Include(x => x.LanguageLocalization).AsNoTracking().Select(x =>
                new LocalizationDto
                {

                }).ToList();
                return items;
            }
            catch
            {
                return null;
            }
        }

      

        [HttpGet("getUsers")]
        public List<UserDto> getUsers()
        {
            try
            {
                var customerId = uow.SessionServices.CustomerID;
                List<UserDto> items = uow.DbContext.Users.Where(x=>x.CustomerId == customerId && !x.IsDeleted).AsNoTracking().Select(x => new UserDto
                {
                    ID = x.ID,
                    NameAr = x.NameAr, 
                    NameEn = x.NameEn,
                }).ToList();
                return items;
            }
            catch
            {
                return null;
            }
        }

     

    }
}
