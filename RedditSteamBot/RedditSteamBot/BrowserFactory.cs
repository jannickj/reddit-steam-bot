using System;
using WatiN.Core;

namespace RedditSteamBot
{
	public class BrowserFactory
	{
		public BrowserFactory ()
		{
		}

		public Browser CreateBrowser()
		{
			return new IE (true);
		}
	}
}

