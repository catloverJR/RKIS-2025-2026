using System;

namespace TodoList
{
	public class InvalidArgumentException : Exception
	{
		public InvalidArgumentException(string message) : base(message) { }
	}
}