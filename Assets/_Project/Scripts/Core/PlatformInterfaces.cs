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

		Task<bool> CreateLobbyAsync(int maxPlayers);
		Task<string> JoinLobbyAsync(string lobbyId);
		void LeaveLobby();
		void OpenInviteOverlay();
	}

	public interface IRichPresenceService
	{
		void SetStatus(string statusKey, string value);
		void Clear();
	}
	public interface IAchievementService
	{
		void Unlock(string achievementId);
	}
}
