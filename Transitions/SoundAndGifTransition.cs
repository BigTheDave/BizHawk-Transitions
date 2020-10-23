using BizHawk.Client.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Transitions.Utilities;

namespace Transitions.Transitions
{
	public class SoundAndGifTransition : TransitionBase
	{
		private AnimatedGif? Gif;
		private IGuiApi? mGui { get; set; }

		private string mName;
		public override string Name => mName;

		private SoundPlayer mPlayer;
		private string mSound;
		private string mAnimation;

		public SoundAndGifTransition(string sound, string animation, float duration) : base(duration)
		{
			mName = $"{animation}/{sound}";
			mSound = sound;
			mAnimation = animation;
			OnTransitionStart += (o, e) =>
			{
				if (!IsLoaded) Load();
				Gif.CurrentTime = 0;
				mPlayer.Play();				
			};
		}
		public override void Load() { 
			Gif = AnimatedGif.LoadFromFile(Path.Combine(CommonUtilities.RootDirectory, mAnimation));
			OnTransitionEnd += (o, e) =>
			{
				Log("Transition Ended");
				mGui.DrawNew(CanvasName, true);
				mGui.DrawFinish();
			};
			mPlayer = new SoundPlayer(Path.Combine(CommonUtilities.RootDirectory, mSound));
			mPlayer.LoadAsync();
			mDuration = Math.Max(mDuration, (Gif?.Duration /1000f) ?? 2);
			IsLoaded = true;
		}

		protected override void Draw(IGuiApi? gui, float T, float dt)
		{
			mGui = gui;
			if (mGui == null)
			{
				Log("GUI is null");
				return;
			}
			mGui.SetDefaultBackgroundColor(Color.Black);
			mGui.SetDefaultForegroundColor(Color.Black);
			mGui.DrawNew(CanvasName, true);
			mGui.DrawRectangle(0, 0, ScreenWidth,ScreenHeight);
			if (Gif != null )
			{
				var image = Gif.Update(dt * 1000);
				var xOffset = (ScreenWidth - image.Width)/2;
				var yOffset = (ScreenHeight - image.Height)/2;
				mGui.DrawImage(image, xOffset, yOffset);
			}
			mGui.DrawFinish();
		} 
	}
}
