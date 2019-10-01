/// This module contains the indexing part of the application
/// It's purpose is to parse the HTML files aqcuired by the crawler and build the keyword index out of it
///
///

module indexer

open System
open System.Collections.Generic
open System.Text.RegularExpressions

open HtmlAgilityPack

let node_weight (node: HtmlAgilityPack.HtmlNode): double =

    1.0

let build_index (html: String) =
    let keyword_index = new Dictionary<string, double> ()

    let document = new HtmlDocument ()
    document.LoadHtml html

    let text_nodes = document.DocumentNode.SelectNodes("//text()")

    let is_empty (s: string) =
        s.Length = 0

    if text_nodes = null then keyword_index
    else
        for text in text_nodes do
            text.InnerText.Trim([|' '|]).Split([|' '|])
                |> Seq.filter (fun s -> not (is_empty (s.Trim())))
                |> Seq.filter (fun s -> Regex.IsMatch(s, @"^\w+$"))
                |> Seq.map (fun s -> s.ToLower())
                |> Seq.iter (fun (keyword: string) ->
                (
                    if not (keyword_index.ContainsKey keyword) then keyword_index.[keyword] <- 0.0
                    keyword_index.[keyword] <- ((keyword_index.[keyword]) + (node_weight text))
                ))

        keyword_index

/// Attempts to completely rebuild the index from the sites given in sites_dir
let build (sites_dir) =
    let domains = System.IO.Directory.GetDirectories sites_dir

    (System.IO.Directory.CreateDirectory (common.OUTPUT_BASE_PATH + "keywords")) |> ignore

    for domain in domains do
        printfn "Building index for domain: %s" domain
        let paths = System.IO.Directory.GetDirectories domain

        for path in paths do
            try
                printfn "\tBuilding index for path: %s" path
                let keyword_index = build_index (IO.File.ReadAllText(path + "/index.html"))

                for pair in keyword_index do
                    IO.File.AppendAllText (
                        common.OUTPUT_BASE_PATH + "keywords/" + pair.Key + ".keyword",
                        sprintf "%f,%s\n" pair.Value (path)
                    )
            with
                | ex -> ex.Message |> printfn "Error while indexing %s: %s" domain
    ()
