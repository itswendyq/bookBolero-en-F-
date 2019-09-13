namespace MyApp.Server

open System
open System.IO
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open MyApp
open FSharp.Control.Tasks.V2
open MyApp.Server.Models
open Bolero.Templating.Server

type BookService(env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.BookService>()

    let books =
        Path.Combine(env.ContentRootPath, "data/books.json")
        |> File.ReadAllText
        |> Json.Deserialize<Client.Main.Book[]>
        |> ResizeArray               
        
    override this.Handler =
        {
            getBooks = Remote.authorize <| fun _ () -> async {
                return books.ToArray()
            }

            addBook = Remote.authorize <| fun _ book -> async {
                books.Add(book)
            }

            signIn = Remote.withContext <| fun http (username, password) -> async {
                if password = "qaz123456!" && username = "WendyQ" then
                    do! http.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = Remote.withContext <| fun http () -> async {
                return! http.AsyncSignOut()
            }

            getUsername = Remote.authorize <| fun http () -> async {
                return http.User.Identity.Name
            }
        }

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvcCore() |> ignore
        services
            .AddDbContext<BooksContext>(fun options -> options.UseSqlServer (@"Server=(localdb)\mssqllocaldb;DataBase=prueba")  |> ignore) |> ignore
        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<BookService>()
#if DEBUG
            .AddHotReload(templateDir = "../MyApp.Client")
#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseAuthentication()
            .UseRemoting()
#if DEBUG
            .UseHotReload()
#endif
            .UseClientSideBlazorFiles<Client.Startup>()
            .UseRouting()
            .UseEndpoints(fun endpoints ->
                endpoints.MapDefaultControllerRoute() |> ignore
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html") |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
