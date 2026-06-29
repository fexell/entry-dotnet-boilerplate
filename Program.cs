using Entry.Auth.Extensions;
using Entry.Auth.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAppDbContext(builder.Configuration)
    .AddAppIdentity()
    .AddJwtAuthentication(builder.Configuration)
    .AddAppServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register middleware as scoped
builder.Services.AddScoped<SilentRefreshMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();

    var requiresAuth = endpoint?.Metadata
        .GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;

    if (requiresAuth)
    {
        var middleware = new SilentRefreshMiddleware();
        await middleware.InvokeAsync(context);
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
