using HarmonyLib;
using System;
using System.Collections.Generic;
using static TerminalApi.TerminalApi;
using System.Text;

namespace PortableMultiTool.Patches;

[HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
public class Terminal_Patches
{
    private static readonly Dictionary<TerminalNode, TerminalActionData> commandCallbacks = [];

    private class TerminalActionData
    {
        public TerminalKeyword Keyword { get; set; }
        public Func<string> ResponseProvider { get; set; }
    }

    static void Postfix(ref TerminalNode __result)
    {
        if (commandCallbacks.TryGetValue(__result, out var actionData))
        {
            PortableMultiToolBase.Instance.Logger.LogWarning($"Invoking custom handler for keyword: {actionData.Keyword.word}");
            __result.displayText = actionData.ResponseProvider();
        }
        else
        {
            PortableMultiToolBase.Instance.Logger.LogWarning($"No custom callback found for command");
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
}
