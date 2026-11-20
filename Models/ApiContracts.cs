namespace HoneyBooksStudio.Models;

public sealed record BookDto(string Id, string Title, string Author, string Genre, decimal Price, string Cover, string Tone, string Description, IReadOnlyList<string> Categories);
public sealed record CategoryDto(int Id, string Name, string Slug, int BookCount);
public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UserDto(int Id, string Name, string Email);
public sealed record CartUpdateRequest(int Quantity);
public sealed record CartLineDto(string BookId, string Title, string Author, string Cover, decimal Price, int Quantity);
public sealed record OrderItemDto(string BookId, string Title, decimal UnitPrice, int Quantity);
public sealed record OrderDto(int Id, DateTime CreatedAtUtc, string Status, decimal Total, IReadOnlyList<OrderItemDto> Items);
public sealed record ContactRequest(string Name, string Email, string Message);
