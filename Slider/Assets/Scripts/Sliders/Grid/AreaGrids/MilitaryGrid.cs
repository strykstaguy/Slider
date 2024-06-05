using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryGrid : SGrid
{
    // public static System.EventHandler<System.EventArgs> OnRestartMilitary;

    private bool isRestarting;

    public MilitarySpriteTable militarySpriteTable; // global reference

    [SerializeField] private Transform playerRestartSpawnPosition;
    [SerializeField] private List<MilitaryUnspawnedAlly> unspawnedAllies; // dont reset one on #16

    public override void Init()
    {
        InitArea(Area.Military);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Military");

        if (unspawnedAllies.Count != 15)
        {
            Debug.LogWarning("Unspawned allies list should be 15 long.");
        }
    }

    private void OnEnable()
    {
        OnGridMove += OnTileMove;
    }

    private void OnDisable()
    {
        OnGridMove -= OnTileMove;
    }

    public override void Save()
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }


    // === Military puzzle specific ==

    public void RestartSimulation()
    {
        if (isRestarting)
            return;
        isRestarting = true;

        UIEffects.FlashWhite(
            () => {
                DoRestartSimulation();
            },
            () => {
                isRestarting = false;
            }, 
            1
        );
    }

    private void DoRestartSimulation()
    {
        Debug.Log("Restart sim!");
        SaveSystem.Current.SetBool("militaryFailedOnce", true);
        SaveSystem.Current.SetInt("militaryAttempts", SaveSystem.Current.GetInt("militaryAttempts", 0) + 1);

        if (Player.GetInstance().GetSTileUnderneath() != null)
        {
            Player.SetPosition(playerRestartSpawnPosition.position);
            Player.SetParent(null);
        }

        DisableSliders();

        RestartTroops();

        MilitaryCollectibleController.Reset();
        MilitaryWaveManager.Reset();

        SaveSystem.SaveGame("Finished Restarting Military Sim");
    }

    private void DisableSliders()
    {
        foreach (STile s in grid)
        {
            if (s.isTileActive)
            {
                s.SetTileActive(false);
                UIArtifact.GetInstance().RemoveButton(s);
            }
        }

        if (GetStileAt(0, 3).islandId != 1)
        {
            SwapTiles(GetStileAt(0, 3), GetStile(1));
        }
        
        PlayerInventory.RemoveCollectible(new Collectible.CollectibleData("Slider 1", myArea));
        PlayerInventory.RemoveCollectible(new Collectible.CollectibleData("New Slider", myArea));

        Debug.Log(PlayerInventory.Contains("Slider 1", myArea));
    }

    private void RestartTroops()
    {
        foreach (MilitaryUnit unit in MilitaryUnit.ActiveUnits)
        {
            unit.KillImmediate();
        }

        foreach (MilitaryUnspawnedAlly m in unspawnedAllies)
        {
            m.Reset();
        }
    }

    public void OnTileMove(object sender, OnGridMoveArgs e)
    {
        MilitaryTurnManager.EndPlayerTurn();
    }
}
