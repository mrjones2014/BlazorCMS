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

        public async Task<ResultDto<SectionDto[]>> Index()
        {
            return await _client.GetJsonAsync<ResultDto<SectionDto[]>>("/api/sections");
        }

        public async Task<ResultDto<bool>> Delete(long sectionId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}");
            if (result.IsSuccessStatusCode)
            {
                return new ResultDto<bool>
                {
                    ResultObject = true,
                    Errors       = null
                };
            }

            return new ResultDto<bool>
            {
                ResultObject = false,
                Errors       = new List<Error>
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

        public async Task<ResultDto<bool>> Edit(SectionDto section)
        {
            return await _client.PostJsonAsync<ResultDto<bool>>($"/api/sections/{section.Id}", section);
        }

        public async Task<ResultDto<SectionDto>> Create(SectionDto section)
        {
            return await _client.PutJsonAsync<ResultDto<SectionDto>>("/api/sections", section);
        }
    }
}
