using System;
using System.Text;

namespace Shadalyze.Editor.Parser
{
    public static class UnityShaderCompileDataParser
    {
        private const string vertMacro = "#ifdef VERTEX\n";
        private const string fragMacro = "#ifdef FRAGMENT\n";
        private const string endIfMacro = "#endif";
        
        public static bool ParseShader(byte[] bytes, out string vert, out string frag)
        {
            return ParseShader(Encoding.ASCII.GetString(bytes).Trim(), out vert, out frag);
        }

        public static string ParseComputeShader(byte[] bytes)
        {
            return ParseComputeShader(Encoding.ASCII.GetString(bytes).Trim());
        }
        
        private static bool ParseShader(string compiledCode, out string vert, out string frag)
        {
            // version 300 es would occur error when analyze by malioc.
            compiledCode = compiledCode.Replace("#version 300 es", "#version 310 es");
            
            int vertexStartIndex = compiledCode.IndexOf(vertMacro, StringComparison.Ordinal);
            int fragmentStartIndex = compiledCode.IndexOf(fragMacro, StringComparison.Ordinal);
            if (vertexStartIndex == -1 || fragmentStartIndex == -1)
            {
                vert = null;
                frag = null;
                return false;
            }

            int startIndex = fragmentStartIndex + fragMacro.Length;
            int endIndex = compiledCode.LastIndexOf(endIfMacro, StringComparison.Ordinal);
            frag = compiledCode.Substring(startIndex, endIndex - startIndex);
            
            startIndex = vertexStartIndex + vertMacro.Length;
            endIndex = fragmentStartIndex;
            vert = compiledCode.Substring(startIndex, endIndex - startIndex);
            
            vert = vert.Substring(0, vert.LastIndexOf(endIfMacro, StringComparison.Ordinal));
            return true;
        }
        
        private static string ParseComputeShader(string compiledCode)
        {
            return compiledCode;
        }
    }
}
