using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.Services
{
    public abstract class Service
    {
        protected HttpClient _client { get; set; }

        public Service(NavigationManager manager)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(manager.BaseUri)
            };
        }
    }
}
