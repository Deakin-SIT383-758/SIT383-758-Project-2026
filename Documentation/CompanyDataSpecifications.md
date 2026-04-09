## **Company Data Specifications**

**Version:** 1.0 **Correct as at:** 9 Apr 26

 ---

**Overview**

This document provides an overview of all Company-wide Data Specifications.

* **Mandatory Fields:** Any field marked mandatory must be included in any data packet generated, even if unused for that specific application.

* **Non-Mandatory Fields:** These can be included if required for a specific use case but must adhere to the specified data type and format.

* **Custom Fields:** Fields not listed can be included for specific use cases. However, it is best practice to request an update to this document to ensure future data type/format commonality.

* **Fallbacks:** If you are using another team's data products and require non-mandatory fields, ensure appropriate fallbacks are included (e.g., check for the JSON key; if it is missing, calculate it or use static/derived fallbacks).

--- 
### **Aircraft Position Data**

Provided as position updates (e.g., 1Hz) while in flight.

| field\_name | data\_type | Description | Mandatory | Comments |
| :---- | :---- | :---- | :---- | :---- |
| **id** | String (8 chars) | Aircraft Callsign | No | Elite 4 owns this data  |
| **lat** | Number | Latitude decimal degrees |  **Yes** |  |
| **lon** | Number | Longitude decimal degrees |  **Yes** |  |
| **alt** | Number/String | ASL (m) or 'ground' |  **Yes** |  |
| **track** | Number | True track over ground | **Yes** | 0-360 degrees  |
| **gs** | Number | Ground speed in m/s |  **Yes** |  |
| **alt\_baro** | Number/String | Barometric altitude in m or 'ground' | No | Meters or text 'ground'  |
| **alt\_geom** | Number | Geometric altitude (WGS84) | No | Height above the ellipsoid  |
| **mag\_heading** | Number | Magnetic heading from north | No |  |
| **ias** | Number | Indicated air speed in m/s | No |  |
| **tas** | Number | True air speed in m/s | No |  |
| **track\_rate** | Number | Rate of change of track (deg/s) | No |  |
| **roll** | Number | Roll in degrees (negative is left) | No |  |
| **baro\_rate** | Number | Barometric rate of change (m/s) | No |  |
| **geom\_rate** | Number | Geometric rate of change (m/s) | No |  |
| **seen\_pos** | Number | Seconds since last position update | No |  |
| **timestamp** | Number | As at timestamp | No | Unix time (seconds since 1 Jan 1970\)  |

**Note:** All timestamps are in Unix time. In C\#, this can be obtained via: System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();.

#### **Example Position Packet**

```JSON

{  
  "id": "BAW1234",  
  "lat": 51.4700,  
  "lon": -0.4543,  
  "alt": 10668.0,  
  "track": 285,  
  "gs": 231.5,  
  "timestamp": 1712582400  
}
```

---

### **Flight Plan**

A pre-flight summary of flight information.

* **Locations:** Can be identified by airfield code or by latitude/longitude (mandatory if codes are not used).

* **Airfield Codes:** Data is TBI and will include latitude, longitude, and height ASL as a lookup table.

* **Route Points:** Waypoints are optional; if omitted, the route is considered direct from departure to destination.

| field\_name | data\_type | Description | Mandatory | Comments |
| :---- | :---- | :---- | :---- | :---- |
| **id** | String (8 chars) | Aircraft Callsign | Optional | Elite 4 owns this  |
| **dep\_code** | String | Departure airport identifier | Optional |  |
| **dest\_code** | String | Destination airport identifier | Optional | Mandatory if lat/lon not used  |
| **dep\_lat** | Number | Departure latitude | No\* | Mandatory if code omitted  |
| **dep\_lon** | Number | Departure longitude | No\* | Mandatory if code omitted  |
| **dep\_alt** | Number | Departure ASL | Optional |  |
| **dest\_lat** | Number | Destination latitude | No\* | Mandatory if code omitted  |
| **dest\_lon** | Number | Destination longitude | No\* | Mandatory if code omitted  |
| **dest\_alt** | Number | Destination ASL | Optional |  |
| **sched\_dep** | Integer | Scheduled departure time | Optional | Unix timestamp  |
| **sched\_arr** | Integer | Scheduled arrival time | Optional | Unix timestamp  |
| **route\_points** | JSON/Array | Ordered list of route points | Optional |  |

#### **route\_points Object**

| field\_name | data\_type | Description | Mandatory |
| :---- | :---- | :---- | :---- |
| **lat** | Number | Waypoint Latitude |  **Yes**  |
| **lon** | Number | Waypoint Longitude |  **Yes**  |
| **alt** | Number | Waypoint ASL |  **Yes**  |
| **eta\_offset** | Integer | Seconds from departure | Optional  |

#### **Example: Airfield Codes with Waypoints**

```JSON

{  
  "id": "QFA432",  
  "dep_code": "SYD",  
  "dest_code": "MEL",  
  "dep_alt": 6.4,  
  "dest_alt": 43.3,  
  "sched_dep": 1712582400,  
  "sched_arr": 1712587800,  
  "route_points": \[  
    {  
      "lat": -34.521,  
      "lon": 150.123,  
      "alt": 3000.0,  
      "eta_offset": 600  
    },  
    {  
      "lat": -35.890,  
      "lon": 148.567,  
      "alt": 10000.0,  
      "eta_offset": 1800  
    },  
    {  
      "lat": -37.123,  
      "lon": 146.012,  
      "alt": 5000.0,  
      "eta_offset": 3600  
    }  
  \]  
}
```

#### **Example: Latitude/Longitude (Direct Flight)**

```JSON

{  
  "id": "N711ZA",  
  "dep_lat": -31.940,  
  "dep_lon": 115.967,  
  "dep_alt": 20.0,  
  "dest_lat": -27.384,  
  "dest_lon": 153.117,  
  "dest_alt": 4.0,  
  "sched_dep": 1712600000,  
  "sched_arr": 1712618000  
}
```
