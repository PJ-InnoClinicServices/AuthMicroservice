using AuthMicroservice.BusinessLogic.Extensions;
using AuthMicroservice.DataAccess;
using Microsoft.EntityFrameworkCore;
using Supabase;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextService(builder.Configuration);

builder.Services.AddServices();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRepositories();

builder.Services.AddCustomRateLimiter(builder.Configuration);

builder.Services.AddSwagger();

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddIdentityServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddDbContextService(builder.Configuration);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Container")
{
    app.UseSwagger(options => options.OpenApiVersion =
        Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRateLimiter();

app.Run();