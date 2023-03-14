using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

/// <summary>
/// Implementation of <see cref="ITodoService"/> that uses Microsoft Graph API to interact with Microsoft To Do.
/// </summary>
public class GraphTodoService : ITodoService
{
	private const string TodoListName = "MyTranscripts";

	private readonly GraphServiceClient _client;

	public GraphTodoService(GraphServiceClient client)
	{
		_client = client;
	}

	/// <inheritdoc />
	public async Task<TodoTaskList> GetApplicationTaskListAsync()
	{
		TodoTaskList? todoList = await GetTodoListByNameAsync(TodoListName);
		if (todoList is null)
		{
			todoList = await CreateTodoListAsync(TodoListName);
		}

		return todoList;
	}

	/// <inheritdoc />
	public async Task<TodoTaskList?> GetTodoListByNameAsync(string name)
	{
		var lists = await _client.Me.Todo.Lists.Request()
			.Filter($"displayName eq '{name}'")
			.GetAsync();
		return lists.CurrentPage.FirstOrDefault();
	}

	/// <inheritdoc />
	public async Task<TodoTaskList> CreateTodoListAsync(string name)
	{
		var list = new TodoTaskList
		{
			DisplayName = name
		};

		return await _client.Me.Todo.Lists.Request().AddAsync(list);
	}

	/// <inheritdoc />
	public async Task<TodoTask> CreateTaskAsync(string listId, string name, DateTime? dueDate = null)
	{
		var task = new TodoTask
		{
			Title = name,
			Body = new ItemBody
			{
				ContentType = BodyType.Text,
				Content = name,
			},
			Importance = Importance.Normal,
		};

		// If a due date is specified, set the due date and reminder to the same date.kd
		if (dueDate.HasValue)
		{
			task.DueDateTime = new DateTimeTimeZone
			{
				DateTime = dueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
				TimeZone = "UTC",
			};

			task.ReminderDateTime = new DateTimeTimeZone
			{
				DateTime = dueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
				TimeZone = "UTC",
			};
		}

		return await _client.Me.Todo.Lists[listId].Tasks.Request().AddAsync(task);
	}
}
