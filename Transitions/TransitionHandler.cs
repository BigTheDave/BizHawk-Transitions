using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using HelloWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transitions
{
	public class TransitionHandler
	{
		public IGuiApi? gui { get; set; }
		public bool IsROMReady { get; set; }
		private bool IsPlaying { get; set; }
		protected readonly TransitionData transition; 
		public TransitionHandler(TransitionData transition)
		{
			this.transition = transition;
		}
		private void DrawBackground(IGuiApi gui)
		{
			gui.DrawNew("native", true);
			gui.SetDefaultBackgroundColor(Color.Black);
			gui.SetDefaultForegroundColor(Color.Black);
			gui.DrawRectangle(0, 0, ClientApi.ScreenWidth(), ClientApi.ScreenHeight());
			gui.DrawFinish();
		}
		private void ClearBackground(IGuiApi gui)
		{
			gui.DrawNew("native", true); 
			gui.DrawFinish();
		}
		public event EventHandler? TransitionCompleted;
		public event EventHandler<Action>? InvokeOnMainThread;
		public void PlayAsync()
		{
			ClientApi.Pause();
			ClientApi.SetSoundOn(false);
			Task.Run(async () =>
			{
				try
				{
					IsPlaying = true;
					int waitAttempts = 10;
					while (gui == null) //waiting for GUI
					{
						await Task.Delay(33);
						waitAttempts--;
						if (waitAttempts <= 0) throw new Exception("No GUI Supplied");
					}

					DrawBackground(gui);
					if (!transition.IsLoadCompleted)
					{
						CustomMainForm.Log("Transition isn't ready, preloading");
						await transition.PreloadAsync();
					}
					waitAttempts = 10;
					while (!IsROMReady && IsPlaying)
					{
						await Task.Delay(33);
						waitAttempts--;
						if (waitAttempts <= 0) throw new Exception("ROM took too long to load");
					}
					await Task.Delay(33);
					double deltaTime = 0;
					DateTime n = DateTime.Now;
					InvokeOnMainThread?.Invoke(this, () =>
					 {
						 transition.Play();
					 });
					for (double t = 0; t <= transition.Duration; t += deltaTime)
					{
						if (!IsPlaying) return;
						ClearBackground(gui);
						InvokeOnMainThread?.Invoke(this, () =>
						{
							transition.DrawStart(gui);
							try
							{
								transition.Draw(gui, t);
							}
							catch (Exception ex)
							{
								CustomMainForm.Log(ex.StackTrace);
								CustomMainForm.Log(ex.Message);
								CustomMainForm.Log("Draw Failed");
							}
							finally
							{
								transition.DrawEnd(gui);
							}
						});
						await Task.Delay(33);
						deltaTime = (DateTime.Now - n).TotalSeconds;
						n = DateTime.Now;

					}
				}
				catch (Exception ex)
				{
					CustomMainForm.Log(ex.StackTrace);
					CustomMainForm.Log(ex.Message);
				}
				finally
				{
					CustomMainForm.Log("Transition Complete");
					ClientApi.Unpause();
					ClientApi.SetSoundOn(true);
					transition.DrawClear(gui);
					transition.Stop();
					ClearBackground(gui);
					IsPlaying = false;
					TransitionCompleted?.Invoke(this, null);
				}
			});
		}

		public void Stop()
		{
			IsPlaying = false;
		}
	}
}
