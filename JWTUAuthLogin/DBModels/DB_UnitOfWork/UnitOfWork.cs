using JWTUAuthLogin.DBModels.JWTUAuthLogin;
using System.ComponentModel.DataAnnotations;

namespace JWTUAuthLogin.DBModels.DB_UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        MBDatabaseContext DBContext { get; }
        void Commit();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }

    public class UnitOfWork : IUnitOfWork
    {
        #region [Declaration Variables]
        public readonly MBDatabaseContext _context;

        #endregion

        #region [Repository Initializatiion]

        public MBDatabaseContext DBContext
        {
            get { return _context; }
        }

        #endregion

        #region [Dispose and Commit]

        public bool disposed = false;

        public UnitOfWork(MBDatabaseContext context)
        {
            _context = context;
        }

        public void Commit()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (ValidationException vexp)
            {
                foreach (var exp in vexp.Data.Values)
                {
                    string error = exp.ToString();
                    Console.WriteLine(error);
                }
            }
        }

        #endregion

        #region [Begin, Commit and Rollback Transaction]

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
        #endregion
}
