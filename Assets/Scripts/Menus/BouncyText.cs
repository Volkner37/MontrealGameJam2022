using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class BouncyText : MonoBehaviour
{
    [SerializeField] private float bigSize;
    [SerializeField] private float timeScale;
    [SerializeField] private bool forceBounceOnStart;
    
    private TextMeshProUGUI text;
    private float initialSize;
    private bool isBouncing = false;
    
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        initialSize = text.fontSize;

        if( forceBounceOnStart )
        {
            isBouncing = true;
            StartCoroutine( BounceAnimation() );
        }
    }

    public void SetBounce( bool bounce )
    {
        if( DOTween.TotalActiveTweens() >= 0 )
        {
            DOTween.KillAll();
        }

        isBouncing = bounce;

        if( !isBouncing )
        {
            text.fontSize = initialSize;
            StopAllCoroutines();
        }
        else 
        {
            StartCoroutine( BounceAnimation() );
        }
    }

    private IEnumerator BounceAnimation()
    {
        yield return DOTween.To( x => text.fontSize = x, text.fontSize, bigSize, timeScale ).SetEase( Ease.InOutSine ).SetSpeedBased().WaitForCompletion();
        yield return DOTween.To( x => text.fontSize = x, text.fontSize, initialSize, timeScale ).SetEase( Ease.InOutSine ).SetSpeedBased().WaitForCompletion();

        StartCoroutine( BounceAnimation() );
    }
}
