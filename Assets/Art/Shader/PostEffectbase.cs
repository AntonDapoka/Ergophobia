
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffectbase : MonoBehaviour
{
    //屏幕后处理效果会使用的shader
    public Shader shader;
    //一个用于动态创建出来的材质球 就不用再工程中手动创建了
    private Material _material;

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //再进行渲染之前 先更新属性 在子类中重写即可
        UpdateProperty();

        //判断这个材质球是否存在 如果不为空 就证明这个shader是支持的 可以正常使用
        if(material != null)
            Graphics.Blit(source, destination, material);
        else//如果材质球不存在 就直接把源纹理复制到目标纹理上 不进行任何处理
            Graphics.Blit(source, destination);

    }

    /// <summary>
    /// 更新材质球属性 由于每个后处理效果需要更新的属性不一样 因此我们在基类中定义一个虚函数 让子类去重写实现
    /// </summary>
    protected virtual void UpdateProperty()
    {
    }


    protected Material material
    {
        get
        {
            //如果shader没有 或 但是不支持当前平台
            if (shader == null)
                return null;
            else
            {
                //避免每次调用该属性都创建一个新的材质球
                //如果之前new过了 并且shader没有被修改过 那么就直接返回之前创建的材质球
                if(_material != null && _material.shader == shader)
                    return _material;

                //用支持的Shader创建材质球
                _material = new Material(shader); 
                //不希望材质球被保存下来 因此我们有一个标识
                _material.hideFlags = HideFlags.DontSave; 
                return _material;
            }
        }
    }

}
