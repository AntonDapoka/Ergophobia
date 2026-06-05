using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private HealthComponent healthComponent;
    private Renderer[] renderers;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnEnable()
    {
        healthComponent.OnHit += HandleHitFlash;
    }

    private void OnDisable()
    {
        healthComponent.OnHit -= HandleHitFlash;
        ResetColor();
    }

    private void HandleHitFlash()
    {
        if (renderers == null || renderers.Length == 0) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", flashColor);
            propBlock.SetColor("_Color", flashColor);
            r.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        ResetColor();
    }

    private void ResetColor()
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            r.SetPropertyBlock(null);
        }
    }
}