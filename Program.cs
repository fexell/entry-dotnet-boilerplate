using Entry.Auth.Extensions;
using Entry.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAppDbContext(builder.Configuration)
    .AddAppIdentity()
    .AddJwtAuthentication(builder.Configuration)
    .AddAppServices()
    .AddAppAuthorization();

builder.Services.AddHttpClient<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailVerificationRefreshService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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