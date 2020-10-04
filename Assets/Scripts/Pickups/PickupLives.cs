using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupLives : Pickup<int> {
    protected override void OnPickedUp(){
        HighwayManagement.AddLives(_value);
    }
}
