// Learn more about F# at http://fsharp.org

open System
open System.Net.Http

open System.Text.RegularExpressions

let outputPath = "output/";

let _regexIgnoreInnerHtml = @"[ \w=\[\]\""]*"
let regexHyperlink = new Regex(@"<a" + _regexIgnoreInnerHtml + "href=\"([\:\-\w\/\.]*)\"" + _regexIgnoreInnerHtml + ">", RegexOptions.Compiled)

let regex_uri = new Regex(@"(https?):\/\/([\w\.-]+)(\/.*)?")

let MAX_DEPTH = 4

// Given a URI generates a slug safe for storing on the filesystem
let slug (uri: string) =
    uri |> String.map (fun c -> match c with
        | '/' -> '_'
        | _ -> c
    )


type Path = {
    protocol: string
    host: string
    path: string
}

let full_path (path: Path) =
    path.protocol + "://" + path.host + path.path

let nice_path (path: Path) =
    path.host + path.path

let parse_full_path (path: string): Path =
    let matches = regex_uri.Matches path

    let m = matches.Item 0

    {
        protocol = (m.Groups.Item 1).Value
        host = (m.Groups.Item 2).Value
        path = (m.Groups.Item 3).Value
    }
    

let parse_path (current: Path) (next: string) = 
    if next.StartsWith("//") then parse_full_path (current.protocol + ":" + next)
    else if next.StartsWith("/") then { current with path = next }
    else if next.Contains("://") then parse_full_path next
    else { current with path = "/" + next }

// Given a webpage return's all outgoing hyperlinks
let find_outgoing page =
    let matches = Seq.cast (regexHyperlink.Matches page)

    matches |>
        Seq.map (fun (m: Match) -> (m.Groups.Item 1).Value)
 

let rec crawl (path: Path) (depth: int) =
    if depth > MAX_DEPTH then ()
    if System.IO.File.Exists (outputPath + (slug (nice_path path)) + ".html") then () 
    else

        ("Crawling: " + full_path path) |> Console.WriteLine

        let client = new HttpClient()

        let result = 
            try 
                Some ((client.GetAsync (full_path path)).Result)
            with
                | ex -> None

        if result.IsNone then ()
        else
            let text = result.Value.Content.ReadAsStringAsync().Result
            System.IO.File.WriteAllText (outputPath + (slug (nice_path path)) + ".html", text)

            let outgoing = find_outgoing text
            System.IO.File.WriteAllLines (outputPath + (slug (nice_path path)) + ".index", outgoing)

            for next in outgoing do
                crawl (parse_path path next) (depth + 1)

[<EntryPoint>]
let main argv =
    let files = System.IO.Directory.GetFiles(outputPath)
    Array.ForEach(files, (fun (f: string) -> System.IO.File.Delete f))

    System.IO.Directory.CreateDirectory(outputPath);

    crawl {
        protocol = "http"
        host = "news.ycombinator.com"
        path = "/"
    } 0

    0 // return an integer exit code
