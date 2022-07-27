using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPacketManager : MonoBehaviour
{
    [SerializeField] private SeedPacket[] allSeeds;

    void Start()
    {
        foreach (SeedPacket sp in allSeeds)
            sp.SeedPacketStart();
    }
}
