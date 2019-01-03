using GameMaker.Implementations;
using GameMaker.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.DataStorageInterface
{
    public abstract class UnitOfWork<TEntity> : IDisposable
    {
        protected bool disposed = false;

        protected abstract void Dispose(bool disposing);
        public abstract GameRepository<Game> GetGameRepository();

        public abstract GameRepository<TEntity> GetEntityRepository();
            public abstract void SaveChanges();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
