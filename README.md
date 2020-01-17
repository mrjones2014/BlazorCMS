# BlazorCMS
A (very) simple CMS to evaluate Microsoft's Blazor WASM framework.

In this tutorial, we'll take a look at Microsoft's Blazor framework, with the WebAssembly option.
We'll build a (very) simple CMS, with a .NET Core hosted backend.

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

        public string Title     { get; set; }
        public string Body      { get; set; }
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
    [ResponseCache(CacheProfileName = "Never")]
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

        #region POST

        [HttpPost]
        public IActionResult Post([FromBody] SectionDto section)
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

        #endregion POST

        #region PATCH

        [HttpPatch]
        public IActionResult Patch([FromBody] SectionDto section)
        {
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

        #endregion PATCH

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
