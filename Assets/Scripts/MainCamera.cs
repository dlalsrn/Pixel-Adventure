using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private Player player;
    private Transform playerTransform;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        playerTransform = player.transform;
    }

    void LateUpdate()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.position = playerTransform.position + new Vector3(0f, 0f, -10f);
    }
}
