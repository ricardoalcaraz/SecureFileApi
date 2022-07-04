using ImageApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageApiIntegrationTests;

internal class ImageApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.Configure(app =>
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<ImageDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
        
        base.ConfigureWebHost(builder);
    }
    
    
}