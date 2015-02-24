namespace RedditSteamBot
open System

module RedditSharp = 
    open RedditSharp.Things
    open RedditSharp
    open FSharpx.Functional.IO

    type IOReddit = { red : Reddit } with
        member x.getHotPosts name = io { let sub = x.red.GetSubreddit name
                                         return seq sub.Hot }
        member x.GetPost uri = io { return x.red.GetPost(new Uri(uri))}

    let toIOReddit reddit = { red = reddit }
    
    //let inline getSubreddit (x:^a) subreddit = ( ^a : (member GetSubreddit: string -> IO<Subreddit>) (x,subreddit)) 
    let inline getPost (x:^a) uri = ( ^a : (member GetPost: string -> IO<Post>) (x,uri))
    let inline getHotPosts (x:^a) subreddit = ( ^a : (member getHotPosts: string -> IO<Post seq>) (x,subreddit))

    type Apost = {data : obj; post: Post} with
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
            | :? Comment as c -> x.post.Url.AbsoluteUri
            | _ -> failwith "unknown type: " <| x.GetType()