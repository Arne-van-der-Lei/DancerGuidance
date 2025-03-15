
using System;
using UdonSharp;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UdonSharpEditor;
using UnityEditor.Profiling;
using UnityEngine.EventSystems;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UISDFImage : UdonSharpBehaviour
{
    internal CanvasRenderer _image; 
    internal RectTransform _rectTransform;
    [SerializeField]
    private Sprite _sprite;
    
    [SerializeField]
    internal Material _material;
    [SerializeField]
    private Vector4 rounding;
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    internal Color OnEnterColor = Color.white;
    [SerializeField]
    internal Color OnPointerDownColor = Color.white;
    
    [SerializeField]
    internal UdonBehaviour ClickEvent;
    [SerializeField]
    internal string ClickEventName;
    [SerializeField]
    internal UISDFImageManager manager;
    
    private UISDFImage[] _children;
    
    public Vector4 Rounding
    {
        get => rounding;
        set
        {
            rounding = value;
            
            Vector4[] corners = new Vector4[4]
            {
                rounding,
                rounding,
                rounding,
                rounding
            };
            
            if(_mesh != null)
                _mesh.SetUVs(2,corners);
            if(_image != null)
                _image.SetMesh(_mesh);
        }
    }
    
    public Material Material
    {
        get => _material;
        set
        {
            _material = value;
            if (_image == null) return;
            _image.materialCount = 1;
            _image.SetMaterial(_material, 0);
        }
    }
    
    public Sprite Sprite
    {
        get => _sprite;
        set
        {
            _sprite = value;
            if (_image == null) return;
            _image.SetTexture(_sprite != null ? _sprite.texture : null);
        }
    }
    
    public Color Color
    {
        get => color;
        set
        {
            color = value;
            if (_image == null) return;
            _image.SetColor(color);
        }
    }
    
    internal Mesh _mesh;
    void Start()
    {
        _image = GetComponent<CanvasRenderer>();
        _rectTransform = GetComponent<RectTransform>();
        _image.materialCount = 1;
        _image.SetMaterial(_material,0);
        _image.SetColor(color);
        if(_sprite != null)
            _image.SetTexture(_sprite.texture);
        SetupMesh();
    }
    
    public void OnEnable()
    {
        if(_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
        if(manager != null)
            manager.Register(this,_rectTransform);
    }
    
    public void OnDisable()
    {
        if(manager != null)
            manager.Unregister(this);
    }

    internal void UpdateMesh()
    {
        Rect rect = _rectTransform.rect;
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(rect.xMin, rect.yMin),
            new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMax, rect.yMin),
            new Vector3(rect.xMax, rect.yMax)
        };
        
        Vector2 data = new Vector2(rect.width,rect.height);
        Vector2[] uv2 = new Vector2[4]
        {
            data,
            data,
            data,
            data
        };
        
        _mesh.vertices = vertices;
        _mesh.SetUVs(1,uv2);
        _mesh.RecalculateBounds();
        
        _image.SetMesh(_mesh);
        
        if(_sprite != null)
            _image.SetTexture(_sprite.texture);
    }

    internal void SetupMesh()
    {
        _mesh = new Mesh();
        
        Rect rect = _rectTransform.rect;
        Vector3[] vertices = new Vector3[4]
        {
            rect.min,
            new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMax, rect.yMin),
            rect.max
        };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1)
        };
        Vector2 data = new Vector2(rect.width,rect.height);
        Vector2[] uv2 = new Vector2[4]
        {
            data,
            data,
            data,
            data
        };
        
        int[] triangles = new int[6]
        {
            0,1,2,1,3,2
        };
        
        Vector4[] corners = new Vector4[4]
        {
            rounding,
            rounding,
            rounding,
            rounding
        };
        
        _mesh.vertices = vertices;
        _mesh.SetUVs(0,uv);
        _mesh.SetUVs(1,uv2);
        _mesh.SetUVs(2,corners);
        _mesh.triangles = triangles;
        
        _mesh.RecalculateBounds();
        
        _image.SetMesh(_mesh);
    }

    public void OnEnter()
    {
        _image.SetColor(OnEnterColor);
    }
    
    public void OnExit()
    {
        _image.SetColor(color);
    }
    
    public void OnPointerDown()
    {
        _image.SetColor(OnPointerDownColor);
    }

    public void OnPointerUp()
    {
        _image.SetColor(OnEnterColor);
    }
    
    public void OnClick()
    {
        if(ClickEvent != null)
            ClickEvent.SendCustomEvent(ClickEventName);
    }

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void OnValidate()
    {
        if(_image == null)
            _image = GetComponent<CanvasRenderer>();
        if(_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
        if(_mesh == null)
            SetupMesh();
        UpdateMesh();
        _image.materialCount = 1;
        _image.SetMaterial(_material,0);
        _image.SetColor(color);
        
        if(manager == null)
            manager = FindObjectOfType<UISDFImageManager>();
        
        EventTrigger trigger = GetComponent<EventTrigger>();
        if(trigger == null)
            trigger = gameObject.AddComponent<EventTrigger>();
        
        trigger.hideFlags = HideFlags.HideInInspector; 
        trigger.triggers.Clear();
        
        MethodInfo info = typeof(UdonBehaviour).GetMethod("SendCustomEvent",BindingFlags.Instance | BindingFlags.Public);
        
        UnityAction<string> action = (UnityAction<string>) Delegate.CreateDelegate(typeof(UnityAction<string>),GetComponent<UdonBehaviour>(),info);
        
        MethodInfo infoRegister = typeof(EventTrigger.TriggerEvent).GetMethod("AddStringPersistentListener",BindingFlags.Instance | BindingFlags.NonPublic);
        
        (EventTriggerType,string)[] events = new (EventTriggerType,string)[]
        {
            (EventTriggerType.PointerEnter,"OnEnter"),
            (EventTriggerType.PointerExit,"OnExit"),
            (EventTriggerType.PointerDown,"OnPointerDown"),
            (EventTriggerType.PointerUp,"OnPointerUp"),
            (EventTriggerType.PointerClick,"OnClick")
        };
        
        for (int i = 0; i < events.Length; i++)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = events[i].Item1;
            entry.callback = new EventTrigger.TriggerEvent();
            infoRegister.Invoke(entry.callback,new object[] {action,events[i].Item2});
            trigger.triggers.Add(entry);
        }
        
        Canvas canvas = GetComponentInParent<Canvas>();
        
        if(canvas != null)
        {
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
        }
    }
    #endif
}

#if !COMPILER_UDONSHARP && UNITY_EDITOR 
[CustomEditor(typeof(UISDFImage))]
public class UISDFImageEditor : Editor
{
    private UISDFImage _target;
    
    private void OnEnable()
    {
        _target = (UISDFImage) target;
    }
    
    public void OnSceneGUI()
    {
        if(_target == null) return;
        if(_target._image == null)
            _target._image = _target.GetComponent<CanvasRenderer>();
        if(_target._rectTransform == null)
            _target._rectTransform = _target.GetComponent<RectTransform>();

        if (_target._mesh == null)
        {
            _target.SetupMesh();
            return;
        }

        _target.UpdateMesh();
        _target._image.materialCount = 1;
        _target._image.SetMaterial(_target._material,0);
        _target._image.SetColor(_target.Color);
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        Sprite sprite = (Sprite) EditorGUILayout.ObjectField("Sprite",_target.Sprite,typeof(Sprite),false);
        Material material = (Material) EditorGUILayout.ObjectField("Material",_target.Material,typeof(Material),false);
        Vector4 rounding = EditorGUILayout.Vector4Field("rounding",_target.Rounding);
        Color color = EditorGUILayout.ColorField("Color",_target.Color);
        Color OnEnterColor = EditorGUILayout.ColorField("OnEnterColor",_target.OnEnterColor);
        Color OnPointerDownColor = EditorGUILayout.ColorField("OnPointerDownColor",_target.OnPointerDownColor);
        
        UdonBehaviour ClickEvent = (UdonBehaviour) EditorGUILayout.ObjectField("Click Event",_target.ClickEvent,typeof(UdonBehaviour),true);
        string ClickEventName = EditorGUILayout.TextField("Click Event Name",_target.ClickEventName);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_target, "Modify");
            _target.Rounding = rounding;
            _target.Sprite = sprite;
            _target.Material = material;
            _target.Color = color;
            
            _target.OnEnterColor = OnEnterColor;
            _target.OnPointerDownColor = OnPointerDownColor;
            
            _target.ClickEvent = ClickEvent;
            _target.ClickEventName = ClickEventName;
            _target.manager = FindObjectOfType<UISDFImageManager>();
        }
    }
}
#endif
