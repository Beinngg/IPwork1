using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject menuRoot;      // menu root (can be this GO)
    [SerializeField] private CanvasGroup canvasGroup;  // visibility without SetActive

    [Header("Animator & State Names (NO transitions needed)")]
    [SerializeField] private Animator animator;        
    [SerializeField] private string openState  = "menu_open";   // exact state name or full path
    [SerializeField] private string closeState = "menu_closed";  // exact state name or full path

    [Header("Clip Timing (for fallback wait)")]
    [SerializeField] private AnimationClip openClip;            // assign the clip used by openState
    [SerializeField] private AnimationClip closeClip;           // assign the clip used by closeState
    [SerializeField] private float fallbackOpenLen  = 0.5f;     // if clips aren’t assigned
    [SerializeField] private float fallbackCloseLen = 0.5f;

    [Header("Gameplay pause")]
    [SerializeField] private MonoBehaviour[] disableWhilePaused; // e.g., controller/camera scripts
    [SerializeField] private bool unlockCursorWhenPaused = true;

    private bool isOpen;
    private bool busy;

    // --- Animation Event receiver flag ---
    private bool closeEventFired = false;

    void Awake()
    {
        if (!menuRoot) menuRoot = gameObject;

        if (!canvasGroup)
        {
            canvasGroup = menuRoot.GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = menuRoot.AddComponent<CanvasGroup>();
        }

        if (!animator) animator = menuRoot.GetComponent<Animator>();
    }

    void Start()
    {
        // start hidden but keep object enabled so Update can read ESC
        SetVisible(false, false);
        isOpen = false;
        busy = false;

        Time.timeScale = 1f;
        SetCursor(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (busy) return;

            if (isOpen) StartCoroutine(CloseRoutine());
            else        StartCoroutine(OpenRoutine());
        }
    }

    // -------- OPEN --------
    private IEnumerator OpenRoutine()
    {
        busy = true;
        isOpen = true;

        // pause game, but keep animator running
        Time.timeScale = 0f;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        SetVisible(true, true);
        foreach (var mb in disableWhilePaused) if (mb) mb.enabled = false;
        SetCursor(true);

        // force play from start, no transitions
        animator.speed = 1f;
        animator.Play(openState, 0, 0f);
        animator.Update(0f); // apply immediately in unscaled time

        // optional: wait the open clip length (remove if you want instant usability)
        float len = openClip ? openClip.length : fallbackOpenLen;
        yield return new WaitForSecondsRealtime(len);

        busy = false;
    }

    // -------- CLOSE --------
    private IEnumerator CloseRoutine()
    {
        busy = true;

        // keep visible but block input while closing animation plays
        SetVisible(true, false);

        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.speed = 1f;

        // arm the event flag and play from start
        closeEventFired = false;
        animator.Play(closeState, 0, 0f);
        animator.Update(0f);

        // wait until the animation event fires OR fallback to clip length
        float fallback = closeClip ? closeClip.length : fallbackCloseLen;
        float t = 0f;
        while (!closeEventFired && t < fallback)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        FinishClose();
    }

    // -------- Animation Event receiver (clip must call this on last frame) --------
    // Add an Animation Event named exactly "OnCloseAnimationComplete" on the last frame of your close clip.
    public void OnCloseAnimationComplete()
    {
        closeEventFired = true;
    }

    // -------- finalize close --------
    private void FinishClose()
    {
        SetVisible(false, false);
        Time.timeScale = 1f;

        foreach (var mb in disableWhilePaused) if (mb) mb.enabled = true;
        SetCursor(false);

        isOpen = false;
        busy = false;

        animator.updateMode = AnimatorUpdateMode.Normal;
    }

    // -------- helpers --------
    private void SetVisible(bool on, bool interactable)
    {
        canvasGroup.alpha = on ? 1f : 0f;
        canvasGroup.blocksRaycasts = on && interactable;
        canvasGroup.interactable   = on && interactable;
        // do NOT SetActive; keeping it enabled lets ESC always work.
    }

    private void SetCursor(bool show)
    {
        if (!unlockCursorWhenPaused) return;
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
