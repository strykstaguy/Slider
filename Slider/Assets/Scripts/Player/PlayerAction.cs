using UnityEngine;

public class PlayerAction : Singleton<PlayerAction>
{
    public static System.EventHandler<System.EventArgs> OnAction;

    public Item pickedItem;

    [SerializeField] private Transform pickedItemLocation;
    [SerializeField] private Transform boatDropItemBaseTransform;
    [SerializeField] private GameObject itemDropIndicator;
    [SerializeField] private LayerMask itemMask;
    [SerializeField] private LayerMask dropCollidingMask;

    [SerializeField] private float minimumDropDistance = 0.5f;
    [SerializeField] private float maximumDropDistance = 1.5f;
    [SerializeField] private float dropCirclecastRadius = 0.5f;
    [SerializeField] private float dropBoxcastWidth = 0.75f; // 12/16
    [SerializeField] private float dropBoxcastHeight = 7f / 16f;

    private Item lastDroppedItem;
    private bool isPicking;  //Picking up animation is happening
    private bool isDropping; //Drop animation is happening
    private bool canDrop;

    private int actionsAvailable = 0;
    [SerializeField] private GameObject actionAvailableIndicator;

    private void Awake() 
    {
        InitializeSingleton(overrideExistingInstanceWith: this);

        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.Action, context => _instance.Action());
        Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.CycleEquip, context => _instance.CycleEquip());
    }

    private void Update()
    {
        pickedItem = PlayerInventory.GetCurrentItem();
        if (pickedItem != null && !isPicking)
        {
            // pickedItem.gameObject.transform.position = pickedItemLocation.position;

            Vector2 closestValidDropPosition = GetClosestValidDropPosition();
            
            // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
            Vector3 basePosition = GetPlayerTransformRaycastPosition();

            canDrop = Vector2.Distance(basePosition, closestValidDropPosition) > minimumDropDistance;
            itemDropIndicator.SetActive(canDrop);
            itemDropIndicator.transform.position = closestValidDropPosition;
        }
        else if (pickedItem == null)
        {
            itemDropIndicator.SetActive(false);
        }
    }

    private Vector2 GetClosestValidDropPosition()
    {
        Vector3 raycastDirection = Player.GetLastMoveDirection().normalized;


        // we offset where the raycast starts because when you're in the boat, the collider is at the boat not the player
        Vector3 basePosition = GetPlayerTransformRaycastPosition();

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            basePosition, 
            new Vector2(dropBoxcastWidth, dropBoxcastHeight), 
            Mathf.Atan2(raycastDirection.y, raycastDirection.x) * Mathf.Rad2Deg,
            raycastDirection, 
            maximumDropDistance,
            layerMask:dropCollidingMask
        );

        Vector2 closestPossibleDropPosition = basePosition + maximumDropDistance * raycastDirection;
        bool collisionOccured = false;

        foreach (RaycastHit2D hit in hits)
        {
            Collider2D hitCollider = hit.collider;
            STile stile = hitCollider.gameObject.GetComponent<STile>();

            // Hit an obstacle or an inactive STile
            if ((hitCollider.gameObject != pickedItem.gameObject && !hitCollider.isTrigger) || (stile != null && !stile.isTileActive))
            {
                collisionOccured = true;
                if (Vector2.Distance(hit.centroid, basePosition) < Vector2.Distance(closestPossibleDropPosition, basePosition))
                {
                    closestPossibleDropPosition = hit.centroid;
                }
            }
        }

        // Use maximum distance if there is no collision
        if (!collisionOccured)
        {
            Vector3 moveDir = Player.GetLastMoveDirection().normalized;
            closestPossibleDropPosition = basePosition + moveDir * maximumDropDistance;
        }

        return closestPossibleDropPosition;
    }

    public Vector3 GetPlayerTransformRaycastPosition()
    {
        if (!Player.GetInstance().GetIsOnWater())
        {
            return transform.position;
        }
        else
        {
            return boatDropItemBaseTransform.transform.position;
        }
    }

    private void Action() 
    {
        if (UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen())
            return;

        if (!Player.GetCanMove())
            return;

        if (TryPickOrDrop())
            return; // if succesfully picked something up, return

        OnAction?.Invoke(this, new System.EventArgs());
    }

    private void CycleEquip()
    {
        if (isPicking || UIManager.IsUIOpen() || UIArtifactMenus.IsArtifactOpen()) 
        {
            return;
        }
        
        if (pickedItem == null || pickedItem.canKeep)
        {
            PlayerInventory.NextItem();
        }
        else
        {
            AudioManager.Play("Artifact Error");
        }
    }

    public bool TryPickOrDrop() 
    {
        if (isPicking || isDropping) 
        {
            return false;
        }

        Collider2D[] nodes = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 1f, itemMask);
        if (pickedItem == null) 
        {
            //Try picking an item up
            if (nodes.Length > 0)
            {
                isPicking = true;

                // find nearest item
                Collider2D nearest = nodes[0];
                float nearestDist = Vector3.Distance(nearest.transform.position, transform.position);
                for (int i = 1; i < nodes.Length; i++) {
                    if (Vector3.Distance(nodes[i].transform.position, transform.position) < nearestDist) {
                        nearest = nodes[i];
                        nearestDist = Vector3.Distance(nodes[i].transform.position, transform.position);
                    }
                }
                
                pickedItem = nearest.GetComponent<Item>();  //Not GetComponentInParent?
                if (pickedItem == null) {
                    Debug.LogError("Picked something that isn't an Item!");
                }

                PlayerInventory.AddItem(pickedItem);
                pickedItem.PickUpItem(pickedItemLocation.transform, callback:FinishPicking);

                return true;
            } 
        }
        else
        {
            // Try dropping the item
            if (canDrop) 
            {
                isDropping = true;
                PlayerInventory.RemoveItem();
                pickedItem.DropItem(itemDropIndicator.transform.position, callback:FinishDropping);
                lastDroppedItem = pickedItem;
                pickedItem = null;
                itemDropIndicator.SetActive(false);

                return true;
            }
        }

        return false;
    }

    private void FinishPicking() 
    {
        isPicking = false;
        itemDropIndicator.SetActive(true);
        
        pickedItem.transform.SetParent(pickedItemLocation);
    }

    private void FinishDropping()
    {
        lastDroppedItem.dropCallback();
        isDropping = false;
        itemDropIndicator.SetActive(false);
    }

    public bool HasItem()
    {
        return pickedItem != null;
    }
    public bool HasItem(string itemName)
    {
        return pickedItem != null && pickedItem.itemName.Equals(itemName);
    }

    public void IncrementActionsAvailable()
    {
        actionsAvailable += 1;
        if (actionsAvailable > 0)
        {
            actionAvailableIndicator.SetActive(true);
        }
    }

    public void DecrementActionsAvailable()
    {
        actionsAvailable -= 1;
        if (actionsAvailable <= 0)
        {
            actionAvailableIndicator.SetActive(false);
            actionsAvailable = 0;
        }
    }
}