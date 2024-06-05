using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: serialize
public class MilitaryUnspawnedAlly : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public List<GameObject> npcBoxes = new();
    public GameObject spawnConfirmer;
    public MilitarySTile parentStile;
    public GameObject militaryAllyPrefab;

    private MilitaryUnit.Type type = MilitaryUnit.Type.Rock;
    
    private void OnEnable()
    {
        
        if (parentStile.islandId % 2 == 0)
        {
            spawnConfirmer.SetActive(false);
            gameObject.SetActive(false);
        }

        SetUnitType((MilitaryUnit.Type)Random.Range(0, 3));

        UITrackerManager.AddNewTracker(gameObject);
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        spawnConfirmer.SetActive(true);
        foreach (GameObject g in npcBoxes)
        {
            g.SetActive(true);
        }
    }
    
    public void SetUnitType(MilitaryUnit.Type type)
    {
        this.type = type;
        
        MilitarySpriteTable militarySpriteTable = (SGrid.Current as MilitaryGrid).militarySpriteTable;

        Sprite flagSprite = militarySpriteTable.GetFlagSpriteForType(type);
        spriteRenderer.sprite = flagSprite;
    }

    public void CycleUnitType()
    {
        SetUnitType((MilitaryUnit.Type)(((int)type + 1) % 3));
    }

    public void SpawnUnit()
    {
        GameObject go = Instantiate(militaryAllyPrefab, transform.position, Quaternion.identity, transform.parent);
        MilitaryUnit unit = go.GetComponent<MilitaryUnit>();
        unit.InitializeNewUnit(type);
        unit.AttachedSTile = parentStile;

        spawnConfirmer.SetActive(false);
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, spawnConfirmer.transform.position, transform.parent);
        foreach (GameObject g in npcBoxes)
        {
            g.SetActive(false);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, g.transform.position, transform.parent);
        }

        StartCoroutine(DoSpawnSound(() => gameObject.SetActive(false)));

        // gameObject.SetActive(false);
    }

    private IEnumerator DoSpawnSound(System.Action callback)
    {
        for (int i = 0; i < 3; i++)
        {
            AudioManager.PlayWithVolume("Hat Click", 0.5f - 0.1f * i);
            yield return new WaitForSeconds(0.1f);
        }

        callback?.Invoke();
    }

    public void IsRock(Condition c) => c.SetSpec(type == MilitaryUnit.Type.Rock);
    public void IsPaper(Condition c) => c.SetSpec(type == MilitaryUnit.Type.Paper);
    public void IsScissors(Condition c) => c.SetSpec(type == MilitaryUnit.Type.Scissors);
}