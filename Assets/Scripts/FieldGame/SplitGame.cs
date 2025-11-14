using System.Collections.Generic;
using UnityEngine;


public class SplitGame : MonoBehaviour
{
    public bool player1, player2;
    public int windowType;
    
    private Transform trans;
    
//  Mesh  //
    private Mesh mesh;
        
    private readonly Vector3[] verts = new Vector3[4 * 4];
    private readonly int[] triangles = new int[4 * 6];
    private readonly Vector4[] uvs = new Vector4[4 * 4];
    private readonly Vector4[] quarterUvInfo = new Vector4[4];
    
    private float height, clipHeight, clipWidth;
    private Camera cam;
    private Vector3 min;
    
    
//  UI  //
    public List<AndyTouch> touches    = new List<AndyTouch>();
    public List<AndyTouch> oldTouches = new List<AndyTouch>();
    public List<Touch> currentTouches = new List<Touch>();  
    private readonly Touch[] dummyArray = new Touch[1];

    private bool paintA, eraseA;


    private bool visibleWindow;

    private bool drawingBlocked;
    
    
    private static readonly int vis = Shader.PropertyToID("Vis");
    private readonly int matID = Shader.PropertyToID("window");
    
    
    protected Material mat;
    
    private MeshRenderer mR;
    private GrabWindow window;
    
    
    public bool DrawingBlocked
    {
        set => drawingBlocked = value;
    }
    
    
    private string GrabWindowName(int id)
    {
        switch (id)
        {
            default:
                return "GrabWindow";
            
            case 1:
                return "GrabWindow2";
        }
    }
    
    
    public virtual void Init()
    {
        trans = transform;
        
    //  Move and Scale for Multiplayer  //
        if (player1 || player2)
        {
            /*const float s = 1920f / 2560;
            const float b = 199f / 2560;
            trans.localScale = Vector3.one * s;
            trans.localPosition = new Vector3(s * 4.5f * (player1? -1 : 1), -4.5f + (b + s * .5f) * 9, 0);*/
            trans.localScale = Vector3.one * .765f;
            trans.localPosition = new Vector3(3.44f * (player1? -1 : 1), .21f, 0);
        }
        
    //  Mesh  //
        cam = Camera.main;

        mesh = new Mesh {bounds = new Bounds(Vector3.zero, Vector3.one * 1000)};
        GetComponent<MeshFilter>().mesh = mesh;
        
        MeshPrepare();
        
        
    //  Windows  //
        GrabWindow.ResetMat(0);
        GrabWindow.ResetMat(1);
        
        Vector2 area  = new Vector2(clipWidth * 2 * (player1 || player2? .95f : 1), clipHeight) * transform.localScale;
        window = Instantiate(Resources.Load<GameObject>(GrabWindowName(windowType)), transform).GetComponent<GrabWindow>().Init(trans.position, area);
        
        
        mR = GetComponent<MeshRenderer>();
        mat = Instantiate(mR.material);
        mR.material = mat;
        mR.enabled = false;
    }

    
    private void OnDisable()
    {
        GrabWindow.ResetMat(0);
        GrabWindow.ResetMat(1);
    }
    
    
    public virtual void GameStop()
    {
        mR.enabled = false;
    }
    
    
    private void MeshPrepare()
    {
        height = cam.orthographicSize;

        clipWidth  = height;
        clipHeight = height;
        
        min = new Vector3(-height, -height);
        
        int i = 0;
        for (int y = 1; y > -1; y--)
        for (int x = 0; x <  2; x++)
        {
            
            int vert = i * 4;
            verts[vert]     = min + new Vector3(x * clipWidth,         y * clipHeight);
            verts[vert + 1] = min + new Vector3(x * clipWidth,         y * clipHeight + clipHeight);
            verts[vert + 2] = min + new Vector3(x * clipWidth + clipWidth, y * clipHeight + clipHeight);
            verts[vert + 3] = min + new Vector3(x * clipWidth + clipWidth, y * clipHeight);
            
            float lr = i % 2;
            
            uvs[vert]     = new Vector4(x * .5f,       y * .5f,      lr      ,lr);
            uvs[vert + 1] = new Vector4(x * .5f,       y * .5f + .5f, lr      , lr + .5f);
            uvs[vert + 2] = new Vector4(x * .5f + .5f, y * .5f + .5f, lr + .5f, lr + .5f);
            uvs[vert + 3] = new Vector4(x * .5f + .5f, y * .5f,      lr + .5f, lr );
            
            int tri = i * 6;
            triangles[tri]     = vert;
            triangles[tri + 1] = vert + 1;
            triangles[tri + 2] = vert + 2;
            
            triangles[tri + 3] = vert + 2;
            triangles[tri + 4] = vert + 3;
            triangles[tri + 5] = vert;
            
            quarterUvInfo[i] = new Vector4(min.x + x * clipWidth + clipWidth * .5f, 
                min.y + y * clipHeight + clipHeight * .5f, 
                x * .5f, 
                y * .5f);
                
            i++;
        }
        
        mesh.SetVertices(verts);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
    }


    protected bool GameUpdate(out Vector4 cursorA)
    {
        cursorA = new Vector4(0, 0, 0, 0);
        if (!mR.enabled)
            return false;
        
        int count = TouchUpdate();
        
        for (int i = 0; i < count; i++)
        {
            AndyTouch aT = touches[i];

            switch (aT.touchType)
            {
                case TouchType.WindowGrab:
                    window.GrabUpdate(aT.pos);
                    break;
            }
        }
        
        Shader.SetGlobalFloat(vis, SplitAnim.gameVis);
        window.WindowUpdate(SplitAnim.side > .85f && visibleWindow);// && !drawingBlocked);
        
     //  Cursors  //
        for (int i = 0; i < count; i++)
        {
            AndyTouch aT = touches[i];
            if (aT.touchType == TouchType.MapPaint)
            {
                Vector4 qInfo = quarterUvInfo[aT.grabID];
                Vector2 p = (transform.InverseTransformPoint(aT.pos).V2() - new Vector2(qInfo.x, qInfo.y)) / clipWidth;
                p += Vector2.one * .5f;
                p *= .5f;
                p += new Vector2(qInfo.z, qInfo.w);
              
                //cursorA = new Vector4(p.x, p.y, paintA? eraseA? -1 : drawingBlocked? 0 : 1 : 0, aT.grabID);
                //cursorA = new Vector4(p.x, p.y, drawingBlocked? 0 : paintA? eraseA? -1 : 1 : 0, aT.grabID);
                cursorA = new Vector4(p.x, p.y,  eraseA? -1 : paintA? (drawingBlocked? 0 : 1) : 0, aT.grabID);
             }
        }

        return true;
    }
    
    
    private int TouchUpdate()
    {
        if(!SplitAnim.GameView)
            return 0;
        
        int count = touches.Count;
        int oldCount = count;
        oldTouches.Clear();
        for (int i = 0; i < count; i++)
            oldTouches.Add(touches[i]);
        touches.Clear();
        
        currentTouches.Clear();
        count = Input.touchSupported? Input.touchCount : Input.GetMouseButton(0)? 1 : 0;
        Touch[] inputTouches = Input.touchSupported? Input.touches : dummyArray;
        for (int i = 0; i < count; i++)
            currentTouches.Add(inputTouches[i]);
        
        
        if (!Input.touchSupported)
            inputTouches[0] = new Touch {position = Input.mousePosition, fingerId = 100};
        
        Cursor.visible = !Input.touchSupported;

        
    #region Identify Touches By ID
        for (int i = 0; i < currentTouches.Count; i++)
        {
            Touch t = currentTouches[i];

            bool foundTouch = false;
            AndyTouch at = new AndyTouch();
            for (int e = 0; e < oldCount; e++)
            {
                AndyTouch ot = oldTouches[e];
                if (ot.fingerID == t.fingerId)
                {
                    foundTouch = true;
                    at = ot;
                    oldTouches.RemoveAt(e);
                    oldCount--;
                    currentTouches.RemoveAt(i);
                    i--;
                    break;
                }
            }
            
            if(!foundTouch)
                continue;
            
            Vector2 pos = cam.ScreenToWorldPoint(t.position);
            at.vel = pos - at.pos;
            at.pos = pos;
            at.fingerID = t.fingerId;

            touches.Add(at);
        }
    #endregion
    
    
    #region Identify Touches by Position

    if (currentTouches.Count > 0)
    {
        for (int i = 0; i < currentTouches.Count; i++)
        {
            Touch t = currentTouches[i];
            Vector2 pos = cam.ScreenToWorldPoint(t.position);
        
            bool foundTouch = false;
            AndyTouch at = new AndyTouch();
            for (int e = 0; e < oldCount; e++)
            {
                AndyTouch ot = oldTouches[e];
                float mag = (ot.pos - pos).magnitude;
                if (mag < 4f)
                {
                    foundTouch = true;
                    at = ot;
                    oldTouches.RemoveAt(e);
                    oldCount--;
                    currentTouches.RemoveAt(i);
                    i--;
                    break;
                }
            }
        
            at.vel = foundTouch? pos - at.pos : Vector2.zero;
            at.pos = pos;
            at.fingerID = t.fingerId;

            if (foundTouch)
            {
                at.grabID    = at.grabID;
                at.touchType = at.touchType;
                
                touches.Add(at);
            }
            else
                touches.Add(NewTouch(at));
        }
    }
    

    #endregion
        
        
        count = oldTouches.Count;
        for (int i = 0; i < count; i++)
        {
            AndyTouch ot = oldTouches[i];

            switch (ot.touchType )
            {
                case TouchType.WindowGrab:
                    window.StopGrabbing();
                    break;
                
                case TouchType.MapPaint:
                    paintA = false;
                   
                    break;
            }
        }
        
        return touches.Count;
    }


    private AndyTouch NewTouch(AndyTouch at)
    {
        if (!SuckButton.CanGameClick || PopUpManager.ShowingPanel)
        {
            at.touchType = TouchType.Nothing;
            return at;
        }
        
        Vector2 p = transform.InverseTransformPoint(at.pos);
        int x = Mathf.FloorToInt((p.x - min.x) / clipWidth);
        int y = Mathf.FloorToInt((p.y - min.y) / clipHeight);
            
        int grabID = (1 - y) * 2 + x;
        TouchType type = x < 0 || x > 1 || y < 0 || y > 1 ? TouchType.UI : TouchType.MapPaint;
                
        if (type == TouchType.MapPaint)
        {
                if (window.StartGrabbing(at.pos))
                {
                    type = TouchType.WindowGrab;
                    grabID = 0;
                }
        }

        if (type == TouchType.MapPaint)
        {
            if (!paintA)
                paintA = true;
            else
                type = TouchType.Nothing;
        }
                
        at.grabID    = grabID;
        at.touchType = type;
        
        return at;
    }
    
    
    public void SetEraser(bool erase)
    {
        eraseA = erase;
    }
    
    
    public Vector2 GetWindowPos()
    {
        Vector3 p = trans.InverseTransformPoint(window.VisPos);
        p.z = 1f / trans.localScale.x;
        mat.SetVector(matID, p);
        return window.VisPos;
    }
    
    
    public virtual SplitGame GameStart(bool activeWindow = true)
    {
        visibleWindow = activeWindow;
        window.ResetPos(trans.localPosition);
        mR.enabled = true;
        drawingBlocked = false;
        eraseA = false;
        
        return this;
    }
    
    
    [System.Serializable]
    public struct AndyTouch
    {
        public Vector2 pos;
        public Vector2 vel;
        public int fingerID, grabID;
        public TouchType touchType;

        public string Info => fingerID + " | " + touchType + " " + grabID + " | " + pos;
    }


    public enum TouchType
    {
        MapPaint,
        UI,
        WindowGrab,
        WindowAction,
        Slider,
        PaintIcon,
        EraseIcon,
        Nothing
    }
}
