using Microsoft.Graph;
using System.Text.RegularExpressions;

namespace Rigutins.MyTranscripts.Server.Services;

public class GraphUserService : IUserService
{
	private readonly GraphServiceClient _client;
	private readonly ILogger<GraphUserService> _logger;

	public GraphUserService(GraphServiceClient client, ILogger<GraphUserService> logger)
	{
		_client = client;
		_logger = logger;
	}

	public Task<User> GetMyUserDetailsAsync()
	{
		return _client.Me.Request().GetAsync();
	}

	public async Task<User?> SearchUserByEmailAsync(string email)
	{
		try
		{
			return await _client.Users[email].Request().GetAsync();
		}
		catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<List<User>> SearchUsersByNameAsync(string name)
	{
		User currentUser = await GetMyUserDetailsAsync();
		if (!IsWorkOrSchoolAccount(currentUser))
		{
			_logger.LogWarning("User is not a work or school account");
			return new();
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			return new();
		}

		List<User> allUsers = new();
		var filter = $"startswith(displayName,'{name}') or startswith(givenName,'{name}') or startswith(surname,'{name}') or startswith(mail,'{name}') or startswith(userPrincipalName,'{name}')";
		// request users using the _client and filtering them by displayName
		var users = await _client.Users.Request()
								 .Filter(filter)
								 .GetAsync();
		// add the users to the list
		allUsers.AddRange(users.CurrentPage);
		// check if this is the last page, if not, get the next page
		while (users.NextPageRequest != null)
		{
			users = await users.NextPageRequest.GetAsync();
			allUsers.AddRange(users.CurrentPage);
		}

		return allUsers;
	}

	public async Task<List<User>> SearchOtherUsersByNameAsync(string name)
	{
		User currentUser = await GetMyUserDetailsAsync();
		var allUsers = await SearchUsersByNameAsync(name);

		return allUsers.Where(u => u.Id != currentUser.Id).ToList();
	}

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
			return string.Empty;
		}
	}

	private bool IsWorkOrSchoolAccount(User user)
	{
		// If the id format is like this: 00000000-0000-0000-0000-000000000000, then it can confirm that this account is not personal account.
		var regex = new Regex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$");
		return regex.IsMatch(user.Id);
	}
}
