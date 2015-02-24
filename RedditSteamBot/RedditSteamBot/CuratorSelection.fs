﻿namespace RedditSteamBot
module CuratorSelection =

    open RedditParser
    open JSLibraryFSharp
    open FSharpx.Functional.Operators
    open FSharpx.Functional.Option

    let inline username (x:^a ) = ( ^a : (member Username: string) (x))
    let inline upvotes (x:^a) = ( ^a : (member Upvotes: int) (x))
    let inline comment (x:^a) = ( ^a : (member Comment: string) (x))


    type CurationWinner<'a> = { TagLine : string; Post: 'a option }

    let hasRecommend result = result.IsRecommended.IsSome

    let hasTagLine maxLength (result:CommentResult) = Option.isSome <| liftM maybe (fun t -> String.length t <= maxLength) result.TagLine

    let inline isRecommended  results =
        let resultsWithRecommend = List.filter (snd >> hasRecommend) results
        let noSamePoster = Seq.map snd <| Seq.groupBy (fun (p,_) -> username p) resultsWithRecommend
        let decisions = Seq.choose Seq.tryHead noSamePoster
        let (isFor,isAgainst) = List.partition id <| List.ofSeq (Seq.choose (fun (_,r) -> r.IsRecommended) decisions)
        List.length isFor > List.length isAgainst

    let inline highestTagLine maxLength results =
        let resultsWithTagline = List.filter (snd >> hasTagLine maxLength) results
        let highest = List.rev <| List.sortBy (fun (p,_) -> upvotes p) resultsWithTagline
        match List.tryHead highest with
        | Some (post,res) -> {Post =Some post; TagLine = Option.get res.TagLine }
        | None -> {Post = None; TagLine = "Was voted in by the community, but nobody provided a tagline ><" }

    let inline select maxLength posts =
        let results = List.map (fun p -> (p, analyze (comment p))) posts
        if isRecommended results then
            Some <| highestTagLine maxLength results 
        else
            None
       