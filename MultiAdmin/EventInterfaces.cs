namespace MultiAdmin
{
	public interface IMAEvent
	{
	}

	public interface IEventServerPreStart : IMAEvent
	{
		void OnServerPreStart();
	}

	public interface IEventServerStart : IMAEvent
	{
		void OnServerStart();
	}

	public interface IEventServerStop : IMAEvent
	{
		void OnServerStop();
	}

	public interface IEventRoundEnd : IMAEvent
	{
		void OnRoundEnd();
	}

	public interface IEventWaitingForPlayers : IMAEvent
	{
		void OnWaitingForPlayers();
	}

	public interface IEventRoundStart : IMAEvent
	{
		void OnRoundStart();
	}

	public interface IEventCrash : IMAEvent
	{
		void OnCrash();
	}

	public interface IEventTick : IMAEvent
	{
		void OnTick();
	}

	public interface IEventServerFull : IMAEvent
	{
		void OnServerFull();
	}

	public interface IEventIdleEnter : IMAEvent
	{
		void OnIdleEnter();
	}

	public interface IEventIdleExit : IMAEvent
	{
		void OnIdleExit();
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
