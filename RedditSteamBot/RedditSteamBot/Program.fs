namespace RedditSteamBot

module Program =
    open System
    open WatiN.Core
    open Bot
    open SteamBot
    open RedditSharp
    open JSLibraryFSharp.Monad
    open RedditSharp.Things


    let subreddit = "pcmastercurator"

    [<EntryPoint>]
    [<STAThreadAttribute>]
    let main argv = 
        let reddit = new Reddit()
        let posts = State.eval (reddit,()) <| downloadPosts subreddit
        //let a = WatiN.runScriptIE <| SteamBot.readAllSteamRecommends "pcmrcccp"

        //ignore <| printfn "%A" a
        0 // return an integer exit code

