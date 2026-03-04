using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.DBModels.JWTUAuthLogin;
using JWTUAuthLogin.DTO.Login_Module;
using JWTUAuthLogin.Infrastructure.Repository.TokenModule;
using JWTUAuthLogin.Shared.Common;
using JWTUAuthLogin.Shared.Models;

namespace JWTUAuthLogin.Infrastructure.Repository.Login_Module
{
    public interface IProgramAccessChecker
    {
        Task<ServiceActionResult> login(UserDataDTO user);
        ServiceActionResult renewToken(string email, string password);
        ServiceActionResult logout(string email, string deviceId);
        ServiceActionResult changePassword(string userid,string oldPassword, string newPassword);
    }
    public class ProgramAccessChecker : IProgramAccessChecker
    {
        private string ControllerLogLabel = "ProgramAccessChecker";
        private readonly MBDatabaseContext mb;
        private readonly ITokenManager _tokenManager;
        public ProgramAccessChecker(IUnitOfWork unitOfWork, ITokenManager tokenManager)
        {
            mb = unitOfWork.DBContext;
            _tokenManager = tokenManager;
        }   

        public ServiceActionResult changePassword(string userid, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
        public ServiceActionResult renewToken(string email, string password)
        {
            throw new NotImplementedException();
        }
        //public ServiceActionResult logout(string email, string deviceId)
        //{
        //    throw new NotImplementedException();
        //}
        public Task<ServiceActionResult> login(UserDataDTO user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                {
                    return ServiceActionResult.Failed("Email and Password are required.");
                }

                var dbUser =  mb.Users
                    .FirstOrDefaultAsync(x => x.Email == user.Email);

                if (dbUser == null)
                {
                    return ServiceActionResult.Failed("User not found.");
                }

                if (dbUser.Password != user.Password)
                {
                    return ServiceActionResult.Failed("Invalid password.");
                }

                // Generate Token
                var token = _tokenManager.GenerateToken(dbUser);

                return ServiceActionResult.Success(new
                {
                    Token = token,
                    UserId = dbUser.UserId,
                    Email = dbUser.Email
                });
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(
                    ControllerLogLabel + "->" + "login",
                    ex.Message,
                    ex,
                    ""
                );
            }
        }
        public Task<ServiceActionResult> login(UserDataDTO user)
        {
            throw new NotImplementedException();
        }
    }
}
