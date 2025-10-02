using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PaintUI : MonoBehaviour
{
    [SerializeField] private PaintController _paintController;
    [SerializeField] private Button _brushButton;
    [SerializeField] private Button _eraserButton;
    [SerializeField] private Button _lineButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Transform _colorPaletteContainer;
    [SerializeField] private GameObject _colorButtonPrefab;
    [SerializeField] private Color[] _paletteColors;

    private void Start()
    {
        _brushButton.onClick.AddListener(() => _paintController.SetTool(ToolType.Brush));
        _eraserButton.onClick.AddListener(() => _paintController.SetTool(ToolType.Eraser));
        _lineButton.onClick.AddListener(() => _paintController.SetTool(ToolType.Line));
        _saveButton.onClick.AddListener(() => _paintController.Save());
        _clearButton.onClick.AddListener(() => _paintController.Clear());

        PopulateColorPalette();
        
        PaintController.OnToolChanged += HandleToolChanged;
        PaintController.OnColorChanged += HandleColorChanged;
    }
    
    private void OnDestroy()
    {
        PaintController.OnToolChanged -= HandleToolChanged;
        PaintController.OnColorChanged -= HandleColorChanged;
    }

    private void PopulateColorPalette()
    {
        foreach (Color color in _paletteColors)
        {
            GameObject buttonObj = Instantiate(_colorButtonPrefab, _colorPaletteContainer);
            Image buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = color;
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                _paintController.SetColor(color);
                AnimateButton(buttonObj.transform);
            });
        }
    }

    private void HandleToolChanged(ToolType tool)
    {
        AnimateButton(_brushButton.transform, tool == ToolType.Brush);
        AnimateButton(_eraserButton.transform, tool == ToolType.Eraser);
        AnimateButton(_lineButton.transform, tool == ToolType.Line);
    }
    
    private void HandleColorChanged(Color color)
    {
        foreach (Transform child in _colorPaletteContainer)
        {
            bool isSelected = child.GetComponent<Image>().color == color;
            AnimateButton(child, isSelected);
        }
    }

    private void AnimateButton(Transform buttonTransform, bool selected = true)
    {
        buttonTransform.DOKill();
        if (selected)
        {
            buttonTransform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutBack);
        }
        else
        {
            buttonTransform.DOScale(Vector3.one, 0.15f);
        }
    }
}