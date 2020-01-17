using System.Collections.Generic;
using BlazorCMS.Shared.Models;

namespace BlazorCMS.Conductors.Interfaces
{
    public interface IResult<T> where T : Entity
    {
        int                 ErrorCount   { get; }
        IEnumerable<string> Errors       { get; set; }
        bool                HasErrors    { get; }
        T                   ResultObject { get; set; }
    }
}
