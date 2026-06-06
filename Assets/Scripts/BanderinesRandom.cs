using UnityEngine;

public class BanderinesRandom : MonoBehaviour
{
    [SerializeField] private GameObject[] banderines;
    [SerializeField] private Material material1;
    [SerializeField] private Material material2;

    private void Start()
    {
        foreach (GameObject banderine in banderines)
        {
            banderine.SetActive(Random.value > 0.8f);
        }
        Color colorrand = Random.ColorHSV();
        material1.color = colorrand;
        material2.color = colorrand;
    }
}
