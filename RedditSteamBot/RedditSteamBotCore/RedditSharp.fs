namespace RedditSteamBotCore
open System

module RedditSharp = 
    open RedditSharp.Things




    type Post with
        static member comment (p:Post) = p.SelfText
        static member upvotes (p:Post) = p.Upvotes
        static member username (p:Post) = p.AuthorName
     

    type Comment with
        static member comment (c:Comment) = c.Body
        static member upvotes (c:Comment) = c.Upvotes
        static member username (c:Comment) = c.Author