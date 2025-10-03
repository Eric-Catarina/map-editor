using UnityEngine;
using System.Collections.Generic;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] private List<LevelLayer> _levelLayers;
    [SerializeField] private Vector3 _gridSize = Vector3.one;
    [SerializeField] private Transform _levelRoot;

    [ContextMenu("Build Level")]
    public void BuildLevel()
    {
        if (_levelLayers == null || _levelLayers.Count == 0)
        {
            Debug.LogError("Level Builder has no layers configured.");
            return;
        }

        ClearLevel();
        GenerateLevel();
    }

    private void ClearLevel()
    {
        if (_levelRoot == null)
        {
            _levelRoot = new GameObject("Level").transform;
            _levelRoot.SetParent(this.transform);
        }

        for (int i = _levelRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_levelRoot.GetChild(i).gameObject);
        }
    }

    private void GenerateLevel()
    {
        foreach (var layer in _levelLayers)
        {
            if (layer.layerTexture == null || layer.layerMapping == null)
            {
                Debug.LogWarning($"Skipping layer '{layer.name}' due to missing texture or mapping.");
                continue;
            }

            var layerParent = new GameObject(layer.name).transform;
            layerParent.SetParent(_levelRoot);

            GenerateLayer(layer, layerParent);
            layerParent.localRotation = Quaternion.Euler(90, 0, 0); // Faz o layer nascer no plano XZ
        }
    }

    private void GenerateLayer(LevelLayer layer, Transform parent)
    {
        Color32[] pixels = layer.layerTexture.GetPixels32();
        int width = layer.layerTexture.width;
        int height = layer.layerTexture.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixelColor = pixels[y * width + x];

                if (pixelColor.a == 0)
                {
                    continue; // Pula pixels transparentes
                }

                if (layer.layerMapping.GetPrefab(pixelColor, out GameObject prefabToInstantiate))
                {
                    Vector3 position = new Vector3(x * _gridSize.x, y * _gridSize.y, 0);
                    Instantiate(prefabToInstantiate, position, Quaternion.identity , parent);
                }
            }
        }
    }
}