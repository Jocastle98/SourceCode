using UnityEngine;
using UnityEngine.UI;

public class UVScroll : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] bool texPositiveGoesRight = false;

    float lookDir = 1f;
    float uiFlip  = 1f;

    RawImage raw;
    Rect     uv;

    public void InitDirs(float lookSign, float uiFlipSign = 1f)
    {
        lookDir = Mathf.Sign(lookSign);
        uiFlip  = Mathf.Sign(uiFlipSign);
    }

    void Awake()
    {
        raw = GetComponent<RawImage>();
        if (!raw) { enabled = false; return; }
        uv = raw.uvRect;
    }

    void Update()
    {
        float baseSign = texPositiveGoesRight ? 1f : -1f;
        float final    = lookDir * uiFlip * baseSign;

        uv.x += speed * final * Time.deltaTime;
        raw.uvRect = uv;
    }
}