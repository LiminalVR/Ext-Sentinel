using UnityEngine;using UnityEngine.Rendering;using UnityEngine.Rendering.Universal;using UnityEngine.Rendering.RenderGraphModule;
namespace Sentinel311.Rendering {
 public sealed class Source311UniformFeature:ScriptableRendererFeature {
  UniformPass pass;
  public override void Create(){pass=new UniformPass{renderPassEvent=RenderPassEvent.BeforeRenderingOpaques};}
  public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data){renderer.EnqueuePass(pass);}
  sealed class UniformPass:ScriptableRenderPass {
   sealed class Data {public Vector4 position,color,spec,ambient;public Vector4[] sh;}
   public override void RecordRenderGraph(RenderGraph graph,ContextContainer context){
    var probe=RenderSettings.ambientProbe;var sh=new Vector4[7];for(int c=0;c<3;c++){sh[c]=new Vector4(probe[c,3],probe[c,1],probe[c,2],probe[c,0]-probe[c,6]);sh[c+3]=new Vector4(probe[c,4],probe[c,5],3*probe[c,6],probe[c,7]);}sh[6]=new Vector4(probe[0,8],probe[1,8],probe[2,8],1);
    var position=Vector4.zero;var color=Vector4.zero;var lights=context.Get<UniversalLightData>();if(lights.mainLightIndex>=0){var l=lights.visibleLights[lights.mainLightIndex].light;if(l!=null&&l.type==LightType.Directional&&l.bakingOutput.lightmapBakeType!=LightmapBakeType.Baked){var dir=-l.transform.forward;position=new Vector4(dir.x,dir.y,dir.z,0);var rgb=l.color*l.intensity;if(QualitySettings.activeColorSpace==ColorSpace.Linear)rgb=rgb.linear;color=new Vector4(rgb.r,rgb.g,rgb.b,l.intensity);}}
    var ambient=QualitySettings.activeColorSpace==ColorSpace.Linear?RenderSettings.ambientLight.linear:RenderSettings.ambientLight;
    using(var builder=graph.AddRasterRenderPass<Data>("311 original ambient and reflection inputs",out var data)){data.sh=sh;data.position=position;data.color=color;data.spec=new Vector4(probe[0,0]*RenderSettings.reflectionIntensity,probe[1,0]*RenderSettings.reflectionIntensity,probe[2,0]*RenderSettings.reflectionIntensity,1);data.ambient=new Vector4(ambient.r*.5f,ambient.g*.5f,ambient.b*.5f,.5f);builder.AllowGlobalStateModification(true);builder.AllowPassCulling(false);builder.SetRenderFunc((Data x,RasterGraphContext ctx)=>{ctx.cmd.SetGlobalVector("_Source311BasePosition",x.position);ctx.cmd.SetGlobalVector("_Source311BaseColor",x.color);ctx.cmd.SetGlobalVector("_Source311IndirectSpecColor",x.spec);ctx.cmd.SetGlobalVector("_Source311Ambient",x.ambient);string[] names={"SHAr","SHAg","SHAb","SHBr","SHBg","SHBb","SHC"};for(int i=0;i<7;i++)ctx.cmd.SetGlobalVector("_Source311"+names[i],x.sh[i]);});}
   }
  }
 }
}
