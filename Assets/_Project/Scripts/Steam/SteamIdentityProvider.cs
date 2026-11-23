using Core;
using Steamworks;

namespace Steam
{
	public class SteamIdentityProvider : IPlayerIdentityProvider
	{
		private string _customDisplayName;
		public string PlayerId => SteamClient.IsValid ?
			SteamClient.SteamId.Value.ToString() 
			: "0";
		public string DisplayName
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(_customDisplayName))
					return _customDisplayName;
				return SteamClient.IsValid ? SteamClient.Name : "Player";
			}
		}

		public void SetDisplayName(string name)
		{
			_customDisplayName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
		}
	}
}