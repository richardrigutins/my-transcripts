using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Interface for interacting with OneDrive.
/// </summary>
public interface IOneDriveService
{
	/// <summary>
	/// Returns the list of all files and folders in the specified folder.
	/// </summary>
	/// <param name="folderId">The folder id.</param>
	Task<List<DriveItem>> GetFolderItemsAsync(string folderId);

	/// <summary>
	/// Gets the list of all files and folders in the root folder.
	/// </summary>
	Task<IEnumerable<DriveItem>> GetRootDriveItemsAsync();

	/// <summary>
	/// Uploads a file with the specified name and content to the specified folder.
	/// </summary>
	/// <param name="fileName">The name of the file.</param>
	/// <param name="fileStream">The file content.</param>
	/// <param name="parentFolderId">The id of the folder in which the file will be uploaded.</param>
	/// <returns>A reference to the uploaded file.</returns>
	Task<DriveItem> UploadFileAsync(string fileName, Stream fileStream, string parentFolderId);

	/// <summary>
	/// Gets the folder for the application. If the folder does not exist, it will be created.
	/// </summary>
	/// <returns>A reference to the folder.</returns>
	Task<DriveItem> GetApplicationFolderAsync();

	/// <summary>
	/// Deletes the file with the specified id.
	/// </summary>
	Task DeleteFileAsync(string fileId);
}
