namespace MultiAdmin
{
	public interface IEventServerPreStart
	{
		void OnServerPreStart();
	}

	public interface IEventServerStart
	{
		void OnServerStart();
	}

	public interface IEventServerStop
	{
		void OnServerStop();
	}

	public interface IEventRoundEnd
	{
		void OnRoundEnd();
	}

	public interface IEventRoundStart
	{
		void OnRoundStart();
	}

	public interface IEventCrash
	{
		void OnCrash();
	}

	public interface IEventTick
	{
		void OnTick();
	}

	public interface IEventServerFull : IServerMod
	{
		void OnServerFull();
	}

	public interface IEventPlayerConnect : IServerMod
	{
		void OnPlayerConnect(string name);
	}

	public interface IEventPlayerDisconnect : IServerMod
	{
		void OnPlayerDisconnect(string name);
	}

	public interface IEventAdminAction : IServerMod
	{
		void OnAdminAction(string message);
	}

	public interface IServerMod
	{
	}

	public interface ICommand
	{
		void OnCall(string[] args);
		string GetCommand();
		string GetUsage();
		bool PassToGame();
		string GetCommandDescription();
	}
}