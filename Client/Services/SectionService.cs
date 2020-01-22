using System.Collections.Generic;
using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Enumerations;
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

        public async Task<IResult<bool>> Delete(long sectionId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}");
            if (result.IsSuccessStatusCode)
            {
                return new Result<bool>
                {
                    ResultObject = true,
                    Errors       = null
                };
            }

            return new Result<bool>
            {
                ResultObject = false,
                Errors       = new List<IError>
                {
                    new Error
                    {
                        ErrorType = ErrorType.Error,
                        Key       = "DeleteError",
                        Message   = "Failed to delete section."
                    }
                }
            };
        }

        public async Task<IResult<bool>> Edit(SectionDto section)
        {
            return await _client.PostJsonAsync<Result<bool>>($"/api/sections/{section.Id}", section);
        }
    }
}
