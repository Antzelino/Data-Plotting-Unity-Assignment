using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Plotter : MonoBehaviour
{
    public static GameObject root; // All spheres are under this root gameobject. Also the lines that divide the clusters.
    public static Dictionary<string, string> countryCodeToCountry =
        new Dictionary<string, string>()
        {
            ["th"] = "Thailand",
            ["us"] = "United States",
            ["au"] = "Australia",
            ["gb"] = "United Kingdom",
            ["ph"] = "Philippines",
            ["id"] = "Indonesia",
            ["fr"] = "France",
            ["br"] = "Brazil"
        };

    public static Material[] materialPerCluster;
    public int[] sizePerCluster; // Number of Datapoints in each cluster
    int numClusters;

    List<GameObject> lines = null;

    void Start()
    {
        Initializations();
        DrawLines();
        PlaceDatapointSpheres();
    }

    void Initializations()
    {
        root = new GameObject("root");
        root.transform.position = new Vector3(0f, 0f, 0f);
        numClusters = ReadCSV.countryNames.Length;
        materialPerCluster = new Material[numClusters];
        sizePerCluster = new int[numClusters];
        lines = new List<GameObject>();
    }

    void DrawLines()
    {
        for (int i = 0; i < numClusters; i++) // Draw lines that split the clusters
        {
            GameObject line_obj = new GameObject("Cluster Division Line");
            lines.Add(line_obj);
            line_obj.transform.parent = root.transform;
            LineRenderer lr = line_obj.AddComponent<LineRenderer>();

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.numCapVertices = 14;
            lr.widthMultiplier = 0.1f;
            lr.positionCount = 2;
            float theta = i * 2 * Mathf.PI / (float)numClusters;
            Vector3 start = PolarToCartesian(new Vector3(1f, theta, 0f));
            Vector3 end = PolarToCartesian(new Vector3(30f, theta, 0f));
            lr.gameObject.transform.parent = root.transform;
            lr.SetPositions(new Vector3[] { start, end });
        }
    }

    void PlaceDatapointSpheres()
    {
        float[] cluster_center_angles = new float[numClusters];
        for (int i = 0; i < numClusters; i++)
        {
            cluster_center_angles[i] = (i - 0.5f) * 2 * Mathf.PI / (float)numClusters; // The angle between the edges of each cluster
            var mat = new Material(Shader.Find("Standard"));

            // Source: https://forum.unity.com/threads/change-standard-shader-render-mode-in-runtime.318815/
            // Change into Transparent mode in runtime. Now setting Color.a actually changes Alpha value
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            var c = Color.HSVToRGB(i / (float)numClusters, 1f, 1f);
            mat.color = c;
            mat.renderQueue = 3000;
            mat.enableInstancing = false;

            materialPerCluster[i] = mat;
            sizePerCluster[i] = ReadCSV.IDsByCountry[ReadCSV.countryNames[i]].Count;
        }

        foreach (var datapoint_gameobject in ReadCSV.allDatapoints)
        {
            var datapoint = datapoint_gameobject.GetComponent<DataPoint>();
            int cluster_index = Array.IndexOf(ReadCSV.countryNames, datapoint.GetComponent<DataPoint>().country);

            datapoint_gameobject.transform.parent = root.transform;
            datapoint_gameobject.transform.name = "Datapoint Sphere";
            datapoint_gameobject.GetComponent<MeshRenderer>().sharedMaterial = materialPerCluster[cluster_index];

            float r = 1f + 29f * (ReadCSV.revenueMax - datapoint.revenue) / ReadCSV.revenueMax;
            float min_angle = cluster_index * 2 * Mathf.PI / (float)numClusters;
            float max_angle = (cluster_index + 1) * 2 * Mathf.PI / (float)numClusters;

            var idIndex = ReadCSV.IDsByCountry[ReadCSV.countryNames[cluster_index]].IndexOf(datapoint.wappierID);
            float n = (float)idIndex / (float)sizePerCluster[cluster_index];
            float theta = min_angle + n * (max_angle - min_angle);

            float height = datapoint.day + 0.5f; // +0.5f because the sphere has radius=0.5f so now the bottom of the sphere touches the ground

            datapoint_gameobject.transform.localPosition = PolarToCartesian(new Vector3(r, theta, height));
            datapoint_gameobject.transform.localScale *= 0.5f;

            datapoint.GetComponent<MeshRenderer>().material = materialPerCluster[cluster_index];
            datapoint_gameobject.GetComponent<MeshRenderer>().material = materialPerCluster[cluster_index];

            datapoint_gameobject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            datapoint_gameobject.AddComponent<cakeslice.Outline>().enabled = false; // disable outline at first. set to true to enable it
        }
    }

    /*
     * https://en.wikipedia.org/wiki/Polar_coordinate_system
     * 
     * Polar coordinates: float r, float theta, float h
     * r:       radius, distance from center
     * theta:   angle
     * h:       height
     */
    Vector3 PolarToCartesian(Vector3 polar)
    {
        float r = polar[0];
        float theta = polar[1];
        float h = polar[2];

        float x = r * Mathf.Cos(theta);
        float y = h;
        float z = r * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }

    /*
     * Cartesian coordinates: float x, float y, float z
     * x: x-axis distance
     * y: y-axis distance (or height)
     * z: z-axis distance
     */
    Vector3 CartesianToPolar(Vector3 cartesian)
    {
        float x = cartesian.x;
        float y = cartesian.y;
        float z = cartesian.z;

        float r = Mathf.Sqrt(x * x + z * z);
        float theta = Mathf.Atan2(z, x);
        float h = y;

        return new Vector3(r, theta, h);
    }
}
