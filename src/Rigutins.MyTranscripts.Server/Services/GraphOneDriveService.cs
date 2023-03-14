using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Implementation of <see cref="IOneDriveService"/> that uses the Microsoft Graph API to interact with OneDrive.
/// </summary>
public class GraphOneDriveService : IOneDriveService
{
	private const string ApplicationFolderName = "MyTranscripts";

	private readonly GraphServiceClient _graphServiceClient;

	public GraphOneDriveService(GraphServiceClient graphServiceClient)
	{
		_graphServiceClient = graphServiceClient;
	}

	/// <inheritdoc />
	public async Task<IEnumerable<DriveItem>> GetRootDriveItemsAsync()
	{
		var childred = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync();
		return childred.CurrentPage;
	}

	/// <inheritdoc />
	public async Task<List<DriveItem>> GetFolderItemsAsync(string folderId)
	{
		List<DriveItem> items = new();
		var elements = await _graphServiceClient.Me.Drive.Items[folderId].Children.Request().GetAsync();
		items.AddRange(elements.CurrentPage);

		// Keep reading the next page until there are no more pages.
		while (elements.NextPageRequest != null)
		{
			elements = await elements.NextPageRequest.GetAsync();
			items.AddRange(elements.CurrentPage);
		}

		return items;
	}

	/// <inheritdoc />
	public async Task<DriveItem> GetApplicationFolderAsync()
	{
		// Get the folder named "Apps" in the root folder, if it exists; otherwise, create it.
		var appsFolder = await GetAppsFolderAsync();
		if (appsFolder == null)
		{
			appsFolder = await CreateFolderAsync("Apps");
		}

		// Read the list of all folders in the "Apps" folder and return the application folder if it exists; otherwise, create it.
		var items = await GetFolderItemsAsync(appsFolder.Id);
		var applicationFolder = items.FirstOrDefault(i => i.Name == ApplicationFolderName);
		if (applicationFolder == null)
		{
			applicationFolder = await CreateFolderAsync(ApplicationFolderName, appsFolder.Id);
		}

		return applicationFolder;
	}

	/// <summary>
	/// Returns the folder named "Apps" in the root folder, if it exists.
	/// </summary>
	/// <returns>The folder named "Apps" in the root folder if it exists, otherwise null.</returns>
	private async Task<DriveItem?> GetAppsFolderAsync()
	{
		var items = await GetRootDriveItemsAsync();
		return items.FirstOrDefault(i => i.Name == "Apps");
	}

	/// <summary>
	/// Creates a folder with the specified name in the specified parent folder.
	/// </summary>
	/// <param name="folderName">The name of the folder.</param>
	/// <param name="parentFolderId">The id of the parent folder.</param>
	/// <returns>A reference to the created folder.</returns>
	private Task<DriveItem> CreateFolderAsync(string folderName, string? parentFolderId = null)
	{
		var folder = new DriveItem
		{
			Name = folderName,
			Folder = new Folder()
		};

		if (parentFolderId == null)
		{
			return _graphServiceClient.Me.Drive.Root.Children.Request().AddAsync(folder);
		}

		return _graphServiceClient.Me.Drive.Items[parentFolderId].Children.Request().AddAsync(folder);
	}

	/// <inheritdoc />
	public async Task<DriveItem> UploadFileAsync(string fileName, Stream fileStream, string parentFolderId)
	{
		return await _graphServiceClient.Me.Drive.Items[parentFolderId].ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(fileStream);
	}

	/// <inheritdoc />
	public Task DeleteFileAsync(string fileId)
	{
		return _graphServiceClient.Me.Drive.Items[fileId].Request().DeleteAsync();
	}
}
