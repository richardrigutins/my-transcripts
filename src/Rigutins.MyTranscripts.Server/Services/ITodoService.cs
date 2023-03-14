using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public interface ITodoService
{
	Task<IEnumerable<TodoTask>> GetTasksAsync(string listId);
	Task<TodoTaskList?> GetTodoListByNameAsync(string name);
	Task<TodoTaskList> CreateTodoListAsync(string name);
	Task<TodoTask> CreateTaskAsync(string listId, string name, DateTime? dueDate = null);
	Task<TodoTaskList> GetApplicationTaskListAsync();
}
