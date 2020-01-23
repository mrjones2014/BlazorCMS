using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AndcultureCode.CSharp.Core.Models.Entities;

namespace BlazorCMS.Server.Data.Models
{
    public class Section : Entity
    {
        #region Properties

        [Required(AllowEmptyStrings = false)]
        public string Name   { get; set; }
        public long   UserId { get; set; }

        #endregion Properties

        #region Navigation Properties

        public IEnumerable<Article> Articles { get; set; }
        public User                 User     { get; set; }

        #endregion Navigation Properties
    }
}
