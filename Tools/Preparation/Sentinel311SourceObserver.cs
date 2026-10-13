using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

// Passive ordinary SDK/Timeline/DSP source observation; never call project gameplay handlers.
[InitializeOnLoad] public static class Sentinel311SourceObserver
{
    public const string Key = "Sentinel311.SourceRoute";
    static string stage; static bool done, sdkComplete, sdkResult, progressPassed, timelineStopped;
    static int lastFrame=-1, checkpoint; static double firstWall=-1, firstDSP=-1, initialTimeline; static float nextSample, completionTime;
    static bool bound;static double readinessStart;static JObject nativeCameraOwnership; static Component manager; static Component control; static PlayableDirector director; static EventInfo sdkEvent; static Action<bool> sdkHandler;
    static JArray samples, events, errors, captures;
    static readonly double[] checkpoints = {6,60,120,180,240};
    [StructLayout(LayoutKind.Sequential)] struct MemoryStatus { public uint length, load; public ulong totalPhysical, freePhysical, totalPage, freePage, totalVirtual, freeVirtual, extended; }
    [DllImport("kernel32.dll",SetLastError=true)] static extern bool GlobalMemoryStatusEx(ref MemoryStatus m);
    static long FreeBytes() { var m = new MemoryStatus { length=(uint)Marshal.SizeOf(typeof(MemoryStatus)) }; if (!GlobalMemoryStatusEx(ref m)) throw new Exception("Native physical-memory read failed."); return checked((long)m.freePhysical); }
    static string ID(Object o) => o==null?null:o.GetEntityId().ToString();
    static JArray V(Vector3 v) => new JArray(v.x,v.y,v.z);
    static JToken PublicValue(Component c,string name)
    {
        if (c==null) return null; var p=c.GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
        if (p==null || p.GetIndexParameters().Length!=0 || !p.CanRead) throw new Exception("Authentic public readonly observation contract absent: "+name);
        var value=p.GetValue(c);return value==null?null:JToken.FromObject(value);
    }
    static object ReadField(Component c,string name){var f=c.GetType().GetField(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);if(f==null)throw new Exception("Exact readonly source field absent: "+name);return f.GetValue(c);}
    static JObject ReadFields(Component c,string[] names){var r=new JObject();foreach(var name in names){var field=c.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance);if(field==null)throw new Exception("Original readonly field absent: "+name);var value=field.GetValue(c);r[name]=value==null?null:JToken.FromObject(value);}return r;}
    static Sentinel311SourceObserver() { EditorApplication.playModeStateChanged+=State;stage=SessionState.GetString(Key,"");if(stage!=""&&SessionState.GetInt(Key+".PID",0)==Process.GetCurrentProcess().Id) Rearm(); }
    public static string PrepareAndPlay()
    {
        var q=Sentinel311SourceBootstrap.Guard();
        if ((bool?)q["allowNormalSourceRoute"]!=true || (bool?)q["allowDiagnosticCaptures"]!=true || SessionState.GetBool(Key+".Consumed",false) || SessionState.GetString(Key,"")!="" || FreeBytes()<6442450944L) throw new Exception("Separate ordinary-source lease/fresh6GiB required.");
        string proof=(string)q["nativeBeforePlay"];
        if(string.IsNullOrEmpty(proof)||Sentinel311SourceBootstrap.Hash(proof)!=(string)q["nativeBeforePlaySHA"])throw new Exception("Current native source/provider preflight proof required.");
        var n=JObject.Parse(File.ReadAllText(proof));if((int?)n["pid"]!=Process.GetCurrentProcess().Id||(bool?)n["sourceGatePassed"]!=true)throw new Exception("Same-process source gate failed.");
        Sentinel311SourceBootstrap.Inventory();
        stage=Sentinel311SourceBootstrap.Root+"/Builds/URPMigration/R311-"+DateTime.UtcNow.ToString("MMddTHHmmssfff");Directory.CreateDirectory(stage);
        File.Copy(Sentinel311SourceBootstrap.Root+"/Assets/_LimappEditor/Sentinel311SourceObserver.cs",stage+"/executed-observer.cs");File.Copy(Sentinel311SourceBootstrap.RequestPath,stage+"/executed-request.json");
        using(var marker=new FileStream(Sentinel311SourceBootstrap.Root+"/Builds/URPMigration/consumed311-"+Process.GetCurrentProcess().Id+".json",FileMode.CreateNew,FileAccess.Write,FileShare.None)){var bytes=System.Text.Encoding.UTF8.GetBytes(new JObject{["pid"]=Process.GetCurrentProcess().Id,["stage"]=stage,["consumed"]=true}.ToString());marker.Write(bytes,0,bytes.Length);}
        SessionState.SetBool(Key+".Consumed",true);SessionState.SetInt(Key+".PID",Process.GetCurrentProcess().Id);SessionState.SetString(Key,stage);Install();Save();EditorApplication.isPlaying=true;return stage;
    }
    static void Rearm()
    {
        try{Install();}catch(Exception e){try{File.WriteAllText(stage+"/rearm-failure.json",new JObject{["pid"]=Process.GetCurrentProcess().Id,["failure"]=e.ToString(),["consumed"]=SessionState.GetBool(Key+".Consumed",false)}.ToString());}finally{SessionState.EraseString(Key);EditorApplication.delayCall+=()=>{if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;};}}
    }
    static void Install()
    {
        done=sdkComplete=sdkResult=progressPassed=timelineStopped=bound=false;firstWall=firstDSP=-1;lastFrame=-1;checkpoint=0;nextSample=0;
        samples=new JArray();events=new JArray();errors=new JArray();captures=new JArray();
        string prior=stage+"/report.json";if(File.Exists(prior)){var r=JObject.Parse(File.ReadAllText(prior));if((int?)r["pid"]!=Process.GetCurrentProcess().Id)throw new Exception("Reload report PID mismatch");samples=(JArray)r["samples"];events=(JArray)r["events"];errors=(JArray)r["errors"];captures=(JArray)r["captures"];done=(bool?)r["completed"]==true;if(done){Detach();SessionState.EraseString(Key);return;}if(samples.Count>0)throw new Exception("Unexpected mid-run reload; original partial report retained");}
        readinessStart=EditorApplication.timeSinceStartup;
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;
    }
    static bool TryBind()
    {
        var all=Sentinel311SourceBootstrap.Components();if(all.Count(c=>c.GetType().FullName=="SpawnerManager"&&c.GetType().Assembly.GetName().Name=="App000000000311")!=1)return false;
        var app=Sentinel311SourceBootstrap.SourceApp(all);var camera=Sentinel311SourceBootstrap.SDKOwnedCamera(all,app,out nativeCameraOwnership);if(camera==null)return false;
        Bind();bound=true;Save();return true;
    }
    static void Bind()
    {
        var all=Sentinel311SourceBootstrap.Components();var app=Sentinel311SourceBootstrap.SourceApp(all);var core=Sentinel311SourceBootstrap.CoreType(app);
        control=Sentinel311SourceBootstrap.ProjectComponent(all,"SpawnerManager");manager=app;director=Sentinel311SourceBootstrap.MainDirector(app);initialTimeline=director==null?0:director.time;
        sdkEvent=core.GetEvent("OnComplete",BindingFlags.Public|BindingFlags.Static);if(sdkEvent==null||sdkEvent.EventHandlerType!=typeof(Action<bool>))throw new Exception("Actual inherited public SDK callback required.");
        sdkHandler=Complete;sdkEvent.RemoveEventHandler(null,sdkHandler);sdkEvent.AddEventHandler(null,sdkHandler);if(director!=null){director.played-=Played;director.played+=Played;director.stopped-=Stopped;director.stopped+=Stopped;}
        events.Add(new JObject{["event"]="LoadedNativeObserverSelector",["actualSDKCameraOwnership"]=nativeCameraOwnership,["coreType"]=core.FullName,["coreMVID"]=core.Module.ModuleVersionId.ToString(),["coreModule"]=core.Assembly.Location,["appModule"]=app.GetType().Assembly.Location,["observerModule"]=typeof(Sentinel311SourceObserver).Assembly.Location,["observerMVID"]=typeof(Sentinel311SourceObserver).Module.ModuleVersionId.ToString()});
    }
    static void Played(PlayableDirector d){Event("ActualDirectorPlayed");}
    static void Stopped(PlayableDirector d){timelineStopped=true;Event("ActualDirectorStopped");}
    static void Complete(bool value){if(done)return;sdkComplete=true;sdkResult=value;completionTime=Time.time;Event("ActualSDKOnComplete");}
    static void Event(string name){events.Add(new JObject{["event"]=name,["time"]=Time.time,["dspTime"]=AudioSettings.dspTime,["timeline"]=director==null?(double?)null:director.time});Save();}
    static void State(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode){stage=SessionState.GetString(Key,"");if(stage!=""&&samples==null)Rearm();}if(s==PlayModeStateChange.ExitingPlayMode&&!done&&!string.IsNullOrEmpty(stage))Finish("PlayStopped");if(s==PlayModeStateChange.EnteredEditMode)Detach();}
    static void Log(string text,string stack,LogType type){if(done||string.IsNullOrEmpty(stage))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors.Add(new JObject{["type"]=type.ToString(),["message"]=text,["stack"]=stack,["time"]=Time.time});Save();}}
    static void Tick()
    {
        if(done||string.IsNullOrEmpty(stage)||!EditorApplication.isPlaying||Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
        try{
            long free=FreeBytes();if(free<3221225472L){Stop("PhysicalMemoryBelow3GiB");return;}if(errors.Count>0){Stop("NativeErrorGate");return;}
            if(!bound){if(!TryBind()){if(EditorApplication.timeSinceStartup-readinessStart>30)Stop("RuntimeBindingReadinessTimeout");return;}}
            if(firstWall<0){firstWall=EditorApplication.timeSinceStartup;firstDSP=AudioSettings.dspTime;}
            if(manager==null||control==null)throw new Exception("Authentic source manager/Timeline disappeared.");
            var all=Sentinel311SourceBootstrap.Components();float time=Time.time;
            if(time>=nextSample){nextSample=time+10;var rs=all.OfType<Renderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
                samples.Add(new JObject{["time"]=time,["frame"]=lastFrame,["wallElapsed"]=EditorApplication.timeSinceStartup-firstWall,["dspTime"]=AudioSettings.dspTime,["dspElapsed"]=AudioSettings.dspTime-firstDSP,["freeBytes"]=free,["workingSet"]=Process.GetCurrentProcess().WorkingSet64,
                    ["timelineTime"]=director==null?(double?)null:director.time,["timelineDuration"]=director==null?(double?)null:director.duration,["timelineState"]=director==null?null:director.state.ToString(),["AppActive"]=manager.gameObject.activeInHierarchy,["AppRaw"]=EditorJsonUtility.ToJson(manager),["directors"]=new JArray(all.OfType<PlayableDirector>().Select(d=>new JObject{["id"]=ID(d),["time"]=d.time,["duration"]=d.duration,["state"]=d.state.ToString(),["asset"]=Sentinel311SourceBootstrap.ID(d.playableAsset)})),
                    ["sourceControlReadOnlyFields"]=ReadFields(control,new[]{"wavesStarted","currentWaveIndex","waveTimer","intermissionTimer","inIntermission","allWavesComplete"}),["actualNormalInputOrPlayStartReadback"]=(bool)ReadField(control,"wavesStarted"),
                    ["movingObjects"]=new JArray(all.Where(c=>c.GetType().FullName=="MoveAvatar"&&c.gameObject.activeInHierarchy).Select(c=>new JObject{["id"]=ID(c),["position"]=V(c.transform.position),["euler"]=V(c.transform.eulerAngles),["raw"]=EditorJsonUtility.ToJson(c)})),
                    ["environment"]=new JObject{["sky"]=Sentinel311SourceBootstrap.ID(RenderSettings.skybox),["skyRaw"]=RenderSettings.skybox==null?null:EditorJsonUtility.ToJson(RenderSettings.skybox),["ambientMode"]=RenderSettings.ambientMode.ToString(),["ambientColor"]=new JArray(RenderSettings.ambientLight.r,RenderSettings.ambientLight.g,RenderSettings.ambientLight.b),["fog"]=RenderSettings.fog,["fogColor"]=new JArray(RenderSettings.fogColor.r,RenderSettings.fogColor.g,RenderSettings.fogColor.b),["fogStart"]=RenderSettings.fogStartDistance,["fogEnd"]=RenderSettings.fogEndDistance,["audioPaused"]=AudioListener.pause,["audioVolume"]=AudioListener.volume},
                    ["particles"]=new JArray(all.OfType<ParticleSystem>().Where(ps=>ps.gameObject.activeInHierarchy).Select(ps=>new JObject{["id"]=ID(ps),["hierarchy"]=Sentinel311SourceBootstrap.Hierarchy(ps.transform),["count"]=ps.particleCount,["time"]=ps.time,["playing"]=ps.isPlaying})),
                    ["eligibleShaderCounts"]=new JObject(rs.SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).GroupBy(m=>m.shader?.name??"null").Select(g=>new JProperty(g.Key,g.Count()))),
                    ["audio"]=new JArray(all.OfType<AudioSource>().Where(a=>a.isActiveAndEnabled).Select(a=>new JObject{["id"]=ID(a),["clip"]=a.clip?.name,["playing"]=a.isPlaying,["time"]=a.time,["samples"]=a.timeSamples})),
                    ["visibleUITexts"]=new JArray(all.Where(c=>c.gameObject.activeInHierarchy&&c.GetType().FullName=="TMPro.TextMeshProUGUI").Select(c=>new JObject{["id"]=ID(c),["hierarchy"]=Sentinel311SourceBootstrap.Hierarchy(c.transform),["text"]=PublicValue(c,"text")})),
                    ["sourceRuntimeComponents"]=new JArray(all.Where(c=>c.gameObject.activeInHierarchy&&c.GetType().Assembly.GetName().Name=="App000000000311"&&new[]{"Cannon","SpawnerManager","GameManager","EnemyHealth","EnemyShip","PowerUp","Projectile"}.Contains(c.GetType().FullName)).Select(c=>new JObject{["id"]=ID(c),["type"]=c.GetType().FullName,["raw"]=EditorJsonUtility.ToJson(c)}))});Save();}
            if(!progressPassed&&time>=6){progressPassed=true;Event("SixSecondSameRunSDKDSPGate");if(AudioSettings.dspTime-firstDSP<.5){Stop("OrdinaryDSPProgressFailure");return;}var gate=JObject.Parse(File.ReadAllText(Sentinel311SourceBootstrap.RequestPath));if((bool?)gate["allowFullSourceContinuation"]!=true){Stop("QualifiedSixSecondHostOnlyInputPending");return;}if(!((bool)ReadField(control,"wavesStarted"))){Stop("NormalInputOrPlayingStartNotObserved");return;}}
            if(checkpoint<checkpoints.Length&&time>=checkpoints[checkpoint]){Capture(checkpoint.ToString("D2")+"-Game"+checkpoints[checkpoint]);checkpoint++;}
            if(sdkComplete&&time-completionTime>=1)Stop("ActualSDKCompletion");else if(progressPassed){var q=JObject.Parse(File.ReadAllText(Sentinel311SourceBootstrap.RequestPath));double ceiling=(double?)q["nativeApprovedObservationSeconds"]??0;if(ceiling<=6||(bool?)q["allowNativeDerivedObservationCeiling"]!=true)throw new Exception("Actual native host/source route observation ceiling lease required");if(time>ceiling||EditorApplication.timeSinceStartup-firstWall>ceiling+150)Stop("QualifiedObservationCeilingNotSDKEnd");}
        }catch(Exception e){errors.Add(new JObject{["type"]="ObserverFailure",["message"]=e.ToString()});Stop("ObserverFailure");}
    }
    static void Capture(string label)
    {
        if(FreeBytes()<3221225472L)throw new Exception("Per-capture memory floor.");var all=Sentinel311SourceBootstrap.Components();var sourceApp=Sentinel311SourceBootstrap.SourceApp(all);JObject ownership;var c=Sentinel311SourceBootstrap.SDKOwnedCamera(all,sourceApp,out ownership);
        if(c==null)throw new Exception("Missing exact active App/SDK manager/registered CenterEye association; no capture/activation repair");
        var old=c.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(640,480,24,RenderTextureFormat.ARGB32);var image=new Texture2D(640,480,TextureFormat.RGBA32,false);
        try{rt.Create();c.targetTexture=rt;c.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,640,480),0,0);image.Apply();string path=stage+"/"+label+".png";File.WriteAllBytes(path,image.EncodeToPNG());
            captures.Add(new JObject{["label"]=label,["path"]=path,["time"]=Time.time,["timelineTime"]=director==null?(double?)null:director.time,["dspTime"]=AudioSettings.dspTime,["actualSDKCameraOwnership"]=ownership,["camera"]=ID(c),["hierarchy"]=Sentinel311SourceBootstrap.Hierarchy(c.transform),["position"]=V(c.transform.position),["euler"]=V(c.transform.eulerAngles),["FOV"]=c.fieldOfView,["near"]=c.nearClipPlane,["far"]=c.farClipPlane,["async"]=ShaderUtil.allowAsyncCompilation,
                ["submission"]="Added diagnostic native Camera.Render from exact registered SDK CenterEye; not automatic/hardware or exact stochastic matched frame",
                ["renderers"]=new JArray(Sentinel311SourceBootstrap.Components().OfType<Renderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy).Select(r=>new JObject{["id"]=ID(r),["hierarchy"]=Sentinel311SourceBootstrap.Hierarchy(r.transform),["boundsCenter"]=V(r.bounds.center),["boundsSize"]=V(r.bounds.size),["isVisibleSampleOnly"]=r.isVisible,["hasMPB"]=r.HasPropertyBlock(),["materials"]=new JArray(r.sharedMaterials.Select(m=>m==null?null:new JObject{["id"]=ID(m),["shader"]=m.shader?.name,["raw"]=EditorJsonUtility.ToJson(m)}))}))});
        }finally{c.targetTexture=old;RenderTexture.active=active;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);}Save();
    }
    static void Detach(){EditorApplication.update-=Tick;if(director!=null){director.played-=Played;director.stopped-=Stopped;}if(sdkEvent!=null&&sdkHandler!=null)sdkEvent.RemoveEventHandler(null,sdkHandler);Application.logMessageReceived-=Log;}
    static void Stop(string reason){Finish(reason);EditorApplication.isPlaying=false;}
    static void Finish(string reason){if(done)return;done=true;events.Add(new JObject{["event"]=reason,["time"]=Time.time});try{Detach();}catch(Exception e){errors.Add(new JObject{["type"]="DetachFailure",["message"]=e.ToString()});}finally{SessionState.EraseString(Key);Save();}}
    static void Save(){if(string.IsNullOrEmpty(stage))return;File.WriteAllText(stage+"/report.json",new JObject{["pid"]=Process.GetCurrentProcess().Id,["completed"]=done,["sdkComplete"]=sdkComplete,["sdkResult"]=sdkResult,["progressGatePassed"]=progressPassed,["timelineStoppedEvent"]=timelineStopped,["samples"]=samples,["events"]=events,["errors"]=errors,["captures"]=captures,
        ["interactionClaim"]="Ordinary initial cannon grab removes tutorial and calls BeginSpawning. Actual SDK two controller VRButton.Three or authenticated hand tracking/pinch path; trigger down/held/release fires original cannon, impulse after FixedUpdate. Passive SDK/DSP startup is not waves or grab coverage. Three source60s waves:6..66 single Meteor,78..138 scatter Meteor/BigMeteor,154..214 fullauto Meteor/Drone; shield reset can alter clocks. Pools/re-enable portals/player+enemy travel/impact/shield/material swaps/UFO/powerups/fireworks remain measured native variants. No private HandleGrab/BeginSpawning/Fire/health/wave/end writes; no idle180 acceptance. Source legacy FollowClick/Launcher input activity remains a native eligibility gate.",
        ["limits"]="Source estimated249.1167 from ordinary grab only: last wave +7s +12s dialogue activates ending Timeline; fade13.9833(2s), End16.1167. Actual native wave/graph/host/SDK callback required, not forced estimated timeout. Captures are added SDK registered CenterEye diagnostic Camera.Render; no automatic/hardware/input claim. Native lease/source callback/default domain reload/current provider acceptance remain pending."}.ToString());}
}
