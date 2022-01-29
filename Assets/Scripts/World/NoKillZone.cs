using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NoKillZone : MonoBehaviour
{
    private Transform _player;
    private Transform _camera;
    private Rigidbody _playerRigidBody;
    private RawImage _rawImage;

    private Vector3 _playerSpawnPosition;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _camera = _player.transform.Find("Camera");
        _playerRigidBody = _player.GetComponent<Rigidbody>();
        _rawImage = transform.Find("Canvas").GetComponent<RawImage>();

        _playerSpawnPosition = _player.position;
    }

    private IEnumerator Respawn()
    {
        // fade from transparent to opaque
        for (float i = 0; i <= 1; i += Time.deltaTime)
        {
            // set color with i as alpha
            _rawImage.color = new Color(1, 1, 1, i);
            yield return null;
        }
        
        // respawn
        _playerRigidBody.velocity = Vector3.zero;
        _playerRigidBody.angularVelocity = Vector3.zero;
        _player.position = _playerSpawnPosition;

        // fade from opaque to transparent
        for (float i = 1; i >= 0; i -= Time.deltaTime)
        {
            _rawImage.color = new Color(1, 1, 1, i);
            yield return null;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "Player")
        {
            StartCoroutine(Respawn());
        }
    }
}
