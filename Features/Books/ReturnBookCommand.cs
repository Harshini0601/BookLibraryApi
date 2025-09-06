using BookLibraryApi.Data;
using BookLibraryApi.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BookLibraryApi.Features.Books;

public record ReturnBookCommand(Guid BookId, Guid UserId) : IRequest<bool>;
public class ReturnBookHandler : IRequestHandler<ReturnBookCommand, bool>
{
    private readonly AppDbContext _db;
    public ReturnBookHandler(AppDbContext db) => _db = db;
    public async Task<bool> Handle(ReturnBookCommand request, CancellationToken
    ct)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id ==
        request.BookId, ct);
        if (book is null) return false;
        // user can only return a book they borrowed
        if (book.Status != BookStatus.Borrowed || book.UserId !=
        request.UserId) return false;
        book.Status = BookStatus.Available;
        book.UserId = null;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}