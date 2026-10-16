using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public sealed class ZombiePlayerAnimation : MonoBehaviour
{
    [Header("Small zombie sheets")]
    [SerializeField] private Texture2D downIdleSheet, downWalkSheet, downBiteSheet;
    [SerializeField] private Texture2D upIdleSheet, upWalkSheet, upBiteSheet;
    [SerializeField] private Texture2D sideIdleSheet, sideWalkSheet, sideBiteSheet;
    [Header("World sizing and playback")]
    [SerializeField, Min(1f)] private float pixelsPerUnit = 16f;
    [SerializeField, Min(1f)] private float idleFramesPerSecond = 6f;
    [SerializeField, Min(1f)] private float walkFramesPerSecond = 10f;
    [SerializeField, Min(1f)] private float biteFramesPerSecond = 12f;
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Sprite[] downIdle, downWalk, downBite, upIdle, upWalk, upBite, sideIdle, sideWalk, sideBite;
    private Vector2 facing = Vector2.down;
    private bool faceLeft, biting;
    private int frameIndex;
    private float frameClock;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        downIdle = MakeFrames(downIdleSheet, 6); downWalk = MakeFrames(downWalkSheet, 6); downBite = MakeFrames(downBiteSheet, 4);
        upIdle = MakeFrames(upIdleSheet, 6); upWalk = MakeFrames(upWalkSheet, 6); upBite = MakeFrames(upBiteSheet, 4);
        sideIdle = MakeFrames(sideIdleSheet, 6); sideWalk = MakeFrames(sideWalkSheet, 6); sideBite = MakeFrames(sideBiteSheet, 4);
        SetFrame(downIdle, 0, false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            StartCoroutine(PlayBite());
        if (biting) return;

        Vector2 velocity = body.linearVelocity;
        if (velocity.sqrMagnitude > movementThreshold * movementThreshold)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                facing = Vector2.right;
                faceLeft = velocity.x < 0f;
            }
            else facing = velocity.y > 0f ? Vector2.up : Vector2.down;
        }

        bool moving = velocity.sqrMagnitude > movementThreshold * movementThreshold;
        Sprite[] frames = GetFrames(moving);
        frameClock += Time.deltaTime * (moving ? walkFramesPerSecond : idleFramesPerSecond);
        if (frameClock >= 1f)
        {
            frameClock %= 1f;
            frameIndex = (frameIndex + 1) % frames.Length;
        }
        SetFrame(frames, frameIndex, faceLeft && facing == Vector2.right);
    }

    private IEnumerator PlayBite()
    {
        if (biting) yield break;
        biting = true;
        frameClock = 0f;
        Sprite[] frames = GetBiteFrames();
        bool flip = faceLeft && facing == Vector2.right;
        for (int i = 0; i < frames.Length; i++)
        {
            SetFrame(frames, i, flip);
            yield return new WaitForSeconds(1f / biteFramesPerSecond);
        }
        biting = false;
        frameIndex = 0;
        frameClock = 0f;
    }

    private Sprite[] GetFrames(bool moving)
    {
        if (facing == Vector2.up) return moving ? upWalk : upIdle;
        if (facing == Vector2.down) return moving ? downWalk : downIdle;
        return moving ? sideWalk : sideIdle;
    }

    private Sprite[] GetBiteFrames()
    {
        if (facing == Vector2.up) return upBite;
        if (facing == Vector2.down) return downBite;
        return sideBite;
    }

    private void SetFrame(Sprite[] frames, int index, bool flip)
    {
        if (frames == null || frames.Length == 0 || frames[index] == null) return;
        spriteRenderer.sprite = frames[index];
        spriteRenderer.flipX = flip;
    }

    private Sprite[] MakeFrames(Texture2D sheet, int count)
    {
        if (sheet == null || count < 1) return new Sprite[0];
        int frameWidth = sheet.width / count;
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            frames[i] = Sprite.Create(sheet, new Rect(i * frameWidth, 0, frameWidth, sheet.height), new Vector2(0.5f, 0f), pixelsPerUnit);
            frames[i].name = sheet.name + "_frame_" + i;
        }
        return frames;
    }
}
