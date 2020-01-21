using System.Threading.Tasks;
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
            return await _client.GetJsonAsync<Result<ArticleDto[]>>($"/api/{sectionId}/articles");
        }

        public async Task<IResult<bool>> Post(ArticleDto article)
        {
            return await _client.PostJsonAsync<Result<bool>>($"/api/{article.SectionId}/articles/{article.Id}", article);
        }

        public async Task<IResult<ArticleDto>> Put(ArticleDto article)
        {
            return await _client.PutJsonAsync<Result<ArticleDto>>($"/api/{article.SectionId}/articles", article);
        }
    }
}
