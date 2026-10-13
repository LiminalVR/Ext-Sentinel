using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Sentinel311OfficialClose
{
    public static string Prepare()
    {
        Sentinel311SourceBootstrap.Guard();
        if (SessionState.GetString("Sentinel311.SourceRoute", "") != "") throw new Exception("Observation still active.");
        string stage = Sentinel311SourceBootstrap.Root + "/Builds/URPMigration/D311-" + DateTime.UtcNow.ToString("MMddTHHmmssfff"); Directory.CreateDirectory(stage);
        var rows = new JArray(); var errors = new JArray();
        foreach (var o in Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Where(o => o != null && EditorUtility.IsDirty(o))) {
            string raw = null; try { raw = EditorJsonUtility.ToJson(o); } catch (Exception e) { errors.Add(e.ToString()); }
            rows.Add(new JObject { ["id"] = Sentinel311SourceBootstrap.ID(o), ["persistent"] = EditorUtility.IsPersistent(o), ["path"] = AssetDatabase.GetAssetPath(o), ["type"] = o.GetType().FullName, ["raw"] = raw });
        }
        var modules = new JArray(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && (a.GetName().Name.StartsWith("App000000000311") || a.GetName().Name.StartsWith("Liminal") || a.GetName().Name == "Core" || a.GetName().Name == "Release")).Select(a => new JObject { ["name"] = a.GetName().Name, ["path"] = a.Location, ["MVID"] = a.ManifestModule.ModuleVersionId.ToString(), ["SHA"] = Sentinel311SourceBootstrap.Hash(a.Location) }));
        File.Copy(Sentinel311SourceBootstrap.Root + "/Tools/Preparation/Sentinel311OfficialClose.cs", stage + "/executed-helper.cs"); File.Copy(Sentinel311SourceBootstrap.RequestPath, stage + "/request.json");
        File.WriteAllText(stage + "/verification.json", new JObject { ["pid"] = Process.GetCurrentProcess().Id, ["sceneClean"] = !SceneManager.GetActiveScene().isDirty, ["dirtyObjects"] = rows, ["serializationErrors"] = errors, ["modules"] = modules, ["indexSHA"] = Sentinel311SourceBootstrap.Hash(Sentinel311SourceBootstrap.IndexPath), ["noSaveOrExitInPrepare"] = true }.ToString());
        if (errors.Count != 0) throw new Exception("Dirty preservation failed; close prohibited."); return stage + "/verification.json";
    }
    public static string Exit()
    {
        Sentinel311SourceBootstrap.Guard(); string path = Sentinel311SourceBootstrap.Root + "/Tools/Preparation/official-close-request.json"; var q = JObject.Parse(File.ReadAllText(path));
        if ((bool?)q["rootApproved"] != true || (int?)q["pid"] != Process.GetCurrentProcess().Id || SessionState.GetString("Sentinel311.SourceRoute", "") != "" ||
            File.Exists(Sentinel311SourceBootstrap.Root + "/Assets/_LimappEditor/Sentinel311SourceObserver.cs") || File.Exists(Sentinel311SourceBootstrap.Root + "/Assets/_LimappEditor/Sentinel311SourceObserver.cs.meta") ||
            Sentinel311SourceBootstrap.Hash(Sentinel311SourceBootstrap.Root + "/Tools/Preparation/Sentinel311OfficialClose.cs") != (string)q["helperSHA"])
            throw new Exception("Reviewed completed-source closure and exact observer absence required.");
        string proof = (string)q["nativeBeforeClose"]; if (Sentinel311SourceBootstrap.Hash(proof) != (string)q["nativeBeforeCloseSHA"]) throw new Exception("Native preservation proof drift.");
        var native = JObject.Parse(File.ReadAllText(proof)); if ((int?)native["pid"] != Process.GetCurrentProcess().Id || (bool?)native["sceneClean"] != true || ((JArray)native["serializationErrors"]).Count != 0) throw new Exception("Native close proof invalid.");
        var pins = q["currentInputs"] as JArray; if (pins == null || pins.Count == 0) throw new Exception("Exact preclose current-input preservation required.");
        foreach (JObject pin in pins) if (Sentinel311SourceBootstrap.Hash((string)pin["path"]) != (string)pin["sha256"]) throw new Exception("Preclose input changed: " + pin["path"]);
        string stage = Sentinel311SourceBootstrap.Root + "/Builds/URPMigration/X311-" + DateTime.UtcNow.ToString("MMddTHHmmssfff"); Directory.CreateDirectory(stage); File.Copy(path, stage + "/executed-request.json");
        EditorApplication.CallbackFunction once = null; once = () => { EditorApplication.update -= once; File.WriteAllText(stage + "/exit-issued.json", new JObject { ["pid"] = Process.GetCurrentProcess().Id, ["utc"] = DateTime.UtcNow.ToString("O"), ["exitCode"] = 0, ["noSaveOrForceKill"] = true }.ToString()); EditorApplication.Exit(0); };
        EditorApplication.update += once; return stage;
    }
}
