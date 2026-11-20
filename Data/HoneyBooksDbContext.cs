using Microsoft.EntityFrameworkCore;

namespace HoneyBooksStudio.Data;

public sealed class HoneyBooksDbContext(DbContextOptions<HoneyBooksDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<CustomerOrder> Orders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Id).HasMaxLength(80);
            entity.Property(book => book.Title).HasMaxLength(180).IsRequired();
            entity.Property(book => book.Author).HasMaxLength(140).IsRequired();
            entity.Property(book => book.Genre).HasMaxLength(80).IsRequired();
            entity.Property(book => book.Price).HasPrecision(10, 2);
            entity.Property(book => book.Cover).HasMaxLength(240).IsRequired();
            entity.Property(book => book.Tone).HasMaxLength(40);
            entity.Property(book => book.Description).HasMaxLength(1200);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(category => category.Slug).IsUnique();
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(link => new { link.BookId, link.CategoryId });
            entity.HasOne(link => link.Book).WithMany(book => book.BookCategories).HasForeignKey(link => link.BookId);
            entity.HasOne(link => link.Category).WithMany(category => category.BookCategories).HasForeignKey(link => link.CategoryId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(120).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(254).IsRequired();
            entity.Property(customer => customer.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(customer => customer.Email).IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.CustomerId, item.BookId }).IsUnique();
            entity.HasOne(item => item.Customer).WithMany(customer => customer.CartItems).HasForeignKey(item => item.CustomerId);
            entity.HasOne(item => item.Book).WithMany().HasForeignKey(item => item.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Total).HasPrecision(10, 2);
            entity.Property(order => order.Status).HasMaxLength(40).IsRequired();
            entity.HasOne(order => order.Customer).WithMany(customer => customer.Orders).HasForeignKey(order => order.CustomerId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.BookTitle).HasMaxLength(180).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.HasOne(item => item.Order).WithMany(order => order.Items).HasForeignKey(item => item.CustomerOrderId);
            entity.HasOne(item => item.Book).WithMany().HasForeignKey(item => item.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Name).HasMaxLength(120).IsRequired();
            entity.Property(message => message.Email).HasMaxLength(254).IsRequired();
            entity.Property(message => message.Message).HasMaxLength(4000).IsRequired();
        });
    }
}

public sealed class Book
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public decimal Price { get; set; }
    public string Cover { get; set; } = "";
    public string Tone { get; set; } = "";
    public string Description { get; set; } = "";
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}

public sealed class BookCategory
{
    public string BookId { get; set; } = "";
    public Book Book { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
}

public sealed class CartItem
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string BookId { get; set; } = "";
    public Book Book { get; set; } = null!;
    public int Quantity { get; set; }
}

public sealed class CustomerOrder
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public string Status { get; set; } = "Placed";
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public sealed class OrderItem
{
    public int Id { get; set; }
    public int CustomerOrderId { get; set; }
    public CustomerOrder Order { get; set; } = null!;
    public string BookId { get; set; } = "";
    public Book Book { get; set; } = null!;
    public string BookTitle { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public sealed class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}
