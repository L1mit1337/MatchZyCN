using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;


namespace MatchZy
{

    public class Team 
    {
        public string id = "";
        public required string teamName;
        public string teamFlag = "";
        public string teamTag = "";

        public JToken? teamPlayers;

        public CCSPlayerController? coach;
        public int seriesScore = 0;
    }

    public partial class MatchZy
    {
        [ConsoleCommand("css_coach", "Sets coach for the requested team")]
        public void OnCoachCommand(CCSPlayerController? player, CommandInfo command) 
        {
            HandleCoachCommand(player, command.ArgString);
        }

        [ConsoleCommand("css_uncoach", "Sets coach for the requested team")]
        public void OnUnCoachCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (player == null || !player.PlayerPawn.IsValid) return;
            if (isPractice) {
                ReplyToUserCommand(player, "Uncoach 的指令只能使用在比赛模式中!");
                return;
            }

            Team matchZyCoachTeam;

            if (matchzyTeam1.coach == player) {
                matchZyCoachTeam = matchzyTeam1;
            }
            else if (matchzyTeam2.coach == player) {
                matchZyCoachTeam = matchzyTeam2;
            }
            else {
                ReplyToUserCommand(player, "你目前没有执教任何队伍!");
                return;
            }

            player.Clan = "";
            if (player.InGameMoneyServices != null) player.InGameMoneyServices.Account = 0;
            matchZyCoachTeam.coach = null;
            ReplyToUserCommand(player, "你目前没有执教任何队伍!!");
        }

        [ConsoleCommand("matchzy_addplayer", "Adds player to the provided team")]
        [ConsoleCommand("get5_addplayer", "Adds player to the provided team")]
        public void OnAddPlayerCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (player != null || command == null) return;
            if (!isMatchSetup) {
                command.ReplyToCommand("没有比赛设置完成!");
                return;
            }
            if (IsHalfTimePhase())
            {
                command.ReplyToCommand("玩家不能在半场休息中加入,请等待至回合开始.");
                return;
            }
            if (command.ArgCount < 3)
            {
                command.ReplyToCommand("请使用: matchzy_addplayertoteam <steam64> <team> \"<name>\"");
                return; 
            }

            string playerSteamId = command.ArgByIndex(1);
            string playerTeam = command.ArgByIndex(2);
            string playerName = command.ArgByIndex(3);
            bool success;
            if (playerTeam == "team1")
            {
                success = AddPlayerToTeam(playerSteamId, playerName, matchzyTeam1.teamPlayers);
            } else if (playerTeam == "team2")
            {
                success = AddPlayerToTeam(playerSteamId, playerName, matchzyTeam2.teamPlayers);
            } else if (playerTeam == "spec")
            {
                success = AddPlayerToTeam(playerSteamId, playerName, matchConfig.Spectators);
            } else 
            {
                command.ReplyToCommand("未知队伍: 必须是team1, team2, spec这三个其中一个");
                return; 
            }
            if (!success)
            {
                command.ReplyToCommand($"玩家 {playerName} 加入 {playerTeam} 失败! 他们可能已经加入了一个团队,或者您提供的Steam ID无效.");
                return;
            }
            command.ReplyToCommand($"玩家 {playerName} 成功加入 {playerTeam} !");
        }

        public void HandleCoachCommand(CCSPlayerController? player, string side) {
            if (player == null || !player.PlayerPawn.IsValid) return;
            if (isPractice) {
                ReplyToUserCommand(player, "教练相关指令只能使用在比赛当中!");
                return;
            }

            side = side.Trim().ToLower();

            if (side != "t" && side != "ct") {
                ReplyToUserCommand(player, "请使用: .coach t 或者 .coach ct");
                return;
            }

            if (matchzyTeam1.coach == player || matchzyTeam2.coach == player) 
            {
                ReplyToUserCommand(player, "你已经是某个队伍的教练了!");
                return;
            }

            Team matchZyCoachTeam;

            if (side == "t") {
                matchZyCoachTeam = reverseTeamSides["TERRORIST"];
            } else if (side == "ct") {
                matchZyCoachTeam = reverseTeamSides["CT"];
            } else {
                return;
            }

            if (matchZyCoachTeam.coach != null) {
                ReplyToUserCommand(player, "该队伍已经有教练了!");
                return;
            }

            matchZyCoachTeam.coach = player;
            player.Clan = $"[{matchZyCoachTeam.teamName} COACH]";
            if (player.InGameMoneyServices != null) player.InGameMoneyServices.Account = 0;
            ReplyToUserCommand(player, $"你现在是 {matchZyCoachTeam.teamName}的教练! 使用 .uncoach 停止执教");
            Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}{player.PlayerName}{ChatColors.Default} 正在执教 {ChatColors.Green}{matchZyCoachTeam.teamName}{ChatColors.Default}!");
        }

        public void HandleCoaches() 
        {
            List<CCSPlayerController?> coaches = new List<CCSPlayerController?>
            {
                matchzyTeam1.coach,
                matchzyTeam2.coach
            };

            foreach (var coach in coaches) 
            {
                if (coach == null) continue;
                Log($"Found coach: {coach.PlayerName}");
                coach.InGameMoneyServices!.Account = 0;
                AddTimer(0.5f, () => HandleCoachTeam(coach, true));
                // AddTimer(1, () => {
                    // Server.ExecuteCommand("mp_suicide_penalty 0; mp_death_drop_gun 0");
                    // coach.PlayerPawn.Value.CommitSuicide(false, true);
                    // Server.ExecuteCommand("mp_suicide_penalty 1; mp_death_drop_gun 1");
                // });
                coach.ActionTrackingServices!.MatchStats.Kills = 0;
                coach.ActionTrackingServices!.MatchStats.Deaths = 0;
                coach.ActionTrackingServices!.MatchStats.Assists = 0;
                coach.ActionTrackingServices!.MatchStats.Damage = 0;
            }
        }

        public bool AddPlayerToTeam(string steamId, string name, JToken? team)
        {
            if (matchzyTeam1.teamPlayers != null && matchzyTeam1.teamPlayers[steamId] != null) return false;
            if (matchzyTeam2.teamPlayers != null && matchzyTeam2.teamPlayers[steamId] != null) return false;
            if (matchConfig.Spectators != null && matchConfig.Spectators[steamId] != null) return false;

            if (team is JObject jObjectTeam)
            {
                jObjectTeam.Add(steamId, name);
                LoadClientNames();
                return true;
            }
            else if (team is JArray jArrayTeam)
            {
                jArrayTeam.Add(name);
                LoadClientNames();
                return true;
            }
            return false;
        }
    }
}
