namespace RedditSteamBot
open System

module RedditSharp = 
    open RedditSharp.Things



    type Apost = {data : obj} with
        member x.Comment = 
            match x.data with
            | :? Post as p -> p.SelfText
            | :? Comment as c -> c.Body
            | _ -> failwith "unknown type: " <| x.GetType()
        member x.Upvotes = 
            match x.data with
            | :? Post as p -> p.Upvotes
            | :? Comment as c -> c.Upvotes
            | _ -> failwith "unknown type: " <| x.GetType()
        member x.Username = 
            match x.data with
            | :? Post as p -> p.AuthorName
            | :? Comment as c -> c.Author 
            | _ -> failwith "unknown type: " <| x.GetType()
        member x.Link = 
            match x.data with
            | :? Post as p -> p.Url.AbsoluteUri
            | :? Comment as c -> c.Shortlink
            | _ -> failwith "unknown type: " <| x.GetType()