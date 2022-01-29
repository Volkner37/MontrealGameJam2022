using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BouncyText : MonoBehaviour
{
    [SerializeField] private float scaleSize;
    [SerializeField] private float timeScale;
    
    private TextMeshProUGUI text;
    private float initialSize;
    private float angle = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        initialSize = text.fontSize;
    }

    // Update is called once per frame
    void Update()
    {
        text.fontSize = Mathf.Sin(angle) * scaleSize + initialSize;

        angle += Time.deltaTime * timeScale;
        if (angle >= 360)
            angle = 0;
    }
}
