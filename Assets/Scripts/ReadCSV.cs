using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class ReadCSV : MonoBehaviour
{
    [SerializeField]
    public TextAsset assignmentDataset; // Input csv file
    public static string[] columnNames = null; // Stores the name of each column from csv file
    public static List<GameObject> allDatapoints = null; // All the GameObjects that store a DataPoint component, in a List
    public static Dictionary<string,List<GameObject>> datapointsByID = null; // Easy to get all DataPoints with same wappierID
    public static Dictionary<string,ArrayList> IDsByCountry = null; // Easy to get all WappierIDs (as ArrayList) in each Country
    public static String[] countryNames = null; // The names of the countries (no duplicates)
    public static float revenueMax = float.NegativeInfinity;

    /*
     * Opens csv file, reads each line and saves each line
     * as a datapoint with the specified members defined by
     * this project (in DataPoint.cs).
     */
    void Start()
    {
        SaveDatapointsFromInputFile();
        SaveColumnNames();
    }

    private void SaveColumnNames()
    {
        string first_line = assignmentDataset.text.Trim().Split('\n')[0];
        columnNames = first_line.Trim().Split(';'); // The names of the columns are in the first line
    }

    private void SaveDatapointsFromInputFile()
    {
        string dataset_content = assignmentDataset.text;
        var content_split_nl = new List<string>(dataset_content.Trim().Split('\n'));
        content_split_nl =  content_split_nl.Distinct().ToList(); // There could be duplicates in the csv, we don't want that

        int num_lines = content_split_nl.Count;

        allDatapoints = new List<GameObject>(); // Initialize list
        datapointsByID = new Dictionary<string, List<GameObject>>();
        IDsByCountry = new Dictionary<string, ArrayList>();
        var countryNamesSet = new HashSet<string>();

        for (int i = 1; i < num_lines; i++)
        {
            GameObject datapoint_gameobject = DataPoint.StrToGameObjectDataPoint(content_split_nl[i]);
            var datapoint_i = datapoint_gameobject.GetComponent<DataPoint>();
            if (revenueMax < datapoint_i.revenue)
            {
                revenueMax = datapoint_i.revenue;
            }
            allDatapoints.Add(datapoint_gameobject);

            if (!datapointsByID.ContainsKey(datapoint_i.wappierID))
            {
                datapointsByID[datapoint_i.wappierID] = new List<GameObject>();
            }
            datapointsByID[datapoint_i.wappierID].Add(datapoint_gameobject);

            if (!IDsByCountry.ContainsKey(datapoint_i.country))
            {
                IDsByCountry[datapoint_i.country] = new ArrayList();
            }
            IDsByCountry[datapoint_i.country].Add(datapoint_i.wappierID);
            countryNamesSet.Add(datapoint_gameobject.GetComponent<DataPoint>().country);
        }
        countryNames = new String[countryNamesSet.Count];
        countryNamesSet.CopyTo(countryNames);
    }

    public string[] GetColumnNames()
    {
        return columnNames;
    }
}
