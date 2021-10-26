using System;
using UnityEngine;

public class CameraControlFromMouse : MonoBehaviour
{
    [SerializeField]
    public Transform defaultCenterPoint; // The transform of the plot. Camera starts "orbiting" around it's position.
    public static Transform cameraFollowingCenterPoint; // The transform the camera is "currently" plotting. This is the one that changes over time.

    public static float sens = 1.9f; // Sensitivity. Both for zoom in/out and rotating.

    public static Vector3 defaultPosition;
    public static Quaternion defaultRotation;
    public static Vector3 defaultScale;

    private static float sensMin = 0.1f;
    private static float minRotationOnX = 0f;
    private static float maxRotationOnX = 89.99f;
    public static float scaleMin = 2f;
    public static float scaleMax = 128f;

    static public Vector3 targetPosition;
    public float translationFactor = 50f;

    // Start is called before the first frame update
    void Start()
    {
        cameraFollowingCenterPoint = defaultCenterPoint;
        defaultPosition = cameraFollowingCenterPoint.localPosition;
        defaultRotation = cameraFollowingCenterPoint.localRotation;
        defaultScale = cameraFollowingCenterPoint.localScale;
        targetPosition = cameraFollowingCenterPoint.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        DoRotation();

        DoZoom();

        DoSensChange();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // In the proccess of moving center position
        if (targetPosition != cameraFollowingCenterPoint.localPosition)
        {
            Vector3 translationVector = (targetPosition - cameraFollowingCenterPoint.localPosition).normalized * translationFactor * Time.deltaTime;
            float r0 = (cameraFollowingCenterPoint.localPosition - Camera.main.transform.position).magnitude;
            float r1 = (cameraFollowingCenterPoint.localPosition + translationVector - Camera.main.transform.position).magnitude;

            if (translationVector.magnitude > (targetPosition - cameraFollowingCenterPoint.localPosition).magnitude)
            {
                cameraFollowingCenterPoint.localPosition = targetPosition;
                cameraFollowingCenterPoint.localScale *= r1 / (targetPosition - Camera.main.transform.position).magnitude;
            }
            else
            {
                cameraFollowingCenterPoint.localPosition += translationVector;
                cameraFollowingCenterPoint.localScale *= r1 / r0;
            }

            if (cameraFollowingCenterPoint.localScale.x < scaleMin)
            {
                cameraFollowingCenterPoint.localScale = Vector3.one * scaleMin;
            }
            else if (cameraFollowingCenterPoint.localScale.x > scaleMax)
            {
                cameraFollowingCenterPoint.localScale = Vector3.one * scaleMax;
            }

        }
    }

    private void DoSensChange()
    {
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) // Press + to increase sensitivity
        {
            sens += 0.1f;
        }
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) // Press - to lower sensitivity
        {
            sens -= 0.1f;
            if (sens <= sensMin)
            {
                sens = sensMin;
            }
        }
    }

    private void DoZoom()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            Vector3 delta_scale = Vector3.one * Input.mouseScrollDelta.y * sens;
            cameraFollowingCenterPoint.localScale -= delta_scale;

            if (cameraFollowingCenterPoint.localScale.y < scaleMin)
            {
                cameraFollowingCenterPoint.localScale += Vector3.one * scaleMin - cameraFollowingCenterPoint.localScale;
            }
            else if (cameraFollowingCenterPoint.localScale.y > scaleMax)
            {
                cameraFollowingCenterPoint.localScale += Vector3.one * scaleMax - cameraFollowingCenterPoint.localScale;
            }
        }
    }

    private void DoRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float angles_on_y = Input.GetAxis("Mouse X");
            float angles_on_x = -Input.GetAxis("Mouse Y");
            float eulerX = cameraFollowingCenterPoint.localRotation.eulerAngles.x;

            if (eulerX + angles_on_x * sens < minRotationOnX) // lower bound rotation
            {
                angles_on_x = (minRotationOnX - eulerX) / sens;
            }
            else if (eulerX + angles_on_x * sens > maxRotationOnX) // upper bound rotation
            {
                angles_on_x = (maxRotationOnX - eulerX) / sens;
            }

            cameraFollowingCenterPoint.Rotate(Vector3.right, angles_on_x * sens, Space.Self);
            cameraFollowingCenterPoint.Rotate(Vector3.up, angles_on_y * sens, Space.World);

            var euler_angles = cameraFollowingCenterPoint.transform.rotation.eulerAngles;
            if (euler_angles.z != 0f)
            {
                cameraFollowingCenterPoint.transform.rotation = Quaternion.Euler(euler_angles.x, euler_angles.y, 0f); // lock Z axis rotation to 0
            }
        }
    }

    public void SetToDefaults()
    {
        sens = 1.5f;
        cameraFollowingCenterPoint.localPosition = defaultPosition;
        cameraFollowingCenterPoint.localRotation = defaultRotation;
        cameraFollowingCenterPoint.localScale = defaultScale;
    }

    public static void MoveTowards(Vector3 position)
    {
        targetPosition = position;
    }

    public float currentScaleAsFactor()
    {
        return (cameraFollowingCenterPoint.localScale.x-scaleMin) / (scaleMax-scaleMin);
    }
}
