using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.Infrastructure.Repository.Login_Module;

namespace JWTUAuthLogin.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInterfraStructure(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IProgramAccessChecker, ProgramAccessChecker>();
        }
    }
}
