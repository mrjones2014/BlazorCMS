# BlazorCMS
A (very) simple CMS to evaluate Microsoft's Blazor WASM framework.

In this tutorial, we'll take a look at Microsoft's Blazor framework, with the WebAssembly option.
We'll build a (very) simple CMS, with a .NET Core hosted backend.

All of the code related to this article can be found [here](https://github.com/andCulture/BlazorCMS).

To start, you will need to install the [dotnet 3.1.0-preview4 CLI](https://dotnet.microsoft.com/download/dotnet-core/3.1).

Start by installing the Blazor project templates into the dotnet CLI.

`dotnet new -i Microsoft.AspNetCore.Blazor.Templates::3.1.0-preview4.19579.2`

Now that we have the project templates installed, let's create a new Blazor WASM Dotnet Core hosted project.

`dotnet new blazorwasm --hosted -o BlazorCMS`

At this point, you should be able to `cd` into your `Server` directory and run the app by executing `dotnet run`.
If you get an error like `ILLink failed with exited code 1`, you can add the property `<BlazorLinkOnBuild>false</BlazorLinkOnBuild>` to the `<PropertyGroup>`
of your `Client.csproj` file as a workaround. Remember, Blazor is still in preview, so you may come across issues like this.

Let's add some data. For this, we'll use Entity Framework Core with a Sqlite database. Add the package to your `Server` project.

`dotnet add package Microsoft.EntityFrameworkCore.Sqlite`;

Let's add some models. In this project, we'll have `Section`s and `Article`s. Add these models in the `Server` project under a `Data/Models` directory.
We'll also use an andculture open-source project, AndcultureCode.CSharp.Conductors, in this project:

`dotnet add package AndcultureCode.CSharp.Conductors` 

`Section.cs`
```c#
namespace BlazorCMS.Server.Data.Models
{
    public class Section : Entity
    {
        #region Properties

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        #endregion Properties

        #region Navigation Properties

        public IEnumerable<Article> Articles { get; set; }

        #endregion Navigation Properties
    }
}
```

`Article.cs`
```c#
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
```

Now, let's add a `DatabaseContext`. Add a `BlazorCmsContext` under `Server/Data/`.

```c#
namespace BlazorCMS.Server.Data
{
    public class BlazorCmsContext : DbContext
    {
        #region Constructor

        public BlazorCmsContext(DbContextOptions<BlazorCmsContext> options) : base(options)
        {
        }

        #endregion Constructor
        
        #region Entities

        public DbSet<Article> Articles { get; set; }
        public DbSet<Section> Sections { get; set; }

        #endregion Entities
    }
}
```

Now add your `BlazorCmsContext` to dependency injection and configure the data source. In `Startup.cs` in the `ConfigureServices` method,
add the following line:

`services.AddDbContext<BlazorCmsContext>(options => options.UseSqlite("Data Source=BlazorCMS.db"));`;

Let's install the dotnet Entity Framework CLI: `dotnet tool install --global dotnet-ef`.
We will also need the `Microsoft.EntityFrameworkCore.Design` package for the CLI to work in this project.

`dotnet add package Microsoft.EntityFrameworkCore.Design`

Now we can create our initial database migration. Note that you should **always** double-check your migrations to avoid losing data.
This should be a simple migration that creates your `Articles` and `Sections` tables, and creates a foreign key relationship from `Articles` to `Sections`.

Let's now configure our application to automatically run migrations on startup. Add the following snippet to the `Startup.Configure` method:

```c#
using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    using (var dbContext = serviceScope.ServiceProvider.GetService<BlazorCmsContext>())
    {
        dbContext.Database.Migrate();
    }
}
```

You'll also need to implement a concrete implementation of `IRepository<T>`, and register it in dependency injection by adding the following lines
to `Startup.cs` in the `ConfigureServices` method:

```c#
services.AddScoped<IRepository<Section>, Repository<Section>>();
services.AddScoped<IRepository<Article>, Repository<Article>>();
```

Alright, now let's get some conductors for each of our entities into dependency injection. In `Startup.cs`, in the `ConfigureServices` method,
add the following lines:

```c#


services.AddScoped<IRepositoryCreateConductor<Section>, RepositoryCreateConductor<Section>>();
services.AddScoped<IRepositoryReadConductor<Section>,   RepositoryReadConductor<Section>>();
services.AddScoped<IRepositoryUpdateConductor<Section>, RepositoryUpdateConductor<Section>>();
services.AddScoped<IRepositoryDeleteConductor<Section>, RepositoryDeleteConductor<Section>>();

services.AddScoped<IRepositoryCreateConductor<Article>, RepositoryCreateConductor<Article>>();
services.AddScoped<IRepositoryReadConductor<Article>,   RepositoryReadConductor<Article>>();
services.AddScoped<IRepositoryUpdateConductor<Article>, RepositoryUpdateConductor<Article>>();
services.AddScoped<IRepositoryDeleteConductor<Article>, RepositoryDeleteConductor<Article>>();
```

For our controllers, we'll need DTOs (Data Transfer Objects) to send our models to the client. Add these in the `Shared` project
so that they can be used directly on the client as well. Add your DTOs under a `Dtos` directory.

Now let's build a controller for each of our entities. Add your controllers in the `Server` project, under the `Controllers` directory.

`EntityDto.cs`
```c#
namespace BlazorCMS.Shared.Dtos
{
    public class EntityDto
    {
        public long Id { get; set; }
    }
}
```

`SectionDto.cs`
```c#
namespace BlazorCMS.Shared.Dtos
{
    public class SectionDto : EntityDto
    {
        public string Name { get; set; }
    }
}
```

`ArticleDto.cs`
```c#
namespace BlazorCMS.Shared.Dtos
{
    public class ArticleDto : EntityDto
    {
        public string Title     { get; set; }
        public string Body      { get; set; }
        public long   SectionId { get; set; }
    }
}
```

We'll use the AutoMapper package to automatically map our models to our DTOs. Add this package to your `Server` project.

`dotnet add package AutoMapper`

Now we can configure our `AutoMapper` in the `ConfigureServices` method of `Startup.cs`:

```c#
var autoMapperConfig = new MapperConfiguration(config =>
{
    config.CreateMap<Section, SectionDto>();
    config.CreateMap<Article, ArticleDto>();
});
IMapper mapper = autoMapperConfig.CreateMapper();
services.AddSingleton<IMapper>(mapper);
```

Now that `AutoMapper` is set up, we can use it in the controllers we'll add under `Server/Controllers`:

`BaseController.cs`
```c#
namespace BlazorCMS.Server.Controllers
{
    public class BaseController : Controller
    {
        #region Public Utility Methods

        /// <summary>
        /// Create a result object given the value and errors list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IResult<T> CreateResult<T>(T value, IEnumerable<IError> errors)
        {
            var result = new Result<T>()
            {
                Errors       = errors?.ToList(),
                ResultObject = value
            };
            return result;
        }

        public OkObjectResult Ok<T>(T value, IEnumerable<IError> errors)
        {
            return base.Ok(CreateResult(value, errors));
        }

        public NotFoundObjectResult NotFound<T>(T value, IEnumerable<IError> errors)
        {
            return base.NotFound(CreateResult(value, errors));
        }

        protected BadRequestObjectResult BadRequest<T>(T value, IEnumerable<IError> errors)
        {
            return base.BadRequest(CreateResult(value, errors));
        }

        public ObjectResult InternalError<T>(T value, IEnumerable<IError> errors)
        {
            return StatusCode(500, CreateResult(value, errors));
        }

        #endregion Public Utility Methods
    }
}
```

`SectionsController.cs`
```c#
namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/sections")]
    public class SectionsController : BaseController
    {
        #region Properties

        private readonly IRepositoryCreateConductor<Section> _createConductor;
        private readonly IRepositoryDeleteConductor<Section> _deleteConductor;
        private readonly IRepositoryReadConductor<Section>   _readConductor;
        private readonly IRepositoryUpdateConductor<Section> _updateConductor;
        private readonly IMapper                             _mapper;

        #endregion Properties

        #region Constructor

        public SectionsController(
            IRepositoryCreateConductor<Section> createConductor,
            IRepositoryDeleteConductor<Section> deleteConductor,
            IRepositoryReadConductor<Section>   readConductor,
            IRepositoryUpdateConductor<Section> updateConductor,
            IMapper                             mapper
        )
        {
            _createConductor = createConductor;
            _deleteConductor = deleteConductor;
            _readConductor   = readConductor;
            _updateConductor = updateConductor;
            _mapper          = mapper;
        }

        #endregion Constructor

        #region PUT

        [HttpPut]
        public IActionResult Put([FromBody] SectionDto section)
        {
            var newSection = new Section
            {
                Name = section.Name
            };
            var createResult = _createConductor.Create(newSection);
            if (createResult.HasErrors)
            {
                return InternalError<SectionDto>(null, createResult.Errors);
            }

            return Ok(_mapper.Map<SectionDto>(createResult.ResultObject), null);
        }

        #endregion PUT

        #region POST

        [HttpPost("{sectionId:long}")]
        public IActionResult Post([FromRoute] long sectionId, [FromBody] SectionDto section)
        {
            section.Id = sectionId;
            var getResult = _readConductor.FindById(section.Id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return NotFound(false, getResult.Errors);
            }

            var updatedSection  = getResult.ResultObject;
            updatedSection.Name = section.Name;

            var updateResult = _updateConductor.Update(updatedSection);
            if (updateResult.HasErrors)
            {
                return InternalError(updateResult.ResultObject, updateResult.Errors);
            }

            return Ok(true, null);
        }

        #endregion POST

        #region GET

        [HttpGet]
        public IActionResult Index()
        {
            var getResult = _readConductor.FindAll();
            if (getResult.HasErrorsOrResultIsNull())
            {
                return InternalError<IEnumerable<SectionDto>>(null, getResult.Errors);
            }

            return Ok<IEnumerable<SectionDto>>(getResult.ResultObject.Select(e => _mapper.Map<SectionDto>(e)), null);
        }

        [HttpGet("{id:long}")]
        public IActionResult Get(long id)
        {
            var getResult = _readConductor.FindById(id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return InternalError<SectionDto>(null, getResult.Errors);
            }

            return Ok(_mapper.Map<SectionDto>(getResult.ResultObject), null);
        }

        #endregion GET

        #region DELETE

        /// <summary>
        /// Deleting a section will automatically delete all the articles because of our foreign key constraint
        /// and delete behavior in our database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public IActionResult Delete(long id)
        {
            var deleteResult = _deleteConductor.Delete(id: id, soft: false);
            if (deleteResult.HasErrors)
            {
                return InternalError(deleteResult.ResultObject, deleteResult.Errors);
            }

            return Ok(deleteResult.ResultObject, null);
        }

        #endregion DELETE
    }
}

```

`ArticlesController.cs`
```c#
namespace BlazorCMS.Server.Controllers
{
    [FormatFilter]
    [Route("/api/{sectionId:long}/articles")]
    public class ArticlesController : BaseController
    {
        #region Properties

        private readonly IRepositoryCreateConductor<Article> _createConductor;
        private readonly IRepositoryDeleteConductor<Article> _deleteConductor;
        private readonly IRepositoryReadConductor<Article>   _readConductor;
        private readonly IRepositoryUpdateConductor<Article> _updateConductor;
        private readonly IMapper                             _mapper;

        #endregion Properties

        #region Constructor

        public ArticlesController(
            IRepositoryCreateConductor<Article> createConductor,
            IRepositoryDeleteConductor<Article> deleteConductor,
            IRepositoryReadConductor<Article>   readConductor,
            IRepositoryUpdateConductor<Article> updateConductor,
            IMapper                             mapper
        )
        {
            _createConductor = createConductor;
            _deleteConductor = deleteConductor;
            _readConductor   = readConductor;
            _updateConductor = updateConductor;
            _mapper          = mapper;
        }

        #endregion Constructor

        #region PUT

        [HttpPut]
        public IActionResult Put([FromBody] ArticleDto article)
        {
            var newArticle = new Article
            {
                Title     = article.Title,
                Body      = article.Body,
                SectionId = article.SectionId
            };
            var createResult = _createConductor.Create(newArticle);
            if (createResult.HasErrors)
            {
                return InternalError<ArticleDto>(null, createResult.Errors);
            }

            return Ok(createResult.ResultObject, null);
        }

        #endregion PUT

        #region POST

        [HttpPost("{articleId:long}")]
        public IActionResult Post([FromRoute] long articleId, [FromBody] ArticleDto article)
        {
            article.Id = articleId;
            var getResult = _readConductor.FindById(article.Id);
            if (getResult.HasErrorsOrResultIsNull())
            {
                return NotFound(false, getResult.Errors);
            }

            var updatedArticle   = getResult.ResultObject;
            updatedArticle.Title = article.Title;
            updatedArticle.Body  = article.Body;

            var updateResult = _updateConductor.Update(updatedArticle);
            if (updateResult.HasErrors)
            {
                return InternalError(updateResult.ResultObject, updateResult.Errors);
            }

            return Ok(true, null);
        }

        #endregion Post

        #region GET

        [HttpGet]
        public IActionResult Index([FromRoute] long sectionId)
        {
            Expression<Func<Article, bool>> filter = e => e.SectionId == sectionId;
            var findResult                         = _readConductor.FindAll(filter);

            if (findResult.HasErrorsOrResultIsNull())
            {
                return NotFound<IEnumerable<ArticleDto>>(null, findResult.Errors);
            }

            return Ok<IEnumerable<ArticleDto>>(findResult.ResultObject.Select(e => _mapper.Map<ArticleDto>(e)), null);
        }

        [HttpGet("{id:long}")]
        public IActionResult Get([FromRoute] long sectionId, [FromRoute] long id)
        {
            var findResult = _readConductor.FindById(id);
            if (findResult.HasErrorsOrResultIsNull())
            {
                return NotFound<ArticleDto>(null, findResult.Errors);
            }

            return Ok(_mapper.Map<ArticleDto>(findResult.ResultObject), null);
        }

        #endregion GET

        #region DELETE

        [HttpDelete("{id:long}")]
        public IActionResult Delete([FromRoute] long sectionId, [FromRoute] long id)
        {
            var deleteResult = _deleteConductor.Delete(id: id, soft: false);
            if (deleteResult.HasErrorsOrResultIsNull())
            {
                return InternalError(false, deleteResult.Errors);
            }

            return Ok(deleteResult.ResultObject, deleteResult.Errors);
        }

        #endregion DELETE
    }
}
```

Now, let's seed some data, so we can test our API. Add a class called `BlazorCmsContextExtensions` under the `Data` directory.

`BlazorCmsContextExtensions.cs`
```c#
namespace BlazorCMS.Server.Data
{
    public static class BlazorCmsContextExtensions
    {
        public static void SeedHelloWorldSectionAndArticle(this BlazorCmsContext context)
        {
            if (context.Sections.Any())
            {
                return;
            }
            var section = new Section
            {
                Name     = "Hello World!",
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Hello World!",
                        Body  = "This is an article!"
                    }
                }
            };

            context.Sections.Add(section);
            context.SaveChanges();
        }
    }
}
```

Then call this extension method in the `Startup.Configure`. The method should look like this in full:

```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseResponseCompression();

    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseBlazorDebugging();
    }

    app.UseStaticFiles();
    app.UseClientSideBlazorFiles<Client.Startup>();

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapDefaultControllerRoute();
        endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
    });

    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        using (var dbContext = serviceScope.ServiceProvider.GetService<BlazorCmsContext>())
        {
            dbContext.Database.Migrate();
            dbContext.SeedHelloWorldSectionAndArticle();
        }
    }
}
```

Now, you should be able to run the app (`dotnet run` in the `Server` directory), and navigate to `http://localhost:5000/api/sections` and see a JSON response
containing our seeded `Section`. Now that our API is done, we can start building the frontend.

Let's add the `Blazor-State` package to the `Client` project.

`dotnet add package Blazor-State`

Let's add our state object now. In a new `State` directory, add the following file:

`ClientState.cs`
```c#
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
```

Now we'll need an HTTP client to implement client side services.

`dotnet add package Microsoft.AspNetCore.Blazor.HttpClient`

Now add a service class under a new `Services`.

`Service.cs`
```c#
namespace BlazorCMS.Client.Services
{
    public abstract class Service
    {
        protected HttpClient _client { get; set; }

        public Service(NavigationManager manager)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(manager.BaseUri)
            };
        }
    }
}
```

`SectionService.cs`
```c#
namespace BlazorCMS.Client.Services
{
    public class SectionService : Service
    {
        public SectionService(NavigationManager navigationManager) : base(navigationManager)
        {
        }

        public async Task<IResult<SectionDto[]>> Index()
        {
            return await _client.GetJsonAsync<Result<SectionDto[]>>("/api/sections");
        }
    }
}
```

Now modify the `NavMenu.razor` to index and show the available sections in the sidebar.

```razor
@foreach (var section in Sections)
{
    <li class="nav-item px-3">
        <span class="oi oi-plus" aria-hidden="true"></span> @section.Name
    </li>
}
...
@code {
    [Inject]
    private NavigationManager _navigationManager { get; set; }
    private SectionService _sectionService       { get; set; }

    private List<SectionDto> Sections => List<SectionDto> Sections => Store.GetState<ClientState>().Sections;

    private async Task LoadSections()
    {
        var result = await _sectionService.Index();
        var state = Store.GetState<ClientState>();
        state.Sections = result.ResultObject?.ToList() ?? new List<SectionDto>();
        Store.SetState(state);
    }

    protected override async Task OnInitializedAsync()
    {
        _sectionService = new SectionService(_navigationManager);
        await LoadSections();
    }
}
```

Now, if you run the app, you should see the "Hello World!" seeded section appear on the sidebar!

Let's work on showing the articles in a section when you select a section. First let's add a loading component to show a loading state.
We'll base this on [SpinKit](https://tobiasahlin.com/spinkit/). Add a `Components` directory under the `Client` project and add a file called
`Loading.razor`, and add a `Loading.css` file under `Client/wwwroot/Components`. Choose a loading spinner from SpinKit and
add the HTML to `Loading.razor` and the CSS to `Loading.css`. Then, in `site.css`, add the following line to the top:

`@import url('Components/Loading.css');`

Next, we'll need an `ArticleService.cs`:

```c#
namespace BlazorCMS.Client.Services
{
    public class ArticleService : Service
    {
        public ArticleService(NavigationManager manager) : base(manager)
        {
        }

        public async Task<IResult<ArticleDto[]>> Index(long sectionId)
        {
            return await _client.GetJsonAsync<Result<ArticleDto[]>>($"/api/{sectionId}/articles");
        }
    }
}
```

Then, add it to `NavMenu.razor`:
```razor
[Inject]
private NavigationManager _navigationManager { get; set; }
private SectionService    _sectionService    { get; set; }
private ArticleService    _articleService    { get; set; }
...
protected override async Task OnInitializedAsync()
{
    _sectionService = new SectionService(_navigationManager);
    _articleService = new ArticleService(_navigationManager);
    await LoadSections();
}
```

Let's add a bit of code to `NavMenu.razor` to expand sections to show articles on click:

```razor
@foreach (var section in Sections)
{
    <li class="nav-item px-3">
        <span @onclick="@(() => LoadArticlesForSections(section.Id))">
            <NavLink class="nav-link">
                <span class="oi oi-book" aria-hidden="true"></span> @section.Name
            </NavLink>
        </span>
        @if (expandedSectionId == section.Id)
        {
            @if (expandedSectionArticles == null || isLoadingArticles)
            {
                <Loading Light="@true"/>
            }
            else
            {
                <ul class="nav flex-column sub-menu">
                    @foreach (var article in expandedSectionArticles)
                    {
                        <li class="nav-item px-3">
                            <NavLink class="nav-link" href="@($"/Section/{section.Id}/Article/{article.Id}")">
                                <span class="oi oi-justify-left" aria-hidden="true"></span> @article.Title
                            </NavLink>
                        </li>
                    }
                </ul>
            }
        }
    </li>
}

...

private long             expandedSectionId       { get; set; }
private List<ArticleDto> expandedSectionArticles { get; set; }
private bool             isLoadingArticles       { get; set; }

private async Task LoadArticlesForSections(long sectionId)
{
    // if clicked from the already expanded section, collapse it
    if (sectionId == expandedSectionId)
    {
        expandedSectionId       = -1;
        expandedSectionArticles = null;
        isLoadingArticles       = false;
        return;
    }

    expandedSectionId = sectionId;
    isLoadingArticles = true;
    var result = await _articleService.Index(expandedSectionId);
    if (result.HasErrorsOrResultIsNull())
    {
        expandedSectionArticles = new List<ArticleDto>();
        isLoadingArticles       = false;
        return;
    }
    expandedSectionArticles = result.ResultObject.ToList();
    isLoadingArticles       = false;
}
```

Now clicking a section in the sidebar should briefly show a loading state, and then show the articles in the section.

Let's start building the article view. To render markdown, we'll use `MarkDig`.

`dotnet add package Markdig`

The actual component itself is pretty straightforward. We'll want to add a preview mode to the editor later, so let's extract the markdown
rendering logic into a component. Add a `Markdown.razor` file in the `Components` directory.

`Markdown.razor`
```razor
@using Markdig;

@GetRenderedMarkdownString()

@code {

    [Parameter]
    public string Content { get; set; }

    private MarkupString GetRenderedMarkdownString() => (MarkupString)Markdig.Markdown.ToHtml(
        markdown: Content,
        pipeline: new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()
    );

}
``` 

We can now use this component to render our article:

`ArticlePage.razor`
```razor
@page "/Section/{SectionId:long}/Article/{ArticleId:long}"
@using BlazorCMS.Client.Services
@using BlazorCMS.Client.State
@using BlazorCMS.Shared.Dtos
@inherits BlazorState.BlazorStateComponent

@if (Article != null)
{
    <h1>@Article.Title</h1>
    <hr/>
    <Markdown Content="@Article.Body"/>
}

@code {

    [Parameter]
    public long? ArticleId { get; set; }

    [Parameter]
    public long? SectionId { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    private ArticleService    _articleService    { get; set; }

    private ArticleDto Article => Store.GetState<ClientState>()?.Articles?.FirstOrDefault(e => e.Id == ArticleId);

    protected override async Task OnInitializedAsync()
    {
        SectionId ??= 1;
        ArticleId ??= 1;
        _articleService = new ArticleService(_navigationManager);
        if (Article == null)
        {
            // populate all the articles for the section so the nav bar updates properly
            var result = await _articleService.Index(SectionId.Value);
            var state  = Store.GetState<ClientState>();
            if (state.Articles == null)
            {
                state.Articles = new List<ArticleDto>();
            }

            state.Articles = state.Articles.Where(e => e.SectionId != SectionId.Value).ToList();
            state.Articles.AddRange(result.ResultObject);
        }
    }

}
```

Now let's build an editor for our create/edit screens. For this we'll add the `BlazorStrap` package (Bootstrap 4 Blazor components);

`dotnet add package BlazorStrap`

Now we can use it to build a simple editor.

`Editor.razor`
```c#
@using BlazorStrap

<BSTabGroup class="editor-tabgroup">
    <BSTabList>
        <BSTab>
            <BSTabLabel>Edit</BSTabLabel>
            <BSTabContent>
                <textarea
                    class="editor-textarea form-control"
                    @bind-value="@Content"
                    @bind-value:event="oninput">
                </textarea>
                <div class="editor-footer">
                    <button type="button" class="btn btn-primary">Save</button>
                    <button type="button" class="btn btn-secondary">Cancel</button>
                </div>
            </BSTabContent>
        </BSTab>
        <BSTab>
            <BSTabLabel>Preview</BSTabLabel>
            <BSTabContent>
                <Markdown Content="@Content"/>
            </BSTabContent>
        </BSTab>
    </BSTabList>
    <BSTabSelectedContent/>
</BSTabGroup>

@code {

    [Parameter]
    public string InitialContent { get; set; }

    [Parameter]
    public Func<string, Task> OnSave { get; set; }

    [Parameter]
    public Action OnCancel { get; set; }

    private string Content { get; set; }

    private void OnSaveCallback() => OnSave(Content);

    protected override void OnInitialized()
    {
        Content = InitialContent ?? "";
    }

}
```

Next we need to add a `POST` method to our service, and create the edit page.

`ArticleService.cs`
```c#
public async Task<IResult<bool>> Post(ArticleDto article)
{
    return await _client.PostJsonAsync<Result<bool>>($"/api/{article.SectionId}/articles/{article.Id}", article);
}
```

`Edit.razor`
```razor
@page "/Section/{SectionId:long}/Article/{ArticleId:long}/Edit"
@using AndcultureCode.CSharp.Core.Extensions
@using BlazorCMS.Client.Services
@using BlazorCMS.Client.State
@using BlazorCMS.Shared.Dtos
@inherits BlazorState.BlazorStateComponent

@if (Article != null)
{
    <h1>Editing: @Article.Title</h1>
    <Editor InitialContent="@Article.Body" OnSave="@OnSave" OnCancel="@OnCancel" IsLoading="@IsLoading"/>
}

@code {

    [Parameter]
    public long? ArticleId { get; set; }

    [Parameter]
    public long? SectionId { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    private ArticleService    _articleService    { get; set; }

    private bool IsLoading { get; set; }

    private ArticleDto Article => Store.GetState<ClientState>()?.Articles?.FirstOrDefault(e => e.Id == ArticleId);

    private async Task OnSave(string newContent)
    {
        IsLoading = true;
        var article = Article;
        article.Body = newContent;
        var result = await _articleService.Post(article);
        if (!result.HasErrorsOrResultIsNull())
        {
            var state = Store.GetState<ClientState>();
            state.Articles = state.Articles.Where(e => e.Id != article.Id).ToList();
            state.Articles.Add(article);
            Store.SetState(state);
            _navigationManager.NavigateTo($"/Section/{SectionId}/Article/{ArticleId}");
        }

        IsLoading = false;
    }

    private void OnCancel()
    {
        _navigationManager.NavigateTo($"/Section/{SectionId}/Article/{ArticleId}");
    }

    protected override async Task OnInitializedAsync()
    {
        SectionId ??= 1;
        ArticleId ??= 1;
        _articleService = new ArticleService(_navigationManager);
        if (Article == null)
        {
            // populate all the articles for the section so the nav bar updates properly
            var result = await _articleService.Index(SectionId.Value);
            var state  = Store.GetState<ClientState>();
            if (state.Articles == null)
            {
                state.Articles = new List<ArticleDto>();
            }

            state.Articles = state.Articles.Where(e => e.SectionId != SectionId.Value).ToList();
            state.Articles.AddRange(result.ResultObject);
        }
    }

}
```

The create page is basically the same, but you'll call a new `Put` method on the service instead of a `Post` method.

`ArticleService.cs`
```c#
public async Task<IResult<ArticleDto>> Put(ArticleDto article)
{
    return await _client.PutJsonAsync<Result<ArticleDto>>($"/api/{article.SectionId}/articles", article);
}
```
