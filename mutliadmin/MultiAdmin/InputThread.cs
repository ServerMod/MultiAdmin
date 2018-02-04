using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    server.SendMessage(message);
                }
            }
        }
    }
}
