using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Collectible : MonoBehaviour
{
    [Header( "Animation" )]

    [SerializeField] private float m_secondsPerLoop;
    [SerializeField] private int m_turnsPerLoop;
    [SerializeField] private float m_upMovementOffset;

    private GameObject m_cheeseContainer;
    private float m_baseHeight;

    private void Awake()
    {
        m_cheeseContainer = transform.Find( "CheeseContainer" ).gameObject;

        StartCoroutine( DoCollectibleAnimation() );
    }

    private IEnumerator DoCollectibleAnimation()
    {
        Sequence animation = DOTween.Sequence();
        m_cheeseContainer.transform.DOLocalRotate( Vector3.up * 360, m_secondsPerLoop, RotateMode.LocalAxisAdd ).SetEase( Ease.Linear );
        yield return m_cheeseContainer.transform.DOLocalMoveY( m_baseHeight + m_upMovementOffset, m_secondsPerLoop / 2 ).SetEase( Ease.InOutSine ).WaitForCompletion();
        yield return m_cheeseContainer.transform.DOLocalMoveY( m_baseHeight, m_secondsPerLoop / 2 ).SetEase( Ease.InOutSine ).WaitForCompletion();

        StartCoroutine( DoCollectibleAnimation() );
    }

    private void OnTriggerEnter( Collider other )
    {
        if( other.tag == "Player" )
        {
            Debug.Log( "End of Level Scripting" );
            DOTween.Clear();
            gameObject.SetActive( false );
        }
    }
}
