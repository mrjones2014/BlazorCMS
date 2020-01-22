using System.Collections.Immutable;
using System.Linq;
using BlazorCMS.Client.Shared;
using BlazorCMS.Shared.Dtos;
using BlazorState;

namespace BlazorCMS.Client.State
{
    public class ClientState : State<ClientState>
    {
        #region Properties

        private NavMenu _sidebarReference = null;

        private ImmutableList<ArticleDto> _articles;
        public ImmutableList<ArticleDto> Articles
        {
            get => _articles;
            set => _articles = value.OrderBy(e => e.Id).ToImmutableList();
        }

        private ImmutableList<SectionDto> _sections;
        public ImmutableList<SectionDto> Sections
        {
            get => _sections;
            set => _sections = value.OrderBy(e => e.Id).ToImmutableList();
        }

        public long             ExpandedSectionId      { get; set; }
        public bool             SidebarLoadingArticles { get; set; }

        #endregion Properties

        #region Public Methods

        public override void Initialize()
        {
            Sections               = ImmutableList<SectionDto>.Empty;
            Articles               = ImmutableList<ArticleDto>.Empty;
            ExpandedSectionId      = -1;
            SidebarLoadingArticles = false;
        }

        public void RegisterNavMenuComponent(NavMenu menu)
        {
            _sidebarReference = menu;
        }

        public void UpdateNavMenu()
        {
            _sidebarReference.Update();
        }

        #endregion Public Methods
    }
}
