using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SafariGame : MonoBehaviour
{
    public Car car;
    public bool autoStart;
    
    public float gameLength;
    public float margin;

    [Space] 
    public GameObject[] objectsPrefab;
    public Sprite[] icons;

    [Space] 
    public SafariSettings settings;

    [Space] 
    public TextMeshProUGUI text;
    public TextMeshProUGUI timeText;

    [Space] 
    public CarInput input;

    [Space] public Sprite[] vultureSprites;
    
    private Vector3[] positions, antennas;
    private bool[] goodPos;
    private SpriteRenderer[] marker;

    private Transform[] vultures, vultureRotators;
    private SpriteRenderer[] vultureIcons, vultureRenderers;
    private float[] vultureAngles;
    
    private bool[] found;
    private int fCount;
    private float runTime;
    private bool gameOver;
    public static GameType CurrentGameType = GameType.None;

    private int count;
    
    public CarDetector detector;
    private static readonly int ACount = Shader.PropertyToID("aCount");

    private Vector3[] allPositions;
    private int spawnCount;
    private Transform trans;
    
    private static readonly int Antennas = Shader.PropertyToID("antennas");
    private static readonly int AntennaThresh = Shader.PropertyToID("antennaThresh");
    
    private readonly List<GameObject> thingsToDestroy = new List<GameObject>();

    private Vector3 carSpawn;

    public delegate void GameEnd();

    public event GameEnd OnGameEnd;

    public static int GameID;


    public static bool Running => !PopUpManager.ShowingPanel && !SplitAnim.Animating;

    public enum GameType
    {
        Solo, Mobilfunk_Solo, Mobilfunk_KI, Mobilfunk_KI_Vernetzt, Satellit_Solo, Satellit_KI, Satellit_KI_Vernetzt, None
    }


    private void Start()
    {
        trans = transform;

        //  Get All Spawns  //
        Transform posGroup = trans.GetChild(0);
        spawnCount = posGroup.childCount;
        allPositions = new Vector3[spawnCount];

        for (int i = 0; i < spawnCount; i++)
            allPositions[i] = posGroup.GetChild(i).position.SetZ(0);

        Destroy(posGroup.gameObject);

        carSpawn = car.transform.position;
        
        if(autoStart)
            StartGame(GameType.None);
    }


    public void DisableTexts()
    {
        text.enabled = false;
        timeText.enabled = false;
    }
    
    public float TimeLeft => CurrentGameType == GameType.None ? gameLength : Mathf.Max(0, gameLength - runTime);
    
    
    public void StartGame(GameType gameType)
    {
        GameID++;
        
        gameOver = false;
        
        car.ResetCarAndCam(carSpawn);
        
        CurrentGameType = gameType;
        
        SafariSettings.GameSettings gameSetting = settings.settings[(int) CurrentGameType];
        count = gameSetting.elephants + gameSetting.zebras;

        int dCount = thingsToDestroy.Count;
        for (int i = 0; i < dCount; i++)
            Destroy(thingsToDestroy[i]);
        thingsToDestroy.Clear();
        
        List<Vector3> all = new List<Vector3>();
        for (int i = 0; i < spawnCount; i++)
            all.Add(allPositions[i]);
       
        
        List<Vector3> leftOvers = new List<Vector3>();
        List<Vector3> picks = new List<Vector3>();

        const float minSpawnDist = 30, minSpawn = minSpawnDist * minSpawnDist;

        int allCount = all.Count;
        while (allCount > 0)
        {
            int pick = Random.Range(0, allCount);
            Vector3 pos = all[pick];
            all.RemoveAt(pick);
            allCount--;
            picks.Add(pos);
            
            for (int i = 0; i < allCount; i++)
            {
                Vector3 pos2 = all[i];
                if ((pos - pos2).sqrMagnitude < minSpawn)
                {
                    all.RemoveAt(i);
                    leftOvers.Add(pos2);
                    allCount--;
                    i--;
                }
            }
        }
        
        if(picks.Count < count)
            Debug.Log("Nooo " + picks.Count);
        else
        {
            //Debug.Log(picks.Count + " F " + count);
            int max = CurrentGameType != GameType.None ? count : 0;
            while (picks.Count > max)
            {
                int pick = Random.Range(0, picks.Count);
                Vector3 pos = picks[pick];
                picks.RemoveAt(pick);
                leftOvers.Add(pos);
            }
        }
        
        
        {
            goodPos = new bool[count];
            int good = 0;
            while (good < gameSetting.elephants)
            {
                int pick = Random.Range(0, count);
                if (!goodPos[pick])
                {
                    goodPos[pick] = true;
                    good++;
                }
            }
            
            positions = new Vector3[count];
            marker = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = picks[i];

                Transform el = Instantiate(objectsPrefab[goodPos[i]? 0 : 1], pos, Quaternion.identity, trans).transform;
                positions[i] = pos;
                marker[i] = el.GetChild(0).GetComponent<SpriteRenderer>();
                marker[i].sprite = icons[gameSetting.certainty == SafariSettings.Detection.Certain ? 0 : 1];
                marker[i].enabled = CurrentGameType != GameType.Solo;
                thingsToDestroy.Add(el.gameObject);
            }
        
            found = new bool[count];
            
        //  Antenna  //
            if (gameSetting.receivers)
            {
                const float minAntennaDist = 30, minAntenna = minAntennaDist * minAntennaDist;
                
                List<Vector2Int> howManySpawns = new List<Vector2Int>();
                int c = leftOvers.Count;
                for (int i = 0; i < c; i++)
                {
                    Vector3 pos = leftOvers[i];
                    int spawnsInSight = 0;
                    for (int e = 0; e < count; e++)
                        if ((pos - positions[e]).sqrMagnitude < minAntenna)
                            spawnsInSight++;

                    if (spawnsInSight == 0)
                    {
                        leftOvers.RemoveAt(i);
                        c--;
                        i--;
                    }
                    else
                        howManySpawns.Add(new Vector2Int(spawnsInSight, i));
                }
                
                List<Vector3> unseenSpawns = new List<Vector3>();
                for (int i = 0; i < count; i++)
                    unseenSpawns.Add(positions[i]);
                
                
                //  Order By SeenCount  //
                howManySpawns = howManySpawns.OrderBy(x => x.x).Reverse().ToList();
                List<Vector3> sortedLeftovers = new List<Vector3>();
                for (int i = 0; i < c; i++)
                    sortedLeftovers.Add(leftOvers[howManySpawns[i].y]);
                
                

                int uCount = unseenSpawns.Count;
                List<Vector3> antennaPos = new List<Vector3>();
                while (uCount > 0)
                {
                    int pick = Random.Range(0, c);
                    Vector3 pos = sortedLeftovers[pick];
                    sortedLeftovers.RemoveAt(pick);
                    c--;

                    Vector2Int seenSpawns = howManySpawns[pick];
                    howManySpawns.RemoveAt(pick);
                    
                    int s = 0;
                    for (int i = 0; i < uCount; i++)
                    {
                        Vector3 pos2 = unseenSpawns[i];
                        
                        if ((pos - pos2).sqrMagnitude < minAntenna)
                        {
                            unseenSpawns.RemoveAt(i);
                            i--;
                            uCount--;
                            s++;
                            if (s == seenSpawns.x)
                                break;
                        }
                    }



                    if (s != 0)
                    {
                        antennaPos.Add(pos);
                        thingsToDestroy.Add(Instantiate(objectsPrefab[2], pos, Quaternion.identity, trans));
                    }  
                }

                antennas = antennaPos.ToArray();

                int aCount = antennas.Length;
                Shader.SetGlobalBuffer(Antennas, Buff.New(antennas, 12));
                Shader.SetGlobalInt(ACount, aCount);
                Shader.SetGlobalFloat(AntennaThresh, 1f / (25 * 25));
            }
            else
            {
                antennas = new Vector3[0];
                Shader.SetGlobalInt(ACount, 0);
            }
            
        //  Vultures  //
            vultures        = new Transform[count];
            vultureRotators = new Transform[count];
            
            vultureAngles    = new float[count];
            vultureIcons     = new SpriteRenderer[count];
            vultureRenderers = new SpriteRenderer[count];
            
            for (int i = 0; i < count; i++)
            {
                     vultures[i] = Instantiate(objectsPrefab[3], trans).transform;
                     vultureRotators[i] = vultures[i].GetChild(0);
                 vultureIcons[i] = vultures[i].GetChild(1).GetComponent<SpriteRenderer>();
                 vultureRenderers[i] = vultureRotators[i].GetComponent<SpriteRenderer>();
                vultureAngles[i] = Random.Range(0, 360f);
                thingsToDestroy.Add(vultures[i].gameObject);
                vultures[i].position = Vector3.right * 10000;
            }
            
            
            text.text = CurrentGameType.ToString().Replace("_", " ") + " " + "0/" + count;
            timeText.text = TimeSpan.FromSeconds(gameLength).ToString(@"mm\:ss");
        }
        
        if(CurrentGameType == GameType.None)
        {
            text.text = "";
            timeText.text = "";
        }

        runTime = 0;
    }

    
    private void LateUpdate()
    {
        input.canControll = false;
        
        for (int i = 0; i < 7; i++)
            if(Input.GetKeyDown((KeyCode)(i + (int)KeyCode.Alpha0)))
            {
                SelectGameType(i);
                return;
            }
        
        if(CurrentGameType == GameType.None)
            return;

        input.canControll = !gameOver;
        
        if (!gameOver)
        {
            Vector3 pos = detector.currentPos;
            for (int i = 0; i < count; i++)
                if (!found[i])
                {
                    float dist = (positions[i] - pos).magnitude;
                    if (dist < detector.currentRadius - margin)
                    {
                        found[i] = true;
                        if (goodPos[i])
                            marker[i].sprite = icons[2];
                        else
                            marker[i].enabled = false;
                    }
                }

            int countF = 0;
            for (int i = 0; i < count; i++)
                if (found[i] && goodPos[i])
                    countF++;

            if (fCount != countF)
            {
                fCount = countF;
                text.text = fCount == count? "Alle gefunden!" : CurrentGameType.ToString().Replace("_", " ") + " " + fCount + "/" + count;
            
                if (fCount == count)
                    GameIsOver();
            }
        }
        
        if (!gameOver)
        {
            if(!PopUpManager.ShowingPanel)
                runTime += Time.deltaTime;

            float timeLeft = Mathf.Max(0, gameLength - runTime);
            if (timeLeft <= 0)
                GameIsOver();
            
            timeText.text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        }
        
        
    //  Vultures  //
        int acount = antennas.Length;
        SafariSettings.GameSettings gameSetting = settings.settings[(int) CurrentGameType];
        
        for (int i = 0; i < count; i++)
        {
            Quaternion rot = Quaternion.AngleAxis(vultureAngles[i] + runTime * 30, Vector3.forward);
            
            Vector3 pos = positions[i] + rot * Vector3.up * 9;
            
            vultures[i].position = pos;
            vultureRotators[i].rotation = rot * Quaternion.AngleAxis(90, Vector3.forward);
            vultureRenderers[i].sprite = vultureSprites[Mathf.FloorToInt(runTime * 24 + i * 10) % 50];

            bool detected = (int)CurrentGameType > 3;
            const float minAntennaDist = 25, minAntenna = minAntennaDist * minAntennaDist;
            
            for (int e = 0; e < acount; e++)
                if ((antennas[e] - pos).sqrMagnitude < minAntenna)
                {
                    detected = true;
                    break;
                }

            vultureIcons[i].enabled = detected;

            if (!found[i])
                marker[i].enabled = detected && gameSetting.activeMarkers;
        }
    }


    private void SelectGameType(int type)
    {
        gameOver = true;
        StartGame((GameType)type);
    }


    private void GameIsOver()
    {
        Debug.Log("Oy");
        bool wasGameOver = gameOver;
        gameOver = true;
        if(!wasGameOver)
            OnGameEnd?.Invoke();
    }


    private void OnDisable()
    {
        Shader.SetGlobalInt(ACount, 0);
    }


    public int Score
    {
        get
        {
            int f = 0;
            for (int i = 0; i < found.Length; i++)
                f += found[i] && goodPos[i] ? 1 : 0;

            return f;
        }
    }


    public float DroneCharge => detector.GetCharge;


    public void ForceGameOver(bool justGameOver = false)
    {
        if (justGameOver)
            gameOver = true;
        else
            GameIsOver();
    }
}
