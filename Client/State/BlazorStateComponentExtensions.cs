using System;
using BlazorState;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.State
{
    public static class BlazorStateComponentExtensions
    {
        /// <summary>
        /// Returns true if unauthorized (i.e. if redirect occurred)
        /// </summary>
        /// <param name="component"></param>
        /// <param name="navigationManager"></param>
        /// <returns></returns>
        public static bool GoToLoginIfUnauthorized(this BlazorStateComponent component, NavigationManager navigationManager)
        {
            if (component.Store.GetState<ClientState>().CurrentUser == null)
            {
                navigationManager.NavigateTo("/");
                return true;
            }

            return false;
        }
    }
}
