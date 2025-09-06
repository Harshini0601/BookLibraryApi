namespace BookLibraryApi.DTOs;
public record BookCreateDto(string Title, string Author, string Genre);
public record BookUpdateDto(string Title, string Author, string Genre);