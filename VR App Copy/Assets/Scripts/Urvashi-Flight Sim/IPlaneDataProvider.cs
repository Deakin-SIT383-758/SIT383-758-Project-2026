using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPlaneDataProvider
{
    IReadOnlyDictionary<string, PlaneData> ActivePlanes { get; }

    event Action<PlaneData> OnPlaneUpdated;
    event Action<string> OnPlaneRemoved;

    void Tick(float deltaTime);


    // Returns the ordered route (waypoints) for the given plane,
    // or null/empty if no route is known for it.
    Vector3[] GetRoute(string hex);
}