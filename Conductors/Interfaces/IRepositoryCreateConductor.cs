using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IRepositoryCreateConductor<T> where T : Entity
    {
        IResult<T> Create(T item);
    }
}
