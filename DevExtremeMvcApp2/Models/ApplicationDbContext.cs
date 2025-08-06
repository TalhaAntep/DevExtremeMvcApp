using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using DevExtremeMvcApp2.Models;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<ShapeInput> ShapeInputs { get; set; }

    public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false) { }

    public static ApplicationDbContext Create()
    {
        return new ApplicationDbContext();
    }
}
