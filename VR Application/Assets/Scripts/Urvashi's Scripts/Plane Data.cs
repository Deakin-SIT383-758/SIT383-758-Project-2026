using System;
using UnityEngine;

public enum PlaneStatus { Normal, Emergency, CommFailure, Hijack }

[Serializable]
public class PlaneData
{
    #region Identity
    public string hex;
    public string flight;
    #endregion

    #region Position
    public float lat;
    public float lon;
    public float ASL;
    public Vector3 mapPosition;
    #endregion

    #region Movement
    public float gs;
    public float track;
    public float baroRate;
    /// <summary>Visual turn rate in degrees per real second (path curvature × ground speed).
    /// Drives the bank target in PlaneNoseRotator.</summary>
    public float turnRateDegPerSec;
    #endregion

    #region Status
    public string squawk;
    public PlaneStatus status;
    #endregion

    #region Timing
    public DateTimeOffset lastSeen;
    public float seenPos;
    #endregion

    public string VerticalTrend =>
        baroRate > 100f  ? "upwards" :
        baroRate < -100f ? "downwards" : "level";

    public bool IsStale(float thresholdSeconds = 30f) => seenPos > thresholdSeconds;

    public static PlaneStatus StatusFromSquawk(string squawk, string emergency) =>
        squawk switch
        {
            "7700" => PlaneStatus.Emergency,
            "7600" => PlaneStatus.CommFailure,
            "7500" => PlaneStatus.Hijack,
            _ => emergency != null && emergency != "none"
                ? PlaneStatus.Emergency
                : PlaneStatus.Normal,
        };

    public static PlaneData FromJson(long now, PlaneDataJson json)
    {
        return new PlaneData
        {
            hex        = json.hex,
            flight     = json.flight?.Trim() ?? json.hex,
            lat        = json.lat,
            lon        = json.lon,
            ASL        = json.alt_baro_value,
            gs         = json.gs,
            track      = json.track,
            baroRate   = json.baro_rate,
            squawk     = json.squawk ?? "0000",
            status     = StatusFromSquawk(json.squawk, json.emergency),
            lastSeen   = DateTimeOffset.FromUnixTimeSeconds(now - (long)json.seen),
            seenPos    = json.seen_pos,
        };
    }
}

[Serializable]
public class PlaneDataJson
{
    public string hex;
    public string flight;
    public float lat;
    public float lon;
    public float alt_baro_value;
    public float gs;
    public float track;
    public float baro_rate;
    public float seen_pos;
    public float seen;
    public string squawk;
    public string emergency;
}

