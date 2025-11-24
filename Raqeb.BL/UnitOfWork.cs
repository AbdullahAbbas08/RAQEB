using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Raqeb.BL.Repositories;
using Raqeb.Shared.Models;

namespace Raqeb.BL
{
    public interface IUnitOfWork
    {
        string ContentRootPath { get; }
        string WebRootPath { get; }
        //IBackgroundJobClient BackgroundJobClient { get; }
        DatabaseContext DbContext { get; }
        int ExecuteSqlRaw(string sql, params object[] parameters);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        IMapper Mapper { get; }
        HttpContext HttpContext { get; }
        IConfiguration Configuration { get; }
        IUserTokenRepository UserTokenRepository { get; }
        ICustomerRepository Customer { get; }
        ITokenStoreRepository TokenStoreRepository { get; }
        ISessionServices SessionServices { get; }
        IDataProtectRepository DataProtectRepository { get; }
        ISecurityRepository SecurityRepository { get; }
        IOptionsSnapshot<BearerTokensOptions> OptionsSnapshot { get; }
        IMailServices MailServices { get; }
        Task<byte[]> ConvertIFormFileToByteArray(IFormFile file);
        public byte[] ConvertBase64ToByteArray(string base64String);

        #region Repositories
        public IUserRepository User { get; }
        public ILocalizationRepository Localization { get; }
        #endregion
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly string contentRootPath;
        private readonly string webRootPath;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly HttpContext httpContext;
        private readonly DatabaseContext db;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly ISessionServices SessionServices;
        private readonly IOptionsSnapshot<BearerTokensOptions> OptionsSnapshot;

        public UnitOfWork(DatabaseContext _db,
                          IConfiguration _configuration,
                          IHostingEnvironment _hostEnvironment,
                          //IBackgroundJobClient _backgroundJobClient,
                          IMapper _mapper,
                           IOptionsSnapshot<BearerTokensOptions> _OptionsSnapshot,
                           ISessionServices _SessionServices,
                          HttpContextAccessor httpContextAccessor)
        {
            db = _db;
            configuration = _configuration;
            //backgroundJobClient = _backgroundJobClient;
            contentRootPath = _hostEnvironment.ContentRootPath;
            webRootPath = _hostEnvironment.WebRootPath;
            mapper = _mapper;
            OptionsSnapshot = _OptionsSnapshot;
            SessionServices = _SessionServices;
            httpContext = httpContextAccessor?.HttpContext;
        }

        IOptionsSnapshot<BearerTokensOptions> IUnitOfWork.OptionsSnapshot => OptionsSnapshot;
        public string ContentRootPath => this.contentRootPath;
        public string WebRootPath => this.webRootPath;
        public IBackgroundJobClient BackgroundJobClient => this.backgroundJobClient;
        public HttpContext HttpContext => this.httpContext;
        public IMapper Mapper => this.mapper;
        public IConfiguration Configuration => this.configuration;
        ISessionServices IUnitOfWork.SessionServices => SessionServices;
        //public IMailServices MailServices => mailServices;

        private DatabaseContext dbContext;
        public DatabaseContext DbContext
        {
            get
            {
                if (this.dbContext == null) this.dbContext = db;
                return dbContext;
            }
        }
        private DatabaseFacade database;
        public DatabaseFacade Database
        {
            get
            {
                if (this.database == null) this.database = DbContext.Database;
                return database;
            }
        }

        public int ExecuteSqlRaw(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlRaw(sql, parameters);
        }

        public int SaveChanges()
        {
            return db.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await db.SaveChangesAsync();
        }

        private IUserRepository user;
        public IUserRepository User
        {
            get
            {
                if (this.user == null)
                {
                    this.user = new UserRepository(this);
                }
                return user;
            }
        }

       

      

        private ILocalizationRepository localization;
        public ILocalizationRepository Localization
        {
            get
            {
                if (this.localization == null)
                {
                    this.localization = new LocalizationRepository(this);
                }
                return localization;
            }
        }

        private IUserTokenRepository userTokenRepository;
        public IUserTokenRepository UserTokenRepository
        {
            get
            {
                userTokenRepository ??= new UserTokenRepository(this);
                return userTokenRepository;
            }
        }
        
        private ICustomerRepository customerRepository;
        public ICustomerRepository Customer
        {
            get
            {
                customerRepository ??= new CustomerRepository(this);
                return customerRepository;
            }
        }
        
       
        
        private IMailServices mailServices;
        public IMailServices MailServices
        {
            get
            {
                mailServices ??= new MailServices(this);
                return mailServices;
            }
        }


        private ITokenStoreRepository tokenStoreRepository;
        public ITokenStoreRepository TokenStoreRepository
        {
            get
            {
                tokenStoreRepository ??= new TokenStoreRepository(this);
                return tokenStoreRepository;
            }
        }

        private IDataProtectRepository dataProtectRepository;
        public IDataProtectRepository DataProtectRepository
        {
            get
            {
                dataProtectRepository ??= new DataProtectRepository();
                return dataProtectRepository;
            }
        }

        private ISecurityRepository securityRepository;
        public ISecurityRepository SecurityRepository
        {
            get
            {
                securityRepository ??= new SecurityRepository();
                return securityRepository;
            }
        }



        public async Task<byte[]> ConvertIFormFileToByteArray(IFormFile file)
        {
            byte[] BinaryContent = null;
            if (file != null)
            {
                using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    BinaryContent = binaryReader.ReadBytes((int)file.Length);
                }
            }
            return BinaryContent;
        }

        public byte[] ConvertBase64ToByteArray(string base64String)
        {
            if (!string.IsNullOrEmpty(base64String))
            {
                try
                {
                    return Convert.FromBase64String(base64String);
                }
                catch (Exception ex)
                {

                }
            }

            return null;
        }




    }
}
