using System;
using System.Collections.Generic;
namespace BookLibraryApi.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Book>? BorrowedBooks { get; set; }
}