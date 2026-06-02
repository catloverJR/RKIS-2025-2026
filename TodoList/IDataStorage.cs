using System.Collections.Generic;

namespace TodoList
{
	public interface IDataStorage
	{
		void SaveProfile(Profile profile);
		Profile LoadProfile();
		void SaveTodos(TodoList todos);
		TodoList LoadTodos();
	}
}