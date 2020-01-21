using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Interfaces.Data;
using AndcultureCode.CSharp.Core.Interfaces.Entity;
using AndcultureCode.CSharp.Core.Models;
using AndcultureCode.CSharp.Core.Models.Entities;
using BlazorCMS.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCMS.Server.Data
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        #region Properties

        private BlazorCmsContext Context { get; set; }

        #endregion Properties

        #region Constructor

        public Repository(BlazorCmsContext context)
        {
            Context = context;
        }

        #endregion Constructor


        #region IRepository Impl

        public IResult<List<T>> BulkCreate(IEnumerable<T> items, long? createdById = null)
        {
            throw new NotImplementedException();
        }

        public IResult<List<T>> BulkCreateDistinct<TKey>(IEnumerable<T> items, Func<T, TKey> property, long? createdById = null)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> BulkDelete(IEnumerable<T> items, long? deletedById = null, bool soft = true)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> BulkUpdate(IEnumerable<T> entities, long? updatedBy = null)
        {
            throw new NotImplementedException();
        }

        public IResult<T> Create(T item, long? createdById = null)
        {
            var result = new Result<T>();
            try
            {
                Context.Add(item);
                Context.ChangeTracker.DetectChanges();
                Context.SaveChanges();
                result.ResultObject = item;
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), $"{ex.Message} -- {ex.InnerException?.Message}");
            }

            return result;
        }

        public IResult<List<T>> Create(IEnumerable<T> items, long? createdById = null)
        {
            var result = new Result<List<T>>();
            try
            {
                var itemList = items.ToList();
                Context.AddRange(itemList);
                Context.ChangeTracker.DetectChanges();
                Context.SaveChanges();
                result.ResultObject = itemList;
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), $"{ex.Message} -- {ex.InnerException?.Message}");
            }

            return result;
        }

        public IResult<List<T>> CreateDistinct<TKey>(IEnumerable<T> items, Func<T, TKey> property, long? createdById = null)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> Delete(long id, long? deletedById = null, bool soft = true)
        {
            IResult<T> findResult;
            if (soft == false)
            {
                findResult = FindById(id, true);
            }
            else
            {
                findResult = FindById(id);
            }
            if (findResult.HasErrors)
            {
                return new Result<bool>
                {
                    Errors       = findResult.Errors,
                    ResultObject = false
                };
            }
            return Delete(findResult.ResultObject, deletedById, soft);
        }

        public IResult<bool> Delete(T entity, long? deletedById = null, bool soft = true)
        {
            var result = new Result<bool> { ResultObject = false };

            try
            {
                if (entity == null)
                {
                    result.AddError("Delete", $"{entity.GetType()} does not exist.");
                    return result;
                }

                if (soft && !(entity is IDeletable))
                {
                    result.AddError("Delete", "In order to perform a soft delete, the object must implement the IDeletable interface.");
                    return result;
                }

                if (soft)
                {
                    if (deletedById.HasValue)
                    {
                        ((IDeletable)entity).DeletedById = deletedById;
                    }
                    ((IDeletable)entity).DeletedOn = DateTimeOffset.UtcNow;
                }
                else
                {
                    Context.Delete(entity);
                }

                Context.SaveChanges();
                result.ResultObject = true;
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<IQueryable<T>> FindAll(
            Expression<Func<T, bool>>                 filter             = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy            = null,
            string                                    includeProperties  = null,
            int?                                      skip               = null,
            int?                                      take               = null,
            bool?                                     ignoreQueryFilters = false,
            bool                                      asNoTracking       = false
        )
        {
            var result = new Result<IQueryable<T>>();

            try
            {
                result.ResultObject = GetQueryable(filter, orderBy, includeProperties, skip, take, ignoreQueryFilters, asNoTracking);
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<IList<T>> FindAllCommitted(
            Expression<Func<T, bool>>                 filter             = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy            = null,
            string                                    includeProperties  = null,
            int?                                      skip               = null,
            int?                                      take               = null,
            bool?                                     ignoreQueryFilters = false
        )
        {
            throw new NotImplementedException();
        }

        public IResult<T> FindById(long id, bool? ignoreQueryFilters = false)
        {
            var           result = new Result<T>();
            IQueryable<T> query  = Context.Set<T>();

            try
            {
                if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
                {
                    query = query.IgnoreQueryFilters();
                }

                result.ResultObject = query.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<T> FindById(long id, params Expression<Func<T, object>>[] includeProperties)
        {
            var result = new Result<T>();
            IQueryable<T> query = Context.Set<T>();

            try
            {
                foreach (var property in includeProperties)
                {
                    query = query.Include(property);
                }

                result.ResultObject = query.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<T> FindById(long id, bool? ignoreQueryFilters, params Expression<Func<T, object>>[] includeProperties)
        {
            var           result = new Result<T>();
            IQueryable<T> query  = Context.Set<T>();

            try
            {
                foreach (var property in includeProperties)
                {
                    query = query.Include(property);
                }

                if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
                {
                    query = query.IgnoreQueryFilters();
                }

                result.ResultObject = query.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<T> FindById(long id, params string[] includeProperties)
        {
            var           result = new Result<T>();
            IQueryable<T> query  = Context.Set<T>();

            try
            {
                foreach (var property in includeProperties)
                {
                    if (!string.IsNullOrEmpty(property))
                    {
                        query = query.Include(property);
                    }
                }

                result.ResultObject = query.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<bool> Restore(T o)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> Restore(long id)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> Update(T item, long? updatedBy = null)
        {
            var result = new Result<bool> { ResultObject = false };

            try
            {
                Context.Update(item);
                Context.SaveChanges();

                result.ResultObject = true;
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public IResult<bool> Update(IEnumerable<T> entities, long? updatedBy = null)
        {
            var result = new Result<bool> { ResultObject = false };

            try
            {
                Context.UpdateRange(entities);
                Context.SaveChanges();

                result.ResultObject = true;
            }
            catch (Exception ex)
            {
                result.AddError(ex.GetType().ToString(), ex.Message);
            }

            return result;
        }

        public int? CommandTimeout
        {
            get => Context?.Database.GetCommandTimeout();
            set => Context?.Database.SetCommandTimeout(value);
        }

        #endregion IRepository Impl

        #region Protected Methods

        protected virtual IQueryable<T> GetQueryable(
            Expression<Func<T, bool>>                 filter             = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy            = null,
            string                                    includeProperties  = null,
            int?                                      skip               = null,
            int?                                      take               = null,
            bool?                                     ignoreQueryFilters = false,
            bool                                      asNoTracking       = false
        )
        {
            includeProperties = includeProperties ?? string.Empty;
            IQueryable<T> query = Context.Set<T>();

            if (ignoreQueryFilters.HasValue && ignoreQueryFilters.Value)
            {
                query = query.IgnoreQueryFilters();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            foreach (var includeProperty in includeProperties.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }

        #endregion Protected Methods
    }
}
