using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transitions.Utilities
{
	public class AnimatedGif
	{
		public List<AnimationFrame> Frames = new List<AnimationFrame>();
		private int? _duration;
		public int Duration => _duration ??= Frames.Sum(f => f.Duration);
		public float CurrentTime = 0;
		public bool Loop;
		public void AddFrame(Image image, int duration)
		{
			Frames.Add(new AnimationFrame(image, duration));
		}
		public Image Get(float time)
		{
			if (Loop) time %= Duration;
			else if (time > Duration) time = Duration;
			int t = 0;
			foreach (var frame in Frames)
			{
				t += frame.Duration;
				if (t >= time) return frame.Image;
			}
			throw new Exception($"Time '{time}' is greater than Duration '{Duration}'");
		}
		public Image Update(float deltaTime)
		{
			CurrentTime += deltaTime; 
			return Get(CurrentTime);
		}
		public class AnimationFrame
		{
			public Image Image;
			public int Duration;
			public AnimationFrame(Image image, int duration)
			{
				Image = image;
				Duration = duration;
			}
		}
		public static AnimatedGif? LoadFromFile(string path)
		{
			StringBuilder sblog = new StringBuilder();
			var gif = new AnimatedGif();
			if (!File.Exists(path)) throw new FileNotFoundException($"'{path}' not found");

			using (var image = Image.FromFile(path))
			{
				if (!image.RawFormat.Equals(ImageFormat.Gif))
				{
					throw new Exception("Image is not a GIF");
				}
				if (!ImageAnimator.CanAnimate(image))
				{
					throw new Exception("Image cant' animate");
				}
				var fileInfo = new FileInfo(path);
				var dimension = new FrameDimension(image.FrameDimensionsList[0]);
				int frameCount = image.GetFrameCount(dimension);

				int index = 0;
				for (int i = 0; i < frameCount; i++)
				{
					image.SelectActiveFrame(dimension, i);
					var frame = image.Clone() as Image;

					var delay = BitConverter.ToInt32(image.GetPropertyItem(20736).Value, index) * 10;
					gif.AddFrame(frame, delay);

					index += 4;
				}
				gif.Loop = BitConverter.ToInt16(image.GetPropertyItem(20737).Value, 0) != 1;
				return gif;
			}
		}

		public static async Task<AnimatedGif?> LoadFromFileAsync(string path)
		{
			var gif = new AnimatedGif();
			await Task.Run(() =>
			{
				gif = LoadFromFile(path);
			}
			);
			return gif;
		}
	}
}
