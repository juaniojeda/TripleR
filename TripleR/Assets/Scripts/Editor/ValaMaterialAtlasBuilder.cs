using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ValaMaterialAtlasBuilder
{
    private const string DefaultSourcePath = "Assets/Vala Data/model.obj";
    private const string OutputFolder = "Assets/Vala Data/Atlas";
    private const int CellSize = 32;
    private const int Padding = 2;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    // Un vértice compartido por submeshes necesita una copia para recibir otro UV.
    private struct VertexKey
    {
        public int SourceIndex;
        public int MaterialIndex;

        public VertexKey(int sourceIndex, int materialIndex)
        {
            SourceIndex = sourceIndex;
            MaterialIndex = materialIndex;
        }
    }

    [MenuItem("Tools/Vala Data/Build Color Atlas From Selection")]
    public static void BuildFromSelection()
    {
        Object selected = Selection.activeObject;
        GameObject sourcePrefab = ResolveSourcePrefab(selected);
        Build(sourcePrefab);
    }

    [MenuItem("Tools/Vala Data/Build Color Atlas For model.obj")]
    public static void BuildDefaultModel()
    {
        Build(AssetDatabase.LoadAssetAtPath<GameObject>(DefaultSourcePath));
    }

    private static void Build(GameObject sourcePrefab)
    {

        if (sourcePrefab == null)
        {
            Debug.LogError($"No source model selected and fallback not found at {DefaultSourcePath}.");
            return;
        }

        EnsureOutputFolder();

        string sourceName = SanitizeName(sourcePrefab.name);
        string sourceAssetPath = AssetDatabase.GetAssetPath(sourcePrefab);
        GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;

        if (instance == null)
            instance = Object.Instantiate(sourcePrefab);

        instance.name = $"{sourceName}_Atlas";
        Undo.RegisterCreatedObjectUndo(instance, "Build Vala material atlas");

        try
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            // Opacos y transparentes usan materiales y colas de render distintas.
            List<Material> opaqueMaterials = CollectMaterials(renderers, transparent: false);
            List<Material> transparentMaterials = CollectMaterials(renderers, transparent: true);

            Material opaqueAtlasMaterial = null;
            Material transparentAtlasMaterial = null;
            Dictionary<Material, Rect> opaqueRects = null;
            Dictionary<Material, Rect> transparentRects = null;

            if (opaqueMaterials.Count > 0)
            {
                Texture2D atlas = CreateColorAtlas(opaqueMaterials, out opaqueRects);
                Texture2D atlasAsset = SaveTexture(atlas, $"{sourceName}_OpaqueAtlas.png");
                opaqueAtlasMaterial = CreateAtlasMaterial($"{sourceName}_OpaqueAtlas_Mat", atlasAsset, transparent: false);
            }

            if (transparentMaterials.Count > 0)
            {
                Texture2D atlas = CreateColorAtlas(transparentMaterials, out transparentRects);
                Texture2D atlasAsset = SaveTexture(atlas, $"{sourceName}_TransparentAtlas.png");
                transparentAtlasMaterial = CreateAtlasMaterial($"{sourceName}_TransparentAtlas_Mat", atlasAsset, transparent: true);
            }

            int convertedRendererCount = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                MeshFilter meshFilter = renderers[i].GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = renderers[i] as MeshRenderer;

                if (meshFilter == null || meshRenderer == null || meshFilter.sharedMesh == null)
                    continue;

                Mesh atlasMesh = BuildAtlasMesh(
                    meshFilter.sharedMesh,
                    meshRenderer.sharedMaterials,
                    opaqueRects,
                    transparentRects,
                    out bool hasOpaque,
                    out bool hasTransparent);

                if (atlasMesh == null)
                    continue;

                string meshPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{sourceName}_{meshRenderer.name}_AtlasMesh.asset");
                AssetDatabase.CreateAsset(atlasMesh, meshPath);
                meshFilter.sharedMesh = atlasMesh;

                List<Material> finalMaterials = new List<Material>(2);

                if (hasOpaque && opaqueAtlasMaterial != null)
                    finalMaterials.Add(opaqueAtlasMaterial);

                if (hasTransparent && transparentAtlasMaterial != null)
                    finalMaterials.Add(transparentAtlasMaterial);

                meshRenderer.sharedMaterials = finalMaterials.ToArray();
                convertedRendererCount++;
            }

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{sourceName}_Atlas.prefab");
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Vala atlas built from '{sourceAssetPath}'. Converted {convertedRendererCount} mesh renderers. " +
                $"Output prefab: {prefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    [MenuItem("Tools/Vala Data/Build Color Atlas From Selection", true)]
    private static bool ValidateBuildFromSelection()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static GameObject ResolveSourcePrefab(Object selected)
    {
        if (selected is GameObject selectedGameObject)
            return selectedGameObject;

        if (selected != null)
        {
            string selectedPath = AssetDatabase.GetAssetPath(selected);
            GameObject loaded = AssetDatabase.LoadAssetAtPath<GameObject>(selectedPath);

            if (loaded != null)
                return loaded;
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(DefaultSourcePath);
    }

    private static List<Material> CollectMaterials(Renderer[] renderers, bool transparent)
    {
        List<Material> materials = new List<Material>(32);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] sharedMaterials = renderers[i].sharedMaterials;

            for (int j = 0; j < sharedMaterials.Length; j++)
            {
                Material material = sharedMaterials[j];

                if (material == null)
                    continue;

                if (IsTransparent(material) != transparent)
                    continue;

                if (!materials.Contains(material))
                    materials.Add(material);
            }
        }

        return materials;
    }

    private static Texture2D CreateColorAtlas(List<Material> materials, out Dictionary<Material, Rect> rectsByMaterial)
    {
        int columns = Mathf.CeilToInt(Mathf.Sqrt(materials.Count));
        int rows = Mathf.CeilToInt(materials.Count / (float)columns);
        int width = Mathf.Max(CellSize, columns * CellSize);
        int height = Mathf.Max(CellSize, rows * CellSize);

        Texture2D atlas = new Texture2D(width, height, TextureFormat.RGBA32, true, false)
        {
            name = "Vala_ColorAtlas",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] clearPixels = new Color[width * height];

        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = Color.clear;

        atlas.SetPixels(clearPixels);

        rectsByMaterial = new Dictionary<Material, Rect>(materials.Count);

        for (int i = 0; i < materials.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            int x = column * CellSize;
            int y = row * CellSize;
            Color color = GetMaterialColor(materials[i]);

            for (int py = y; py < y + CellSize; py++)
            {
                for (int px = x; px < x + CellSize; px++)
                {
                    atlas.SetPixel(px, py, color);
                }
            }

            // El margen evita que los mipmaps tomen color de una celda vecina.
            float paddedX = (x + Padding) / (float)width;
            float paddedY = (y + Padding) / (float)height;
            float paddedW = (CellSize - Padding * 2) / (float)width;
            float paddedH = (CellSize - Padding * 2) / (float)height;
            rectsByMaterial.Add(materials[i], new Rect(paddedX, paddedY, paddedW, paddedH));
        }

        atlas.Apply(true, false);
        return atlas;
    }

    private static Mesh BuildAtlasMesh(
        Mesh source,
        Material[] sourceMaterials,
        Dictionary<Material, Rect> opaqueRects,
        Dictionary<Material, Rect> transparentRects,
        out bool hasOpaque,
        out bool hasTransparent)
    {
        hasOpaque = false;
        hasTransparent = false;

        Vector3[] sourceVertices = source.vertices;
        Vector3[] sourceNormals = source.normals;
        Vector4[] sourceTangents = source.tangents;
        Color[] sourceColors = source.colors;
        Vector2[] sourceUv2 = source.uv2;

        List<Vector3> vertices = new List<Vector3>(source.vertexCount);
        List<Vector3> normals = sourceNormals.Length == source.vertexCount ? new List<Vector3>(source.vertexCount) : null;
        List<Vector4> tangents = sourceTangents.Length == source.vertexCount ? new List<Vector4>(source.vertexCount) : null;
        List<Color> colors = sourceColors.Length == source.vertexCount ? new List<Color>(source.vertexCount) : null;
        List<Vector2> uv = new List<Vector2>(source.vertexCount);
        List<Vector2> uv2 = sourceUv2.Length == source.vertexCount ? new List<Vector2>(source.vertexCount) : null;
        List<int> opaqueTriangles = new List<int>();
        List<int> transparentTriangles = new List<int>();
        Dictionary<VertexKey, int> remap = new Dictionary<VertexKey, int>();

        for (int submesh = 0; submesh < source.subMeshCount; submesh++)
        {
            Material sourceMaterial = submesh < sourceMaterials.Length ? sourceMaterials[submesh] : null;

            if (sourceMaterial == null)
                continue;

            bool isTransparent = IsTransparent(sourceMaterial);
            Dictionary<Material, Rect> rectLookup = isTransparent ? transparentRects : opaqueRects;

            if (rectLookup == null || !rectLookup.TryGetValue(sourceMaterial, out Rect rect))
                continue;

            List<int> targetTriangles = isTransparent ? transparentTriangles : opaqueTriangles;
            int[] sourceTriangles = source.GetTriangles(submesh);
            // Cada celda es plana; todos los vértices del submesh apuntan al centro.
            Vector2 atlasUv = rect.center;

            if (isTransparent)
                hasTransparent = true;
            else
                hasOpaque = true;

            for (int i = 0; i < sourceTriangles.Length; i++)
            {
                int sourceIndex = sourceTriangles[i];
                VertexKey key = new VertexKey(sourceIndex, submesh);

                if (!remap.TryGetValue(key, out int targetIndex))
                {
                    targetIndex = vertices.Count;
                    remap.Add(key, targetIndex);

                    vertices.Add(sourceVertices[sourceIndex]);
                    uv.Add(atlasUv);

                    if (normals != null)
                        normals.Add(sourceNormals[sourceIndex]);

                    if (tangents != null)
                        tangents.Add(sourceTangents[sourceIndex]);

                    if (colors != null)
                        colors.Add(sourceColors[sourceIndex]);

                    if (uv2 != null)
                        uv2.Add(sourceUv2[sourceIndex]);
                }

                targetTriangles.Add(targetIndex);
            }
        }

        if (vertices.Count == 0)
            return null;

        Mesh mesh = new Mesh
        {
            name = $"{source.name}_Atlas",
            indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
        };

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);

        if (uv2 != null)
            mesh.SetUVs(1, uv2);

        if (normals != null)
            mesh.SetNormals(normals);

        if (tangents != null)
            mesh.SetTangents(tangents);

        if (colors != null)
            mesh.SetColors(colors);

        mesh.subMeshCount = (hasOpaque ? 1 : 0) + (hasTransparent ? 1 : 0);
        int targetSubmesh = 0;

        if (hasOpaque)
            mesh.SetTriangles(opaqueTriangles, targetSubmesh++);

        if (hasTransparent)
            mesh.SetTriangles(transparentTriangles, targetSubmesh);

        if (normals == null)
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        return mesh;
    }

    private static Material CreateAtlasMaterial(string materialName, Texture2D atlas, bool transparent)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader)
        {
            name = materialName
        };

        if (material.HasProperty(BaseMapId))
            material.SetTexture(BaseMapId, atlas);

        if (material.HasProperty(MainTexId))
            material.SetTexture(MainTexId, atlas);

        if (material.HasProperty(BaseColorId))
            material.SetColor(BaseColorId, Color.white);

        if (material.HasProperty(ColorId))
            material.SetColor(ColorId, Color.white);

        if (transparent)
            ConfigureTransparentMaterial(material);

        string materialPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{materialName}.mat");
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.renderQueue = (int)RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private static bool IsTransparent(Material material)
    {
        Color color = GetMaterialColor(material);

        if (color.a < 0.99f)
            return true;

        if (material.HasProperty("_Surface") && material.GetFloat("_Surface") > 0.5f)
            return true;

        return material.renderQueue >= (int)RenderQueue.Transparent;
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty(BaseColorId))
            return material.GetColor(BaseColorId);

        if (material.HasProperty(ColorId))
            return material.GetColor(ColorId);

        return Color.white;
    }

    private static Texture2D SaveTexture(Texture2D texture, string fileName)
    {
        string path = $"{OutputFolder}/{fileName}";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = true;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void EnsureOutputFolder()
    {
        EnsureFolder("Assets", "Vala Data");
        EnsureFolder("Assets/Vala Data", "Atlas");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static string SanitizeName(string name)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            name = name.Replace(invalidChar, '_');

        return name.Replace(' ', '_');
    }
}
