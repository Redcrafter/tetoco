using BepInEx;
using HarmonyLib;

namespace tetoco;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    private WebServer server;

#pragma warning disable IDE0051 // Remove unused private members
    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.CreateAndPatchAll(typeof(GWSafeFile_Patches));
        Harmony.CreateAndPatchAll(typeof(Patches));

        server = new WebServer();
        server.Start();
    }
#pragma warning restore IDE0051 // Remove unused private members
}
