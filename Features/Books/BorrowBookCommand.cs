using BookLibraryApi.Data;
using BookLibraryApi.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BookLibraryApi.Features.Books;

public record BorrowBookCommand(Guid BookId, Guid UserId) : IRequest<bool>;
public class BorrowBookHandler : IRequestHandler<BorrowBookCommand, bool>
{
    private readonly AppDbContext _db;
    public BorrowBookHandler(AppDbContext db) => _db = db;
    public async Task<bool> Handle(BorrowBookCommand request, CancellationToken
    ct)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id ==
        request.BookId, ct);
        if (book is null) return false;
        if (book.Status == BookStatus.Borrowed) return false;
        book.Status = BookStatus.Borrowed;
        book.UserId = request.UserId;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
