using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public sealed class ZombiePlayerAnimation : MonoBehaviour
{
    // Direction order is down, up, right, left.
    [Header("Directional idle and movement sheets")]
    [SerializeField] private Texture2D[] idleSheets = new Texture2D[4];
    [SerializeField] private Texture2D[] walkSheets = new Texture2D[4];
    [Header("Directional attacks")]
    [SerializeField] private Texture2D[] firstAttackSheets = new Texture2D[4];
    [SerializeField] private Texture2D[] secondAttackSheets = new Texture2D[4];
    [Header("Side death sheets (the pack only includes side-facing deaths)")]
    [SerializeField] private Texture2D[] firstDeathSheets = new Texture2D[4];
    [SerializeField] private Texture2D[] secondDeathSheets = new Texture2D[4];
    [Header("Frame counts")]
    [SerializeField, Min(1)] private int idleFrameCount = 6;
    [SerializeField, Min(1)] private int walkFrameCount = 6;
    [SerializeField, Min(1)] private int firstAttackFrameCount = 4;
    [SerializeField, Min(1)] private int secondAttackFrameCount = 11;
    [SerializeField, Min(1)] private int firstDeathFrameCount = 6;
    [SerializeField, Min(1)] private int secondDeathFrameCount = 7;
    [Header("Playback")]
    [SerializeField, Min(1f)] private float pixelsPerUnit = 16f;
    [SerializeField, Min(1f)] private float idleFramesPerSecond = 6f;
    [SerializeField, Min(1f)] private float walkFramesPerSecond = 10f;
    [SerializeField, Min(1f)] private float attackFramesPerSecond = 12f;
    [SerializeField, Min(1f)] private float deathFramesPerSecond = 10f;
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;
    [SerializeField] private bool listenForAttackInput = true;
    [SerializeField] private bool useSecondAttack = true;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private ZombieHealth health;
    private Sprite[][] idleFrames, walkFrames, firstAttackFrames, secondAttackFrames;
    private Sprite[][] firstDeathFrames, secondDeathFrames;
    private int facingIndex;
    private int frameIndex;
    private int attackCount, deathCount;
    private float frameClock;
    private bool framesCreated, playingAction, dead;

    private void Awake()
    {
        if (!listenForAttackInput) return;
        CacheComponents();
        BuildFrames();
        ShowIdleFrame();
    }

    private void OnEnable()
    {
        CacheComponents();
        BuildFrames();
        if (!dead) ShowIdleFrame();
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<ZombieHealth>();
    }

    private void BuildFrames()
    {
        if (framesCreated) return;
        idleFrames = MakeDirectionalFrames(idleSheets, idleFrameCount);
        walkFrames = MakeDirectionalFrames(walkSheets, walkFrameCount);
        firstAttackFrames = MakeDirectionalFrames(firstAttackSheets, firstAttackFrameCount);
        secondAttackFrames = MakeDirectionalFrames(secondAttackSheets, secondAttackFrameCount);
        firstDeathFrames = MakeDirectionalFrames(firstDeathSheets, firstDeathFrameCount);
        secondDeathFrames = MakeDirectionalFrames(secondDeathSheets, secondDeathFrameCount);
        framesCreated = true;
    }

    private void Update()
    {
        CacheComponents();
        BuildFrames();
        if (dead) return;

        if (listenForAttackInput && health != null && health.currentHealth <= 0f)
        {
            PlayDeath();
            return;
        }
        bool attackPressed = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
        attackPressed |= Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (listenForAttackInput && attackPressed) PlayAttack();
        if (playingAction) return;

        Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
        bool moving = velocity.sqrMagnitude > movementThreshold * movementThreshold;
        if (moving)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                facingIndex = velocity.x < 0f ? 3 : 2;
            else
                facingIndex = velocity.y > 0f ? 1 : 0;
        }

        bool flipX;
        Sprite[] frames = GetDirectionalFrames(moving ? walkFrames : idleFrames, facingIndex, out flipX);
        if (frames == null || frames.Length == 0) return;

        frameClock += Time.deltaTime * (moving ? walkFramesPerSecond : idleFramesPerSecond);
        if (frameClock >= 1f)
        {
            frameClock %= 1f;
            frameIndex = (frameIndex + 1) % frames.Length;
        }
        SetFrame(frames, frameIndex, flipX);
    }

    /// <summary>Starts the small-zombie animation after a civilian turns.</summary>
    public void StartZombieAnimations()
    {
        enabled = true;
        CacheComponents();
        BuildFrames();
        dead = false;
        playingAction = false;
        facingIndex = 0;
        frameIndex = 0;
        frameClock = 0f;
        ShowIdleFrame();
    }

    /// <summary>Plays the first and second attack sheets in alternation.</summary>
    public float PlayAttack()
    {
        if (dead || playingAction) return 0f;
        BuildFrames();
        int sequence = useSecondAttack ? attackCount++ % 2 : 0;
        Sprite[][] chosen = sequence == 0 ? firstAttackFrames : secondAttackFrames;
        if (useSecondAttack && !HasAnyFrames(chosen)) chosen = sequence == 0 ? secondAttackFrames : firstAttackFrames;
        bool flipX;
        Sprite[] frames = GetDirectionalFrames(chosen, facingIndex, out flipX);
        return StartOneShot(frames, attackFramesPerSecond, flipX, false);
    }

    /// <summary>Plays the available side-facing death sheet; callers may deactivate after the returned duration.</summary>
    public float PlayDeath()
    {
        if (dead) return GetDeathDuration();
        BuildFrames();
        dead = true;
        int sequence = deathCount++ % 2;
        Sprite[][] chosen = sequence == 0 ? firstDeathFrames : secondDeathFrames;
        if (!HasAnyFrames(chosen)) chosen = sequence == 0 ? secondDeathFrames : firstDeathFrames;
        bool flipX;
        Sprite[] frames = GetDirectionalFrames(chosen, facingIndex, out flipX);
        float duration = StartOneShot(frames, deathFramesPerSecond, flipX, true);
        return duration > 0f ? duration : GetDeathDuration();
    }

    public float GetDeathDuration()
    {
        return Mathf.Max(firstDeathFrameCount, secondDeathFrameCount) / Mathf.Max(1f, deathFramesPerSecond);
    }

    private float StartOneShot(Sprite[] frames, float fps, bool flipX, bool stayAtEnd)
    {
        if (frames == null || frames.Length == 0)
        {
            if (stayAtEnd) dead = false;
            return 0f;
        }
        playingAction = true;
        frameClock = 0f;
        StartCoroutine(PlayOneShot(frames, fps, flipX, stayAtEnd));
        return frames.Length / Mathf.Max(1f, fps);
    }

    private IEnumerator PlayOneShot(Sprite[] frames, float fps, bool flipX, bool stayAtEnd)
    {
        float frameDuration = 1f / Mathf.Max(1f, fps);
        for (int i = 0; i < frames.Length; i++)
        {
            SetFrame(frames, i, flipX);
            yield return new WaitForSeconds(frameDuration);
        }
        if (stayAtEnd) SetFrame(frames, frames.Length - 1, flipX);
        playingAction = false;
        frameClock = 0f;
        frameIndex = 0;
    }

    private void ShowIdleFrame()
    {
        if (spriteRenderer == null) return;
        bool flipX;
        Sprite[] frames = GetDirectionalFrames(idleFrames, facingIndex, out flipX);
        if (frames != null && frames.Length > 0) SetFrame(frames, 0, flipX);
    }

    private Sprite[][] MakeDirectionalFrames(Texture2D[] sheets, int count)
    {
        var result = new Sprite[4][];
        for (int i = 0; i < result.Length; i++)
        {
            Texture2D sheet = sheets != null && i < sheets.Length ? sheets[i] : null;
            result[i] = MakeFrames(sheet, count);
        }
        return result;
    }

    private Sprite[] MakeFrames(Texture2D sheet, int count)
    {
        if (sheet == null || count < 1) return null;
        int frameWidth = sheet.width / count;
        if (frameWidth < 1) return null;
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            frames[i] = Sprite.Create(sheet, new Rect(i * frameWidth, 0, frameWidth, sheet.height),
                new Vector2(0.5f, 0f), pixelsPerUnit);
            frames[i].name = sheet.name + "_frame_" + i;
        }
        return frames;
    }

    private Sprite[] GetDirectionalFrames(Sprite[][] set, int direction, out bool flipX)
    {
        flipX = false;
        if (set == null) return null;
        direction = Mathf.Clamp(direction, 0, 3);
        if (HasFrames(set, direction)) return set[direction];
        if (direction == 3 && HasFrames(set, 2))
        {
            flipX = true;
            return set[2];
        }
        if (direction < 2 && HasFrames(set, 2)) return set[2];
        for (int i = 0; i < set.Length; i++)
            if (HasFrames(set, i)) return set[i];
        return null;
    }

    private bool HasAnyFrames(Sprite[][] set)
    {
        if (set == null) return false;
        for (int i = 0; i < set.Length; i++)
            if (HasFrames(set, i)) return true;
        return false;
    }

    private bool HasFrames(Sprite[][] set, int index)
    {
        return set != null && index >= 0 && index < set.Length && set[index] != null && set[index].Length > 0;
    }

    private void SetFrame(Sprite[] frames, int index, bool flipX)
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0) return;
        index = Mathf.Clamp(index, 0, frames.Length - 1);
        if (frames[index] == null) return;
        spriteRenderer.sprite = frames[index];
        spriteRenderer.flipX = flipX;
    }
}

