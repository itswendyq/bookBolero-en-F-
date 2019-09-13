namespace MyApp.Server.Models

open Microsoft.EntityFrameworkCore

type BooksContext(options: DbContextOptions) =
    inherit DbContext(options)
    
    [<DefaultValue>]
    val mutable private books: DbSet<Book>
    member __.Books
        with get() = __.books
        and private set value = __.books <- value
    
    override __.OnConfiguring optionsBuilder =
        if not optionsBuilder.IsConfigured then
            optionsBuilder.UseSqlServer (@"Server=(localdb)\mssqllocaldb;DataBase=prueba") |> ignore

    override __.OnModelCreating modelBuilder =
        modelBuilder.Entity<Book>(fun entity ->
            entity.HasKey(fun e -> e.id :> obj) |> ignore

            entity.Property(fun e -> e.title )
                .IsRequired()
                .HasMaxLength(50) |> ignore
            
            entity.Property(fun e -> e.author )
                .IsRequired()
                .HasMaxLength(50) |> ignore

            entity.Property(fun e -> e.publishDate ) |> ignore

            entity.Property(fun e -> e.isbn )
                .IsRequired()
                .HasMaxLength(50) |> ignore
        ) |> ignore