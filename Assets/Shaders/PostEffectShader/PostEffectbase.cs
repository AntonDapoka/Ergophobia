
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffectbase : MonoBehaviour
{
    public Shader shader;
    private Material _material;

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        UpdateProperty();

        if(material != null) Graphics.Blit(source, destination, material);
        else Graphics.Blit(source, destination);

    }
    protected virtual void UpdateProperty() {}

    protected Material material
    {
        get
        {
            if (shader == null) return null;
            else
            {
                if(_material != null && _material.shader == shader) return _material;

                _material = new Material(shader){hideFlags = HideFlags.DontSave};
                return _material;
            }
        }
    }
}
