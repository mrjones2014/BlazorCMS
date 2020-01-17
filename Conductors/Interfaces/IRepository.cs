using System;
using System.Linq.Expressions;
using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        IResult<T> Create(T                          item);
        IResult<T> Delete(long                       id);
        IResult<T> FindAll(Expression<Func<T, bool>> filterExpression);
        IResult<T> FindById(long                     id);
        IResult<T> Update(T                          item);
    }
}
