using System.ComponentModel.DataAnnotations;
using AndcultureCode.CSharp.Core.Models.Entities;

namespace BlazorCMS.Server.Data.Models
{
    public class Article : Entity
    {
        #region Properties

        [Required(AllowEmptyStrings = false)]
        public string Title     { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Body      { get; set; }
        [Required]
        public long   SectionId { get; set; }

        #endregion Properties

        #region Navigation Properties

        public Section Section { get; set; }

        #endregion Navigation Properties
    }
}
