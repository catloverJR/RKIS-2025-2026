using System.Collections.Generic;

namespace TodoList
{
	public static class AppInfo
	{
		public static IDataStorage Storage { get; set; }
		public static TodoList Todos { get; set; }
		public static Profile CurrentProfile { get; set; }
		public static string TodoFilePath { get; set; } = "data/tasks.enc";
		public static string ProfileFilePath { get; set; } = "data/profile.enc";
		public static Stack<ICommand> UndoStack { get; } = new Stack<ICommand>();
		public static Stack<ICommand> RedoStack { get; } = new Stack<ICommand>();
	}
}