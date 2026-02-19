using UnityEngine;
using System.Collections.Generic;

public class BuildingFade : MonoBehaviour
{
    public float fadeAlpha = 0.3f;
    public float fadeSpeed = 5f;

    private Dictionary<GameObject, List<Material>> buildingMaterials = new Dictionary<GameObject, List<Material>>();
    private Dictionary<GameObject, float> buildingAlpha = new Dictionary<GameObject, float>();
    private HashSet<GameObject> currentlyBlocking = new HashSet<GameObject>();
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // cache all building renderers at start
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        foreach (GameObject building in buildings)
        {
            List<Material> mats = new List<Material>();
            foreach (Renderer rend in building.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in rend.materials)
                {
                    mats.Add(mat);
                }
            }
            buildingMaterials[building] = mats;
            buildingAlpha[building] = 1f;
        }
    }

    void Update()
    {
        currentlyBlocking.Clear();

        // cast rays across the screen
        for (float x = 0.1f; x <= 0.9f; x += 0.2f)
        {
            for (float y = 0.1f; y <= 0.9f; y += 0.2f)
            {
                Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width * x, Screen.height * y));
                RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

                foreach (RaycastHit hit in hits)
                {
                    // find the root building object
                    GameObject obj = hit.collider.gameObject;
                    if (obj.CompareTag("Building"))
                    {
                        currentlyBlocking.Add(obj);
                    }
                    else if (hit.collider.transform.parent != null &&
                             hit.collider.transform.parent.CompareTag("Building"))
                    {
                        currentlyBlocking.Add(hit.collider.transform.parent.gameObject);
                    }
                }
            }
        }

        // update all building alphas
        foreach (var pair in buildingMaterials)
        {
            GameObject building = pair.Key;
            List<Material> mats = pair.Value;

            float targetAlpha = currentlyBlocking.Contains(building) ? fadeAlpha : 1f;
            float currentAlpha = buildingAlpha[building];
            float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            buildingAlpha[building] = newAlpha;

            foreach (Material mat in mats)
            {
                if (newAlpha < 0.99f)
                {
                    // switch to transparent
                    mat.SetFloat("_Surface", 1);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = 3000;
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }
                else
                {
                    // restore opaque
                    mat.SetFloat("_Surface", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = 2000;
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }

                Color c = mat.color;
                c.a = newAlpha;
                mat.color = c;
            }
        }
    }
}