using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lazy state machine
public class FactoryNorthWestTracker : MonoBehaviour
{
    public PoweredDoor LeftDoor;
    public PoweredDoor RightDoor;

    public enum NorthWestPosition
    {
        Airlock,
        Electric,
        Closet,
    }

    private NorthWestPosition playerPosition;

    public void SetPlayerAirlock()  => SetPlayerPosition(NorthWestPosition.Airlock);
    public void SetPlayerElectric() => SetPlayerPosition(NorthWestPosition.Electric);
    public void SetPlayerCloset()   => SetPlayerPosition(NorthWestPosition.Closet);

    public void SetPlayerPosition(NorthWestPosition position)
    {
        switch (position)
        {
            case NorthWestPosition.Airlock:
                // Electric -> Airlock
                if (playerPosition == NorthWestPosition.Electric)
                {
                    // if Player is holding bob and door is not powered -> cheating
                    // if Player is holding bob and door is powered -> hasbob
                    if (IsPlayerHoldingBob())
                    {
                        if (IsRightDoorPowered())
                        {
                            Debug.Log("factoryAcquiredBob");
                        }
                        else
                        {
                            Debug.Log("factoryBobCheated");
                        }
                    }
                }
                break;

            case NorthWestPosition.Electric:
                break;

            case NorthWestPosition.Closet:
                // if left door is closed -> softlock
                if (!IsLeftDoorPowered())
                {
                    Debug.Log("Softlock!");
                }
                break;
        }
        playerPosition = position;
    }

    private bool IsPlayerHoldingBob()   => PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().itemName == "Conductive Bob";
    private bool IsLeftDoorPowered()    => LeftDoor.Powered;
    private bool IsRightDoorPowered()   => RightDoor.Powered;
}