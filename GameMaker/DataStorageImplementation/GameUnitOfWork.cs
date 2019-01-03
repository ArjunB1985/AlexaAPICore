using GameMaker.DataStorageInterface;
using GameMaker.Implementations;
using GameMaker.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.DataStorageImplementation
{
    public class GameUnitOfWork<TEntity> : UnitOfWork<TEntity> where TEntity : class
    {

        private GameRepositoryImpl<TEntity> entityRepository;
        private GameRepositoryImpl<Game> gameRepository;
       

        private GameContext context;
        public GameUnitOfWork()
        {
            this.context = new GameContext();
        }

        public override GameRepository<Game> GetGameRepository()
        {
            if (gameRepository == null)
            {
                gameRepository = new GameRepositoryImpl<Game>(context);
            }
            return gameRepository;
        }

        public override GameRepository<TEntity> GetEntityRepository()
        {
            if (entityRepository == null)
            {
                entityRepository = new GameRepositoryImpl<TEntity>(context);
            }
            return entityRepository;
        }

        public override void SaveChanges()
        {
            context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}
