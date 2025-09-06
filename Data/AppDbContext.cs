using BookLibraryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasMany(u=>u.BorrowedBooks).WithOne(b=>b.User).HasForeignKey(b=>b.UserId).OnDelete(DeleteBehavior.SetNull);
    }
        
}