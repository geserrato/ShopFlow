// ShopFlow.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Registrar CatalogoService — raíz del catálogo B2B de ShopFlow
var catalogo = builder.AddProject<Projects.CatalogoService>("catalogo")
    .WithHttpEndpoint(port: 5001);

// OrdenesService depende de CatalogoService (lo consultará en Lab 3)
var ordenes = builder.AddProject<Projects.OrdenesService>("ordenes")
    .WithHttpEndpoint(port: 5002)
    .WithReference(catalogo);

// NotificacionesService reacciona a eventos de Ordenes (Lab 5)
builder.AddProject<Projects.NotificacionesService>("notificaciones")
    .WithHttpEndpoint(port: 5003)
    .WithReference(ordenes);

await builder.Build().RunAsync();