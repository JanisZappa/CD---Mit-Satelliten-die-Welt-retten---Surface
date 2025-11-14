using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeCopy : MonoBehaviour
{
    public TextMeshProUGUI target;
    private TextMeshProUGUI me;
    
    
    private void Start()
    {
        me = GetComponent<TextMeshProUGUI>();
    }

    
    private void Update()
    {
        if(gameObject.activeInHierarchy)
            me.text = target.text;
    }
}
