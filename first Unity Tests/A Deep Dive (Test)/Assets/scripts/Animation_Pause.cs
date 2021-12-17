using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Pause : MonoBehaviour
{public Animation anim;
    // Start is called before the first frame update
    void Start()
    {
        anim=GetComponent<Animation>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnHoverEnter(Collider other)
        {
            anim.enabled=false;
            foreach(AnimationState state in anim )
           { state.speed = 0F;
            }
        }

        private void OnLastHoverExit(Collider other)
        {
            anim.enabled=true;
             foreach(AnimationState state in anim )
           { state.speed = 1F;
            }
     
        }
}
