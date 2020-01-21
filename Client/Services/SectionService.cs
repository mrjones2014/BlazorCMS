using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.Services
{
    public class SectionService : Service
    {
        public SectionService(NavigationManager navigationManager) : base(navigationManager)
        {
        }

        public async Task<IResult<SectionDto[]>> Index()
        {
            return await _client.GetJsonAsync<Result<SectionDto[]>>("/api/sections");
        }
    }
}
