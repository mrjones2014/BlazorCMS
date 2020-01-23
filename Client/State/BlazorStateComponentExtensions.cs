using System;
using BlazorState;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.State
{
    public static class BlazorStateComponentExtensions
    {
        public static void GoToLoginIfUnauthorized(this BlazorStateComponent component, NavigationManager navigationManager)
        {
            if (component.Store.GetState<ClientState>().CurrentUser == null)
            {
                navigationManager.NavigateTo("/");
            }
        }
    }
}
