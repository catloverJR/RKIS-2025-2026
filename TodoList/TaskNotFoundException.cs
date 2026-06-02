using System;

namespace TodoList
{
	public class TaskNotFoundException : Exception
	{
		public TaskNotFoundException(string message) : base(message) { }
	}
}