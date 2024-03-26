using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text.RegularExpressions;

namespace MatchZy
{
    public partial class MatchZy
    {
        [ConsoleCommand("css_whitelist", "Toggles Whitelisting of players")]
        public void OnWLCommand(CCSPlayerController? player, CommandInfo? command) {            
            if (IsPlayerAdmin(player, "css_whitelist", "@css/config")) {
                isWhitelistRequired = !isWhitelistRequired;
                string WLStatus = isWhitelistRequired ? "开启" : "关闭";
                if (player == null) {
                    ReplyToUserCommand(player, $"白名单功能状态现在是 {WLStatus} 的!");
                } else {
                    player.PrintToChat($"{chatPrefix} 白名单功能状态现在是 {ChatColors.Green}{WLStatus}{ChatColors.Default} 的!");
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_save_nades_as_global", "Toggles Global Lineups for players")]
        public void OnSaveNadesAsGlobalCommand(CCSPlayerController? player, CommandInfo? command) {            
            if (IsPlayerAdmin(player, "css_save_nades_as_global", "@css/config")) {
                isSaveNadesAsGlobalEnabled = !isSaveNadesAsGlobalEnabled;
                string GlobalNadesStatus = isSaveNadesAsGlobalEnabled ? "是" : "否";
                if (player == null) {
                    ReplyToUserCommand(player, $"是否全局保存/加载阵容 {GlobalNadesStatus}!");
                } else {
                    player.PrintToChat($"{chatPrefix} 是否全局保存/加载阵容 {ChatColors.Green}{GlobalNadesStatus}{ChatColors.Default}!");
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }
        
        [ConsoleCommand("css_ready", "Marks the player ready")]
        public void OnPlayerReady(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;
            Log($"[!ready command] Sent by: {player.UserId} readyAvailable: {readyAvailable} matchStarted: {matchStarted}");
            if (readyAvailable && !matchStarted) {
                if (player.UserId.HasValue) {
                    if (!playerReadyStatus.ContainsKey(player.UserId.Value)) {
                        playerReadyStatus[player.UserId.Value] = false;
                    }
                    if (playerReadyStatus[player.UserId.Value]) {
                        player.PrintToChat($"{chatPrefix} 您已经准备了!");
                    } else {
                        playerReadyStatus[player.UserId.Value] = true;
                        player.PrintToChat($"{chatPrefix} 您已准备!");
                    }
                    CheckLiveRequired();
                    HandleClanTags();
                }
            }
        }

        [ConsoleCommand("css_unready", "Marks the player unready")]
        public void OnPlayerUnReady(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;
            Log($"[!unready command] {player.UserId}");
            if (readyAvailable && !matchStarted) {
                if (player.UserId.HasValue) {
                    if (!playerReadyStatus.ContainsKey(player.UserId.Value)) {
                        playerReadyStatus[player.UserId.Value] = false;
                    }
                    if (!playerReadyStatus[player.UserId.Value]) {
                        player.PrintToChat($"{chatPrefix} 您还没有准备!");
                    } else {
                        playerReadyStatus[player.UserId.Value] = false;
                        player.PrintToChat($"{chatPrefix} 您取消准备了!");
                    }
                    HandleClanTags();
                }
            }
        }

        [ConsoleCommand("css_stay", "Stays after knife round")]
        public void OnTeamStay(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;
            
            Log($"[!stay command] {player.UserId}, TeamNum: {player.TeamNum}, knifeWinner: {knifeWinner}, isSideSelectionPhase: {isSideSelectionPhase}");
            if (isSideSelectionPhase) {
                if (player.TeamNum == knifeWinner) {
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}{knifeWinnerName} 选择了 {ChatColors.Green} 保持原先阵营{ChatColors.Default} !");
                    StartLive();
                }
            }
        }

        [ConsoleCommand("css_switch", "Switch after knife round")]
        [ConsoleCommand("css_swap", "Switch after knife round")]
        public void OnTeamSwitch(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;
            
            Log($"[!switch command] {player.UserId}, TeamNum: {player.TeamNum}, knifeWinner: {knifeWinner}, isSideSelectionPhase: {isSideSelectionPhase}");
            if (isSideSelectionPhase) {
                if (player.TeamNum == knifeWinner) {
                    Server.ExecuteCommand("mp_swapteams;");
                    SwapSidesInTeamData(true);
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}{knifeWinnerName} 选择了 {ChatColors.Red} 交换阵营{ChatColors.Default} !");
                    StartLive();
                }
            }
        }

        [ConsoleCommand("css_tech", "Pause the match")]
        public void OnTechCommand(CCSPlayerController? player, CommandInfo? command) {            
            PauseMatch(player, command);
        }

        [ConsoleCommand("css_pause", "Pause the match")]
        public void OnPauseCommand(CCSPlayerController? player, CommandInfo? command) {     
            if (isPauseCommandForTactical)
            {
                OnTacCommand(player, command);
            }   
            else 
            {
                PauseMatch(player, command);
            }    
        }

        [ConsoleCommand("css_fp", "Pause the match an admin")]
        [ConsoleCommand("css_forcepause", "Pause the match as an admin")]
        [ConsoleCommand("sm_pause", "Pause the match as an admin")]
        public void OnForcePauseCommand(CCSPlayerController? player, CommandInfo? command) {            
            ForcePauseMatch(player, command);
        }

        [ConsoleCommand("css_fup", "Unpause the match an admin")]
        [ConsoleCommand("css_forceunpause", "Unpause the match as an admin")]
        [ConsoleCommand("sm_unpause", "Unpause the match as an admin")]
        public void OnForceUnpauseCommand(CCSPlayerController? player, CommandInfo? command) {            
            ForceUnpauseMatch(player, command);
        }

        [ConsoleCommand("css_unpause", "Unpause the match")]
        public void OnUnpauseCommand(CCSPlayerController? player, CommandInfo? command) {
            if (isMatchLive && isPaused) {
                var pauseTeamName = unpauseData["pauseTeam"];
                if ((string)pauseTeamName == "Admin") {
                    player?.PrintToChat($"{chatPrefix} 比赛已被管理员暂停，因此只能由管理员取消暂停.");
                    return;
                }

                string unpauseTeamName = "Admin";
                string remainingUnpauseTeam = "Admin";
                if (player?.TeamNum == 2) {
                    unpauseTeamName = reverseTeamSides["TERRORIST"].teamName;
                    remainingUnpauseTeam = reverseTeamSides["CT"].teamName;
                    if (!(bool)unpauseData["t"]) {
                        unpauseData["t"] = true;
                    }
                    
                } else if (player?.TeamNum == 3) {
                    unpauseTeamName = reverseTeamSides["CT"].teamName;
                    remainingUnpauseTeam = reverseTeamSides["TERRORIST"].teamName;
                    if (!(bool)unpauseData["ct"]) {
                        unpauseData["ct"] = true;
                    }
                } else {
                    return;
                }
                if ((bool)unpauseData["t"] && (bool)unpauseData["ct"]) {
                    Server.PrintToChatAll($"{chatPrefix} 双方均未暂停比赛，比赛继续！");
                    Server.ExecuteCommand("mp_unpause_match;");
                    isPaused = false;
                    unpauseData["ct"] = false;
                    unpauseData["t"] = false;
                } else if (unpauseTeamName == "Admin") {
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}{unpauseTeamName}{ChatColors.Default} 取消了暂停,比赛继续!");
                    Server.ExecuteCommand("mp_unpause_match;");
                    isPaused = false;
                    unpauseData["ct"] = false;
                    unpauseData["t"] = false;
                } else {
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}{unpauseTeamName}{ChatColors.Default} 想要结束暂停. {ChatColors.Green}{remainingUnpauseTeam}{ChatColors.Default},输入!unpause 以同意取消暂停.");
                }
                if (!isPaused && pausedStateTimer != null) {
                    pausedStateTimer.Kill();
                    pausedStateTimer = null;
                }
            }
        }

        [ConsoleCommand("css_tac", "Starts a tactical timeout for the requested team")]
        public void OnTacCommand(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;
            
            if (matchStarted && isMatchLive) {
                Log($"[.tac command sent via chat] Sent by: {player.UserId}, connectedPlayers: {connectedPlayers}");
                if (isPaused)
                {
                    ReplyToUserCommand(player, "比赛已经暂停，无法发起技术暂停！");
                    return;
                }
                var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
                if (player.TeamNum == 2) {
                    if (gameRules.TerroristTimeOuts > 0) {
                        Server.ExecuteCommand("timeout_terrorist_start");
                    } else {
                        ReplyToUserCommand(player, "你已经没有发起技术暂停的次数了!");
                    }
                } else if (player.TeamNum == 3) {
                    if (gameRules.CTTimeOuts > 0) {
                        Server.ExecuteCommand("timeout_ct_start");
                    } else {
                        ReplyToUserCommand(player, "你已经没有发起技术暂停的次数了!");
                    }
                } 
            }
        }

        [ConsoleCommand("css_roundknife", "Toggles knife round for the match")]
        [ConsoleCommand("css_rk", "Toggles knife round for the match")]
        public void OnKnifeCommand(CCSPlayerController? player, CommandInfo? command) {            
            if (IsPlayerAdmin(player, "css_roundknife", "@css/config")) {
                isKnifeRequired = !isKnifeRequired;
                string knifeStatus = isKnifeRequired ? "开启" : "关闭";
                if (player == null) {
                    ReplyToUserCommand(player, $"是否开启刀局: {knifeStatus}!");
                } else {
                    player.PrintToChat($"{chatPrefix} 是否开启刀局: {ChatColors.Green}{knifeStatus}{ChatColors.Default}!");
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_readyrequired", "Sets number of ready players required to start the match")]
        public void OnReadyRequiredCommand(CCSPlayerController? player, CommandInfo command) {
            if (IsPlayerAdmin(player, "css_readyrequired", "@css/config")) {
                if (command.ArgCount >= 2) {
                    string commandArg = command.ArgByIndex(1);
                    HandleReadyRequiredCommand(player, commandArg);
                }
                else {
                    string minimumReadyRequiredFormatted = (player == null) ? $"{minimumReadyRequired}" : $"{ChatColors.Green}{minimumReadyRequired}{ChatColors.Default}";
                    ReplyToUserCommand(player, $"当前最少准备人数: {minimumReadyRequiredFormatted} .输入: !readyrequired <number_of_ready_players_required>来设置人数");
                }                
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_settings", "Shows the current match configuration/settings")]
        public void OnMatchSettingsCommand(CCSPlayerController? player, CommandInfo? command) {
            if (player == null) return;

            if (IsPlayerAdmin(player, "css_settings", "@css/config")) {
                string knifeStatus = isKnifeRequired ? "是" : "否";
                string playoutStatus = isPlayOutEnabled ? "是" : "否";
                player.PrintToChat($"{chatPrefix} 当前设置:");
                player.PrintToChat($"{chatPrefix} 是否开启刀局: {ChatColors.Green}{knifeStatus}{ChatColors.Default}");
                if (isMatchSetup)
                {
                    player.PrintToChat($"{chatPrefix} 至少要有 {ChatColors.Green}{matchConfig.MinPlayersToReady} 名已准备的玩家(每支队伍)！{ChatColors.Default}");
                    player.PrintToChat($"{chatPrefix} 至少要有 {ChatColors.Green}{matchConfig.MinSpectatorsToReady} 名观察者！{ChatColors.Default}");
                }
                else
                {
                    player.PrintToChat($"{chatPrefix} 至少要有 {ChatColors.Green}{minimumReadyRequired} 人准备！{ChatColors.Default}");
                }
                player.PrintToChat($"{chatPrefix} Playout: {ChatColors.Green}{playoutStatus}{ChatColors.Default}");
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_endmatch", "Ends and resets the current match")]
        [ConsoleCommand("get5_endmatch", "Ends and resets the current match")]
        public void OnEndMatchCommand(CCSPlayerController? player, CommandInfo? command) {
            if (IsPlayerAdmin(player, "css_endmatch", "@css/config")) {
                if (!isPractice) {
                    Server.PrintToChatAll($"{chatPrefix} 管理员强制结束了比赛");
                    ResetMatch();
                } else {
                    ReplyToUserCommand(player, "练习模式已开启，不能进行比赛！");
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_restart", "Restarts the match")]
        public void OnRestartMatchCommand(CCSPlayerController? player, CommandInfo? command) {
            if (IsPlayerAdmin(player, "css_restart", "@css/config")) {
                if (!isPractice) {
                    ResetMatch();
                } else {
                    ReplyToUserCommand(player, "练习模式已开启，不能进行比赛！");
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_map", "Changes the map using changelevel")]
        public void OnChangeMapCommand(CCSPlayerController? player, CommandInfo command) {
            var mapName = command.ArgByIndex(1);
            HandleMapChangeCommand(player, mapName);
        }

        [ConsoleCommand("css_rmap", "Reloads the current map")]
        private void OnMapReloadCommand(CCSPlayerController? player, CommandInfo? command) {

            if (!IsPlayerAdmin(player)) {
                SendPlayerNotAdminMessage(player);
                return;
            }
            string currentMapName = Server.MapName;
            if (long.TryParse(currentMapName, out _)) { // Check if mapName is a long for workshop map ids
                Server.ExecuteCommand($"bot_kick");
                Server.ExecuteCommand($"host_workshop_map \"{currentMapName}\"");
            } else if (Server.IsMapValid(currentMapName)) {
                Server.ExecuteCommand($"bot_kick");
                Server.ExecuteCommand($"changelevel \"{currentMapName}\"");
            } else {
                ReplyToUserCommand(player, "无效的地图名!");
            }
        }

        [ConsoleCommand("css_start", "Force starts the match")]
        public void OnStartCommand(CCSPlayerController? player, CommandInfo? command) {
            if (IsPlayerAdmin(player, "css_start", "@css/config")) {
                if (isPractice) {
                    ReplyToUserCommand(player, "在练习模式中不能开启一场比赛，请先退出练习模式！输入 .exitprac 可退出练习模式.");
                    return;
                }
                if (matchStarted) {
                    ReplyToUserCommand(player, "开启比赛的指令不能使用在已开始的比赛中! 如果你想暂停，请输入 .unpause");
                } else {
                    Server.PrintToChatAll($"{chatPrefix} {ChatColors.Green}管理员{ChatColors.Default} 开启了比赛!");
                    HandleMatchStart();
                }
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_asay", "Say as an admin")]
        public void OnAdminSay(CCSPlayerController? player, CommandInfo? command) {
            if (command == null) return;
            if (player == null) {
                Server.PrintToChatAll($"{adminChatPrefix} {command.ArgString}");
                return;
            }
            if (!IsPlayerAdmin(player, "css_asay", "@css/chat")) {
                SendPlayerNotAdminMessage(player);
                return;
            }
            string message = "";
            for (int i = 1; i < command.ArgCount; i++) {
                message += command.ArgByIndex(i) + " ";
            }
            Server.PrintToChatAll($"{adminChatPrefix} {message}");
        }

        [ConsoleCommand("reload_admins", "Reload admins of MatchZy")]
        public void OnReloadAdmins(CCSPlayerController? player, CommandInfo? command) {
            if (IsPlayerAdmin(player, "reload_admins", "@css/config")) {
                LoadAdmins();
                UpdatePlayersMap();
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("css_match", "Starts match mode")]
        public void OnMatchCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (!IsPlayerAdmin(player, "css_match", "@css/map", "@custom/prac")) {
                SendPlayerNotAdminMessage(player);
                return;
            }

            if (matchStarted) {
                ReplyToUserCommand(player, "已经在比赛模式中!");
                return;
            }

            StartMatchMode();
        }

        [ConsoleCommand("css_exitprac", "Starts match mode")]
        public void OnExitPracCommand(CCSPlayerController? player, CommandInfo? command)
        {
            if (!IsPlayerAdmin(player, "css_exitprac", "@css/map", "@custom/prac")) {
                SendPlayerNotAdminMessage(player);
                return;
            }

            if (matchStarted) {
                ReplyToUserCommand(player, "已经在比赛模式中!");
                return;
            }

            StartMatchMode();
        }

        [ConsoleCommand("css_rcon", "Triggers provided command on the server")]
        public void OnRconCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (!IsPlayerAdmin(player, "css_rcon", "@css/rcon")) {
                SendPlayerNotAdminMessage(player);
                return;
            }
            Server.ExecuteCommand(command.ArgString);
            ReplyToUserCommand(player, "指令成功发送!");
        }

        [ConsoleCommand("css_help", "Triggers provided command on the server")]
        public void OnHelpCommand(CCSPlayerController? player, CommandInfo? command)
        {
            SendAvailableCommandsMessage(player);
        }

        [ConsoleCommand("css_playout", "Toggles playout (Playing of max rounds)")]
        public void OnPlayoutCommand(CCSPlayerController? player, CommandInfo? command) {            
            if (IsPlayerAdmin(player, "css_playout", "@css/config")) {
                isPlayOutEnabled = !isPlayOutEnabled;
                string playoutStatus = isPlayOutEnabled? "是" : "否";
                if (player == null) {
                    ReplyToUserCommand(player, $"Playout is now {playoutStatus}!");
                } else {
                    player.PrintToChat($"{chatPrefix} Playout is now {ChatColors.Green}{playoutStatus}{ChatColors.Default}!");
                }
                
                if (isPlayOutEnabled) {
                    Server.ExecuteCommand("mp_match_can_clinch false");
                } else {
                    Server.ExecuteCommand("mp_match_can_clinch true");
                }
                
            } else {
                SendPlayerNotAdminMessage(player);
            }
        }

        [ConsoleCommand("version", "Returns server version")]
        public void OnVersionCommand(CCSPlayerController? player, CommandInfo? command) {      
            if (command == null) return;
            string steamInfFilePath = Path.Combine(Server.GameDirectory, "csgo", "steam.inf");

            if (!File.Exists(steamInfFilePath))
            {
                command.ReplyToCommand("无法找到 steam.inf 文件!");
            }
            var steamInfContent = File.ReadAllText(steamInfFilePath);

            Regex regex = new(@"ServerVersion=(\d+)");
            Match match = regex.Match(steamInfContent);

            // Extract the version number
            string? serverVersion = match.Success ? match.Groups[1].Value : null;

            // Currently returning only server version to show server status as available on Get5
            command.ReplyToCommand((serverVersion != null) ? $"Protocol version {serverVersion} [{serverVersion}/{serverVersion}]" : "Unable to get server version");
        }
    }
}
