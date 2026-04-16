using UnityEngine;

public class InteractHintFlipFix : MonoBehaviour
{
    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        Transform root = transform.root;
        if (root == null) return;

        bool lookingRight = root.localScale.x < 0;

        transform.localPosition = new Vector3(
            basePosition.x,
            basePosition.y,
            basePosition.z
        );

        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector3 ls = rect.localScale;
            ls.x = lookingRight ? -1 : 1;
            rect.localScale = ls;
        }
    }
}