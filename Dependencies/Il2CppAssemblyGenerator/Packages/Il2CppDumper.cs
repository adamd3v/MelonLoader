﻿using System.IO;
using MelonLoader.Il2CppAssemblyGenerator.Packages;
using MelonLoader.TinyJSON;

namespace MelonLoader.Il2CppAssemblyGenerator
{
    internal class Il2CppDumper : DumperBase
    {
        internal Il2CppDumper()
        {
            Version = "6.6.5";
            URL = "https://github.com/Perfare/Il2CppDumper/releases/download/v" + Version + "/Il2CppDumper-v" + Version + ".zip";
            Destination = Path.Combine(Core.BasePath, "Il2CppDumper");
            Output = Path.Combine(Destination, "DummyDll");
            ExePath = Path.Combine(Destination, "Il2CppDumper.exe");
        }

        private void Save()
        {
            Config.Values.DumperVersion = Version;
            Config.Save();
        }

        private bool ShouldDownload() => (
            string.IsNullOrEmpty(Config.Values.DumperVersion)
            || !Config.Values.DumperVersion.Equals(Version));

        internal override void Cleanup()
        {
            string dumpcspath = Path.Combine(Destination, "dump.cs");
            if (File.Exists(dumpcspath))
                File.Delete(dumpcspath);
        }

        internal override bool Download()
        {
            if (!ShouldDownload())
            {
                MelonLogger.Msg("Il2CppDumper is up to date. No Download Needed.");
                return true;
            }
            MelonLogger.Msg("Downloading Il2CppDumper...");
            if (base.Download())
            {
                Save();
                return true;
            }

            ThrowInternalFailure("Failed to Download Il2CppDumper!");
            return false;
        }

        internal override bool Execute()
        {
            FixConfig();
            MelonLogger.Msg("Executing Il2CppDumper...");
            if (Execute(new string[] {
                Core.GameAssemblyPath,
                Path.Combine(MelonUtils.GetGameDataDirectory(), "il2cpp_data", "Metadata", "global-metadata.dat")
            }))
                return true;

            ThrowInternalFailure("Failed to Execute Il2CppDumper!");
            return false;
        }

        private void FixConfig() => File.WriteAllText(Path.Combine(Destination, "config.json"), Encoder.Encode(new Il2CppDumperConfig(), EncodeOptions.NoTypeHints | EncodeOptions.PrettyPrint));
    }

    internal class Il2CppDumperConfig
    {
        public bool DumpMethod = true;
        public bool DumpField = true;
        public bool DumpProperty = true;
        public bool DumpAttribute = true;
        public bool DumpFieldOffset = false;
        public bool DumpMethodOffset = false;
        public bool DumpTypeDefIndex = false;
        public bool GenerateDummyDll = true;
        public bool GenerateScript = false;
        public bool RequireAnyKey = false;
        public bool ForceIl2CppVersion = false;
        public float ForceVersion = 24.3f;
    }
}