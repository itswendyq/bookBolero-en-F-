namespace MyApp.Server.Models
open System


[<CLIMutable>]
type Book =
    {   
        id: int 
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }