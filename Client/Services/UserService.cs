using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using BlazorCMS.Shared.Dtos;
using BlazorCMS.Shared.Forms;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.Services
{
    public class UserService : Service
    {
        public UserService(NavigationManager manager) : base(manager)
        {
        }

        public async Task<ResultDto<UserDto>> Register(RegisterForm form)
        {
            return await _client.PostJsonAsync<ResultDto<UserDto>>("/api/users/register", form);
        }

        public async Task<ResultDto<bool>> Login(LoginForm form)
        {
            return await _client.PostJsonAsync<ResultDto<bool>>("/api/users/login", form);
        }

        public async Task<ResultDto<UserDto>> GetCurrentUser()
        {
            return await _client.GetJsonAsync<ResultDto<UserDto>>("/api/users/current");
        }
    }
}
