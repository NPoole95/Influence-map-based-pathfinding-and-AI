using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WallController : StationaryObject
{
    public LoadMapFromFile LMFF;
    private const int MAXHP = 100;
    // Start is called before the first frame update
    protected override void Awake()
    {
        HP = MAXHP;
        LMFF = GameObject.Find("LoadMapFromFile").GetComponent<LoadMapFromFile>();
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Find("HealthBar").localScale = new Vector3(HP/MAXHP, 1, 1);
        if (HP <= 0)
        {
            currentDefenceState = defenceState.destroyed;
        }

        if (currentDefenceState == defenceState.destroyed)
        {
            transform.gameObject.tag = "Untagged";
            Destroy(gameObject);
            LMFF.map[(int)transform.position.x, (int)transform.position.z] = 0;
        }
    }
}
