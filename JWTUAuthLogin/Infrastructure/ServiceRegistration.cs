using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.Infrastructure.Repository.Login_Module;
using JWTUAuthLogin.Infrastructure.Repository.System_Module;
using JWTUAuthLogin.Infrastructure.Repository.Token_Module;

namespace JWTUAuthLogin.Infrastructure
{
    public static class fluttServiceRegistration
    {
        public static void AddInterfraStructure(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IProgramAccessChecker, ProgramAccessChecker>();
            services.AddTransient<ITokenManager, TokenManagerController>();
            services.AddTransient<ISysAttachmentController, SysAttachmentController>();
        }
    }
}
