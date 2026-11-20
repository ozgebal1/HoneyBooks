using Microsoft.EntityFrameworkCore;

namespace HoneyBooksStudio.Data;

public static class DatabaseSeeder
{
    private static readonly (string Name, string Slug)[] CategorySeeds =
    [
        ("Korku", "korku"), ("Dram", "dram"), ("Polisiye", "polisiye"), ("Fantastik", "fantastik"),
        ("Roman", "roman"), ("Hikâye", "hikaye"), ("Şiir", "siir"), ("Deneme", "deneme"),
        ("Biyografi", "biyografi"), ("Bilimsel", "bilimsel"), ("Felsefi", "felsefi"), ("Tarih", "tarih"),
        ("Çocuk kitapları", "cocuk-kitaplari"), ("Tiyatro", "tiyatro"), ("Mektup", "mektup"), ("Anı", "ani"),
        ("Eleştiri", "elestiri"), ("Suç", "suc"), ("Aşk-romantik", "ask-romantik"), ("Macera", "macera"),
        ("Psikoloji", "psikoloji"), ("Mitoloji", "mitoloji"), ("Masal", "masal"), ("Manga", "manga"),
        ("Çizgi roman", "cizgi-roman"), ("Sanat", "sanat"), ("Dini", "dini"), ("Karikatür", "karikatur"),
        ("Gezi", "gezi"), ("Çocuk hikâyesi", "cocuk-hikayesi")
    ];

    private static readonly Book[] BookSeeds =
    [
        new() { Id = "noraliya", Title = "Matmazel Noraliya'nın Koltuğu", Author = "Peyami Safa", Genre = "Roman", Price = 297.50m, Cover = "/images/cover-noraliya.jpg", Tone = "plum", Description = "Gizemli bir pansiyonda başlayan, insanın iç dünyasına ve inançlarına uzanan bir roman." },
        new() { Id = "yaprak", Title = "Yaprak Dökümü", Author = "Reşat Nuri Güntekin", Genre = "Klasik", Price = 232.19m, Cover = "/images/cover-yaprak.jpg", Tone = "sage", Description = "Bir ailenin değişen zaman karşısında yaşadığı kırılmaları anlatan Türk edebiyatı klasiği." },
        new() { Id = "dexter", Title = "Dexter Karanlıkta", Author = "Jeff Lindsay", Genre = "Polisiye", Price = 288.75m, Cover = "/images/cover-dexter.jpg", Tone = "red", Description = "Karanlık sırlar ve beklenmedik izlerle ilerleyen polisiye bir macera." },
        new() { Id = "kucuk-prens", Title = "Küçük Prens", Author = "Antoine de Saint-Exupéry", Genre = "Masal", Price = 94.50m, Cover = "/images/cover-prens.jpg", Tone = "blue", Description = "Dostluk, merak ve büyümek üzerine her yaştan okura seslenen bir klasik." },
        new() { Id = "uc-buyuk-usta", Title = "Üç Büyük Usta", Author = "Stefan Zweig", Genre = "Biyografi", Price = 119, Cover = "/images/cover-ustalar.webp", Tone = "ochre", Description = "Üç büyük yazarın dünyasına edebiyat üzerinden bir bakış." },
        new() { Id = "dogu-ekspresi", Title = "Doğu Ekspresinde Cinayet", Author = "Agatha Christie", Genre = "Polisiye", Price = 340, Cover = "/images/cover-ekspres.jpg", Tone = "navy", Description = "Karlı bir gecede, tren yolculuğunu saran klasik bir cinayet gizemi." },
        new() { Id = "su-cilgin-turkler", Title = "Şu Çılgın Türkler", Author = "Turgut Özakman", Genre = "Tarih", Price = 546.58m, Cover = "/images/cover-turkler.jpg", Tone = "rust", Description = "Kurtuluş Savaşı dönemini roman tadında anlatan tarihî bir eser." },
        new() { Id = "nutuk", Title = "Nutuk", Author = "Mustafa Kemal Atatürk", Genre = "Tarih", Price = 713.90m, Cover = "/images/cover-nutuk.webp", Tone = "cream", Description = "Millî Mücadele ve Cumhuriyetin kuruluş yıllarına birinci elden tanıklık." },
        new() { Id = "yildiz", Title = "Aynı Yıldızın Altında", Author = "John Green", Genre = "Roman", Price = 399, Cover = "/images/cover-yildiz.jpg", Tone = "teal", Description = "Gençlik, sevgi ve umut üzerine duygusal bir roman." },
        new() { Id = "kilitli-kapi", Title = "Kilitli Kapı", Author = "Freida McFadden", Genre = "Korku · Gerilim", Price = 210, Cover = "/images/cover-kilitli-kapi.jpg", Tone = "black", Description = "Geçmişin karanlık sırları, Nora’nın sakin hayatını yeniden tehdit etmeye başlar." },
        new() { Id = "ruya-ev", Title = "Rüya Ev", Author = "T. M. Logan", Genre = "Korku · Gerilim", Price = 300, Cover = "/images/cover-ruya-ev.jpg", Tone = "violet", Description = "Yeni evlerinde gizli bir oda ve karanlık bir geçmiş bulan bir ailenin gerilim dolu hikâyesi." },
        new() { Id = "psikiyatrist", Title = "Psikiyatrist", Author = "Wulf Dorn", Genre = "Korku · Gerilim", Price = 425, Cover = "/images/cover-psikiyatrist.jpg", Tone = "green", Description = "Bir hastanın kayboluşu, psikiyatrist Ellen Roth’u korku ve sırlarla dolu bir arayışa sürükler." },
        new() { Id = "vampir-kani", Title = "Vampir Kanı", Author = "Florence Marryat", Genre = "Korku · Gerilim", Price = 225, Cover = "/images/cover-vampir-kani.jpg", Tone = "wine", Description = "Florence Marryat’ın gotik klasiğinde Harriet Brandt, vampir doğasıyla ve sevdiği insanlara etkisiyle yüzleşir." },
        new() { Id = "aranizdan-biri", Title = "Aranızdan Biri", Author = "Holly Jackson", Genre = "Korku · Gerilim", Price = 276, Cover = "/images/cover-aranizdan.jpg", Tone = "gray", Description = "Bahar tatili yolculuğuna çıkan Red ve arkadaşları, çölde bir keskin nişancının hedefi olur." },
        new() { Id = "korku", Title = "Korku", Author = "Stefan Zweig", Genre = "Korku · Gerilim", Price = 67.20m, Cover = "/images/cover-korku.webp", Tone = "amber", Description = "Vicdan, kaygı ve sırlar üzerine kısa ve çarpıcı bir klasik." }
    ];

    private static readonly Dictionary<string, string[]> CategoryBooks = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dram"] = ["yaprak"], ["fantastik"] = ["kucuk-prens", "ruya-ev", "vampir-kani"],
        ["hikaye"] = ["aranizdan-biri", "ruya-ev", "korku"], ["deneme"] = ["uc-buyuk-usta"],
        ["felsefi"] = ["noraliya", "kucuk-prens"], ["cocuk-kitaplari"] = ["kucuk-prens"],
        ["ani"] = ["nutuk"], ["elestiri"] = ["uc-buyuk-usta"],
        ["suc"] = ["dexter", "dogu-ekspresi", "kilitli-kapi", "aranizdan-biri"],
        ["ask-romantik"] = ["yildiz", "yaprak"], ["macera"] = ["dogu-ekspresi", "kucuk-prens", "vampir-kani"],
        ["psikoloji"] = ["noraliya", "psikiyatrist"], ["mitoloji"] = ["vampir-kani"],
        ["masal"] = ["kucuk-prens"], ["sanat"] = ["uc-buyuk-usta"], ["dini"] = ["noraliya"],
        ["cocuk-hikayesi"] = ["kucuk-prens"]
    };

    public static async Task SeedAsync(HoneyBooksDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(CategorySeeds.Select(seed => new Category { Name = seed.Name, Slug = seed.Slug }));
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Books.AnyAsync(cancellationToken))
        {
            db.Books.AddRange(BookSeeds);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var savedBooks = await db.Books.ToDictionaryAsync(book => book.Id, cancellationToken);
            foreach (var seed in BookSeeds)
                if (savedBooks.TryGetValue(seed.Id, out var savedBook))
                {
                    savedBook.Cover = seed.Cover;
                    savedBook.Price = seed.Price;
                    savedBook.Author = seed.Author;
                    savedBook.Description = seed.Description;
                }
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.BookCategories.AnyAsync(cancellationToken))
        {
            var categories = await db.Categories.ToDictionaryAsync(category => category.Slug, cancellationToken);
            var links = new List<BookCategory>();
            foreach (var (bookId, categorySlugs) in BookSeeds.SelectMany(book => GetCategoriesForBook(book).Select(slug => (book.Id, slug))))
            {
                if (categories.TryGetValue(categorySlugs, out var category))
                    links.Add(new BookCategory { BookId = bookId, CategoryId = category.Id });
            }
            db.BookCategories.AddRange(links);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static IEnumerable<string> GetCategoriesForBook(Book book)
    {
        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, slug) in CategorySeeds)
        {
            if (name == "Korku" && book.Genre.Contains("Korku", StringComparison.OrdinalIgnoreCase) ||
                name == "Polisiye" && book.Genre.Contains("Polisiye", StringComparison.OrdinalIgnoreCase) ||
                name == "Roman" && book.Genre.Equals("Roman", StringComparison.OrdinalIgnoreCase) ||
                name == "Biyografi" && book.Genre.Equals("Biyografi", StringComparison.OrdinalIgnoreCase) ||
                name == "Tarih" && book.Genre.Equals("Tarih", StringComparison.OrdinalIgnoreCase))
                slugs.Add(slug);
        }

        foreach (var (slug, ids) in CategoryBooks)
            if (ids.Contains(book.Id, StringComparer.OrdinalIgnoreCase))
                slugs.Add(slug);

        return slugs;
    }
}
