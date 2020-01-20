using System;
using System.Collections.Generic;
using System.Linq;
using BlazorCMS.Shared.Dtos;
using BlazorState;

namespace BlazorCMS.Client.State
{
    public class ClientState : IState
    {
        public List<SectionDto> Sections { get; set; }
        public List<ArticleDto> Articles { get; set; }

        public List<ArticleDto> GetArticlesBySectionId(long sectionId)
        {
            return Articles?.Where(e => e.SectionId == sectionId)?.ToList();
        }

        public void Initialize()
        {
            Sections = new List<SectionDto>();
            Articles = new List<ArticleDto>();
        }

        public Guid Guid { get; }
    }
}
