namespace RedditSteamBot
module Bot =
    open System
    open FSharpx.Functional.IO
    open FSharpx.Functional
    open FSharpx.Stm.Core
    open RedditSharp
    open RedditSharp.Things
    open SteamBot
    open JSLibraryFSharp
    open JSLibraryFSharp.Monad
    open FSharpx.Functional.Option
    open CuratorSelection
    open WatiN.Core            
    open WatiN
    open JSLibraryFSharp.IO.Logger

    let inline downloadPosts reddit subredditName = io {
        let! hotPost = getHotPosts reddit subredditName
        return List.ofSeq <| hotPost
        }
    type PostId = string
    type BotKnowledge = 
               { IdRcmd : Map<PostId,SteamRecommend>
                 LogLevel : Level
                 Browser : IOWatinBrowser
                 Reddit : IOReddit
                 Curator : string
                 Subreddit : string
                 MaxTaglineLength : int
               }
    

    let inline setup bot = io {
        let! curRecommends = SteamBot.readAllSteamRecommends bot.Browser bot.Curator
        let idRecommends = List.map (fun s -> (s.PostId,s)) curRecommends
        return { bot with IdRcmd = Map.ofList idRecommends }
        }
    
    

    let toApost post d : Apost= { data = d; post = post } 

    let allAPost post comments = { data = post; post = post } :: List.map (toApost post) comments

    let findPostWinner maxTaglineLength (post:Post) =
        let comments : _ = List.ofArray post.Comments
        let all = allAPost post comments 
        maybe {
            let! winner = select maxTaglineLength all
            let! apost = winner.Post
            return (winner.TagLine,apost.Link)
            }


    let inline updateRecommend bot (sr:SteamRecommend) = io {
        let! post = getPost bot.Reddit sr.Link
        let comments : _ = List.ofArray post.Comments
        let all = allAPost post comments 

        let winner = findPostWinner bot.MaxTaglineLength post
        match winner with
        | None -> 
            do! SteamBot.deleteCuration bot.Browser bot.Curator sr.GameId
            return None
        | Some (tl,lk) when tl <> sr.TagLine || lk <> sr.Link -> 

            do! logInfo bot.LogLevel <| sprintf "\"%s\" and \"%s\" is not same or \"%s\" and \"%s\"" tl sr.TagLine lk sr.Link
            do! SteamBot.editCuration bot.Browser bot.Curator sr.GameId tl lk  
            return Some { sr with TagLine = tl; Link = lk } 
        | Some _ -> return Some sr
        }

    let inline updateRecommendSequential (bot:BotKnowledge) (id,sr) = io { 
        let! maybeSr = attempt <| updateRecommend bot sr
        match maybeSr with
        | Choice1Of2 (Some sr') -> return {bot with IdRcmd = Map.add id sr' bot.IdRcmd }
        | Choice1Of2 (None) -> return {bot with IdRcmd = Map.remove id bot.IdRcmd }
        | Choice2Of2 exn -> 
           do! logError bot.LogLevel <| sprintf "Failed to update recommend %A" sr
           do! logError bot.LogLevel <| sprintf "Exception: %s" exn.Message
           return bot 
         }
    let inline updatePostSequential (bot:BotKnowledge) () (post:Post) = io {   
        let winner = findPostWinner bot.MaxTaglineLength post

        match winner with
        | Some (tl,lk) -> 
            let title = post.Title
            do! logInfo bot.LogLevel <| sprintf "Posting curation for %s with tagline %s" title tl 
            let! att = attempt <| SteamBot.postCuration bot.Browser bot.Curator title tl lk
            match att with
            | Choice1Of2 () -> return ()
            | Choice2Of2 exn ->
                do! logError bot.LogLevel <| sprintf "Failed to post winner of %s with tagline: %s" title tl
                do! logCritical bot.LogLevel <| sprintf "Exception: %s" exn.Message
        | _ -> return ()
        }

    let inline runOnce bot = io {
        do! logInfo bot.LogLevel <| "Checking steam for all curations"
        let! bot = setup bot

        do! logInfo bot.LogLevel <| "Updating curations based on changes made to reddit posts"
        let! bot' = IO.foldM (updateRecommendSequential) bot (Map.toList bot.IdRcmd)

        do! logInfo bot.LogLevel <| "Downloading all posts from reddit which are hot"
        let! attemptPosts = attempt <| downloadPosts bot.Reddit bot.Subreddit
        match attemptPosts with
        | Choice1Of2 posts ->
            let newPosts = List.filter (fun (p:Post) -> not <| Map.containsKey p.Id bot.IdRcmd) posts
            do! IO.foldM (updatePostSequential bot) () newPosts
        | Choice2Of2 exn ->
            do! logCritical bot.LogLevel <| "Failed to download posts from reddit"
            do! logCritical bot.LogLevel <| sprintf "Exception: %s" exn.Message
        }

    let runConstFirefox loglevel subreddit curator = io {
        let fox = toIOBrowser <| new IE()
        let reddit = toIOReddit <| new Reddit()
        let bot = { Browser = fox
                  ; Reddit = reddit
                  ; LogLevel = loglevel
                  ; IdRcmd = Map.empty
                  ; Curator = curator
                  ; Subreddit = subreddit 
                  ; MaxTaglineLength = 152
                  }

        while true do
           do! runOnce bot
        
        }
