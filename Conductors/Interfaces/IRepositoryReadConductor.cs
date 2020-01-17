using System;
using System.Linq.Expressions;
using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IRepositoryReadConductor<T> where T : Entity
    {
        IResult<T> FindAll(Expression<Func<T, bool>> filterExpression);
        IResult<T> FindById(long                     id);
    }
}
