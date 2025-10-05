using UnityEngine;
using System.IO;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
public class PaintCanvas : MonoBehaviour
{
    [SerializeField] private int _textureWidth = 512;
    [SerializeField] private int _textureHeight = 512;
    [SerializeField] private Color _backgroundColor = Color.white;
    [Tooltip("Brush width in pixels (e.g., 1 = 1x1, 2 = 2x2).")]
    [SerializeField][Min(1)] private int _brushSize = 1;

    [Header("Quick Build References")]
    [SerializeField] private LevelBuilder _levelBuilder;
    [SerializeField] private LevelLayerMapping _SOQuickLayer;

    private Texture2D _texture;
    private Color _currentBrushColor = Color.black;
    private new Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        InitializeTexture();
        
        PaintController.OnSaveTexture += SaveTexture;
        PaintController.OnClearTexture += ClearTexture;
        PaintController.OnQuickBuild += DoQuickBuild;
    }



    private void OnDestroy()
    {
        PaintController.OnSaveTexture -= SaveTexture;
        PaintController.OnClearTexture -= ClearTexture;
    }

    private void InitializeTexture()
    {
        _texture = new Texture2D(_textureWidth, _textureHeight);
        _texture.filterMode = FilterMode.Point;
        _renderer.material.mainTexture = _texture;
        ClearTexture();
    }
    
    public void ClearTexture()
    {
        Color[] colors = new Color[_textureWidth * _textureHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = _backgroundColor;
        }
        _texture.SetPixels(colors);
        _texture.Apply();
    }

    public void SetBrushColor(Color color)
    {
        _currentBrushColor = color;
    }

    public void Draw(Vector2 textureCoord, ToolType tool)
    {
        Color colorToDraw = (tool == ToolType.Eraser) ? _backgroundColor : _currentBrushColor;
        
        int centerX = (int)(textureCoord.x * _textureWidth);
        int centerY = (int)(textureCoord.y * _textureHeight);
        
        int halfSize = _brushSize / 2;
        int startX = centerX - halfSize;
        int startY = centerY - halfSize;
        
        // Para garantir que o tamanho seja exato, mesmo para números pares.
        int width = _brushSize;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                int pixelX = startX + x;
                int pixelY = startY + y;

                if (pixelX >= 0 && pixelX < _textureWidth && pixelY >= 0 && pixelY < _textureHeight)
                {
                    _texture.SetPixel(pixelX, pixelY, colorToDraw);
                }
            }
        }
        _texture.Apply();
    }

    public void DrawLine(Vector2 start, Vector2 end, ToolType tool)
    {
        int x0 = (int)(start.x * _textureWidth);
        int y0 = (int)(start.y * _textureHeight);
        int x1 = (int)(end.x * _textureWidth);
        int y1 = (int)(end.y * _textureHeight);
        
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            Draw(new Vector2((float)x0 / _textureWidth, (float)y0 / _textureHeight), tool);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
        _texture.Apply();
    }

    public void SaveTexture()
    {
        byte[] bytes = _texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SavedImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string fileName = "PaintedTexture_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        File.WriteAllBytes(dirPath + fileName, bytes);
        Debug.Log($"Saved texture to {dirPath}{fileName}");

        UnityEditor.AssetDatabase.Refresh();

        transform.DOScale(Vector3.one * 1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    public void DoQuickBuild()
    {
        // Adicionar como layer o arquivo atual
        LevelBuilder levelBuilderDebug = _levelBuilder;

        Debug.Log(levelBuilderDebug);

        _levelBuilder.LevelLayers.Add(new LevelLayer
        {
            name = "QuickLayer",
            layerTexture = _texture,
            layerMapping = _SOQuickLayer
        });

        // Chama o LevelBuilder para construir o nível
        _levelBuilder.BuildLevel();
    }
}