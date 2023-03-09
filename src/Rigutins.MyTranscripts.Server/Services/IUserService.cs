using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public interface IUserService
{
	Task<User> GetMyUserDetailsAsync();
	Task<List<User>> SearchOtherUsersByNameAsync(string name);
	Task<User?> SearchUserByEmailAsync(string email);
	Task<List<User>> SearchUsersByNameAsync(string name);
	Task<string> GetProfilePictureAsync();
}
