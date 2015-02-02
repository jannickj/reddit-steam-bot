using System;
using RedditSharp;
using System.Linq;
using RedditSharp.Things;
using System.Text.RegularExpressions;

namespace RedditSteamBot
{
	public class RedditScanner
	{



		public RedditScanner ()
		{


		}


		public void Begin()
		{
			//string game = post.Title;

		}

		public ReviewComment Scan(string comment)
		{
			string pattern = @"\*\*.*?\*\*";

			MatchCollection matches = Regex.Matches (comment, pattern);


			return default(ReviewComment);
		}


		private string analyzeMatch(string match, ReviewComment comment)
		{

			switch (match) {
			case "tagline":
				break;
			case "recommended":
				break;
			case "not recommended": 
				break;
			}
			return "";
		}


		public struct ReviewComment
		{
			public string Tagline { get; set; }
			public bool HasTagLine { get; set; }
			public bool IsRecommended { get; set; }


		}


	}
}

