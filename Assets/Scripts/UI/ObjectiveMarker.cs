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

        m_objectiveMarker.color = new Color( 0, 0, 0, 0 );
        m_distanceDisplay.color = new Color( 0, 0, 0, 0 );

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

            Vector3 pos = Camera.main.WorldToScreenPoint( m_objective.position );

            if( !IsObjectiveInViewport( m_objective.position ) )
            {
                // Flipping if the objective is behind us
                if( pos.z < 0 )
                {
                    pos *= -1;
                }

                Vector3 screenCenter = new Vector3( Screen.width, Screen.height, 0 ) / 2;

                pos -= screenCenter;

                float angle = Mathf.Atan2( pos.y, pos.x );
                angle -= 90 * Mathf.Deg2Rad;

                float cos = Mathf.Cos( angle );
                float sin = -Mathf.Sin( angle );

                pos = screenCenter + new Vector3( sin, cos, 0 );

                float m = cos / sin;

                if( cos > 0 )
                {
                    pos = new Vector3( screenCenter.y / m, screenCenter.y, 0 );
                }
                else
                {
                    pos = new Vector3( -screenCenter.y / m, -screenCenter.y, 0 );
                }

                if( pos.x > screenCenter.x )
                {
                    pos = new Vector3( screenCenter.x, screenCenter.x * m, 0 );
                }
                else if( pos.x < -screenCenter.x )
                {
                    pos = new Vector3( -screenCenter.x, -screenCenter.x * m, 0 );
                }

                pos += screenCenter;
            }

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

    private bool IsObjectiveInViewport( Vector3 pos )
    {
        Vector3 testedPos = Camera.main.WorldToViewportPoint( pos );
        return ( testedPos.x > 0 && testedPos.x < 1 ) && ( testedPos.y > 0 && testedPos.y < 1 ) && testedPos.z > 0;
    }
}
