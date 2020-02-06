# Marshmallow.HotChocolate

<img src="https://raw.githubusercontent.com/lucasphi/Marshmallow.HotChocolate/v1.1/Marshmallow.png" width="96">

Marshmallow is a [Hot Chocolate](https://hotchocolate.io/) addon that creates Linq projection expressions, based on you GraphQL query, that can be used with ORMs like Entity Framework.

In other words, Marshmallow will convert you GraphQL query into a Lambda Expression:

GraphQL query
```
{
   getClients {
      name
      age,
      contacts {
          number
      }
   }
}
```
Lambda expression
```
a => new {
    Name = a.Name,
    Age = a.Age,
    Contacts => a.Contacts.Select(b => new {
      Number = b.Number
    }
}
```

### Getting Started
Install the package using Nuget.org [Marshmallow Nuget](https://www.nuget.org/packages/Marshmallow.HotChocolate/)

### Using Marshmallow
To start using Marshmallow, add a request interceptor to *Hot Chocolate* and register the QueryProjection interface.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddGraphQL(...);
    
    services.AddQueryRequestInterceptor((httpContext, requestBuilder, cancellationToken) =>
    {
        httpContext.SetRequestQuery(requestBuilder);
        return Task.CompletedTask;
    });
    services.AddQueryProjection();
}
```

You can inject *IQueryProjection* interface on your Queries and call *CreateExpression* to create you projection. After the data is loaded, use the method *CreateScheme* or your favorite mapping package to convert the result into your scheme.

```
public class ClientsQuery
{
    private readonly IClientsRepository _clientsRepository;
    private readonly IQueryProjection _queryProjection;

    public ClientsQuery(
        IQueryProjection queryProjection,
        IClientsRepository clientsRepository)
    {
        _clientsRepository = clientsRepository;
        _queryProjection = queryProjection;
    }

    [GraphQLDescription("List all clients")]
    [GraphQLName("getClients")]
    public List<Client> GetClients()
    {
        // Creates the expression
        Expression<Func<Client, object>> projection = _queryProjection.CreateExpression<Client>();
        
        // Run the expression through your ORM
        var result = _clientsRepository.Search(projection);
        
        // Convert the result
        return _queryProjection.CreateScheme<List<Client>>(result);
    }
}
```

#### JoinAttribute

The JoinAttribute can be used to decorate the properties of your scheme, to flatten a 1 to 1 relationship.

The following scheme
```
public class MyClassScheme
{
   public string Fow { get; set; }

   [Join(nameof(MyClassEntity.InnerClassData)]
   public string Foo { get; set; }
   
   [Join(nameof(MyClassEntity.InnerClassData)]
   public string Bar { get; set; }
}
```
can be used to load the entity below
```
public class MyClassEntity
{
   public string Fow { get; set; }

   public InnerClassEntity { get; set; }
}

public class InnerClassEntity
{
   public string Foo { get; set; }
   
   public string Bar { get; set; }
}
```
