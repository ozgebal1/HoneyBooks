using System.Security.Claims;
using System.Net.Mail;
using HoneyBooksStudio.Data;
using HoneyBooksStudio.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("HoneyBooks")
    ?? throw new InvalidOperationException("HoneyBooks bağlantı dizesi appsettings.json içinde tanımlı değil.");

builder.Services.AddDbContext<HoneyBooksDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
builder.Services.AddDataProtection().PersistKeysToFileSystem(
    new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "HoneyBooks.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect("/giris.html");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
var databaseReady = false;
try
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<HoneyBooksDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(db);
    databaseReady = true;
    app.Logger.LogInformation("HoneyBooksDb veritabanı hazır.");
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "SQL Server'a bağlanılamadı. SQL Server Express'i başlatıp uygulamayı yeniden çalıştırın.");
}

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var extension = Path.GetExtension(context.File.Name);
        if (extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".css", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Context.Response.Headers.Pragma = "no-cache";
            context.Context.Response.Headers.Expires = "0";
        }
    }
});
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.Equals("/api/health") && !databaseReady)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { message = "Veritabanına bağlanılamadı. SQL Server Express'i başlatıp siteyi yeniden çalıştırın." });
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { database = databaseReady ? "ready" : "unavailable" }));

app.MapGet("/api/books", async (HoneyBooksDbContext db) =>
{
    var books = await db.Books.AsNoTracking().Include(book => book.BookCategories).ThenInclude(link => link.Category).ToListAsync();
    return Results.Ok(books.Select(ToBookDto));
});

app.MapGet("/api/books/{id}", async (string id, HoneyBooksDbContext db) =>
{
    var book = await db.Books.AsNoTracking().Include(item => item.BookCategories).ThenInclude(link => link.Category)
        .FirstOrDefaultAsync(item => item.Id == id);
    return book is null ? Results.NotFound() : Results.Ok(ToBookDto(book));
});

app.MapGet("/api/categories", async (HoneyBooksDbContext db) =>
{
    var categories = await db.Categories.AsNoTracking()
        .OrderBy(category => category.Id)
        .Select(category => new CategoryDto(category.Id, category.Name, category.Slug, category.BookCategories.Count))
        .ToListAsync();
    return Results.Ok(categories);
});

app.MapPost("/api/contact", async (ContactRequest request, HoneyBooksDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120 ||
        !IsValidEmail(request.Email) || string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length > 4000)
        return Results.BadRequest(new { message = "Adını, geçerli e-posta adresini ve en fazla 4000 karakterlik mesajını gir." });

    db.ContactMessages.Add(new ContactMessage
    {
        Name = request.Name.Trim(),
        Email = request.Email.Trim(),
        Message = request.Message.Trim(),
        CreatedAtUtc = DateTime.UtcNow
    });
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Mesajın veritabanına kaydedildi." });
}).AllowAnonymous();

app.MapPost("/api/auth/register", async (RegisterRequest request, HoneyBooksDbContext db,
    IPasswordHasher<Customer> passwordHasher, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120 ||
        !IsValidEmail(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        return Results.BadRequest(new { message = "Adını, geçerli e-posta adresini ve en az 8 karakterli şifreni gir." });

    var email = request.Email.Trim().ToLowerInvariant();
    if (await db.Customers.AnyAsync(customer => customer.Email == email))
        return Results.Conflict(new { message = "Bu e-posta adresiyle zaten bir hesap var. Giriş yapabilirsin." });

    var customer = new Customer { Name = request.Name.Trim(), Email = email, CreatedAtUtc = DateTime.UtcNow };
    customer.PasswordHash = passwordHasher.HashPassword(customer, request.Password);
    db.Customers.Add(customer);
    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { message = "Bu e-posta adresiyle zaten bir hesap var. Giriş yapabilirsin." });
    }

    await SignInCustomer(http, customer);
    return Results.Created("/api/auth/me", ToUserDto(customer));
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (LoginRequest request, HoneyBooksDbContext db,
    IPasswordHasher<Customer> passwordHasher, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { message = "E-posta adresini ve şifreni gir." });

    var email = request.Email.Trim().ToLowerInvariant();
    var customer = await db.Customers.FirstOrDefaultAsync(item => item.Email == email);
    if (customer is null || passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    await SignInCustomer(http, customer);
    return Results.Ok(ToUserDto(customer));
}).AllowAnonymous();

app.MapPost("/api/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
{
    var id = GetCustomerId(user);
    var name = user.Identity?.Name ?? "";
    var email = user.FindFirstValue(ClaimTypes.Email) ?? "";
    return id is null ? Results.Unauthorized() : Results.Ok(new UserDto(id.Value, name, email));
}).RequireAuthorization();

app.MapGet("/api/cart", async (ClaimsPrincipal user, HoneyBooksDbContext db) =>
{
    var customerId = GetCustomerId(user)!.Value;
    var items = await db.CartItems.AsNoTracking().Include(item => item.Book)
        .Where(item => item.CustomerId == customerId).OrderBy(item => item.Id).ToListAsync();
    return Results.Ok(items.Select(item => new CartLineDto(item.BookId, item.Book.Title, item.Book.Author,
        item.Book.Cover, item.Book.Price, item.Quantity)));
}).RequireAuthorization();

app.MapPut("/api/cart/{bookId}", async (string bookId, CartUpdateRequest request, ClaimsPrincipal user, HoneyBooksDbContext db) =>
{
    if (request.Quantity is < 0 or > 99)
        return Results.BadRequest(new { message = "Adet 0 ile 99 arasında olmalı." });

    var customerId = GetCustomerId(user)!.Value;
    var book = await db.Books.FindAsync(bookId);
    if (book is null) return Results.NotFound(new { message = "Kitap bulunamadı." });

    var item = await db.CartItems.FirstOrDefaultAsync(row => row.CustomerId == customerId && row.BookId == bookId);
    if (request.Quantity == 0)
    {
        if (item is not null) db.CartItems.Remove(item);
    }
    else if (item is null)
    {
        db.CartItems.Add(new CartItem { CustomerId = customerId, BookId = bookId, Quantity = request.Quantity });
    }
    else
    {
        item.Quantity = request.Quantity;
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/api/cart/{bookId}", async (string bookId, ClaimsPrincipal user, HoneyBooksDbContext db) =>
{
    var customerId = GetCustomerId(user)!.Value;
    var item = await db.CartItems.FirstOrDefaultAsync(row => row.CustomerId == customerId && row.BookId == bookId);
    if (item is not null)
    {
        db.CartItems.Remove(item);
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/api/orders", async (ClaimsPrincipal user, HoneyBooksDbContext db) =>
{
    var customerId = GetCustomerId(user)!.Value;
    var cart = await db.CartItems.Include(item => item.Book).Where(item => item.CustomerId == customerId).ToListAsync();
    if (cart.Count == 0) return Results.BadRequest(new { message = "Sepetin boş." });

    var order = new CustomerOrder
    {
        CustomerId = customerId,
        CreatedAtUtc = DateTime.UtcNow,
        Status = "Ödeme bekleniyor",
        Items = cart.Select(item => new OrderItem
        {
            BookId = item.BookId,
            BookTitle = item.Book.Title,
            UnitPrice = item.Book.Price,
            Quantity = item.Quantity
        }).ToList()
    };
    order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);
    db.Orders.Add(order);
    db.CartItems.RemoveRange(cart);
    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}", ToOrderDto(order));
}).RequireAuthorization();

app.MapGet("/api/orders", async (ClaimsPrincipal user, HoneyBooksDbContext db) =>
{
    var customerId = GetCustomerId(user)!.Value;
    var orders = await db.Orders.AsNoTracking().Include(order => order.Items)
        .Where(order => order.CustomerId == customerId).OrderByDescending(order => order.CreatedAtUtc).ToListAsync();
    return Results.Ok(orders.Select(ToOrderDto));
}).RequireAuthorization();

app.Run();

static BookDto ToBookDto(Book book) => new(book.Id, book.Title, book.Author, book.Genre, book.Price,
    book.Cover, book.Tone, book.Description, book.BookCategories.Select(link => link.Category.Name).ToArray());

static UserDto ToUserDto(Customer customer) => new(customer.Id, customer.Name, customer.Email);

static OrderDto ToOrderDto(CustomerOrder order) => new(order.Id, order.CreatedAtUtc, order.Status, order.Total,
    order.Items.Select(item => new OrderItemDto(item.BookId, item.BookTitle, item.UnitPrice, item.Quantity)).ToArray());

static int? GetCustomerId(ClaimsPrincipal principal) =>
    int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

static bool IsValidEmail(string? value)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length > 254) return false;
    try { return new MailAddress(value.Trim()).Address.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase); }
    catch (FormatException) { return false; }
}

static async Task SignInCustomer(HttpContext http, Customer customer)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
        new(ClaimTypes.Name, customer.Name),
        new(ClaimTypes.Email, customer.Email)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
}
