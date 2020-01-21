using System;
using System.Collections.Generic;
using BlazorCMS.Shared.Dtos;
using BlazorState;

namespace BlazorCMS.Client.State
{
    public class ClientState : State<ClientState>
    {
        public List<SectionDto> Sections          { get; set; }
        public List<ArticleDto> Articles          { get; set; }
        public long             ExpandedSectionId { get; set; }

        public override void Initialize()
        {
            Sections          = new List<SectionDto>();
            Articles          = new List<ArticleDto>();
            ExpandedSectionId = -1;
        }
    }
}
