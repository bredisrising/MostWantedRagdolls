using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    // When enemies will be spawned, immediately add them to this list.
    public Transform[] enemies;

    public CinemachineTargetGroup cinemachineTargetGroup;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform enemy in enemies)
        {
            cinemachineTargetGroup.AddMember(enemy, 1, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
