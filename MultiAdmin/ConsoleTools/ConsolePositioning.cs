using System;

namespace MultiAdmin.ConsoleTools
{
	public static class ConsolePositioning
	{
		#region Console Point Properties

		public static BufferPoint BufferCursor
		{
			get => new(Console.CursorLeft, Console.CursorTop);
			set => Console.SetCursorPosition(value.x, value.y);
		}

		public static ConsolePoint ConsoleCursor
		{
			get => BufferCursor.ConsolePoint;
			set => BufferCursor = value.BufferPoint;
		}

		public static BufferPoint BufferLeft
		{
			get => new(0, 0);
		}

		public static BufferPoint BufferRight
		{
			get => new(Console.BufferWidth - 1, 0);
		}

		public static BufferPoint BufferTop
		{
			get => new(0, 0);
		}

		public static BufferPoint BufferBottom
		{
			get => new(0, Console.BufferHeight - 1);
		}

		#endregion
	}

	public readonly struct ConsolePoint
	{
		public readonly int x, y;

		public BufferPoint BufferPoint => new(this);

		public ConsolePoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public ConsolePoint(BufferPoint bufferPoint) : this(bufferPoint.x - Console.WindowLeft,
			bufferPoint.y - Console.WindowTop)
		{
		}

		public void SetAsCursor()
		{
			BufferPoint.SetAsCursor();
		}

		public void SetAsCursorX()
		{
			BufferPoint.SetAsCursorX();
		}

		public void SetAsCursorY()
		{
			BufferPoint.SetAsCursorY();
		}
	}

	public readonly struct BufferPoint
	{
		public readonly int x, y;

		public ConsolePoint ConsolePoint => new(this);

		public BufferPoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public BufferPoint(ConsolePoint consolePoint) : this(consolePoint.x + Console.WindowLeft,
			consolePoint.y + Console.WindowTop)
		{
		}

		public void SetAsCursor()
		{
			ConsolePositioning.BufferCursor = this;
		}

		public void SetAsCursorX()
		{
			Console.CursorLeft = x;
		}

		public void SetAsCursorY()
		{
			Console.CursorTop = y;
		}
	}
}
