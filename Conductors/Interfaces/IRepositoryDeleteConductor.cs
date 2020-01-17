using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IRepositoryDeleteConductor<T> where T : Entity
    {
        IResult<T> Delete(long id);
    }
}
