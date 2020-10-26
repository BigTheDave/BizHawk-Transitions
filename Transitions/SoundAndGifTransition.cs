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
		public override string CanvasName => "emu";
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
			if (Gif != null )
			{
				var image = Gif.Update(dt * 1000);
				var aspect = image.Width / (float)image.Height;
				var screenW = (int)(ScreenHeight * aspect);
				var screenH = ScreenHeight;
				var xOffset = (ScreenWidth - screenW) /2;
				var yOffset = (ScreenHeight - screenH) /2;
				gui.DrawImageRegion(image, 0, 0,image.Width,image.Height, xOffset, yOffset, screenW, screenH);
			} 
		}

		protected override void DrawEnd(IGuiApi? gui)
		{
			gui.DrawFinish();
		}
		protected override void DrawStart(IGuiApi? gui)
		{
			mGui = gui;
			if (gui == null)
			{
				Log("GUI is null");
				return;
			} 
			gui.SetDefaultBackgroundColor(Color.Black);
			gui.SetDefaultForegroundColor(Color.Black); 
			gui.DrawNew(CanvasName, true);
			gui.DrawRectangle(0, 0, ScreenWidth, ScreenHeight);
		}
	}
}
