using System;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using BlazorCMS.Server.Data.Models;

namespace BlazorCMS.Server.Conductors
{
    public class ArticleAuthorizationConductor : IAuthorizationConductor<Article>
    {
        #region Properties

        private readonly IRepositoryReadConductor<Article> _readConductor;

        #endregion Properties

        #region Constructor

        public ArticleAuthorizationConductor(IRepositoryReadConductor<Article> readConductor)
        {
            _readConductor = readConductor;
        }

        #endregion Constructor

        #region IAuthorizationConductor Implementation

        public IResult<bool> IsAuthorized(long entityId, long userId) => Do<bool>.Try(result =>
        {
            var getResult = _readConductor.FindById(id: entityId, includeProperties: nameof(Article.Section));
            if (!getResult.HasErrorsOrResultIsNull())
            {
                return getResult.ResultObject.Section.UserId == userId;
            }

            result.AddErrors(getResult.Errors);
            return false;
        }).Result;

        public Expression<Func<Article, bool>> FilterByUserId(long userId) => e => e.Section.UserId == userId;

        #endregion IAuthorizationConductor Implementation
    }
}
