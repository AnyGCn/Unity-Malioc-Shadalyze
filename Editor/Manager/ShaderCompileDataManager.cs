using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Shadalyze.Editor.Data;
using UnityEngine;

namespace Shadalyze.Editor.Manager
{
    /// <summary>
    /// Cache compiled and analyzed shader data
    /// </summary>
    public static class ShaderCompileDataManager
    {
        private static SHA256 encoderSHA256 = SHA256.Create();
        
        internal static void DumpToFile(string filePath, string content, int bufferSize)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize);
            using StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(content);
        }
        
        internal static string GetSHA256(string data)
        {
            return GetSHA256(Encoding.UTF8.GetBytes(data));
        }
        
        internal static string GetSHA256(byte[] data)
        {
            return BitConverter.ToString(encoderSHA256.ComputeHash(data)).Replace("-","");
        }
        
        internal static bool IsShaderCompileCodeInCache(string fileName)
        {
            string path = $"{ShadalyzeGlobalSettings.CompileCodePath}/{fileName}";
            return File.Exists(path + ".vert") && File.Exists(path + ".frag");
        }
    }
}
