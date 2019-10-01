// Learn more about F# at http://fsharp.org

open System
open System.Net.Http

open System.Text.RegularExpressions

exception SubcommandDoesNotExistException of string

let parseSubcommand commands arg =
        Seq.tryFind (fun (i, _) -> i = arg) commands
            |> Option.map (fun (_, cb) -> cb)
            |> Option.orElse (Some(fun argv -> raise (SubcommandDoesNotExistException(arg))))
            |> Option.get

[<EntryPoint>]
let main argv =
    let program = parseSubcommand [
        ("search", fun (argv: string []) -> argv.[0] |> search.search_keyword |> printfn "%A")
        ("crawl", fun (argv: string []) -> [argv.[0]] |> crawler.run)
        ("index", fun (argv: string []) -> indexer.build (common.OUTPUT_BASE_PATH + "sites/") |> ignore)
    ]

    argv.[1..]
        |> program (argv.[0])
        |> ignore

    0
