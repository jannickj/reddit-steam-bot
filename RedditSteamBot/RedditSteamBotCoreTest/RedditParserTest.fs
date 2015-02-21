namespace RedditSteamBotCoreTest
open System
open RedditSteamBot
open NUnit.Framework
open RedditSteamBot.RedditParser
open FSharpx.Functional.State
open FSharpx.Stm.Core
open WatiN.Core
open WatiN

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
        let script = SteamBot.postCuration "pcmrcccp" "Half-Life: Source" "yo taggy" ""
        //let vals = runScriptFox script
        ()