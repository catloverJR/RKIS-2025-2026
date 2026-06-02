using System;

namespace TodoList
{
	public class InvalidCommandException : Exception
	{
		public InvalidCommandException(string message) : base(message) { }
	}
}