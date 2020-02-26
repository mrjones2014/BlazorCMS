using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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

        public bool    SidebarLoadingArticles { get; set; }
        public UserDto CurrentUser            { get; set; }

        #endregion Properties

        #region Public Methods

        public override void Initialize()
        {
            Sections               = ImmutableList<SectionDto>.Empty;
            Articles               = ImmutableList<ArticleDto>.Empty;
            SidebarLoadingArticles = false;
            CurrentUser            = null;
        }

        public void RegisterNavMenuComponent(NavMenu menu)
        {
            _sidebarReference = menu;
        }

        public void ShowSidebarCreateSectionForm()
        {
            _sidebarReference.OnSectionCreate();
        }

        #endregion Public Methods
    }
}
