using System;
using UnityEngine;

namespace MM.WorldInteraction
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;
        public WorldPosition playerPosition = new WorldPosition() { Latitude = -33.87261508492854, Longitude = 151.20627287453982, HeightASL = 200 };

        public POI[] PointsOfInterest = new POI[] {
            new POI()
            {
                name = "Sydney Harbour Bridge",
                position = new WorldPosition() {Latitude = -33.85266958345332, Longitude = 151.21026508173753, HeightASL=140},
                description = "A big famous bridge"

            },
            new POI()
            {
                name = "Sydney Opera House",
                position = new WorldPosition() {Latitude = -33.85684933060164, Longitude = 151.21516705112717, HeightASL=70},
                description = "A big famous house!"

            },
            new POI()
            {
                name = "Darling Harbour",
                position = new WorldPosition() {Latitude = -33.8738584964656, Longitude = 151.20078624827704, HeightASL=20},
                description = "A very sweet harbour"

            }
        };

        MM.RangeInvariantMarkers.MarkerManager markerManager;
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Multiple World Manager objects in scene. Disabling this one.", gameObject);
                this.enabled = false;
                return;
            }

            Instance = this;

            markerManager = GameObject.FindFirstObjectByType<MM.RangeInvariantMarkers.MarkerManager>();
            if (markerManager == null)
            {
                Debug.LogError("No marker manager found - World manager closing down");
                this.enabled = false;
                return;
            }
        }

        private void Start()
        {
            foreach (var poi in PointsOfInterest)
            {
                Vector3Double position = ConvertToUnitySpace(poi.position);
                var markerData = new MM.RangeInvariantMarkers.MarkerData(position.X, position.Z, position.Y, poi.name, poi.description);
                markerManager.AddMarker(markerData);
            }
        }

        [System.Serializable]
        public struct WorldPosition
        {
            public double Latitude;
            public double Longitude;
            public double HeightASL;
        };

        [System.Serializable]
        public struct POI
        {
            public string name;
            public WorldPosition position;
            public string description;
        }

        public struct Vector3Double
        {
            public Vector3 Vector3
            {
                get
                {
                    return new Vector3(
                        (float)(X),
                        (float)(Y),
                        (float)(Z));
                }
            }

            public double X;
            public double Y;
            public double Z;
        }

        //Flat earth approximation to convert lat long to local coordinates
        public Vector3Double ConvertToUnitySpace(WorldPosition targetPosition)
        {
            const double EarthRadius = 6371000.0;
            const double DegToRad = Math.PI / 180.0;

            double deltaLat = targetPosition.Latitude - playerPosition.Latitude;
            double deltaLon = targetPosition.Longitude - playerPosition.Longitude;

            double metersPerLatDegree = EarthRadius * DegToRad;
            double metersPerLonDegree = metersPerLatDegree * Math.Cos(playerPosition.Latitude * DegToRad);

            double x = deltaLon * metersPerLonDegree;
            double z = deltaLat * metersPerLatDegree;
            double y = targetPosition.HeightASL;

            return new Vector3Double()
            {
                X = x,
                Y = y,
                Z = z
            };

        }
    }
}