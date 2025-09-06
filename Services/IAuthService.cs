using BookLibraryApi.DTOs;
using BookLibraryApi.Entities;

namespace BookLibraryApi.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto);
}