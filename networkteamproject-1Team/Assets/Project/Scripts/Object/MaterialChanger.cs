using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public MeshRenderer bulbRenderer;
    public MeshRenderer pillarRenderer;
    public Material completedMaterials;

    public void ChangeMaterial()
    {
        if (bulbRenderer == null || pillarRenderer == null || completedMaterials == null) return;
        
        bulbRenderer.material = completedMaterials;
        pillarRenderer.material = completedMaterials;
    }
}
