using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public interface IOneDriveService
{
	Task<DriveItem> GetRootDriveAsync();
	Task<List<DriveItem>> GetFolderItemsAsync(string folderId);
	Task<IEnumerable<DriveItem>> GetRootDriveItemsAsync();
	Task<DriveItem> GetDriveItemAsync(string itemId);
	Task<DriveItem> UploadFileAsync(string fileName, Stream fileStream, DriveItem parentFolder);
	Task<DriveItem> GetApplicationFolderAsync();
}
