using UnityEngine;
using System.Collections.Generic;

public class TankDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public Shader dissolveShader;
    public float dissolveDuration = 2f;
    public Color edgeColor = new Color(0, 0.5f, 1, 1);
    public float edgeWidth = 0.1f;

    private List<Material> m_OriginalMaterials = new List<Material>();
    private List<Material> m_DissolveMaterials = new List<Material>();
    private bool m_IsDissolving = false;
    private float m_DissolveProgress = 0f;

    public void Initialize()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            // Store original materials
            foreach (Material mat in renderer.sharedMaterials)
            {
                m_OriginalMaterials.Add(new Material(mat));
            }

            // Create dissolve materials
            foreach (Material originalMat in renderer.sharedMaterials)
            {
                Material dissolveMat = new Material(dissolveShader);
                dissolveMat.CopyPropertiesFromMaterial(originalMat);

                // Set dissolve properties
                dissolveMat.SetColor("_DissolveEdgeColor", edgeColor);
                dissolveMat.SetFloat("_DissolveEdgeWidth", edgeWidth);
                dissolveMat.SetFloat("_DissolveAmount", 1f); // Start fully dissolved

                m_DissolveMaterials.Add(dissolveMat);
            }
        }
    }

    public void StartDissolve()
    {
        if (m_DissolveMaterials.Count == 0) InitializeMaterials();

        m_IsDissolving = true;
        m_DissolveProgress = 1f;
        ApplyDissolveMaterials();
    }

    private void Update()
    {
        if (!m_IsDissolving) return;

        m_DissolveProgress -= Time.deltaTime / dissolveDuration;
        m_DissolveProgress = Mathf.Clamp01(m_DissolveProgress);

        foreach (Material mat in m_DissolveMaterials)
        {
            mat.SetFloat("_DissolveAmount", m_DissolveProgress);
        }

        if (m_DissolveProgress <= 0f)
        {
            EndDissolve();
        }
    }

    private void ApplyDissolveMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        int matIndex = 0;

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = m_DissolveMaterials[matIndex++];
            }
            renderer.materials = mats;
        }
    }

    private void EndDissolve()
    {
        m_IsDissolving = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        int matIndex = 0;

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = m_OriginalMaterials[matIndex++];
            }
            renderer.materials = mats;
        }
    }
}