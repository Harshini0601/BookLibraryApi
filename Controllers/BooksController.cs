using System.Security.Claims;
using BookLibraryApi.Data;
using BookLibraryApi.DTOs;
using BookLibraryApi.Entities;
using BookLibraryApi.Features.Books;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BookLibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;
    public BooksController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? genre,
    [FromQuery] BookStatus? status)
    {
        IQueryable<Book> query = _db.Books.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(genre)) query = query.Where(b => b.Genre
        == genre);
        if (status.HasValue) query = query.Where(b => b.Status == status);
        var list = await query.OrderBy(b => b.Title).ToListAsync();
        return Ok(list);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookCreateDto dto)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Author = dto.Author,
            Genre = dto.Genre,
            Status = BookStatus.Available
        };
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _db.Books.FindAsync(id);
        return book is null ? NotFound() : Ok(book);
    }
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BookUpdateDto
    dto)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();
        book.Title = dto.Title;
        book.Author = dto.Author;
        book.Genre = dto.Genre;
        await _db.SaveChangesAsync();
        return NoContent();
    }
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();
        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    [Authorize]
    [HttpPost("{id:guid}/borrow")]
    public async Task<IActionResult> Borrow(Guid id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await _mediator.Send(new BorrowBookCommand(id, userId.Value));
        return result ? Ok(new { message = "Borrowed" }) : BadRequest(new
        {
            message = "Book not available"
        });
    }
    [Authorize]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await _mediator.Send(new ReturnBookCommand(id,
        userId.Value));
        return result ? Ok(new { message = "Returned" }) : BadRequest(new
        {
            message = "Cannot return"
        });
    }
    private Guid? GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}