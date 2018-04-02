using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin
{
    class InputThread
    {
        public static void Write(Server server)
        {
            while (!server.IsStopping())
            {
                while (!Console.KeyAvailable)
                {
                    if (server.IsStopping())
                    {
                        return;
                    }
                    Thread.Sleep(server.runOptimized ? 500 : 300);
                }
                string message = Console.ReadLine();
                int cursorTop = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                server.Write(">>> " + message, ConsoleColor.DarkMagenta, -1);
                Console.SetCursorPosition(0, cursorTop);
                string[] strArray = message.ToUpper().Split(' ');
                if (strArray.Length > 0)
                {
                    ICommand command;
                    Boolean callServer = true;
                    server.Commands.TryGetValue(strArray[0].ToLower().Trim(), out command);
                    if (command != null)
                    {
                        command.OnCall(strArray.Skip(1).Take(strArray.Length - 1).ToArray());
                        callServer = command.PassToGame();
                    }

                    if (callServer) server.SendMessage(message);
                }
            }
        }
    }
}
