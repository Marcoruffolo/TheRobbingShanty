using UnityEngine;
using UnityEngine.VFX;

public class SwingEffectScript : MonoBehaviour
{
    [SerializeField] VisualEffect swingVFX;
    public void SwingVFX()
    {
        swingVFX.Play();
    }
}
