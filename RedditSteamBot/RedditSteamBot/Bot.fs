namespace RedditSteamBot
module Bot =
    open System
    open FSharpx.Functional.State
    open FSharpx.Stm.Core
    open RedditSharp
    open RedditSharp.Things
    open SteamBot
    open JSLibraryFSharp
    open JSLibraryFSharp.Monad
    open FSharpx.Functional.Option
    open CuratorSelection
    open WatiN.Core            

    let inline getSubreddit (x:^a) name = ( ^a : (member GetSubreddit: string -> Subreddit) (x,name)) 
    let inline downloadPosts subredditName = state {
        let! (reddit,_) = getState
        let subreddit = getSubreddit reddit subredditName
        return List.ofSeq <| seq (subreddit.Hot)

        }
    type PostId = string
    type Bot = { idRcmd : Map<PostId,SteamRecommend>
               }


    let inline setup subreddit curator = state {
        let! curRecommends = SteamBot.readAllSteamRecommends curator
        let idRecommends = List.map (fun s -> (s.PostId,s)) curRecommends
        return { idRcmd = Map.ofList idRecommends }
        }
    
    

    let toApost d : Apost= { data = d } 

    let findPostWinner (post:Post) =
        let comments : _ = List.ofArray post.Comments
        let all = { data = post } :: List.map toApost comments
        maybe {
            let! winner = select all
            let! apost = winner.Post
            return (winner.TagLine,apost.Link)
            }


    let inline updateRecommend curator (sr:SteamRecommend) = state {
        let! (reddit:Reddit, browser) = getState
        let post = reddit.GetPost(new Uri(sr.Link))
        let comments : _ = List.ofArray post.Comments
        let all = { data = post } :: List.map toApost comments
        let post = reddit.GetPost(new Uri(sr.Link))
        let winner = findPostWinner post
        match winner with
        | None -> 
            do State.eval browser <| SteamBot.deleteCuration curator sr.GameId
            return None
        | Some (tl,lk) when tl <> sr.TagLine || lk <> sr.Link -> 

            do State.eval browser <| SteamBot.editCuration curator sr.GameId tl lk  
            return Some { sr with TagLine = tl; Link = lk } 
        | Some _ -> return Some sr
        }

    let inline updateRecommendSequential curator (bot:Bot) (id,sr) = state { 
        let! maybeSr = updateRecommend curator sr
        return match maybeSr with
               | Some sr' -> {bot with idRcmd = Map.add id sr' bot.idRcmd }
               | None -> {bot with idRcmd = Map.remove id bot.idRcmd }
         }
    let inline updatePostSequential curator () (post:Post) = state {   
        let! (_, browser) = getState
        let winner = findPostWinner post
        match winner with
        | Some (tl,lk) -> 
            let title = post.Title
            do State.eval browser <| SteamBot.postCuration curator title tl lk
            return ()
        | _ -> return ()
        }
    
    let inline runOnce subreddit curator = state {
        let! (_, browser) = getState
        let bot = State.eval browser <| setup subreddit curator
        let! bot' = State.fold (updateRecommendSequential curator) bot <| Map.toList bot.idRcmd
        let! posts = downloadPosts subreddit
        let newPosts = List.filter (fun (p:Post) -> not <| Map.containsKey p.Id bot.idRcmd) posts
        do! State.fold (updatePostSequential curator) () newPosts

        return ()
        }

    let runConstFirefox subreddit curator = 
        let fox = new FireFox()

        let reddit = new Reddit()
        while true do
            ignore << State.exec (reddit,fox) <| runOnce subreddit curator
        ()
