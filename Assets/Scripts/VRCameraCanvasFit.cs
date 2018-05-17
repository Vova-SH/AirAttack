using UnityEngine;
using UnityEngine.VR;

[ExecuteInEditMode]
public class VRCameraCanvasFit : MonoBehaviour
{
    [SerializeField] private Camera _targetCamera;

    [SerializeField] private float _distance = 1f;

    [SerializeField] private float _scaleFactor = 0.8f;

    private RectTransform _rectTransform;

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_targetCamera) return;
        SetSize();
        SetPositionAndRotation();
        CalculateScale();
    }

    private void CalculateScale()
    {
        Vector3[] frustumCorners = new Vector3[4];
        _targetCamera.CalculateFrustumCorners(_targetCamera.rect, _distance, Camera.MonoOrStereoscopicEye.Mono,
            frustumCorners);

        Rect rect = new Rect(Vector2.zero, AbsVector2(frustumCorners[0]) * 2f);


        Vector3[] rectCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(rectCorners);
        rectCorners[0] = _targetCamera.transform.InverseTransformPoint(rectCorners[0]);
        Rect rect2 = new Rect(Vector2.zero, AbsVector2(rectCorners[0]) * 2f);
        _rectTransform.localScale *= (rect.width / rect2.width) * _scaleFactor;
    }

    private void SetPositionAndRotation()
    {
        Transform cameraTransform = _targetCamera.transform;
        _rectTransform.position = cameraTransform.position + cameraTransform.forward.normalized * _distance;
        //_rectTransform.rotation = cameraTransform.rotation;
    }

    private void SetSize()
    {
        SetSize(UnityEngine.XR.XRSettings.isDeviceActive
            ? new Vector2(UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight)
            : new Vector2(Screen.width, Screen.height));
    }

    private void SetSize(Vector2 newSize)
    {
        Vector2 oldSize = _rectTransform.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        _rectTransform.offsetMin = _rectTransform.offsetMin -
                                   new Vector2(deltaSize.x * _rectTransform.pivot.x,
                                       deltaSize.y * _rectTransform.pivot.y);
        _rectTransform.offsetMax = _rectTransform.offsetMax + new Vector2(deltaSize.x * (1f - _rectTransform.pivot.x),
                                       deltaSize.y * (1f - _rectTransform.pivot.y));
    }

    private static Vector2 AbsVector2(Vector2 target)
    {
        return new Vector2(Mathf.Abs(target.x), Mathf.Abs(target.y));
    }
}
