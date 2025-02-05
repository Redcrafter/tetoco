using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace tetoco;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static ConfigEntry<bool> useLocalServer;
    internal static ConfigEntry<string> remoteServer;
    internal static ConfigEntry<string> cardId;

    private WebServer server;

#pragma warning disable IDE0051 // Remove unused private members
    private void Awake() {
        LoadConfig();

        Harmony.CreateAndPatchAll(typeof(GWSafeFile_Patches));
        Harmony.CreateAndPatchAll(typeof(Patches));

        if(useLocalServer.Value) {
            server = new WebServer();
            server.Start();
        }

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
#pragma warning restore IDE0051 // Remove unused private members

    public void LoadConfig() {
        useLocalServer = Config.Bind("General", "UseLocalServer", true, "Host server for local play");
        remoteServer = Config.Bind("General", "RemoteServer", "", "Remote server URL");
        cardId = Config.Bind("General", "CardId", "", "Used to authenticate with a remote server");
    }
}
