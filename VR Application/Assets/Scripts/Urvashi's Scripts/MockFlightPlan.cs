using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MockFlightPlan", menuName = "FlightOps/Mock Flight Plan")]
public class MockFlightPlan : ScriptableObject
{
    public string callsign;
    public string aircraftType;

    [Tooltip("Ordered world-space positions on the 3D map")]
    public List<Vector3> waypoints = new();

    [Tooltip("Cruise speed in knots")]
    public float cruiseSpeed = 145f;

    [Tooltip("Cruise altitude in feet ASL")]
    public float cruiseAltitude = 8500f;
}