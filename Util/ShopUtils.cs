using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Items;

namespace PortableMultiTool.Util;

public static class ShopUtils
{
    private readonly static Dictionary<Item, Func<int>> itemCreditsWorthMap = [];
    private readonly static Dictionary<TerminalNode, Func<int>> terminalNodeCostMap = [];

    public static void RegisterDynamicShopItem(Item item, Func<int> costProvider)
    {
        var terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
        terminalNode.name = item.itemName.Replace(" ", "-") + "BuyNode1";
        terminalNode.displayText = "You have requested to order " + item.itemName + ". Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
        terminalNode.clearPreviousText = true;
        terminalNode.maxCharactersToType = 35;

        itemCreditsWorthMap.Add(item, costProvider);
        terminalNodeCostMap.Add(terminalNode, costProvider);

        RegisterShopItem(item, buyNode1: terminalNode);
    }

    public static int GetTerminalNodeCost(TerminalNode node)
    {
        if (terminalNodeCostMap.TryGetValue(node, out var value))
        {
            return value();
        }
        else
        {
            return node.itemCost;
        }
    }

    public static int GetItemCreditsWorth(Item item)
    {
        if (itemCreditsWorthMap.TryGetValue(item, out var value))
        {
            return value();
        }
        else
        {
            return item.creditsWorth;
        }
    }
}
