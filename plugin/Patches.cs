using HarmonyLib;
using Lod;
using Lod.Dialog;
using Lod.ImageRecognition;
using Lod.Net;
using Lod.TypeX4;
using UnityEngine;
using VisionUSBIO;

namespace tetoco;

public class Patches {
    #region fix boot checks
    [HarmonyPatch(typeof(ArcadeIOManager), "isServiceSWError", MethodType.Getter), HarmonyPrefix]
    public static bool isServiceSWError(ref bool __result) {
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(DepthCamera), "hasCameraProblems", MethodType.Getter), HarmonyPrefix]
    public static bool hasCameraProblems(ref bool __result) {
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(DeviceCheck), "IsTouchPanelEnable"), HarmonyPrefix]
    public static bool IsTouchPanelEnable(ref bool __result) {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(GameInstance), "DummyNesysOnline", MethodType.Getter), HarmonyPrefix]
    public static bool DummyNesysOnline(ref bool __result) {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(VisionUSBIODll), "VisionUSBIOGetStatus"), HarmonyPrefix]
    public static bool VisionUSBIOGetStatus(ref int __result) {
        __result = 512; // ready
        return false;
    }
    #endregion

    [HarmonyPatch(typeof(GameServer), "DebugGetEndpointUrl", MethodType.Getter), HarmonyPrefix]
    public static bool DebugGetEndpointUrl(ref string __result) {
        __result = "http://localhost:8080";
        return false;
    }

    // block screen rotation/resize
    [HarmonyPatch(typeof(ScreenObserver), "Update"), HarmonyPrefix]
    public static bool ScreenObserver_Update() {
        return false;
    }

    [HarmonyPatch(typeof(MachineLocalSettingManager), "IsEnableLanguageSelect", MethodType.Getter), HarmonyPrefix]
    public static bool IsEnableLanguageSelect(ref bool __result) {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(MachineLocalSettingManager), "isFreePlay", MethodType.Getter), HarmonyPrefix]
    public static bool isFreePlay(ref bool __result) {
        __result = true;
        return false;
    }

    #region Disable CountDownTimer
    [HarmonyPatch(typeof(CountDownTimer), "CountDownMode", MethodType.Getter), HarmonyPrefix]
    public static bool CountDownMode(ref CountDownMode __result) {
        __result = Lod.CountDownMode.Disabled;
        return false;
    }

    [HarmonyPatch(typeof(UIPlayerInfoController), "SetCounterShowState"), HarmonyPrefix]
    public static bool SetCounterShowState(ref bool isShow) {
        isShow = false;
        return true;
    }
    /*[HarmonyPatch(typeof(UIPlayerInfoController), "SetExternalCounterActivation"), HarmonyPrefix]
    public static bool SetExternalCounterActivation() {
        return false;
    }*/

    [HarmonyPatch(typeof(DialogBase), "Update"), HarmonyPrefix]
    public static bool DialogBase_Update(ref Context ___m_Context) {
        ___m_Context.data.timeout = -1; // remove timeout
        return true;
    }

    [HarmonyPatch(typeof(ResultContinueView), "CreditAdded", MethodType.Getter), HarmonyPrefix]
    public static bool CreditAdded(ref bool __result) {
        __result = true; // prevent time from running out by always adding credits
        return false;
    }
    #endregion

    [HarmonyPatch(typeof(AeroBootCheck), "CheckAndReset"), HarmonyPrefix]
    public static bool CheckAndReset(ref bool __result) {
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(Lod.Input), "GetKeyDown"), HarmonyPrefix]
    public static bool GetKeyDown(ref bool __result, KeyCode key) {
        __result = UnityEngine.Input.GetKeyDown(key);
        return false;
    }

    [HarmonyPatch(typeof(UIEntryController), "Update"), HarmonyPrefix]
    public static void UIEntryController_Update(UIEntryController __instance) {
        if(GameObject.Find("GuestLoginButton(Clone)") != null)
            return;

        var button = GameObject.Find("GuestLoginButton"); // make copy
        if(button == null)
            return;
        var copy = Object.Instantiate(button);
        copy.transform.SetParent(button.transform.parent, false);

        var pos = button.transform.localPosition;
        button.transform.localPosition = new Vector3(-183, pos.y, pos.z);
        copy.transform.localPosition = new Vector3(183, pos.y, pos.z);

        var text = copy.GetComponentInChildren<UIText>();
        text.textId = "";
        text.text = "Login as\nLocal";

        var btn = copy.GetComponent<UIButton>();
        btn.interactable = true;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            var login = __instance.GetType().GetMethod("Login", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            login.Invoke(__instance, ["1234567890"]);
        });
    }

    [HarmonyPatch(typeof(StageInfo), "CheckPermission"), HarmonyPrefix]
    public static bool CheckPermission(ref bool __result) {
        __result = true;
        return false;
    }
}
