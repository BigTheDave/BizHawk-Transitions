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
	public abstract class TransitionData
	{
		public virtual int Hash => BitConverter.ToInt32(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Key)),0);
		public int Count = 1;
		public abstract string Key { get; }
		public abstract string Name { get; }
		public bool IsLoadCompleted { get; protected set; }
		public float Duration { get; protected set; }
		public TransitionData(float duration)
		{
			Duration = duration;
		}
		public abstract Task PreloadAsync();
		public abstract void Play();
		public abstract void Stop();
		public int ScreenWidth => ClientApi.BufferWidth();
		public int ScreenHeight => ClientApi.BufferHeight();
		public virtual void DrawStart(IGuiApi gui)
		{
			gui.DrawNew("emu", true);
			gui.SetDefaultBackgroundColor(Color.Black);
			gui.SetDefaultForegroundColor(Color.Black);
			gui.DrawRectangle(0, 0, ScreenWidth, ScreenHeight);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t">value from 0 to 1</param>
		public abstract void Draw(IGuiApi gui, double t);
		public virtual void DrawEnd(IGuiApi gui)
		{
			gui.DrawFinish();
		}
		public virtual void DrawClear(IGuiApi gui)
		{
			gui.DrawNew("emu", true);
			gui.DrawFinish();
		}
	}

}
