using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Presentation.Infrastructure.Authentication;
using Presentation.Infrastructure.DependencyInjection;
using Presentation.Infrastructure.Middleware;
using Presentation.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder);
builder.AddAuthentication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "YaEvents");
    //    c.OAuthClientId("44f81bee-1f64-4a4d-bf93-c452b2e98b51");
    //});
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.AddEndpoints();

app.Run();
