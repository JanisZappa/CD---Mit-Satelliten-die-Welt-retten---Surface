using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class SpriteReplacer : MonoBehaviour
{
    public bool doInEditor;
    private void Start()
    {
        if(Application.isEditor && !doInEditor)
            return;
        
        Image[] images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];
       
        int count = images.Length;
        for (int i = 0; i < count; i++)
        {
            Image im = images[i];
            Sprite s = im.sprite;
            if (s != null)
                StartCoroutine(ReplaceSprite(im, s));
        }
    }

    private static IEnumerator ReplaceSprite(Image im, Sprite s)
    {
        string path = new DirectoryInfo(Application.streamingAssetsPath).FullName.Replace("/", "\\") + "\\" + s.name + ".png";
        
        if (!File.Exists(path))
            yield break;
        
        WWW www = new WWW("file://" + path);
        yield return www;

        Texture2D texTmp = new Texture2D(1920 * 2, 1080 * 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };


        www.LoadImageIntoTexture(texTmp);
        if (texTmp.width < 32 || texTmp.height < 32)
        { 
            yield break;
        }

        
       
        Sprite newS = Sprite.Create(texTmp, new Rect(0, 0, texTmp.width, texTmp.height), new Vector2(0.5f, 0.5f));
     
        im.sprite = newS;
    }
}
