using System;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	internal class FileCopyRoundQueue : Feature, IEventRoundEnd
	{
		private string[] queue = Array.Empty<string>();
		private string[] whitelist = Array.Empty<string>();
		private string[] blacklist = Array.Empty<string>();
		private bool randomizeQueue;
		private int queueIndex;

		public FileCopyRoundQueue(Server server) : base(server)
		{
		}

		public bool HasValidQueue => !queue.IsNullOrEmpty();

		public void OnRoundEnd()
		{
			if (!HasValidQueue) return;

			CopyNextQueueFolder();

			Server.SendMessage("CONFIG RELOAD");
		}

		public void CopyNextQueueFolder()
		{
			if (!HasValidQueue) return;

			queueIndex = LoopingLimitIndex(queueIndex);

			string copyFrom = queue[queueIndex];

			if (string.IsNullOrEmpty(copyFrom)) return;

			Server.CopyFromDir(copyFrom, whitelist, blacklist);

			queueIndex = randomizeQueue ? GetNextRandomIndex() : LoopingLimitIndex(queueIndex + 1);
		}

		private int LoopingLimitIndex(int index)
		{
			if (!HasValidQueue) return 0;

			if (index < 0)
				return queue.Length - 1;

			if (index >= queue.Length)
				return 0;

			return index;
		}

		private int GetNextRandomIndex()
		{
			if (!HasValidQueue) return 0;

			Random random = new();

			int index;
			do
			{
				index = random.Next(0, queue.Length);
			} while (index == queueIndex);

			return index;
		}

		public override void Init()
		{
			queueIndex = 0;

			CopyNextQueueFolder();
		}

		public override void OnConfigReload()
		{
			queue = Server.ServerConfig.FolderCopyRoundQueue.Value ?? Array.Empty<string>();
			whitelist = Server.ServerConfig.FolderCopyRoundQueueWhitelist.Value ?? Array.Empty<string>();
			blacklist = Server.ServerConfig.FolderCopyRoundQueueBlacklist.Value ?? Array.Empty<string>();
			randomizeQueue = Server.ServerConfig.RandomizeFolderCopyRoundQueue.Value;
		}

		public override string GetFeatureDescription()
		{
			return "Copies files from folders in a queue";
		}

		public override string GetFeatureName()
		{
			return "File Copy Round Queue";
		}
	}
}
