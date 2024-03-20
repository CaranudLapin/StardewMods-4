using ItemExtensions.Models;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace ItemExtensions.Patches;

public partial class ShopMenuPatches
{
    private static Dictionary<ISalable, List<ExtraTrade>> ExtraBySalable { get; set; }
    private static Dictionary<string, List<ExtraTrade>> ByQualifiedId { get; set; }
    internal static bool Pre_receiveLeftClick(ShopMenu __instance, int x, int y, bool playSound = true)
    {
        if (__instance.safetyTimer > 0)
            return true;
        
        //if no data in neither
        if(ExtraBySalable is not { Count: > 0 } && ByQualifiedId is not { Count: > 0 })
            return true;
        
        for (var index1 = 0; index1 < __instance.forSaleButtons.Count; ++index1)
        {
            if (__instance.currentItemIndex + index1 >= __instance.forSale.Count ||
                !__instance.forSaleButtons[index1].containsPoint(x, y)) 
                continue;
            
            var index2 = __instance.currentItemIndex + index1;
            var thisItem = __instance.forSale[index2];

            //if holding item AND not same as one buying
            if (__instance.heldItem != null && __instance.heldItem.QualifiedItemId != thisItem.QualifiedItemId)
                return true;
            
            //if neither this trade OR this id is found
            if (!ExtraBySalable.ContainsKey(thisItem) && !ByQualifiedId.ContainsKey(thisItem.QualifiedItemId))
                return true;
            
            if (__instance.forSale[index2] != null)
            {
                var val1 = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, __instance.currency) / Math.Max(1, __instance.itemPriceAndStock[__instance.forSale[index2]].Price)), Math.Max(1, __instance.itemPriceAndStock[__instance.forSale[index2]].Stock)) : 1;
                
                if (__instance.ShopId == "ReturnedDonations")
                    val1 = __instance.itemPriceAndStock[__instance.forSale[index2]].Stock;
                
                var stockToBuy = Math.Min(val1, __instance.forSale[index2].maximumStackSize());
                if (stockToBuy == -1)
                    stockToBuy = 1;
                
                if (__instance.canPurchaseCheck != null && !__instance.canPurchaseCheck(index2))
                    return true;
                
                var valid = CanPurchase(__instance.forSale[index2], stockToBuy);
                
                if (stockToBuy > 0 && valid)
                {
                    var tryPurchase = Reflection.GetMethod(__instance, "tryToPurchaseItem");
                    tryPurchase.Invoke<bool>(__instance.forSale[index2], __instance.heldItem, stockToBuy, x, y);
                    ReduceExtraItems(__instance, __instance.forSale[index2], stockToBuy);
                }
                else
                {
                    if (__instance.itemPriceAndStock[__instance.forSale[index2]].Price > 0)
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    Game1.playSound("cancel");
                    return false;
                }

                var storageShop = Reflection.GetField<bool>(__instance, "_isStorageShop").GetValue();
                
                if (__instance.heldItem != null && (storageShop || Game1.options.SnappyMenus || Game1.oldKBState.IsKeyDown(Keys.LeftShift) && __instance.heldItem.maximumStackSize() == 1) && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(__instance.heldItem as Item))
                {
                    __instance.heldItem = null;
                    DelayedAction.playSoundAfterDelay("coin", 100);
                }
            }
            __instance.currentItemIndex = Math.Max(0, Math.Min(__instance.forSale.Count - 4, __instance.currentItemIndex));
            __instance.updateSaleButtonNeighbors();
            Reflection.GetMethod(__instance, "setScrollBarToCurrentIndex").Invoke();
            return false;
        }
        
        return true;
    }

    private static List<ExtraTrade> GetData(ISalable item)
    {
        List<ExtraTrade> data = null;
        //if extrabysalable has no data
        if (ExtraBySalable is not { Count: > 0 })
        {
            //check if id has
            if (ByQualifiedId is not { Count: > 0 })
                return null;
        }
        
        
        //if data wasn't in salable
        if (ExtraBySalable.TryGetValue(item, out data) == false)
        {
            //try fallback to Id
            if (ByQualifiedId.TryGetValue(item.QualifiedItemId, out data) == false)
                return null;
        }

        return data;
    }
    
    private static bool CanPurchase(ISalable item, int stockToBuy)
    {
        if (Game1.player.Money < item.salePrice() * stockToBuy)
            return false;
        
        #if DEBUG
        Log($"item: {item.DisplayName}, stockToBuy {stockToBuy}, in _extraSaleItems {ExtraBySalable.Count}");
        #endif

        var data = GetData(item);
        
        if (data is null)
            return false;

        foreach (var extra in data)
        {
            if (HasMatch(Game1.player, extra, stockToBuy))
                continue;
            
            return false;
        }

        return true;
    }

    private static void ReduceExtraItems(ShopMenu __instance, ISalable item, int stockToBuy)
    {
        Log($"Buying {stockToBuy} {item.DisplayName}...");
        
        var data = GetData(item);
        
        if (data is null)
            return;

        foreach (var extra in data)
        {
            Log($"Reducing {extra.Data.DisplayName} by {extra.Count * stockToBuy}...");
            Game1.player.Items.ReduceId(extra.QualifiedItemId, extra.Count * stockToBuy);
        }
    }
    
    private static bool HasMatch(Farmer farmer, ExtraTrade c, int bought = 1)
    {
        var all = farmer.Items.GetById(c.QualifiedItemId);
        var total = 0;

        foreach (var item in all)
        {
            if (item.QualifiedItemId != c.QualifiedItemId)
                continue;

            total += item.Stack;
        }

        var withStock = c.Count * bought;
        return total >= withStock;
    }
}