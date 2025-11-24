using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Raqeb.BL.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        ILogger<GlobalExceptionFilter> logger = null;
        ISessionServices sessionServices;
        //private DatabaseContext _DbContext = new();
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> exceptionLogger, ISessionServices _sessionServices)
        {
            logger = exceptionLogger;
            sessionServices = _sessionServices;
        }

        public void OnException(ExceptionContext context)
        {
            //_DbContext.MoiaExceptions.Add(new MoiaException
            //{
            //    Message = context.Exception.Message.ToString(),
            //    StackTrace = context.Exception.StackTrace,
            //    Time = DateTime.Now,
            //    UserRoleId = sessionServices.UserRoleId
            //});
            //_DbContext.SaveChanges();
            // log the exception
            logger.LogError(0, context.Exception.GetBaseException(), "Exception occurred.");
        }
    }

}
