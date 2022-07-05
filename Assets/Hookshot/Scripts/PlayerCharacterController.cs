/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour {

    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;

    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float characterVelocityY;
    private Vector3 characterVelocityMomentum;
    private Camera playerCamera;
    private CameraFov cameraFov;
    private ParticleSystem speedLinesParticleSystem;
    private State state;
    private Vector3 hookshotPosition;
    private float hookshotSize;

    private enum State {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
    }

    private void Awake() {
       
        
        cameraFov = playerCamera.GetComponent<CameraFov>();
        speedLinesParticleSystem = transform.Find("Camera").Find("SpeedLinesParticleSystem").GetComponent<ParticleSystem>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
    }

    private void Update() {
        switch (state) {
        default:
        case State.Normal:
            
            HandleHookshotStart();
            break;
        case State.HookshotThrown:
            HandleHookshotThrow();
           
            break;
        case State.HookshotFlyingPlayer:
           
            HandleHookshotMovement();
            break;
        }
    }

   

  
    private void ResetGravityEffect() {
        characterVelocityY = 0f;
    }

    private void HandleHookshotStart() {
        if (TestInputDownHookshot()) {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit)) {
                // Hit something
                debugHitPointTransform.position = raycastHit.point;
                hookshotPosition = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.HookshotThrown;
            }
        }
    }

    private void HandleHookshotThrow() {
        hookshotTransform.LookAt(hookshotPosition);

        float hookshotThrowSpeed = 500f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPosition)) {
            state = State.HookshotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
            speedLinesParticleSystem.Play();
        }
    }

    private void HandleHookshotMovement() {
        hookshotTransform.LookAt(hookshotPosition);

        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 5f;

        // Move Character Controller
        characterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        float reachedHookshotPositionDistance = 1f;
        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance) {
            // Reached Hookshot Position
            StopHookshot();
        }

        if (TestInputDownHookshot()) {
            // Cancel Hookshot
            StopHookshot();
        }

        if (TestInputJump()) {
            // Cancelled with Jump
            float momentumExtraSpeed = 7f;
            characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 40f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookshot();
        }
    }

    private void StopHookshot() {
        state = State.Normal;
        ResetGravityEffect();
        hookshotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
        speedLinesParticleSystem.Stop();
    }

    private bool TestInputDownHookshot() {
        return Input.GetKeyDown(KeyCode.E);
    }

    private bool TestInputJump() {
        return Input.GetKeyDown(KeyCode.Space);
    }


}
