# MatchZyCN

## 介绍
基于**[MatchZy](https://github.com/shobhit-pathak/MatchZy)** v0.7(24年2月更新)的汉化版(有些翻译不准90%的内容进行了汉化和优化提示文本)  CS2满十跑图插件




## 安装教程

### 必须**安装依赖**:

1. **MetaMod** 2.**CounterStrikeSharp (CSSharp)**  请**注意依赖平台选择**！**win端下载win的版本！** **Linux同理           ** 目前我只测试了截至**3月25日**的最新版旧版不祥
2. 下载最新的发行版 解压至CS2服务端中的 csgo目录中
3. 通过在服务器游戏控制台输入**css_plugins list**命令，来验证你的插件是否生效，如果成功你会看到 ***MatchZyCN by L1mit1337....- listed there.***

### 使用说明

#### 配置：

**原版英文使用说明在documentation/docs/下**

所有与 MatchZy 相关的配置文件都可以在 csgo/cfg/MatchZy 中找到

##### 创建管理员

在csgo/cfg/MatchZy中，应该存在一个名为admins.json的文件。如果它不存在，它将在加载插件时自动创建。您可以在该 JSON 文件中添加管理员的 Steam64 ID，如下例所述：

```
{
    "76561198154367261": "",
    "<another_steam_id>: ""
}
```

##### 配置MatchZy的ConVars

同样，在 csgo/cfg/MatchZy 中，应该存在一个名为 config.cfg 的文件。每当加载插件时，都会执行此文件。如果您对此文件进行了任何更改并想要重新加载配置，只需在服务器上执行 exec MatchZy/config.cfg 命令即可。

##### 配置热身/拼刀/比赛/训练模式的cfg

同样，在csgo/cfg/MatchZy中，应该存在名为warmup.cfg，knife.cfg，live.cfg和prac.cfg的文件。这些配置分别在热身、刀、直播和练习模式启动时执行。

您可以根据需要修改这些文件。

如果在预期位置找不到这些配置，则 MatchZy 将执行代码中存在的默认配置。

### 比赛/玩家战绩信息

#### 数据库战绩

MatchZy 带有一个默认数据库 （SQLite），它会自动配置自己。目前我们使用 2 个表，matchzy_match_data 和 matchzy_player_stats。顾名思义，matchzy_match_data保存每场比赛的数据，如matchid、阵营名称、比分等。然而，matchzy_player_stats存储了参加该比赛的每位玩家的数据/统计数据。它存储匹配 id、击杀、死亡、助攻和其他重要统计数据等数据！

#### CSV战绩

比赛结束后，将从SQLite数据库中提取数据，并在csgo/MatchZy_Stats文件夹中写入一个CSV文件。此文件夹将包含每个匹配项的 CSV 文件（文件名模式：match_data_{matchid}.csv），并且它将具有与matchzy_player_stats中存在的相同数据。



## **功能亮点：**

- 使用`.bot`，`.spawn`，`.ctspawn`，`.tspawn`，`.nobots`，`.clear`，`.exitprac`等许多命令进行练习模式！
- 无限金钱的热身模式🤑
- 刀子回合（根据预期逻辑，即玩家最多的队伍获胜。如果玩家数量相同，则HP优势的队伍获胜。如果HP相同，则随机决定胜者）
- 开始实时比赛（在刀子回合决定边的选择之后开始。也可以禁用刀子回合）。
- 自动开始录制演示，并在比赛结束时停止录制（确保您的tv_enable为1）
- 玩家白名单（感谢[DEAFPS](https://github.com/DEAFPS)！）
- 教练系统
- 每轮结束后的伤害报告
- 支持回合恢复（目前使用原始的valve备份系统）
- 创建管理员并允许他们访问管理员命令的能力
- 数据库统计和CSV统计！MatchZy将所有比赛的数据和统计信息存储在本地SQLite数据库中（还支持MySQL数据库！），并为该比赛中的每个玩家创建一个CSV文件以获取详细统计信息！
- 提供简单的配置
- 还有更多！！

#### 使用命令

大多数命令也可以使用!前缀而不是.来使用（例如!ready）

- `.ready` 标记玩家准备就绪
- `.unready` 取消标记玩家准备就绪
- `.pause` 在冷冻时间暂停比赛。
- `.tech` 在冷冻时间暂停比赛。
- `.unpause` 请求取消暂停比赛。两个队伍都需要输入.unpause来取消暂停比赛
- `.stay` 保持在同一边（对于刀子回合的获胜者，在刀子回合之后）
- `.switch` 切换边（对于刀子回合的获胜者，在刀子回合之后）
- `.stop` 恢复当前回合的备份（两个队伍都需要输入.stop来恢复当前回合）
- `.tac` 开始战术暂停
- `.coach <side>` 开始指导指定的边。例如：`.coach t`开始指导恐怖分子方！

#### 练习模式命令

- `.spawn <number>` 在相同队伍的提供的竞技场出生点号码上生成
- `.ctspawn <number>` 在CT的提供的竞技场出生点号码上生成
- `.tspawn <number>` 在T的提供的竞技场出生点号码上生成
- `.bot` 在用户当前位置添加一个机器人
- `.nobots` 移除所有机器人
- `.clear` 清除所有活动的烟雾弹，燃烧瓶和燃烧弹
- `.fastforward` 将服务器时间快进到20秒
- `.god` 打开上帝模式
- `.savenade <name> <optional description>` 保存一个投掷物线路
- `.loadnade <name>` 加载一个投掷物线路
- `.deletenade <name>` 从文件中删除一个投掷物线路
- `.importnade <code>` 在保存线路时，将在聊天中打印一个代码，或者可以从savednades.cfg中检索到这些代码
- `.listnades <optional filter>` 列出所有保存的线路，或者如果给定了过滤器，则只列出与过滤器匹配的线路

#### 管理员命令

- `.start` 强制开始比赛。

- `.restart` 强制重新开始/重置比赛。

- `.forcepause` 作为管理员暂停比赛（玩家无法取消管理员暂停的比赛）。 （`.fp`是较短的命令）

- `.forceunpause` 强制取消暂停比赛。 （`.fup`是较短的命令）

- `.restore <round>` 恢复提供的回合备份。

- `.knife` 切换刀子回合。如果禁用，则比赛将直接从热身阶段进入实时阶段。

- `.playout` 切换播放（如果启用播放，将播放所有回合，无论胜者如何。在切磋中很有用！）

- `.whitelist` 切换玩家白名单。要将玩家加入白名单，请将steam64id添加到`cfg/MatchZy/whitelist.cfg`

- `.readyrequired <number>` 设置开始比赛所需的准备就绪玩家数量。如果设置为0，则所有连接的玩家都必须准备好开始比赛。

- `.settings` 显示当前设置，例如刀子是否启用，readyrequired玩家的值等。

- `.map <mapname>` 更改地图

- `.asay <message>` 以管理员身份在所有聊天中说话

- `.reload_admins` 从`admins.json`重新加载管理员

- `.team1 <name>` 设置队伍1的名称（默认为反恐精英）

- `.team2 <name>` 设置队伍2的名称（默认为恐怖分子）

- `.prac` 开始练习模式

- `.exitprac` 退出练习模式并加载比赛模式。

- `.rcon <command>` 将命令发送到服务器

  

## 开发者须知

MatchZy插件使用C#语言编写，如果你想要参与插件开发，你需要安装C#开发环境 [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) ，安装完毕之后，你可以做一下操作

1. 克隆MatchZy的git仓库

2. 修改 MatchZy.csproj 文件，然后将正确的 CounterStrikeSarp.API.dll(v1.0.164) 的文件路径写到对应位置：
   ![img](https://bbs.csgocn.net/upload/attach/202311/1_VE55G4BFEW8QA6J.png)(CounterStrikeSarp.API.dll 文件来自 CounterStrikeSharp(v1.0.164) 插件, 在上面的安装步骤里面提过)

3. 在终端输入 **dotnet restore** .\MatchZy.csproj 来恢复和安装依赖

4. 修改代码（按你的想法来

5. 在终端输入 **dotnet publish** .\MatchZy.csproj ，然后你会得到一个叫做bin的文件夹，这就是插件编译之后的程序目录

6. 定位到 bin/Debug/net7.0/publish/ 然后把所有的内容复制到服务器的 csgo/addons/counterstrikesharp/plugins/MatchZy (CounterStrikeSharp.API.dll 和CounterStrikeSharp.API.pdb 这两个文件可以跳过不覆盖)

7. 完成! 你可以测试你的修改，也欢迎对本插件贡献你的代码！:p（暂时不能参与 因为我还没弄懂怎么使用git XD）

   

## 感谢

MatchZy原作者- [shobhit-pathak (Shobhit Pathak) (github.com)](https://github.com/shobhit-pathak)

Readme.md的内容大部分来自-[【CS2】MatchZy - 基于CSSharp实现的满10比赛服插件-插件分享-CSGO插件分享-CSGO资料库 (csgocn.net)](https://bbs.csgocn.net/thread-662.htm)

## 贡献

[L1mit1337 (L1mit1337) - Gitee.com](https://gitee.com/L1mit1337) -负责汉化和优化MatchZy提示内容

## 关于问题与BUG反馈

进入Issues提交你的问题或者BUG反馈 最好附带截图
