using UnityEngine;
using UnityEngine.UI;

public class ReticleUI : MonoBehaviour
{
    [SerializeField] private Sprite reticleSprite;
    [SerializeField] private float normalSize = 64f;
    [SerializeField] private float aimSize = 32f;
    [SerializeField] private float fireSize = 96f;
    [SerializeField] private float shrinkSpeed = 10f;

    private Image reticleImage;
    private float targetSize;

    private void Start()
    {
        Canvas canvas = new GameObject("ReticleCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        GameObject reticleObj = new GameObject("Reticle");
        reticleObj.transform.SetParent(canvas.transform, false);

        reticleImage = reticleObj.AddComponent<Image>();
        reticleImage.sprite = reticleSprite;
        reticleImage.color = Color.white;

        RectTransform rt = reticleImage.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(normalSize, normalSize);

        targetSize = normalSize;
    }

    private void Update()
    {
        if (reticleImage == null) return;

        if (PlayerController.Instance != null && PlayerController.Instance.isAiming)
        {
            targetSize = aimSize;
        }
        else if (Mathf.Abs(reticleImage.rectTransform.sizeDelta.x - fireSize) < 1f)
        {
            targetSize = normalSize;
        }
        else
        {
            targetSize = normalSize;
        }

        float current = reticleImage.rectTransform.sizeDelta.x;
        float next = Mathf.Lerp(current, targetSize, Time.deltaTime * shrinkSpeed);
        reticleImage.rectTransform.sizeDelta = new Vector2(next, next);
    }

    public void OnFire()
    {
        if (reticleImage != null)
        {
            reticleImage.rectTransform.sizeDelta = new Vector2(fireSize, fireSize);
            targetSize = normalSize;
        }
    }
}