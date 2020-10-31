using System;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Text; 
using System.Media;
using System.Collections.Generic;
using System.Security.Cryptography;
using Transitions;
using Transitions.Utilities;
using System.Linq;
using System.Runtime.InteropServices;

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

		TransitionLibrary? mTransitionLibrary;
		TransitionLibrary TransitionLibrary => mTransitionLibrary ??= new TransitionLibrary();

		TransitionHandler? currentTransitionHandler;

		public CustomMainForm()
		{
			Instance = this;
			InitializeComponent();
		}
		public void UpdateValues(ToolFormUpdateType type)
		{ 
		}
		public void LoadTransitions()
		{
			TransitionLibrary.LoadProgress += (o, e) =>
			{
				this.Invoke(() =>
				{
					Log($"TransitionLibrary Progress {e.Current}/{e.Maximum} => {e.Status}");
					pbLoading.Visible = true;
					pbLoading.Maximum = e.Maximum;
					pbLoading.Value = e.Current;
					lblStatus.Text = e.Status;
				});
			};
			TransitionLibrary.LoadComplete += (o, e) =>
			{
				this.Invoke(() =>
				{
					Log("TransitionLibrary LoadComplete");
					pbLoading.Visible = false;
					lblStatus.Text = "";
				});
			};
			TransitionLibrary.ListReady += (o, e) =>
			{
				this.Invoke(() =>
				{
					Log("TransitionLibrary ListReady");
					UpdateUI();
				});
			};
			Task.Run(async () => {
				Log($"Loading Transitions");
				await TransitionLibrary.LoadLibraryAsync(Path.Combine(CommonUtilities.RootDirectory, "transitions.txt"),true); 
			});
		}
		void UpdateUI()
		{
			lbTransitions.Items.Clear();
			foreach (var transition in TransitionLibrary.TransitionCache)
			{
				lbTransitions.Items.Add(transition.Value);
			}
		}
		private void ClientApi_StateLoaded(object sender, StateLoadedEventArgs e)
		{
			Log("ClientApi_StateLoaded");
			if(currentTransitionHandler == null)
			{
				PrepareTransition(TransitionLibrary.GetNext());
				currentTransitionHandler.IsROMReady = true;
			}
		}

		private void ClientApi_BeforeQuickSave(object sender, BeforeQuickSaveEventArgs e)
		{
			Log("Before Quick Save");
			//Prepare new Transition
			PrepareTransition(TransitionLibrary.GetNext());
		}


		private void ClientApi_BeforeQuickLoad(object sender, BeforeQuickLoadEventArgs e)
		{
			Log("Before Quick Load");
		} 

		private void ClientApi_RomLoaded(object sender, EventArgs e)
		{
			Log("ROM Loaded");
			//Begin new Random Transition
			if (currentTransitionHandler != null)
			{
				currentTransitionHandler.IsROMReady = true;
			}
		}

		/// <remarks>This is called once when the form is opened, and every time a new movie session starts.</remarks>
		public void Restart()
		{
			_emuClient.StateLoaded += ClientApi_StateLoaded;
			_emuClient.RomLoaded += ClientApi_RomLoaded;
			_emuClient.BeforeQuickLoad += ClientApi_BeforeQuickLoad;
			_emuClient.BeforeQuickSave += ClientApi_BeforeQuickSave;
			if (currentTransitionHandler != null)
			{
				currentTransitionHandler.gui = _gui;
			}
			Log("Ready...");
		}

		
		private void CustomMainForm_Load(object sender, EventArgs e)
		{
			LoadTransitions();
		}

		StringBuilder log = new StringBuilder();
		private static CustomMainForm Instance;
		public static void Log(string line) {
			Instance.log.Insert(0,$"{line}\r\n");
			Instance.Invalidate();
		}

		public bool AskSaveChanges() => true;

		private void PrepareTransition(TransitionData transitionData)
		{
			Log($"PrepareTransition {transitionData}");
			currentTransitionHandler?.Stop();
			currentTransitionHandler = new TransitionHandler(transitionData);
			currentTransitionHandler.gui = _gui;
			currentTransitionHandler.InvokeOnMainThread += (o, e) =>
			{
				this.Invoke(e);
			};
			currentTransitionHandler.PlayAsync(); 
			currentTransitionHandler.TransitionCompleted += (o, e) =>
			{
				if (currentTransitionHandler == o) currentTransitionHandler = null;
			};
		}
		private void btnTransitionSimple_Click(object sender, EventArgs e)
		{
			Log($"Test Transition {lbTransitions.SelectedItem}");
			PrepareTransition((lbTransitions.SelectedItem as TransitionData));
			currentTransitionHandler.IsROMReady = true;
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			log.Clear();
			txtLog.Text = log.ToString();
		}

		private void toolStripComboBox1_Click(object sender, EventArgs e)
		{

		}

		private void CustomMainForm_Paint(object sender, PaintEventArgs e)
		{ 			
			txtLog.Text = Instance.log.ToString();
		}

		private void lbTransitions_SelectedIndexChanged(object sender, EventArgs e)
		{
			var data = (lbTransitions.SelectedItem as TransitionData);			
			chkEnabled.Checked = data?.Enabled ?? false;
			lblDetails.Text = data?.Description ?? "";
			pbPreview.ImageLocation = data?.Preview;
			txtChance.Text = $"{data?.Count}";
			layoutInfo.Visible = true;
		}

		private void chkEnabled_CheckedChanged(object sender, EventArgs e)
		{
			var data = (lbTransitions.SelectedItem as TransitionData);
			if (data == null) return;
			data.Enabled = (sender as CheckBox).Checked;
			lbTransitions.Refresh();
			lbTransitions.Invalidate();

		}

		private void txtChance_TextChanged(object sender, EventArgs e)
		{
			if(int.TryParse(txtChance.Text, out int chance) )
			{
				var data = (lbTransitions.SelectedItem as TransitionData);
				if (data == null) return;
				data.Count = chance;
				lbTransitions.Refresh();
				lbTransitions.Invalidate();
			}
		}
	}
}
