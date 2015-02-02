namespace RedditSteamBotCore
open System
open WatiN.Core
open WatiNExtend

module Program = 

    [<EntryPoint>]
    let main args =
        let recs = runScript SteamBot.readAllSteamRecommends
        // Return 0. This indicates success.
        0
