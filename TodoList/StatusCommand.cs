using System;

namespace TodoList
{
	public class StatusCommand : ICommand
	{
		private readonly TodoList _todoList;
		private readonly string _todoFilePath;
		private readonly int _index;
		private readonly TodoStatus _newStatus;

		public StatusCommand(TodoList todoList, string todoFilePath, int index, TodoStatus newStatus)
		{
			_todoList = todoList;
			_todoFilePath = todoFilePath;
			_index = index;
			_newStatus = newStatus;
		}

		public void Execute()
		{
			TodoItem item = _todoList[_index];
			if (item == null)
			{
				throw new TaskNotFoundException($"Задача с индексом {_index} не найдена в текущем списке.");
			}

			item.ChangeStatus(_newStatus);
			FileManager.SaveTasks(_todoList, _todoFilePath);
			Console.WriteLine($"Статус задачи {_index} изменен на: {item.GetStatusString()}");
		}

		public void Undo()
		{
		}
	}
}