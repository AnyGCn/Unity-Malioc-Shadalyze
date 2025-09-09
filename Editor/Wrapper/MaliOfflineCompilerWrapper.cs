using System;
using System.Diagnostics;

namespace Shadalyze.Editor.Wrapper
{
    internal enum MaliDeviceType
    {
        // Arm 5th Generation architecture
        Immortalis_G925,
        Immortalis_G720,
        Mali_G725,
        Mali_G720,
        Mali_G625,
        Mali_G620,

        // Valhall architecture
        Immortalis_G715,
        Mali_G715,
        Mali_G710,
        Mali_G615,
        Mali_G610,
        Mali_G510,
        Mali_G310,
        Mali_G78AE,
        Mali_G78,
        Mali_G77,
        Mali_G68,
        Mali_G57,

        // Bifrost architecture
        Mali_G76,
        Mali_G72,
        Mali_G71,
        Mali_G52,
        Mali_G51,
        Mali_G31,

        // Midgard architecture
        Mali_T880,
        Mali_T860,
        Mali_T830,
        Mali_T820,
        Mali_T760,
        Mali_T720,
    }
    
    /// <summary>
    /// Wrapper for Mali Offline Compiler.
    /// </summary>
    public static class MaliOfflineCompilerWrapper
    {
        public static bool Analyze(string fileName, out string output)
        {
            using Process p = new Process();
            p.StartInfo = new ProcessStartInfo(ShadalyzeGlobalSettings.MaliocExePath)
            {
                ArgumentList = { fileName, $"-c {ShadalyzeGlobalSettings.Instance.BaselineDevice.ToString().Replace('_', '-')}" },
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.ASCII,
            };
            
            p.Start();
            p.WaitForExit();
            bool success = p.ExitCode == 0;
            output = success ? p.StandardOutput.ReadToEnd().Trim() : String.Empty;
            if (!success) UnityEngine.Debug.LogError(p.StandardError.ReadToEnd().Trim());
            return success;
        }
    }
}
