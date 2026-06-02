using System;

namespace TodoList
{
	public class EmptyStackException : Exception
	{
		public EmptyStackException(string message) : base(message) { }
	}
}