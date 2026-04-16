using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    
    bool inAction;
    
    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;
    
    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update() // Checking if the object is possible to climb and to start the appropriate animation
    {
        if (Input.GetButton("Jump") && !inAction)
        {
            var hitData = environmentScanner.ObstacleCheck();
            if (hitData.forwardHitFound)
            {
              foreach (var action in parkourActions)
              {
                if (action.CheckIfPossible(hitData, transform))
                {
                    StartCoroutine(DoParkourAction(action)); 
                    break;
                }
              }
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action) // player looses control and the parkour action is played
    {
        inAction = true;
        playerController.SetControl(false);
        
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
    
        var animState = animator.GetNextAnimatorStateInfo(0); // Checking if the correct animation is played
        if (!animState.IsName(action.AnimName))
            Debug.LogError("The parkour animation is wrong!");
        

        float timer = 0f;
        while(timer <= animState.length)
        {
            timer += Time.deltaTime;

            if (action.RotateToObstacle) // Ensaures that the action is performed facing the object, will rotate player to face the correct direction
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotationSpeed + Time.deltaTime);
            
                if (action.EnableTargetMatching)
                    MatchTarget(action);

                if (animator.IsInTransition(0) && timer > 0.5f)
                    break;
            
            yield return null;
        }

       yield return new  WaitForSeconds(action.PostActionDelay); // Delay the player from moving until the animation is complete
       
        playerController.SetControl(true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action) // Enuring that the animation matches the object and that the correct body part is the contact point for the animation
    {
        var animState = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.isMatchingTarget || animState.IsTag("Transition")) return;
        if (animator.IsInTransition(0)) return; 
        
        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }

}
