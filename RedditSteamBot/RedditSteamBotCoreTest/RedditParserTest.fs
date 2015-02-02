namespace RedditSteamBotCoreTest
open System
open RedditSteamBotCore
open NUnit.Framework
open RedditSteamBotCore.RedditParser
open WatiNExtend
open FSharpx.State

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
        let recs = runScript SteamBot.readAllSteamRecommends
        ()