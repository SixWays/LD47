using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupLives : Pickup<int> {
    protected override void OnPickedUp(Player p){
        HighwayManagement.AddLives(_value);
    }
}
