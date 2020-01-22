using BlazorState;

namespace BlazorCMS.Client.State
{
    public abstract class UpdatableComponent : BlazorStateComponent
    {
        public void Update()
        {
            this.StateHasChanged();
        }
    }
}
