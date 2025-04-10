using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using System.Text.Json;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CS2_MapPerk
{
	public class MapPerk : BasePlugin
	{
		bool g_bEnable = false;
		ConfigJSON? cfg = new ConfigJSON();
		public override string ModuleName => "Map Perk";
		public override string ModuleDescription => "Adds his steamid to the player attribute";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.1.1";
		public override void Load(bool hotReload)
		{
			RegisterListener<OnMapStart>(OnMapStart_Listener);
			RegisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);
		}
		public override void Unload(bool hotReload)
		{
			RemoveListener<OnMapStart>(OnMapStart_Listener);
			DeregisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);
			RemoveCommand("css_mp_reload", OnReload);
		}
		void OnMapStart_Listener(string sMapName)
		{
			LoadCFG(sMapName.ToLower());
		}
		HookResult OnEventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
		{
			if (g_bEnable)
			{
				CCSPlayerController? player = @event.Userid;
				if (player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
				{
					player.PlayerPawn.Value.AcceptInput("AddAttribute", null, null, player.SteamID.ToString());
					#if DEBUG
					PrintToConsole($"AddAttribute to player: {player.PlayerName} ({player.SteamID.ToString()})");
					#endif
				}
			}
			return HookResult.Continue;
		}
		[ConsoleCommand("css_mp_reload", "Reload whitelist of MapPerk")]
		[RequiresPermissions("@css/root")]
		public void OnReload(CCSPlayerController? player, CommandInfo command)
		{
			if (player != null && !player.IsValid) return;
			if (LoadCFG(Server.MapName.ToLower()))
			{
				if (player != null)
				{
					command.ReplyToCommand(" \x0B[\x04MapPerk\x0B]\x01 Whitelist reloaded!");
					PrintToConsole($"Whitelist reloaded by {player.PlayerName} ({player.NetworkIDString})");
				} else PrintToConsole($"Whitelist reloaded!");
			}
		}
		bool LoadCFG(string sMapName)
		{
			g_bEnable = false;
			string sConfig = $"{Path.Join(ModuleDirectory, "config.json")}";
			string sData;
			if (File.Exists(sConfig))
			{
				try
				{
					sData = File.ReadAllText(sConfig);
					cfg = JsonSerializer.Deserialize<ConfigJSON>(sData);
				} catch { PrintToConsole($"Bad Config file ({sConfig})"); return false; }

				if (cfg == null)
				{
					PrintToConsole($"Bad Config file ({sConfig})");
					return false;
				}
				if (cfg.whitelist)
				{
					foreach (string sMap in cfg.maps_whitelist.ToList())
					{
						if (string.Equals(sMapName, sMap.ToLower()))
						{
							g_bEnable = true;
							return true;
						}
					}
				}
				else
				{
					foreach (string sMap in cfg.maps_blacklist.ToList())
					{
						if (string.Equals(sMapName, sMap.ToLower())) return true;
					}
					g_bEnable = true;
				}
					
				return true;
			} else PrintToConsole($"Config file ({sConfig}) not found");
			return false;
		}
		void PrintToConsole(string sValue)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("MapPerk");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)3;
			Console.WriteLine(sValue);
			Console.ResetColor();
		}
		class ConfigJSON
		{
			public bool whitelist { get; set; }
			public List<string> maps_whitelist { get; set; }
			public List<string> maps_blacklist { get; set; }
			public ConfigJSON()
			{
				whitelist = true;
				maps_whitelist = new List<string>();
				maps_blacklist = new List<string>();
			}
		}
	}
}
