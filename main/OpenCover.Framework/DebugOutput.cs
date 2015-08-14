using System.Diagnostics;

namespace OpenCover.Framework
{
	internal static class DebugOutput
	{
		public static void Print(string message)
		{
			Debug.WriteLine(string.Format("OpenCover: {0}", message));
		}
	}
}
