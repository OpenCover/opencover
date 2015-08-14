using System.Runtime.InteropServices;

namespace OpenCover.Framework
{
	internal static class DebugOutput
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern void OutputDebugString(string message);

		public static void Print(string message)
		{
			OutputDebugString(string.Format("OpenCover: {0}", message));
		}
	}
}
