using HarmonyLib;
using Lod;
using Lod.Dialog;
using Lod.ImageRecognition;
using Lod.Net;
using Lod.TypeX4;
using System.Collections.Generic;
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
        if(Plugin.useLocalServer.Value) {
            __result = "http://localhost:38817";
        } else {
            __result = Plugin.remoteServer.Value;
        }
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

        btn.onClick.m_PersistentCalls.Clear();
        btn.onClick.AddListener(() => {
            var login = __instance.GetType().GetMethod("Login", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            login.Invoke(__instance, [Plugin.useLocalServer.Value ? "1234567890" : Plugin.cardId.Value]);
        });
    }

    [HarmonyPatch(typeof(StageInfo), "CheckPermission"), HarmonyPrefix]
    public static bool CheckPermission(ref bool __result) {
        __result = true;
        return false;
    }

    #region Hatsune Miku Partner Unlock
    [HarmonyPatch(typeof(PartnerUtil), "CanUse", typeof(Lod.CharacterInfo)), HarmonyPrefix]
    public static bool CanUse(ref bool __result) {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(PartnerUtil), "HasDearness", typeof(Lod.CharacterInfo)), HarmonyPrefix]
    public static bool HasDearness(ref bool __result) {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(UIEntryController), "GoToNextScene"), HarmonyPostfix]
    public static void GoToNextScene(UIEntryController __instance) {
        GameInstance.Instance.ActiveCharacterInfos = CharacterMaster.GetInstance().SortedAllCharacterInfos;
    }

    [HarmonyPatch(typeof(PartnerSelectController), "Start"), HarmonyPrefix]
    public static bool PartnerSelectController_Start(ref PartnerSelectCharaIcon[] ___charaButtonList, ref List<PartnerSelectController.DearnessGauge> ___dearnessGauge) {
        { // character icon
            var button = GameObject.Find("ButtonSet/CHR_C_02");
            var copy = Object.Instantiate(button);
    
            copy.transform.SetParent(button.transform.parent, false);
            copy.transform.localPosition = new Vector3(50, -145, 0);
    
            var comp = copy.GetComponent<PartnerSelectCharaIcon>();
            typeof(PartnerSelectCharaIcon).GetField("CharacterIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(comp, 10);
            ___charaButtonList = ___charaButtonList.AddToArray(comp);
        }
        
        { // dearness display
            var el = GameObject.Find("DearnessVisibleControlObject").transform.Find("CHR_C_02").gameObject;
            var copy = Object.Instantiate(el);

            copy.transform.SetParent(el.transform.parent, false);
            ___dearnessGauge.Add(new PartnerSelectController.DearnessGauge {
                root = copy,
                heart = copy.GetComponentInChildren<UnityEngine.UI.Slider>(),
                level = copy.transform.Find("DearDegreeLevelText").gameObject.GetComponent<UIText>()
            });
        }
    
        return true;
    }

    [HarmonyPatch(typeof(PartnerSelectCharaIcon), "InitCharaIcons"), HarmonyPrefix]
    public static bool InitCharaIcons(ref PartnerSelectCharaIcon __instance, UIImage ____charaIconOn, UIImage ____charaIconOff) {
        if(____charaIconOn == null || ____charaIconOff == null) return false;

        var regularCharacterInfos = CharacterMaster.GetInstance().SortedAllCharacterInfos;
        var characterInfo = regularCharacterInfos[__instance.myIndex];
        __instance.gameObject.SetActive(true);

        // if(GameInstance.Instance.CollaborationManager.ActiveCollaborationInfo != null && characterInfo.id == GameInstance.Instance.CollaborationManager.ActiveCollaborationInfo.CharacterId && __instance.gameObject.name != "Button_Collaboration")
        //    __instance.gameObject.SetActive(false);

        ____charaIconOn.Init(string.Format(characterInfo.partnerSelectIconAssetPath, "ON"));
        ____charaIconOff.Init(string.Format(characterInfo.partnerSelectIconAssetPath, "OFF"));

        return false;
    }
    #endregion
}
