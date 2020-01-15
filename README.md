# Marshmallow.HotChocolate

Marshmallow is a [Hot Chocolate](https://hotchocolate.io/) addon that creates projection expressions, that can be used with ORMs like Entity Framework, based on you GraphQL query.

Marshmallow will convert you GraphQL query into a Lambda Expression, like:

```
{
   getClients {
      name
      age
   }
}
```
into
```
a => new {
    a.Name,
    a.Age
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

You can inject *IQueryProjection* interface on your Queries and call *CreateExpression* to create you projection.

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
        Expression<Func<Client, object>> projection = _queryProjection.CreateExpression<Client>();
        var result = _clientsRepository.Search(projection);
        return result;
    }
}
```
