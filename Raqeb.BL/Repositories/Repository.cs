using Raqeb.Shared.Models;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic.Core;


namespace Raqeb.BL.Repositories
{
    public interface IRepository<Entity> where Entity : class
    {
        DbSet<Entity> DbSet { get; }
        Entity Add(Entity entity);
        IEnumerable<Entity> AddRange(IEnumerable<Entity> entities);
        bool Any();
        bool Any(Expression<Func<Entity, bool>> predicate);
        int Count();
        int Count(Func<Entity, bool> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<Entity, bool>> predicate);
        Entity this[int id] { get; }
        Entity this[string id] { get; }
        Entity Find(int id);
        Entity Find(string id);
        Task<Entity> FindAsync(int id);
        Task<Entity> FindAsync(string id);
        Entity FirstOrDefault();
        Entity FirstOrDefault(Func<Entity, bool> predicate);
        Task<Entity> FirstOrDefaultAsync();
        Task<Entity> FirstOrDefaultAsync(Expression<Func<Entity, bool>> predicate);
        IEnumerable<Entity> Get();
        IQueryable<Entity> GetAsNoTracking();
        Entity Remove(Entity entity);
        IEnumerable<Entity> RemoveRange(IEnumerable<Entity> entities);
        Entity Update(Entity entity);
        IEnumerable<Entity> Where(Func<Entity, bool> predicate);
        IQueryable<Entity> Query(Expression<Func<Entity, bool>> predicate);
        IOrderedEnumerable<Entity> OrderBy(Func<Entity, object> keySelector);
        IOrderedEnumerable<Entity> OrderByDescending(Func<Entity, object> keySelector);
        IQueryable<Entity> OrderBy(string ordering, params object[] args);
        IUnitOfWork UOW { get; }
        IQueryable<Entity> Skip(int count);
        IQueryable<Entity> Take(int count);
        IQueryable<Entity> Paging(int pageNumber, int PageSize);
        IQueryable<Entity> Paging(string ordering, int pageNumber, int PageSize);
        decimal Sum(Func<Entity, decimal> selector);
    }

    public class Repository<Entity> : IRepository<Entity> where Entity : class
    {
        private readonly DatabaseContext db;
        public IUnitOfWork UOW { get; }

        public Repository(IUnitOfWork uow)
        {
            this.UOW = uow;
            this.db = uow.DbContext;
        }

        public DbSet<Entity> DbSet
        {
            get
            {
                return db.Set<Entity>();
            }
        }

        public virtual IQueryable<Entity> Skip(int count)
        {
            return DbSet.Skip(count);
        }

        public virtual IQueryable<Entity> Take(int count)
        {
            return DbSet.Take(count);
        }

        public virtual IQueryable<Entity> Paging(int pageNumber, int PageSize)
        {
            return DbSet.Skip((pageNumber - 1) * PageSize).Take(PageSize);
        }

        public virtual IQueryable<Entity> Paging(string ordering, int pageNumber, int PageSize)
        {
            return DbSet.OrderBy(ordering).Skip((pageNumber - 1) * PageSize).Take(PageSize);
        }

        public virtual IEnumerable<Entity> Get()
        {
            return DbSet;
        }

        public virtual IQueryable<Entity> GetAsNoTracking()
        {
            return DbSet.AsNoTracking();
        }

        public IOrderedEnumerable<Entity> OrderBy(Func<Entity, object> keySelector)
        {
            return DbSet.OrderBy(keySelector);
        }

        public IQueryable<Entity> OrderBy(string ordering, params object[] args)
        {
            return DbSet.OrderBy(ordering, args);
        }

        public IOrderedEnumerable<Entity> OrderByDescending(Func<Entity, object> keySelector)
        {
            return DbSet.OrderByDescending(keySelector);
        }

        public virtual IEnumerable<Entity> Where(Func<Entity, bool> predicate)
        {
            return DbSet.Where(predicate);
        }

        public virtual IQueryable<Entity> Query(Expression<Func<Entity, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public virtual Entity FirstOrDefault()
        {
            return DbSet.FirstOrDefault();
        }

        public virtual Entity FirstOrDefault(Func<Entity, bool> predicate)
        {
            return DbSet.FirstOrDefault(predicate);
        }

        public virtual async Task<Entity> FirstOrDefaultAsync()
        {
            return await DbSet.FirstOrDefaultAsync();
        }

        public virtual async Task<Entity> FirstOrDefaultAsync(Expression<Func<Entity, bool>> predicate)
        {
            return await DbSet.FirstOrDefaultAsync(predicate);
        }

        public bool Any()
        {
            return DbSet.Any();
        }

        public bool Any(Expression<Func<Entity, bool>> predicate)
        {
            return DbSet.Any(predicate);
        }

        public virtual int Count()
        {
            return DbSet.Count();
        }

        public virtual int Count(Func<Entity, bool> predicate)
        {
            return DbSet.Count(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<Entity, bool>> predicate)
        {
            return await DbSet.CountAsync(predicate);
        }

        private PropertyInfo IdPropInfo
        {
            get
            {
                return typeof(Entity).GetProperties().FirstOrDefault(p => p.Name.ToLower() == "id");
            }
        }

        public Entity this[string id] => Find(id);

        public Entity this[int id] => Find(id);

        public virtual Entity Find(int id)
        {
            return this.IdPropInfo.PropertyType == typeof(int) ? DbSet.Find(id) : null;
        }

        public virtual Entity Find(string id)
        {
            switch (this.IdPropInfo.PropertyType.Name)
            {
                case "Int32":
                    if (int.TryParse(id.DecryptId(), out int encryptedIntId)) return DbSet.Find(encryptedIntId);
                    break;
                case "String":
                    Entity entity = DbSet.Find(id);
                    if (entity == null) entity = DbSet.Find(id.DecryptId());
                    return entity;
                default:
                    break;
            }

            return null;
        }

        public virtual async Task<Entity> FindAsync(int id)
        {
            return this.IdPropInfo.PropertyType == typeof(int) ? await DbSet.FindAsync(id) : null;
        }

        public virtual async Task<Entity> FindAsync(string id)
        {
            switch (this.IdPropInfo.PropertyType.Name)
            {
                case "Int32":
                    if (int.TryParse(id.DecryptId(), out int encryptedIntId)) return await DbSet.FindAsync(encryptedIntId);
                    break;
                case "String":
                    Entity entity = await DbSet.FindAsync(id);
                    if (entity == null) entity = await DbSet.FindAsync(id.DecryptId());
                    return entity;
                default:
                    break;
            }

            return null;
        }

        public virtual Entity Add(Entity entity)
        {
            return DbSet.Add(entity).Entity;
        }

        public virtual IEnumerable<Entity> AddRange(IEnumerable<Entity> entities)
        {
            DbSet.AddRange(entities);
            return entities;
        }

        public virtual Entity Remove(Entity entity)
        {
            return DbSet.Remove(entity).Entity;
        }

        public virtual IEnumerable<Entity> RemoveRange(IEnumerable<Entity> entities)
        {
            DbSet.RemoveRange(entities);
            return entities;
        }

        public virtual Entity Update(Entity entity)
        {
            DbSet.Update(entity);
            return entity;
        }

        public decimal Sum(Func<Entity, decimal> selector)
        {
            return DbSet.Sum(selector);
        }
    }
}
