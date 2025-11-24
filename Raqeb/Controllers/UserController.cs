using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Raqeb.Shared.DTOs;
using Raqeb.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
namespace Raqeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        [HttpGet("EncryptValue")]
        public string EncryptConnectionString(string ConnectionString)
        {
            return EncryptHelper.Encrypt(ConnectionString);
        }

        [AllowAnonymous]
        [HttpGet("DecryptValue")]
        public string DecryptConnectionString(string ConnectionString)
        {
            return EncryptHelper.Decrypt(ConnectionString);
        }

        //[AllowAnonymous]
        //[HttpPost("InsertUserImage")]
        //public async Task InsertImage([FromForm] InsertImage image)
        //{
        //    var user = unitOfWork.DbContext.Users.FirstOrDefault(x => x.ID == image.ID);
        //    if (user != null)
        //    {
        //        user.Image = await unitOfWork.ConvertIFormFileToByteArray(image.image);
        //        unitOfWork.SaveChanges();
        //    }
        //}

        //[AllowAnonymous]
        //[HttpPost("InsertUserImage")]
        //public async Task InsertImage([FromForm] InsertImage image)
        //{
        //    var user = unitOfWork.DbContext.Users.FirstOrDefault(x => x.ID == image.ID);
        //    if (user != null)
        //    {
        //        using (var imageStream = image.image.OpenReadStream())
        //        {
        //            var resizedImageStream = new MemoryStream();
        //            using (var imageSharp = Image.Load(imageStream))
        //            {
        //                imageSharp.Mutate(x => x.Resize(new ResizeOptions
        //                {
        //                    Size = new Size(35, 35),
        //                    Mode = ResizeMode.Max 
        //                }));

        //                imageSharp.Save(resizedImageStream, new JpegEncoder()); 
        //            }
        //            byte[] resizedImageBytes = resizedImageStream.ToArray();
        //            user.Image = resizedImageBytes;
        //            unitOfWork.SaveChanges();
        //        }
        //    }
        //}

        [AllowAnonymous]
        [HttpPost("InsertUserImage")]
        public async Task InsertImage([FromForm] InsertImage image)
        {
            var user = unitOfWork.DbContext.Users.FirstOrDefault(x => x.ID == image.ID);
            if (user != null)
            {
                using (var imageStream = image.image.OpenReadStream())
                {
                    var originalImageStream = new MemoryStream();
                    await imageStream.CopyToAsync(originalImageStream); // Copy the original image stream
                    user.Logo = originalImageStream.ToArray();
                    user.Image = $"data:image/png;base64,{Convert.ToBase64String(originalImageStream.ToArray())}";
                    unitOfWork.SaveChanges();
                }
            }
        }

        [HttpPost("GetUserImage")]
        public async Task<string> GetUserImage(int userId)
        {
            var user =await unitOfWork.DbContext.Users.FirstOrDefaultAsync(x => x.ID == userId);
            if (user != null) return user.Image;
            return null;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginModel model)
        {
            try
            {
                //var res = unitOfWork.DbContext.Users.FirstOrDefault(x => x.Email == model.Email);
                //if (res != null)
                //{
                //    if (res.TryloginCount > 5)
                //    {
                //        return Ok(EncryptHelper.ShiftString(EncryptHelper.EncryptString(JsonConvert.SerializeObject(new LoginResult
                //        {
                //            Token = null,
                //            refreshToken = null,
                //            Message = "LockAccount"
                //        })), 6));
                //    }
                //}


                var result = await unitOfWork.User.Login(model);
                if (result.Status == 200 && result.Message == null)
                {
                    //if (res != null)
                    //{
                    //    if (res.TryloginCount == null) res.TryloginCount = 0;
                    //    res.TryloginCount = 0;
                    //    unitOfWork.SaveChanges();
                    //}

                    return Ok(EncryptHelper.ShiftString(EncryptHelper.EncryptString(JsonConvert.SerializeObject(new LoginResult
                    {
                        Token = result.Token,
                        refreshToken = result.refreshToken,
                        Message = null,
                    })), 6));
                }
                else if (result.Status == 400 && result.Message != null)
                {
                    //if (res != null)
                    //{
                    //    if (res.TryloginCount == null) res.TryloginCount = 0;
                    //    res.TryloginCount += 1;
                    //    unitOfWork.SaveChanges();
                    //}

                    return Ok(EncryptHelper.ShiftString(EncryptHelper.EncryptString(JsonConvert.SerializeObject(new LoginResult
                    {
                        Token = null,
                        refreshToken = null,
                        Message = result.Message
                    })), 6));
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }


        }


        //[AllowAnonymous]
        //[HttpPost("ChangePassword")]
        //public async Task<bool> ChangePassword([FromBody] ChangePasswordModel model)
        //{
        //    try
        //    {
        //        var result = await unitOfWork.User.ChangePassword(model);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //[AllowAnonymous]
        //[HttpPost("ResetPassword")]
        //public async Task<bool> ResetPassword([FromBody] ChangePasswordModel model)
        //{
        //    try
        //    {
        //        var result = await unitOfWork.User.ResetPassword(model);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        [AllowAnonymous]
        [HttpPost("[action]")]
        [ProducesResponseType(200, Type = typeof(AccessToken))]
        public async Task<IActionResult> RefreshToken([FromBody] JToken jsonBody)
        {
            string refreshToken = jsonBody.Value<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await unitOfWork.TokenStoreRepository.FindTokenAsync(refreshToken);
            if (token == null)
            {
                return Unauthorized();
            }
            int applicationType = int.Parse(unitOfWork.SessionServices.ApplicationType);

            var (accessToken, newRefreshToken, claims, count) = await unitOfWork.TokenStoreRepository.CreateJwtTokens(token.User, applicationType, refreshToken);
            //_antiforgery.RegenerateAntiForgeryCookies(claims);
            return Ok(new AccessToken { access_token = accessToken, refresh_token = newRefreshToken });
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(bool))]
        public async Task<bool> Logout(string refreshToken)
        {
            try
            {
                ClaimsIdentity claimsIdentity = this.User.Identity as ClaimsIdentity;
                string userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

                //string name = User.Identity.Name;

                // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
                // Delete the user's tokens from the database (revoke its bearer token)
                await unitOfWork.TokenStoreRepository.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
                string[] ExecptParm = new string[] { };
                unitOfWork.SessionServices.ClearSessionsExcept(ExecptParm);
                // _antiforgery.DeleteAntiForgeryCookies();
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }



        #region ***************  Lookup ************************************************************
        [HttpPost]
        [Route("getWithPaginate")]
        [ProducesResponseType(typeof(ViewerPagination<UserDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult getWithPaginate(string? searchTerm, int page, int pageSize)
        {
            try
            {
                var myList = unitOfWork.User.getWithPaginate(page, pageSize, searchTerm);
                return Ok(myList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById")]
        public virtual async Task<UserDTO> GetById(int id)
        {
            var res = await unitOfWork.DbContext.Users.Include(x=>x.Customer).FirstOrDefaultAsync(x => x.ID == id && !x.IsDeleted);
            UserDTO ReturnedRes = unitOfWork.Mapper.Map<UserDTO>(res);
            ReturnedRes.Password = EncryptHelper.Decrypt(ReturnedRes.Password);
            ReturnedRes.CustomerName = res.Customer?.NameAr;
            if (res.Logo != null)
                ReturnedRes.LogoBase64 = "data:image/png;base64," + Convert.ToBase64String(res.Logo);
            return ReturnedRes;
        }


        [HttpPost("SaveData")]
        public virtual async Task<bool> SaveData([FromForm] UserDTO entity)
        {
            try
            {
                var ExistItem = await unitOfWork.User.DbSet.FirstOrDefaultAsync(x => x.ID == entity.ID);
                if (ExistItem == null)
                {
                        //entity.Logo = unitOfWork.ConvertIFormFileToByteArray(entity.LogoForm).Result;
                    entity.Password = EncryptHelper.Encrypt(entity.Password);
                    User model = unitOfWork.Mapper.Map<User>(entity);
                        entity.Image = entity.Image;
                    await unitOfWork.User.DbSet.AddAsync(model);
                    await unitOfWork.SaveChangesAsync();
                    return true;
                }
                else
                {
                    ExistItem.NameAr = string.IsNullOrEmpty(entity.NameAr) ? ExistItem.NameAr : entity.NameAr;
                    ExistItem.NameEn = string.IsNullOrEmpty(entity.NameEn) ? ExistItem.NameAr : entity.NameEn;
                    ExistItem.Email = string.IsNullOrEmpty(entity.Email) ? ExistItem.Email : entity.Email;
                    ExistItem.Mobile = string.IsNullOrEmpty(entity.Mobile) ? ExistItem.Mobile : entity.Mobile;
                    ExistItem.CustomerId = entity.CustomerId;
                    if(entity.OldPassword == EncryptHelper.Decrypt(ExistItem.Password))
                    ExistItem.Password = EncryptHelper.Encrypt(entity.Password);
                        ExistItem.Image = entity.Image;
                    //ExistItem.Logo = unitOfWork.ConvertIFormFileToByteArray(entity.LogoForm).Result;
                    unitOfWork.SaveChanges();
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
                var res = await unitOfWork.DbContext.Users.FindAsync(id);
                if (res != null)
                {
                    res.IsDeleted = true;
                    unitOfWork.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }
        #endregion

    }
}