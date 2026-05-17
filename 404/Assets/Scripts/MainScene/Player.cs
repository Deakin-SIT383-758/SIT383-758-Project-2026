using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked]
    public NetworkObject Vehicle {get;set;}
    private Vector3 spawnPoint = new Vector3(0.826f, -1.214f, 0.633f);
    public override void Spawned()
    {
        spawnPoint = new Vector3(-0.5075f, -1.214f, 0.633f);
        Vehicle = FindAnyObjectByType<Cockpit>().GetComponent<NetworkObject>();
    }

    public override void FixedUpdateNetwork()
    {
        UpdateParent();
    }

    void UpdateParent()
    {
        if (Vehicle != null)
        {
            transform.SetParent(Vehicle.transform);

            transform.localPosition = spawnPoint;
            transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.SetParent(null);
            if (FindAnyObjectByType<Cockpit>().GetComponent<NetworkObject>() != null)
            {
                spawnPoint = new Vector3(0.826f, -1.214f, 0.633f);
                Vehicle = FindAnyObjectByType<Cockpit>().GetComponent<NetworkObject>();
            }

            }

        }
    }
