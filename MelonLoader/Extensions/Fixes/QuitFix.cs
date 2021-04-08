﻿using System.Diagnostics;

namespace MelonLoader.Fixes
{
	internal static class QuitFix
	{
		internal static void Run()
		{
			if (!MelonCommandLine.Core.QuitFix)
				return;
			Process.GetCurrentProcess().Kill();
		}
	}
}