using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class Lab02SceneBuilder
{
    [MenuItem("Tools/Lab 02/Generate Level01")]
    public static void Generate()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var env = new GameObject("ENV");
        var gameplay = new GameObject("GAMEPLAY");
        var systems = new GameObject("SYSTEMS");
        var cam = Camera.main;
        if (cam == null) { var go = new GameObject("Main Camera"); cam = go.AddComponent<Camera>(); go.tag = "MainCamera"; }
        cam.orthographic = true;
        cam.orthographicSize = 6;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = Hex("#252A34");
        cam.clearFlags = CameraClearFlags.SolidColor;
        EnsureSortingLayers();

        string root = "Assets/Art/Tilesets/Kenney/";
        var all = AssetDatabase.FindAssets("t:Texture2D", new [] {root});
        var tilePaths = new System.Collections.Generic.List<string>();
        foreach (var id in all)
        {
            string path = AssetDatabase.GUIDToAssetPath(id);
            if (Path.GetDirectoryName(path).Replace("\\","/") == root.TrimEnd('/')) tilePaths.Add(path);
        }
        tilePaths.Sort();
        if (tilePaths.Count == 0) { Debug.LogError("Kenney tiles not found"); return; }
        Sprite Tile(int n) { return AssetDatabase.LoadAssetAtPath<Sprite>(tilePaths[n % tilePaths.Count]); }

        // Dark atmosphere, platformer layout with genuine Kenney tile sprites.
        var background = new GameObject("Background");
        background.transform.SetParent(env.transform);
        var b = background.AddComponent<SpriteRenderer>();
        b.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(root+"Backgrounds/tile_0000.png");
        b.sortingLayerName = "Background";
        b.color = new Color(0.19f,0.23f,0.29f,0.38f);
        background.transform.position = new Vector3(0,0,0);
        background.transform.localScale = new Vector3(15,10,1);

        var prefabGo = new GameObject("Platform");
        var sr = prefabGo.AddComponent<SpriteRenderer>();
        sr.sprite = Tile(0);
        sr.sortingLayerName = "Environment";
        sr.color = Hex("#E8E8E8");
        prefabGo.AddComponent<BoxCollider2D>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(prefabGo, "Assets/Prefabs/Platform.prefab");
        Object.DestroyImmediate(prefabGo);

        void Place(float x,float y,string name,Color tint)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name=name;
            go.transform.SetParent(gameplay.transform);
            go.transform.position=new Vector3(x,y,0);
            go.GetComponent<SpriteRenderer>().color=tint;
        }
        var dark = Hex("#AAB3C0");
        for(int i=-10;i<=10;i++) Place(i,-4.5f,"Ground_"+(i+10).ToString("00"),dark);
        for(int i=0;i<5;i++) Place(-7+i,-2.5f,"Platform_L_"+i,dark);
        for(int i=0;i<5;i++) Place(-1+i,-0.5f,"Platform_M_"+i,dark);
        for(int i=0;i<5;i++) Place(4+i,1.5f,"Platform_R_"+i,dark);

        var a = new GameObject("LoggerA"); a.transform.SetParent(systems.transform); a.AddComponent<LifecycleLogger>();
        var bLogger = new GameObject("LoggerB"); bLogger.transform.SetParent(systems.transform); bLogger.AddComponent<LifecycleLogger>();
        var soA = new SerializedObject(a.GetComponent<LifecycleLogger>());
        soA.FindProperty("label").stringValue = "A"; soA.ApplyModifiedProperties();
        var soB = new SerializedObject(bLogger.GetComponent<LifecycleLogger>());
        soB.FindProperty("label").stringValue = "B"; soB.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Level01.unity");
        var scenes = new [] { new EditorBuildSettingsScene("Assets/Scenes/Level01.unity",true) };
        EditorBuildSettings.scenes = scenes;
        AssetDatabase.SaveAssets();
        Debug.Log("Lab02: Level01 and Platform prefab generated. Check Scene/Game view and build.");
    }
    static Color Hex(string code) { ColorUtility.TryParseHtmlString(code,out var c); return c; }
    static void EnsureSortingLayers()
    {
        var asset=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var so=new SerializedObject(asset);
        var layers=so.FindProperty("m_SortingLayers");
        string[] names={"Background","Environment","Player","Enemies","Foreground"};
        foreach(string name in names)
        {
            bool exists=false;
            for(int i=0;i<layers.arraySize;i++) if(layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue==name) exists=true;
            if(!exists) {
                layers.InsertArrayElementAtIndex(layers.arraySize);
                var entry=layers.GetArrayElementAtIndex(layers.arraySize-1);
                entry.FindPropertyRelative("name").stringValue=name;
                entry.FindPropertyRelative("uniqueID").intValue=System.Math.Abs(name.GetHashCode())+1;
                entry.FindPropertyRelative("locked").boolValue=false;
            }
        }
        so.ApplyModifiedProperties();
    }
}
