using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Interface for interacting with Microsoft To Do.
/// </summary>
public interface ITodoService
{
	/// <summary>
	/// Searches for a task list with the specified name.
	/// </summary>
	/// <param name="name">The name of the task list.</param>
	/// <returns>The task list if it exists, otherwise null.</returns>
	Task<TodoTaskList?> GetTodoListByNameAsync(string name);

	/// <summary>
	/// Creates a new task list with the specified name.
	/// </summary>
	/// <param name="name">The name of the task list.</param>
	/// <returns>A reference to the created task list.</returns>
	Task<TodoTaskList> CreateTodoListAsync(string name);

	/// <summary>
	/// Creates a new task in the specified task list.
	/// </summary>
	/// <param name="listId">The id of the task list.</param>
	/// <param name="name">The name of the task.</param>
	/// <param name="dueDate">The due date of the task. If null, the task will not have a due date and reminder.</param>
	/// <returns>A reference to the created task.</returns>
	Task<TodoTask> CreateTaskAsync(string listId, string name, DateTime? dueDate = null);

	/// <summary>
	/// Gets the task list for the application. If the list does not exist, it will be created.
	/// </summary>
	/// <returns>A reference to the task list.</returns>
	Task<TodoTaskList> GetApplicationTaskListAsync();
}
