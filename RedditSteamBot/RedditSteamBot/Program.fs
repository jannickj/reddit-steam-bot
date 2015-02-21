namespace RedditSteamBot
open System
open WatiN.Core

module Program =

    [<EntryPoint>]
    [<STAThreadAttribute>]
    let main argv = 
        let a = WatiN.runScriptIE <| SteamBot.readAllSteamRecommends "pcmrcccp"
        ignore <| printfn "%A" a
        0 // return an integer exit code

