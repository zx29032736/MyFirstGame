using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

public class ItemViewerController : MonoBehaviour {

    public Text CurrentItemName;

    public Button CloseButton;

    public List<CatalogItem> items = new List<CatalogItem>();
    public ItemViewerDisplayItem[] UnpackedItemPrefab;
    public Transform ItemGroup;
    public CatalogItem selectedItem;


    public void InitiateViewer(List<string> items, bool unpackToPlayer)
    {
        foreach (var prefab in UnpackedItemPrefab)
            prefab.gameObject.SetActive(false);
        this.items.Clear();
        CheckUnlock(items[0]);

    }


    public void CheckUnlock(string id)
    {
        string item = selectedItem.ItemId;

        PF_GameData.TryOpenContainer(id, null, AfterUnlock);
        
    }


    public void AfterUnlock(UnlockContainerItemResult result)
    {

        // build our list for displaying the container results
        List<ContainerResultItem> items = new List<ContainerResultItem>();
        int counts = 0;
        foreach (var award in result.GrantedItems)
        {
            string awardIcon = "Default";
            CatalogItem catItem = PF_GameData.catalogItems.Find((i) => { return i.ItemId == award.ItemId; });
            Dictionary<string, string> kvps = PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(catItem.CustomData);
            //kvps.TryGetValue("icon", out awardIcon);

            items.Add(new ContainerResultItem()
            {
                displayIcon = new Sprite(),//GameController.Instance.iconManager.GetIconById(awardIcon),
                displayName = award.DisplayName
            });

            if (counts < 5)
            {
                UnpackedItemPrefab[counts].gameObject.SetActive(true);
                UnpackedItemPrefab[counts].BtnInitialize(new Sprite(), award.DisplayName, (int)award.UsesIncrementedBy);
            }
            else
                return;

            counts++;
        }

        if (result.VirtualCurrency != null)
        {
            foreach (var award in result.VirtualCurrency)
            {
                items.Add(new ContainerResultItem()
                {
                    displayIcon = new Sprite(),//GameController.Instance.iconManager.GetIconById(award.Key),
                    displayName = string.Format("{1} Award: {0}", award.Value, award.Key)
                });

                if (counts < 5)
                {
                    UnpackedItemPrefab[counts].gameObject.SetActive(true);
                    UnpackedItemPrefab[counts].BtnInitialize(new Sprite(), award.Key, (int)award.Value);
                }
                else
                    return;

                counts++;
            }
            PF_PlayerData.GetUserAccountInfo();
        }
        else
        {
            Debug.LogError("check plz");
            //CatalogItem catRef = PF_GameData.catalogItems.Find((i) => { return i.ItemId == this.selectedItem.ItemId; });
            //if (catRef.Container.VirtualCurrencyContents.Count > 0)
            //{
            //    foreach (var vc in catRef.Container.VirtualCurrencyContents)
            //    {
            //        items.Add(new ContainerResultItem()
            //        {
            //            displayIcon = GameController.Instance.iconManager.GetIconById(vc.Key),
            //            displayName = string.Format("{1} Award: {0}", vc.Value, vc.Key)
            //        });
            //    }
            //}
        }

        gameObject.SetActive(true);

        DialogCanvasController.RequestInventoryPrompt();

    }

    public void CloseInventory()
    {
        GameController.Instance.soundManager.PlaySound(Vector3.zero, GlobalStrings.BUTTON_LEAVE_SOUND_EFFECT);

        for(int i = 0; i < UnpackedItemPrefab.Length; i++)
        {
            UnpackedItemPrefab[i].gameObject.SetActive(false);
        }
        // get this to close down and also close the tint.	
        // get a confirmation here
        this.gameObject.SetActive(false);
    }

    public class ContainerResultItem
    {
        public Sprite displayIcon;
        public string displayName;
    }
}
