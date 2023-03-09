using Microsoft.Graph;

namespace Rigutins.MyTranscripts.Server.Services;

public class GraphTodoService : ITodoService
{
	private readonly GraphServiceClient _client;
	private readonly ILogger<GraphTodoService> _logger;

	public GraphTodoService(GraphServiceClient client, ILogger<GraphTodoService> logger)
	{
		_client = client;
		_logger = logger;
	}

	public async Task<IEnumerable<TodoTask>> GetTasksAsync(string listId)
	{
		var toDo = await _client.Me.Todo.Lists[listId].Tasks.Request().GetAsync();
		return toDo.CurrentPage;
	}

	public async Task<TodoTaskList?> GetTodoListByNameAsync(string name)
	{
		var lists = await _client.Me.Todo.Lists.Request()
			.Filter($"displayName eq '{name}'")
			.GetAsync();
		return lists.CurrentPage.FirstOrDefault();
	}

	public async Task<TodoTaskList> CreateTodoListAsync(string name)
	{
		var list = new TodoTaskList
		{
			DisplayName = name
		};

		return await _client.Me.Todo.Lists.Request().AddAsync(list);
	}

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

		if (dueDate.HasValue)
		{
			_logger.LogInformation($"Due date: {dueDate.Value} ({dueDate.Value.ToString("zzz")} timezone)");
			_logger.LogInformation($"Due date: {dueDate.Value.ToUniversalTime()} (UTC timezone)");

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
