using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Transitions.Transitions
{
	public abstract class TransitionBase
	{
		public enum EmuCanvasType
		{
			Emu,
			Native
		}
		private EmuCanvasType mCanvasType;
		public abstract string Name { get; }
		public EmuCanvasType CanvasType
		{
			get
			{
				return mCanvasType;
			}
			set
			{
				if (mCanvasType != value)
				{
					mCanvasType = value;
					mCanvasName = string.Empty;
				}
			}
		}
		private string mCanvasName;
		public virtual string CanvasName => string.IsNullOrWhiteSpace(mCanvasName) ? (mCanvasName = CanvasType == EmuCanvasType.Emu ? "emu" : "native") : mCanvasName;
		public event EventHandler? OnTransitionStart,OnTransitionMidpoint,OnTransitionEnd;  
		public float T { get; protected set; } = 0;
		protected float mDeltaTime = 0.0166666f;
		protected float mDuration;
		public int ScreenWidth => (CanvasType == EmuCanvasType.Emu) ? ClientApi.BufferWidth() : ClientApi.ScreenWidth();
		public int ScreenHeight => (CanvasType == EmuCanvasType.Emu) ? ClientApi.BufferHeight() : ClientApi.ScreenHeight();
		public bool IsPlaying { get; private set; }
		public bool IsLoaded { get; protected set; }
		protected void Log(string msg)
		{

			HelloWorld.CustomMainForm.Log($"{this.GetType().Name} => '{msg}'");
		}
		private long _dt;
		public TransitionBase(float Duration)
		{
			mDuration = Duration;
			_dt = DateTime.Now.Ticks;
			CanvasType = EmuCanvasType.Native;			
		}
		public abstract void Load();
		
		public virtual void Start()
		{
			Log("Start");
			IsPlaying = true;
			T = 0;
			_dt = DateTime.Now.Ticks;
			OnTransitionStart?.Invoke(this, null);
		}
		public virtual void Stop()
		{
			Log("Stop"); 
			IsPlaying = false;
			 T = 0;
			OnTransitionEnd?.Invoke(this, null);
		} 
		protected abstract void Draw(IGuiApi? gui, float T, float dT);
		protected abstract void DrawStart(IGuiApi? gui);
		protected abstract void DrawEnd(IGuiApi? gui);
		public void Update(IGuiApi? gui)
		{
			if (gui == null) return;
			try
			{
				DrawStart(gui);
				if (!IsPlaying)
				{
					Log("Stopped");
					DrawEnd(gui);
					return;
				}

				mDeltaTime = (DateTime.Now.Ticks - _dt) / (float)TimeSpan.TicksPerSecond;
				_dt = DateTime.Now.Ticks;

				T += (mDeltaTime / mDuration);
				Draw(gui, (T > 1) ? 1 : (T < 0) ? 0 : T, mDeltaTime);

				if (T >= 1)
				{
					Log("End of Transition");
					Stop();
				}
			} catch(Exception ex)
			{
				Log(ex.StackTrace);
				Log(ex.Message);

			} finally
			{
				DrawEnd(gui);
			}
		}
	}
}
