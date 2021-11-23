using UnityEngine;
using System.Collections;
public class FacePlayer : MonoBehaviour
{

    private Transform player;

    void Start()
    {
        player = GameObject.Find("CenterEyeAnchor").transform;
    }

    void Update()
    {
        Vector3 v3 = player.position - transform.position;
        v3.y = 0.0f;
        transform.rotation = Quaternion.LookRotation(-v3);
    }
}