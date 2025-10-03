using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelLayerMapping", menuName = "Level Editor/Level Layer Mapping")]
public class LevelLayerMapping : ScriptableObject
{
    public string layerName = "New Layer";
    public List<ColorPrefabPair> colorMappings;

    private Dictionary<Color32, GameObject> _mappingDict;

    private void OnEnable()
    {
        _mappingDict = new Dictionary<Color32, GameObject>();
        foreach (var mapping in colorMappings)
        {
            if (!_mappingDict.ContainsKey(mapping.color))
            {
                _mappingDict.Add(mapping.color, mapping.prefab);
            }
        }
    }

    public bool GetPrefab(Color32 color, out GameObject prefab)
    {
        return _mappingDict.TryGetValue(color, out prefab);
    }
}