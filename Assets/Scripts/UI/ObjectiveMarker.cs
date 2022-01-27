using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ObjectiveMarker : MonoBehaviour
{
    [SerializeField] private Image m_objectiveMarker;
    private Transform m_objective;
    private TextMeshProUGUI m_distanceDisplay;
    private Canvas m_canvas;

    [SerializeField] private float m_minX;
    [SerializeField] private float m_maxX;
    [SerializeField] private float m_minY;
    [SerializeField] private float m_maxY;

    private float m_displayTime = 3f;
    private float m_timeRemainingOnDisplay = 0f;
    private bool m_isDisplayed;
    private float m_fadeTime = 0.3f;

    private Color m_baseObjectiveMarkerColor;
    private Color m_baseDistanceDisplayColor;

    private void Awake()
    {
        m_distanceDisplay = m_objectiveMarker.GetComponentInChildren<TextMeshProUGUI>();
        m_baseObjectiveMarkerColor = m_objectiveMarker.color;
        m_baseDistanceDisplayColor = m_distanceDisplay.color;

        /*m_objectiveMarker.color = new Color( 0, 0, 0, 0 );
        m_distanceDisplay.color = new Color( 0, 0, 0, 0 );*/

        SetObjective( GameObject.Find( "CheesePrefab" ).transform );
    }

    private void SetObjective( Transform obj = null )
    {
        m_objective = obj.transform;
    }

    private void FadeInObjectiveMarker()
    {
        m_objectiveMarker.DOColor( m_baseObjectiveMarkerColor, m_fadeTime );
        m_distanceDisplay.DOColor( m_baseDistanceDisplayColor, m_fadeTime );
    }

    private void FadeOutObjectiveMarker()
    {
        m_objectiveMarker.DOColor( new Color( 0, 0, 0, 0 ), m_fadeTime );
        m_distanceDisplay.DOColor( new Color( 0, 0, 0, 0 ), m_fadeTime );
    }

    private void Update()
    {
        if( Input.GetButton( "ShowObjectiveMarker" ) && m_objective )
        {
            if( !m_isDisplayed )
            {
                m_isDisplayed = true;
                FadeInObjectiveMarker();
            }
                
            m_timeRemainingOnDisplay = m_displayTime;
        }

        if( m_isDisplayed )
        {
            m_minX = m_objectiveMarker.GetPixelAdjustedRect().width / 2;
            m_maxX = Screen.width - m_minX;

            m_minY = m_objectiveMarker.GetPixelAdjustedRect().height / 2;
            m_maxY = Screen.height - m_minY;

            Vector2 pos = Camera.main.WorldToScreenPoint( m_objective.position );

            if( Vector3.Dot( ( m_objective.position - transform.position ), transform.forward ) < 0 )
            {
                if( pos.x < Screen.width / 2 )
                    pos.x = m_maxX;
                else
                    pos.x = m_minX;
            }

            Debug.Log( pos.y );

            pos.x = Mathf.Clamp( pos.x, m_minX, m_maxX );
            pos.y = Mathf.Clamp( pos.y, m_minY, m_maxY );

            m_objectiveMarker.transform.position = pos;

            m_distanceDisplay.text = ConvertUtils.ConvertDistanceToReadableValue( m_objective.position, transform.position );

            m_timeRemainingOnDisplay -= Time.deltaTime;

            if( m_timeRemainingOnDisplay <= 0 )
            {
                m_isDisplayed = false;
                FadeOutObjectiveMarker();
            }
        }
    }
}
