using System.Text;
using Unity.Profiling;
using UnityEngine;

/// @author 渡鸦 
/// <summary>
///     渲染统计信息
/// </summary>

public sealed class RenderingProfiler : MonoBehaviour
{
    private readonly StringBuilder _stringBuilder = new StringBuilder(223);

    private int _frameCount;
    private float _accumulatedFrameTime;
    private const float FrameSampleRate = 0.3f * 1000;

    private ProfilerRecorder _drawCallsRecorder;
    private ProfilerRecorder _batchesRecorder;
    private ProfilerRecorder _trianglesRecorder;
    private ProfilerRecorder _verticesRecorder;
    private ProfilerRecorder _setPassCallsRecorder;
    private ProfilerRecorder _shadowCastersRecorder;

    private bool _isDisplay;
    private Texture2D _background;
    private Color _textColor;
    private RectOffset _padding;

    private void Awake()
    {
        _background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));
        _textColor = new Color(0.9f, 0.9f, 0.9f);
        _padding = new RectOffset(10, 10, 5, 5);

        DontDestroyOnLoad(gameObject);
    }

    private GUIStyle CreateLabelStyle()
    {
        var style = new GUIStyle();
        style.normal.background = _background;
        style.normal.textColor = _textColor;
        style.padding = _padding;
        style.fontSize = 32;
        return style;
    }

    private GUIStyle CreateButtonStyle()
    {
        var style = new GUIStyle(GUI.skin.button);
        style.fontSize = 32;
        return style;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private void OnEnable()
    {
        _drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        _batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        _trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        _verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        _setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        _shadowCastersRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
    }

    private void OnDisable()
    {
        _drawCallsRecorder.Dispose();
        _batchesRecorder.Dispose();
        _trianglesRecorder.Dispose();
        _verticesRecorder.Dispose();
        _setPassCallsRecorder.Dispose();
        _shadowCastersRecorder.Dispose();
    }

    private void LateUpdate()
    {
        _accumulatedFrameTime += Time.unscaledDeltaTime * 1000.0f;
        ++_frameCount;
        if (_accumulatedFrameTime < FrameSampleRate)
        {
            return;
        }

        _stringBuilder.Clear();

        _stringBuilder.AppendLine("  >>> Statistics <<<");

        float fps = 1.0f / ((_accumulatedFrameTime * 0.001f) / _frameCount);
        _stringBuilder.AppendLine($"FPS: {fps:0.0}");
        _frameCount = 0;
        _accumulatedFrameTime = 0.0f;

        _stringBuilder.AppendLine($"Draw Calls: {_drawCallsRecorder.LastValue}");
        _stringBuilder.AppendLine($"Batches: {_batchesRecorder.LastValue}");
        _stringBuilder.AppendLine($"Tris: {_trianglesRecorder.LastValue / 1000f:0.0}k");
        _stringBuilder.AppendLine($"Verts: {_verticesRecorder.LastValue / 1000f:0.0}k");
        _stringBuilder.AppendLine($"Screen: {Screen.width}x{Screen.height}");
        _stringBuilder.AppendLine($"SetPass Calls: {_setPassCallsRecorder.LastValue}");
        _stringBuilder.AppendLine($"Shadow Casters: {_shadowCastersRecorder.LastValue}");
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(13, 100, 166, 66), "Statistics", CreateButtonStyle()))
        {
            _isDisplay = !_isDisplay;
        }

        if (_isDisplay)
        {
            GUI.Label(new Rect(13, 166, 321, 345), _stringBuilder.ToString(), CreateLabelStyle());
        }
    }
}