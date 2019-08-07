## Angular Tutorial - Part I

### About this Tutorial

In this tutorial series, you will build an application that is used to manage a list of books & their authors. **Angular** will be used as the UI framework and **MongoDB** will be used as the database provider.

This is the first part of the ASP.NET Core MVC tutorial series. See all parts:

- **Part I: Create the project and a book list page (this tutorial)**
- [Part II: Create, Update and Delete books](Part-II.md)

You can access to the **source code** of the application from the GitHub repository (TOD: link).

### Creating the Project

Create a new project named `Acme.BookStore` by selecting the Angular as the UI framework and MongoDB as the database provider, create the database and run the application by following the [Getting Started document](../../Getting-Started-Angular-Template.md).

### Solution Structure (Backend)

This is how the layered solution structure looks after it's created:

![bookstore-backend-solution](images\bookstore-backend-solution-v2.png)

> You can see the [Application template document](../../Startup-Templates/Application.md) to understand the solution structure in details. However, you will understand the basics with this tutorial.

### Create the Book Entity

Domain layer in the startup template is separated into two projects:

- `Acme.BookStore.Domain` contains your [entities](../../Entities.md), [domain services](../../Domain-Services.md) and other core domain objects.
- `Acme.BookStore.Domain.Shared` contains constants, enums or other domain related objects those can be shared with clients.

Define [entities](../../Entities.md) in the **domain layer** (`Acme.BookStore.Domain` project) of the solution. The main entity of the application is the `Book`. Create a class, named `Book`, in the `Acme.BookStore.Domain` project as shown below:

```C#
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.BookStore
{
    public class Book : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }

        public BookType Type { get; set; }

        public DateTime PublishDate { get; set; }

        public float Price { get; set; }
    }
}
```

- ABP has two fundamental base classes for entities: `AggregateRoot` and `Entity`. **Aggregate Root** is one of the **Domain Driven Design (DDD)** concepts. See [entity document](../../Entities.md) for details and best practices.
- `Book` entity inherits `AuditedAggregateRoot` which adds some auditing properties (`CreationTime`, `CreatorId`, `LastModificationTime`... etc.) on top of the `AggregateRoot` class.
- `Guid` is the **primary key type** of the `Book` entity.

#### BookType Enum

Define the `BookType` enum in the `Acme.BookStore.Domain.Shared` project:

```C#
namespace Acme.BookStore
{
    public enum BookType
    {
        Undefined,
        Adventure,
        Biography,
        Dystopia,
        Fantastic,
        Horror,
        Science,
        ScienceFiction,
        Poetry
    }
}
```

#### Add Book Entity to Your DbContext

Add a `IMongoCollection` property to the `BookStoreMongoDbContext` inside the `Acme.BookStore.MongoDB` project:

````csharp
public class BookStoreMongoDbContext : AbpMongoDbContext
{
    public IMongoCollection<Book> Books => Collection<Book>();
    ...
}
````

#### Add Seed (Sample) Data

This section is optional, but it would be good to have an initial data in the database in the first run. ABP provides a [data seed system](../../Data-Seeding.md). Create a class deriving from the `IDataSeedContributor` in the `.Domain` project:

````csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.BookStore
{
    public class BookStoreDataSeederContributor
        : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Book, Guid> _bookRepository;

        public BookStoreDataSeederContributor(IRepository<Book, Guid> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _bookRepository.GetCountAsync() > 0)
            {
                return;
            }

            await _bookRepository.InsertAsync(
                new Book
                {
                    Name = "1984",
                    Type = BookType.Dystopia,
                    PublishDate = new DateTime(1949, 6, 8),
                    Price = 19.84f
                }
            );

            await _bookRepository.InsertAsync(
                new Book
                {
                    Name = "The Hitchhiker's Guide to the Galaxy",
                    Type = BookType.ScienceFiction,
                    PublishDate = new DateTime(1995, 9, 27),
                    Price = 42.0f
                }
            );
        }
    }
}

````

`BookStoreDataSeederContributor` simply inserts two books into database if there is no book added before. ABP automatically discovers and executes this class when you seed the database by running the `Acme.BookStore.DbMigrator` project.

### Create the Application Service

The next step is to create an [application service](../../Application-Services.md) to manage (create, list, update, delete...) the books. Application layer in the startup template is separated into two projects:

- `Acme.BookStore.Application.Contracts` mainly contains your DTOs and application service interfaces.
- `Acme.BookStore.Application` contains the implementations of your application services.

#### BookDto

Create a DTO class named `BookDto` into the `Acme.BookStore.Application.Contracts` project:

```C#
using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore
{
    public class BookDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }

        public BookType Type { get; set; }

        public DateTime PublishDate { get; set; }

        public float Price { get; set; }
    }
}
```

- **DTO** classes are used to **transfer data** between the *presentation layer* and the *application layer*. See the [Data Transfer Objects document](../../Data-Transfer-Objects.md) for more details.
- `BookDto` is used to transfer book data to the presentation layer in order to show the book information on the UI.
- `BookDto` is derived from the `AuditedEntityDto<Guid>` which has audit properties just like the `Book` class defined above.

It will be needed to convert `Book` entities to `BookDto` objects while returning books to the presentation layer. [AutoMapper](https://automapper.org) library can automate this conversion when you define the proper mapping. Startup template comes with AutoMapper configured, so you can just define the mapping in the `BookStoreApplicationAutoMapperProfile` class in the `Acme.BookStore.Application` project:

```csharp
using AutoMapper;

namespace Acme.BookStore
{
    public class BookStoreApplicationAutoMapperProfile : Profile
    {
        public BookStoreApplicationAutoMapperProfile()
        {
            CreateMap<Book, BookDto>();
        }
    }
}
```

#### CreateUpdateBookDto

Create a DTO class named `CreateUpdateBookDto` into the `Acme.BookStore.Application.Contracts` project:

```c#
using System;
using System.ComponentModel.DataAnnotations;

namespace Acme.BookStore
{
    public class CreateUpdateBookDto
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        public BookType Type { get; set; } = BookType.Undefined;

        [Required]
        public DateTime PublishDate { get; set; }

        [Required]
        public float Price { get; set; }
    }
}
```

- This DTO class is used to get book information from the user interface while creating or updating a book.
- It defines data annotation attributes (like `[Required]`) to define validations for the properties. DTOs are [automatically validated](../../Validation.md) by the ABP framework.

Next, add a mapping in `BookStoreApplicationAutoMapperProfile` from the `CreateUpdateBookDto` object to the `Book` entity:

```csharp
CreateMap<CreateUpdateBookDto, Book>();
```

#### IBookAppService

Define an interface named `IBookAppService` in the `Acme.BookStore.Application.Contracts` project:

```C#
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.BookStore
{
    public interface IBookAppService : 
        ICrudAppService< //Defines CRUD methods
            BookDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting on getting a list of books
            CreateUpdateBookDto, //Used to create a new book
            CreateUpdateBookDto> //Used to update a book
    {

    }
}
```

- Defining interfaces for application services is <u>not required</u> by the framework. However, it's suggested as a best practice.
- `ICrudAppService` defines common **CRUD** methods: `GetAsync`, `GetListAsync`, `CreateAsync`, `UpdateAsync` and `DeleteAsync`. It's not required to extend it. Instead, you could inherit from the empty `IApplicationService` interface and define your own methods manually.
- There are some variations of the `ICrudAppService` where you can use separated DTOs for each method.

#### BookAppService

Implement the `IBookAppService` as named `BookAppService` in the `Acme.BookStore.Application` project:

```C#
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Acme.BookStore
{
    public class BookAppService : 
        CrudAppService<Book, BookDto, Guid, PagedAndSortedResultRequestDto,
                            CreateUpdateBookDto, CreateUpdateBookDto>,
        IBookAppService
    {
        public BookAppService(IRepository<Book, Guid> repository) 
            : base(repository)
        {

        }
    }
}
```

- `BookAppService` is derived from `CrudAppService<...>` which implements all the CRUD methods defined above.
- `BookAppService` injects `IRepository<Book, Guid>` which is the default repository for the `Book` entity. ABP automatically creates default repositories for each aggregate root (or entity). See the [repository document](../../Repositories.md).
- `BookAppService` uses `IObjectMapper` to convert `Book` objects to `BookDto` objects and `CreateUpdateBookDto` objects to `Book` objects. The Startup template uses the [AutoMapper](http://automapper.org/) library as the object mapping provider. You defined the mappings before, so it will work as expected.

### Auto API Controllers

You normally create **Controllers** to expose application services as **HTTP API** endpoints. Thus allowing browser or 3rd-party clients to call them via AJAX. ABP can [**automagically**](../../AspNetCore/Auto-API-Controllers.md) configures your application services as MVC API Controllers by convention.

#### Swagger UI

The startup template is configured to run the [swagger UI](https://swagger.io/tools/swagger-ui/) using the [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) library. Run the `Acme.BookStore.HttpApi.Host` application and enter `https://localhost:XXXX/swagger/` (replace XXXX by your own port) as URL on your browser.

You will see some built-in service endpoints as well as the `Book` service and its REST-style endpoints:

![bookstore-swagger](images/bookstore-swagger-api.png)

Swagger has a nice UI to test APIs. You can try to execute the `[GET] /api/app/book` API to get a list of books.

### Create the Books Page

It's time to create something visible and usable!