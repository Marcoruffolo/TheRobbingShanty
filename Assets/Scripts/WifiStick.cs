using UnityEngine;

public class WifiStick : MonoBehaviour
{
    public bool TurnedOn;

    public Material _lightMat;

    [SerializeField] private Color offColor = new(0.025f, 0.03f, 0.04f, 1f);
    [SerializeField] private Color onColor = new(0.05f, 0.9f, 1f, 1f);
    [SerializeField] private float offEmission = 0f;
    [SerializeField] private float onEmission = 12.84f;
    [SerializeField] private bool controlChildLights = true;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int AccentColorId = Shader.PropertyToID("_Color_1");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionStrengthId = Shader.PropertyToID("_Emmision");

    private Renderer[] _renderers;
    private Light[] _lights;
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        CacheComponents();
        ApplyState();
    }

    private void OnEnable()
    {
        ApplyState();
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;

        CacheComponents();
        ApplyState();
    }

    public void SetTurnedOn(bool turnedOn)
    {
        TurnedOn = turnedOn;
        ApplyState();
    }

    private void CacheComponents()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _lights = GetComponentsInChildren<Light>(true);
        _propertyBlock ??= new MaterialPropertyBlock();
    }

    private void ApplyState()
    {
        if (_renderers == null || _propertyBlock == null)
            CacheComponents();

        Color color = TurnedOn ? onColor : offColor;
        float emission = TurnedOn ? onEmission : offEmission;
        Color emissionColor = color * emission;

        foreach (Renderer stickRenderer in _renderers)
        {
            if (stickRenderer == null) continue;

            Material[] materials = stickRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (!ShouldControlMaterial(materials[i])) continue;

                stickRenderer.GetPropertyBlock(_propertyBlock, i);
                _propertyBlock.SetColor(ColorId, color);
                _propertyBlock.SetColor(BaseColorId, color);
                _propertyBlock.SetColor(AccentColorId, emissionColor);
                _propertyBlock.SetColor(EmissionColorId, emissionColor);
                _propertyBlock.SetFloat(EmissionStrengthId, emission);
                stickRenderer.SetPropertyBlock(_propertyBlock, i);
            }
        }

        if (!controlChildLights) return;

        foreach (Light stickLight in _lights)
        {
            if (stickLight == null) continue;

            stickLight.enabled = TurnedOn;
            stickLight.color = color;
            stickLight.intensity = emission;
        }
    }

    private bool ShouldControlMaterial(Material material)
    {
        if (material == null) return false;
        if (_lightMat == null) return true;

        return material == _lightMat || material.name.StartsWith(_lightMat.name, System.StringComparison.Ordinal);
    }
}