using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class TelescopeRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material telescopeMaterial;
    private TelescopePass _pass;

    public static TelescopeRendererFeature Instance { get; private set; }
    public bool IsActive { get; set; } = false;

    class TelescopePass : ScriptableRenderPass
    {
        private Material _mat;
        private TelescopeRendererFeature _feature;

        public TelescopePass(Material mat, TelescopeRendererFeature feature)
        {
            _mat = mat;
            _feature = feature;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        private class PassData
        {
            public TextureHandle src;
            public Material mat;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!_feature.IsActive || _mat == null) return;
            Debug.Log($"RecordRenderGraph - IsActive:{_feature.IsActive} | mat:{_mat != null}");
            var resourceData = frameData.Get<UniversalResourceData>();

            var desc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            desc.name = "TelescopeTempTex";
            desc.clearBuffer = false;
            TextureHandle tempTex = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("TelescopeEffect", out var passData))
            {
                passData.src = resourceData.activeColorTexture;
                passData.mat = _mat;

                builder.UseTexture(passData.src);
                builder.SetRenderAttachment(tempTex, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), data.mat, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("TelescopeCopy", out var passData))
            {
                passData.src = tempTex;

                builder.UseTexture(passData.src);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.src, new Vector4(1, 1, 0, 0), null, 0);
                });
            }
        }
    }

    public override void Create()
    {
        Instance = this;
        _pass = new TelescopePass(telescopeMaterial, this);
        Debug.Log("TelescopeRendererFeature creado, Instance asignada");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (IsActive)
            renderer.EnqueuePass(_pass);
    }
}