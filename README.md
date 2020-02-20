# BlazorCMS

[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) is a framework for building client-side applications with C#.
There are two different variations of Blazor; Blazor-Server, in which C# processing is performed on the server and the results
are sent to the client via a websocket connection, or Blazor-Wasm, which actually ships a full WebAssembly .NET runtime to the browser. 

Let's build a (very) simple CMS using Blazor-Wasm, with a .NET Core hosted backend. Or, you can skip to the [analytics](#analytics);

All of the code related to this article can be found [here](https://github.com/andCulture/BlazorCMS).

Note: `using` import statements are omitted here for brevity.

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
We'll also use an andculture open-source project, [AndcultureCode.CSharp.Conductors](https://github.com/AndcultureCode/AndcultureCode.CSharp.Conductors), in this project:

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

We'll use the [AutoMapper](https://automapper.org/) package to automatically map our models to our DTOs. Add this package to your `Server` project.

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

Let's add the [Blazor-State](https://timewarpengineering.github.io/blazor-state/), [Blazor.ContextMenu](https://github.com/stavroskasidis/BlazorContextMenu),
and [System.Collections.Immutable](https://www.nuget.org/packages/System.Collections.Immutable/) packages to the `Client` project.

`dotnet add package Blazor-State`

`dotnet add package Blazor.ContextMenu`

`dotnet add package System.Collections.Immutable`

Blazor is still in preview, so you may experience some issues, such as needing to call `this.StateHasChanged()` within a component to get it to
re-render. For this reason, we need a way for other components to update the nav bar when a new section or article is created.

Create a new abstract class in a new `State` directory:

`UpdatableComponent.cs`
```c#
namespace BlazorCMS.Client.State
{
    public abstract class UpdatableComponent : BlazorStateComponent
    {
        public void Update()
        {
            this.StateHasChanged();
        }
    }
}
```

Then create your state object in the same directory:

`ClientState.cs`
```c#
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
```

Now we'll need to add client-side services to use our API. Create your services in a `Services` directory.

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

`ArticleService.cs`
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
            return await _client.GetJsonAsync<Result<ArticleDto[]>>($"/api/sections/{sectionId}/articles");
        }

        public async Task<IResult<bool>> Post(ArticleDto article)
        {
            return await _client.PostJsonAsync<Result<bool>>($"/api/sections/{article.SectionId}/articles/{article.Id}", article);
        }

        public async Task<IResult<ArticleDto>> Put(ArticleDto article)
        {
            return await _client.PutJsonAsync<Result<ArticleDto>>($"/api/sections/{article.SectionId}/articles", article);
        }
        public async Task<IResult<bool>> Delete(long sectionId, long articleId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}/articles/{articleId}");
            if (result.IsSuccessStatusCode)
            {
                return new Result<bool>
                {
                    ResultObject = true,
                    Errors       = null
                };
            }

            return new Result<bool>
            {
                ResultObject = false,
                Errors = new List<IError>
                {
                    new Error
                    {
                        ErrorType = ErrorType.Error,
                        Key       = "DeleteError",
                        Message   = "Failed to delete section."
                    }
                }
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

        public async Task<IResult<bool>> Delete(long sectionId)
        {
            var result = await _client.DeleteAsync($"/api/sections/{sectionId}");
            if (result.IsSuccessStatusCode)
            {
                return new Result<bool>
                {
                    ResultObject = true,
                    Errors       = null
                };
            }

            return new Result<bool>
            {
                ResultObject = false,
                Errors       = new List<IError>
                {
                    new Error
                    {
                        ErrorType = ErrorType.Error,
                        Key       = "DeleteError",
                        Message   = "Failed to delete section."
                    }
                }
            };
        }

        public async Task<IResult<bool>> Edit(SectionDto section)
        {
            return await _client.PostJsonAsync<Result<bool>>($"/api/sections/{section.Id}", section);
        }

        public async Task<IResult<SectionDto>> Create(SectionDto section)
        {
            return await _client.PutJsonAsync<Result<SectionDto>>("/api/sections", section);
        }
    }
}
```

Let's build some components that we'll use to customize the nav bar. Let's start with a loading indicator, using [SpinKit](https://tobiasahlin.com/spinkit/).
Choose your favorite variation and copy the markup and css into your project.

`Loading.razor`
```razor
@namespace BlazorCMS.Client.Components

<div class="@SpinnerClass">
    <div class="rect1"></div>
    <div class="rect2"></div>
    <div class="rect3"></div>
    <div class="rect4"></div>
    <div class="rect5"></div>
</div>

@code {

    [Parameter]
    public bool Light { get; set; }

    private string SpinnerClass => Light ? "spinner light" : "spinner";

}
```

`Loading.css`
```css
.spinner.light > div {
    background-color: white;
}
/* the rest is copy/pasted; omitted for brevity */
```

Now the context menu (so we can right-click to edit or delete things):

`SidebarContextMenu.razor`
```razor
<ContextMenu Id="@GetMenuId(GetId(), Type ?? SidebarContextMenuType.SECTION)">
    @if (ShowEdit.HasValue && ShowEdit.Value)
    {
        <Item OnClick="@OnEditCallback">
            <i class="oi oi-pencil"></i>
            &nbsp;Edit
        </Item>
    }
    <Item OnClick="@OnDeleteCallback">
        <i class="oi oi-trash"></i>
        &nbsp;Delete
    </Item>
</ContextMenu>

@code {

    public enum SidebarContextMenuType
    {
        SECTION,
        ARTICLE,
    }

    public static string GetMenuId(long id, SidebarContextMenuType type = SidebarContextMenuType.SECTION) => $"SidebarContextMenu-{type}-{id}";

    [Parameter]
    public Action<long>? OnEdit { get; set; }

    [Parameter]
    public Func<long, Task> OnDelete { get; set; }

    [Parameter]
    public SectionDto? Section { get; set; }

    [Parameter]
    public ArticleDto? Article { get; set; }

    [Parameter]
    public SidebarContextMenuType? Type { get; set; }

    [Parameter]
    public bool? ShowEdit { get; set; }

    private void OnEditCallback()
    {
        if (OnEdit != null)
        {
            OnEdit(GetId());
        }
    }

    private void OnDeleteCallback()
    {
        OnDelete(GetId());
    }

    private long GetId()
    {
        switch (Type)
        {
            case SidebarContextMenuType.ARTICLE:
                return Article?.Id ?? -1;
            case SidebarContextMenuType.SECTION:
                return Section?.Id ?? -1;
            default:
                return -1;
        }
    }

    protected override void OnInitialized()
    {
        ShowEdit ??= true;
        Type ??= SidebarContextMenuType.SECTION;
        base.OnInitialized();
    }

}
```

And finally, a section create/edit form component:

`SidebarEditSection.razor`
```razor
<input
    id="section-edit-input"
    type="text"
    class="form-control"
    placeholder="Section Title"
    @bind-value="@Title"
    @bind-value:event="oninput"
    @onblur="@OnSaveCallback"/>

@code {

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Parameter]
    public long SectionId { get; set; }

    [Parameter]
    public string InitialTitle { get; set; }

    [Parameter]
    public Func<long, string, Task> OnSave { get; set; }

    private string Title { get; set; }

    private void OnSaveCallback()
    {
        OnSave(SectionId, Title);
    }

    protected override Task OnParametersSetAsync()
    {
        Title = InitialTitle;
        return base.OnParametersSetAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            JsRuntime.InvokeVoidAsync("BlazorCmsJsFunctions.focus", "#section-edit-input");
        }
        base.OnAfterRender(firstRender);
    }

}
```

Now we can start customizing the nav bar. Open `NavMenu.razor` and edit the markup to add our sections and articles to the sidebar:

```razor
<ul class="nav flex-column">
    <li class="nav-item px-3">
        <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
            <span class="oi oi-home" aria-hidden="true"></span> Home
        </NavLink>
    </li>
    @if (_isLoadingAll)
    {
        <Loading Light="@true"/>
    }
    else
    {
        <li class="nav-item px-3">
            <button type="button" class="btn btn-success w-100" @onclick="@OnSectionCreate">
                Create Section&nbsp;<i class="oi oi-plus"></i>
            </button>
        </li>
        @foreach (var section in Sections)
        {
            <li class="nav-item px-3">
                @if (_editingSectionId == section.Id)
                {
                    <SidebarEditSection
                        SectionId="@section.Id"
                        InitialTitle="@section.Name"
                        OnSave="@OnSectionEditConfirm"/>
                }
                else
                {
                    <span @onclick="@(() => LoadArticlesForSections(section.Id))">
                        <ContextMenuTrigger MenuId="@(SidebarContextMenu.GetMenuId(section.Id))">
                            <NavLink class="nav-link">
                                <span class="oi oi-book" aria-hidden="true"></span> @section.Name
                            </NavLink>
                        </ContextMenuTrigger>
                        <SidebarContextMenu Section="@section" OnEdit="@OnSectionEdit" OnDelete="@OnSectionDelete"/>
                    </span>
                }
                @if (ExpandedSectionId == section.Id)
                {
                    @if (IsLoadingArticles)
                    {
                        <Loading Light="@true"/>
                    }
                    else
                    {
                        <ul class="nav flex-column sub-menu">
                            @foreach (var article in ExpandedSectionArticles)
                            {
                                <li class="nav-item px-3">
                                    <ContextMenuTrigger MenuId="@SidebarContextMenu.GetMenuId(article.Id, SidebarContextMenu.SidebarContextMenuType.ARTICLE)">
                                        <NavLink class="nav-link" id="@($"nav-link-article-{article.Id}")" href="@($"/Section/{section.Id}/Article/{article.Id}")">
                                            <span class="oi oi-justify-left" aria-hidden="true"></span> @article.Title
                                        </NavLink>
                                    </ContextMenuTrigger>
                                    <SidebarContextMenu Article="@article" OnDelete="@OnArticleDelete" Type="@SidebarContextMenu.SidebarContextMenuType.ARTICLE" ShowEdit="@false"/>
                                </li>
                            }
                            <li class="nav-item px-3">
                                <button type="button" class="btn btn-success w-100" @onclick="@(() => Create(section.Id))">
                                    Create Article&nbsp;<i class="oi oi-plus"></i>
                                </button>
                            </li>
                        </ul>
                    }
                }
            </li>
        }
        @if (_showSectionCreate)
        {
            <li class="nav-item px-3">
                <SidebarEditSection
                    SectionId="@(-1)"
                    InitialTitle=""
                    OnSave="@OnSectionCreateConfirm"/>
            </li>
        }
    }
</ul>
```

Now let's add some Javascript functions to showcase Javascript interop, and do some things with the DOM that aren't yet supported.
Create a file under `wwwroot` called `BlazorCmsFunctions.js` and import it in the `<head>` of your `index.html`.

`BlazorCmsFunctions.js`
```javascript
window.BlazorCmsJsFunctions = {
    focus: function (selector) {
        var el = document.querySelector(selector);
        if (el != null && el.focus != null) {
            el.focus();
        }
    },
    hasClass: function (selector, className) {
        var el = document.querySelector(selector);
        if (el == null || el.classList == null) {
            return false;
        }

        return el.classList.contains(className);
    }
};
```

Now we can add the code for our `NavMenu.razor` component:

```razor
@code {
    [Inject]
    private NavigationManager NavigationManager { get; set; }
    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    private SectionService    _sectionService;
    private ArticleService    _articleService;
    private bool collapseNavMenu = true;
    private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private bool _isLoadingAll = false;
    private long _editingSectionId = -1;
    private bool _showSectionCreate = false;

    private long ExpandedSectionId
    {
        get => Store.GetState<ClientState>().ExpandedSectionId;
        set
        {
            var state = Store.GetState<ClientState>();
            state.ExpandedSectionId = value;
            Store.SetState(state);
        }
    }

    private List<ArticleDto> ExpandedSectionArticles
    {
        get => Store.GetState<ClientState>().Articles?.Where(e => e.SectionId == ExpandedSectionId).ToList() ?? new List<ArticleDto>();
    }

    private bool IsLoadingArticles
    {
        get => Store.GetState<ClientState>().SidebarLoadingArticles;
        set
        {
            var state = Store.GetState<ClientState>();
            state.SidebarLoadingArticles = value;
            Store.SetState(state);
        }
    }

    private void OnSectionEdit(long sectionId)
    {
        _editingSectionId = sectionId;
        this.StateHasChanged();
    }

    private void OnSectionCreate()
    {
        _showSectionCreate = true;
    }

    private async Task OnSectionCreateConfirm(long sectionId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            _showSectionCreate = false;
            this.StateHasChanged();
            return;
        }

        _isLoadingAll = true;
        var section   = new SectionDto
        {
            Name = title
        };
        var state  = Store.GetState<ClientState>();
        var result = await _sectionService.Create(section);
        if (!result.HasErrorsOrResultIsNull())
        {
            state.Sections = state.Sections.Add(result.ResultObject);
        }

        _isLoadingAll      = false;
        _showSectionCreate = false;
        this.StateHasChanged();
    }

    private async Task OnArticleDelete(long articleId)
    {
        _isLoadingAll = true;
        var shouldRedirect = await JsRuntime.InvokeAsync<bool>("BlazorCmsJsFunctions.hasClass", $"#nav-link-article-{articleId}", "active");
        Console.WriteLine(shouldRedirect);
        var state          = Store.GetState<ClientState>();
        var article        = state.Articles.First(e => e.Id == articleId);
        var result         = await _articleService.Delete(article.SectionId, articleId);
        if (result.ResultObject)
        {
            state.Articles = state.Articles.Where(e => e.Id != articleId).ToImmutableList();
            Store.SetState(state);
        }
        _isLoadingAll = false;
        this.StateHasChanged();
        if (shouldRedirect)
        {
            NavigationManager.NavigateTo("/");
        }
    }

    private async Task OnSectionEditConfirm(long sectionId, string title)
    {
        _isLoadingAll = true;
        var state = Store.GetState<ClientState>();
        var section = state.Sections.First(e => e.Id == sectionId);
        section.Name = title;
        var result = await _sectionService.Edit(section);
        if (result.ResultObject)
        {
            state.Sections = state.Sections.Where(e => e.Id != sectionId).ToImmutableList();
            state.Sections = state.Sections.Add(section);
            Store.SetState(state);
        }
        _isLoadingAll     = false;
        _editingSectionId = -1;
        this.StateHasChanged();
    }

    private async Task OnSectionDelete(long sectionId)
    {
        _isLoadingAll = true;
        var result = await _sectionService.Delete(sectionId);
        if (result.ResultObject)
        {
            var state = Store.GetState<ClientState>();
            state.Articles = state.Articles?.Where(e => e.SectionId != sectionId)?.ToImmutableList() ?? ImmutableList<ArticleDto>.Empty;
            state.Sections = state.Sections?.Where(e => e.Id != sectionId)?.ToImmutableList() ?? ImmutableList<SectionDto>.Empty;
            Store.SetState(state);
        }
        _isLoadingAll = false;
        this.StateHasChanged();
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private ImmutableList<SectionDto> Sections => Store.GetState<ClientState>().Sections;

    private void Create(long sectionId)
    {
        NavigationManager.NavigateTo($"/Section/{sectionId}/Create");
    }

    private async Task LoadArticlesForSections(long sectionId)
    {
        // if clicked from the already expanded section, collapse it
        if (sectionId == ExpandedSectionId)
        {
            ExpandedSectionId  = -1;
            IsLoadingArticles = false;
            return;
        }

        ExpandedSectionId = sectionId;
        IsLoadingArticles = true;

        var existingArticles = Store.GetState<ClientState>().Articles?.Where(e => e.SectionId == sectionId)?.ToList();
        if (existingArticles != null && existingArticles.Any())
        {
            IsLoadingArticles = false;
            return;
        }

        var result = await _articleService.Index(ExpandedSectionId);
        if (result.HasErrorsOrResultIsNull())
        {
            IsLoadingArticles = false;
            return;
        }

        // update shared state
        var state = Store.GetState<ClientState>();
        state.Articles = state.Articles.AddRange(result.ResultObject);
        Store.SetState(state);
        IsLoadingArticles = false;
    }

    private async Task LoadSections()
    {
        var result = await _sectionService.Index();
        var state = Store.GetState<ClientState>();
        state.Sections = result.ResultObject?.ToImmutableList() ?? ImmutableList<SectionDto>.Empty;
        Store.SetState(state);
    }

    protected override void OnInitialized()
    {
        Store.GetState<ClientState>().RegisterNavMenuComponent(this);
    }

    protected override async Task OnInitializedAsync()
    {
        _sectionService = new SectionService(NavigationManager);
        _articleService = new ArticleService(NavigationManager);
        await LoadSections();
    }

}
```

Writing Blazor components is extremely similar to writing regular old C# code!

Now we can work on creating/editing articles. Let's start with a generic markdown component. For this we'll use the
[MarkDig](https://github.com/lunet-io/markdig) package:

`dotnet add package MarkDig`

The component itself is very straightforward.

`Markdown.razor`
```razor
<div class="markdown-container">
    @GetRenderedMarkdownString()
</div>

@code {

    [Parameter]
    public string Content { get; set; }

    private MarkupString GetRenderedMarkdownString() => (MarkupString)Markdig.Markdown.ToHtml(
        markdown: Content,
        pipeline: new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()
    );

}
```

Now we can use this to build our view/create/edit screens. Let's start with the view article screen.

Under the `Pages` directory, add `ArticlePage.razor`:

```razor
@page "/Section/{SectionId:long}/Article/{ArticleId:long}"
@inherits BlazorState.BlazorStateComponent

@if (Article != null)
{
    <h1>
        @Article.Title
        <button type="button" class="btn btn-warning float-right" @onclick="Edit">
            <i class="oi oi-pencil"></i>
        </button>
    </h1>
    <hr/>
    <Markdown Content="@Article.Body"/>
}

@code {

    [Parameter]
    public long ArticleId { get; set; }

    [Parameter]
    public long SectionId { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }
    private ArticleService    _articleService;

    private ArticleDto Article => Store.GetState<ClientState>()?.Articles?.FirstOrDefault(e => e.Id == ArticleId);

    private void Edit()
    {
        NavigationManager.NavigateTo($"/Section/{SectionId}/Article/{ArticleId}/Edit");
    }

    protected override void OnInitialized()
    {
        _articleService = new ArticleService(NavigationManager);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Article == null)
        {
            // populate all the articles for the section so the nav bar updates properly
            var state                    = Store.GetState<ClientState>();
            state.ExpandedSectionId      = SectionId;
            state.SidebarLoadingArticles = true;
            Store.SetState(state);
            var result     = await _articleService.Index(SectionId);
            state.Articles = state.Articles.Where(e => e.SectionId != SectionId).ToImmutableList();
            state.Articles = state.Articles.AddRange(result.ResultObject);
            state.SidebarLoadingArticles = false;
            Store.SetState(state);
            state.UpdateNavMenu();
        }
    }

}
```

For the actual markdown editor, we'll use a package called [BlazorStrap](https://blazorstrap.io/), a Blazor implementation of
Bootstrap 4 components.

`dotnet add package BlazorStrap`

Now we can use the `BSTabs` components to create a markdown editor with a preview tab. In the `Components` directory, add `Editor.razor`:

```razor
<input type="text" @bind-value="@Content.Title" @bind-value:event="oninput" class="form-control editor-title-input"/>
<BSTabGroup class="editor-tabgroup">
    <BSTabList>
        <BSTab>
            <BSTabLabel>Edit</BSTabLabel>
            <BSTabContent>
                <textarea
                    class="editor-textarea form-control"
                    @bind-value="@Content.Body"
                    @bind-value:event="oninput"></textarea>
                <div class="editor-footer">
                    <button type="button" class="btn btn-primary" disabled="@IsLoading" @onclick="@OnSaveCallback">
                        @if (IsLoading)
                        {
                            <Loading/>
                        }
                        else
                        {
                            <span>Save</span>
                        }
                    </button>
                    <button type="button" class="btn btn-secondary" disabled="@IsLoading" @onclick="@OnCancelCallback">Cancel</button>
                </div>
            </BSTabContent>
        </BSTab>
        <BSTab>
            <BSTabLabel>Preview</BSTabLabel>
            <BSTabContent>
                <Markdown Content="@Content.Body"/>
            </BSTabContent>
        </BSTab>
    </BSTabList>
    <BSTabSelectedContent/>
</BSTabGroup>

@code {

    [Inject]
    IJSRuntime JsRuntime { get; set; }

    [Parameter]
    public ArticleDto InitialContent { get; set; }

    [Parameter]
    public Func<ArticleDto, Task> OnSave { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public Action OnCancel { get; set; }

    private ArticleDto Content { get; set; }

    private void OnSaveCallback() => OnSave(Content);
    private void OnCancelCallback() => OnCancel();

    protected override Task OnParametersSetAsync()
    {
        Content       = InitialContent ?? new ArticleDto();
        Content.Body  = Content.Body?.Trim() ?? "";
        Content.Title = Content.Title?.Trim() ?? "";
        return base.OnParametersSetAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JsRuntime.InvokeVoidAsync("BlazorCmsJsFunctions.focus", ".form-control.editor-title-input");
        }
        return base.OnAfterRenderAsync(firstRender);
    }

}

```

And use this component to build our edit screen. In the `Pages` directory, add `Edit.razor`:

```razor
@page "/Section/{SectionId:long}/Article/{ArticleId:long}/Edit"
@inherits BlazorState.BlazorStateComponent

@if (Article != null)
{
    <Editor InitialContent="@Article" OnSave="@OnSave" OnCancel="@OnCancel" IsLoading="@IsLoading"/>
}

@code {

    [Parameter]
    public long ArticleId { get; set; }

    [Parameter]
    public long SectionId { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }
    private ArticleService    _articleService;

    private bool IsLoading { get; set; }
    
    private ArticleDto Article => Store.GetState<ClientState>()?.Articles?.FirstOrDefault(e => e.Id == ArticleId);

    private async Task OnSave(ArticleDto article)
    {
        IsLoading     = true;
        var result    = await _articleService.Post(article);
        var state     = Store.GetState<ClientState>();
        state.ExpandedSectionId = SectionId;
        Store.SetState(state);
        if (!result.HasErrorsOrResultIsNull())
        {
            state.Articles = state.Articles.Where(e => e.Id != article.Id).ToImmutableList();
            state.Articles = state.Articles.Add(article);
            Store.SetState(state);
            NavigationManager.NavigateTo($"/Section/{SectionId}/Article/{ArticleId}");
        }

        IsLoading = false;

        state.UpdateNavMenu();
    }

    private void OnCancel()
    {
        NavigationManager.NavigateTo($"/Section/{SectionId}/Article/{ArticleId}");
    }

    protected override void OnInitialized()
    {
        _articleService = new ArticleService(NavigationManager);
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Article == null)
        {
            // populate all the articles for the section so the nav bar updates properly
            var state                    = Store.GetState<ClientState>();
            state.ExpandedSectionId      = SectionId;
            state.SidebarLoadingArticles = true;
            var result                   = await _articleService.Index(SectionId);
            state.Articles               = state.Articles.Where(e => e.SectionId != SectionId).ToImmutableList();
            state.Articles               = state.Articles.AddRange(result.ResultObject);
            state.SidebarLoadingArticles = false;
            Store.SetState(state);
        }
    }

}
``` 

The create screen uses the same markup, but with slightly different initialization and persistence logic:

`Create.razor`
```razor
@page "/Section/{SectionId:long}/Create"
@inherits BlazorState.BlazorStateComponent

...

@code {

    [Parameter]
    public long SectionId { get; set; }

    private ArticleDto Article = new ArticleDto();

    [Inject]
    private NavigationManager NavigationManager { get; set; }
    private ArticleService    _articleService;

    private bool IsLoading { get; set; }

    private async Task OnSave(ArticleDto article)
    {
        IsLoading  = true;
        var state  = Store.GetState<ClientState>();
        var result = await _articleService.Put(Article);
        if (!result.HasErrorsOrResultIsNull())
        {
            Article = result.ResultObject;
            state.Articles = state.Articles.Where(e => e.Id != Article.Id).ToImmutableList();
            state.Articles = state.Articles.Add(result.ResultObject);
            Store.SetState(state);
            NavigationManager.NavigateTo($"/Section/{SectionId}/Article/{Article.Id}");
        }

        IsLoading = false;
        state.UpdateNavMenu();
    }

    private void OnCancel()
    {
        NavigationManager.NavigateTo("/");
    }

    private async Task PopulateArticlesForSection()
    {
        // populate all the articles for the section so the nav bar updates properly
        var state                    = Store.GetState<ClientState>();
        state.ExpandedSectionId      = SectionId;
        state.SidebarLoadingArticles = true;
        Store.SetState(state);
        var result     = await _articleService.Index(SectionId);
        state.Articles = state.Articles.Where(e => e.SectionId != SectionId).ToImmutableList();
        state.Articles = state.Articles.AddRange(result.ResultObject);
        state.SidebarLoadingArticles = false;
        Store.SetState(state);
        state.UpdateNavMenu();
    }

    protected override void OnInitialized()
    {
        _articleService = new ArticleService(NavigationManager);
    }

    protected override async Task OnParametersSetAsync()
    {
        var state = Store.GetState<ClientState>();
        state.ExpandedSectionId = SectionId;
        Store.SetState(state);
        await PopulateArticlesForSection();
    }

}
```

There you have basic create/read/update/delete functionality for Markdown content. There are a number of enhancements
which are not outlined here, but you can find the full code [here](https://github.com/andCulture/BlazorCMS).

# Analytics

So, what impact on download size and page load speed does shipping a WASM-compiled .NET runtime to the browser have?
Let's find out.
 
 Using Chrome developer tools, I analyzed the network traffic created by visiting the website. I ran 5 page loads
 ignoring cache (using Chrome's "Empty Cache and Hard Reload") and 5 normal page loads (with caches).
 
 |               | Data Transferred | Resources |
 | ------------- | ---------------- | --------- |
 | Without Cache | 7.5 MB           | 17.7 MB   |
 | With Cache    | 34.5 KB          | 17.7 MB   |
 
 These results show that, at least with an application this small scale, the browser is able to cache about 99%
 of the data it needs to download in order to run the application. Based on these numbers, the WASM .NET runtime
 most likely clocks in somewhere around 7.25 MB when published for production.
 
 Most users won't care about these numbers, though. Let's take a look at the numbers they will care about;
 the time it takes to load the page.
 
 "Load Time" is the time to get a response from the server.
 
 "`DOMContentLoaded` Time" is the time for all HTML markup to be served, and the `DOMContentLoaded` Javascript event to be triggered.
 
 "Finish Time" is the time for the page to be fully loaded and become interactive.
 
 
 |               | Average Load Time (seconds) | Average `DOMContentLoaded` Time (seconds) | Average Finish Time (seconds) |
 | ------------- | --------------------------- | ----------------------------------------- | ----------------------------- |
 | Without Cache | 0.672                       | 0.6614                                    | 2.258                         |
 | With Cache    | 0.391                       | 0.3798                                    | 1.682                         |
 
