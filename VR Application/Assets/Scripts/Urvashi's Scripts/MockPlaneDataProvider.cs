using System;
using System.Collections.Generic;
using OasXr.Flight;
using UnityEngine;

[Serializable]
public class Airport
{
    public string name = "Airport";

    [Tooltip("One end of the runway. Y is ignored.")]
    public Vector3 runwayEndA;

    [Tooltip("Opposite end of the runway. Y is ignored.")]
    public Vector3 runwayEndB;

    public Vector3 Center =>
        new Vector3(
            (runwayEndA.x + runwayEndB.x) * 0.5f,
            0f,
            (runwayEndA.z + runwayEndB.z) * 0.5f);
}

public class MockPlaneDataProvider : MonoBehaviour, IPlaneDataProvider
{
    // Inspector

    [Header("Flight Plans")]
    [SerializeField] private List<MockFlightPlan> flightPlans = new();

    [Header("Airports & Runways")]
    [SerializeField] private List<Airport> airports = new();

    [Header("Random Route Shape")]
    [Tooltip("Min/max number of interior XZ waypoints between rotation and touchdown.")]
    [SerializeField] private Vector2Int interiorWaypointRange = new Vector2Int(5, 9);
    [Tooltip("Maximum lateral offset (perpendicular to chord) for interior XZ waypoints, in authored units.")]
    [SerializeField] private float lateralJitter = 0.012f;
    [Tooltip("Minimum Euclidean distance allowed between any two XZ waypoints (and between " +
             "interior waypoints and the rotation/touchdown endpoints), in authored units.")]
    [SerializeField] private float minWaypointSpacing = 0.008f;
    [Tooltip("Set non-zero for reproducible routes. 0 uses a time-based seed.")]
    [SerializeField] private int randomSeed = 0;

    [Header("Altitude Profile")]
    [SerializeField] private float baseAltitudeFeet = 4000f;
    [SerializeField] private float verticalSeparationFeet = 5000f;
    [Tooltip("Per-plane random offset on cruise altitude, in feet (uniform).")]
    [SerializeField] private Vector2 cruiseAltJitterFeet = new Vector2(-200f, 200f);
    [Tooltip("Authored arc-length distance from rotation to top-of-climb.")]
    [SerializeField] private float climbDistance = 0.08f;
    [Tooltip("Authored arc-length distance from top-of-descent to touchdown.")]
    [SerializeField] private float descentDistance = 0.1f;
    [Tooltip("Continuous cruise wobble (smooth replacement for per-waypoint altitude jitter).")]
    [SerializeField] private float cruiseWobbleFeet = 80f;
    [SerializeField] private float cruiseWobbleCyclesPerUnit = 0.02f;

    [Header("World Scale")]
    [SerializeField] private float feetPerWorldUnit = 1640f;
    [SerializeField] private float horizontalScale = 1f;
    [Tooltip("Constant added to every plane's world Y position. Use to lift planes off " +
             "an inverted/offset origin so ground-level spawn appears at the right height.")]
    [SerializeField] private float worldYOffset = 1.2f;

    [Header("Speed")]
    [SerializeField] private float knotsToUnitsPerSecond = 0.000514f;
    [SerializeField] private float simTimeMultiplier = 60f;

    [Header("Phase Speed Factors")]
    [Range(0.1f, 1f)][SerializeField] private float climbSpeedFactor = 0.85f;
    [Range(0.1f, 1f)][SerializeField] private float descentSpeedFactor = 0.9f;
    [Range(0.1f, 1f)][SerializeField] private float groundRollPeakFactor = 0.6f;
    [Range(0.01f, 0.3f)][SerializeField] private float rollFloorFactor = 0.05f;
    [Tooltip("Per-plane uniform speed jitter applied to cruise speed, in knots.")]
    [SerializeField] private Vector2 perPlaneSpeedJitterKnots = new Vector2(-15f, 15f);

    [Header("Path Quality")]
    [Tooltip("Samples per Hermite segment used to invert arc length.")]
    [SerializeField] private int arcLengthSamplesPerSegment = 32;

    [Header("Debug Logging")]
    [SerializeField] private bool logPathSummary = true;
    [SerializeField] private bool logStepState = false;
    [SerializeField] private float stepLogIntervalSeconds = 1.0f;

    // -------- State ----------------------------------------------------------

    private const int WaypointPlacementRetries = 12;

    private readonly Dictionary<string, PlaneData> _planes = new();
    private readonly Dictionary<string, FlightState> _states = new();
    private float _stepLogAccumulator;

    private class FlightState
    {
        public MockFlightPlan plan;
        public FlightPath path;
        public float s;
        public float speedJitterKnots;
    }

    // IPlaneDataProvider

    public IReadOnlyDictionary<string, PlaneData> ActivePlanes => _planes;
    public event Action<PlaneData> OnPlaneUpdated;

#pragma warning disable CS0067
    public event Action<string> OnPlaneRemoved;
#pragma warning restore CS0067

    public Vector3[] GetRoute(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;
        if (!_states.TryGetValue(hex, out var state) || state?.path == null) return null;

        int n = Mathf.Clamp(Mathf.RoundToInt(state.path.TotalLength / 5f), 32, 512);
        var authored = state.path.DenseSamples(n);
        var world = new Vector3[authored.Length];
        for (int i = 0; i < authored.Length; i++) world[i] = ToWorldSpace(authored[i]);
        return world;
    }

    // Lifecycle 

    private void Start() => InitializePlanes();
    private void Update() => Tick(Time.deltaTime);

    public void Tick(float deltaTime)
    {
        foreach (var (id, plane) in _planes)
        {
            StepPlane(plane, _states[id], deltaTime);
            plane.lastSeen = DateTimeOffset.UtcNow;
            plane.seenPos = 0f;
            OnPlaneUpdated?.Invoke(plane);
        }

        if (logStepState)
        {
            _stepLogAccumulator += deltaTime;
            if (_stepLogAccumulator >= Mathf.Max(0.05f, stepLogIntervalSeconds))
            {
                _stepLogAccumulator = 0f;
                LogStepStateAllPlanes();
            }
        }
    }

    // Init

    private void InitializePlanes()
    {
        var rng = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

        if (airports == null || airports.Count < 2)
        {
            Debug.LogError("[MockPlaneDataProvider] Need at least 2 airports to generate routes.");
            return;
        }

        var usedPairs = new HashSet<(int, int)>();
        var usedOrigins = new HashSet<int>();
        var usedDests = new HashSet<int>();

        for (int i = 0; i < flightPlans.Count; i++)
        {
            var plan = flightPlans[i];
            if (plan == null) continue;

            string hex = GenerateMockHex(rng);

            (int originIdx, int destIdx) = PickAirportPair(rng, usedPairs, usedOrigins, usedDests);
            usedPairs.Add((originIdx, destIdx));
            usedOrigins.Add(originIdx);
            usedDests.Add(destIdx);

            var origin = airports[originIdx];
            var dest = airports[destIdx];

            var (rollStart, rotation) = PickTakeoffEnds(origin, dest.Center);
            var (touchdown, rollOutEnd) = PickLandingEnds(dest, origin.Center);

            Vector3 takeoffBearing = SafeNormalize(rotation - rollStart, Vector3.forward);
            Vector3 landingBearing = SafeNormalize(rollOutEnd - touchdown, Vector3.forward);

            float cruiseAlt = baseAltitudeFeet + i * verticalSeparationFeet
                + Mathf.Lerp(cruiseAltJitterFeet.x, cruiseAltJitterFeet.y, (float)rng.NextDouble());
            cruiseAlt = Mathf.Max(500f, cruiseAlt);

            int interiorCount = PickInteriorCount(rng);
            var interior = GenerateInteriorWaypoints(
                rotation, touchdown, interiorCount,
                lateralJitter, minWaypointSpacing, rng);

            var bp = new FlightPath.BuildParams
            {
                rollStart = rollStart,
                rotation = rotation,
                touchdown = touchdown,
                rollOutEnd = rollOutEnd,
                takeoffBearing = takeoffBearing,
                landingBearing = landingBearing,
                interiorXZ = interior,
                climbDistance = climbDistance,
                descentDistance = descentDistance,
                cruiseAltFeet = cruiseAlt,
                wobbleAmplitudeFeet = cruiseWobbleFeet,
                wobbleFrequency = cruiseWobbleCyclesPerUnit,
                wobblePhase = (float)(rng.NextDouble() * 2.0 * Math.PI),
                groundRollPeakFactor = groundRollPeakFactor,
                climbSpeedFactor = climbSpeedFactor,
                cruiseSpeedFactor = 1f,
                descentSpeedFactor = descentSpeedFactor,
                rollFloorFactor = rollFloorFactor,
                arcLengthSamples = arcLengthSamplesPerSegment,
            };
            var path = FlightPath.Build(bp);

            plan.waypoints.Clear();
            plan.waypoints.Add(rollStart);
            plan.waypoints.Add(rotation);
            if (interior != null) plan.waypoints.AddRange(interior);
            plan.waypoints.Add(touchdown);
            plan.waypoints.Add(rollOutEnd);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(plan);
#endif

            Vector3 startAuthored = path.Sample(0f);
            float initialTrack = path.TrackDeg(0f);

            var plane = new PlaneData
            {
                hex = hex,
                flight = plan.callsign,
                lat = startAuthored.z,
                lon = startAuthored.x,
                ASL = startAuthored.y,
                gs = 0f,
                track = initialTrack,
                baroRate = 0f,
                squawk = "1200",
                status = PlaneStatus.Normal,
                lastSeen = DateTimeOffset.UtcNow,
                seenPos = 0f,
                mapPosition = ToWorldSpace(startAuthored),
            };

            _planes[hex] = plane;
            _states[hex] = new FlightState
            {
                plan = plan,
                path = path,
                s = 0f,
                speedJitterKnots = Mathf.Lerp(perPlaneSpeedJitterKnots.x, perPlaneSpeedJitterKnots.y, (float)rng.NextDouble()),
            };

            if (logPathSummary)
                LogPathSummary(hex, plan.callsign, origin, dest,
                               rollStart, rotation, touchdown, rollOutEnd,
                               takeoffBearing, landingBearing,
                               interior, cruiseAlt, path);
        }
    }

    private void LogPathSummary(
        string hex, string callsign, Airport origin, Airport dest,
        Vector3 rollStart, Vector3 rotation, Vector3 touchdown, Vector3 rollOutEnd,
        Vector3 takeoffBearing, Vector3 landingBearing,
        List<Vector3> interior, float cruiseAlt, FlightPath path)
    {
        var sb = new System.Text.StringBuilder(512);
        sb.AppendLine($"[FlightDebug][{hex}] === path summary === flight={callsign} {origin.name} → {dest.name}");
        sb.AppendLine($"  rollStart  = {rollStart.x:F1}, {rollStart.z:F1}");
        sb.AppendLine($"  rotation   = {rotation.x:F1}, {rotation.z:F1}    bearing={Bearing(takeoffBearing):F1}°");
        if (interior != null)
            for (int i = 0; i < interior.Count; i++)
                sb.AppendLine($"  interior[{i}] = {interior[i].x:F1}, {interior[i].z:F1}");
        sb.AppendLine($"  touchdown  = {touchdown.x:F1}, {touchdown.z:F1}   bearing={Bearing(landingBearing):F1}°");
        sb.AppendLine($"  rollOutEnd = {rollOutEnd.x:F1}, {rollOutEnd.z:F1}");
        sb.AppendLine($"  totalArcLength = {path.TotalLength:F1}  cruiseAlt = {cruiseAlt:F0} ft");
        sb.AppendLine($"  sRotation={path.SRotation:F1}  sTopOfClimb={path.STopOfClimb:F1}  " +
                      $"sTopOfDescent={path.STopOfDescent:F1}  sTouchdown={path.STouchdown:F1}");
        Debug.Log(sb.ToString());
    }

    private static float Bearing(Vector3 unitXZ)
    {
        return ((Mathf.Atan2(unitXZ.x, unitXZ.z) * Mathf.Rad2Deg) + 360f) % 360f;
    }

    private void LogStepStateAllPlanes()
    {
        foreach (var (id, plane) in _planes)
        {
            var state = _states[id];
            var phase = state.path != null ? state.path.PhaseAt(state.s) : FlightPhase.Arrived;
            
            Debug.Log($"[FlightDebug][{id}] s={state.s:F1}/{state.path?.TotalLength:F1} " +
                      $"phase={phase} alt={plane.ASL:F0}ft track={plane.track:F1}° gs={plane.gs:F1}kt " +
                      $"baroRate={plane.baroRate:F0}fpm turnRate={plane.turnRateDegPerSec:F2}°/s");
        }
    }

    // Per-frame stepping

    private void StepPlane(PlaneData plane, FlightState state, float deltaTime)
    {
        var path = state.path;
        if (path == null) return;

        if (state.s >= path.TotalLength)
        {
            var end = path.Sample(path.TotalLength);
            plane.mapPosition = ToWorldSpace(end);
            plane.ASL = end.y;
            plane.gs = 0f;
            plane.baroRate = 0f;
            return;
        }

        float factor = path.SpeedFactor(state.s);
        var phase = path.PhaseAt(state.s);

        float effectiveSpeedKnots = state.plan.cruiseSpeed * factor;
        if (phase == FlightPhase.Cruise) effectiveSpeedKnots += state.speedJitterKnots;
        float floor = (phase == FlightPhase.TakeoffRoll || phase == FlightPhase.LandingRoll) ? 5f : 30f;
        effectiveSpeedKnots = Mathf.Max(effectiveSpeedKnots, floor);

        // knotsToUnitsPerSecond converts knots → authored units / sec directly. Display
        // scale (horizontalScale) only affects rendered position, not traversal speed.
        float authoredPerSec = effectiveSpeedKnots * knotsToUnitsPerSecond * simTimeMultiplier;
        state.s = Mathf.Min(path.TotalLength, state.s + authoredPerSec * deltaTime);

        Vector3 pAuthored = path.Sample(state.s);
        plane.mapPosition = ToWorldSpace(pAuthored);
        plane.lat = pAuthored.z;
        plane.lon = pAuthored.x;
        plane.ASL = pAuthored.y;
        plane.gs = effectiveSpeedKnots;
        plane.track = path.TrackDeg(state.s);
        plane.turnRateDegPerSec = path.CurvatureXZ(state.s) * authoredPerSec * Mathf.Rad2Deg;

        float ftPerRealSec = path.DyDsFeetPerUnit(state.s) * authoredPerSec;
        float simMinPerReal = Mathf.Max(0.0001f, simTimeMultiplier) / 60f;
        plane.baroRate = ftPerRealSec / simMinPerReal;
    }

    // Interior waypoint generation 

    private int PickInteriorCount(System.Random rng)
    {
        int lo = Mathf.Max(0, Mathf.Min(interiorWaypointRange.x, interiorWaypointRange.y));
        int hi = Mathf.Max(0, Mathf.Max(interiorWaypointRange.x, interiorWaypointRange.y));
        return rng.Next(lo, hi + 1);
    }

    /// <summary>
    /// Place `count` waypoints between a and b on evenly-spaced sections of the chord,
    /// each offset perpendicular by a random amount up to ±lateralJitter. Candidates
    /// that violate minSpacing are rejected and resampled; if all retries fail the
    /// waypoint falls back to its on-chord position (which always satisfies spacing
    /// because on-chord points sit one section length apart).
    /// </summary>
    private static List<Vector3> GenerateInteriorWaypoints(
        Vector3 a, Vector3 b,
        int count, float lateralJitter, float minSpacing,
        System.Random rng)
    {
        var result = new List<Vector3>(Mathf.Max(0, count));
        if (count <= 0) return result;

        Vector3 chord = b - a;
        float chordLen = chord.magnitude;
        if (chordLen < 1e-3f) return result;

        Vector3 along = chord / chordLen;
        Vector3 perp = new Vector3(-along.z, 0f, along.x);

        float sectionLen = chordLen / (count + 1);
        float effMin = Mathf.Min(minSpacing, sectionLen * 0.7f);
        float minSq = effMin * effMin;

        var placed = new List<Vector3>(count + 2) { a, b };

        for (int i = 1; i <= count; i++)
        {
            Vector3 onChord = a + along * (i * sectionLen);
            Vector3 chosen = onChord;

            for (int attempt = 0; attempt < WaypointPlacementRetries; attempt++)
            {
                float lateral = ((float)rng.NextDouble() * 2f - 1f) * lateralJitter;
                Vector3 candidate = onChord + perp * lateral;

                bool ok = true;
                for (int j = 0; j < placed.Count; j++)
                {
                    Vector3 d = placed[j] - candidate;
                    d.y = 0f;
                    if (d.sqrMagnitude < minSq) { ok = false; break; }
                }
                if (ok) { chosen = candidate; break; }
            }

            result.Add(chosen);
            placed.Add(chosen);
        }

        return result;
    }

    // Airport pair selection

    private (int origin, int dest) PickAirportPair(
        System.Random rng,
        HashSet<(int, int)> usedPairs,
        HashSet<int> usedOrigins,
        HashSet<int> usedDests)
    {
        int n = airports.Count;
        if (TryPickPair(rng, n, usedPairs, usedOrigins, usedDests, out var pair)) return pair;
        if (TryPickPair(rng, n, usedPairs, null, usedDests, out pair)) return pair;
        if (TryPickPair(rng, n, usedPairs, null, null, out pair)) return pair;

        int oFallback = rng.Next(n);
        int dFallback;
        do { dFallback = rng.Next(n); } while (dFallback == oFallback);
        return (oFallback, dFallback);
    }

    private bool TryPickPair(
        System.Random rng,
        int n,
        HashSet<(int, int)> usedPairs,
        HashSet<int> excludeOrigins,
        HashSet<int> excludeDests,
        out (int origin, int dest) pair)
    {
        int candidates = 0;
        for (int o = 0; o < n; o++)
        {
            if (excludeOrigins != null && excludeOrigins.Contains(o)) continue;
            for (int d = 0; d < n; d++)
            {
                if (o == d) continue;
                if (excludeDests != null && excludeDests.Contains(d)) continue;
                if (usedPairs.Contains((o, d))) continue;
                candidates++;
            }
        }

        if (candidates == 0) { pair = (0, 0); return false; }

        int target = rng.Next(candidates);
        int seen = 0;
        for (int o = 0; o < n; o++)
        {
            if (excludeOrigins != null && excludeOrigins.Contains(o)) continue;
            for (int d = 0; d < n; d++)
            {
                if (o == d) continue;
                if (excludeDests != null && excludeDests.Contains(d)) continue;
                if (usedPairs.Contains((o, d))) continue;
                if (seen == target) { pair = (o, d); return true; }
                seen++;
            }
        }

        pair = (0, 0);
        return false;
    }


    private static (Vector3 rollStart, Vector3 rotation) PickTakeoffEnds(Airport ap, Vector3 toward)
    {
        Vector3 a = Flat(ap.runwayEndA);
        Vector3 b = Flat(ap.runwayEndB);
        Vector3 t = Flat(toward);
        bool aIsRotation = (a - t).sqrMagnitude < (b - t).sqrMagnitude;
        return aIsRotation ? (b, a) : (a, b);
    }

    private static (Vector3 touchdown, Vector3 rollOutEnd) PickLandingEnds(Airport ap, Vector3 from)
    {
        Vector3 a = Flat(ap.runwayEndA);
        Vector3 b = Flat(ap.runwayEndB);
        Vector3 f = Flat(from);
        bool aIsTouchdown = (a - f).sqrMagnitude < (b - f).sqrMagnitude;
        return aIsTouchdown ? (a, b) : (b, a);
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    private static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
    {
        v.y = 0f;
        return v.sqrMagnitude < 0.0001f ? fallback : v.normalized;
    }

    private Vector3 ToWorldSpace(Vector3 authored)
    {
        float yUnits = feetPerWorldUnit > 0.0001f ? authored.y / feetPerWorldUnit : authored.y;
        return new Vector3(
            authored.x * horizontalScale,
            yUnits * horizontalScale + worldYOffset,
            authored.z * horizontalScale);
    }

    private static string GenerateMockHex(System.Random rng)
    {
        int value = rng.Next(0x7C0000, 0x7CFFFF);
        return value.ToString("X6");
    }
}

