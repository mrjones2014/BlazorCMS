using System.Collections.Generic;
using AndcultureCode.CSharp.Core.Models.Entities;

namespace BlazorCMS.Server.Data.Models
{
    public class Section : Entity
    {
        #region Properties

        public string Name { get; set; }

        #endregion Properties

        #region Navigation Properties

        public IEnumerable<Article> Articles { get; set; }

        #endregion Navigation Properties
    }
}
