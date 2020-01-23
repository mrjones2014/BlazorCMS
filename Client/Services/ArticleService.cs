using System.Collections.Generic;
using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Enumerations;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.Services
{
    public class ArticleService : Service
    {
        public ArticleService(NavigationManager manager) : base(manager)
        {
        }

        public async Task<ResultDto<ArticleDto[]>> Index(long sectionId)
        {
            return await _client.GetJsonAsync<ResultDto<ArticleDto[]>>($"/api/sections/{sectionId}/articles");
        }

        public async Task<ResultDto<bool>> Post(ArticleDto article)
        {
            return await _client.PostJsonAsync<ResultDto<bool>>($"/api/sections/{article.SectionId}/articles/{article.Id}", article);
        }

        public async Task<ResultDto<ArticleDto>> Put(ArticleDto article)
        {
            return await _client.PutJsonAsync<ResultDto<ArticleDto>>($"/api/sections/{article.SectionId}/articles", article);
        }
        public async Task<ResultDto<bool>> Delete(long sectionId, long articleId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}/articles/{articleId}");
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
                Errors = new List<Error>
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
    }
}
