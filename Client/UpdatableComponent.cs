using BlazorState;

namespace BlazorCMS.Client
{
    public abstract class UpdatableComponent : BlazorStateComponent
    {
        public void Update()
        {
            this.StateHasChanged();
        }
    }
}
