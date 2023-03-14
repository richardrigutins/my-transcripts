using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Implementation of <see cref="IUserService"/> that uses Microsoft Graph API to interact with user data.
/// </summary>
public class GraphUserService : IUserService
{
	private readonly GraphServiceClient _client;

	public GraphUserService(GraphServiceClient client)
	{
		_client = client;
	}

	/// <inheritdoc />
	public Task<User> GetMyUserDetailsAsync()
	{
		return _client.Me.Request().GetAsync();
	}

	/// <inheritdoc />
	public async Task<string> GetProfilePictureAsync()
	{
		try
		{
			using Stream picture = await _client.Me.Photo.Content.Request().GetAsync();
			if (picture.Length == 0)
			{
				return string.Empty;
			}

			using MemoryStream ms = new();
			await picture.CopyToAsync(ms);
			byte[] bytes = ms.ToArray();
			return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
		}
		catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			// User has no profile picture
			return string.Empty;
		}
	}
}
