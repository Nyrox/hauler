module crawler
    open System
    open System.Net.Http


    open HtmlAgilityPack

    let error_log (error: Exception) =
        System.IO.File.AppendAllText(common.ERROR_LOG_PATH, error.Message)

    /// Convert's a URI String to a slug by replacing all valid URI characters that are "dangerous" to safe alternatives
    /// Note that this only takes care of the host and path section of a URI, not the protocol section
    let slug (uri: string): string =
        uri |> String.map (fun c -> match c with
            | '/' -> '_'
            | _ -> c
        )


    let uri_site_directory_path (uri: Uri): string =
        assert uri.IsAbsoluteUri

        common.OUTPUT_BASE_PATH + "sites/" + uri.Host + "/"

    let uri_site_path_directory_path (uri: Uri): string =
        assert uri.IsAbsoluteUri

        (uri_site_directory_path uri) + (slug uri.AbsolutePath) + "/"



    let parse (html: string): string seq =
        let mutable hrefs = List.empty

        let document = new HtmlDocument ()
        document.LoadHtml html

        let hyperlinks = document.DocumentNode.SelectNodes("//a")

        if hyperlinks = null then Seq.empty
        else
            for node in hyperlinks do
                let href = node.Attributes.Item "href"
                if href = null then ()
                else
                    hrefs <- match href.Value with
                        | "" -> hrefs
                        | _ -> (
                            "\t-" + href.Value |> Console.WriteLine
                            href.Value::hrefs
                        )

            Seq.cast hrefs

    let get_absolute_uri (base_uri: Uri) (link: string): Uri =
        new Uri (base_uri, link)

    /// <summary>
    /// Given a URI crawls that URI and adds it to the index
    /// </summary>
    /// <param name="recrawl">If set to false, will check if the page has been crawled before and if so skip it</param>
    let crawl (uri: Uri) (recrawl: bool): Uri seq =
        assert uri.IsAbsoluteUri
        if not recrawl && System.IO.Directory.Exists (uri_site_directory_path uri) then Seq.empty
        else
            Console.WriteLine ("Crawling: " + uri.AbsoluteUri)

            let httpClient = new HttpClient ()

            try
                let task = httpClient.GetAsync uri.AbsoluteUri
                let response = task.Result.Content
                let response_html = response.ReadAsStringAsync().Result
                let raw_links = parse (response_html)


                System.IO.Directory.CreateDirectory (uri_site_directory_path uri) |> ignore
                System.IO.Directory.CreateDirectory (uri_site_path_directory_path uri) |> ignore

                System.IO.File.WriteAllLines ((uri_site_path_directory_path uri) + "index.links", raw_links)
                System.IO.File.WriteAllText  ((uri_site_path_directory_path uri) + "index.html", response_html)

                raw_links |> Seq.map (get_absolute_uri uri)
            with
                 | ex ->
                    ex |> error_log
                    Seq.cast List.empty


    let run (seeds: string seq) =
        let mutable open_list = List.empty
        for s in seeds do open_list <- (new Uri(s))::open_list

        while not open_list.IsEmpty do
            let new_seeds = crawl open_list.Head false
            open_list <- open_list.Tail

            for s in new_seeds do
                open_list <- s::open_list



