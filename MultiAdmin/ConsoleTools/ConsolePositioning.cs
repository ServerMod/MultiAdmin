﻿using System;

namespace MultiAdmin.ConsoleTools
{
	public static class ConsolePositioning
	{
		#region Console Point Properties

		public static BufferPoint BufferCursor
		{
			get => new BufferPoint(Console.CursorLeft, Console.CursorTop);
			set => Console.SetCursorPosition(value.x, value.y);
		}

		public static ConsolePoint ConsoleCursor
		{
			get => BufferCursor.ConsolePoint;
			set => BufferCursor = value.BufferPoint;
		}

		public static BufferPoint BufferLeft
		{
			get => new BufferPoint(0, 0);
			set => Console.WindowLeft = -value.ConsolePoint.x;
		}

		public static BufferPoint BufferRight
		{
			get => new BufferPoint(Console.BufferWidth - 1, 0);
			set => Console.BufferWidth = value.x + 1;
		}

		public static BufferPoint BufferTop
		{
			get => new BufferPoint(0, 0);
			set => Console.WindowTop = -value.ConsolePoint.y;
		}

		public static BufferPoint BufferBottom
		{
			get => new BufferPoint(0, Console.BufferHeight - 1);
			set => Console.BufferHeight = value.y + 1;
		}

		#endregion
	}

	public struct ConsolePoint
	{
		public readonly int x, y;

		public BufferPoint BufferPoint => new BufferPoint(this);

		public ConsolePoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public ConsolePoint(BufferPoint bufferPoint) : this(bufferPoint.x - Console.WindowLeft, bufferPoint.y - Console.WindowTop)
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

	public struct BufferPoint
	{
		public readonly int x, y;

		public ConsolePoint ConsolePoint => new ConsolePoint(this);

		public BufferPoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public BufferPoint(ConsolePoint consolePoint) : this(consolePoint.x + Console.WindowLeft, consolePoint.y + Console.WindowTop)
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