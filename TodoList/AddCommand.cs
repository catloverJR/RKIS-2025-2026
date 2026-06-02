using System;

namespace TodoList
{
	public class AddCommand : ICommand
	{
		public string TaskText { get; set; }
		private TodoItem _addedItem;

		public void Execute()
		{
			if (string.IsNullOrWhiteSpace(TaskText)) return;

			_addedItem = new TodoItem(TaskText);
			AppInfo.Todos.Add(_addedItem);
			AppInfo.RedoStack.Clear();
			AppInfo.UndoStack.Push(this);

			AppInfo.Storage.SaveTodos(AppInfo.Todos);
			Console.WriteLine("Задача добавлена.");
		}
	}

		public void Undo()
		{
			Console.WriteLine("Отмена: задача удалена.");
		}
	}
}