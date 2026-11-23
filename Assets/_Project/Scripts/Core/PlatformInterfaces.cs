using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
	public interface ILobbyService
	{
		bool IsSupported { get; }
		string CurrentLobbyId { get; }
		event Action<string> OnJoinRequested;

		Task<bool> CreateLobbyAsync(LobbySettings settings);
		Task<string> JoinLobbyAsync(string lobbyId);
		void UpdateSessionState(int playerCount, int maxPlayers, bool inGame);
		void LeaveLobby();
		void OpenInviteOverlay();
	}

	public enum LobbyVisibility
	{
		FriendsOnly = 0,
		Private = 1
	}

	public readonly struct LobbySettings
	{
		public string Name { get; }
		public int MaxPlayers { get; }
		public LobbyVisibility Visibility { get; }

		public LobbySettings(string name, int maxPlayers, LobbyVisibility visibility)
		{
			Name = name;
			MaxPlayers = maxPlayers;
			Visibility = visibility;
		}
	}

	public interface IRichPresenceService
	{
		void SetStatus(string statusKey, string value);
		void Clear();
	}

	public interface IAchievementService
	{
		void IncrementStat(string statName, int amount = 1);
		void Unlock(string achievementId);
		void StoreStats();
	}
}