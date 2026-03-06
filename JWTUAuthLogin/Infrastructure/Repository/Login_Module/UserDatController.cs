
using JWTUAuthLogin.DBModel;
using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.DTO.Login_Module;
using JWTUAuthLogin.Shared.Common;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JWTUAuthLogin.Infrastructure.Repository.Login_Module
{
    public interface IProgramAccessChecker
    {
        Task<ServiceActionResult> login(string email, string password);
        Task<ServiceActionResult> register(UserDataDTO user);
        Task<ServiceActionResult> logout(string email, string deviceId);
        ServiceActionResult changePassword(int userId, string oldPassword, string newPassword);
    }

    public class ProgramAccessChecker : IProgramAccessChecker
    {
        private string ControllerLogLabel = "ProgramAccessChecker";
        private readonly MBDatabaseContext mb;

        public ProgramAccessChecker(IUnitOfWork unitOfWork)
        {
            mb = unitOfWork.DBContext;
        }

        public ServiceActionResult changePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                // 1. Validate inputs
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidUserCodeOrEmailOrPassword,
                        "Old password and new password are required."
                    );
                }

                // 2. Fetch user from database
                var user = mb.UserData.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                {
                    return new ServiceActionResult(
                        ReturnStatus.NotFound,
                        "User not found."
                    );
                }

                // 3. Check old password
                if (user.Password != oldPassword)
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidPassword,
                        "Old password is incorrect."
                    );
                }

                // 4. Update password
                user.Password = newPassword;
                mb.UserData.Update(user);
                mb.SaveChanges();

                return new ServiceActionResult(
                    ReturnStatus.success,
                    "Password changed successfully."
                );
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(
                    ControllerLogLabel + "->ChangePassword",
                    ex.Message,
                    ex,
                    ""
                );
            }
        }

        public async Task<ServiceActionResult> register(UserDataDTO user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidUserCodeOrEmailOrPassword,
                        "Email and Password are required."
                    );
                }

                // Check Email already exists
                var existUser = await mb.UserData
                    .FirstOrDefaultAsync(x => x.Email == user.Email);

                if (existUser != null)
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidUserCodeOrEmailOrPassword,
                        "Email already exists."
                    );
                }

                // Create new user
                var newUser = new UserDatum
                {
                    Username = string.IsNullOrWhiteSpace(user.UserName) ? "UnknownUser" : user.UserName,
                    Email = string.IsNullOrWhiteSpace(user.Email) ? "unknown@example.com" : user.Email,
                    Password = string.IsNullOrWhiteSpace(user.Password) ? "defaultpass" : user.Password,
                    OS = user.Os,
                    OSVersion = user.OSVersion,
                    ModelNo = user.ModelNo,
                    Manufacturer = user.Manufacturer,
                    RegToken = user.RegToken,
                    DeviceID = user.DeviceID,
                    LocalTimeZone = user.LocalTimeZone,
                };

                mb.UserData.Add(newUser);
                await mb.SaveChangesAsync();

                return new ServiceActionResult(
                    ReturnStatus.success,
                    "User registered successfully.",
                    newUser
                );
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(
                    ControllerLogLabel + "->Register",
                    ex.Message,
                    ex,
                    ""
                );
            }
        }
        public async Task<ServiceActionResult> logout(string email, string deviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(deviceId))
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidUserCodeOrEmailOrPassword,
                        "Invalid email or device ID."
                    );
                }
                var dbUser = await mb.UserData.FirstOrDefaultAsync(x => x.Email == email);

                if (dbUser == null)
                {
                    return new ServiceActionResult(
                        ReturnStatus.NotFound,
                        "User not found."
                    );
                }
                // Password check (Production: hashed password)
                if (dbUser.DeviceID != deviceId)
                {
                    return new ServiceActionResult(
                        ReturnStatus.NotFound,
                        "Device ID not found."
                    );
                }
                return new ServiceActionResult(
                   ReturnStatus.success,
                   "Logout successful"
               );


            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(
                    ControllerLogLabel + "->Logout",
                    ex.Message,
                    ex,
                    ""
                );
            }
        }
        public async Task<ServiceActionResult> login(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidUserCodeOrEmailOrPassword,
                        "Authentication failed: incorrect email or password."
                    );
                }
                var dbUser = await mb.UserData
                    .FirstOrDefaultAsync(x => x.Email == email);

                if (dbUser == null)
                {
                    return new ServiceActionResult(
                        ReturnStatus.NotFound,
                        "User not found."
                    );
                }

                // Password check (Production: hashed password)
                if (dbUser.Password != password)
                {
                    return new ServiceActionResult(
                        ReturnStatus.error_InvalidPassword,
                        "Invalid password."
                    );
                }

                return new ServiceActionResult(
                    ReturnStatus.success,
                    "Login successful",
                    new
                    {
                        UserId = dbUser.UserId,
                        Username = dbUser.Username,
                        Email = dbUser.Email
                    }
                );
            }
            catch (Exception ex)
            {
                return Common_Methods.doErrorLog(
                    ControllerLogLabel + "->login",
                    ex.Message,
                    ex,
                    ""
                );
            }
        }
    }
}