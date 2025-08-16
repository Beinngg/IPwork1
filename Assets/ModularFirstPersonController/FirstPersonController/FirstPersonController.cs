using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Animation")]
    public Animator animator; // Assign Animator in Inspector

    #region Camera Movement Variables
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;
    private bool isZoomed = false;
    #endregion
    #endregion

    #region Movement Variables
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    private bool isWalking = false;

    #region Sprint
    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;
    #endregion

    #region Jump
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    private bool isGrounded = false;
    #endregion

    #region Crouch
    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;
    private bool isCrouched = false;
    private Vector3 originalScale;
    #endregion
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        if (crosshair && crosshairObject != null)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else if (crosshairObject != null)
        {
            crosshairObject.gameObject.SetActive(false);
        }

        sprintBarCG = GetComponentInChildren<CanvasGroup>();
        if (useSprintBar && sprintBarBG != null && sprintBar != null)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
                sprintBarCG.alpha = 0;
        }
        else if (sprintBarBG != null && sprintBar != null)
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }
    }

    private void Update()
{
    #region Camera
    if (cameraCanMove)
    {
        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch += invertCamera ? mouseSensitivity * Input.GetAxis("Mouse Y") : -mouseSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    if (enableZoom)
    {
        if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            isZoomed = !isZoomed;

        if (holdToZoom && !isSprinting)
            isZoomed = Input.GetKey(zoomKey);

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView,
            isZoomed ? zoomFOV : fov,
            zoomStepTime * Time.deltaTime);
    }
    #endregion

    #region Sprint
    if (enableSprint)
    {
        if (isSprinting)
        {
            isZoomed = false;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                }
            }
        }
        else
        {
            sprintRemaining = Mathf.Clamp(sprintRemaining + Time.deltaTime, 0, sprintDuration);
        }

        if (isSprintCooldown)
        {
            sprintCooldown -= Time.deltaTime;
            if (sprintCooldown <= 0)
                isSprintCooldown = false;
        }
        else
        {
            sprintCooldown = sprintCooldownReset;
        }

        if (useSprintBar && !unlimitedSprint && sprintBar != null)
        {
            float sprintPercent = sprintRemaining / sprintDuration;
            sprintBar.transform.localScale = new Vector3(sprintPercent, 1f, 1f);
            if (hideBarWhenFull && sprintBarCG != null)
                sprintBarCG.alpha = sprintRemaining == sprintDuration ? 0 : 1;
        }
    }
    #endregion

    #region Jump
    if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        Jump();
    #endregion

    #region Crouch
    if (enableCrouch)
    {
        if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
            Crouch();

        if (holdToCrouch)
        {
            if (Input.GetKeyDown(crouchKey)) { isCrouched = false; Crouch(); }
            else if (Input.GetKeyUp(crouchKey)) { isCrouched = true; Crouch(); }
        }
    }
    #endregion

    CheckGround();

    // --- Animation Handling ---
    bool hasMovementInput = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

    if (animator != null)
    {
        bool isCurrentlySlashing = animator.GetCurrentAnimatorStateInfo(0).IsName("Slashing");

        // Only set running if not slashing
        animator.SetBool("isRunning", hasMovementInput && !isCurrentlySlashing);

        if (Input.GetMouseButtonDown(0) && !isCurrentlySlashing)
        {
            animator.ResetTrigger("isSlashing");
            animator.SetTrigger("isSlashing");
        }
    }

}


    void FixedUpdate()
    {
        if (playerCanMove)
        {
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if ((targetVelocity.x != 0 || targetVelocity.z != 0) && isGrounded)
                isWalking = true;
            else
                isWalking = false;

            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0 && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;
                ApplyMovement(targetVelocity);
                isSprinting = true;
            }
            else
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;
                ApplyMovement(targetVelocity);
                isSprinting = false;
            }
        }
    }

    private void ApplyMovement(Vector3 targetVelocity)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.down * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, 0.75f))
            isGrounded = true;
        else
            isGrounded = false;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;
        }

        if (isCrouched && !holdToCrouch)
            Crouch();
    }

    private void Crouch()
    {
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }

    // Public methods for CameraStateController
    public float GetYaw() { return yaw; }
    public float GetPitch() { return pitch; }
}
