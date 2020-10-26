﻿using BizHawk.Client.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Transitions.Transitions
{
	public class SimpleTransition : TransitionBase
	{
		private IGuiApi? mGui { get; set; }
		public override string Name => "SimpleTransition";

		public SimpleTransition(float duration) : base(duration)
		{
			OnTransitionStart += (o, e) => new SoundPlayer(Path.Combine(CommonUtilities.RootDirectory,"batman.wav")).Play();
		}

		protected override void Draw(IGuiApi? gui, float T, float dT)
		{
			mGui = gui;
			if(mGui == null)
			{
				Log("GUI is null");
				return;
			}
			mGui.DrawRectangle(0, 0, ScreenWidth, CommonUtilities.Lerp(0, ScreenHeight, T));
		}

		public override void Load()
		{ 
		}

		protected override void DrawEnd(IGuiApi? gui)
		{
			mGui.DrawFinish();
		}
		protected override void DrawStart(IGuiApi? gui)
		{
			mGui.SetDefaultBackgroundColor(Color.Black);
			mGui.SetDefaultForegroundColor(Color.Black);
			mGui.DrawNew(CanvasName, true); 
		} 
	}
}
