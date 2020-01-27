using System;
using System.Linq.Expressions;
using AndcultureCode.CSharp.Core;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using BlazorCMS.Server.Data.Models;

namespace BlazorCMS.Server.Conductors
{
    public class SectionAuthorizationConductor : IAuthorizationConductor<Section>
    {
        #region Properties

        private readonly IRepositoryReadConductor<Section> _readConductor;

        #endregion Properties

        #region Constructor

        public SectionAuthorizationConductor(IRepositoryReadConductor<Section>   readConductor)
        {
            _readConductor = readConductor;
        }

        #endregion Constructor

        #region IAuthorizationConductor Implementation

        public IResult<bool> IsAuthorized(long entityId, long userId) => Do<bool>.Try(result =>
        {
            var getResult = _readConductor.FindById(entityId);
            if (!getResult.HasErrorsOrResultIsNull())
            {
                return getResult.ResultObject.UserId == userId;
            }

            result.AddErrors(getResult.Errors);
            return false;

        }).Result;

        public Expression<Func<Section, bool>> FilterByUserId(long userId) => e => e.UserId == userId;

        #endregion IAuthorizationConductor Implementation
    }
}
