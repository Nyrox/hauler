# hauler
A web crawling and indexing engine that works off an initial seed URI and tries to download the internet.

## Crawling
To start the crawler just run `dotnet run -- crawl "http(s)//yourdomain.com"` and it will happily go ahead and try to find any and all things it can.
This creates a new directory under `index/sites/%1` for each domain found and a subfolder under that for every subpath of that domain and so on.

## Indexing
To be able to search things we need to put it into a format that is more easily searchable than scourint thousands and thousands of html files.
To do this just run `dotnet run -- index` and it will start parsing the downloaded sites for keywords and put them into `index/keyword/keyword` for you.

## Searching
To search the indxexed database just run `dotnet run -- search keyword` and you will get a sorted list of sites that contain your keyword based on where and how often they appear!

## Problems
The program chokes on a lot of things. The internet is large and with large comes variance, weird non-standard formatting and all hosts of problems.  
As such some of these variation will inevitably cause problems and a lot of them do. For the most part these are just logged and ignored, although some bugs do terminate the crawling process, making it hard to get large amounts of data, as this happens after a few thousand pages on average.  
