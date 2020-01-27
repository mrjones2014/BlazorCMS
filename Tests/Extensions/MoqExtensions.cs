using System.Collections.Generic;
using AndcultureCode.CSharp.Core.Enumerations;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using Moq.Language.Flow;

namespace BlazorCMS.Tests.Extensions
{
    public static class MoqExtensions
    {
        public static IReturnsResult<T> ReturnsGivenResult<T, TResult>(
            this ISetup<T, IResult<TResult>> setup,
            TResult                          resultObject = default(TResult)
        ) where T : class
        {
            return setup
                .Returns(new Result<TResult> {
                    Errors       = new List<IError>(),
                    ResultObject = resultObject
                });
        }

        public static IReturnsResult<T> ReturnsBasicErrorResult<T, TResult>(
            this ISetup<T, IResult<TResult>> setup,
            TResult                          resultObject = default(TResult)
        ) where T : class
        {
            return setup
                .Returns(new Result<TResult> {
                    Errors = new List<IError> {
                        new Error()
                        {
                            ErrorType = ErrorType.Error,
                            Key       = "BasicError",
                            Message   = "Basic error."
                        }
                    },
                    ResultObject = resultObject
                });
        }
    }
}
