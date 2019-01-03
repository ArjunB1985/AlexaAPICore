using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace GameMaker.DataStorageInterface
{
    public abstract class GameRepository<TEntity>
    {
        public abstract void Add(TEntity entity);
        public abstract void Update(TEntity entity);
        public abstract void Remove(TEntity entity);

        public abstract void Remove(int entityId);
        public abstract IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        public abstract TEntity FindById(int entityId);

    }
}
