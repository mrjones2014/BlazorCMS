using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Enumerations;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using AutoMapper;
using BlazorCMS.Server.Data.Models;
using BlazorCMS.Shared.Dtos;
using BlazorCMS.Shared.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/users")]
    public class UsersController : BaseController
    {
        #region Properties

        private readonly SignInManager<User> _signInManager;
        private readonly IMapper             _mapper;

        #endregion Properties

        #region Constructor

        public UsersController(
            UserManager<User>   userManager,
            SignInManager<User> signInManager,
            IMapper             mapper) : base(userManager)
        {
            _signInManager = signInManager;
            _mapper        = mapper;
        }

        #endregion Constructor

        #region Register

        [HttpPost]
        [AllowAnonymous]
        [Route("/register")]
        public IActionResult Register([FromBody] RegisterForm form)
        {
            if (form.Password != form.PasswordConfirm)
            {
                return BadRequest<UserDto>(null, new List<IError>
                {
                    new Error
                    {
                        ErrorType = ErrorType.ValidationError,
                        Key       = "PasswordsDontMatch",
                        Message   = "Passwords do not match."
                    }
                });
            }

            var user   = new User {UserName = form.UserName};
            var result = _userManager.CreateAsync(user, form.Password).Result;
            if (result.Succeeded)
            {
                result = _userManager.AddToRoleAsync(user, "User").Result;
                return Ok(_mapper.Map<UserDto>(user));
            }

            var errors = result.Errors.Select(
                e => new Error
                {
                    ErrorType = ErrorType.Error,
                    Key       = e.Code,
                    Message   = e.Description
                }
            );
            return InternalError<UserDto>(null, errors);
        }

        #endregion Register

        #region Login

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public IActionResult Login([FromBody] LoginForm form)
        {
            if (CurrentUser != null)
            {
                return Ok(true, null);
            }

            var loginResult = _signInManager.PasswordSignInAsync(
                userName:         form.UserName,
                password:         form.Password,
                isPersistent:     true,
                lockoutOnFailure: false
            ).Result;

            if (loginResult.Succeeded)
            {
                return Ok(loginResult.Succeeded, null);
            }

            return BadRequest<bool>(false, new List<IError>
            {
                new Error
                {
                    ErrorType = ErrorType.Error,
                    Key       = "LoginFailed",
                    Message   = "Invalid Credentials"
                }
            });
        }

        #endregion Login

        #region CurrentUser

        [HttpGet]
        public IActionResult GetCurrentUSer()
        {
            if (CurrentUser == null)
            {
                return Ok<UserDto>(null, null);
            }

            return Ok(_mapper.Map<UserDto>(CurrentUser), null);
        }

        #endregion CurrentUser
    }
}
