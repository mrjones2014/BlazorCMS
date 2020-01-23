using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using BlazorCMS.Server.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.Server.Controllers
{
    public class BaseController : Controller
    {
        #region Properties

        protected readonly UserManager<User> _userManager;
        public virtual ClaimsPrincipal ClaimsPrincipal { get; set; }

        private User _currentUser;
        protected User CurrentUser
        {
            get
            {
                if (_currentUser != null)
                {
                    return _currentUser;
                }
                try
                {
                    var userName = User.Identity?.Name;
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        return _currentUser;
                    }
                    _currentUser = _userManager.FindByNameAsync(userName).Result;
                    return _currentUser;
                }
                catch (Exception ex)
                {
                    return _currentUser;
                }
            }
        }

        #endregion Properties

        #region Constructor

        public BaseController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        #endregion Constructor

        #region Public Utility Methods

        /// <summary>
        /// Create a result object given the value and errors list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IResult<T> CreateResult<T>(T value, IEnumerable<IError> errors)
        {
            var result = new Result<T>()
            {
                Errors       = errors?.ToList(),
                ResultObject = value
            };
            return result;
        }

        public OkObjectResult Ok<T>(T value, IEnumerable<IError> errors)
        {
            return base.Ok(CreateResult(value, errors));
        }

        public NotFoundObjectResult NotFound<T>(T value, IEnumerable<IError> errors)
        {
            return base.NotFound(CreateResult(value, errors));
        }

        protected BadRequestObjectResult BadRequest<T>(T value, IEnumerable<IError> errors)
        {
            return base.BadRequest(CreateResult(value, errors));
        }

        public ObjectResult InternalError<T>(T value, IEnumerable<IError> errors)
        {
            return StatusCode(500, CreateResult(value, errors));
        }

        #endregion Public Utility Methods
    }
}
