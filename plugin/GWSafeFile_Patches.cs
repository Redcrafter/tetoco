using HarmonyLib;
using Lod.TypeX4;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Lod.TypeX4.GWSafeFile;
using Utf8Json;
using System.IO;

namespace tetoco;

// GWSafeFile would normally access an encrypted usb drive
internal class GWSafeFile_Patches {
    internal static Dictionary<string, string> storage = new();

    static GWSafeFile_Patches() {
        if(File.Exists("settings.json")) {
            storage = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("settings.json"));
        }
    }

    [HarmonyPatch(typeof(GWSafeFile), "WriteStr"), HarmonyPrefix]
    public static bool WriteStr(string _targetStr, string _baseName, int _MaxLog, [MarshalAs(UnmanagedType.Bool)] bool _bUseCommit = false) {
        storage[_baseName] = _targetStr;
        File.WriteAllText("settings.json", JsonSerializer.ToJsonString(storage));
        return false;
    }

    [HarmonyPatch(typeof(GWSafeFile), "ReadStr"), HarmonyPrefix]
    public static bool ReadStr(string _baseName, ref string __result) {
        storage.TryGetValue(_baseName, out __result);
        return false;
    }

    [HarmonyPatch(typeof(GWSafeFile), "GetError"), HarmonyPrefix]
    public static bool GetError(ref FILE_ERROR __result) {
        __result = FILE_ERROR.ERROR_NONE;
        return false;
    }
}
