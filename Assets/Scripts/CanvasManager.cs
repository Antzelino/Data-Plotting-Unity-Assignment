using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CanvasManager : MonoBehaviour
{
    public TextMeshProUGUI textID;
    public TextMeshProUGUI textRevenue;
    public TextMeshProUGUI textCountry;
    public TextMeshProUGUI textDay;
    public TextMeshProUGUI textDateReg;
    public TextMeshProUGUI textSub;

    float scaleFactor = 0.1f;
    float offsetFactor;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.enabled)
        {
            Transform t = gameObject.transform;
            t.parent.rotation = Camera.main.transform.rotation;
            var distance = (CameraControlFromMouse.targetPosition - Camera.main.transform.position).magnitude;

            // Setting size to always stay the same on screen, and the offset always the same world-distance from sphere
            t.localScale = Vector3.one * distance * scaleFactor;
            offsetFactor = Mathf.Atan(FindObjectOfType<CameraControlFromMouse>().currentScaleAsFactor());
            var lerpmax = Mathf.Atan(CameraControlFromMouse.scaleMax-CameraControlFromMouse.scaleMin);
            offsetFactor = Mathf.Lerp(0f, lerpmax, offsetFactor);
            t.localPosition = new Vector3(0.4f , 0.4f, 0f) + new Vector3(0.3f,0.3f,0f) * offsetFactor;
        }
    }

    public void SetParent(Transform newParent)
    {
        gameObject.transform.parent = newParent;
        SetPosition(newParent.position);
    }

    public void SetPosition(Vector3 newPosition)
    {
        gameObject.transform.position = newPosition;
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void SetText(ref DataPoint point)
    {
        textID.text = point.wappierID;
        textRevenue.text = point.revenue.ToString("0.00");
        textCountry.text = point.country;
        textDay.text = point.day.ToString();
        textDateReg.text = point.dateRegistered.ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("es-ES")); // DD-MM-YYYY format

        if (point.subscriptionStatus)
        {
            textSub.text = "Yes";
        }
        else
        {
            textSub.text = "No";
        }
    }
}