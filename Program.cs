using Entry.Auth.Extensions;
using Entry.Auth.Services;
using Entry.Auth.Filters;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services
    .AddAppDbContext(builder.Configuration)
    .AddAppIdentity()
    .AddJwtAuthentication(builder.Configuration)
    .AddAppServices()
    .AddAppAuthorization();

builder.Services.AddHttpClient<IEmailService, EmailService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<EmailVerificationRefreshService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
