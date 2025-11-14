using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

public class SpriteLibrary : MonoBehaviour
{
    public Image[] sprites;
    public Image[] abc;
    
    private static bool english;
    
    
    public void SetGerman()
    {
        SetB(false);
    }

    public void SetEnglish()
    {
        SetB(true);
    }

    private void SetB(bool setB)
    {
        int c = sprites.Length;
        for (int i = 0; i < c; i++)
            sprites[i].sprite = null;
        
        SpritePackage package = Resources.Load<SpritePackage>(setB? "EN":"DE");
        for (int i = 0; i < c; i++)
            sprites[i].sprite = package.sprites[i];

        package = null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteLibrary))]
public class SpriteLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Library"))
        {
            Object[] activeAndInactive = Resources.FindObjectsOfTypeAll(typeof(Image));

            int count = activeAndInactive.Length;
            
            List<SwapSprites> collection = new List<SwapSprites>();
            
            List<Image> images = new List<Image>();
            List<Sprite> de = new List<Sprite>();
            List<Sprite> en = new List<Sprite>();

            Scene active = SceneManager.GetActiveScene();
            for (int i = 0; i < count; i++)
            {
                Image im = activeAndInactive[i] as Image;
                if (im != null && im.gameObject.scene == active && im.sprite != null)
                {
                    Sprite a = im.sprite;
                    string path = AssetDatabase.GetAssetPath(a);
                   
                    Sprite b = AssetDatabase.LoadAssetAtPath<Sprite>(path.Substring(0, path.Length - 4) + "_EN.png");
                    if (b != null)
                    {
                        collection.Add(new SwapSprites(im, a, b)); 
                        
                        images.Add(im);
                        de.Add(a);
                        en.Add(b);
                    } 
                } 
            }
            
            SpriteLibrary lib = target as SpriteLibrary;
            lib.sprites = images.ToArray();
            lib.abc = images.OrderBy(x =>  x.name).ToArray();
            EditorUtility.SetDirty(lib);

            SpritePackage package = Resources.Load<SpritePackage>("DE");
            package.sprites = de.ToArray();
            EditorUtility.SetDirty(package);
            package = Resources.Load<SpritePackage>("EN");
            package.sprites = en.ToArray();
            EditorUtility.SetDirty(package);
        }
    }
}
#endif

[System.Serializable]
public class SwapSprites
{
    public Image image;
    public Sprite a, b;

    public SwapSprites(Image image, Sprite a, Sprite b)
    {
        this.image = image;
        
        this.a = a;
        this.b = b;
    }


    public void SetSprite(bool setB)
    {
        image.sprite = setB ? b : a;
    }


    public Sprite GetSprite(bool setB)
    {
        return setB ? b : a;
    }
}

