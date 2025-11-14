using GeoMath;
using UnityEngine;


public class OilTrap : MonoBehaviour
{
    public ComputeShader compute;

    [Space] 
    public Texture2D[] spills;
   
    
    private RenderTexture tex;

    private readonly int[]  values   = new int[total];
    private readonly int[]  frontier = new int[total];
    private readonly int[]  check    = new int[total];
    private readonly int[]  spill    = new int[total];
    private readonly bool[] good     = new bool[total];
    private readonly Color[] spillColor = new Color[total];
    private Texture2D spillTex;
    
    private const int size = 256, total = size * size;

    private int drawK, lineK, eraseK;
    private ComputeBuffer canvas, args;

    private bool drawing, blocked;
    private Vector2Int last, start;
    private int area;
    public int score;
   
    private static readonly int Drawings = Shader.PropertyToID("_Drawings");
    private static readonly int Oil      = Shader.PropertyToID("_Oil");
    private static readonly int Eraser   = Shader.PropertyToID("_Eraser");

    public Score result;
    [Space] 
    public Texture2D[] ships;
    
    private bool edit;
    private Vector3 oldCursor;

    private int[] order;
    private Material mat;
    private int shipA, shipB;
    private static int Rand;
    private float anim;
    
    public void Init(Material mat, bool player1, bool player2)
    {
        this.mat = mat;
        
        CreateBlockedArea(player1 ? 1 : player2 ? 2 : 0);
        
        order = new int[spills.Length];
        compute = Instantiate(compute);
        
        drawK = compute.FindKernel("Draw");
        lineK = compute.FindKernel("Line");
        eraseK = compute.FindKernel("Erase");
        
        tex = new RenderTexture(size, size, 8) { enableRandomWrite = true };
        tex.Create();
        compute.SetTexture(drawK, "Result", tex);
        compute.SetTexture(lineK, "Result", tex);
        
        canvas = Buff.New(values, 4);
        compute.SetBuffer(drawK, "Canvas", canvas);
        compute.SetBuffer(lineK, "Canvas", canvas);
        
        args = Buff.Add(compute.SetupIndirect(total, "size"));
        
        compute.SetBuffer(drawK, "Colors", Buff.New(GetTestColors(), 4 * 4));
        
        RenderTexture erase = new RenderTexture(size, size, 16) { enableRandomWrite = true };
        erase.Create();
        compute.SetTexture(eraseK, "Eraser", erase);
        mat.SetTexture(Eraser, erase);
        
        mat.SetTexture(Drawings, tex);
        mat.SetTexture(Oil, CreateSpillTex(true));
    }


    private float S(float t)
    {
        float s = Mathf.Sin(t);
        return s;
        return (1f - Mathf.Pow(1f - Mathf.Abs(s), 2)) * Mathf.Sign(s);
    }

    
    public void GameUpdate(Vector4 cursorA)
    {
        int x = Mathf.FloorToInt(cursorA.x * size);
        int y = Mathf.FloorToInt(cursorA.y * size);
            
        Vector2Int coords = new Vector2Int(Mathf.Clamp(x, 1, size - 2), Mathf.Clamp(y, 1, size - 2));
        int input = Mathf.RoundToInt(cursorA.z);
        switch (input)
        {
            case 1:
                if (drawing)
                {
                    plotLine(last.x, last.y, coords.x, coords.y);
                    edit = true;
                }
                    
                else
                {
                    start = coords;
                    SetPixel(coords);
                    edit = true;
                }   

                drawing = true;
                last = coords;
                
                break;
            
            case 0:
                if (drawing)
                {
                    if((new Vector2(start.x, start.y) - new Vector2(last.x, last.y)).magnitude < 15)
                        plotLine(last.x, last.y, start.x, start.y);

                    Validate();
                    edit = true;
                } 
                drawing = false;
                blocked = false;
                break;
            
            case -1:
                if(!blocked)
                    if (Erase(coords))
                    {
                        edit = true;
                        // blocked = true;
                    }
                        
                break;
        }

        if (edit)
        {
            canvas.SetData(values);
            compute.DispatchIndirect(lineK, args);
        }
        
        edit = false;
        
        Vector3 newCursor = new Vector3(coords.x * 1f / size, coords.y * 1f / size, input == -1? 1 : 0);
        if (Mathf.Approximately(newCursor.z, 0))
        {
            newCursor = new Vector3(oldCursor.x, oldCursor.y, 0);
        }
            
        
        if (Mathf.Approximately(oldCursor.z, 0))
            oldCursor = newCursor;
        
        compute.SetVector("cursorB", newCursor);
        compute.SetVector("cursor", oldCursor);
        oldCursor = newCursor;
        compute.DispatchIndirect(eraseK, args);

        anim += Time.deltaTime;
        mat.SetTexture(ShipA, ships[shipA + Mathf.FloorToInt((S(anim * 1.5f) * .5f + .5f) * 18) % 18]);
        mat.SetTexture(ShipB, ships[shipB + Mathf.FloorToInt((S(anim * 1.5f + 1.23f) * .5f + .5f) * 18) % 18]);
        
        
        return;
        for (int i = 0; i < placedQuads; i++)
            boxes[i].Draw(Color.magenta);

        //if(playerType != 2)
            for (int i = 0; i < blocks; i++)
                blockedArea[i].Draw(COLOR.blue.cornflower);

        DRAW.Rectangle(Vector3.one * .5f, Vector2.one).SetColor(Color.white);
    }


    public void Clear(bool newSpills = false)
    {
        if (newSpills)
        {
            result = new Score(0);
            anim = Random.Range(0, 20f);
            
            for (int i = 0; i < total; i++)
                values[i] = 0;
        }
           

        if (newSpills)
            CreateSpillTex(false);

        edit = true;
    }


    private void Validate()
    {
        //float t = Time.realtimeSinceStartup;
        DetectAreas();
        Outside0Inside2();
        LinesThatAreOnTheOutside();
        LinesThatAreNextToBubbles();
        SpillValidation();
        //Debug.Log(Time.realtimeSinceStartup - t);
    }


    private void DetectAreas()
    {
        int fCount = 0, cCount = 0;
        area = 0;

        for (int i = 0; i < total; i++)
        {
            int v = values[i];
            if (v != 1 && v != 3)
            {
                values[i] = 0;
                check[cCount++] = i;   
            }
            else
                values[i] = 1;
        }
        
        while (cCount > 0)
        {
            if(fCount == 0)
            {
                cCount--;
                if(cCount < 0)
                    goto done;

                int index = check[cCount];
                while (true)
                {
                    if (values[index] != 0)
                    {
                        cCount--;
                        if(cCount < 0)
                            goto done;
                        index = check[cCount];
                    }  
                    else
                        break;
                }
                
                frontier[fCount++] = index;
                values[index] = ++area + 1;
            }

            int f = frontier[--fCount];
            int x = f % size;
            int y = f / size;

            {
                int ax = x - 1;
                int ay = y;
                
                if (ax >= 0)
                {
                    int index = ax + ay * size;
                    if (values[index] == 0)
                    {
                        frontier[fCount++] = index;
                        values[index] = area + 1;
                    } 
                }
            }
            {
                int ax = x + 1;
                int ay = y;
                
                if (ax < size)
                {
                    int index = ax + ay * size;
                    if (values[index] == 0)
                    {
                        frontier[fCount++] = index;
                        values[index] = area + 1;
                    } 
                }
            }
            {
                int ax = x;
                int ay = y - 1;
                
                if (ay >= 0)
                {
                    int index = ax + ay * size;
                    if (values[index] == 0)
                    {
                        frontier[fCount++] = index;
                        values[index] = area + 1;
                    } 
                }
            }
            {
                int ax = x;
                int ay = y + 1;
                
                if (ay < size)
                {
                    int index = ax + ay * size;
                    if (values[index] == 0)
                    {
                        frontier[fCount++] = index;
                        values[index] = area + 1;
                    } 
                }
            }
        }
        
        done: ;
    }


    private void Outside0Inside2()
    {
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            int index = x + y * size;
            
            switch (values[index])
            {
                default: 
                    values[index] = 2;
                    break;
                
                case 0:
                case 1:
                    break;
                
                case 2:
                    values[index] = 0;
                    break;
            }
        }
    }
    
    private void LinesThatAreOnTheOutside()
    {
        int collect = 0;

        for (int i = 0; i < total; i++)
        {
            if(values[i] == 1)
            {
                int x = i % size;
                int y = i / size;
                
                {
                    int ax = x - 1;
                    int ay = y;
                
                    if (ax >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 0) 
                            goto isborder;
                    }
                }
                {
                    int ax = x + 1;
                    int ay = y;
                
                    if (ax < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 0) 
                            goto isborder;
                    }
                }
                {
                    int ax = x;
                    int ay = y - 1;
                
                    if (ay >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 0) 
                            goto isborder;
                    }
                }
                {
                    int ax = x;
                    int ay = y + 1;
                
                    if (ay < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 0) 
                            goto isborder;
                    }
                }

                check[collect++] = i;
                
                isborder: ;
            }
        }
        
        for (int i = 0; i < collect; i++)
            values[check[i]] = 2;
    }
    
    
    private void LinesThatAreNextToBubbles()
    {
        int collect = 0;
        
        for (int i = 0; i < total; i++)
        {
            if (values[i] == 1)
            {
                int x = i % size;
                int y = i / size;
                
                {
                    int ax = x - 1;
                    int ay = y;
                
                    if (ax >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 2) 
                            goto isborder;
                    }
                }
                {
                    int ax = x + 1;
                    int ay = y;
                
                    if (ax < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 2) 
                            goto isborder;
                    }
                }
                {
                    int ax = x;
                    int ay = y - 1;
                
                    if (ay >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 2) 
                            goto isborder;
                    }
                }
                {
                    int ax = x;
                    int ay = y + 1;
                
                    if (ay < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 2) 
                            goto isborder;
                    }
                }

                check[collect++] = i;
                
                isborder: ;
            }
        }
        
        for (int i = 0; i < collect; i++)
            values[check[i]] = 0;
    }


    private void SpillValidation()
    {
        DetectAreas();

        for (int i = 0; i < total; i++)
            good[i] = false;

        score = 0;
        for (int a = 0; a < area; a++)
        {
            int aValue = a + 3;
            int value = -1;
            int valueCount = 0;
            bool invalid = false;
            
            for (int i = 0; i < total; i++)
                if (values[i] == aValue)
                {
                    int sValue = spill[i];
                    if(sValue != -1)
                    {
                        if (value == -1)
                            value = sValue;
                        else
                            if (value != sValue)
                            {
                                invalid = true;
                                break;
                            }

                        valueCount++;
                    }
                }

            if (!invalid && value != -1 && valueCount == spillSize[value])
            {
                //Debug.Log(aValue + " !");
                for (int i = 0; i < total; i++)
                    if (values[i] == aValue)
                        good[i] = true;

                score++;
            }  
        }
        
        for (int i = 0; i < total; i++)
            switch (values[i])
            {
                case 1:
                    int x = i % size;
                    int y = i / size;
            
                    {
                        int ax = x - 1;
                        int ay = y;
                
                        if (ax >= 0)
                        {
                            int checkid = ax + ay * size;
                            if (good[checkid])
                                values[i] = 3;
                        }
                    }
                    {
                        int ax = x + 1;
                        int ay = y;
                
                        if (ax < size)
                        {
                            int checkid = ax + ay * size;
                            if (good[checkid])
                                values[i] = 3;
                        }
                    }
                    {
                        int ax = x;
                        int ay = y - 1;
                
                        if (ay >= 0)
                        {
                            int checkid = ax + ay * size;
                            if (good[checkid])
                                values[i] = 3;
                        }
                    }
                    {
                        int ax = x;
                        int ay = y + 1;
                
                        if (ay < size)
                        {
                            int checkid = ax + ay * size;
                            if (good[checkid])
                                values[i] = 3;
                        }
                    }
                    break;
                case 2:
                    values[i] = 0;
                    break;
                default:
                    values[i] = good[i] ? 4 : 2;
                    break;
            }
        
        result = new Score(score * 1f / maxSpills);
        //Debug.LogFormat("Score: {0} / {1}", score, maxSpills);
    }


    private bool Erase(Vector2Int coords)
    {
        int value = values[coords.x + coords.y * size];
        if(value != 2 && value != 4)
            return false;
        
        //Debug.Log("Value at Cursor: " + value);
        DetectAreas();
        
        DeleteSpill(values[coords.x + coords.y * size]);
        
        DetectAreas();
        Outside0Inside2();
        LinesThatAreOnTheOutside();
        LinesThatAreNextToBubbles();
        SpillValidation();

        return true;
    }


    private void DeleteSpill(int value)
    {
        //Debug.Log("Deleting Spill: " + value);
        for (int i = 0; i < total; i++)
        {
            int v = values[i];
            if (v == value)
            {
                values[i] = 0;

                int x = i % size;
                int y = i / size;

                {
                    int ax = x - 1;
                    int ay = y;

                    if (ax >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 1)
                            values[checkid] = 0;
                    }
                }
                {
                    int ax = x + 1;
                    int ay = y;

                    if (ax < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 1)
                            values[checkid] = 0;
                    }
                }
                {
                    int ax = x;
                    int ay = y - 1;

                    if (ay >= 0)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 1)
                            values[checkid] = 0;
                    }
                }
                {
                    int ax = x;
                    int ay = y + 1;

                    if (ay < size)
                    {
                        int checkid = ax + ay * size;
                        if (values[checkid] == 1)
                            values[checkid] = 0;
                    }
                }
            }
        }
    }





     private int placedQuads;
     private const int maxSpills = 5;
     private readonly Box[] boxes = new Box[maxSpills];
     private readonly int[] spillSize = new int[maxSpills];
     private readonly Texture2D[] sTex = new Texture2D[maxSpills];
     private Box[] blockedArea;
     private int blocks, playerType;
     private static readonly int ShipA = Shader.PropertyToID("_ShipA");
     private static readonly int ShipB = Shader.PropertyToID("_ShipB");
     private static readonly int ShipPos = Shader.PropertyToID("shipPos");


     private void CreateBlockedArea(int playerType)
     {
         this.playerType = playerType;

         Vector2 tools = new Vector2(.15f, .075f);//playerType == 0? .075f : .125f);
         Vector2 bar   = new Vector2(.075f, 1f);
         //Debug.Log(playerType);
         
         switch (this.playerType)
         {
             case 0:
                 blocks = 1;
                 blockedArea    = new Box[blocks];
                 blockedArea[0] = new Box(tools * .5f + new Vector2((1f - tools.x) * .5f, 0), 0, tools);
                 break;
             case 1:
                 blocks = 2;
                 blockedArea    = new Box[blocks];
                 blockedArea[0] = new Box(tools * .5f + new Vector2((1f - tools.x) * .5f, 0), 0, tools);
                 blockedArea[1] = new Box(bar * .5f + new Vector2((1f - bar.x), 0), 0, bar);
                 break;
             case 2: 
                 blocks = 2;
                 blockedArea    = new Box[blocks];
                 blockedArea[0] = new Box(tools * .5f + new Vector2((1f - tools.x) * .5f, 0), 0, tools);
                 blockedArea[1] = new Box(bar * .5f, 0, bar);
                 break;
         }
     }

     private Texture2D CreateSpillTex(bool create)
     {
         if(create)
             spillTex = new Texture2D(size, size);
         
         for (int i = 0; i < total; i++)
             spill[i] = -1;
         for (int i = 0; i < total; i++)
             spillColor[i] = Color.black;

         placedQuads = 0;

         const float margin = .005f, pad = .01f;

         int sCount = order.Length;
         for (int i = 0; i < sCount; i++)
             order[i] = i;

         int rCount = sCount * 4;
         for (int i = 0; i < rCount; i++)
         {
             int index = i % sCount;
             int v = order[index];
             int index2 = Random.Range(0, sCount);
             int v2 = order[index2];
             order[index2] = v;
             order[index] = v2;
         }
         
         while (placedQuads < maxSpills)
         {
             placedQuads = 0;
             
             for (int i = 0; i < 200; i++)
             {
                 Texture2D s = spills[order[placedQuads]];//Random.Range(0, sCount)];
                 Box b = new Box(Vector2.zero, Random.Range(0, 360f), new Vector2(s.width, s.height) * .00055f + Vector2.one * pad);
                 Bounds2D bounds = b.GetBounds;
                 Vector2 bsize = bounds.Size;
                 b = b.Move(bsize * .5f + new Vector2(margin + Random.Range(0, 1 - (bsize.x + margin * 2)),
                                Random.Range(0, 1 - (bsize.y + margin * 2))));

                 for (int j = 0; j < blocks; j++)
                     if (GJK.Intersection(blockedArea[j], b))
                         goto nope;
                 
                 for (int j = 0; j < placedQuads; j++)
                     if (GJK.Intersection(boxes[j], b))
                         goto nope;

                 sTex[placedQuads] = s;
                 boxes[placedQuads++] = b;
                 if (placedQuads == maxSpills)
                     break;

                 nope: ;
             }
         }


         for (int i = 0; i < maxSpills; i++)
         {
             Box b = boxes[i];
             Bounds2D bounds = b.GetBounds;
             int miX = Mathf.FloorToInt(bounds.minX * size);
             int miY = Mathf.FloorToInt(bounds.minY * size);
             int maX = Mathf.FloorToInt(bounds.maxX * size) + 1;
             int maY = Mathf.FloorToInt(bounds.maxY * size) + 1;
             int cX = maX - miX;
             int cY = maY - miY;

             Vector2 dc = b.d - b.c;
             Vector2 bc = b.b - b.c;
             float magX = dc.magnitude;
             float magY = bc.magnitude;
             dc /= magX;
             bc /= magY;
             magX = 1f / magX;
             magY = 1f / magY;

             float multi = 1f - i * (1f / 6) * .7f;
             int sSize = 0;

             Texture2D s = sTex[i];
             for (int x = 0; x < cX; x++)
             for (int y = 0; y < cY; y++)
             {
                Vector2 p = new Vector2(x + miX, y + miY) / size;
                Vector2 fromC = p - b.c;
                Vector2 uv = new Vector2(Vector2.Dot(fromC, dc) * magX, Vector2.Dot(fromC, bc) * magY);
                if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
                {
                    Color c = s.GetPixelBilinear(uv.x, uv.y);
                    float v = c.r;
                    int index = x + miX + (y + miY) * size;
                    bool countIt = v > .025f;
                    spillColor[index] = new Color(v, v * multi, countIt ? 1 : 0, 1);
                    if (countIt)
                    {
                      spill[index] = i;
                      sSize++;
                    }
                }
            }
            
            spillSize[i] = sSize;
         }
         

         spillTex.SetPixels(spillColor);
         spillTex.Apply();
         
         
     //  Ships  //
         shipA = (Rand++%4) * 18;
         shipB = (Rand++%4) * 18 + 4 * 18;

         Vector3 dir = Quaternion.AngleAxis(Rand++*66, Vector3.up) * Vector3.forward * .3f;
         
         mat.SetVector(ShipPos, new Vector4(dir.x * Random.Range(.8f, 1.1f), dir.z * Random.Range(.8f, 1.1f), -dir.x * Random.Range(.8f, 1.1f), -dir.z * Random.Range(.8f, 1.1f)));
         return spillTex;
     }
    
    
//  Whatever  //
    private void plotLine(int x0, int y0, int x1, int y1)
    {
        int dx =  Mathf.Abs(x1-x0), sx = x0<x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1-y0), sy = y0<y1 ? 1 : -1;
        int err = dx+dy;

        while(true)
        {
            SetPixel(new Vector2Int(x0,y0));
            
            if (x0==x1 && y0==y1) 
                break;
            
            int e2 = 2 * err; 
            
            if (e2 >= dy) 
            {
                err += dy; 
                x0  += sx; 
            }

            if (e2 <= dx)
            {
                err += dx; 
                y0  += sy;
            }
        }
    }


    private void SetPixel(Vector2Int coord)
    { 
        values[coord.y * size + coord.x] = 1;
    }


    private static Vector4[] GetTestColors()
    {
        Color[] colors =
        {
            Color.white, Color.black, 
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine,
            COLOR.red.tomato, COLOR.blue.cornflower, COLOR.green.spring, COLOR.orange.coral, COLOR.purple.orchid, COLOR.turquois.aquamarine
        };
        
        Vector4[] col = new Vector4[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Color c = colors[i];
            col[i] = new Vector4(c.r, c.g, c.b, c.a);
        }

        return col;
    }
    
    
    [System.Serializable]
    public struct Score
    {
        public float trappedSpills;

        public Score(float trappedSpills)
        {
            this.trappedSpills = trappedSpills;
        }
    }
}
