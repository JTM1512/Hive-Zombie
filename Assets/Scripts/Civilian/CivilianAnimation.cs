using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class CivilianAnimation : MonoBehaviour
{
    // Direction order: down, up, right, left.
    [Header("Post-apocalyptic main character sheets")]
    [SerializeField] private Texture2D[] idleSheets = new Texture2D[4];
    [SerializeField] private Texture2D[] runSheets = new Texture2D[4];
    [SerializeField, Min(1)] private int frameCount = 6;
    [SerializeField, Min(1f)] private float pixelsPerUnit = 16f;
    [SerializeField, Min(1f)] private float idleFramesPerSecond = 6f;
    [SerializeField, Min(1f)] private float runFramesPerSecond = 10f;
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;
    [SerializeField] private string characterSortingLayer = "Default";

    private SpriteRenderer spriteRenderer;
    private Sprite[][] idleFrames;
    private Sprite[][] runFrames;
    private Vector2 previousPosition;
    private int facingIndex;
    private int frameIndex;
    private float frameClock;
    private bool framesCreated;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyCharacterSorting();
        BuildFrames();
        previousPosition = transform.position;
        ShowFrame(idleFrames, 0);
    }

    private void OnEnable()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyCharacterSorting();
        BuildFrames();
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Civilian civilian = GetComponent<Civilian>();
        if (civilian != null && civilian.HasTurned)
        {
            enabled = false;
            return;
        }

        Vector2 position = transform.position;
        Vector2 movement = (position - previousPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousPosition = position;
        bool moving = movement.sqrMagnitude > movementThreshold * movementThreshold;
        if (moving)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                facingIndex = movement.x < 0f ? 3 : 2;
            else
                facingIndex = movement.y > 0f ? 1 : 0;
        }

        Sprite[] frames = moving ? runFrames[facingIndex] : idleFrames[facingIndex];
        if (frames == null || frames.Length == 0) return;

        frameClock += Time.deltaTime * (moving ? runFramesPerSecond : idleFramesPerSecond);
        while (frameClock >= 1f)
        {
            frameClock -= 1f;
            frameIndex = (frameIndex + 1) % frames.Length;
        }
        spriteRenderer.sprite = frames[frameIndex];
    }

    private void ApplyCharacterSorting()
    {
        if (spriteRenderer == null) return;
        if (SortingLayer.NameToID(characterSortingLayer) != 0)
            spriteRenderer.sortingLayerName = characterSortingLayer;
        spriteRenderer.sortingOrder = short.MaxValue;
    }

    private void BuildFrames()
    {
        if (framesCreated) return;
        idleFrames = MakeDirectionalFrames(idleSheets);
        runFrames = MakeDirectionalFrames(runSheets);
        framesCreated = true;
    }

    private Sprite[][] MakeDirectionalFrames(Texture2D[] sheets)
    {
        var result = new Sprite[4][];
        for (int direction = 0; direction < result.Length; direction++)
        {
            Texture2D sheet = sheets != null && direction < sheets.Length ? sheets[direction] : null;
            if (sheet == null || frameCount < 1 || sheet.width < frameCount) continue;

            int width = sheet.width / frameCount;
            result[direction] = new Sprite[frameCount];
            for (int frame = 0; frame < frameCount; frame++)
            {
                result[direction][frame] = Sprite.Create(sheet,
                    new Rect(frame * width, 0, width, sheet.height), new Vector2(0.5f, 0f), pixelsPerUnit);
            }
        }
        return result;
    }

    private void ShowFrame(Sprite[][] animations, int direction)
    {
        if (animations != null && animations[direction] != null && animations[direction].Length > 0)
            spriteRenderer.sprite = animations[direction][0];
    }
}
