using System.Collections.Generic;
using System.Linq;
using BlazorCMS.Shared.Dtos;

namespace BlazorCMS.Client.Shared
{
    public class ClientState
    {
        public List<SectionDto> Sections { get; set; }
        public List<ArticleDto> Articles { get; set; }

        public ClientState()
        {
            Sections = new List<SectionDto>();
            Articles = new List<ArticleDto>();
        }

        public List<ArticleDto> GetArticlesBySectionId(long sectionId)
        {
            return Articles?.Where(e => e.SectionId == sectionId)?.ToList();
        }
    }
}
