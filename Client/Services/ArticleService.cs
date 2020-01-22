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

        public async Task<IResult<ArticleDto[]>> Index(long sectionId)
        {
            return await _client.GetJsonAsync<Result<ArticleDto[]>>($"/api/sections/{sectionId}/articles");
        }

        public async Task<IResult<bool>> Post(ArticleDto article)
        {
            return await _client.PostJsonAsync<Result<bool>>($"/api/sections/{article.SectionId}/articles/{article.Id}", article);
        }

        public async Task<IResult<ArticleDto>> Put(ArticleDto article)
        {
            return await _client.PutJsonAsync<Result<ArticleDto>>($"/api/sections/{article.SectionId}/articles", article);
        }
        public async Task<IResult<bool>> Delete(long sectionId, long articleId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}/articles/{articleId}");
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
                Errors = new List<IError>
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
