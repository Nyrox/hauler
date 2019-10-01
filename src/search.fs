

module search

open System

let search_keyword (keyword) =
    let domainfile = common.OUTPUT_BASE_PATH + "keywords/" + keyword + ".keyword"

    let text = IO.File.ReadAllText (domainfile)

    text
