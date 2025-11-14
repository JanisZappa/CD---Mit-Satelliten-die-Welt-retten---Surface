using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.Serialization;


public class CFTGame : SplitGame
{
    public VideoPlayer[] player;
    private int selected;
    private static readonly int VideoMask = Shader.PropertyToID("VideoMask");

    public static CFT_Quiz quiz;
    private static readonly int VideoB = Shader.PropertyToID("_VideoB");

    public static Question GetQuestion(int id)
    {
        if (quiz == null)
            GetQuiz();
        
        return quiz.Questions[id];
    }


    public static void GetQuiz()
    {
        DirectoryInfo rootDir = new DirectoryInfo(Application.streamingAssetsPath);
        TextReader reader = new StreamReader(rootDir + "\\CFT_Quiz_DE_EN.xml");
        quiz = (CFT_Quiz) new XmlSerializer(typeof(CFT_Quiz)).Deserialize(reader);
        reader.Close();
        reader.Dispose();
    }

    
    public override void Init()
    {
        base.Init();

        if (quiz == null)
            GetQuiz();
        
        
        SetActive(false);

        string[] p = { "CTA-Heute", "CTA-Szenario-A", "CTA-Szenario-B", "CTA-Szenario-C" };
        string ext = ".mp4";
       // ext = ".webm";
        
   ;     for (int i = 0; i < 4; i++)
        {
            player[i].url = Application.streamingAssetsPath + "/" + p[i] + ext;
            player[i].isLooping = true;
            player[i].Prepare();
        }  
    }
    
    
    public override SplitGame GameStart(bool activeWindow = true)
    {
        Debug.Log("CFT Start");
        
        SetActive(true);
        //game.StartGame(SafariGame.GameType.None);
        
        return base.GameStart(activeWindow);
    }


    public override void GameStop()
    {
        base.GameStop();
        
        Debug.Log("CFT Stop");
        //game.ForceGameOver(true);
        
        SetActive(false);
    }


    
    private void SetActive(bool value)
    {
        if (value)
        {
            for (int i = 0; i < 4; i++)
                player[i].Play();
            Shader.SetGlobalFloat(VideoMask, 0);
        }
        else
        {
            for (int i = 0; i < 4; i++)
                player[i].Stop();
            
            /*if(selected != 0)
                player[selected].targetTexture = null;*/
            
            Debug.Log("NotPlaying");
            
            Shader.SetGlobalFloat(VideoMask, 0);
        }
    }


    public void ShowVideo(int selected)
    {
        /*if(this.selected != 0)
            player[this.selected].targetTexture = null;*/
        
        this.selected = selected + 1;
        Debug.Log("Video: " + this.selected);

        //player[this.selected].targetTexture = rt;
        mat.SetTexture(VideoB, player[this.selected].targetTexture);
        Shader.SetGlobalFloat(VideoMask, 1);
    }


    public void StartFresh()
    {
       /* if(selected != 0)
            player[selected].targetTexture = null;*/
        
        selected = 0;
        Shader.SetGlobalFloat(VideoMask, 0);
        
    }


    private void Update()
    {
        if (GameUpdate(out Vector4 cursorA))
        {
            
        }
    }
}


[System.Serializable]
public class CFT_Quiz
{
    public List<Question> Questions = new List<Question>();
}

[System.Serializable]
public class Question
{
    public string QText;
    public string QText_EN;
    public List<Answer> Answers = new List<Answer>();
}
    
[System.Serializable]
public class Answer
{
    public string AText;
    public string AText_EN;
    public int Value;
}
