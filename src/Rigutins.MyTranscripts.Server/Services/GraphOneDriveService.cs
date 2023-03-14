using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public class GraphOneDriveService : IOneDriveService
{
	private const string ApplicationFolderName = "MyTranscripts";

	private readonly GraphServiceClient _graphServiceClient;

	public GraphOneDriveService(GraphServiceClient graphServiceClient)
	{
		_graphServiceClient = graphServiceClient;
	}

	public async Task<DriveItem> GetRootDriveAsync()
	{
		return await _graphServiceClient.Me.Drive.Root.Request().GetAsync();
	}

	public async Task<IEnumerable<DriveItem>> GetRootDriveItemsAsync()
	{
		var childred = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync();
		return childred.CurrentPage;
	}

	public async Task<DriveItem> GetDriveItemAsync(string itemId)
	{
		return await _graphServiceClient.Me.Drive.Items[itemId].Request().GetAsync();
	}

	public async Task<List<DriveItem>> GetFolderItemsAsync(string folderId)
	{
		List<DriveItem> items = new();
		var elements = await _graphServiceClient.Me.Drive.Items[folderId].Children.Request().GetAsync();
		items.AddRange(elements.CurrentPage);

		while (elements.NextPageRequest != null)
		{
			elements = await elements.NextPageRequest.GetAsync();
			items.AddRange(elements.CurrentPage);
		}

		return items;
	}

	public async Task<DriveItem> GetApplicationFolderAsync()
	{
		var appsFolder = await GetAppsFolderAsync();
		if (appsFolder == null)
		{
			appsFolder = await CreateFolderAsync("Apps");
		}

		var items = await GetFolderItemsAsync(appsFolder.Id);
		var applicationFolder = items.FirstOrDefault(i => i.Name == ApplicationFolderName);
		if (applicationFolder == null)
		{
			applicationFolder = await CreateFolderAsync(ApplicationFolderName, appsFolder.Id);
		}

		return applicationFolder;
	}

	private async Task<DriveItem?> GetAppsFolderAsync()
	{
		var items = await GetRootDriveItemsAsync();
		return items.FirstOrDefault(i => i.Name == "Apps");
	}

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

	public async Task<DriveItem> UploadFileAsync(string fileName, Stream fileStream, string parentFolderId)
	{
		return await _graphServiceClient.Me.Drive.Items[parentFolderId].ItemWithPath(fileName).Content.Request().PutAsync<DriveItem>(fileStream);
	}

	public Task DeleteFileAsync(string fileId)
	{
		return _graphServiceClient.Me.Drive.Items[fileId].Request().DeleteAsync();
	}
}
