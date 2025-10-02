using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PaintController : MonoBehaviour
{
    public static event Action<Color> OnColorChanged;
    public static event Action<ToolType> OnToolChanged;
    public static event Action OnSaveTexture;
    public static event Action OnClearTexture;

    [SerializeField] private PaintCanvas _paintCanvas;
    [SerializeField] private InputActionReference _paintAction;
    [SerializeField] private InputActionReference _mousePositionAction;

    private Camera _mainCamera;
    private ToolType _currentTool = ToolType.Brush;
    private Vector2? _lineStartPoint = null;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _paintAction.action.performed += OnPaintPerformed;
        _paintAction.action.canceled += OnPaintCanceled;
    }

    private void OnDisable()
    {
        _paintAction.action.performed -= OnPaintPerformed;
        _paintAction.action.canceled -= OnPaintCanceled;
    }

    private void Update()
    {
        if (_paintAction.action.IsPressed())
        {
            HandlePainting();
        }
    }

    private void HandlePainting()
    {
        Vector2 mousePosition = _mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.transform == _paintCanvas.transform)
            {
                switch (_currentTool)
                {
                    case ToolType.Brush:
                    case ToolType.Eraser:
                        _paintCanvas.Draw(hit.textureCoord);
                        break;
                }
            }
        }
    }

    private void OnPaintPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = _mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.transform == _paintCanvas.transform)
        {
            switch (_currentTool)
            {
                case ToolType.Line:
                    if (_lineStartPoint == null)
                    {
                        _lineStartPoint = hit.textureCoord;
                    }
                    else
                    {
                        _paintCanvas.DrawLine(_lineStartPoint.Value, hit.textureCoord);
                        _lineStartPoint = null;
                    }
                    break;
            }
        }
    }
    
    private void OnPaintCanceled(InputAction.CallbackContext context)
    {
         // Lógica futura para ferramentas que precisam de um "soltar" do mouse
    }

    public void SetTool(ToolType newTool)
    {
        _currentTool = newTool;
        _lineStartPoint = null;
        OnToolChanged?.Invoke(newTool);

        if (newTool == ToolType.Eraser)
        {
            _paintCanvas.SetBrushColor(Color.clear); 
        }
    }

    public void SetColor(Color newColor)
    {
        if (_currentTool == ToolType.Eraser)
        {
            SetTool(ToolType.Brush);
        }
        _paintCanvas.SetBrushColor(newColor);
        OnColorChanged?.Invoke(newColor);
    }
    
    public void Save()
    {
        OnSaveTexture?.Invoke();
    }

    public void Clear()
    {
        OnClearTexture?.Invoke();
    }
}

public enum ToolType
{
    Brush,
    Eraser,
    Line
}