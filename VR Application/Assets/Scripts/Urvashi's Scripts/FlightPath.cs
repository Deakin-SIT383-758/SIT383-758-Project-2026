using System.Collections.Generic;
using UnityEngine;

namespace OasXr.Flight
{
    public enum FlightPhase
    {
        TakeoffRoll,
        Climb,
        Cruise,
        Descent,
        LandingRoll,
        Arrived
    }

    /// <summary>
    /// Arc-length parameterised flight path with decoupled XZ shape, altitude,
    /// and speed profiles.
    ///
    /// XZ : straight line (rollStart → rotation), Hermite cubic spline through
    ///      [rotation, interior waypoints..., approach1] with end tangents pinned
    ///      to runway bearings, straight line (approach1 → rollOutEnd). C¹ at
    ///      every join.
    ///
    /// Altitude : smoothstep-blended function of arc length. Climb and descent use
    ///      Hermite-style smoothstep (3t²-2t³) which has zero slope at both ends,
    ///      so dy/ds is continuous across phase boundaries.
    ///
    /// Speed : smoothstep-blended factor on cruise speed.
    ///
    /// All inputs are in authored coordinates (the same units as runway endpoints
    /// and altitudeFeet). World-space conversion is the caller's job.
    /// </summary>
    public class FlightPath
    {
        // -------- XZ path --------------------------------------------------------

        private struct Hermite
        {
            public Vector3 p0, p1;   // endpoints (Y component carries no meaning here — XZ only)
            public Vector3 m0, m1;   // tangent vectors at endpoints
        }

        private struct Segment
        {
            public bool isLine;
            public Vector3 a, b;     // line endpoints (used if isLine)
            public Hermite h;        // hermite (used if !isLine)
            public float length;
            // Arc-length samples within the segment for inversion.
            public float[] tSamples;     // monotonic in [0,1]
            public float[] sSamples;     // monotonic in [0, length]
        }

        private readonly Segment[] _segments;
        private readonly float[]   _segCumLen;   // cumulative length at *end* of segment i
        public float TotalLength => _segCumLen[_segCumLen.Length - 1];

        // Arc-length boundaries between phases.
        public float SRotation     { get; private set; }
        public float STopOfClimb   { get; private set; }
        public float STopOfDescent { get; private set; }
        public float STouchdown    { get; private set; }
        public float SEnd          => TotalLength;

        // Altitude profile.
        private readonly float _cruiseAltFt;
        private readonly float _wobbleAmpFt;
        private readonly float _wobbleFreq;       // cycles per authored unit of arc length
        private readonly float _wobblePhase;

        // Speed profile (factors on cruise speed).
        private readonly float _groundRollPeak;
        private readonly float _climbFactor;
        private readonly float _cruiseFactor;
        private readonly float _descentFactor;
        private readonly float _rollFloor;        // factor at very start/end of rolls

        private FlightPath(
            Segment[] segments, float[] segCumLen,
            float sRotation, float sTopOfClimb, float sTopOfDescent, float sTouchdown,
            float cruiseAltFt, float wobbleAmpFt, float wobbleFreq, float wobblePhase,
            float groundRollPeak, float climbFactor, float cruiseFactor, float descentFactor,
            float rollFloor)
        {
            _segments = segments;
            _segCumLen = segCumLen;
            SRotation = sRotation;
            STopOfClimb = sTopOfClimb;
            STopOfDescent = sTopOfDescent;
            STouchdown = sTouchdown;
            _cruiseAltFt = cruiseAltFt;
            _wobbleAmpFt = wobbleAmpFt;
            _wobbleFreq = wobbleFreq;
            _wobblePhase = wobblePhase;
            _groundRollPeak = groundRollPeak;
            _climbFactor = climbFactor;
            _cruiseFactor = cruiseFactor;
            _descentFactor = descentFactor;
            _rollFloor = rollFloor;
        }

        // -------- Construction ---------------------------------------------------

        public struct BuildParams
        {
            public Vector3 rollStart, rotation, touchdown, rollOutEnd;
            public Vector3 takeoffBearing, landingBearing; // unit XZ vectors

            public List<Vector3> interiorXZ;               // interior waypoints (XZ only, Y ignored)

            // Distances along arc length for altitude milestones.
            public float climbDistance;     // s_topOfClimb - s_rotation
            public float descentDistance;   // s_touchdown  - s_topOfDescent

            public float cruiseAltFeet;
            public float wobbleAmplitudeFeet;
            public float wobbleFrequency;
            public float wobblePhase;

            public float groundRollPeakFactor;
            public float climbSpeedFactor;
            public float cruiseSpeedFactor;
            public float descentSpeedFactor;
            public float rollFloorFactor;

            public int   arcLengthSamples;  // samples per spline segment for arc-length inversion
        }

        public static FlightPath Build(BuildParams p)
        {
            var segments = new List<Segment>(2 + Mathf.Max(1, (p.interiorXZ?.Count ?? 0) + 1));

            Vector3 takeoff = SafeNormalize(p.takeoffBearing);
            Vector3 landing = SafeNormalize(p.landingBearing);

            // -- Centripetal Catmull-Rom tangents -------------------------------
            //
            // Knots: rotation, interior waypoints..., touchdown. Parameter spacing
            // between adjacent knots is sqrt(chord) (centripetal, α = 0.5). This
            // prevents curvature spikes and cusps when knot spacing is non-uniform
            // (which it is — Chebyshev clusters waypoints near the runway ends).
            //
            // For a Catmull-Rom curve P(t) the *global* tangent at interior knot i is
            // a cross-weighted blend of the two one-sided differences:
            //   T_i = (dt_i · ΔP_prev/dt_prev + dt_prev · ΔP_next/dt_i) / (dt_prev+dt_i)
            // where dt_j = sqrt(|P_{j+1} − P_j|) and ΔP_j = P_{j+1} − P_j.
            //
            // Endpoint tangents are *direction*-pinned to runway bearing. We set
            // their *magnitude* to match the global tangent magnitude at the
            // adjacent interior knot, so parametric speed is continuous across the
            // line→spline (and spline→line) joins. This is what eliminates the whip
            // at rotation and the slide into touchdown.
            //
            // Per-segment Hermite tangents are then m0 = dt_i · T_i and
            // m1 = dt_i · T_{i+1}. Both ends of a segment scale by the *same*
            // segment-local dt; adjacent segments scale by their own dt, which is
            // precisely the magnitude difference that makes non-uniform Catmull-Rom
            // C¹ in global t while remaining valid Hermite cubics in local u.

            var keys = new List<Vector3>(2 + (p.interiorXZ?.Count ?? 0));
            keys.Add(Flat(p.rotation));
            if (p.interiorXZ != null) foreach (var w in p.interiorXZ) keys.Add(Flat(w));
            keys.Add(Flat(p.touchdown));
            int N = keys.Count;

            var dt = new float[N - 1];
            for (int i = 0; i < N - 1; i++)
                dt[i] = Mathf.Sqrt(Mathf.Max(1e-4f, Vector3.Distance(keys[i], keys[i + 1])));

            var T = new Vector3[N];
            for (int i = 1; i < N - 1; i++)
            {
                float dtPrev = dt[i - 1];
                float dtNext = dt[i];
                Vector3 dPrev = (keys[i]     - keys[i - 1]) / dtPrev;
                Vector3 dNext = (keys[i + 1] - keys[i]    ) / dtNext;
                T[i] = (dtNext * dPrev + dtPrev * dNext) / (dtPrev + dtNext);
            }
            // Endpoints: direction pinned, magnitude matched to neighbour.
            float magNearStart = N >= 3 ? T[1].magnitude        : (keys[1] - keys[0]).magnitude / Mathf.Max(1e-4f, dt[0]);
            float magNearEnd   = N >= 3 ? T[N - 2].magnitude    : (keys[N - 1] - keys[N - 2]).magnitude / Mathf.Max(1e-4f, dt[N - 2]);
            T[0]     = takeoff * magNearStart;
            T[N - 1] = landing * magNearEnd;

            // -- Segments: line | spline (multi-segment) | line -----------------

            // Segment 0: takeoff roll.
            segments.Add(BuildLineSegment(Flat(p.rollStart), Flat(p.rotation)));

            // Curving spline rotation → ... → touchdown.
            for (int i = 0; i < N - 1; i++)
            {
                var h = new Hermite
                {
                    p0 = keys[i], p1 = keys[i + 1],
                    m0 = dt[i] * T[i],
                    m1 = dt[i] * T[i + 1],
                };
                segments.Add(BuildHermiteSegment(h, p.arcLengthSamples));
            }

            // Final segment: landing roll.
            segments.Add(BuildLineSegment(Flat(p.touchdown), Flat(p.rollOutEnd)));

            // -- Cumulative segment lengths --
            var cum = new float[segments.Count];
            float acc = 0f;
            for (int i = 0; i < segments.Count; i++)
            {
                acc += segments[i].length;
                cum[i] = acc;
            }

            // Arc-length phase boundaries.
            //   sRotation  = end of segment 0 (lift-off)
            //   sTouchdown = end of penultimate-or-spline segment (= start of final landing-roll line)
            float sRotation  = cum[0];
            float sTouchdown = cum[cum.Length - 2];
            float sTopOfClimb   = Mathf.Min(sRotation + Mathf.Max(0f, p.climbDistance),  sTouchdown);
            float sTopOfDescent = Mathf.Max(sTouchdown - Mathf.Max(0f, p.descentDistance), sTopOfClimb);

            // -- Sanity checks ----------------------------------------------------
            float chordLen = Vector3.Distance(Flat(p.rotation), Flat(p.touchdown));
            if (chordLen < 1f)
                Debug.LogWarning($"[FlightDebug] Degenerate spline: rotation→touchdown chord is only " +
                                 $"{chordLen:F2} authored units. Origin and destination are too close.");
            if (sTopOfClimb >= sTopOfDescent - 1f)
                Debug.LogWarning($"[FlightDebug] Cruise has near-zero arc length: " +
                                 $"sTopOfClimb={sTopOfClimb:F1}, sTopOfDescent={sTopOfDescent:F1}. " +
                                 $"climbDistance + descentDistance is too large for this route, OR " +
                                 $"the route itself is too short. Plane will go climb-straight-into-descent.");

            return new FlightPath(
                segments.ToArray(), cum,
                sRotation, sTopOfClimb, sTopOfDescent, sTouchdown,
                p.cruiseAltFeet, p.wobbleAmplitudeFeet, p.wobbleFrequency, p.wobblePhase,
                p.groundRollPeakFactor, p.climbSpeedFactor, p.cruiseSpeedFactor, p.descentSpeedFactor,
                p.rollFloorFactor);
        }

        private static Segment BuildLineSegment(Vector3 a, Vector3 b)
        {
            float len = Vector3.Distance(a, b);
            return new Segment
            {
                isLine = true,
                a = a, b = b,
                length = len,
                tSamples = new[] { 0f, 1f },
                sSamples = new[] { 0f, len },
            };
        }

        private static Segment BuildHermiteSegment(Hermite h, int samples)
        {
            samples = Mathf.Max(8, samples);
            var ts = new float[samples + 1];
            var ss = new float[samples + 1];
            ts[0] = 0f; ss[0] = 0f;
            Vector3 prev = HermitePoint(h, 0f);
            float acc = 0f;
            for (int i = 1; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 cur = HermitePoint(h, t);
                acc += Vector3.Distance(prev, cur);
                ts[i] = t;
                ss[i] = acc;
                prev = cur;
            }
            return new Segment
            {
                isLine = false,
                h = h,
                length = acc,
                tSamples = ts,
                sSamples = ss,
            };
        }

        // -------- Sampling -------------------------------------------------------

        /// <summary>Authored-space position at arc length s, with altitude in feet on Y.</summary>
        public Vector3 Sample(float s)
        {
            ResolveSegment(s, out int idx, out float localS);
            Vector3 xz = SegmentPoint(_segments[idx], localS);
            float altFt = AltitudeFeet(s);
            return new Vector3(xz.x, altFt, xz.z);
        }

        public float TrackDeg(float s)
        {
            ResolveSegment(s, out int idx, out float localS);
            Vector3 t = SegmentTangent(_segments[idx], localS);
            if (t.x * t.x + t.z * t.z < 1e-8f) return 0f;
            return ((Mathf.Atan2(t.x, t.z) * Mathf.Rad2Deg) + 360f) % 360f;
        }

        /// <summary>Vertical slope (feet per authored unit of arc length).</summary>
        public float DyDsFeetPerUnit(float s) => AltitudeSlope(s);

        /// <summary>Signed XZ curvature in 1/authored unit (used for bank target).</summary>
        public float CurvatureXZ(float s)
        {
            const float h = 0.5f;
            float a = TrackDeg(Mathf.Max(0f, s - h));
            float b = TrackDeg(Mathf.Min(TotalLength, s + h));
            float dTrack = Mathf.DeltaAngle(a, b) * Mathf.Deg2Rad;
            float ds = Mathf.Min(TotalLength, s + h) - Mathf.Max(0f, s - h);
            return ds > 1e-5f ? dTrack / ds : 0f;
        }

        public FlightPhase PhaseAt(float s)
        {
            if (s <= SRotation)     return FlightPhase.TakeoffRoll;
            if (s <= STopOfClimb)   return FlightPhase.Climb;
            if (s <= STopOfDescent) return FlightPhase.Cruise;
            if (s <= STouchdown)    return FlightPhase.Descent;
            if (s <  SEnd)          return FlightPhase.LandingRoll;
            return FlightPhase.Arrived;
        }

        /// <summary>Speed factor on cruise speed at arc length s, smooth across phase boundaries.</summary>
        public float SpeedFactor(float s)
        {
            // Boundary anchors: (s, factor)
            //   0                : rollFloor
            //   sRotation        : groundRollPeak
            //   sRotation + L_b  : climbFactor          (small ramp into airborne speed)
            //   sTopOfClimb      : cruiseFactor
            //   sTopOfDescent    : cruiseFactor
            //   sTouchdown - L_b : descentFactor
            //   sTouchdown       : groundRollPeak
            //   sEnd             : rollFloor
            //
            // Use smoothstep between adjacent anchors so factor is C¹.
            float climbBlend = Mathf.Min(0.25f * (STopOfClimb - SRotation), Mathf.Max(0f, STopOfClimb - SRotation));
            float descentBlend = Mathf.Min(0.25f * (STouchdown - STopOfDescent), Mathf.Max(0f, STouchdown - STopOfDescent));

            // Anchor list is implicit in the comparisons below.
            if (s <= SRotation)
                return SmoothLerp(s, 0f, SRotation, _rollFloor, _groundRollPeak);
            if (s <= SRotation + climbBlend)
                return SmoothLerp(s, SRotation, SRotation + climbBlend, _groundRollPeak, _climbFactor);
            if (s <= STopOfClimb)
                return SmoothLerp(s, SRotation + climbBlend, STopOfClimb, _climbFactor, _cruiseFactor);
            if (s <= STopOfDescent)
                return _cruiseFactor;
            if (s <= STouchdown - descentBlend)
                return SmoothLerp(s, STopOfDescent, STouchdown - descentBlend, _cruiseFactor, _descentFactor);
            if (s <= STouchdown)
                return SmoothLerp(s, STouchdown - descentBlend, STouchdown, _descentFactor, _groundRollPeak);
            return SmoothLerp(s, STouchdown, SEnd, _groundRollPeak, _rollFloor);
        }

        // -------- Inspector / RouteVisualizer helper ----------------------------

        public Vector3[] DenseSamples(int totalSamples)
        {
            totalSamples = Mathf.Max(2, totalSamples);
            var result = new Vector3[totalSamples];
            for (int i = 0; i < totalSamples; i++)
            {
                float s = (i / (float)(totalSamples - 1)) * TotalLength;
                result[i] = Sample(s);
            }
            return result;
        }

        // -------- Internals: altitude profile ------------------------------------

        private float AltitudeFeet(float s)
        {
            if (s <= SRotation || s >= STouchdown && s < SEnd) return 0f;
            if (s >= SEnd) return 0f;

            if (s <= STopOfClimb)
            {
                float u = (s - SRotation) / Mathf.Max(1e-5f, STopOfClimb - SRotation);
                return Smoothstep01(u) * _cruiseAltFt;
            }
            if (s <= STopOfDescent)
            {
                // Cruise + continuous wobble. Wobble is a sin so it's smooth in s and zero-mean.
                float wobble = _wobbleAmpFt > 0f
                    ? _wobbleAmpFt * Mathf.Sin(2f * Mathf.PI * _wobbleFreq * s + _wobblePhase)
                    : 0f;
                return _cruiseAltFt + wobble;
            }
            // Descent.
            float v = (s - STopOfDescent) / Mathf.Max(1e-5f, STouchdown - STopOfDescent);
            return (1f - Smoothstep01(v)) * _cruiseAltFt;
        }

        // dy/ds via finite difference — small and analytic-shaped enough that this is fine.
        private float AltitudeSlope(float s)
        {
            const float h = 0.25f;
            float a = AltitudeFeet(Mathf.Clamp(s - h, 0f, SEnd));
            float b = AltitudeFeet(Mathf.Clamp(s + h, 0f, SEnd));
            float ds = Mathf.Clamp(s + h, 0f, SEnd) - Mathf.Clamp(s - h, 0f, SEnd);
            return ds > 1e-5f ? (b - a) / ds : 0f;
        }

        // -------- Internals: segment sampling ------------------------------------

        private void ResolveSegment(float s, out int segIdx, out float localS)
        {
            s = Mathf.Clamp(s, 0f, TotalLength);
            // Linear scan — segment count is small (<= ~8).
            for (int i = 0; i < _segments.Length; i++)
            {
                if (s <= _segCumLen[i])
                {
                    segIdx = i;
                    float prev = i == 0 ? 0f : _segCumLen[i - 1];
                    localS = s - prev;
                    return;
                }
            }
            segIdx = _segments.Length - 1;
            localS = _segments[segIdx].length;
        }

        private static Vector3 SegmentPoint(Segment seg, float localS)
        {
            if (seg.isLine)
            {
                float u = seg.length > 1e-5f ? Mathf.Clamp01(localS / seg.length) : 0f;
                return Vector3.Lerp(seg.a, seg.b, u);
            }
            float t = ArcToParam(seg, localS);
            return HermitePoint(seg.h, t);
        }

        private static Vector3 SegmentTangent(Segment seg, float localS)
        {
            if (seg.isLine) return (seg.b - seg.a).normalized;
            float t = ArcToParam(seg, localS);
            return HermiteTangent(seg.h, t).normalized;
        }

        private static float ArcToParam(Segment seg, float localS)
        {
            // Binary search the (sSamples, tSamples) tables.
            var ss = seg.sSamples;
            var ts = seg.tSamples;
            int lo = 0, hi = ss.Length - 1;
            if (localS <= ss[0]) return ts[0];
            if (localS >= ss[hi]) return ts[hi];
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (ss[mid] <= localS) lo = mid; else hi = mid;
            }
            float span = ss[hi] - ss[lo];
            float u = span > 1e-5f ? (localS - ss[lo]) / span : 0f;
            return Mathf.Lerp(ts[lo], ts[hi], u);
        }

        // -------- Math primitives -----------------------------------------------

        private static Vector3 HermitePoint(Hermite h, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 =  2f * t3 - 3f * t2 + 1f;
            float h10 =       t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 =       t3 -      t2;
            return h00 * h.p0 + h10 * h.m0 + h01 * h.p1 + h11 * h.m1;
        }

        private static Vector3 HermiteTangent(Hermite h, float t)
        {
            float t2 = t * t;
            float dh00 =  6f * t2 - 6f * t;
            float dh10 =  3f * t2 - 4f * t + 1f;
            float dh01 = -6f * t2 + 6f * t;
            float dh11 =  3f * t2 - 2f * t;
            return dh00 * h.p0 + dh10 * h.m0 + dh01 * h.p1 + dh11 * h.m1;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private static Vector3 SafeNormalize(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized;
        }

        private static float Smoothstep01(float u)
        {
            u = Mathf.Clamp01(u);
            return u * u * (3f - 2f * u);
        }

        private static float SmoothLerp(float s, float s0, float s1, float v0, float v1)
        {
            if (s1 - s0 < 1e-5f) return v1;
            float u = Mathf.Clamp01((s - s0) / (s1 - s0));
            return Mathf.Lerp(v0, v1, Smoothstep01(u));
        }
    }
}