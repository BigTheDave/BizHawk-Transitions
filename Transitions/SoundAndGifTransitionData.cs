using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Transitions.Utilities;

namespace Transitions
{

	public class SoundAndGifTransitionData : TransitionData
	{
		private SoundPlayer mPlayer;
		private AnimatedGif? mGif;
		private readonly string mSound;
		private readonly string mAnimation;

		public override string Name => $"SoundGif {mSound}";
		public override string Key => $"SoundGif{mSound}{mAnimation}";

		public SoundAndGifTransitionData(string sound, string animation, float duration) : base(duration)
		{
			this.mAnimation = animation;
			this.mSound = sound;
			this.mPlayer = new SoundPlayer(Path.Combine(CommonUtilities.RootDirectory, mSound));
		}
		public override void Draw(IGuiApi gui, double t)
		{
			if (mGif != null)
			{
				var image = mGif.Get((float)t * 1000f);
				var aspect = image.Width / (float)image.Height;
				var screenW = (int)(ScreenHeight * aspect);
				var screenH = ScreenHeight;
				var xOffset = (ScreenWidth - screenW) / 2;
				var yOffset = (ScreenHeight - screenH) / 2;
				gui.DrawImageRegion(image, 0, 0, image.Width, image.Height, xOffset, yOffset, screenW, screenH);
			}
		}

		public override void Play()
		{
			mPlayer.Play();
		}

		public override void Stop()
		{
			mPlayer.Stop();
		}
		public override async Task PreloadAsync()
		{
			IsLoadCompleted = false;
			Task loadAudio = Task.Run(async () =>
			{
				mPlayer.LoadAsync();
				while (!mPlayer.IsLoadCompleted)
				{
					await Task.Delay(10);
				}
			});
			Task<AnimatedGif?> loadGif = AnimatedGif.LoadFromFileAsync(Path.Combine(CommonUtilities.RootDirectory, mAnimation));
			await Task.WhenAll(loadAudio, loadGif);
			mGif = loadGif.Result;
			IsLoadCompleted = true;
			Duration = Math.Max(Duration, (mGif?.Duration / 1000f) ?? 2);

		}
	}
}
