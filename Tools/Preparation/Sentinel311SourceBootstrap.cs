using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Prepared outside Assets; no authoring saves, Play, renderer warmup or source repair.
public static class Sentinel311SourceBootstrap
{
    public const string Root = "D:/Work/Liminal/Platform/Upgrade/Limapps/311_Sentinel";
    public const string IndexPath = "D:/Work/Liminal/Platform/Upgrade/Limapps/311_Sentinel/.git/index";
    public const string Scene = "Assets/Scenes/TurretGame.unity";
    public const string SceneGUID = "68f5e153123670b42b535ff225cedb91";
    public const string RequestPath = Root + "/Tools/Preparation/source-bootstrap-request.json";
    public static string Hash(string path) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
    public static string ID(UnityEngine.Object o) => o == null ? null : GlobalObjectId.GetGlobalObjectIdSlow(o).ToString();
    public static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;
    public static Component[] Components() => Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Where(s=>s.isLoaded).SelectMany(s=>s.GetRootGameObjects()).SelectMany(g=>g.GetComponentsInChildren<Component>(true)).Where(c=>c!=null).ToArray();
    public static Component ProjectComponent(Component[] all, string fullName) => all.Single(c => c.GetType().FullName == fullName && c.GetType().Assembly.GetName().Name == "App000000000311");
    public static Component SourceApp(Component[] all) => all.Single(c=>{for(var t=c.GetType();t!=null;t=t.BaseType)if(t.FullName=="Liminal.SDK.Core.ExperienceApp")return true;return false;});
    public static PlayableDirector MainDirector(Component app) {var f=app.GetType().GetField("Timeline",BindingFlags.Public|BindingFlags.Instance);return f!=null&&f.FieldType==typeof(PlayableDirector)?(PlayableDirector)f.GetValue(app):null;}
    public static Type CoreType(Component app) { for (var t = app.GetType(); t != null; t = t.BaseType) if (t.FullName == "Liminal.SDK.Core.ExperienceApp") return t; throw new Exception("Actual loaded MyExperienceApp inheritance lacks SDK Core."); }

    public static object PublicSDKRead(object target,string name)
    {
        if(target==null)return null;var type=target.GetType();var f=type.GetField(name,BindingFlags.Instance|BindingFlags.Public);if(f!=null)return f.GetValue(target);
        var p=type.GetProperty(name,BindingFlags.Instance|BindingFlags.Public);if(p==null||!p.CanRead||p.GetIndexParameters().Length!=0)throw new Exception("Authentic public SDK observation member absent: "+type.FullName+"."+name);return p.GetValue(target);
    }
    public static JObject SDKHostContract(Component app)
    {
        var types=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Liminal.SDK.V2.ExperienceAppManager",false)).Where(t=>t!=null).ToArray();
        if(types.Length!=1)throw new Exception("Exactly one genuine loaded SDK V2 manager type required");var type=types[0];
        var appField=type.GetField("ExperienceApp",BindingFlags.Public|BindingFlags.Instance);var headField=type.GetField("AvatarHead",BindingFlags.Public|BindingFlags.Instance);var avatarField=type.GetField("VRAvatar",BindingFlags.Public|BindingFlags.Instance);
        var rig=type.GetProperty("SpawnedRig",BindingFlags.Public|BindingFlags.Instance);var origin=type.GetProperty("XROrigin",BindingFlags.Public|BindingFlags.Instance);
        if(appField==null||!appField.FieldType.IsInstanceOfType(app)||headField==null||avatarField==null||rig==null||!rig.CanRead||origin==null||!origin.CanRead)throw new Exception("Actual SDK App/head/avatar/rig public contracts missing");
        var camera=headField.FieldType.GetProperty("CenterEyeCamera",BindingFlags.Public|BindingFlags.Instance);
        if(camera==null||!camera.CanRead||camera.PropertyType!=typeof(Camera))throw new Exception("Actual AvatarHead.CenterEyeCamera contract missing");
        return new JObject{["managerType"]=type.FullName,["managerModule"]=type.Assembly.Location,["managerModuleSHA"]=Hash(type.Assembly.Location),["managerMVID"]=type.Module.ModuleVersionId.ToString(),["headType"]=headField.FieldType.FullName,["headModule"]=headField.FieldType.Assembly.Location,["headMVID"]=headField.FieldType.Module.ModuleVersionId.ToString(),["cameraContract"]=camera.ToString(),["scope"]="Loaded public contracts only; active association waits ordinary SDK startup, no activation/reparenting/manual registration"};
    }
    public static Camera SDKOwnedCamera(Component[] all,Component app,out JObject receipt)
    {
        receipt=null;SDKHostContract(app);
        if(!(app is Behaviour)||!((Behaviour)app).isActiveAndEnabled)return null;
        var hosts=all.Where(c=>c.GetType().FullName=="Liminal.SDK.V2.ExperienceAppManager"&&c is Behaviour&&((Behaviour)c).isActiveAndEnabled&&ReferenceEquals(PublicSDKRead(c,"ExperienceApp"),app)).ToArray();
        if(hosts.Length==0)return null;if(hosts.Length!=1)throw new Exception("Ambiguous active SDK manager for exact source App");var host=hosts[0];
        var head=PublicSDKRead(host,"AvatarHead") as Component;var avatar=PublicSDKRead(host,"VRAvatar") as Component;var rig=PublicSDKRead(host,"SpawnedRig") as UnityEngine.Object;var origin=PublicSDKRead(host,"XROrigin");
        if(head==null||avatar==null||rig==null||origin==null)return null;
        if(!ReferenceEquals(PublicSDKRead(avatar,"Head"),head))throw new Exception("SDK avatar.Head differs from manager registered head");
        var camera=PublicSDKRead(head,"CenterEyeCamera") as Camera;if(camera==null||!camera.isActiveAndEnabled)return null;
        if(!ReferenceEquals(PublicSDKRead(origin,"Camera"),camera))throw new Exception("SDK XROrigin.Camera differs from registered AvatarHead.CenterEyeCamera");
        receipt=new JObject{["app"]=ID(app),["appHierarchy"]=Hierarchy(app.transform),["appModule"]=app.GetType().Assembly.Location,["appMVID"]=app.GetType().Module.ModuleVersionId.ToString(),["appActive"]=true,["manager"]=ID(host),["managerHierarchy"]=Hierarchy(host.transform),["managerModule"]=host.GetType().Assembly.Location,["managerModuleSHA"]=Hash(host.GetType().Assembly.Location),["managerMVID"]=host.GetType().Module.ModuleVersionId.ToString(),["managerAppReferenceExact"]=true,["avatar"]=ID(avatar),["head"]=ID(head),["headHierarchy"]=Hierarchy(head.transform),["headModule"]=head.GetType().Assembly.Location,["headMVID"]=head.GetType().Module.ModuleVersionId.ToString(),["rig"]=ID(rig),["camera"]=ID(camera),["cameraHierarchy"]=Hierarchy(camera.transform),["registeredCenterEyeAndOriginExact"]=true,["CameraMainIdentitySampleOnly"]=ID(Camera.main),["manualRegistrationOrActivation"]=false};return camera;
    }

    public static JObject Guard()
    {
        var q = JObject.Parse(File.ReadAllText(RequestPath));
        if ((bool?)q["rootLeased"] != true || (int?)q["pid"] != Process.GetCurrentProcess().Id || Path.GetDirectoryName(Application.dataPath).Replace('\\','/') != Root ||
            !Application.isBatchMode || Application.unityVersion != "6000.7.0b2" || SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11 || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android ||
            EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating || SceneManager.GetActiveScene().isDirty ||
            GraphicsSettings.defaultRenderPipeline != null || GraphicsSettings.currentRenderPipeline != null || QualitySettings.renderPipeline != null ||
            AssetDatabase.AssetPathToGUID(Scene) != SceneGUID || Hash(IndexPath) != (string)q["indexSHA"])
            throw new Exception("Exact leased/stopped/clean/source Builtin graphics-batch Android session/index required.");
        var pins = q["currentPins"] as JArray; if (pins == null || pins.Count == 0) throw new Exception("Concrete source pins required.");
        foreach (JObject p in pins) if (Hash((string)p["path"]) != (string)p["sha256"]) throw new Exception("Current input mismatch: " + p["path"]);
        var enabled = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray(); if (enabled.Length != 1 || enabled[0].path != Scene) throw new Exception("Authored enabled scene changed.");
        return q;
    }
    public static void Begin()
    {
        double start = EditorApplication.timeSinceStartup; EditorApplication.CallbackFunction tick = null;
        tick = () => {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
            var q = JObject.Parse(File.ReadAllText(RequestPath)); if ((int?)q["pid"] != Process.GetCurrentProcess().Id && EditorApplication.timeSinceStartup-start < 120) return;
            EditorApplication.update -= tick;
            string stage = Root + "/Builds/URPMigration/B311-" + DateTime.UtcNow.ToString("MMddTHHmmssfff"); Directory.CreateDirectory(stage);
            File.Copy(Root + "/Tools/Preparation/Sentinel311SourceBootstrap.cs", stage + "/executed-helper.cs"); File.Copy(RequestPath, stage + "/executed-request.json");
            var r = new JObject { ["pid"] = Process.GetCurrentProcess().Id, ["importTerminal"] = true, ["compiling"] = EditorApplication.isCompiling, ["updating"] = EditorApplication.isUpdating,
                ["batch"] = Application.isBatchMode, ["backend"] = SystemInfo.graphicsDeviceType.ToString(), ["graphicsDevice"] = SystemInfo.graphicsDeviceName };
            try { r["source"] = OpenCleanEmptyAuthored(); r["sourceGatePassed"] = true; } catch (Exception e) { r["sourceGatePassed"] = false; r["failure"] = e.ToString(); }
            File.WriteAllText(stage + "/verification.json", r.ToString()); // Remain stopped on a genuine gate failure; no timeout restart/Exit.
        }; EditorApplication.update += tick;
    }
    public static JObject OpenCleanEmptyAuthored()
    {
        Guard(); var s = SceneManager.GetActiveScene();
        if (s.path != Scene) { if (s.isDirty || s.rootCount != 0 || !string.IsNullOrEmpty(s.path)) throw new Exception("Refusing replacement of nonempty scene."); EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single); }
        return Inventory();
    }
    public static JObject Inventory()
    {
        Guard(); if (SceneManager.GetActiveScene().path != Scene) throw new Exception("Exact authored scene required.");
        var all = Components(); var app = SourceApp(all); var core = CoreType(app);
        var sourceComponents=all.Where(c=>c.GetType().Assembly.GetName().Name=="App000000000311"&&new[]{"Cannon","SpawnerManager","GameManager","EnemyHealth","EnemyShip","PowerUp","Projectile"}.Contains(c.GetType().FullName)).ToArray();
        var directors = all.OfType<PlayableDirector>().ToArray(); var main = MainDirector(app); 
        int missing = all.OfType<Transform>().Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        var callback = core.GetEvent("OnComplete",BindingFlags.Public|BindingFlags.Static);
        if(missing!=0||callback==null||callback.EventHandlerType!=typeof(Action<bool>))throw new Exception("Missing scripts/actual Core callback/selected source binding gate failed.");
        var renderers = all.OfType<Renderer>().ToArray(); var mats = renderers.SelectMany(r=>r.sharedMaterials).Concat(new[]{RenderSettings.skybox}).Where(m=>m!=null).Distinct();
        return new JObject {
            ["scene"] = Scene, ["sceneClean"] = !SceneManager.GetActiveScene().isDirty, ["missingScripts"] = missing,
            ["app"] = ID(app), ["appRaw"] = EditorJsonUtility.ToJson(app), ["appModule"] = app.GetType().Assembly.Location, ["appMVID"] = app.GetType().Module.ModuleVersionId.ToString(),
            ["SDKHostPublicContract"]=SDKHostContract(app), ["coreType"] = core.FullName, ["coreModule"] = core.Assembly.Location, ["coreMVID"] = core.Module.ModuleVersionId.ToString(), ["SDKCallback"] = callback.ToString(),
            ["sourceRuntimeComponents"]=new JArray(sourceComponents.Select(c=>new JObject{["id"]=ID(c),["type"]=c.GetType().FullName,["hierarchy"]=Hierarchy(c.transform),["enabled"]=c is Behaviour?(bool?)((Behaviour)c).enabled:null,["active"]=c.gameObject.activeInHierarchy,["raw"]=EditorJsonUtility.ToJson(c)})),
            ["hostManagers"]=new JArray(all.Where(c=>c.GetType().FullName=="Liminal.Shared.LimappManager").Select(c=>new JObject{["id"]=ID(c),["hierarchy"]=Hierarchy(c.transform),["raw"]=EditorJsonUtility.ToJson(c),["module"]=c.GetType().Assembly.Location})),
            ["directors"] = new JArray(directors.Select(d=>new JObject { ["id"]=ID(d), ["path"]=AssetDatabase.GetAssetPath(d.playableAsset), ["duration"]=d.duration, ["time"]=d.time, ["state"]=d.state.ToString(), ["raw"]=EditorJsonUtility.ToJson(d) })),
            ["quality"] = QualitySettings.GetQualityLevel(), ["pixelLightCount"] = QualitySettings.pixelLightCount, ["AA"] = QualitySettings.antiAliasing, ["lightFlags"] = new JArray(GraphicsSettings.lightsUseLinearIntensity, GraphicsSettings.lightsUseColorTemperature),
            ["ambientMode"] = RenderSettings.ambientMode.ToString(), ["ambientProbe"] = JsonUtility.ToJson(RenderSettings.ambientProbe), ["sky"] = ID(RenderSettings.skybox), ["sun"] = ID(RenderSettings.sun),
            ["lightmaps"] = new JArray(LightmapSettings.lightmaps.Select(l=>new JObject { ["color"]=ID(l.lightmapColor), ["direction"]=ID(l.lightmapDir), ["mask"]=ID(l.shadowMask) })),
            ["lights"] = new JArray(all.OfType<Light>().Select(l=>new JObject { ["id"]=ID(l), ["active"]=l.isActiveAndEnabled, ["bakeType"]=l.lightmapBakeType.ToString(), ["bakingOutput"]=JsonUtility.ToJson(l.bakingOutput), ["raw"]=EditorJsonUtility.ToJson(l) })),
            ["renderers"] = new JArray(renderers.Select(r=>new JObject { ["id"]=ID(r), ["hierarchy"]=Hierarchy(r.transform), ["active"]=r.gameObject.activeInHierarchy, ["enabled"]=r.enabled, ["slots"]=new JArray(r.sharedMaterials.Select(ID)), ["lightmapIndex"]=r.lightmapIndex, ["hasMPB"]=r.HasPropertyBlock() })),
            ["materials"] = new JArray(mats.Select(m=>new JObject { ["id"]=ID(m), ["path"]=AssetDatabase.GetAssetPath(m), ["shader"]=m.shader?.name, ["shaderID"]=ID(m.shader), ["supported"]=m.shader!=null&&m.shader.isSupported, ["raw"]=EditorJsonUtility.ToJson(m), ["headers"]=m.shader==null?new JArray():new JArray(Enumerable.Range(0,m.shader.GetPropertyCount()).Select(i=>new JObject { ["name"]=m.shader.GetPropertyName(i), ["type"]=m.shader.GetPropertyType(i).ToString(), ["flags"]=m.shader.GetPropertyFlags(i).ToString(), ["attributes"]=new JArray(m.shader.GetPropertyAttributes(i)) })) })),
            ["XRObjects"] = new JArray(AssetDatabase.FindAssets("t:ScriptableObject",new[]{"Assets/XR"}).Select(g=>AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(g))).Where(o=>o!=null).Select(o=>new JObject { ["id"]=ID(o), ["type"]=o.GetType().FullName, ["raw"]=EditorJsonUtility.ToJson(o) })),
            ["sourceRouteQualification"] = "Source estimated249.1167 from ordinary grab only: last wave +7s +12s dialogue activates ending Timeline; fade13.9833(2s), End16.1167. Actual native wave/graph/host/SDK callback required, not forced estimated timeout.",
            ["limits"] = "Stopped loaded source only. Actual SDK start/Timeline graph/renderer visibility/perdraw uniforms/controller physics/SDK ending remain separate. No Play/save/render/pipeline changes."
        };
    }
}
