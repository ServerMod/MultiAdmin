using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin
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

    public interface IEventMatchStart
    {
        void OnMatchStart();
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
        void OnPlayerConnect(String name);
    }

    public interface IEventPlayerDisconnect : IServerMod
    {
        void OnPlayerDisconnect(String name);
    }

	public interface IEventAdminAction : IServerMod
	{
		void OnAdminAction(String message);
	}

	public interface IServerMod
    {
    }


    public interface ICommand
    {
        void OnCall(String[] args);
        String GetCommand();
        String GetUsage();
        Boolean PassToGame();
        String GetCommandDescription();
    }
}
