
using JWTUAuthLogin.DBModel;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTUAuthLogin.Shared.Common
{
    public static class Common_Methods
    {
        public static ServiceActionResult doErrorLog(
             string logLabel,
             string message,
             Exception ex,
             string requestor
         )
        {
            string internalMessage = "";
            if (ex.InnerException != null)
            {
                internalMessage = ex.InnerException.Message;
            }
            SysErrorLog theLog = new SysErrorLog();
            #region write log to database
            try
            {
                MBDatabaseContext dc = CreateMainDbContext();
                theLog = new SysErrorLog()
                {
                    ErrorLogId = Guid.NewGuid().ToString(),
                    LogOn = DateTime.Now,
                    Description = message + "..." + internalMessage,
                    Priority = "Normal",
                    Program = logLabel,
                    UserId = requestor,
                    UserCode = ""
                };
                dc.SysErrorLogs.Add(theLog);
                dc.SaveChanges();
            }
            catch (Exception ex2) { }
            #endregion

            return new ServiceActionResult(
                ReturnStatus.InternalServerError,
                logLabel + "::" + message + "..." + internalMessage,
                theLog.ErrorLogId
            );
        }

        public static ServiceActionResult doErrorLogCode(
            ReturnStatus statusCode,
            string logLabel,
            string message,
            Exception ex,
            string requestor
        )
        {
            string internalMessage = "";
            if (ex.InnerException != null)
            {
                internalMessage = ex.InnerException.Message;
            }
            SysErrorLog theLog = new SysErrorLog();
            #region write log to database
            try
            {
                MBDatabaseContext dc = CreateMainDbContext();
                theLog = new SysErrorLog()
                {
                    ErrorLogId = Guid.NewGuid().ToString(),
                    LogOn = DateTime.Now,
                    Description = message + "..." + internalMessage,
                    Priority = "Normal",
                    Program = logLabel,
                    UserId = requestor,
                    UserCode = ""
                };
                dc.SysErrorLogs.Add(theLog);
                dc.SaveChanges();
            }
            catch (Exception ex2) { }
            #endregion

            return new ServiceActionResult(statusCode, internalMessage, theLog.ErrorLogId);
        }

        public static string GetAppSettingValue(string session, string variableName)
        {
            try
            {
                var result = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection(session)[variableName];
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static MBDatabaseContext CreateMainDbContext()
        {
            DbContextOptionsBuilder<MBDatabaseContext> _optionsBuilder =
                new DbContextOptionsBuilder<MBDatabaseContext>();
            _optionsBuilder.UseSqlServer(
                Common_Methods.GetAppSettingValue("ConnectionStrings", "Connection")
            );

            MBDatabaseContext _DBContext = new MBDatabaseContext(_optionsBuilder.Options);
            return _DBContext;
        }
    }
}
