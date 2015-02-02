using System;
using WatiN.Core;

namespace RedditSteamBot
{
	public class SteamBot
	{
		private string group;
		private BrowserFactory factory;

		public SteamBot (BrowserFactory factory, string group)
		{
			this.group = group;
			this.factory = factory;
		}

		public void PostNewCuration(string game, string msg, string link)
		{
			using (Browser fox = factory.CreateBrowser()) {
				fox.GoTo ("http://steamcommunity.com/groups/" + group + "/curation/new");

				TextField appField = fox.TextField ("curationAppInput");
				appField.AppendText (game);

				TextField recommendField = fox.TextField ("curationBlurbInput");
				recommendField.AppendText ("This is even more amazing");

				Div div = fox.Div (Find.ByClass ("btnv6_green_white_innerfade btn_medium"));
				div.Click ();
			}
		}
	}
}

