using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 1.5f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 1→0 curve

    private List<Material> tankMaterials = new List<Material>();
    private const string DISSOLVE_PROPERTY = "_Amount";
    private Coroutine dissolveRoutine;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        if (isInitialized) return;

        // Start fully visible in menus
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty(DISSOLVE_PROPERTY))
                {
                    tankMaterials.Add(mat);
                    mat.SetFloat(DISSOLVE_PROPERTY, 0f); // Start fully visible
                }
            }
        }
        isInitialized = true;
    }

    public void StartDissolve()
    {

        foreach (Material mat in tankMaterials)
        {
            mat.SetFloat(DISSOLVE_PROPERTY, 1f);
        }

        dissolveRoutine = StartCoroutine(RunDissolve());
    }

    private IEnumerator RunDissolve()
    {
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / dissolveDuration);
            float dissolveValue = dissolveCurve.Evaluate(progress);

            foreach (Material mat in tankMaterials)
            {
                mat.SetFloat(DISSOLVE_PROPERTY, dissolveValue);
            }

            yield return null;
        }

        // Finalize to fully visible
        foreach (Material mat in tankMaterials)
        {
            mat.SetFloat(DISSOLVE_PROPERTY, 0f);

        }
    }

}