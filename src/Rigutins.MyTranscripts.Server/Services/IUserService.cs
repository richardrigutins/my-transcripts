using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Interface for interacting with user data.
/// </summary>
public interface IUserService
{
	/// <summary>
	/// Gets the the details of the current user.
	/// </summary>
	/// <returns>The details of the current user.</returns>
	Task<User> GetMyUserDetailsAsync();

	/// <summary>
	/// Gets the profile picture of the current user.
	/// </summary>
	/// <returns>A base64 encoded string of the profile picture.</returns>
	Task<string> GetProfilePictureAsync();
}
