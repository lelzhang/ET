using System;
using System.IO;
using ET;
using UnityEditor;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        private const string CodeDir = "Assets/Res/Code/";
        private const string HotfixDll = "Unity.HotfixView.dll";
        private const string HotfixPdb = "Unity.HotfixView.pdb";

        static Startup()
        {
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "HotfixView.dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "HotfixView.pdb.bytes"), true);
            Log.Info($"复制HotfixView.dll, HotfixView.pdb到Res/Code完成");
            AssetDatabase.Refresh ();
        }
    }
}