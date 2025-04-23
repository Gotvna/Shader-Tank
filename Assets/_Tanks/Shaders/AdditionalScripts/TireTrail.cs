using System.Collections.Generic;
using Tanks.Complete;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TireTrail : MonoBehaviour
{
    [Header("Tire Marks")]
    public float markWidth = 1.0f;
    public float markLength = 0.5f;
    public float fadeDuration = 2.0f;
    public Material markMaterial;
    public LayerMask groundLayer;

    [Header("Emission")]
    public float minToEmit = 0.1f;
    public float emissionInterval = 0.1f;

    public Transform[] wheels; // Assign in Inspector
    private List<Decal> activeDecals = new List<Decal>();
    private float lastEmitTime;

    private Mesh quadMesh;

    void Start() { quadMesh = CreateQuadMesh(); }
    void Update()
    {
        if (ShouldEmit() && Time.time - lastEmitTime > emissionInterval)
        {
            EmitMarks();
            lastEmitTime = Time.time;
        }

        UpdateDecals();
    }

    bool ShouldEmit()
    {
        float moving = GetComponent<TankMovement>().MovementInputValue;
        float turning = GetComponent<TankMovement>().TurnInputValue;
        return Mathf.Abs(moving) > minToEmit || Mathf.Abs(turning) > minToEmit;
    }

    void EmitMarks()
    {
        foreach (Transform wheel in wheels)
        {
            if (Physics.Raycast(wheel.position, Vector3.down, out RaycastHit hit, 10.0f, groundLayer))
            {
                GameObject decal = new GameObject("TireMark");
                decal.transform.position = hit.point + hit.normal * 0.01f;
                decal.transform.rotation = Quaternion.LookRotation(Vector3.Cross(wheel.right, hit.normal), hit.normal);
                decal.transform.localScale = new Vector3(markWidth, 1f, markLength);

                MeshRenderer renderer = decal.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                MeshFilter filter = decal.AddComponent<MeshFilter>();
                filter.mesh = quadMesh;

                renderer.material = new Material(markMaterial);
                activeDecals.Add(new Decal(decal, fadeDuration));
            }
        }
    }

    void UpdateDecals()
    {
        for (int i = activeDecals.Count - 1; i >= 0; i--)
        {
            if (activeDecals[i].UpdateFade(Time.deltaTime))
            {
                Destroy(activeDecals[i].obj);
                activeDecals.RemoveAt(i);
            }
        }
    }

    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, 0, -0.5f);
        vertices[1] = new Vector3(-0.5f, 0, 0.5f);
        vertices[2] = new Vector3(0.5f, 0, 0.5f);
        vertices[3] = new Vector3(0.5f, 0, -0.5f);
        mesh.vertices = vertices;

        int[] triangles = { 0, 1, 2, 0, 2, 3 };
        mesh.triangles = triangles;

        Vector2[] uv = { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };
        mesh.uv = uv;

        return mesh;
    }
}

public class Decal
{
    public GameObject obj;
    private float duration;
    private float timer;
    private Material material;

    public Decal(GameObject obj, float duration)
    {
        this.obj = obj;
        this.duration = duration;
        this.material = obj.GetComponent<Renderer>().material;
    }

    public bool UpdateFade(float deltaTime)
    {
        timer += deltaTime;
        float fade = Mathf.Clamp01(1 - (timer / duration));
        material.SetFloat("_Fade", fade);
        return timer >= duration;
    }
}