using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiAdmin
{
	public class FileManager
	{
		public static string AppFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
		                                  Path.DirectorySeparatorChar + "SCP Secret Laboratory" +
		                                  Path.DirectorySeparatorChar;

		public static string[] ReadAllLines(string path)
		{
			return File.ReadAllLines(path, Encoding.UTF8);
		}

		public static void WriteToFile(string[] data, string path)
		{
			File.WriteAllLines(path, data, Encoding.UTF8);
		}

		public static void WriteStringToFile(string data, string path)
		{
			File.WriteAllText(path, data, Encoding.UTF8);
		}

		public static void AppendFile(string data, string path, bool newLine = true)
		{
			string[] lines = ReadAllLines(path);
			if (!newLine || lines.Length == 0 || lines[lines.Length - 1].EndsWith(Environment.NewLine) ||
			    lines[lines.Length - 1].EndsWith("\n")) File.AppendAllText(path, data, Encoding.UTF8);
			else File.AppendAllText(path, Environment.NewLine + data, Encoding.UTF8);
		}

		public static void RenameFile(string path, string newpath)
		{
			File.Move(path, newpath);
		}

		public static void DeleteFile(string path)
		{
			File.Delete(path);
		}

		public static void ReplaceLine(int line, string text, string path)
		{
			string[] data = ReadAllLines(path);
			data[line] = text;
			WriteToFile(data, path);
		}

		public static void RemoveEmptyLines(string path)
		{
			string[] data = ReadAllLines(path).Where(s =>
				!string.IsNullOrEmpty(s.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty))).ToArray();
			WriteToFile(data, path);
		}
	}
}