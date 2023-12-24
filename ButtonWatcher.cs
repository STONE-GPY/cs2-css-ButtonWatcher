using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace ButtonWatcher;

[MinimumApiVersion(80)]
public class ButtonWatcherPlugin : BasePlugin
{
    public override string ModuleName => "ButtonWatcher";
    public override string ModuleVersion => "1";
    public override string ModuleAuthor => "石";
    public override string ModuleDescription => "Watcher for when func_button pressed";

    public override void Load(bool hotReload)
    {
        HookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Post);
    }

    public HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {   
        CEntityIdentity? buttonEntity = caller.Entity;
        if(buttonEntity == null)
            return HookResult.Continue;
            
        int pawnIdx = (int)activator.Index;
        CCSPlayerPawn? playerPawn =  Utilities.GetEntityFromIndex<CCSPlayerPawn>(pawnIdx);
        if(playerPawn == null || !playerPawn.IsValid || playerPawn.OriginalController == null || !playerPawn.OriginalController.IsValid)
            return HookResult.Continue;

        CCSPlayerController? playerController = playerPawn.OriginalController.Value;

        if(playerController == null || !playerController.IsValid)
            return HookResult.Continue;

        string sPlayerName = playerController.PlayerName;
        ulong sSteamID = playerController.SteamID;
        int? sUserID = playerController.UserId;
        string sButtonEntityName = buttonEntity.Name;
        uint iButtonEntityIndex = caller.Index;

        CCSGameRules gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

        int iTime = gameRules.RoundTime - (int)(gameRules.LastThinkTime-gameRules.RoundStartTime);
        int iMin = iTime/60;
        int iSec = iTime%60;
        Server.PrintToChatAll($"[BW] {ChatColors.White}[{ChatColors.LightRed}{iMin}:{iSec}{ChatColors.White}]{ChatColors.Lime}{sPlayerName}{ChatColors.White}[{ChatColors.Orange}{sSteamID}{ChatColors.White}][{ChatColors.Lime}#{sUserID}{ChatColors.White}] 이 {ChatColors.LightRed}{sButtonEntityName}[#{iButtonEntityIndex}] {ChatColors.White}를 눌렀습니다!");
        Logger.LogInformation($"[BW] [{iMin}:{iSec}]{sPlayerName}[{sSteamID}][#{sUserID}] 이 {sButtonEntityName}[#{iButtonEntityIndex}] 를 눌렀습니다!");

        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        UnhookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Post);
    }
}
