using AndcultureCode.CSharp.Core.Models.Entities;

namespace BlazorCMS.Shared.Models
{
    public class Article : Entity
    {
        #region Properties

        public string Title     { get; set; }
        public string Body      { get; set; }
        public long   SectionId { get; set; }

        #endregion Properties

        #region Navigation Properties

        public Section Section { get; set; }

        #endregion Navigation Properties
    }
}
