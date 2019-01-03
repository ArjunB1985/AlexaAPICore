using GameMaker.DataStorageInterface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace GameMaker.DataStorageImplementation
{
    public class GameRepositoryImpl<TEntity> : GameRepository<TEntity> where TEntity : class
    {
        internal GameContext context;
        internal DbSet<TEntity> dbSet;

        public GameRepositoryImpl(GameContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }
        public override void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public override TEntity FindById(int entityId)
        {
            return dbSet.Find(entityId);
        }

        public override IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public override void Remove(TEntity entity)
        {
            if (context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public override void Remove(int entityId)
        {
            TEntity entityToDelete = dbSet.Find(entityId);
            Remove(entityToDelete);
        }



        public override void Update(TEntity entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
