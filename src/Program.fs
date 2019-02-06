// Learn more about F# at http://fsharp.org

open System
open System.Net.Http

open System.Text.RegularExpressions


[<EntryPoint>]
let main argv =

    // crawler.run ["http://news.ycombinator.com"; "http://4chan.org"; "http://reddit.com"]
    indexer.build (common.OUTPUT_BASE_PATH + "sites/")


    0 // return an integer exit code
