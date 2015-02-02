using NUnit.Framework;
using System;
using RedditSteamBot;

namespace RedditSteamBotTest
{
	[TestFixture ()]
	public class Test
	{

		public RedditScanner CreateScanner()
		{
			return new RedditScanner ();
		}

		[Test ()]
		public void Scan_CorrectFormated_GetsReview ()
		{
			string tagline = "This is the expected tag";
			string recommend = "Recommended";

			string comment = "**" + recommend + "**\n\n**Tagline**\n" + tagline + "\n\nblablabla dont care";

			RedditScanner scanner = CreateScanner();


			RedditSteamBot.RedditScanner.ReviewComment actual = scanner.Scan (comment);

			Assert.AreEqual (tagline, actual.Tagline);
			Assert.IsTrue (actual.IsRecommended);
		}
	}
}

