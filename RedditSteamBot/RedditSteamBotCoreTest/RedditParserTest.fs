namespace RedditSteamBotCoreTest
open System
open RedditSteamBot
open NUnit.Framework
open RedditSteamBot.RedditParser
open FSharpx.Functional
open FSharpx.Stm.Core
open WatiN.Core
open WatiN
open RedditSharp
open JSLibraryFSharp.Monad
open JSLibraryFSharp.IO.Logger

[<TestFixture>]
type RedditParserTest() = 

    [<Test>]
    member x.Scan_CorrectFormated_GetsReview() =
        let tagline = "This is the expected tag"
        let recommend = "Recommended"
        let comments = "**" + recommend + "**\n\n**Tagline**\n" + tagline + "\n\nblablabla dont care"

        let actual = analyze comments 

        Assert.AreEqual (tagline, actual.TagLine.Value);
        Assert.IsTrue (actual.IsRecommended.Value);
        ()
      

    
    [<Test>]
    member x.test() =
        //let script = SteamBot.editCuration "pcmrcccp" 257350 "yoyo" "linky.sweet"  // "Half-Life: Source" "yo taggy" ""
        //let posts = State.eval (new Reddit(), ()) script
        //let ap = List.toArray posts
        //let vals = runScriptFox script
        //let reddit = new Reddit();
        //let post = reddit.GetPost(new Uri("http://www.reddit.com/r/pcmasterrace/comments/2ws5h0/i_saw_the_post_on_biased_benchmarks_and_raise_you/cotm90f"))
        Async.RunSynchronously <| Bot.runConstFirefox Level.All "pcmastercurator" "pcmrcccp"
        //let a = WatiN.runScriptFox <| WatiN.o
        ()