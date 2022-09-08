using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class PixelImageConverter : EditorWindow
{
    public Texture2D image;
    public Transform spawnable;
    public Transform spawnPoint;
    public Material baseMat;
    public float transparency = 1f;
    public float scaleFactor = 1f;

    private List<Material> matDatabase = new List<Material>();
    string folderName = "PixelImageConverter";
    [MenuItem("Window/PixelImageConverter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PixelImageConverter));
    }
    private void OnGUI()
    {
        GUILayout.Label("Hello!", EditorStyles.boldLabel);
        GUILayout.Label("This is Pixel to Mesh Converter", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("This tool enables user to create 2d mesh array from pixelated image and fills them with related colors. Moreover, you can determine which point the cubes will be spawned.", EditorStyles.helpBox);
        GUILayout.Label("Spawn Point", EditorStyles.boldLabel);
        spawnPoint = (Transform)EditorGUILayout.ObjectField(spawnPoint, typeof(Transform), true);
        GUILayout.Label("Image Reference to be converted", EditorStyles.boldLabel);
        image = (Texture2D)EditorGUILayout.ObjectField(image, typeof(Texture2D), true);
        GUILayout.Label("Objects that will be spawned", EditorStyles.boldLabel);
        spawnable = (Transform)EditorGUILayout.ObjectField(spawnable, typeof(Transform), true);
        GUILayout.Label("Base material", EditorStyles.boldLabel);
        baseMat = (Material)EditorGUILayout.ObjectField(baseMat, typeof(Material), true); 
        GUILayout.Label("Transparency of spawned objects", EditorStyles.boldLabel);
        GUILayout.Label("0 - 1 values for the transparency of the objects", EditorStyles.helpBox);
        transparency = (float)EditorGUILayout.FloatField(transparency);
        GUILayout.Label("Scaling Factor for the objects", EditorStyles.boldLabel);
        scaleFactor = (float)EditorGUILayout.FloatField(scaleFactor);
        GUILayout.Space(5);
        var Button = GUILayout.Button("Generate");
        var Button2 = GUILayout.Button("Clear");

        if (Button)
        {
            CreateParentFolder();
            Generate();
        }
        else if (Button2)
        {
            Clear();
        }
    }
    private void CreateParentFolder()
    {
        //Debug.Log($"{Application.dataPath}/Art/" + folderName);

        if (System.IO.Directory.Exists($"{Application.dataPath}/Art/" + folderName)){

        }
        else
        {
            AssetDatabase.CreateFolder("Assets/Art", folderName);
        }
    }
    private void Generate()
    {
        for(int i = 0; i < image.width; i++)
        {
            for(int j = 0; j < image.height; j++)
            {
                if(image.GetPixel(i,j).a < 1)
                {
                    continue;
                }

                Vector3 targetLocalPosition = Vector3.zero;

                targetLocalPosition.x = (i - (image.width / 2)) * scaleFactor;
                targetLocalPosition.y = j * scaleFactor;

                Transform spawned = Instantiate(spawnable, spawnPoint);
                spawned.transform.localPosition = targetLocalPosition;
                spawned.transform.localScale *= scaleFactor;
                InitializeCubes(spawned, image.GetPixel(i,j), transparency);
            }
        }
    }
    private void Clear()
    {
        if (spawnPoint != null && spawnPoint.childCount >= 1)
        {
            Transform[] children = spawnPoint.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == spawnPoint)
                {
                    continue;
                }
                else
                {
                    DestroyImmediate(children[i].gameObject, true);
                }
            }
        }
        matDatabase.Clear();
    }

    private void InitializeCubes(Transform target, Color Color, float value)
    {
        //Debug.Log("Database count: " + matDatabase.Count);

        var mat = new Material(baseMat);
        mat.color = new Color(Color.r, Color.g, Color.b, value);
        Material search = null;
        search = SearchOnMaterialDatabase(mat.color);
        if (search != null)
        {
            target.gameObject.GetComponent<MeshRenderer>().material = search;
        }
        else
        {
            AssetDatabase.CreateAsset(mat, "Assets/Art/" + folderName + "/cubeMat" + matDatabase.Count + ".mat");
            matDatabase.Add(mat);
            target.gameObject.GetComponent<MeshRenderer>().material = mat;
        }
    }
    private Material SearchOnMaterialDatabase(Color color)
    {
        bool found = false;
        int index = 0;
        for(int i = 0; i < matDatabase.Count; i++)
        {
            var vec1 = new Vector3(color.r, color.g, color.b);
            var vec2 = new Vector3(matDatabase[i].color.r, matDatabase[i].color.g, matDatabase[i].color.b);
            if(Vector3.Distance(vec1, vec2) <= 0.1f)
            {
                found = true;
                index = i;
            }
        }
        if (!found)
        {
            return null;
        }
        else
        {
            return matDatabase[index];
        }
    }
}
