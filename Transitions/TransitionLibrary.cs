using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using HelloWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks; 
using Transitions.Utilities;

namespace Transitions
{
	public class TransitionLibrary
	{
		public Dictionary<int, TransitionData> TransitionCache = new Dictionary<int, TransitionData>();
		private Random rng = new Random();
		public class TransitionLibraryProgressEventArgs
		{
			public int Current;
			public int Maximum;
			public string Status; 

			public TransitionLibraryProgressEventArgs(int current, int total, string status)
			{
				Current = current;
				Maximum = total;
				Status = status;
			}
		}
		public event EventHandler? LoadComplete, ListReady;
		public event EventHandler<TransitionLibraryProgressEventArgs>? LoadProgress;
		public TransitionData Get(int key)
		{
			return TransitionCache[key];
		}
		public TransitionData GetNext()
		{
			var values = TransitionCache.Values.ToArray();
			var max = values.Sum(kv => kv.Count);
			var roll = rng.Next(0, max);
			var rollLeft = roll;
			foreach(var v in values)
			{
				rollLeft -= v.Count;
				if (rollLeft < 0) return v;
			}
			throw new Exception($"Roll of '{roll}' failed with total {max}");
		}
		public async Task LoadLibraryAsync(string fileName, bool precache = false)
		{
			try
			{
				LoadProgress?.Invoke(this, new TransitionLibraryProgressEventArgs(0, 1, "loading transitions settings"));
				var transitions = await ConfigParser.ParseAsync(fileName);
				foreach (var transition in transitions)
				{
					TransitionData? transitionData = null;
					switch (transition[0])
					{
						case "SoundAndGif":
							float.TryParse(transition[3], out var duration);
							transitionData = new SoundAndGifTransitionData(transition[1], transition[2], duration);

							break;
					}
					if (transitionData != null)
					{
						if (TransitionCache.ContainsKey(transitionData.Hash))
						{
							TransitionCache[transitionData.Hash].Count++;
						}
						else
						{
							TransitionCache.Add(transitionData.Hash, transitionData);
						}
					}
				}
				ListReady?.Invoke(this, null);
				if (precache)
				{
					int current = 0;
					int total = TransitionCache.Count();
					while (current < TransitionCache.Count())
					{
						var toCache = TransitionCache.Skip(current).Take(6).ToList();
						current += toCache.Count();
						Task[] tasks = toCache.Select(tc => tc.Value.PreloadAsync()).ToArray();
						await Task.WhenAll(tasks);
						LoadProgress?.Invoke(this, new TransitionLibraryProgressEventArgs(++current, total, "Preloading"));
							
					}
				}
			}
			catch (Exception ex)
			{
				CustomMainForm.Log($"{ex.StackTrace}");
				CustomMainForm.Log($"{ex.Message}");
			}
			finally
			{
				LoadComplete?.Invoke(this, null);
			}
		}
	}
}
