using System;
using System.Collections.Generic;
using BlazorCMS.Shared.Dtos;
using BlazorState;
using Microsoft.AspNetCore.Components;

namespace BlazorCMS.Client.State
{
    public class ClientState : State<ClientState>
    {
        #region Properties

        private List<SectionDto> _sections;
        public List<SectionDto> Sections
        {
            get => _sections;
            set
            {
                _sections = value;
                _subscribers.ForEach(c => c.Update());
            }
        }

        private List<ArticleDto> _articles;
        public List<ArticleDto> Articles
        {
            get => _articles;
            set
            {
                _articles = value;
                _subscribers.ForEach(c => c.Update());
            }
        }

        private long _expandedSectionId;
        public long ExpandedSectionId
        {
            get => _expandedSectionId;
            set
            {
                _expandedSectionId = value;
                _subscribers.ForEach(c => c.Update());
            }
        }

        private bool _sidebarLoadingArticles;
        public bool SidebarLoadingArticles
        {
            get => _sidebarLoadingArticles;
            set
            {
                _sidebarLoadingArticles = value;
                _subscribers.ForEach(c => c.Update());
            }
        }

        private List<UpdatableComponent> _subscribers = new List<UpdatableComponent>();

        #endregion Properties

        #region Public Methods

        public override void Initialize()
        {
            Sections               = new List<SectionDto>();
            Articles               = new List<ArticleDto>();
            ExpandedSectionId      = -1;
            SidebarLoadingArticles = false;
        }

        public void RegisterSubscriber(UpdatableComponent component)
        {
            _subscribers.Add(component);
        }

        #endregion Public Methods
    }
}
