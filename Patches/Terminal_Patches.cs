using HarmonyLib;
using System;
using System.Collections.Generic;
using static TerminalApi.TerminalApi;
using System.Text;
using System.Reflection.Emit;
using PortableMultiTool.Util;

namespace PortableMultiTool.Patches;

[HarmonyPatch(typeof(Terminal))]
public class Terminal_Patches
{
    /*
    private static readonly Dictionary<TerminalNode, TerminalActionData> commandCallbacks = [];

    private class TerminalActionData
    {
        public TerminalKeyword Keyword { get; set; }
        public Func<string> ResponseProvider { get; set; }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
    static void ParsePlayerSentence_Postfix(ref TerminalNode __result)
    {
        if (__result  == null) return;
        if (commandCallbacks.TryGetValue(__result, out var actionData))
        {
            PortableMultiToolBase.Instance.Logger.LogInfo($"Invoking custom handler for keyword: {actionData.Keyword.word}");
            __result.displayText = actionData.ResponseProvider();
        }
        else
        {
            PortableMultiToolBase.Instance.Logger.LogInfo($"No custom callback found for command");
        }
    }

    public static void AddCommand(string command, Func<string> responseSupplier)
    {
        command = command.ToLower();
        TerminalKeyword mainKeyword = CreateTerminalKeyword(command);
        TerminalNode triggerNode = CreateTerminalNode("", true);
        mainKeyword.specialKeywordResult = triggerNode;
        AddTerminalKeyword(mainKeyword);
        commandCallbacks.Add(triggerNode, new TerminalActionData
        {
            Keyword = mainKeyword,
            ResponseProvider = responseSupplier
        });
    }
    */

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Terminal.LoadNewNodeIfAffordable))]
    [HarmonyPatch(nameof(Terminal.OnSubmit))]
    [HarmonyPatch(nameof(Terminal.TextPostProcess))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
    {
        var itemCostFieldInfo = AccessTools.Field(typeof(TerminalNode), nameof(TerminalNode.itemCost));
        var getTerminalNodeCostMemthodInfo = AccessTools.Method(typeof(ShopUtils), nameof(ShopUtils.GetTerminalNodeCost));

        var creditsWorthFieldInfo = AccessTools.Field(typeof(Item), nameof(Item.creditsWorth));
        var getItemCreditsWorthMethodInfo = AccessTools.Method(typeof(ShopUtils), nameof(ShopUtils.GetItemCreditsWorth));

        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldfld && instruction.operand == itemCostFieldInfo)
            {
                yield return new CodeInstruction(OpCodes.Call, getTerminalNodeCostMemthodInfo);
            }
            else if (instruction.opcode == OpCodes.Ldfld && instruction.operand == creditsWorthFieldInfo)
            {
                yield return new CodeInstruction(OpCodes.Call, getItemCreditsWorthMethodInfo);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}
