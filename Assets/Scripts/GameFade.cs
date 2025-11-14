using UnityEngine;

public class GameFade : MonoBehaviour
{
    private static readonly int GameVis = Shader.PropertyToID("GameVis");


    private void LateUpdate()
    {
        Shader.SetGlobalFloat(GameVis, SplitAnim.gameVis);
    }
}
