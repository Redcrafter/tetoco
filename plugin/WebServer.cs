using BepInEx.Logging;
using Lod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utf8Json;

namespace tetoco;

public class WebServer {
    private ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Server");
    private HttpListener listener;
    private PlayerSaveData saveData;

    public void Start() {
        Task.Run(WebLoop);
    }

    private async Task WebLoop() {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        while(true) {
            var ctx = await listener.GetContextAsync();
            logger.LogInfo($"{ctx.Request.HttpMethod} {ctx.Request.Url}");

            try {
                var res = Handle(ctx);

                ctx.Response.StatusCode = 200;
                if(res != null) {
                    var json = JsonSerializer.Serialize(res);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength64 = json.Length;
                    ctx.Response.OutputStream.Write(json, 0, json.Length);
                }
            } catch(Exception e) {
                logger.LogError(e);
                ctx.Response.StatusCode = 500;
            }
            ctx.Response.Close();
        }
    }

    private object Handle(HttpListenerContext ctx) {
        string body = null;
        // Logger.LogInfo(ctx.Request.Headers);
        if(ctx.Request.HasEntityBody) {
            using var reader = new StreamReader(ctx.Request.InputStream);
            body = reader.ReadToEnd();
            logger.LogInfo(body);
        }

        var m = ctx.Request.HttpMethod;
        var p = ctx.Request.Url.AbsolutePath;

        if(m == "GET" && p == "/env") { }
        if(m == "GET" && p == "/ngwords") {
            // var param = JsonSerializer.Deserialize<Lod.Net.GameServerRequests.NGWords.Param>(body);
            return new Lod.Net.GameServerRequests.NGWords.Response { result = true };
        }

        if(m == "POST" && p == "/login") {
            Load(); // have to delay load to here cause InitForNESiCA would throw errors

            return new Lod.Net.GameServerRequests.Login.Response {
                accessToken = "1234567890",
                tokenType = "",
                expiresIn = 9999999
            };
        }

        if(m == "GET" && p == "/savedata") {
            // var auth = ctx.Request.Headers.Get("Authorization"); // for real server use auth to get correct save
            return new Lod.Net.GameServerRequests.LoadPlayerSaveData.Response { savedata = saveData };
        }

        if(m == "POST" && p == "/savedata" && body != null) {
            var data = JsonSerializer.Deserialize<Lod.Net.GameServerRequests.SaveData.Param>(body);
            ApplyDiff(data.savedata);
        }
        if(m == "POST" && p == "/game/end" && body != null) {
            var data = JsonSerializer.Deserialize<Lod.Net.GameServerRequests.Game_End.Param>(body);
            ApplyDiff(data.savedata);

            File.AppendAllLines("results.txt", [
                DateTime.Now.ToString("o"),
                    JsonSerializer.ToJsonString(data.result),
                    JsonSerializer.ToJsonString(data.notelogs),
                ]);
        }

        return null;
    }

    private void ApplyDiff(PlayerSaveData diff) {
        saveData.playerData = diff.playerData;
        saveData.systemSettingData = diff.systemSettingData;
        saveData.poseRecordData = diff.poseRecordData;
        saveData.loginBonusProgressData = diff.loginBonusProgressData;
        saveData.collaborationData = diff.collaborationData;
        saveData.poseCardInventoryData = diff.poseCardInventoryData;
        saveData.consumableItemInventoryData = diff.consumableItemInventoryData;
        saveData.shopPreviewHistoryData = diff.shopPreviewHistoryData;
        saveData.reactionHistoryData = diff.reactionHistoryData;
        saveData.outgameTutorialHistoryData = diff.outgameTutorialHistoryData;

        foreach(var item in diff.musicRecordData.records)
            saveData.musicRecordData.records[item.Key] = item.Value;
        saveData.musicRecordData.multiBonusCount = diff.musicRecordData.multiBonusCount;

        Apply(ref saveData.partnerData.partners, diff.partnerData.partners, x => x.characterInfoId);
        Apply(ref saveData.missionProgressData.progresses, diff.missionProgressData.progresses, x => x.MissionInfoId);
        Apply(ref saveData.eventProgressData.progresses, diff.eventProgressData.progresses, x => x.EventId);
        Apply(ref saveData.accessoryInventoryData.contents, diff.accessoryInventoryData.contents, x => x.id);
        Apply(ref saveData.costumeInventoryData.contents, diff.costumeInventoryData.contents, x => x.id);
        Apply(ref saveData.degreeInventoryData.contents, diff.degreeInventoryData.contents, x => x.id);
        Apply(ref saveData.coinInventoryData.contents, diff.coinInventoryData.contents, x => x.id);
        Apply(ref saveData.iconInventoryData.contents, diff.iconInventoryData.contents, x => x.id);

        Save();
    }

    private void Apply<T, K>(ref List<T> to, List<T> diff, Func<T, K> keySelector) {
        var dict = to.ToDictionary(keySelector);
        foreach(var item in diff) {
            dict[keySelector(item)] = item;
        }
        to = dict.Values.ToList();
    }

    private void Load() {
        if(saveData != null)
            return;

        if(File.Exists("save.json")) {
            saveData = JsonSerializer.Deserialize<PlayerSaveData>(File.ReadAllText("save.json"));
        } else {
            saveData = new PlayerSaveData();
            saveData.InitForNESiCA();
            saveData.playerData.playerName = "Player";
            Save();
        }
    }

    private void Save() {
        File.WriteAllBytes("save.json", JsonSerializer.Serialize(saveData));
    }
}