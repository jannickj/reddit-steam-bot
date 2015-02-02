using System;
using WatiN.Core;
using RedditSharp.Things;
using RedditSharp;
using System.Linq;

namespace RedditSteamBot
{
	class MainClass
	{


		[STAThread]
		public static void Main (string[] args)
		{

			IE fox = new IE ();
			fox.GoTo ("http://steamcommunity.com/groups/pcmrcccp/curation/edit/257350");
			string test = fox.Text;

			TextField field = fox.TextField ("curationBlurbInput");
			field.Clear ();
			field.AppendText ("This is even more amazing");

			Div div = fox.Div(Find.ByClass ("btn_green_white_innerfade btn_medium"));
			div.Click ();
//
//			var s = new RedditSharp.Reddit ();
//			s.

//			Reddit reddit = new Reddit ();
//
//			Subreddit sub = reddit.GetSubreddit ("pcmastercurator");
//
//			var posts = sub.Posts;
//
//			var post = posts.First ();
//
//			string text = post.SelfText;
//
//			string x = "**Recommended**\n\n**Tagline**\nIf you love a quality PC-game that with a great story and exciting gameplay then you must get this game.\n\n**Review**\nBaldurs gate is pretty good.";
		}
	}
}
