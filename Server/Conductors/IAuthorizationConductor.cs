using System;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core.Interfaces;

namespace BlazorCMS.Server.Conductors
{
    public interface IAuthorizationConductor<T>
    {
        /// <summary>
        /// Check if the user with ID userId is authorized to access entity with ID entityId
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IResult<bool> IsAuthorized(long entityId, long userId);

        /// <summary>
        /// Build an expression to filter by user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Expression<Func<T, bool>> FilterByUserId(long userId);
    }
}
