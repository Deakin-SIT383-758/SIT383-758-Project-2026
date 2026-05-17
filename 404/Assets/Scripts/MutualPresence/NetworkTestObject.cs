using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
//This script just moves the object around to establish multiple connections
//Note this has a folly of if timing is perfect the objects become one, also causes screen tearing
public class NetworkTestObject : NetworkBehaviour
{
    public bool left = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    //Fixed update network needs to be used on network objects
    public override void FixedUpdateNetwork()
    {
        //just makes the object move left and right alot make it distinguishable from other users objects
        if (left)
        {
            transform.Translate(1,0,0);
            if (transform.position.x > 1)
            {
                left = false;
            }
        }
        else
        {
            transform.Translate(-1,0,0);
            if (transform.position.x < -1)
            {
                left = true;
            }
        }
    }
}
