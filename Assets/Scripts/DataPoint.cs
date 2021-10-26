using System; // struct DateTime, class Convert
using UnityEngine;

/*
 * A DataPoint stores all the data that exists
 * in a row of the input file, publicly.
 */
public class DataPoint : MonoBehaviour
{
    public string wappierID;
    public float revenue;
    public string country;
    public int day;
    public DateTime dateRegistered;
    public bool subscriptionStatus;

    /*
     * Function for easy use from outside, to get
     * a DataPoint straight from the input line
     * of a csv file. Default separator is ';'.
     */
    static public GameObject StrToGameObjectDataPoint(string line, char separator = ';')
    {
        string[] split_line = line.Trim().Split(separator);

        string wappier_id = split_line[0];
        float revenue;
        try
        {
            revenue = float.Parse(split_line[1].Replace(",", "."));
        }
        catch (FormatException)
        {
            Debug.LogError(split_line[1] + "not in correct format for parsing float.");
            throw;
        }
        string country = split_line[2];
        if (Plotter.countryCodeToCountry.ContainsKey(country))
        {
            country = Plotter.countryCodeToCountry[country];
        }
        int day = int.Parse(split_line[3]);
        DateTime date_registered;
        date_registered = Convert.ToDateTime(split_line[4]);
        bool subscription_status = (split_line[5] == "subscribed");

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.AddComponent<DataPoint>().SetMembers(wappier_id, revenue, country, day, date_registered, subscription_status);
        return go;
    }

    public void SetMembers(string wappierID, float revenue, string country, int day, DateTime dateRegistered, bool subscriptionStatus)
    {
        this.wappierID = wappierID;
        this.revenue = revenue;
        this.country = country;
        this.day = day;
        this.dateRegistered = dateRegistered;
        this.subscriptionStatus = subscriptionStatus;
    }
}
