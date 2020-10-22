using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Transitions
{
	public static class CommonUtilities
	{
		private static string? _rootDirectory;
		public static string RootDirectory => _rootDirectory ??= Path.Combine(
			new DirectoryInfo(Application.ExecutablePath).Parent.FullName, "ExternalTools", "Transitions");
		private static string ScreenshotPath =>
				Path.Combine(GlobalWin.Config.PathEntries.ScreenshotAbsolutePathFor("Transitions")
				, "Transitions");
		public static string SaveTransitionScreenshot(string file)
		{
			var basePath = ScreenshotPath;
			if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
			ClientApi.Screenshot(Path.Combine(basePath, file));
			return Path.Combine(basePath, file);
		}
		public static float Lerp(float a, float b, float t)
		{
			return t * (b - a) + a;
		}
		public static int Lerp(int a, int b, float t)
		{
			return (int)Math.Round(t * (b - a) + a);
		}
	}
}
