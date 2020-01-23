using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BlazorCMS.Server.Data.Models
{
    public class User : IdentityUser<long>
    {
        #region Navigation Properties

        public List<Section> Sections { get; set; }

        #endregion Navigation Properties
    }
}
