namespace RedditSteamBot
open System

module RedditSharp = 
    open RedditSharp.Things




    type Post with
        member p.comment = p.SelfText
        member p.upvotes = p.Upvotes
        member p.username = p.AuthorName
     

    type Comment with
        member c.comment = c.Body
        member c.upvotes = c.Upvotes
        member c.username = c.Author