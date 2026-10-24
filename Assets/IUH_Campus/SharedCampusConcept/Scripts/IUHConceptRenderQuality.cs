using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IUHCampus.World
{
    // The desktop art preview owns its override and restores the previous project pipeline on exit.
    public class IUHConceptRenderQuality : MonoBehaviour
    {
        public RenderPipelineAsset desktopPipeline;
        RenderPipelineAsset previous;
        bool applied;
        readonly Dictionary<UniversalAdditionalCameraData,AntialiasingMode> previousAA=new Dictionary<UniversalAdditionalCameraData,AntialiasingMode>();
        void OnEnable()
        {
            if(!Application.isPlaying||Application.isMobilePlatform||desktopPipeline==null)return;
            previous=QualitySettings.renderPipeline;QualitySettings.renderPipeline=desktopPipeline;applied=true;
            foreach(var camera in FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data==null)continue;
                previousAA[data]=data.antialiasing;data.antialiasing=AntialiasingMode.None;
            }
        }
        void OnDisable()
        {
            if(applied){QualitySettings.renderPipeline=previous;applied=false;}
            foreach(var item in previousAA)if(item.Key!=null)item.Key.antialiasing=item.Value;
            previousAA.Clear();
        }
    }
}
