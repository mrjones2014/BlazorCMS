using System;
using System.Net.Http;
using System.Threading.Tasks;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Core.Models;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.Services
{
    public class SectionService
    {
        private static HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5000")
        };

        public static async Task<IResult<SectionDto[]>> Index()
        {
            return await client.GetJsonAsync<Result<SectionDto[]>>("/api/sections");
        }
    }
}
