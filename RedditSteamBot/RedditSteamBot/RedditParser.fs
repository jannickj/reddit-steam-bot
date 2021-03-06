﻿namespace RedditSteamBot
open System
open JSLibraryFSharp

module RedditParser =
    type CommentResult = { IsRecommended : bool option; TagLine : string option }


    let findTagLine (comment: string) beginAt =
        let rest = String.skip comment beginAt
        let split = String.seperate rest "\n"      
        List.tryFind (fun s -> not <| String.IsNullOrWhiteSpace s) split

    let  analyzeMatch comment curOut (strMatch,endpos)  =
        let text = String.toLower <| String.replace "*" "" strMatch
        match text with
        | "not recommended" ->
            { curOut with IsRecommended = Some false }
        | "recommended" ->
            { curOut with IsRecommended = Some true }
        | "tagline" ->
            { curOut with TagLine = findTagLine comment endpos }
        | _ -> curOut
         

    let analyze comment = 
        Regex.matches @"\*\*.*?\*\*" comment 
        |> Seq.map (fun (idx,value) -> (value,idx+value.Length))
        |> Seq.fold (analyzeMatch comment) { IsRecommended = None; TagLine = None }

