using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageApi.Data;

public class ImageDbContext : IdentityDbContext<ApplicationUser>
{
    public ImageDbContext(DbContextOptions<ImageDbContext> options)
        : base(options)
    {
    }

    public DbSet<ImageInfo> Images { get; set; } = null!;
}

public class ImageInfo
{
    public Guid Id { get; set; }
    public byte[] Hash { get; set; }
    public Uri FileLocation { get; set; }
    public Guid UploaderId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}