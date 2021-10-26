using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    GameObject selectedObject = null; // GameObject that has been clicked and it shows a different Outline
    GameObject hoverObject = null; // GameObject that the mouse is currently hovering over
    RaycastHit hitInfo; // This is detecting if the mouse is on an Object's Collider

    [SerializeField]
    CanvasManager canvasManager = null; // Easy access to the Canvas

    GameObject lineOfSameID = null; // Using just 1 GameObject for the line that shows up when selecting a Datapoint

    float unselectedAlpha = 0.1f; // Alpha of other objects when one is selected
    Material materialDefaultSelected = null; // Storing the material of the selected object, before it becomes selected
    Material materialOpaque; // Set the selected object's material to this one temporarily while it's selected

    float factorScaleOnSelect = 1.6f; // Multiply the selected object's scale to this factor while it's selected

    void Start()
    {
        materialOpaque = new Material(Shader.Find("Standard"));
    }

    // Handles what happens when clicking/selecting an object or not
    void Update()
    {
        /*
         * hoverObject :    The last object the mouse was hovering
         * selectedObject : The last object that has been selected
         * hitObject :      The object the mouse is on, in this frame
         */
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        GameObject hitObject = null;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            hitObject = hitInfo.transform.gameObject;
        }

        if (hitObject)
        {
            SetObjectOutline(ref hitObject, 0);

            if (hoverObject != null)
            {
                if (hoverObject == selectedObject && hitObject != selectedObject)
                {
                    SetObjectOutline(ref hoverObject, 1); // Case: mouse was hovering a selected object, and now isnt
                }
                else if (hoverObject != hitObject)
                {
                    RemoveObjectOutline(ref hoverObject); // Case: mouse was hovering an object (that wasn't selected), now it's hovering another one
                }
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                // With this, the canvas is always looking towards the camera
                hitObject.transform.rotation = Camera.main.transform.rotation;

                // Case where we click on a sphere while another is still selected
                if (selectedObject != null)
                {
                    SetObjectOutline(ref selectedObject, 0);
                    RemoveObjectOutline(ref selectedObject);

                    // Reset the material of all the objects the were opaque (the ones with same id as selectedObject)
                    SetSHMaterialByID(selectedObject.GetComponent<DataPoint>().wappierID, materialDefaultSelected);
                    materialDefaultSelected = null;

                    selectedObject.transform.localScale /= factorScaleOnSelect;
                }

                SetObjectOutline(ref hitObject, 1);
                selectedObject = hitObject;

                // Setting the materials to seem more transparent
                setAlphaAllClusterMaterials(unselectedAlpha);

                // Temporary opaque material for the selected sphere
                selectedObject.transform.localScale *= factorScaleOnSelect;
                materialOpaque.color = selectedObject.GetComponent<MeshRenderer>().sharedMaterial.color;
                materialDefaultSelected = selectedObject.GetComponent<MeshRenderer>().sharedMaterial; // Storing default material to reset after it's not selected

                // All objects with the same ID show up opaque when one of them is selected
                SetSHMaterialByID(selectedObject.GetComponent<DataPoint>().wappierID, materialOpaque);

                CameraControlFromMouse.MoveTowards(selectedObject.transform.position);
                DrawLineSameID(ref selectedObject);

                canvasManager.Enable();
                canvasManager.SetParent(selectedObject.transform);
                var dp = selectedObject.GetComponent<DataPoint>();
                canvasManager.SetText(ref dp);
            }

            hoverObject = hitObject;
        }
        else // If ray did NOT hit an Object
        {
            if (hoverObject != null)
            {
                if (hoverObject == selectedObject)
                {
                    SetObjectOutline(ref hoverObject, 1); // Case: Was hovering the selected and now isn't
                }
                else
                {
                    RemoveObjectOutline(ref hoverObject); // Case: Was hovering a sphere and now isn't
                }
            }

            hoverObject = null;

            if (Input.GetMouseButtonDown(0) && selectedObject != null)
            {
                SetObjectOutline(ref selectedObject, 0);
                RemoveObjectOutline(ref selectedObject);
                selectedObject.transform.localScale /= factorScaleOnSelect;

                // Reset material of selected (and those with same id) to its default
                var id = selectedObject.GetComponent<DataPoint>().wappierID;
                SetSHMaterialByID(id, materialDefaultSelected);
                materialDefaultSelected = null;

                // Setting alpha of color of materials back to 1 (fully opaque)
                setAlphaAllClusterMaterials(1f);

                selectedObject = null;

                CameraControlFromMouse.MoveTowards(CameraControlFromMouse.defaultPosition);
                RemoveLineSameID();
                var panel = FindObjectOfType<CanvasManager>();
                panel.Disable();
            }
        }
    }

    void RemoveObjectOutline(ref GameObject go)
    {
        if (go != null)
        {
            go.GetComponent<cakeslice.Outline>().enabled = false;
            go.GetComponent<cakeslice.Outline>().color = 0;
        }
    }
    void SetObjectOutline(ref GameObject go, int colorIndex)
    {
        if (go != null)
        {
            go.GetComponent<cakeslice.Outline>().enabled = true;
            go.GetComponent<cakeslice.Outline>().color = colorIndex;
        }
    }

    void DrawLineSameID(ref GameObject datapointGameobject)
    {
        if (lineOfSameID != null)
        {
            Destroy(lineOfSameID);
            lineOfSameID = null;
        }

        var id = datapointGameobject.GetComponent<DataPoint>().wappierID;
        var dataList = ReadCSV.datapointsByID[id];
        if (dataList.Count == 1)
        {
            return; // If it's only 1 sphere, no need to draw any lines
        }

        lineOfSameID = new GameObject("Line through same-ID-datapoints");
        lineOfSameID.transform.parent = Plotter.root.transform;

        var lineRenderer = lineOfSameID.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        var lineColor = Color.HSVToRGB(52f / 360f, 1f, 1f);
        lineColor.a = 0.1f;
        lineRenderer.material.color = lineColor;
        lineRenderer.numCornerVertices = 8;
        lineRenderer.numCapVertices = 14;
        lineRenderer.widthMultiplier = 0.3f;
        lineRenderer.positionCount = dataList.Count;
        for (int i = 0; i < dataList.Count; i++)
        {
            lineRenderer.SetPosition(i, dataList[i].transform.localPosition);
        }
    }
    void RemoveLineSameID()
    {
        if (lineOfSameID)
        {
            Destroy(lineOfSameID);
        }
    }

    void SetSHMaterialByID(string id, Material mat)
    {
        var dataList = ReadCSV.datapointsByID[id];
        foreach (var sameIDObjects in dataList)
        {
            sameIDObjects.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }

    void setAlphaAllClusterMaterials(float alpha)
    {
        foreach (var material in Plotter.materialPerCluster) // Set alpha of color of material back to 1
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }
}
