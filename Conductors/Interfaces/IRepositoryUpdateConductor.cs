using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IRepositoryUpdateConductor<T> where T : Entity
    {
        IResult<T> Update(T item);
    }
}
