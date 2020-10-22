﻿using System;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Text;
using Transitions.Transitions;
using System.Media;
using System.Collections.Generic;
using System.Security.Cryptography;
using Transitions;
using Transitions.Utilities;
using System.Linq;

namespace HelloWorld
{
	/// <remarks>All of this is example code, but it's at least a little more substantiative than a simple "hello world".</remarks>
	[ExternalTool("Transitions", Description = "Funky Transitions when switching ROMS and Save States")]
//	[ExternalToolApplicability.SingleRom(CoreSystem.NES, "EA343F4E445A9050D4B4FBAC2C77D0693B1D0922")] // example of limiting tool usage (this is SMB1)
	[ExternalToolEmbeddedIcon("Transitions.icon_Hello.ico")]
	public partial class CustomMainForm : Form, IExternalToolForm
	{
		/// <remarks><see cref="RequiredServiceAttribute">RequiredServices</see> are populated by EmuHawk at runtime.</remarks>
		[RequiredService]
		private IEmulator? _emu { get; set; }
		[RequiredApi]
		private IGuiApi? _gui { get; set; }
		[RequiredApi]
		private IEmuClientApi? _emuClient { get; set; }

		private List<string[]> Transitions = new List<string[]>();
		private TransitionBase? CurrentTransition { get; set; } = null;
		private Random random = new Random();
		public CustomMainForm()
		{
			InitializeComponent();
		}
		public void UpdateValues(ToolFormUpdateType type)
		{ 
			switch (type)
			{
				case ToolFormUpdateType.PostFrame:
					//DoScreenUpdate();
					//lblInfo.Text = $"Playing:{CurrentTransition?.IsPlaying}, {CurrentTransition?.CanvasName}, {CurrentTransition?.T}";
					break;
			}
		}
		public void LoadTransitions() {
			
			var transitions = ConfigParser.Parse(Path.Combine(CommonUtilities.RootDirectory, "transitions.txt"));
			Log($"Found {transitions.Count} Transitions");
			foreach(var transition in transitions)
			{
				Log($"> '{transition[0]}': '{transition.Aggregate((a,b)=>$"{a},{b}")}'");
				Transitions.Add(transition); 
			}
		}
		private TransitionBase GetTransition(params string[] args)
		{
			TransitionBase? transition = null;
			switch (args[0])
			{
				case "SoundAndGif":
					float.TryParse(args[3], out var duration);
					transition = new SoundAndGifTransition(args[1], args[2], duration);
					break;
			}
			if (transition != null)
			{
				transition.CanvasType = defaultCanvasType;
				transition.OnLog += (o, msg) => Log(msg);
			}
			return transition;
		}

		private void ClientApi_StateLoaded(object sender, StateLoadedEventArgs e)
		{
			Log("ClientApi_StateLoaded");
			ClientApi.SetSoundOn(true);
			BeginTransition(); 
			//CurrentTransition?.Start();
			//ClientApi.Pause();
		}

		private void ClientApi_BeforeQuickSave(object sender, BeforeQuickSaveEventArgs e)
		{
			Log("Before Quick Save");
			ClientApi.SetSoundOn(false);
		}

		private void ClientApi_BeforeQuickLoad(object sender, BeforeQuickLoadEventArgs e)
		{
			Log("Before Quick Load");
		} 

		private void ClientApi_RomLoaded(object sender, EventArgs e)
		{
			Log("ROM Loaded");
		}

		/// <remarks>This is called once when the form is opened, and every time a new movie session starts.</remarks>
		public void Restart()
		{
			_emuClient.StateLoaded += ClientApi_StateLoaded;
			_emuClient.RomLoaded += ClientApi_RomLoaded;
			_emuClient.BeforeQuickLoad += ClientApi_BeforeQuickLoad;
			_emuClient.BeforeQuickSave += ClientApi_BeforeQuickSave;
			Log("Ready...");
		}

		public void DoScreenUpdate(TransitionBase transition)
		{ 
			transition?.Update(_gui);
		}
		public Action UpdateTransition(int id)
		{
			Log($"Start UpdateTransition({id})");
			//return () => { };
			var t = GetTransition(Transitions[id]);
			CurrentTransition = t;
			var start = DateTime.Now;
			ClientApi.Pause();
			if (t == null) throw new NullReferenceException("Transition cannot be null ya daftie");
			Log("t.Start()");
			_gui.DrawNew("emu", true);
			_gui.DrawRectangle(0, 0, ClientApi.BufferWidth(), ClientApi.BufferHeight());
			_gui.DrawFinish();
			t.Start();
			DoScreenUpdate(t);
			return () =>
			{
				try
				{
					do
					{
						DoScreenUpdate(t);
						Task.Delay(33);
						if (DateTime.Now - start > TimeSpan.FromSeconds(5))
						{
							Log("ERROR: TRANSITION WENT ON TOO LONG");
							break;
						}
					} while (t.IsPlaying);
				} catch(Exception ex)
				{
					Log(ex.StackTrace);
					Log(ex.Message);
				} finally
				{
					Log("End UpdateTransition()");
					ClientApi.Unpause();
					CurrentTransition = null;
				}
			};
		}

		private void CustomMainForm_Load(object sender, EventArgs e)
		{
			LoadTransitions();
		}

		StringBuilder log = new StringBuilder();
		public void Log(string line)
		{
			log.Insert(0,$"{line}\r\n");
			txtLog.Text = log.ToString();
		}

		public bool AskSaveChanges() => true;

		private void btnTransitionSimple_Click(object sender, EventArgs e)
		{
			BeginTransition(); 
		}
		private void BeginTransition()
		{ 
			Task.Run(UpdateTransition(random.Next(0, Transitions.Count))); 
		} 

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			log.Clear();
			txtLog.Text = log.ToString();
		}

		private void toolStripComboBox1_Click(object sender, EventArgs e)
		{

		}
		TransitionBase.EmuCanvasType defaultCanvasType = TransitionBase.EmuCanvasType.Emu;
		private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
		{
			switch((sender as ToolStripComboBox).Text.ToLower())
			{
				case "emu":
					defaultCanvasType = TransitionBase.EmuCanvasType.Emu;
					break;
				case "native":
					defaultCanvasType = TransitionBase.EmuCanvasType.Native;
					break;
			}
		}
	}
}