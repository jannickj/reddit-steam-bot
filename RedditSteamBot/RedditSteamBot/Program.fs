namespace RedditSteamBot

module Program =
    open System
    open WatiN.Core
    open Bot
    open SteamBot
    open RedditSharp
    open JSLibraryFSharp.Monad
    open RedditSharp.Things
    open FSharpx.Functional
    open JSLibraryFSharp.IO.Logger



    [<EntryPoint>]
    [<STAThreadAttribute>]
    let main argv = 
        //let reddit = new Reddit()
        //let posts = State.eval (reddit,()) <| downloadPosts subreddit
        //let a = WatiN.runScriptIE <| WatiN.closeBrowser()
        Async.StartImmediate <| Bot.runConstFirefox Level.All "pcmastercurator" "pcmrcccp"
        //ignore <| printfn "%A" a
        0 // return an integer exit code

