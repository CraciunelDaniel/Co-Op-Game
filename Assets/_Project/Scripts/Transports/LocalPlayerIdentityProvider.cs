using System;
using Scripts.Core;
using UnityEngine;

namespace Scripts.Transports
{
	public class LocalPlayerIdentityProvider :IPlayerIdentityProvider
	{
		private const string PlayerIdKey = "LocalPlayerId";
		private const string PlayerNameKey = "LocalPlayerName";
		public string PlayerId { get; }
		public string DisplayName { get; private set; }

		public LocalPlayerIdentityProvider()
		{
			string profileSuffix=GetProfileSuffix();
			string idKey=$"{PlayerIdKey}{profileSuffix}";
			string nameKey=$"{PlayerNameKey}{profileSuffix}";

			if (PlayerPrefs.HasKey(idKey))
			{
				PlayerId = PlayerPrefs.GetString(idKey);
			}
			else
			{
				PlayerId = Guid.NewGuid().ToString("N");
				PlayerPrefs.SetString(idKey, PlayerId);
				PlayerPrefs.Save();
			}
			DisplayName = PlayerPrefs.GetString(nameKey,$"Player_{PlayerId[..4]}");
		}
		public void SetDisplayName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return;
			DisplayName = name.Trim();
			string nameKey = $"{PlayerNameKey}{GetProfileSuffix()}";
			PlayerPrefs.SetString(nameKey, DisplayName);
			PlayerPrefs.Save();
		}

		private static string GetProfileSuffix()
		{
			var args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length - 1; i++)
			{
				if (args[i].Equals("-profile", StringComparison.OrdinalIgnoreCase))
					return $"_{args[i + 1]}";
			}
			return $"_{Application.dataPath.GetHashCode()}";
		}
		
	}
}
