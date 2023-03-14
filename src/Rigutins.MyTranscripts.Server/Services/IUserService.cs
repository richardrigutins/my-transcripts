using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public interface IUserService
{
	Task<User> GetMyUserDetailsAsync();
	Task<string> GetProfilePictureAsync();
}
