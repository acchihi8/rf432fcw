using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyCSharp.Mod.Xmap;

// Token: 0x0200000F RID: 15
internal class AutoBroly
{
	// Token: 0x0600004E RID: 78
	private static void Wait(int time)
	{
		AutoBroly.IsWait = true;
		AutoBroly.TimeStartWait = mSystem.currentTimeMillis();
		AutoBroly.TimeWait = (long)time;
	}

	// Token: 0x0600004F RID: 79
	private static bool IsWaiting()
	{
		if (AutoBroly.IsWait && mSystem.currentTimeMillis() - AutoBroly.TimeStartWait >= AutoBroly.TimeWait)
		{
			AutoBroly.IsWait = false;
		}
		return AutoBroly.IsWait;
	}

	// Token: 0x06000050 RID: 80
	public static bool IsBoss()
	{
		for (int i = 0; i < GameScr.vCharInMap.size(); i++)
		{
			global::Char @char = (global::Char)GameScr.vCharInMap.elementAt(i);
			if (@char != null && @char.cName.Contains("Broly") && @char.cName.Contains("Super") && @char.cHPFull >= 16070777L)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000051 RID: 81
	public static void SearchBoss()
	{
		int currentMap = TileMap.mapID;
		int currentZone = TileMap.zoneID;
		int num = GameScr.gI().zones.Length;
		if (AutoBroly.IsBoss())
		{
			AutoBroly.visitedZones.Clear();
			return;
		}
		AutoBroly.visitedZones.Add(currentZone);
		AutoBroly.AddGlobalVisitedZone(currentMap, currentZone);
		List<int> list = (from z in Enumerable.Range(0, num)
			where z != currentZone && !AutoBroly.visitedZones.Contains(z) && !AutoBroly.IsGlobalZoneVisited(currentMap, z)
			select z).ToList<int>();
		if (list.Count == 0)
		{
			AutoBroly.visitedZones.Clear();
			return;
		}
		int num2 = list[AutoBroly.random.Next(list.Count)];
		Service.gI().requestChangeZone(num2, -1);
	}

	// Token: 0x06000052 RID: 82
	public static void FocusSuperBroly()
	{
		for (int i = 0; i < GameScr.vCharInMap.size(); i++)
		{
			global::Char @char = (global::Char)GameScr.vCharInMap.elementAt(i);
			if (@char != null && @char.cName.Contains("Broly") && @char.cName.Contains("Super") && @char.cHP > 0L && global::Char.myCharz().charFocus != @char)
			{
				global::Char.myCharz().npcFocus = null;
				global::Char.myCharz().mobFocus = null;
				global::Char.myCharz().charFocus = @char;
				return;
			}
		}
	}

	// Token: 0x06000053 RID: 83
	public static void Update()
	{
		AutoBroly.PeriodicStatusUpdate();
		AutoBroly.CheckAndUpdateStatus();
		if (!AutoBroly.IsWaiting())
		{
			if (global::Char.myCharz().cHP <= 0L || global::Char.myCharz().meDead)
			{
				if (AutoBroly.IsBoss())
				{
					AutoBroly.Map = TileMap.mapID;
					AutoBroly.Khu = TileMap.zoneID;
				}
				Service.gI().returnTownFromDead();
				AutoBroly.UpdateMyStatus("hoàn thành", "none");
				AutoBroly.Wait(3000);
				return;
			}
			if (AutoBroly.Map != -1 && AutoBroly.Khu != -1 && TileMap.mapID == AutoBroly.Map && TileMap.zoneID == AutoBroly.Khu && !AutoBroly.IsBoss())
			{
				AutoBroly.Map = -1;
				AutoBroly.Khu = -1;
			}
			if (AutoBroly.IsBoss())
			{
				if (DataAccount.Type > 1)
				{
					AutoBroly.Map = TileMap.mapID;
					AutoBroly.Khu = TileMap.zoneID;
				}
				AutoBroly.TrangThai = "SP: " + TileMap.mapNames[TileMap.mapID].ToString() + " - " + TileMap.zoneID.ToString();
				if (AutoBroly.visitedZones.Count > 0)
				{
					AutoBroly.visitedZones.Clear();
				}
			}
			else
			{
				AutoBroly.TrangThai = "Không có thông tin ";
			}
			if (AutoBroly.Map != -1 && TileMap.mapID != AutoBroly.Map && !Pk9rXmap.IsXmapRunning)
			{
				XmapController.StartRunToMapId(AutoBroly.Map);
				AutoBroly.Wait(3000);
				return;
			}
			if (TileMap.mapID == AutoBroly.Map && TileMap.zoneID != AutoBroly.Khu && AutoBroly.Khu != -1)
			{
				Service.gI().requestChangeZone(AutoBroly.Khu, -1);
				AutoBroly.Wait(2000);
				return;
			}
			if (TileMap.mapID == AutoBroly.Map && TileMap.zoneID == AutoBroly.Khu && AutoBroly.IsBoss())
			{
				AutoBroly.FocusSuperBroly();
			}
			if (!AutoBroly.IsBoss() && AutoBroly.isDoKhu)
			{
				AutoBroly.SearchBoss();
				AutoBroly.Wait(2000);
				return;
			}
			if (DataAccount.Type == 1)
			{
				if (AutoBroly.NhayNe == 0 && AutoBroly.IsBoss())
				{
					AutoBroly.NhayNe = 1;
					AutoBroly.NhayCuoiMap();
					AutoBroly.Wait(1000);
					return;
				}
				if (!AutoBroly.IsBoss() && AutoBroly.NhayNe == 1)
				{
					AutoBroly.NhayNe = 0;
				}
			}
			if (DataAccount.Type == 3)
			{
				if (mSystem.currentTimeMillis() - AutoBroly.lastStatusUpdateTime > 60000L)
				{
					AutoBroly.lastStatusUpdateTime = mSystem.currentTimeMillis();
					AutoBroly.ChiaKhuTheoTrangThai();
				}
				if (AutoBroly.NhayNe == 0 && !AutoBroly.IsBoss())
				{
					AutoBroly.NhayNe = 1;
					AutoBroly.NhayCuoiMap();
					AutoBroly.Wait(1000);
					return;
				}
				if (!AutoBroly.IsBoss() && AutoBroly.NhayNe == 1)
				{
					AutoBroly.NhayNe = 0;
				}
			}
			AutoBroly.AutoKichSP();
			AutoBroly.Wait(500);
		}
	}

	// Token: 0x06000054 RID: 84
	public static void NhayCuoiMap()
	{
		if (GameScr.getX(2) > 0 && GameScr.getY(2) > 0)
		{
			KsSupper.TelePortTo(GameScr.getX(2) - 50, GameScr.getY(2));
		}
	}

	// Token: 0x06000055 RID: 85
	public static bool IsBroly()
	{
		for (int i = 0; i < GameScr.vCharInMap.size(); i++)
		{
			global::Char @char = (global::Char)GameScr.vCharInMap.elementAt(i);
			if (@char != null && @char.cName.Contains("Broly") && !@char.cName.Contains("Super"))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000056 RID: 86
	public static void Painting(mGraphics g)
	{
		string text = TileMap.mapNames[TileMap.mapID];
		string text2 = " - " + TileMap.zoneID.ToString();
		string text3 = NinjaUtil.getMoneys(global::Char.myCharz().cHP).ToString() + " / " + NinjaUtil.getMoneys(global::Char.myCharz().cHPFull).ToString();
		string text4 = NinjaUtil.getMoneys(global::Char.myCharz().cMP).ToString() + " / " + NinjaUtil.getMoneys(global::Char.myCharz().cMPFull).ToString();
		if (AutoBroly.IsBoss())
		{
			for (int i = 0; i < GameScr.vCharInMap.size(); i++)
			{
				global::Char @char = (global::Char)GameScr.vCharInMap.elementAt(i);
				if (@char != null && @char.cName.Contains("Broly") && @char.cName.Contains("Super") && @char.cHPFull >= 16070777L)
				{
					string text5 = string.Concat(new string[]
					{
						@char.cName,
						" [ ",
						NinjaUtil.getMoneys(@char.cHP).ToString(),
						" / ",
						NinjaUtil.getMoneys(@char.cHPFull).ToString(),
						" ]"
					});
					mFont.tahoma_7b_yellow.drawString(g, text5, 20, GameCanvas.h - (GameCanvas.h - GameCanvas.h / 3), 0);
				}
			}
		}
		if (AutoBroly.IsBroly())
		{
			for (int j = 0; j < GameScr.vCharInMap.size(); j++)
			{
				global::Char char2 = (global::Char)GameScr.vCharInMap.elementAt(j);
				if (char2 != null && char2.cName.Contains("Broly") && !char2.cName.Contains("Super"))
				{
					string text6 = string.Concat(new string[]
					{
						char2.cName,
						" [ ",
						NinjaUtil.getMoneys(char2.cHP).ToString(),
						" / ",
						NinjaUtil.getMoneys(char2.cHPFull).ToString(),
						" ]"
					});
					mFont.tahoma_7b_white.drawString(g, text6, 20, GameCanvas.h - (GameCanvas.h - GameCanvas.h / 3), 0);
				}
			}
		}
		mFont.tahoma_7b_white.drawString(g, "HP: " + text3, 30, GameCanvas.h - (GameCanvas.h - 25), 0);
		mFont.tahoma_7b_white.drawString(g, "MP: " + text4, 30, GameCanvas.h - (GameCanvas.h - 35), 0);
		mFont.tahoma_7b_white.drawString(g, text + " " + text2 + " ", 30, GameCanvas.h - (GameCanvas.h - 10), 0);
		mFont.tahoma_7b_white.drawString(g, AutoBroly.Map.ToString() + " " + AutoBroly.Khu.ToString() + " ", GameCanvas.w - 30, GameCanvas.h - (GameCanvas.h - 10), 0);
	}

	// Token: 0x06000059 RID: 89
	private static int MyMax(int a, int b)
	{
		if (a <= b)
		{
			return b;
		}
		return a;
	}

	// Token: 0x0600005A RID: 90
	private static int MyMin(int a, int b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	// Token: 0x0600005B RID: 91
	private static double MySqrt(double x)
	{
		if (x < 0.0)
		{
			return double.NaN;
		}
		double num = x;
		double num2;
		do
		{
			num2 = num;
			num = (num + x / num) / 2.0;
		}
		while (global::System.Math.Abs(num - num2) > 1E-07);
		return num;
	}

	// Token: 0x0600005C RID: 92
	public static void AutoKichSP()
	{
		if (DataAccount.Type != 1)
		{
			return;
		}
		global::Char @char = global::Char.myCharz();
		global::Char char2 = null;
		for (int i = 0; i < GameScr.vCharInMap.size(); i++)
		{
			global::Char char3 = (global::Char)GameScr.vCharInMap.elementAt(i);
			if (char3 != null && char3.cName.Contains("Broly") && !char3.cName.Contains("Super"))
			{
				char2 = char3;
				break;
			}
		}
		if (char2 == null)
		{
			return;
		}
		int num9 = @char.cx - char2.cx;
		int num2 = @char.cy - char2.cy;
		int num10 = (int)AutoBroly.MySqrt((double)(num9 * num9 + num2 * num2));
		long num3 = mSystem.currentTimeMillis();
		int num4 = 0;
		int num5 = 3840;
		int num6 = 120;
		int num7 = 10;
		if (num10 <= num6)
		{
			if (num3 - AutoBroly.lastAutoKichSPTime >= 350L)
			{
				int num8 = @char.cx;
				if (@char.cx <= num4 + num7 && @char.cx <= char2.cx)
				{
					num8 = char2.cx + num6;
					if (num8 > num5 - num7)
					{
						num8 = num5 - num7;
					}
				}
				else if (@char.cx >= num5 - num7 && @char.cx >= char2.cx)
				{
					num8 = char2.cx - num6;
					if (num8 < num4 + num7)
					{
						num8 = num4 + num7;
					}
				}
				else if (@char.cx < char2.cx)
				{
					num8 = char2.cx - num6;
					if (num8 < num4 + num7)
					{
						num8 = num4 + num7;
					}
				}
				else if (@char.cx > char2.cx)
				{
					num8 = char2.cx + num6;
					if (num8 > num5 - num7)
					{
						num8 = num5 - num7;
					}
				}
				KsSupper.TelePortTo(num8, @char.cy);
				AutoBroly.lastAutoKichSPTime = num3;
				return;
			}
		}
		else
		{
			AutoBroly.lastAutoKichSPTime = num3;
		}
	}

	// Token: 0x0600005D RID: 93
	public static void AddGlobalVisitedZone(int mapId, int zoneId)
	{
		string text = string.Format("{0},{1}", mapId, zoneId);
		if (!AutoBroly.globalVisitedZones.Contains(text))
		{
			AutoBroly.globalVisitedZones.Add(text);
			AutoBroly.WriteGlobalVisitedZonesToFile();
		}
	}

	// Token: 0x0600005E RID: 94
	public static bool IsGlobalZoneVisited(int mapId, int zoneId)
	{
		string text = string.Format("{0},{1}", mapId, zoneId);
		return AutoBroly.globalVisitedZones.Contains(text);
	}

	// Token: 0x0600005F RID: 95
	public static void ReadGlobalVisitedZonesFromFile()
	{
		try
		{
			if (File.Exists(AutoBroly.GLOBAL_VISITED_ZONES_FILE))
			{
				string[] array = File.ReadAllLines(AutoBroly.GLOBAL_VISITED_ZONES_FILE);
				HashSet<string> hashSet = AutoBroly.globalVisitedZones;
				lock (hashSet)
				{
					AutoBroly.globalVisitedZones.Clear();
					foreach (string text in array)
					{
						if (!string.IsNullOrEmpty(text.Trim()))
						{
							AutoBroly.globalVisitedZones.Add(text.Trim());
						}
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	// Token: 0x06000060 RID: 96
	public static void WriteGlobalVisitedZonesToFile()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(AutoBroly.GLOBAL_VISITED_ZONES_FILE);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			HashSet<string> hashSet = AutoBroly.globalVisitedZones;
			lock (hashSet)
			{
				File.WriteAllLines(AutoBroly.GLOBAL_VISITED_ZONES_FILE, AutoBroly.globalVisitedZones.ToArray<string>());
			}
		}
		catch (Exception)
		{
		}
	}

	// Token: 0x06000061 RID: 97
	public static void ResetGlobalVisitedZones()
	{
		HashSet<string> hashSet = AutoBroly.globalVisitedZones;
		lock (hashSet)
		{
			AutoBroly.globalVisitedZones.Clear();
		}
		AutoBroly.WriteGlobalVisitedZonesToFile();
	}

	// Token: 0x06000062 RID: 98
	public static void UpdateMyStatus(string status, string zones = "none")
	{
		if (DataAccount.Type != 3)
		{
			return;
		}
		string text = "Nro_244_Data/Resources/Data/online.txt";
		string text2 = DataAccount.ID.ToString();
		string text3 = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", new object[]
		{
			DateTime.Now.Second,
			DateTime.Now.Minute,
			DateTime.Now.Hour,
			DateTime.Now.Day,
			DateTime.Now.Month,
			DateTime.Now.Year
		});
		string text4 = string.Concat(new string[] { text2, "|", text3, "|", status, "|", zones });
		if (text4 == AutoBroly.lastStatusLine)
		{
			return;
		}
		AutoBroly.lastStatusLine = text4;
		string[] array = (File.Exists(text) ? File.ReadAllLines(text) : new string[0]);
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith(text2 + "|"))
			{
				array[i] = text4;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			List<string> list = array.ToList<string>();
			list.Add(text4);
			array = list.ToArray();
		}
		File.WriteAllLines(text, array);
	}

	// Token: 0x06000063 RID: 99
	public static void CheckAndUpdateStatus()
	{
		if (DataAccount.Type != 3)
		{
			return;
		}
		global::Char @char = null;
		for (int i = 0; i < GameScr.vCharInMap.size(); i++)
		{
			global::Char char2 = (global::Char)GameScr.vCharInMap.elementAt(i);
			if (char2 != null && char2.cName.Contains("Broly") && char2.cName.Contains("Super"))
			{
				@char = char2;
				break;
			}
		}
		if (@char == null)
		{
			AutoBroly.BossInvalidStartTime = -1L;
			AutoBroly.UpdateMyStatus("rảnh", "none");
			return;
		}
		if (@char.cHPFull >= 16070777L && @char.cHP > 1L)
		{
			AutoBroly.BossInvalidStartTime = -1L;
			AutoBroly.UpdateMyStatus("bận", "none");
			return;
		}
		if (AutoBroly.BossInvalidStartTime == -1L)
		{
			AutoBroly.BossInvalidStartTime = mSystem.currentTimeMillis();
			return;
		}
	}

	// Token: 0x06000064 RID: 100
	private static long GetCurrentUnixTimeMillis()
	{
		return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	// Token: 0x06000065 RID: 101
	public static void ChiaKhuTheoTrangThai()
	{
		if (DataAccount.Type != 3)
		{
			return;
		}
		string text = "Nro_244_Data/Resources/Data/online.txt";
		string[] array = (File.Exists(text) ? File.ReadAllLines(text) : new string[0]);
		List<string> list = new List<string>();
		long currentUnixTimeMillis = AutoBroly.GetCurrentUnixTimeMillis();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[] { '|' });
			if (array2.Length >= 4)
			{
				string text2 = array2[0].Trim();
				string text3 = array2[1].Trim();
				string text4 = array2[2].Trim();
				string[] array3 = text3.Split(new char[] { ':' });
				if (array3.Length == 6)
				{
					try
					{
						DateTime dateTime = new DateTime(int.Parse(array3[5]), int.Parse(array3[4]), int.Parse(array3[3]), int.Parse(array3[2]), int.Parse(array3[1]), int.Parse(array3[0]));
						long num = (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
						if (text4 == "rảnh" && currentUnixTimeMillis - num < 70000L)
						{
							list.Add(text2);
						}
					}
					catch
					{
					}
				}
			}
		}
		Random rnd = new Random();
		list = list.OrderBy((string x) => rnd.Next()).ToList<string>();
		List<int> list2 = Enumerable.Range(2, 13).ToList<int>();
		list2 = list2.OrderBy((int x) => rnd.Next()).ToList<int>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int j = 0; j < list.Count; j++)
		{
			if (j < list2.Count)
			{
				dictionary[list[j]] = list2[j].ToString();
			}
			else
			{
				dictionary[list[j]] = "none";
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			string[] array4 = array[k].Split(new char[] { '|' });
			if (array4.Length >= 4)
			{
				string text5 = array4[0].Trim();
				if (dictionary.ContainsKey(text5))
				{
					array4[3] = dictionary[text5];
					array[k] = string.Join("|", array4);
				}
			}
		}
		File.WriteAllLines(text, array);
		foreach (KeyValuePair<string, string> keyValuePair in dictionary)
		{
			if (keyValuePair.Value != "none")
			{
				AutoBroly.GuiLenhChiaKhu(keyValuePair.Key, int.Parse(keyValuePair.Value));
			}
		}
	}

	// Token: 0x06000066 RID: 102
	private static void GuiLenhChiaKhu(string acc, int zone)
	{
		Console.WriteLine("Chia acc " + acc + " vào zone " + zone.ToString());
	}

	// Token: 0x06000067 RID: 103
	public static void PeriodicStatusUpdate()
	{
		if (DataAccount.Type != 3)
		{
			return;
		}
		long num = mSystem.currentTimeMillis();
		long accOffset = AutoBroly.GetAccOffset();
		if (num - AutoBroly.lastStatusUpdateTime >= 60000L + accOffset)
		{
			AutoBroly.lastStatusUpdateTime = num;
			AutoBroly.CheckAndUpdateStatus();
		}
	}

	// Token: 0x06000068 RID: 104
	private static long GetAccOffset()
	{
		return (long)(DataAccount.ID * 137 % 60000);
	}

	// Token: 0x0400004D RID: 77
	public static string TrangThai = "Không có thông tin";

	// Token: 0x0400004E RID: 78
	public static int Map = -1;

	// Token: 0x0400004F RID: 79
	public static int Khu = -1;

	// Token: 0x04000050 RID: 80
	private static bool IsWait;

	// Token: 0x04000051 RID: 81
	private static long TimeStartWait;

	// Token: 0x04000052 RID: 82
	private static long TimeWait;

	// Token: 0x04000053 RID: 83
	public static bool isDoKhu = false;

	// Token: 0x04000054 RID: 84
	private static HashSet<int> visitedZones = new HashSet<int>();

	// Token: 0x04000055 RID: 85
	private static Random random = new Random();

	// Token: 0x04000056 RID: 86
	public static int NhayNe = 0;

	// Token: 0x04000057 RID: 87
	private static long lastAutoKichSPTime = 0L;

	// Token: 0x04000058 RID: 88
	public static HashSet<string> globalVisitedZones = new HashSet<string>();

	// Token: 0x04000059 RID: 89
	public static long lastGlobalZoneUpdateTime = 0L;

	// Token: 0x0400005A RID: 90
	private static readonly string GLOBAL_VISITED_ZONES_FILE = "Nro_244_Data/Resources/GlobalVisitedZones.txt";

	// Token: 0x0400005B RID: 91
	private static long BossInvalidStartTime = -1L;

	// Token: 0x0400005C RID: 92
	private static long lastStatusUpdateTime = 0L;

	// Token: 0x0400005D RID: 93
	private static bool wasBossPresent = false;

	// Token: 0x0400005E RID: 94
	private static string lastStatusLine = "";

	// Token: 0x0400180C RID: 6156
	private static DateTime lastOnlineUpdate = DateTime.MinValue;

	// Token: 0x0400180D RID: 6157
	public static int MyZone = -1;
}
using System;
using Assets.src.e;
using Assets.src.g;
using UnityEngine;

// Token: 0x0200005C RID: 92
public class GameCanvas : IActionListener
{
	// Token: 0x060003FE RID: 1022 RVA: 0x0004C8B4 File Offset: 0x0004AAB4
	public GameCanvas()
	{
		int num = Rms.loadRMSInt("languageVersion");
		int num2 = num;
		if (num2 != -1)
		{
			if (num2 != 2)
			{
				Main.main.doClearRMS();
				Rms.saveRMSInt("languageVersion", 2);
			}
		}
		else
		{
			Rms.saveRMSInt("languageVersion", 2);
		}
		GameCanvas.clearOldData = Rms.loadRMSInt(GameMidlet.VERSION);
		bool flag = GameCanvas.clearOldData != 1;
		if (flag)
		{
			Main.main.doClearRMS();
			Rms.saveRMSInt(GameMidlet.VERSION, 1);
		}
		this.initGame();
	}

	// Token: 0x060003FF RID: 1023 RVA: 0x0004C980 File Offset: 0x0004AB80
	public static string getPlatformName()
	{
		return "Pc platform xxx";
	}

	// Token: 0x06000400 RID: 1024 RVA: 0x0004C998 File Offset: 0x0004AB98
	public void initGame()
	{
		try
		{
			MotherCanvas.instance.setChildCanvas(this);
			GameCanvas.w = MotherCanvas.instance.getWidthz();
			GameCanvas.h = MotherCanvas.instance.getHeightz();
			GameCanvas.hw = GameCanvas.w / 2;
			GameCanvas.hh = GameCanvas.h / 2;
			GameCanvas.isTouch = true;
			bool flag = GameCanvas.w >= 240;
			if (flag)
			{
				GameCanvas.isTouchControl = true;
			}
			bool flag2 = GameCanvas.w < 320;
			if (flag2)
			{
				GameCanvas.isTouchControlSmallScreen = true;
			}
			bool flag3 = GameCanvas.w >= 320;
			if (flag3)
			{
				GameCanvas.isTouchControlLargeScreen = true;
			}
			GameCanvas.msgdlg = new MsgDlg();
			bool flag4 = GameCanvas.h <= 160;
			if (flag4)
			{
				Paint.hTab = 15;
				mScreen.cmdH = 17;
			}
			GameScr.d = ((GameCanvas.w <= GameCanvas.h) ? GameCanvas.h : GameCanvas.w) + 20;
			GameCanvas.instance = this;
			mFont.init();
			mScreen.ITEM_HEIGHT = mFont.tahoma_8b.getHeight() + 8;
			this.initPaint();
			this.loadDust();
			this.loadWaterSplash();
			GameCanvas.panel = new Panel();
			GameCanvas.imgShuriken = GameCanvas.loadImage("/mainImage/myTexture2df.png");
			int num = Rms.loadRMSInt("clienttype");
			bool flag5 = num != -1;
			if (flag5)
			{
				bool flag6 = num > 7;
				if (flag6)
				{
					Rms.saveRMSInt("clienttype", mSystem.clientType);
				}
				else
				{
					mSystem.clientType = num;
				}
			}
			bool flag7 = mSystem.clientType == 7 && (Rms.loadRMSString("fake") == null || Rms.loadRMSString("fake") == string.Empty);
			if (flag7)
			{
				GameCanvas.imgShuriken = GameCanvas.loadImage("/mainImage/wait.png");
			}
			GameCanvas.imgClear = GameCanvas.loadImage("/mainImage/myTexture2der.png");
			GameCanvas.img12 = GameCanvas.loadImage("/mainImage/12+.png");
			GameCanvas.debugUpdate = new MyVector();
			GameCanvas.debugPaint = new MyVector();
			GameCanvas.debugSession = new MyVector();
			for (int i = 0; i < 3; i++)
			{
				GameCanvas.imgBorder[i] = GameCanvas.loadImage("/mainImage/myTexture2dbd" + i.ToString() + ".png");
			}
			GameCanvas.borderConnerW = mGraphics.getImageWidth(GameCanvas.imgBorder[0]);
			GameCanvas.borderConnerH = mGraphics.getImageHeight(GameCanvas.imgBorder[0]);
			GameCanvas.borderCenterW = mGraphics.getImageWidth(GameCanvas.imgBorder[1]);
			GameCanvas.borderCenterH = mGraphics.getImageHeight(GameCanvas.imgBorder[1]);
			Panel.graphics = Rms.loadRMSInt("lowGraphic");
			GameCanvas.lowGraphic = Rms.loadRMSInt("lowGraphic") == 1;
			GameScr.isPaintChatVip = Rms.loadRMSInt("serverchat") != 1;
			global::Char.isPaintAura = Rms.loadRMSInt("isPaintAura") == 1;
			global::Char.isPaintAura2 = Rms.loadRMSInt("isPaintAura2") == 1;
			Res.init();
			SmallImage.loadBigImage();
			Panel.WIDTH_PANEL = 176;
			bool flag8 = Panel.WIDTH_PANEL > GameCanvas.w;
			if (flag8)
			{
				Panel.WIDTH_PANEL = GameCanvas.w;
			}
			InfoMe.gI().loadCharId();
			Command.btn0left = GameCanvas.loadImage("/mainImage/btn0left.png");
			Command.btn0mid = GameCanvas.loadImage("/mainImage/btn0mid.png");
			Command.btn0right = GameCanvas.loadImage("/mainImage/btn0right.png");
			Command.btn1left = GameCanvas.loadImage("/mainImage/btn1left.png");
			Command.btn1mid = GameCanvas.loadImage("/mainImage/btn1mid.png");
			Command.btn1right = GameCanvas.loadImage("/mainImage/btn1right.png");
			GameCanvas.serverScreen = new ServerListScreen();
			GameCanvas.img12 = GameCanvas.loadImage("/mainImage/12+.png");
			for (int j = 0; j < 7; j++)
			{
				GameCanvas.imgBlue[j] = GameCanvas.loadImage("/effectdata/blue/" + j.ToString() + ".png");
				GameCanvas.imgViolet[j] = GameCanvas.loadImage("/effectdata/violet/" + j.ToString() + ".png");
			}
			ServerListScreen.createDeleteRMS();
			GameCanvas.serverScr = new ServerScr();
			GameCanvas.loginScr = new LoginScr();
			GameCanvas._SelectCharScr = new SelectCharScr();
		}
		catch (Exception)
		{
			Debug.LogError("----------------->>>>>>>>>>errr");
		}
	}

	// Token: 0x06000401 RID: 1025 RVA: 0x0004CDD4 File Offset: 0x0004AFD4
	public static GameCanvas gI()
	{
		return GameCanvas.instance;
	}

	// Token: 0x06000402 RID: 1026 RVA: 0x00004A8B File Offset: 0x00002C8B
	public void initPaint()
	{
		GameCanvas.paintz = new Paint();
	}

	// Token: 0x06000403 RID: 1027 RVA: 0x00004A98 File Offset: 0x00002C98
	public static void closeKeyBoard()
	{
		mGraphics.addYWhenOpenKeyBoard = 0;
		GameCanvas.timeOpenKeyBoard = 0;
		Main.closeKeyBoard();
	}

	// Token: 0x06000404 RID: 1028 RVA: 0x0004CDEC File Offset: 0x0004AFEC
	public void update()
	{
		bool flag = GameCanvas.currentScreen == GameCanvas._SelectCharScr;
		if (flag)
		{
			bool flag2 = GameCanvas.gameTick % 2 == 0 && SmallImage.vt_images_watingDowload.size() > 0;
			if (flag2)
			{
				Small small = (Small)SmallImage.vt_images_watingDowload.elementAt(0);
				Service.gI().requestIcon(small.id);
				SmallImage.vt_images_watingDowload.removeElementAt(0);
			}
		}
		else
		{
			bool flag3 = GameCanvas.isRequestMapID == 2 && GameCanvas.waitingTimeChangeMap < mSystem.currentTimeMillis() && GameCanvas.gameTick % 2 == 0 && GameCanvas.currentScreen != null;
			if (flag3)
			{
				bool flag4 = GameCanvas.currentScreen == GameScr.gI();
				if (flag4)
				{
					bool isLoadingMap = global::Char.isLoadingMap;
					if (isLoadingMap)
					{
						global::Char.isLoadingMap = false;
					}
					bool waitToLogin = ServerListScreen.waitToLogin;
					if (waitToLogin)
					{
						ServerListScreen.waitToLogin = false;
					}
				}
				bool flag5 = SmallImage.vt_images_watingDowload.size() > 0;
				if (flag5)
				{
					Small small2 = (Small)SmallImage.vt_images_watingDowload.elementAt(0);
					Service.gI().requestIcon(small2.id);
					SmallImage.vt_images_watingDowload.removeElementAt(0);
				}
				bool flag6 = Effect.dowloadEff.size() <= 0;
				if (flag6)
				{
				}
			}
		}
		bool flag7 = mSystem.currentTimeMillis() > this.timefps;
		if (flag7)
		{
			this.timefps += 1000L;
			GameCanvas.max = GameCanvas.fps;
			GameCanvas.fps = 0;
		}
		GameCanvas.fps++;
		bool flag8 = GameCanvas.messageServer.size() > 0 && GameCanvas.thongBaoTest == null;
		if (flag8)
		{
			GameCanvas.startserverThongBao((string)GameCanvas.messageServer.elementAt(0));
			GameCanvas.messageServer.removeElementAt(0);
		}
		bool flag9 = GameCanvas.gameTick % 5 == 0;
		if (flag9)
		{
			GameCanvas.timeNow = mSystem.currentTimeMillis();
		}
		Res.updateOnScreenDebug();
		try
		{
			bool visible = global::TouchScreenKeyboard.visible;
			if (visible)
			{
				GameCanvas.timeOpenKeyBoard++;
				bool flag10 = GameCanvas.timeOpenKeyBoard > ((!Main.isWindowsPhone) ? 10 : 5);
				if (flag10)
				{
					mGraphics.addYWhenOpenKeyBoard = 94;
				}
			}
			else
			{
				mGraphics.addYWhenOpenKeyBoard = 0;
				GameCanvas.timeOpenKeyBoard = 0;
			}
			GameCanvas.debugUpdate.removeAllElements();
			long num = mSystem.currentTimeMillis();
			bool flag11 = num - GameCanvas.timeTickEff1 >= 780L && !GameCanvas.isEff1;
			if (flag11)
			{
				GameCanvas.timeTickEff1 = num;
				GameCanvas.isEff1 = true;
			}
			else
			{
				GameCanvas.isEff1 = false;
			}
			bool flag12 = num - GameCanvas.timeTickEff2 >= 7800L && !GameCanvas.isEff2;
			if (flag12)
			{
				GameCanvas.timeTickEff2 = num;
				GameCanvas.isEff2 = true;
			}
			else
			{
				GameCanvas.isEff2 = false;
			}
			bool flag13 = GameCanvas.taskTick > 0;
			if (flag13)
			{
				GameCanvas.taskTick--;
			}
			GameCanvas.gameTick++;
			bool flag14 = GameCanvas.gameTick > 10000;
			if (flag14)
			{
				bool flag15 = mSystem.currentTimeMillis() - GameCanvas.lastTimePress > 20000L && GameCanvas.currentScreen == GameCanvas.loginScr;
				if (flag15)
				{
					GameMidlet.instance.exit();
				}
				GameCanvas.gameTick = 0;
			}
			bool flag16 = GameCanvas.currentScreen != null;
			if (flag16)
			{
				bool flag17 = ChatPopup.serverChatPopUp != null;
				if (flag17)
				{
					ChatPopup.serverChatPopUp.update();
					ChatPopup.serverChatPopUp.updateKey();
				}
				else
				{
					bool flag18 = ChatPopup.currChatPopup != null;
					if (flag18)
					{
						ChatPopup.currChatPopup.update();
						ChatPopup.currChatPopup.updateKey();
					}
					else
					{
						bool flag19 = GameCanvas.currentDialog != null;
						if (flag19)
						{
							GameCanvas.debug("B", 0);
							GameCanvas.currentDialog.update();
						}
						else
						{
							bool showMenu = GameCanvas.menu.showMenu;
							if (showMenu)
							{
								GameCanvas.debug("C", 0);
								GameCanvas.menu.updateMenu();
								GameCanvas.debug("D", 0);
								GameCanvas.menu.updateMenuKey();
							}
							else
							{
								bool isShow = GameCanvas.panel.isShow;
								if (isShow)
								{
									GameCanvas.panel.update();
									bool flag20 = GameCanvas.isPointer(GameCanvas.panel.X, GameCanvas.panel.Y, GameCanvas.panel.W, GameCanvas.panel.H);
									if (flag20)
									{
										GameCanvas.isFocusPanel2 = false;
									}
									bool flag21 = GameCanvas.panel2 != null && GameCanvas.panel2.isShow;
									if (flag21)
									{
										GameCanvas.panel2.update();
										bool flag22 = GameCanvas.isPointer(GameCanvas.panel2.X, GameCanvas.panel2.Y, GameCanvas.panel2.W, GameCanvas.panel2.H);
										if (flag22)
										{
											GameCanvas.isFocusPanel2 = true;
										}
									}
									bool flag23 = GameCanvas.panel2 != null;
									if (flag23)
									{
										bool flag24 = GameCanvas.isFocusPanel2;
										if (flag24)
										{
											GameCanvas.panel2.updateKey();
										}
										else
										{
											GameCanvas.panel.updateKey();
										}
									}
									else
									{
										GameCanvas.panel.updateKey();
									}
									bool flag25 = GameCanvas.panel.chatTField != null && GameCanvas.panel.chatTField.isShow;
									if (flag25)
									{
										GameCanvas.panel.chatTFUpdateKey();
									}
									else
									{
										bool flag26 = GameCanvas.panel2 != null && GameCanvas.panel2.chatTField != null && GameCanvas.panel2.chatTField.isShow;
										if (flag26)
										{
											GameCanvas.panel2.chatTFUpdateKey();
										}
										else
										{
											bool flag27 = (GameCanvas.isPointer(GameCanvas.panel.X, GameCanvas.panel.Y, GameCanvas.panel.W, GameCanvas.panel.H) && GameCanvas.panel2 != null) || GameCanvas.panel2 == null;
											if (flag27)
											{
												GameCanvas.panel.updateKey();
											}
											else
											{
												bool flag28 = GameCanvas.panel2 != null && GameCanvas.panel2.isShow && GameCanvas.isPointer(GameCanvas.panel2.X, GameCanvas.panel2.Y, GameCanvas.panel2.W, GameCanvas.panel2.H);
												if (flag28)
												{
													GameCanvas.panel2.updateKey();
												}
											}
										}
									}
									bool flag29 = GameCanvas.isPointer(GameCanvas.panel.X + GameCanvas.panel.W, GameCanvas.panel.Y, GameCanvas.w - GameCanvas.panel.W * 2, GameCanvas.panel.H) && GameCanvas.isPointerJustRelease && GameCanvas.panel.isDoneCombine;
									if (flag29)
									{
										GameCanvas.panel.hide();
									}
								}
							}
						}
					}
				}
				GameCanvas.debug("E", 0);
				bool flag30 = !GameCanvas.isLoading;
				if (flag30)
				{
					GameCanvas.currentScreen.update();
				}
				GameCanvas.debug("F", 0);
				bool flag31 = !GameCanvas.panel.isShow && ChatPopup.serverChatPopUp == null;
				if (flag31)
				{
					GameCanvas.currentScreen.updateKey();
				}
				Hint.update();
				SoundMn.gI().update();
			}
			GameCanvas.debug("Ix", 0);
			Timer.update();
			GameCanvas.debug("Hx", 0);
			InfoDlg.update();
			GameCanvas.debug("G", 0);
			bool flag32 = this.resetToLoginScr;
			if (flag32)
			{
				this.resetToLoginScr = false;
				this.doResetToLoginScr(GameCanvas.loginScr);
			}
			GameCanvas.debug("Zzz", 0);
			bool flag33 = (GameCanvas.currentScreen != GameCanvas.serverScr || !GameCanvas.serverScr.isPaintNewUi) && Controller.isConnectOK;
			if (flag33)
			{
				bool isMain = Controller.isMain;
				if (isMain)
				{
					ServerListScreen.testConnect = 2;
					Service.gI().setClientType();
					Service.gI().androidPack();
				}
				else
				{
					Service.gI().setClientType2();
					Service.gI().androidPack2();
				}
				Controller.isConnectOK = false;
			}
			bool isDisconnected = Controller.isDisconnected;
			if (isDisconnected)
			{
				bool flag34 = !Controller.isMain;
				if (flag34)
				{
					bool flag35 = GameCanvas.currentScreen == GameCanvas.serverScreen && !Service.reciveFromMainSession;
					if (flag35)
					{
						GameCanvas.serverScreen.cancel();
					}
					bool flag36 = GameCanvas.currentScreen == GameCanvas.loginScr && !Service.reciveFromMainSession;
					if (flag36)
					{
						this.onDisconnected();
					}
				}
				else
				{
					this.onDisconnected();
				}
				Controller.isDisconnected = false;
			}
			bool isConnectionFail = Controller.isConnectionFail;
			if (isConnectionFail)
			{
				bool flag37 = !Controller.isMain;
				if (flag37)
				{
					bool flag38 = GameCanvas.currentScreen == GameCanvas.serverScreen && ServerListScreen.isGetData && !Service.reciveFromMainSession;
					if (flag38)
					{
						ServerListScreen.testConnect = 0;
						GameCanvas.serverScreen.cancel();
						Debug.Log("connect fail 1");
					}
					bool flag39 = GameCanvas.currentScreen == GameCanvas.loginScr && !Service.reciveFromMainSession;
					if (flag39)
					{
						this.onConnectionFail();
						Debug.Log("connect fail 2");
					}
				}
				else
				{
					bool flag40 = Session_ME.gI().isCompareIPConnect();
					if (flag40)
					{
						this.onConnectionFail();
					}
					Debug.Log("connect fail 3");
				}
				Controller.isConnectionFail = false;
			}
			bool flag41 = Main.isResume;
			if (flag41)
			{
				Main.isResume = false;
				bool flag42 = GameCanvas.currentDialog != null && GameCanvas.currentDialog.left != null && GameCanvas.currentDialog.left.actionListener != null;
				if (flag42)
				{
					GameCanvas.currentDialog.left.performAction();
				}
			}
			bool flag43 = GameCanvas.currentScreen != null && GameCanvas.currentScreen is GameScr;
			if (flag43)
			{
				GameCanvas.xThongBaoTranslate += GameCanvas.dir_ * 2;
				bool flag44 = GameCanvas.xThongBaoTranslate - Panel.imgNew.getWidth() <= 60;
				if (flag44)
				{
					GameCanvas.dir_ = 0;
					this.tickWaitThongBao++;
					bool flag45 = this.tickWaitThongBao > 150;
					if (flag45)
					{
						this.tickWaitThongBao = 0;
						GameCanvas.thongBaoTest = null;
					}
				}
			}
			bool flag46 = GameCanvas.currentScreen != null && GameCanvas.currentScreen.Equals(GameScr.gI());
			if (flag46)
			{
				bool flag47 = GameScr.info1 != null;
				if (flag47)
				{
					GameScr.info1.update();
				}
				bool flag48 = GameScr.info2 != null;
				if (flag48)
				{
					GameScr.info2.update();
				}
			}
			GameCanvas.isPointerSelect = false;
		}
		catch (Exception)
		{
		}
	}

	// Token: 0x06000405 RID: 1029 RVA: 0x0004D854 File Offset: 0x0004BA54
	public void onDisconnected()
	{
		bool isConnectionFail = Controller.isConnectionFail;
		if (isConnectionFail)
		{
			Controller.isConnectionFail = false;
		}
		GameCanvas.isResume = true;
		Session_ME.gI().clearSendingMessage();
		Session_ME2.gI().clearSendingMessage();
		Session_ME.gI().close();
		Session_ME2.gI().close();
		bool isLoadingData = Controller.isLoadingData;
		if (isLoadingData)
		{
			GameCanvas.startOK(mResources.pls_restart_game_error, 8885, null);
			Controller.isDisconnected = false;
		}
		else
		{
			Debug.LogError(">>>>onDisconnected");
			bool flag = GameCanvas.currentScreen != GameCanvas.serverScreen;
			if (flag)
			{
				GameCanvas.serverScreen.switchToMe();
				GameCanvas.startOK(mResources.maychutathoacmatsong + " [4]", 8884, null);
				Main.exit();
			}
			else
			{
				GameCanvas.endDlg();
			}
			global::Char.isLoadingMap = false;
			bool isMain = Controller.isMain;
			if (isMain)
			{
				ServerListScreen.testConnect = 0;
			}
			mSystem.endKey();
		}
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x0004D93C File Offset: 0x0004BB3C
	public void onConnectionFail()
	{
		bool flag = GameCanvas.currentScreen.Equals(SplashScr.instance);
		if (flag)
		{
			GameCanvas.startOK(mResources.maychutathoacmatsong + " [1]", 8884, null);
		}
		else
		{
			Session_ME.gI().clearSendingMessage();
			Session_ME2.gI().clearSendingMessage();
			ServerListScreen.isWait = false;
			bool isLoadingData = Controller.isLoadingData;
			if (isLoadingData)
			{
				GameCanvas.startOK(mResources.maychutathoacmatsong + " [2]", 8884, null);
				Controller.isConnectionFail = false;
			}
			else
			{
				GameCanvas.isResume = true;
				LoginScr.isContinueToLogin = false;
				LoginScr.serverName = ServerListScreen.nameServer[ServerListScreen.ipSelect];
				bool flag2 = GameCanvas.currentScreen != GameCanvas.serverScreen;
				if (flag2)
				{
					ServerListScreen.countDieConnect = 0;
				}
				else
				{
					GameCanvas.endDlg();
					ServerListScreen.loadScreen = true;
					GameCanvas.serverScreen.switchToMe();
				}
				global::Char.isLoadingMap = false;
				bool isMain = Controller.isMain;
				if (isMain)
				{
					ServerListScreen.testConnect = 0;
				}
				mSystem.endKey();
			}
		}
	}

	// Token: 0x06000407 RID: 1031 RVA: 0x0004DA38 File Offset: 0x0004BC38
	public static bool isWaiting()
	{
		return InfoDlg.isShow || (GameCanvas.msgdlg != null && GameCanvas.msgdlg.info.Equals(mResources.PLEASEWAIT)) || global::Char.isLoadingMap || LoginScr.isContinueToLogin;
	}

	// Token: 0x06000408 RID: 1032 RVA: 0x0004DA88 File Offset: 0x0004BC88
	public static void connect()
	{
		bool flag = !Session_ME.gI().isConnected();
		if (flag)
		{
			Session_ME.gI().connect(GameMidlet.IP, GameMidlet.PORT);
		}
	}

	// Token: 0x06000409 RID: 1033 RVA: 0x0004DAC0 File Offset: 0x0004BCC0
	public static void connect2()
	{
		bool flag = !Session_ME2.gI().isConnected();
		if (flag)
		{
			Res.outz("IP2= " + GameMidlet.IP2 + " PORT 2= " + GameMidlet.PORT2.ToString());
			Session_ME2.gI().connect(GameMidlet.IP2, GameMidlet.PORT2);
		}
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x00004AAD File Offset: 0x00002CAD
	public static void resetTrans(mGraphics g)
	{
		g.translate(-g.getTranslateX(), -g.getTranslateY());
		g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x0004DB1C File Offset: 0x0004BD1C
	public static void resetTransGameScr(mGraphics g)
	{
		g.translate(-g.getTranslateX(), -g.getTranslateY());
		g.translate(0, 0);
		g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
		g.translate(-GameScr.cmx, -GameScr.cmy);
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x0004DB70 File Offset: 0x0004BD70
	public void initGameCanvas()
	{
		GameCanvas.debug("SP2i1", 0);
		GameCanvas.w = MotherCanvas.instance.getWidthz();
		GameCanvas.h = MotherCanvas.instance.getHeightz();
		GameCanvas.debug("SP2i2", 0);
		GameCanvas.hw = GameCanvas.w / 2;
		GameCanvas.hh = GameCanvas.h / 2;
		GameCanvas.wd3 = GameCanvas.w / 3;
		GameCanvas.hd3 = GameCanvas.h / 3;
		GameCanvas.w2d3 = 2 * GameCanvas.w / 3;
		GameCanvas.h2d3 = 2 * GameCanvas.h / 3;
		GameCanvas.w3d4 = 3 * GameCanvas.w / 4;
		GameCanvas.h3d4 = 3 * GameCanvas.h / 4;
		GameCanvas.wd6 = GameCanvas.w / 6;
		GameCanvas.hd6 = GameCanvas.h / 6;
		GameCanvas.debug("SP2i3", 0);
		mScreen.initPos();
		GameCanvas.debug("SP2i4", 0);
		GameCanvas.debug("SP2i5", 0);
		GameCanvas.inputDlg = new InputDlg();
		GameCanvas.debug("SP2i6", 0);
		GameCanvas.listPoint = new MyVector();
		GameCanvas.debug("SP2i7", 0);
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00003E4C File Offset: 0x0000204C
	public void start()
	{
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x0004DC8C File Offset: 0x0004BE8C
	public int getWidth()
	{
		return (int)ScaleGUI.WIDTH;
	}

	// Token: 0x0600040F RID: 1039 RVA: 0x0004DCA4 File Offset: 0x0004BEA4
	public int getHeight()
	{
		return (int)ScaleGUI.HEIGHT;
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x00003E4C File Offset: 0x0000204C
	public static void debug(string s, int type)
	{
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x0004DCBC File Offset: 0x0004BEBC
	public void doResetToLoginScr(mScreen screen)
	{
		try
		{
			SoundMn.gI().stopAll();
			LoginScr.isContinueToLogin = false;
			TileMap.lastType = (TileMap.bgType = 0);
			global::Char.clearMyChar();
			GameScr.clearGameScr();
			GameScr.resetAllvector();
			InfoDlg.hide();
			GameScr.info1.hide();
			GameScr.info2.hide();
			GameScr.info2.cmdChat = null;
			Hint.isShow = false;
			ChatPopup.currChatPopup = null;
			Controller.isStopReadMessage = false;
			GameScr.loadCamera(true, -1, -1);
			GameScr.cmx = 100;
			GameCanvas.panel.currentTabIndex = 0;
			GameCanvas.panel.selected = (GameCanvas.isTouch ? (-1) : 0);
			GameCanvas.panel.init();
			GameCanvas.panel2 = null;
			GameScr.isPaint = true;
			ClanMessage.vMessage.removeAllElements();
			GameScr.textTime.removeAllElements();
			GameScr.vClan.removeAllElements();
			GameScr.vFriend.removeAllElements();
			GameScr.vEnemies.removeAllElements();
			TileMap.vCurrItem.removeAllElements();
			BackgroudEffect.vBgEffect.removeAllElements();
			EffecMn.vEff.removeAllElements();
			Effect.newEff.removeAllElements();
			GameCanvas.menu.showMenu = false;
			GameCanvas.panel.vItemCombine.removeAllElements();
			GameCanvas.panel.isShow = false;
			bool flag = GameCanvas.panel.tabIcon != null;
			if (flag)
			{
				GameCanvas.panel.tabIcon.isShow = false;
			}
			bool flag2 = mGraphics.zoomLevel == 1;
			if (flag2)
			{
				SmallImage.clearHastable();
			}
			Session_ME.gI().close();
			Session_ME2.gI().close();
		}
		catch (Exception ex)
		{
			Cout.println("Loi tai doResetToLoginScr " + ex.ToString());
		}
		ServerListScreen.isAutoConect = true;
		ServerListScreen.countDieConnect = 0;
		ServerListScreen.testConnect = -1;
		ServerListScreen.loadScreen = true;
		bool flag3 = ServerListScreen.ipSelect == -1;
		if (flag3)
		{
			GameCanvas.serverScr.switchToMe();
		}
		else
		{
			bool flag4 = GameCanvas.serverScreen == null;
			if (flag4)
			{
				GameCanvas.serverScreen = new ServerListScreen();
			}
			GameCanvas.serverScreen.switchToMe();
		}
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x00003E4C File Offset: 0x0000204C
	public static void showErrorForm(int type, string moreInfo)
	{
	}

	// Token: 0x06000413 RID: 1043 RVA: 0x00003E4C File Offset: 0x0000204C
	public static void paintCloud(mGraphics g)
	{
	}

	// Token: 0x06000414 RID: 1044 RVA: 0x00003E4C File Offset: 0x0000204C
	public static void updateBG()
	{
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x0004DEE4 File Offset: 0x0004C0E4
	public static void fillRect(mGraphics g, int color, int x, int y, int w, int h, int detalY)
	{
		g.setColor(color);
		int cmy = GameScr.cmy;
		bool flag = cmy > GameCanvas.h;
		if (flag)
		{
			cmy = GameCanvas.h;
		}
		g.fillRect(x, y - ((detalY != 0) ? (cmy >> detalY) : 0), w, h + ((detalY != 0) ? (cmy >> detalY) : 0));
	}

	// Token: 0x06000416 RID: 1046 RVA: 0x0004DF40 File Offset: 0x0004C140
	public static void paintBackgroundtLayer(mGraphics g, int layer, int deltaY, int color1, int color2)
	{
		try
		{
			int num = layer - 1;
			bool flag = num == GameCanvas.imgBG.Length - 1 && (GameScr.gI().isRongThanXuatHien || GameScr.gI().isFireWorks);
			if (flag)
			{
				g.setColor(GameScr.gI().mautroi);
				g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
				bool flag2 = GameCanvas.typeBg == 2 || GameCanvas.typeBg == 4 || GameCanvas.typeBg == 7;
				if (flag2)
				{
					GameCanvas.drawSun1(g);
					GameCanvas.drawSun2(g);
				}
				bool flag3 = GameScr.gI().isFireWorks && !GameCanvas.lowGraphic;
				if (flag3)
				{
					FireWorkEff.paint(g);
				}
			}
			else
			{
				bool flag4 = GameCanvas.imgBG == null || GameCanvas.imgBG[num] == null;
				if (!flag4)
				{
					bool flag5 = GameCanvas.moveX[num] != 0;
					if (flag5)
					{
						GameCanvas.moveX[num] += GameCanvas.moveXSpeed[num];
					}
					int cmy = GameScr.cmy;
					bool flag6 = cmy > GameCanvas.h;
					if (flag6)
					{
						cmy = GameCanvas.h;
					}
					bool flag7 = GameCanvas.layerSpeed[num] != 0;
					if (flag7)
					{
						for (int i = -((GameScr.cmx + GameCanvas.moveX[num] >> GameCanvas.layerSpeed[num]) % GameCanvas.bgW[num]); i < GameScr.gW; i += GameCanvas.bgW[num])
						{
							g.drawImage(GameCanvas.imgBG[num], i, GameCanvas.yb[num] - ((deltaY > 0) ? (cmy >> deltaY) : 0), 0);
						}
					}
					else
					{
						for (int j = 0; j < GameScr.gW; j += GameCanvas.bgW[num])
						{
							g.drawImage(GameCanvas.imgBG[num], j, GameCanvas.yb[num] - ((deltaY > 0) ? (cmy >> deltaY) : 0), 0);
						}
					}
					bool flag8 = color1 != -1;
					if (flag8)
					{
						bool flag9 = num == GameCanvas.nBg - 1;
						if (flag9)
						{
							GameCanvas.fillRect(g, color1, 0, -(cmy >> deltaY), GameScr.gW, GameCanvas.yb[num], deltaY);
						}
						else
						{
							GameCanvas.fillRect(g, color1, 0, GameCanvas.yb[num - 1] + GameCanvas.bgH[num - 1], GameScr.gW, GameCanvas.yb[num] - (GameCanvas.yb[num - 1] + GameCanvas.bgH[num - 1]), deltaY);
						}
					}
					bool flag10 = color2 != -1;
					if (flag10)
					{
						bool flag11 = num == 0;
						if (flag11)
						{
							GameCanvas.fillRect(g, color2, 0, GameCanvas.yb[num] + GameCanvas.bgH[num], GameScr.gW, GameScr.gH - (GameCanvas.yb[num] + GameCanvas.bgH[num]), deltaY);
						}
						else
						{
							GameCanvas.fillRect(g, color2, 0, GameCanvas.yb[num] + GameCanvas.bgH[num], GameScr.gW, GameCanvas.yb[num - 1] - (GameCanvas.yb[num] + GameCanvas.bgH[num]) + 80, deltaY);
						}
					}
					bool flag12 = GameCanvas.currentScreen == GameScr.instance;
					if (flag12)
					{
						bool flag13 = layer == 1 && GameCanvas.typeBg == 11;
						if (flag13)
						{
							g.drawImage(GameCanvas.imgSun2, -(GameScr.cmx >> GameCanvas.layerSpeed[0]) + 400, GameCanvas.yb[0] + 30 - (cmy >> 2), StaticObj.BOTTOM_HCENTER);
						}
						bool flag14 = layer == 1 && GameCanvas.typeBg == 13;
						if (flag14)
						{
							g.drawImage(GameCanvas.imgBG[1], -(GameScr.cmx >> GameCanvas.layerSpeed[0]) + TileMap.tmw * 24 / 4, GameCanvas.yb[0] - (cmy >> 3) + 30, 0);
							g.drawRegion(GameCanvas.imgBG[1], 0, 0, GameCanvas.bgW[1], GameCanvas.bgH[1], 2, -(GameScr.cmx >> GameCanvas.layerSpeed[0]) + TileMap.tmw * 24 / 4 + GameCanvas.bgW[1], GameCanvas.yb[0] - (cmy >> 3) + 30, 0);
						}
						bool flag15 = layer == 3 && TileMap.mapID == 1;
						if (flag15)
						{
							for (int k = 0; k < TileMap.pxh / mGraphics.getImageHeight(GameCanvas.imgCaycot); k++)
							{
								g.drawImage(GameCanvas.imgCaycot, -(GameScr.cmx >> GameCanvas.layerSpeed[2]) + 300, k * mGraphics.getImageHeight(GameCanvas.imgCaycot) - (cmy >> 3), 0);
							}
						}
					}
					int num2 = -(GameScr.cmx + GameCanvas.moveX[num] >> GameCanvas.layerSpeed[num]);
					EffecMn.paintBackGroundUnderLayer(g, num2, GameCanvas.yb[num] + GameCanvas.bgH[num] - (cmy >> deltaY), num);
				}
			}
		}
		catch (Exception ex)
		{
			Cout.LogError("Loi ham paint bground: " + ex.ToString());
		}
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x0004E43C File Offset: 0x0004C63C
	public static void drawSun1(mGraphics g)
	{
		bool flag = GameCanvas.imgSun != null;
		if (flag)
		{
			g.drawImage(GameCanvas.imgSun, GameCanvas.sunX, GameCanvas.sunY, 0);
		}
		bool flag2 = !GameCanvas.isBoltEff;
		if (!flag2)
		{
			bool flag3 = GameCanvas.gameTick % 200 == 0;
			if (flag3)
			{
				GameCanvas.boltActive = true;
			}
			bool flag4 = GameCanvas.boltActive;
			if (flag4)
			{
				GameCanvas.tBolt++;
				bool flag5 = GameCanvas.tBolt == 10;
				if (flag5)
				{
					GameCanvas.tBolt = 0;
					GameCanvas.boltActive = false;
				}
				bool flag6 = GameCanvas.tBolt % 2 == 0;
				if (flag6)
				{
					g.setColor(16777215);
					g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
				}
			}
		}
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x0004E500 File Offset: 0x0004C700
	public static void drawSun2(mGraphics g)
	{
		bool flag = GameCanvas.imgSun2 != null;
		if (flag)
		{
			g.drawImage(GameCanvas.imgSun2, GameCanvas.sunX2, GameCanvas.sunY2, 0);
		}
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x0004E534 File Offset: 0x0004C734
	public static bool isHDVersion()
	{
		return mGraphics.zoomLevel > 1;
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x0004E558 File Offset: 0x0004C758
	public static void paint_ios_bg(mGraphics g)
	{
		bool flag = mSystem.clientType != 5;
		if (!flag)
		{
			bool flag2 = GameCanvas.imgBgIOS != null;
			if (flag2)
			{
				g.setColor(0);
				g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
				for (int i = 0; i < 3; i++)
				{
					g.drawImage(GameCanvas.imgBgIOS, GameCanvas.imgBgIOS.getWidth() * i, GameCanvas.h / 2, mGraphics.VCENTER | mGraphics.HCENTER);
				}
			}
			else
			{
				GameCanvas.imgBgIOS = mSystem.loadImage("/bg/bg_ios_" + ((TileMap.bgID % 2 != 0) ? 1 : 2).ToString() + ".png");
			}
		}
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x0004E614 File Offset: 0x0004C814
	public static void paintBGGameScr(mGraphics g)
	{
		bool flag = !GameCanvas.isLoadBGok;
		if (flag)
		{
			g.setColor(0);
			g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
		}
		bool isLoadingMap = global::Char.isLoadingMap;
		if (!isLoadingMap)
		{
			int gW = GameScr.gW;
			int gH = GameScr.gH;
			g.translate(-g.getTranslateX(), -g.getTranslateY());
			g.setColor(8421504);
			g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
		}
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x00003E4C File Offset: 0x0000204C
	public static void resetBg()
	{
	}

	// Token: 0x0600041D RID: 1053 RVA: 0x0004E69C File Offset: 0x0004C89C
	public static void getYBackground(int typeBg)
	{
		try
		{
			int gH = GameScr.gH23;
			switch (typeBg)
			{
			case 0:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 70;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 20;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 30;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 50;
				goto IL_0688;
			case 1:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 120;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 40;
				GameCanvas.yb[2] = GameCanvas.yb[1] - 90;
				GameCanvas.yb[3] = GameCanvas.yb[2] - 25;
				goto IL_0688;
			case 2:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 150;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] - 60;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 40;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] - 10;
				GameCanvas.yb[4] = GameCanvas.yb[3] - GameCanvas.bgH[4];
				goto IL_0688;
			case 3:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 10;
				GameCanvas.yb[1] = GameCanvas.yb[0] + 80;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 10;
				goto IL_0688;
			case 4:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 130;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1];
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 20;
				GameCanvas.yb[3] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 80;
				goto IL_0688;
			case 5:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 40;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 10;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 15;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 50;
				goto IL_0688;
			case 6:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 100;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] - 30;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 10;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 15;
				GameCanvas.yb[4] = GameCanvas.yb[3] - GameCanvas.bgH[4] + 15;
				goto IL_0688;
			case 7:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 20;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 15;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 20;
				GameCanvas.yb[3] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 10;
				goto IL_0688;
			case 8:
			{
				GameCanvas.yb[0] = gH - 103 + 150;
				bool flag = TileMap.mapID == 103;
				if (flag)
				{
					GameCanvas.yb[0] -= 100;
				}
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] - 10;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 40;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 10;
				goto IL_0688;
			}
			case 9:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 100;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 22;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 50;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3];
				goto IL_0688;
			case 10:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] - 45;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] - 10;
				goto IL_0688;
			case 11:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 60;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 5;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 15;
				goto IL_0688;
			case 12:
				GameCanvas.yb[0] = gH + 40;
				GameCanvas.yb[1] = GameCanvas.yb[0] - 40;
				GameCanvas.yb[2] = GameCanvas.yb[1] - 40;
				goto IL_0688;
			case 13:
				GameCanvas.yb[0] = gH - 80;
				GameCanvas.yb[1] = GameCanvas.yb[0];
				goto IL_0688;
			case 15:
				GameCanvas.yb[0] = gH - 20;
				GameCanvas.yb[1] = GameCanvas.yb[0] - 80;
				goto IL_0688;
			case 16:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 75;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 50;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 50;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 90;
				goto IL_0688;
			case 19:
				GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 150;
				GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] - 60;
				GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] - 40;
				GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] - 10;
				GameCanvas.yb[4] = GameCanvas.yb[3] - GameCanvas.bgH[4];
				goto IL_0688;
			}
			GameCanvas.yb[0] = gH - GameCanvas.bgH[0] + 75;
			GameCanvas.yb[1] = GameCanvas.yb[0] - GameCanvas.bgH[1] + 50;
			GameCanvas.yb[2] = GameCanvas.yb[1] - GameCanvas.bgH[2] + 50;
			GameCanvas.yb[3] = GameCanvas.yb[2] - GameCanvas.bgH[3] + 90;
			IL_0688:;
		}
		catch (Exception)
		{
			int gH2 = GameScr.gH23;
			for (int i = 0; i < GameCanvas.yb.Length; i++)
			{
				GameCanvas.yb[i] = 1;
			}
		}
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x0004ED84 File Offset: 0x0004CF84
	public static void loadBG(int typeBG)
	{
		try
		{
			GameCanvas.isLoadBGok = true;
			bool flag = GameCanvas.typeBg == 12;
			if (flag)
			{
				BackgroudEffect.yfog = TileMap.pxh - 100;
			}
			else
			{
				BackgroudEffect.yfog = TileMap.pxh - 160;
			}
			BackgroudEffect.clearImage();
			GameCanvas.randomRaintEff(typeBG);
			bool flag2 = (TileMap.lastBgID == typeBG && TileMap.lastType == TileMap.bgType) || typeBG == -1;
			if (!flag2)
			{
				GameCanvas.transY = 12;
				TileMap.lastBgID = (int)((sbyte)typeBG);
				TileMap.lastType = (int)((sbyte)TileMap.bgType);
				GameCanvas.layerSpeed = new int[] { 1, 2, 3, 7, 8 };
				GameCanvas.moveX = new int[5];
				GameCanvas.moveXSpeed = new int[5];
				GameCanvas.typeBg = typeBG;
				GameCanvas.isBoltEff = false;
				GameScr.firstY = GameScr.cmy;
				GameCanvas.imgBG = null;
				GameCanvas.imgCloud = null;
				GameCanvas.imgSun = null;
				GameCanvas.imgCaycot = null;
				GameScr.firstY = -1;
				switch (GameCanvas.typeBg)
				{
				case 0:
				{
					GameCanvas.imgCaycot = GameCanvas.loadImageRMS("/bg/caycot.png");
					GameCanvas.layerSpeed = new int[] { 1, 3, 5, 7 };
					GameCanvas.nBg = 4;
					bool flag3 = TileMap.bgType == 2;
					if (flag3)
					{
						GameCanvas.transY = 8;
					}
					goto IL_033E;
				}
				case 1:
					GameCanvas.transY = 7;
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 2:
				{
					int[] array = new int[5];
					array[2] = 1;
					GameCanvas.moveX = array;
					int[] array2 = new int[5];
					array2[2] = 2;
					GameCanvas.moveXSpeed = array2;
					GameCanvas.nBg = 5;
					goto IL_033E;
				}
				case 3:
					GameCanvas.nBg = 3;
					goto IL_033E;
				case 4:
				{
					BackgroudEffect.addEffect(3);
					int[] array3 = new int[5];
					array3[1] = 1;
					GameCanvas.moveX = array3;
					int[] array4 = new int[5];
					array4[1] = 1;
					GameCanvas.moveXSpeed = array4;
					GameCanvas.nBg = 4;
					goto IL_033E;
				}
				case 5:
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 6:
				{
					int[] array5 = new int[5];
					array5[0] = 1;
					GameCanvas.moveX = array5;
					int[] array6 = new int[5];
					array6[0] = 2;
					GameCanvas.moveXSpeed = array6;
					GameCanvas.nBg = 5;
					goto IL_033E;
				}
				case 7:
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 8:
					GameCanvas.transY = 8;
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 9:
					BackgroudEffect.addEffect(9);
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 10:
					GameCanvas.nBg = 2;
					goto IL_033E;
				case 11:
					GameCanvas.transY = 7;
					GameCanvas.layerSpeed[2] = 0;
					GameCanvas.nBg = 3;
					goto IL_033E;
				case 12:
				{
					int[] array7 = new int[5];
					array7[0] = 1;
					array7[1] = 1;
					GameCanvas.moveX = array7;
					int[] array8 = new int[5];
					array8[0] = 2;
					array8[1] = 1;
					GameCanvas.moveXSpeed = array8;
					GameCanvas.nBg = 3;
					goto IL_033E;
				}
				case 13:
					GameCanvas.nBg = 2;
					goto IL_033E;
				case 15:
					Res.outz("HELL");
					GameCanvas.nBg = 2;
					goto IL_033E;
				case 16:
					GameCanvas.layerSpeed = new int[] { 1, 3, 5, 7 };
					GameCanvas.nBg = 4;
					goto IL_033E;
				case 19:
				{
					int[] array9 = new int[5];
					array9[1] = 2;
					array9[2] = 1;
					GameCanvas.moveX = array9;
					int[] array10 = new int[5];
					array10[1] = 2;
					array10[2] = 1;
					GameCanvas.moveXSpeed = array10;
					GameCanvas.nBg = 5;
					goto IL_033E;
				}
				}
				GameCanvas.layerSpeed = new int[] { 1, 3, 5, 7 };
				GameCanvas.nBg = 4;
				IL_033E:
				bool flag4 = typeBG <= 16;
				if (flag4)
				{
					GameCanvas.skyColor = StaticObj.SKYCOLOR[GameCanvas.typeBg];
				}
				else
				{
					try
					{
						string text = "/bg/b" + GameCanvas.typeBg.ToString() + 3.ToString() + ".png";
						bool flag5 = TileMap.bgType != 0;
						if (flag5)
						{
							text = string.Concat(new string[]
							{
								"/bg/b",
								GameCanvas.typeBg.ToString(),
								3.ToString(),
								"-",
								TileMap.bgType.ToString(),
								".png"
							});
						}
						int[] array11 = new int[1];
						Image image = GameCanvas.loadImageRMS(text);
						image.getRGB(ref array11, 0, 1, mGraphics.getRealImageWidth(image) / 2, 0, 1, 1);
						GameCanvas.skyColor = array11[0];
					}
					catch (Exception)
					{
						GameCanvas.skyColor = StaticObj.SKYCOLOR[StaticObj.SKYCOLOR.Length - 1];
					}
				}
				GameCanvas.colorTop = new int[StaticObj.SKYCOLOR.Length];
				GameCanvas.colorBotton = new int[StaticObj.SKYCOLOR.Length];
				for (int i = 0; i < StaticObj.SKYCOLOR.Length; i++)
				{
					GameCanvas.colorTop[i] = StaticObj.SKYCOLOR[i];
					GameCanvas.colorBotton[i] = StaticObj.SKYCOLOR[i];
				}
				bool flag6 = GameCanvas.lowGraphic;
				if (flag6)
				{
					GameCanvas.tam = GameCanvas.loadImageRMS("/bg/b63.png");
				}
				else
				{
					GameCanvas.imgBG = new Image[GameCanvas.nBg];
					GameCanvas.bgW = new int[GameCanvas.nBg];
					GameCanvas.bgH = new int[GameCanvas.nBg];
					GameCanvas.colorBotton = new int[GameCanvas.nBg];
					GameCanvas.colorTop = new int[GameCanvas.nBg];
					bool flag7 = TileMap.bgType == 100;
					if (flag7)
					{
						GameCanvas.imgBG[0] = GameCanvas.loadImageRMS("/bg/b100.png");
						GameCanvas.imgBG[1] = GameCanvas.loadImageRMS("/bg/b100.png");
						GameCanvas.imgBG[2] = GameCanvas.loadImageRMS("/bg/b82-1.png");
						GameCanvas.imgBG[3] = GameCanvas.loadImageRMS("/bg/b93.png");
						for (int j = 0; j < GameCanvas.nBg; j++)
						{
							bool flag8 = GameCanvas.imgBG[j] != null;
							if (flag8)
							{
								int[] array12 = new int[1];
								GameCanvas.imgBG[j].getRGB(ref array12, 0, 1, mGraphics.getRealImageWidth(GameCanvas.imgBG[j]) / 2, 0, 1, 1);
								GameCanvas.colorTop[j] = array12[0];
								array12 = new int[1];
								GameCanvas.imgBG[j].getRGB(ref array12, 0, 1, mGraphics.getRealImageWidth(GameCanvas.imgBG[j]) / 2, mGraphics.getRealImageHeight(GameCanvas.imgBG[j]) - 1, 1, 1);
								GameCanvas.colorBotton[j] = array12[0];
								GameCanvas.bgW[j] = mGraphics.getImageWidth(GameCanvas.imgBG[j]);
								GameCanvas.bgH[j] = mGraphics.getImageHeight(GameCanvas.imgBG[j]);
							}
							else
							{
								bool flag9 = GameCanvas.nBg > 1;
								if (flag9)
								{
									GameCanvas.imgBG[j] = GameCanvas.loadImageRMS("/bg/b" + GameCanvas.typeBg.ToString() + "0.png");
									GameCanvas.bgW[j] = mGraphics.getImageWidth(GameCanvas.imgBG[j]);
									GameCanvas.bgH[j] = mGraphics.getImageHeight(GameCanvas.imgBG[j]);
								}
							}
						}
					}
					else
					{
						for (int k = 0; k < GameCanvas.nBg; k++)
						{
							string text2 = "/bg/b" + GameCanvas.typeBg.ToString() + k.ToString() + ".png";
							bool flag10 = TileMap.bgType != 0;
							if (flag10)
							{
								text2 = string.Concat(new string[]
								{
									"/bg/b",
									GameCanvas.typeBg.ToString(),
									k.ToString(),
									"-",
									TileMap.bgType.ToString(),
									".png"
								});
							}
							GameCanvas.imgBG[k] = GameCanvas.loadImageRMS(text2);
							bool flag11 = GameCanvas.imgBG[k] != null;
							if (flag11)
							{
								int[] array13 = new int[1];
								GameCanvas.imgBG[k].getRGB(ref array13, 0, 1, mGraphics.getRealImageWidth(GameCanvas.imgBG[k]) / 2, 0, 1, 1);
								GameCanvas.colorTop[k] = array13[0];
								array13 = new int[1];
								GameCanvas.imgBG[k].getRGB(ref array13, 0, 1, mGraphics.getRealImageWidth(GameCanvas.imgBG[k]) / 2, mGraphics.getRealImageHeight(GameCanvas.imgBG[k]) - 1, 1, 1);
								GameCanvas.colorBotton[k] = array13[0];
								GameCanvas.bgW[k] = mGraphics.getImageWidth(GameCanvas.imgBG[k]);
								GameCanvas.bgH[k] = mGraphics.getImageHeight(GameCanvas.imgBG[k]);
							}
							else
							{
								bool flag12 = GameCanvas.nBg > 1;
								if (flag12)
								{
									GameCanvas.imgBG[k] = GameCanvas.loadImageRMS("/bg/b" + GameCanvas.typeBg.ToString() + "0.png");
									GameCanvas.bgW[k] = mGraphics.getImageWidth(GameCanvas.imgBG[k]);
									GameCanvas.bgH[k] = mGraphics.getImageHeight(GameCanvas.imgBG[k]);
								}
							}
						}
					}
					GameCanvas.getYBackground(GameCanvas.typeBg);
					GameCanvas.cloudX = new int[]
					{
						GameScr.gW / 2 - 40,
						GameScr.gW / 2 + 40,
						GameScr.gW / 2 - 100,
						GameScr.gW / 2 - 80,
						GameScr.gW / 2 - 120
					};
					GameCanvas.cloudY = new int[] { 130, 100, 150, 140, 80 };
					GameCanvas.imgSunSpec = null;
					bool flag13 = GameCanvas.typeBg != 0;
					if (flag13)
					{
						bool flag14 = GameCanvas.typeBg == 2;
						if (flag14)
						{
							GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun0.png");
							GameCanvas.sunX = GameScr.gW / 2 + 50;
							GameCanvas.sunY = GameCanvas.yb[4] - 40;
							TileMap.imgWaterflow = GameCanvas.loadImageRMS("/tWater/wts");
						}
						else
						{
							bool flag15 = GameCanvas.typeBg == 19;
							if (flag15)
							{
								TileMap.imgWaterflow = GameCanvas.loadImageRMS("/tWater/water_flow_32");
							}
							else
							{
								bool flag16 = GameCanvas.typeBg == 4;
								if (flag16)
								{
									GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun2.png");
									GameCanvas.sunX = GameScr.gW / 2 + 30;
									GameCanvas.sunY = GameCanvas.yb[3];
								}
								else
								{
									bool flag17 = GameCanvas.typeBg == 7;
									if (flag17)
									{
										GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun3" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
										GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun4" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
										GameCanvas.sunX = GameScr.gW - GameScr.gW / 3;
										GameCanvas.sunY = GameCanvas.yb[3] - 80;
										GameCanvas.sunX2 = GameCanvas.sunX - 100;
										GameCanvas.sunY2 = GameCanvas.yb[3] - 30;
									}
									else
									{
										bool flag18 = GameCanvas.typeBg == 6;
										if (flag18)
										{
											GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun5" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
											GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun6" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
											GameCanvas.sunX = GameScr.gW - GameScr.gW / 3;
											GameCanvas.sunY = GameCanvas.yb[4];
											GameCanvas.sunX2 = GameCanvas.sunX - 100;
											GameCanvas.sunY2 = GameCanvas.yb[4] + 20;
										}
										else
										{
											bool flag19 = typeBG == 5;
											if (flag19)
											{
												GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun8" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
												GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun7" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
												GameCanvas.sunX = GameScr.gW / 2 - 50;
												GameCanvas.sunY = GameCanvas.yb[3] + 20;
												GameCanvas.sunX2 = GameScr.gW / 2 + 20;
												GameCanvas.sunY2 = GameCanvas.yb[3] - 30;
											}
											else
											{
												bool flag20 = GameCanvas.typeBg == 8 && TileMap.mapID < 90;
												if (flag20)
												{
													GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun9" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
													GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun10" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
													GameCanvas.sunX = GameScr.gW / 2 - 30;
													GameCanvas.sunY = GameCanvas.yb[3] + 60;
													GameCanvas.sunX2 = GameScr.gW / 2 + 20;
													GameCanvas.sunY2 = GameCanvas.yb[3] + 10;
												}
												else
												{
													switch (typeBG)
													{
													case 9:
														GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun11" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun12" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.sunX = GameScr.gW - GameScr.gW / 3;
														GameCanvas.sunY = GameCanvas.yb[4] + 20;
														GameCanvas.sunX2 = GameCanvas.sunX - 80;
														GameCanvas.sunY2 = GameCanvas.yb[4] + 40;
														goto IL_1119;
													case 10:
														GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun13" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/sun14" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.sunX = GameScr.gW - GameScr.gW / 3;
														GameCanvas.sunY = GameCanvas.yb[1] - 30;
														GameCanvas.sunX2 = GameCanvas.sunX - 80;
														GameCanvas.sunY2 = GameCanvas.yb[1];
														goto IL_1119;
													case 11:
														GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun15" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.imgSun2 = GameCanvas.loadImageRMS("/bg/b113" + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
														GameCanvas.sunX = GameScr.gW / 2 - 30;
														GameCanvas.sunY = GameCanvas.yb[2] - 30;
														goto IL_1119;
													case 12:
														GameCanvas.cloudY = new int[] { 200, 170, 220, 150, 250 };
														goto IL_1119;
													case 16:
													{
														GameCanvas.cloudX = new int[] { 90, 170, 250, 320, 400, 450, 500 };
														GameCanvas.cloudY = new int[]
														{
															GameCanvas.yb[2] + 5,
															GameCanvas.yb[2] - 20,
															GameCanvas.yb[2] - 50,
															GameCanvas.yb[2] - 30,
															GameCanvas.yb[2] - 50,
															GameCanvas.yb[2],
															GameCanvas.yb[2] - 40
														};
														GameCanvas.imgSunSpec = new Image[7];
														for (int l = 0; l < GameCanvas.imgSunSpec.Length; l++)
														{
															int num = 161;
															bool flag21 = l == 0 || l == 2 || l == 3 || l == 2 || l == 6;
															if (flag21)
															{
																num = 160;
															}
															GameCanvas.imgSunSpec[l] = GameCanvas.loadImageRMS("/bg/sun" + num.ToString() + ".png");
														}
														goto IL_1119;
													}
													case 19:
													{
														int[] array14 = new int[5];
														array14[1] = 2;
														array14[2] = 1;
														GameCanvas.moveX = array14;
														int[] array15 = new int[5];
														array15[1] = 2;
														array15[2] = 1;
														GameCanvas.moveXSpeed = array15;
														GameCanvas.nBg = 5;
														goto IL_1119;
													}
													}
													GameCanvas.imgCloud = null;
													GameCanvas.imgSun = null;
													GameCanvas.imgSun2 = null;
													GameCanvas.imgSun = GameCanvas.loadImageRMS("/bg/sun" + typeBG.ToString() + ((TileMap.bgType != 0) ? ("-" + TileMap.bgType.ToString()) : string.Empty) + ".png");
													bool flag22 = GameCanvas.loadImageRMS("/tWater/water_flow_" + typeBG.ToString()) != null;
													if (flag22)
													{
														TileMap.imgWaterflow = GameCanvas.loadImageRMS("/tWater/water_flow_" + typeBG.ToString());
													}
													GameCanvas.sunX = GameScr.gW - GameScr.gW / 3;
													GameCanvas.sunY = GameCanvas.yb[2] - 30;
													IL_1119:;
												}
											}
										}
									}
								}
							}
						}
					}
					GameCanvas.paintBG = false;
					bool flag23 = !GameCanvas.paintBG;
					if (flag23)
					{
						GameCanvas.paintBG = true;
					}
				}
			}
		}
		catch (Exception)
		{
			GameCanvas.isLoadBGok = false;
		}
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x0004FF0C File Offset: 0x0004E10C
	private static void randomRaintEff(int typeBG)
	{
		for (int i = 0; i < GameCanvas.bgRain.Length; i++)
		{
			bool flag = typeBG == GameCanvas.bgRain[i] && Res.random(0, 2) == 0;
			if (flag)
			{
				BackgroudEffect.addEffect(0);
				break;
			}
		}
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x0004FF58 File Offset: 0x0004E158
	public void keyPressedz(int keyCode)
	{
		GameCanvas.lastTimePress = mSystem.currentTimeMillis();
		bool flag = (keyCode >= 48 && keyCode <= 57) || (keyCode >= 65 && keyCode <= 122) || keyCode == 10 || keyCode == 8 || keyCode == 13 || keyCode == 32 || keyCode == 31;
		if (flag)
		{
			GameCanvas.keyAsciiPress = keyCode;
		}
		this.mapKeyPress(keyCode);
	}

	// Token: 0x06000421 RID: 1057 RVA: 0x0004FFB4 File Offset: 0x0004E1B4
	public void mapKeyPress(int keyCode)
	{
		bool flag = GameCanvas.currentDialog != null;
		if (flag)
		{
			GameCanvas.currentDialog.keyPress(keyCode);
			GameCanvas.keyAsciiPress = 0;
		}
		else
		{
			GameCanvas.currentScreen.keyPress(keyCode);
			if (keyCode <= -22)
			{
				if (keyCode <= -38)
				{
					if (keyCode == -39)
					{
						goto IL_0179;
					}
					if (keyCode != -38)
					{
						return;
					}
				}
				else
				{
					if (keyCode == -26)
					{
						GameCanvas.keyHold[16] = true;
						GameCanvas.keyPressed[16] = true;
						return;
					}
					if (keyCode != -22)
					{
						return;
					}
					goto IL_03FF;
				}
			}
			else
			{
				if (keyCode <= -1)
				{
					if (keyCode != -21)
					{
						switch (keyCode)
						{
						case -8:
							GameCanvas.keyHold[14] = true;
							GameCanvas.keyPressed[14] = true;
							return;
						case -7:
							goto IL_03FF;
						case -6:
							break;
						case -5:
							goto IL_0275;
						case -4:
						{
							bool flag2 = (GameCanvas.currentScreen is GameScr || GameCanvas.currentScreen is CrackBallScr) && global::Char.myCharz().isAttack;
							if (flag2)
							{
								GameCanvas.clearKeyHold();
								GameCanvas.clearKeyPressed();
							}
							else
							{
								GameCanvas.keyHold[24] = true;
								GameCanvas.keyPressed[24] = true;
							}
							return;
						}
						case -3:
						{
							bool flag3 = (GameCanvas.currentScreen is GameScr || GameCanvas.currentScreen is CrackBallScr) && global::Char.myCharz().isAttack;
							if (flag3)
							{
								GameCanvas.clearKeyHold();
								GameCanvas.clearKeyPressed();
							}
							else
							{
								GameCanvas.keyHold[23] = true;
								GameCanvas.keyPressed[23] = true;
							}
							return;
						}
						case -2:
							goto IL_0179;
						case -1:
							goto IL_0127;
						default:
							return;
						}
					}
					GameCanvas.keyHold[12] = true;
					GameCanvas.keyPressed[12] = true;
					return;
				}
				if (keyCode != 10)
				{
					switch (keyCode)
					{
					case 35:
						GameCanvas.keyHold[11] = true;
						GameCanvas.keyPressed[11] = true;
						return;
					case 36:
					case 37:
					case 38:
					case 39:
					case 40:
					case 41:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
						return;
					case 42:
						GameCanvas.keyHold[10] = true;
						GameCanvas.keyPressed[10] = true;
						return;
					case 48:
						GameCanvas.keyHold[0] = true;
						GameCanvas.keyPressed[0] = true;
						return;
					case 49:
					{
						bool flag4 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag4)
						{
							GameCanvas.keyHold[1] = true;
							GameCanvas.keyPressed[1] = true;
						}
						return;
					}
					case 50:
					{
						bool flag5 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag5)
						{
							GameCanvas.keyHold[2] = true;
							GameCanvas.keyPressed[2] = true;
						}
						return;
					}
					case 51:
					{
						bool flag6 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag6)
						{
							GameCanvas.keyHold[3] = true;
							GameCanvas.keyPressed[3] = true;
						}
						return;
					}
					case 52:
					{
						bool flag7 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag7)
						{
							GameCanvas.keyHold[4] = true;
							GameCanvas.keyPressed[4] = true;
						}
						return;
					}
					case 53:
					{
						bool flag8 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag8)
						{
							GameCanvas.keyHold[5] = true;
							GameCanvas.keyPressed[5] = true;
						}
						return;
					}
					case 54:
					{
						bool flag9 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag9)
						{
							GameCanvas.keyHold[6] = true;
							GameCanvas.keyPressed[6] = true;
						}
						return;
					}
					case 55:
						GameCanvas.keyHold[7] = true;
						GameCanvas.keyPressed[7] = true;
						return;
					case 56:
					{
						bool flag10 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
						if (flag10)
						{
							GameCanvas.keyHold[8] = true;
							GameCanvas.keyPressed[8] = true;
						}
						return;
					}
					case 57:
						GameCanvas.keyHold[9] = true;
						GameCanvas.keyPressed[9] = true;
						return;
					default:
						if (keyCode != 113)
						{
							return;
						}
						GameCanvas.keyHold[17] = true;
						GameCanvas.keyPressed[17] = true;
						return;
					}
				}
				IL_0275:
				bool flag11 = (GameCanvas.currentScreen is GameScr || GameCanvas.currentScreen is CrackBallScr) && global::Char.myCharz().isAttack;
				if (flag11)
				{
					GameCanvas.clearKeyHold();
					GameCanvas.clearKeyPressed();
					return;
				}
				GameCanvas.keyHold[25] = true;
				GameCanvas.keyPressed[25] = true;
				GameCanvas.keyHold[15] = true;
				GameCanvas.keyPressed[15] = true;
				return;
			}
			IL_0127:
			bool flag12 = (GameCanvas.currentScreen is GameScr || GameCanvas.currentScreen is CrackBallScr) && global::Char.myCharz().isAttack;
			if (flag12)
			{
				GameCanvas.clearKeyHold();
				GameCanvas.clearKeyPressed();
			}
			else
			{
				GameCanvas.keyHold[21] = true;
				GameCanvas.keyPressed[21] = true;
			}
			return;
			IL_0179:
			bool flag13 = (GameCanvas.currentScreen is GameScr || GameCanvas.currentScreen is CrackBallScr) && global::Char.myCharz().isAttack;
			if (flag13)
			{
				GameCanvas.clearKeyHold();
				GameCanvas.clearKeyPressed();
			}
			else
			{
				GameCanvas.keyHold[22] = true;
				GameCanvas.keyPressed[22] = true;
			}
			return;
			IL_03FF:
			GameCanvas.keyHold[13] = true;
			GameCanvas.keyPressed[13] = true;
		}
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x00004AD8 File Offset: 0x00002CD8
	public void keyReleasedz(int keyCode)
	{
		GameCanvas.keyAsciiPress = 0;
		this.mapKeyRelease(keyCode);
	}

	// Token: 0x06000423 RID: 1059 RVA: 0x0005059C File Offset: 0x0004E79C
	public void mapKeyRelease(int keyCode)
	{
		if (keyCode > -22)
		{
			if (keyCode <= -1)
			{
				if (keyCode != -21)
				{
					switch (keyCode)
					{
					case -8:
						GameCanvas.keyHold[14] = false;
						return;
					case -7:
						goto IL_0278;
					case -6:
						break;
					case -5:
						goto IL_012F;
					case -4:
						GameCanvas.keyHold[24] = false;
						return;
					case -3:
						GameCanvas.keyHold[23] = false;
						return;
					case -2:
						goto IL_0105;
					case -1:
						goto IL_00F7;
					default:
						return;
					}
				}
				GameCanvas.keyHold[12] = false;
				GameCanvas.keyReleased[12] = true;
				return;
			}
			if (keyCode != 10)
			{
				switch (keyCode)
				{
				case 35:
					GameCanvas.keyHold[11] = false;
					GameCanvas.keyReleased[11] = true;
					return;
				case 36:
				case 37:
				case 38:
				case 39:
				case 40:
				case 41:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
					return;
				case 42:
					GameCanvas.keyHold[10] = false;
					GameCanvas.keyReleased[10] = true;
					return;
				case 48:
					GameCanvas.keyHold[0] = false;
					GameCanvas.keyReleased[0] = true;
					return;
				case 49:
				{
					bool flag = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag)
					{
						GameCanvas.keyHold[1] = false;
						GameCanvas.keyReleased[1] = true;
					}
					return;
				}
				case 50:
				{
					bool flag2 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag2)
					{
						GameCanvas.keyHold[2] = false;
						GameCanvas.keyReleased[2] = true;
					}
					return;
				}
				case 51:
				{
					bool flag3 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag3)
					{
						GameCanvas.keyHold[3] = false;
						GameCanvas.keyReleased[3] = true;
					}
					return;
				}
				case 52:
				{
					bool flag4 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag4)
					{
						GameCanvas.keyHold[4] = false;
						GameCanvas.keyReleased[4] = true;
					}
					return;
				}
				case 53:
				{
					bool flag5 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag5)
					{
						GameCanvas.keyHold[5] = false;
						GameCanvas.keyReleased[5] = true;
					}
					return;
				}
				case 54:
				{
					bool flag6 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag6)
					{
						GameCanvas.keyHold[6] = false;
						GameCanvas.keyReleased[6] = true;
					}
					return;
				}
				case 55:
					GameCanvas.keyHold[7] = false;
					GameCanvas.keyReleased[7] = true;
					return;
				case 56:
				{
					bool flag7 = GameCanvas.currentScreen == CrackBallScr.instance || (GameCanvas.currentScreen == GameScr.instance && GameCanvas.isMoveNumberPad && !ChatTextField.gI().isShow);
					if (flag7)
					{
						GameCanvas.keyHold[8] = false;
						GameCanvas.keyReleased[8] = true;
					}
					return;
				}
				case 57:
					GameCanvas.keyHold[9] = false;
					GameCanvas.keyReleased[9] = true;
					return;
				default:
					if (keyCode != 113)
					{
						return;
					}
					GameCanvas.keyHold[17] = false;
					GameCanvas.keyReleased[17] = true;
					return;
				}
			}
			IL_012F:
			GameCanvas.keyHold[25] = false;
			GameCanvas.keyReleased[25] = true;
			GameCanvas.keyHold[15] = true;
			GameCanvas.keyPressed[15] = true;
			return;
		}
		if (keyCode <= -38)
		{
			if (keyCode == -39)
			{
				goto IL_0105;
			}
			if (keyCode != -38)
			{
				return;
			}
		}
		else
		{
			if (keyCode == -26)
			{
				GameCanvas.keyHold[16] = false;
				return;
			}
			if (keyCode != -22)
			{
				return;
			}
			goto IL_0278;
		}
		IL_00F7:
		GameCanvas.keyHold[21] = false;
		return;
		IL_0105:
		GameCanvas.keyHold[22] = false;
		return;
		IL_0278:
		GameCanvas.keyHold[13] = false;
		GameCanvas.keyReleased[13] = true;
	}

	// Token: 0x06000424 RID: 1060 RVA: 0x00004AE9 File Offset: 0x00002CE9
	public void pointerMouse(int x, int y)
	{
		GameCanvas.pxMouse = x;
		GameCanvas.pyMouse = y;
	}

	// Token: 0x06000425 RID: 1061 RVA: 0x000509E8 File Offset: 0x0004EBE8
	public void scrollMouse(int a)
	{
		GameCanvas.pXYScrollMouse = a;
		bool flag = GameCanvas.panel != null && GameCanvas.panel.isShow;
		if (flag)
		{
			GameCanvas.panel.updateScroolMouse(a);
		}
	}

	// Token: 0x06000426 RID: 1062 RVA: 0x00050A24 File Offset: 0x0004EC24
	public void pointerDragged(int x, int y)
	{
		GameCanvas.isPointerSelect = false;
		bool flag = Res.abs(x - GameCanvas.pxLast) >= 10 || Res.abs(y - GameCanvas.pyLast) >= 10;
		if (flag)
		{
			GameCanvas.isPointerClick = false;
			GameCanvas.isPointerDown = true;
			GameCanvas.isPointerMove = true;
		}
		GameCanvas.px = x;
		GameCanvas.py = y;
		GameCanvas.curPos++;
		bool flag2 = GameCanvas.curPos > 3;
		if (flag2)
		{
			GameCanvas.curPos = 0;
		}
		GameCanvas.arrPos[GameCanvas.curPos] = new Position(x, y);
	}

	// Token: 0x06000427 RID: 1063 RVA: 0x00050AB4 File Offset: 0x0004ECB4
	public static bool isHoldPress()
	{
		return mSystem.currentTimeMillis() - GameCanvas.lastTimePress >= 800L;
	}

	// Token: 0x06000428 RID: 1064 RVA: 0x00050AE8 File Offset: 0x0004ECE8
	public void pointerPressed(int x, int y)
	{
		GameCanvas.isPointerSelect = false;
		GameCanvas.isPointerJustRelease = false;
		GameCanvas.isPointerJustDown = true;
		GameCanvas.isPointerDown = true;
		GameCanvas.isPointerClick = false;
		GameCanvas.isPointerMove = false;
		GameCanvas.lastTimePress = mSystem.currentTimeMillis();
		GameCanvas.pxFirst = x;
		GameCanvas.pyFirst = y;
		GameCanvas.pxLast = x;
		GameCanvas.pyLast = y;
		GameCanvas.px = x;
		GameCanvas.py = y;
	}

	// Token: 0x06000429 RID: 1065 RVA: 0x00050B48 File Offset: 0x0004ED48
	public void pointerReleased(int x, int y)
	{
		bool flag = !GameCanvas.isPointerMove;
		if (flag)
		{
			GameCanvas.isPointerSelect = true;
		}
		GameCanvas.isPointerDown = false;
		GameCanvas.isPointerMove = false;
		GameCanvas.isPointerJustRelease = true;
		GameCanvas.isPointerClick = true;
		mScreen.keyTouch = -1;
		GameCanvas.px = x;
		GameCanvas.py = y;
	}

	// Token: 0x0600042A RID: 1066 RVA: 0x00050B94 File Offset: 0x0004ED94
	public static bool isPointerHoldIn(int x, int y, int w, int h)
	{
		bool flag = !GameCanvas.isPointerDown && !GameCanvas.isPointerJustRelease;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			bool flag3 = GameCanvas.px >= x && GameCanvas.px <= x + w && GameCanvas.py >= y && GameCanvas.py <= y + h;
			flag2 = flag3;
		}
		return flag2;
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x00050BF8 File Offset: 0x0004EDF8
	public static bool isPointSelect(int x, int y, int w, int h)
	{
		bool flag = !GameCanvas.isPointerSelect;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			bool flag3 = GameCanvas.px >= x && GameCanvas.px <= x + w && GameCanvas.py >= y && GameCanvas.py <= y + h;
			flag2 = flag3;
		}
		return flag2;
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00050C50 File Offset: 0x0004EE50
	public static bool isMouseFocus(int x, int y, int w, int h)
	{
		return GameCanvas.pxMouse >= x && GameCanvas.pxMouse <= x + w && GameCanvas.pyMouse >= y && GameCanvas.pyMouse <= y + h;
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00050C98 File Offset: 0x0004EE98
	public static void clearKeyPressed()
	{
		for (int i = 0; i < GameCanvas.keyPressed.Length; i++)
		{
			GameCanvas.keyPressed[i] = false;
		}
		GameCanvas.isPointerJustRelease = false;
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00050CCC File Offset: 0x0004EECC
	public static void clearKeyHold()
	{
		for (int i = 0; i < GameCanvas.keyHold.Length; i++)
		{
			GameCanvas.keyHold[i] = false;
		}
	}

	// Token: 0x0600042F RID: 1071 RVA: 0x00050CFC File Offset: 0x0004EEFC
	public static void checkBackButton()
	{
		bool flag = ChatPopup.serverChatPopUp == null && ChatPopup.currChatPopup == null;
		if (flag)
		{
			GameCanvas.startYesNoDlg(mResources.DOYOUWANTEXIT, new Command(mResources.YES, GameCanvas.instance, 8885, null), new Command(mResources.NO, GameCanvas.instance, 8882, null));
		}
	}

	// Token: 0x06000430 RID: 1072 RVA: 0x00050D58 File Offset: 0x0004EF58
	public void paintChangeMap(mGraphics g)
	{
		string empty = string.Empty;
		GameCanvas.resetTrans(g);
		g.setColor(0);
		g.fillRect(0, 0, GameCanvas.w, GameCanvas.h);
		g.drawImage(LoginScr.imgTitle, GameCanvas.w / 2, GameCanvas.h / 2 - 24, StaticObj.BOTTOM_HCENTER);
		GameCanvas.paintShukiren(GameCanvas.hw, GameCanvas.h / 2 + 24, g);
		mFont.tahoma_7b_white.drawString(g, mResources.PLEASEWAIT + ((LoginScr.timeLogin <= 0) ? empty : (" " + LoginScr.timeLogin.ToString() + "s")), GameCanvas.w / 2, GameCanvas.h / 2, 2);
	}

	// Token: 0x06000431 RID: 1073 RVA: 0x00050E10 File Offset: 0x0004F010
	public void paint(mGraphics gx)
	{
		try
		{
			GameCanvas.debugPaint.removeAllElements();
			GameCanvas.debug("PA", 1);
			bool flag = GameCanvas.currentScreen != null;
			if (flag)
			{
				GameCanvas.currentScreen.paint(this.g);
			}
			GameCanvas.debug("PB", 1);
			this.g.translate(-this.g.getTranslateX(), -this.g.getTranslateY());
			this.g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			bool isShow = GameCanvas.panel.isShow;
			if (isShow)
			{
				GameCanvas.panel.paint(this.g);
				bool flag2 = GameCanvas.panel2 != null && GameCanvas.panel2.isShow;
				if (flag2)
				{
					GameCanvas.panel2.paint(this.g);
				}
				bool flag3 = GameCanvas.panel.chatTField != null && GameCanvas.panel.chatTField.isShow;
				if (flag3)
				{
					GameCanvas.panel.chatTField.paint(this.g);
				}
				bool flag4 = GameCanvas.panel2 != null && GameCanvas.panel2.chatTField != null && GameCanvas.panel2.chatTField.isShow;
				if (flag4)
				{
					GameCanvas.panel2.chatTField.paint(this.g);
				}
			}
			Res.paintOnScreenDebug(this.g);
			InfoDlg.paint(this.g);
			bool flag5 = GameCanvas.currentDialog != null;
			if (flag5)
			{
				GameCanvas.debug("PC", 1);
				GameCanvas.currentDialog.paint(this.g);
			}
			else
			{
				bool showMenu = GameCanvas.menu.showMenu;
				if (showMenu)
				{
					GameCanvas.debug("PD", 1);
					GameCanvas.resetTrans(this.g);
					GameCanvas.menu.paintMenu(this.g);
				}
			}
			GameScr.info1.paint(this.g);
			GameScr.info2.paint(this.g);
			bool flag6 = GameScr.gI().popUpYesNo != null;
			if (flag6)
			{
				GameScr.gI().popUpYesNo.paint(this.g);
			}
			bool flag7 = ChatPopup.currChatPopup != null;
			if (flag7)
			{
				ChatPopup.currChatPopup.paint(this.g);
			}
			Hint.paint(this.g);
			bool flag8 = ChatPopup.serverChatPopUp != null;
			if (flag8)
			{
				ChatPopup.serverChatPopUp.paint(this.g);
			}
			for (int i = 0; i < Effect2.vEffect2.size(); i++)
			{
				Effect2 effect = (Effect2)Effect2.vEffect2.elementAt(i);
				bool flag9 = effect is ChatPopup && !effect.Equals(ChatPopup.currChatPopup) && !effect.Equals(ChatPopup.serverChatPopUp);
				if (flag9)
				{
					effect.paint(this.g);
				}
			}
			bool flag10 = GameCanvas.currentDialog != null;
			if (flag10)
			{
				GameCanvas.currentDialog.paint(this.g);
			}
			bool flag11 = GameCanvas.isWait();
			if (flag11)
			{
				this.paintChangeMap(this.g);
				bool flag12 = GameCanvas.timeLoading > 0 && LoginScr.timeLogin <= 0 && mSystem.currentTimeMillis() - GameCanvas.TIMEOUT >= 1000L;
				if (flag12)
				{
					GameCanvas.timeLoading--;
					bool flag13 = GameCanvas.timeLoading == 0;
					if (flag13)
					{
						GameCanvas.timeLoading = 15;
					}
					GameCanvas.TIMEOUT = mSystem.currentTimeMillis();
				}
			}
			GameCanvas.debug("PE", 1);
			GameCanvas.resetTrans(this.g);
			EffecMn.paintLayer4(this.g);
			bool flag14 = GameCanvas.open3Hour && !GameCanvas.isLoading;
			if (flag14)
			{
				bool flag15 = GameCanvas.currentScreen == GameCanvas.loginScr || GameCanvas.currentScreen == GameCanvas.serverScreen || GameCanvas.currentScreen == GameCanvas.serverScr;
				if (flag15)
				{
					this.g.drawImage(GameCanvas.img12, 5, 5, 0);
				}
				bool flag16 = GameCanvas.currentScreen == CreateCharScr.instance;
				if (flag16)
				{
					this.g.drawImage(GameCanvas.img12, 5, 20, 0);
				}
			}
			GameCanvas.resetTrans(this.g);
			int num = GameCanvas.h / 4;
			bool flag17 = GameCanvas.currentScreen != null && GameCanvas.currentScreen is GameScr && GameCanvas.thongBaoTest != null;
			if (flag17)
			{
				this.g.setClip(60, num, GameCanvas.w - 120, mFont.tahoma_7_white.getHeight() + 2);
				mFont.tahoma_7_grey.drawString(this.g, GameCanvas.thongBaoTest, GameCanvas.xThongBaoTranslate, num + 1, 0);
				mFont.tahoma_7_yellow.drawString(this.g, GameCanvas.thongBaoTest, GameCanvas.xThongBaoTranslate, num, 0);
				this.g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
			}
		}
		catch (Exception)
		{
		}
	}

	// Token: 0x06000432 RID: 1074 RVA: 0x00051314 File Offset: 0x0004F514
	public static void endDlg()
	{
		bool flag = GameCanvas.inputDlg != null;
		if (flag)
		{
			GameCanvas.inputDlg.tfInput.setMaxTextLenght(500);
		}
		GameCanvas.currentDialog = null;
		InfoDlg.hide();
	}

	// Token: 0x06000433 RID: 1075 RVA: 0x00051354 File Offset: 0x0004F554
	public static void startOKDlg(string info)
	{
		bool flag = info == "Không thể đổi khu vực trong map này";
		if (!flag)
		{
			GameCanvas.closeKeyBoard();
			GameCanvas.msgdlg.setInfo(info, null, new Command(mResources.OK, GameCanvas.instance, 8882, null), null);
			GameCanvas.currentDialog = GameCanvas.msgdlg;
		}
	}

	// Token: 0x06000434 RID: 1076 RVA: 0x000513A8 File Offset: 0x0004F5A8
	public static void startWaitDlg(string info)
	{
		GameCanvas.closeKeyBoard();
		GameCanvas.msgdlg.setInfo(info, null, new Command(mResources.CANCEL, GameCanvas.instance, 8882, null), null);
		GameCanvas.currentDialog = GameCanvas.msgdlg;
		GameCanvas.msgdlg.isWait = true;
	}

	// Token: 0x06000435 RID: 1077 RVA: 0x000513A8 File Offset: 0x0004F5A8
	public static void startOKDlg(string info, bool isError)
	{
		GameCanvas.closeKeyBoard();
		GameCanvas.msgdlg.setInfo(info, null, new Command(mResources.CANCEL, GameCanvas.instance, 8882, null), null);
		GameCanvas.currentDialog = GameCanvas.msgdlg;
		GameCanvas.msgdlg.isWait = true;
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x00004AF8 File Offset: 0x00002CF8
	public static void startWaitDlg()
	{
		GameCanvas.closeKeyBoard();
		global::Char.isLoadingMap = true;
	}

	// Token: 0x06000437 RID: 1079 RVA: 0x00004B07 File Offset: 0x00002D07
	public void openWeb(string strLeft, string strRight, string url, string str)
	{
		GameCanvas.msgdlg.setInfo(str, new Command(strLeft, this, 8881, url), null, new Command(strRight, this, 8882, null));
		GameCanvas.currentDialog = GameCanvas.msgdlg;
	}

	// Token: 0x06000438 RID: 1080 RVA: 0x00004B3C File Offset: 0x00002D3C
	public static void startOK(string info, int actionID, object p)
	{
		GameCanvas.closeKeyBoard();
		GameCanvas.msgdlg.setInfo(info, null, new Command(mResources.OK, GameCanvas.instance, actionID, p), null);
		GameCanvas.msgdlg.show();
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x000513F4 File Offset: 0x0004F5F4
	public static void startYesNoDlg(string info, int iYes, object pYes, int iNo, object pNo)
	{
		GameCanvas.closeKeyBoard();
		GameCanvas.msgdlg.setInfo(info, new Command(mResources.YES, GameCanvas.instance, iYes, pYes), new Command(string.Empty, GameCanvas.instance, iYes, pYes), new Command(mResources.NO, GameCanvas.instance, iNo, pNo));
		GameCanvas.msgdlg.show();
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x00004B6F File Offset: 0x00002D6F
	public static void startYesNoDlg(string info, Command cmdYes, Command cmdNo)
	{
		GameCanvas.closeKeyBoard();
		GameCanvas.msgdlg.setInfo(info, cmdYes, null, cmdNo);
		GameCanvas.msgdlg.show();
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x00004B92 File Offset: 0x00002D92
	public static void startserverThongBao(string msgSv)
	{
		GameCanvas.thongBaoTest = msgSv;
		GameCanvas.xThongBaoTranslate = GameCanvas.w - 60;
		GameCanvas.dir_ = -1;
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x00051454 File Offset: 0x0004F654
	public static string getMoneys(int m)
	{
		string text = string.Empty;
		int num = m / 1000 + 1;
		for (int i = 0; i < num; i++)
		{
			bool flag = m >= 1000;
			if (!flag)
			{
				text = m.ToString() + text;
				break;
			}
			int num2 = m % 1000;
			text = ((num2 != 0) ? ((num2 >= 10) ? ((num2 >= 100) ? ("." + num2.ToString() + text) : (".0" + num2.ToString() + text)) : (".00" + num2.ToString() + text)) : (".000" + text));
			m /= 1000;
		}
		return text;
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00051520 File Offset: 0x0004F720
	public static int getX(int start, int w)
	{
		return (GameCanvas.px - start) / w;
	}

	// Token: 0x0600043E RID: 1086 RVA: 0x0005153C File Offset: 0x0004F73C
	public static int getY(int start, int w)
	{
		return (GameCanvas.py - start) / w;
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x00003E4C File Offset: 0x0000204C
	protected void sizeChanged(int w, int h)
	{
	}

	// Token: 0x06000440 RID: 1088 RVA: 0x00051558 File Offset: 0x0004F758
	public static bool isGetResourceFromServer()
	{
		return true;
	}

	// Token: 0x06000441 RID: 1089 RVA: 0x0005156C File Offset: 0x0004F76C
	public static Image loadImageRMS(string path)
	{
		path = Main.res + "/x" + mGraphics.zoomLevel.ToString() + path;
		path = GameCanvas.cutPng(path);
		Image image = null;
		try
		{
			image = Image.createImage(path);
		}
		catch (Exception ex)
		{
			try
			{
				string[] array = Res.split(path, "/", 0);
				string text = "x" + mGraphics.zoomLevel.ToString() + array[array.Length - 1];
				sbyte[] array2 = Rms.loadRMS(text);
				bool flag = array2 != null;
				if (flag)
				{
					image = Image.createImage(array2, 0, array2.Length);
				}
			}
			catch (Exception)
			{
				Cout.LogError("Loi ham khong tim thay a: " + ex.ToString());
			}
		}
		return image;
	}

	// Token: 0x06000442 RID: 1090 RVA: 0x00051640 File Offset: 0x0004F840
	public static Image loadImage(string path)
	{
		path = Main.res + "/x" + mGraphics.zoomLevel.ToString() + path;
		path = GameCanvas.cutPng(path);
		Image image = null;
		try
		{
			image = Image.createImage(path);
		}
		catch (Exception)
		{
		}
		return image;
	}

	// Token: 0x06000443 RID: 1091 RVA: 0x0005169C File Offset: 0x0004F89C
	public static string cutPng(string str)
	{
		string text = str;
		bool flag = str.Contains(".png");
		if (flag)
		{
			text = str.Replace(".png", string.Empty);
		}
		return text;
	}

	// Token: 0x06000444 RID: 1092 RVA: 0x000516D4 File Offset: 0x0004F8D4
	public static int random(int a, int b)
	{
		return a + GameCanvas.r.nextInt(b - a);
	}

	// Token: 0x06000445 RID: 1093 RVA: 0x000516F8 File Offset: 0x0004F8F8
	public bool startDust(int dir, int x, int y)
	{
		bool flag = GameCanvas.lowGraphic;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			int num = ((dir != 1) ? 1 : 0);
			bool flag3 = this.dustState[num] != -1;
			if (flag3)
			{
				flag2 = false;
			}
			else
			{
				this.dustState[num] = 0;
				this.dustX[num] = x;
				this.dustY[num] = y;
				flag2 = true;
			}
		}
		return flag2;
	}

	// Token: 0x06000446 RID: 1094 RVA: 0x00051754 File Offset: 0x0004F954
	public void loadWaterSplash()
	{
		bool flag = !GameCanvas.lowGraphic;
		if (flag)
		{
			GameCanvas.imgWS = new Image[3];
			for (int i = 0; i < 3; i++)
			{
				GameCanvas.imgWS[i] = GameCanvas.loadImage("/e/w" + i.ToString() + ".png");
			}
			GameCanvas.wsX = new int[2];
			GameCanvas.wsY = new int[2];
			GameCanvas.wsState = new int[2];
			GameCanvas.wsF = new int[2];
			GameCanvas.wsState[0] = (GameCanvas.wsState[1] = -1);
		}
	}

	// Token: 0x06000447 RID: 1095 RVA: 0x000517F0 File Offset: 0x0004F9F0
	public bool startWaterSplash(int x, int y)
	{
		bool flag = GameCanvas.lowGraphic;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			int num = ((GameCanvas.wsState[0] != -1) ? 1 : 0);
			bool flag3 = GameCanvas.wsState[num] != -1;
			if (flag3)
			{
				flag2 = false;
			}
			else
			{
				GameCanvas.wsState[num] = 0;
				GameCanvas.wsX[num] = x;
				GameCanvas.wsY[num] = y;
				flag2 = true;
			}
		}
		return flag2;
	}

	// Token: 0x06000448 RID: 1096 RVA: 0x00051850 File Offset: 0x0004FA50
	public void updateWaterSplash()
	{
		bool flag = GameCanvas.lowGraphic;
		if (!flag)
		{
			for (int i = 0; i < 2; i++)
			{
				bool flag2 = GameCanvas.wsState[i] == -1;
				if (!flag2)
				{
					GameCanvas.wsY[i]--;
					bool flag3 = GameCanvas.gameTick % 2 == 0;
					if (flag3)
					{
						GameCanvas.wsState[i]++;
						bool flag4 = GameCanvas.wsState[i] > 2;
						if (flag4)
						{
							GameCanvas.wsState[i] = -1;
						}
						else
						{
							GameCanvas.wsF[i] = GameCanvas.wsState[i];
						}
					}
				}
			}
		}
	}

	// Token: 0x06000449 RID: 1097 RVA: 0x000518F0 File Offset: 0x0004FAF0
	public void updateDust()
	{
		bool flag = GameCanvas.lowGraphic;
		if (!flag)
		{
			for (int i = 0; i < 2; i++)
			{
				bool flag2 = this.dustState[i] != -1;
				if (flag2)
				{
					this.dustState[i]++;
					bool flag3 = this.dustState[i] >= 5;
					if (flag3)
					{
						this.dustState[i] = -1;
					}
					bool flag4 = i == 0;
					if (flag4)
					{
						this.dustX[i]--;
					}
					else
					{
						this.dustX[i]++;
					}
					this.dustY[i]--;
				}
			}
		}
	}

	// Token: 0x0600044A RID: 1098 RVA: 0x000519AC File Offset: 0x0004FBAC
	public static bool isPaint(int x, int y)
	{
		bool flag = x < GameScr.cmx;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			bool flag3 = x > GameScr.cmx + GameScr.gW;
			if (flag3)
			{
				flag2 = false;
			}
			else
			{
				bool flag4 = y < GameScr.cmy;
				if (flag4)
				{
					flag2 = false;
				}
				else
				{
					bool flag5 = y > GameScr.cmy + GameScr.gH + 30;
					flag2 = !flag5;
				}
			}
		}
		return flag2;
	}

	// Token: 0x0600044B RID: 1099 RVA: 0x00051A14 File Offset: 0x0004FC14
	public void paintDust(mGraphics g)
	{
		bool flag = GameCanvas.lowGraphic;
		if (!flag)
		{
			for (int i = 0; i < 2; i++)
			{
				bool flag2 = this.dustState[i] != -1 && GameCanvas.isPaint(this.dustX[i], this.dustY[i]);
				if (flag2)
				{
					g.drawImage(GameCanvas.imgDust[i][this.dustState[i]], this.dustX[i], this.dustY[i], 3);
				}
			}
		}
	}

	// Token: 0x0600044C RID: 1100 RVA: 0x00051A94 File Offset: 0x0004FC94
	public void loadDust()
	{
		bool flag = GameCanvas.lowGraphic;
		if (!flag)
		{
			bool flag2 = GameCanvas.imgDust == null;
			if (flag2)
			{
				GameCanvas.imgDust = new Image[2][];
				for (int i = 0; i < GameCanvas.imgDust.Length; i++)
				{
					GameCanvas.imgDust[i] = new Image[5];
				}
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 5; k++)
					{
						GameCanvas.imgDust[j][k] = GameCanvas.loadImage("/e/d" + j.ToString() + k.ToString() + ".png");
					}
				}
			}
			this.dustX = new int[2];
			this.dustY = new int[2];
			this.dustState = new int[2];
			this.dustState[0] = (this.dustState[1] = -1);
		}
	}

	// Token: 0x0600044D RID: 1101 RVA: 0x00051B8C File Offset: 0x0004FD8C
	public static void paintShukiren(int x, int y, mGraphics g)
	{
		g.drawRegion(GameCanvas.imgShuriken, 0, Main.f * 16, 16, 16, 0, x, y, mGraphics.HCENTER | mGraphics.VCENTER);
	}

	// Token: 0x0600044E RID: 1102 RVA: 0x00004BAE File Offset: 0x00002DAE
	public void resetToLoginScrz()
	{
		this.resetToLoginScr = true;
	}

	// Token: 0x0600044F RID: 1103 RVA: 0x00050B94 File Offset: 0x0004ED94
	public static bool isPointer(int x, int y, int w, int h)
	{
		bool flag = !GameCanvas.isPointerDown && !GameCanvas.isPointerJustRelease;
		bool flag2;
		if (flag)
		{
			flag2 = false;
		}
		else
		{
			bool flag3 = GameCanvas.px >= x && GameCanvas.px <= x + w && GameCanvas.py >= y && GameCanvas.py <= y + h;
			flag2 = flag3;
		}
		return flag2;
	}

	// Token: 0x06000450 RID: 1104 RVA: 0x00051BC4 File Offset: 0x0004FDC4
	public void perform(int idAction, object p)
	{
		if (idAction <= 88839)
		{
			if (idAction <= 8889)
			{
				if (idAction == 999)
				{
					mSystem.closeBanner();
					GameCanvas.endDlg();
					return;
				}
				switch (idAction)
				{
				case 8881:
				{
					string text = (string)p;
					try
					{
						GameMidlet.instance.platformRequest(text);
					}
					catch (Exception)
					{
					}
					GameCanvas.currentDialog = null;
					return;
				}
				case 8882:
					InfoDlg.hide();
					GameCanvas.currentDialog = null;
					ServerListScreen.isAutoConect = false;
					ServerListScreen.countDieConnect = 0;
					return;
				case 8883:
					return;
				case 8884:
				{
					GameCanvas.endDlg();
					bool flag = GameCanvas.serverScr == null;
					if (flag)
					{
						GameCanvas.serverScr = new ServerScr();
					}
					GameCanvas.serverScr.switchToMe();
					return;
				}
				case 8885:
					GameMidlet.instance.exit();
					return;
				case 8886:
				{
					GameCanvas.endDlg();
					string text2 = (string)p;
					Service.gI().addFriend(text2);
					return;
				}
				case 8887:
				{
					GameCanvas.endDlg();
					int num = (int)p;
					Service.gI().addPartyAccept(num);
					return;
				}
				case 8888:
				{
					int num2 = (int)p;
					Service.gI().addPartyCancel(num2);
					GameCanvas.endDlg();
					return;
				}
				case 8889:
				{
					string text3 = (string)p;
					GameCanvas.endDlg();
					Service.gI().acceptPleaseParty(text3);
					return;
				}
				default:
					return;
				}
			}
			else
			{
				if (idAction == 9000)
				{
					GameCanvas.endDlg();
					SplashScr.imgLogo = null;
					SmallImage.loadBigRMS();
					mSystem.gcc();
					ServerListScreen.bigOk = true;
					ServerListScreen.loadScreen = true;
					GameScr.gI().loadGameScr();
					bool flag2 = GameCanvas.currentScreen != GameCanvas.loginScr;
					if (flag2)
					{
						GameCanvas.serverScreen.switchToMe2();
					}
					return;
				}
				if (idAction == 9999)
				{
					GameCanvas.endDlg();
					GameCanvas.connect();
					Service.gI().setClientType();
					bool flag3 = GameCanvas.loginScr == null;
					if (flag3)
					{
						GameCanvas.loginScr = new LoginScr();
					}
					GameCanvas.loginScr.doLogin();
					return;
				}
				switch (idAction)
				{
				case 88810:
				{
					int num3 = (int)p;
					GameCanvas.endDlg();
					Service.gI().acceptInviteTrade(num3);
					return;
				}
				case 88811:
					GameCanvas.endDlg();
					Service.gI().cancelInviteTrade();
					return;
				case 88812:
				case 88813:
				case 88815:
				case 88816:
				case 88830:
				case 88831:
				case 88832:
				case 88833:
				case 88834:
				case 88835:
				case 88838:
					return;
				case 88814:
				{
					Item[] array = (Item[])p;
					GameCanvas.endDlg();
					Service.gI().crystalCollectLock(array);
					return;
				}
				case 88817:
					ChatPopup.addChatPopup(string.Empty, 1, global::Char.myCharz().npcFocus);
					Service.gI().menu(global::Char.myCharz().npcFocus.template.npcTemplateId, GameCanvas.menu.menuSelectedItem, 0);
					return;
				case 88818:
				{
					short num4 = (short)p;
					Service.gI().textBoxId(num4, GameCanvas.inputDlg.tfInput.getText());
					GameCanvas.endDlg();
					return;
				}
				case 88819:
				{
					short num5 = (short)p;
					Service.gI().menuId(num5);
					return;
				}
				case 88820:
				{
					string[] array2 = (string[])p;
					bool flag4 = global::Char.myCharz().npcFocus == null;
					if (flag4)
					{
						return;
					}
					int menuSelectedItem = GameCanvas.menu.menuSelectedItem;
					bool flag5 = array2.Length > 1;
					if (flag5)
					{
						MyVector myVector = new MyVector();
						for (int i = 0; i < array2.Length - 1; i++)
						{
							myVector.addElement(new Command(array2[i + 1], GameCanvas.instance, 88821, menuSelectedItem));
						}
						GameCanvas.menu.startAt(myVector, 3);
					}
					else
					{
						ChatPopup.addChatPopup(string.Empty, 1, global::Char.myCharz().npcFocus);
						Service.gI().menu(global::Char.myCharz().npcFocus.template.npcTemplateId, menuSelectedItem, 0);
					}
					return;
				}
				case 88821:
				{
					int num6 = (int)p;
					ChatPopup.addChatPopup(string.Empty, 1, global::Char.myCharz().npcFocus);
					Service.gI().menu(global::Char.myCharz().npcFocus.template.npcTemplateId, num6, GameCanvas.menu.menuSelectedItem);
					return;
				}
				case 88822:
					ChatPopup.addChatPopup(string.Empty, 1, global::Char.myCharz().npcFocus);
					Service.gI().menu(global::Char.myCharz().npcFocus.template.npcTemplateId, GameCanvas.menu.menuSelectedItem, 0);
					return;
				case 88823:
					GameCanvas.startOKDlg(mResources.SENTMSG);
					return;
				case 88824:
					GameCanvas.startOKDlg(mResources.NOSENDMSG);
					return;
				case 88825:
					GameCanvas.startOKDlg(mResources.sendMsgSuccess, false);
					return;
				case 88826:
					GameCanvas.startOKDlg(mResources.cannotSendMsg, false);
					return;
				case 88827:
					GameCanvas.startOKDlg(mResources.sendGuessMsgSuccess);
					return;
				case 88828:
					GameCanvas.startOKDlg(mResources.sendMsgFail);
					return;
				case 88829:
				{
					string text4 = GameCanvas.inputDlg.tfInput.getText();
					bool flag6 = !text4.Equals(string.Empty);
					if (flag6)
					{
						Service.gI().changeName(text4, (int)p);
						InfoDlg.showWait();
					}
					return;
				}
				case 88836:
					GameCanvas.inputDlg.tfInput.setMaxTextLenght(6);
					GameCanvas.inputDlg.show(mResources.INPUT_PRIVATE_PASS, new Command(mResources.ACCEPT, GameCanvas.instance, 888361, null), TField.INPUT_TYPE_NUMERIC);
					return;
				case 88837:
					break;
				case 88839:
					goto IL_0775;
				default:
					return;
				}
			}
		}
		else if (idAction <= 100016)
		{
			switch (idAction)
			{
			case 100001:
				Service.gI().getFlag(0, -1);
				InfoDlg.showWait();
				return;
			case 100002:
			{
				bool flag7 = GameCanvas.loginScr == null;
				if (flag7)
				{
					GameCanvas.loginScr = new LoginScr();
				}
				GameCanvas.loginScr.backToRegister();
				return;
			}
			case 100003:
			case 100004:
				return;
			case 100005:
			{
				bool flag8 = global::Char.myCharz().statusMe == 14;
				if (flag8)
				{
					GameCanvas.startOKDlg(mResources.can_not_do_when_die);
				}
				else
				{
					Service.gI().openUIZone();
				}
				return;
			}
			case 100006:
				mSystem.onDisconnected();
				return;
			default:
				if (idAction != 100016)
				{
					return;
				}
				ServerListScreen.SetIpSelect(17, false);
				GameCanvas.instance.doResetToLoginScr(GameCanvas.serverScreen);
				ServerListScreen.waitToLogin = true;
				GameCanvas.endDlg();
				return;
			}
		}
		else
		{
			switch (idAction)
			{
			case 101023:
				Main.numberQuit = 0;
				return;
			case 101024:
				Res.outz("output 101024");
				GameCanvas.endDlg();
				return;
			case 101025:
			{
				GameCanvas.endDlg();
				bool loadScreen = ServerListScreen.loadScreen;
				if (loadScreen)
				{
					GameCanvas.serverScreen.switchToMe();
				}
				else
				{
					GameCanvas.serverScreen.show2();
				}
				return;
			}
			case 101026:
				mSystem.onDisconnected();
				return;
			default:
				if (idAction != 888361)
				{
					switch (idAction)
					{
					case 888391:
						goto IL_07EE;
					case 888392:
						Service.gI().menu(4, GameCanvas.menu.menuSelectedItem, 0);
						return;
					case 888393:
					{
						bool flag9 = GameCanvas.loginScr == null;
						if (flag9)
						{
							GameCanvas.loginScr = new LoginScr();
						}
						GameCanvas.loginScr.doLogin();
						Main.closeKeyBoard();
						return;
					}
					case 888394:
						GameCanvas.endDlg();
						return;
					case 888395:
						GameCanvas.endDlg();
						return;
					case 888396:
						GameCanvas.endDlg();
						return;
					case 888397:
					{
						string text5 = (string)p;
						return;
					}
					default:
						return;
					}
				}
				else
				{
					string text6 = GameCanvas.inputDlg.tfInput.getText();
					GameCanvas.endDlg();
					bool flag10 = text6.Length < 6 || text6.Equals(string.Empty);
					if (flag10)
					{
						GameCanvas.startOKDlg(mResources.ALERT_PRIVATE_PASS_1);
						return;
					}
					try
					{
						Service.gI().activeAccProtect(int.Parse(text6));
						return;
					}
					catch (Exception ex)
					{
						GameCanvas.startOKDlg(mResources.ALERT_PRIVATE_PASS_2);
						Cout.println("Loi tai 888361 Gamescavas " + ex.ToString());
						return;
					}
				}
				break;
			}
		}
		string text7 = GameCanvas.inputDlg.tfInput.getText();
		GameCanvas.endDlg();
		try
		{
			Service.gI().openLockAccProtect(int.Parse(text7.Trim()));
			return;
		}
		catch (Exception ex2)
		{
			Cout.println("Loi tai 88837 " + ex2.ToString());
			return;
		}
		IL_0775:
		string text8 = GameCanvas.inputDlg.tfInput.getText();
		GameCanvas.endDlg();
		bool flag11 = text8.Length < 6 || text8.Equals(string.Empty);
		if (flag11)
		{
			GameCanvas.startOKDlg(mResources.ALERT_PRIVATE_PASS_1);
			return;
		}
		try
		{
			GameCanvas.startYesNoDlg(mResources.cancelAccountProtection, 888391, text8, 8882, null);
			return;
		}
		catch (Exception)
		{
			GameCanvas.startOKDlg(mResources.ALERT_PRIVATE_PASS_2);
			return;
		}
		IL_07EE:
		string text9 = (string)p;
		GameCanvas.endDlg();
		Service.gI().clearAccProtect(int.Parse(text9));
	}

	// Token: 0x06000451 RID: 1105 RVA: 0x00004BB8 File Offset: 0x00002DB8
	public static void clearAllPointerEvent()
	{
		GameCanvas.isPointerClick = false;
		GameCanvas.isPointerDown = false;
		GameCanvas.isPointerJustDown = false;
		GameCanvas.isPointerJustRelease = false;
		GameCanvas.isPointerSelect = false;
		GameScr.gI().lastSingleClick = 0L;
		GameScr.gI().isPointerDowning = false;
	}

	// Token: 0x06000452 RID: 1106 RVA: 0x0005257C File Offset: 0x0005077C
	public static bool isWait()
	{
		return global::Char.isLoadingMap || LoginScr.isContinueToLogin || ServerListScreen.waitToLogin || ServerListScreen.isWait || SelectCharScr.isWait;
	}

	// Token: 0x0400069D RID: 1693
	public static long timeNow = 0L;

	// Token: 0x0400069E RID: 1694
	public static bool open3Hour;

	// Token: 0x0400069F RID: 1695
	public static bool lowGraphic = true;

	// Token: 0x040006A0 RID: 1696
	public static bool serverchat = false;

	// Token: 0x040006A1 RID: 1697
	public static bool isMoveNumberPad = true;

	// Token: 0x040006A2 RID: 1698
	public static bool isLoading;

	// Token: 0x040006A3 RID: 1699
	public static bool isTouch = false;

	// Token: 0x040006A4 RID: 1700
	public static bool isTouchControl;

	// Token: 0x040006A5 RID: 1701
	public static bool isTouchControlSmallScreen;

	// Token: 0x040006A6 RID: 1702
	public static bool isTouchControlLargeScreen;

	// Token: 0x040006A7 RID: 1703
	public static bool isConnectFail;

	// Token: 0x040006A8 RID: 1704
	public static GameCanvas instance;

	// Token: 0x040006A9 RID: 1705
	public static bool bRun;

	// Token: 0x040006AA RID: 1706
	public static bool[] keyPressed = new bool[30];

	// Token: 0x040006AB RID: 1707
	public static bool[] keyReleased = new bool[30];

	// Token: 0x040006AC RID: 1708
	public static bool[] keyHold = new bool[30];

	// Token: 0x040006AD RID: 1709
	public static bool isPointerDown;

	// Token: 0x040006AE RID: 1710
	public static bool isPointerClick;

	// Token: 0x040006AF RID: 1711
	public static bool isPointerJustRelease;

	// Token: 0x040006B0 RID: 1712
	public static bool isPointerSelect;

	// Token: 0x040006B1 RID: 1713
	public static bool isPointerMove;

	// Token: 0x040006B2 RID: 1714
	public static int px;

	// Token: 0x040006B3 RID: 1715
	public static int py;

	// Token: 0x040006B4 RID: 1716
	public static int pxFirst;

	// Token: 0x040006B5 RID: 1717
	public static int pyFirst;

	// Token: 0x040006B6 RID: 1718
	public static int pxLast;

	// Token: 0x040006B7 RID: 1719
	public static int pyLast;

	// Token: 0x040006B8 RID: 1720
	public static int pxMouse;

	// Token: 0x040006B9 RID: 1721
	public static int pyMouse;

	// Token: 0x040006BA RID: 1722
	public static Position[] arrPos = new Position[4];

	// Token: 0x040006BB RID: 1723
	public static int gameTick;

	// Token: 0x040006BC RID: 1724
	public static int taskTick;

	// Token: 0x040006BD RID: 1725
	public static bool isEff1;

	// Token: 0x040006BE RID: 1726
	public static bool isEff2;

	// Token: 0x040006BF RID: 1727
	public static long timeTickEff1;

	// Token: 0x040006C0 RID: 1728
	public static long timeTickEff2;

	// Token: 0x040006C1 RID: 1729
	public static int w;

	// Token: 0x040006C2 RID: 1730
	public static int h;

	// Token: 0x040006C3 RID: 1731
	public static int hw;

	// Token: 0x040006C4 RID: 1732
	public static int hh;

	// Token: 0x040006C5 RID: 1733
	public static int wd3;

	// Token: 0x040006C6 RID: 1734
	public static int hd3;

	// Token: 0x040006C7 RID: 1735
	public static int w2d3;

	// Token: 0x040006C8 RID: 1736
	public static int h2d3;

	// Token: 0x040006C9 RID: 1737
	public static int w3d4;

	// Token: 0x040006CA RID: 1738
	public static int h3d4;

	// Token: 0x040006CB RID: 1739
	public static int wd6;

	// Token: 0x040006CC RID: 1740
	public static int hd6;

	// Token: 0x040006CD RID: 1741
	public static mScreen currentScreen;

	// Token: 0x040006CE RID: 1742
	public static Menu menu = new Menu();

	// Token: 0x040006CF RID: 1743
	public static Panel panel;

	// Token: 0x040006D0 RID: 1744
	public static Panel panel2;

	// Token: 0x040006D1 RID: 1745
	public static ChooseCharScr chooseCharScr;

	// Token: 0x040006D2 RID: 1746
	public static LoginScr loginScr;

	// Token: 0x040006D3 RID: 1747
	public static RegisterScreen registerScr;

	// Token: 0x040006D4 RID: 1748
	public static Dialog currentDialog;

	// Token: 0x040006D5 RID: 1749
	public static MsgDlg msgdlg;

	// Token: 0x040006D6 RID: 1750
	public static InputDlg inputDlg;

	// Token: 0x040006D7 RID: 1751
	public static MyVector currentPopup = new MyVector();

	// Token: 0x040006D8 RID: 1752
	public static int requestLoseCount;

	// Token: 0x040006D9 RID: 1753
	public static MyVector listPoint;

	// Token: 0x040006DA RID: 1754
	public static Paint paintz;

	// Token: 0x040006DB RID: 1755
	public static bool isGetResFromServer;

	// Token: 0x040006DC RID: 1756
	public static Image[] imgBG;

	// Token: 0x040006DD RID: 1757
	public static int skyColor;

	// Token: 0x040006DE RID: 1758
	public static int curPos = 0;

	// Token: 0x040006DF RID: 1759
	public static int[] bgW;

	// Token: 0x040006E0 RID: 1760
	public static int[] bgH;

	// Token: 0x040006E1 RID: 1761
	public static int planet = 0;

	// Token: 0x040006E2 RID: 1762
	private mGraphics g = new mGraphics();

	// Token: 0x040006E3 RID: 1763
	public static Image img12;

	// Token: 0x040006E4 RID: 1764
	public static Image[] imgBlue = new Image[7];

	// Token: 0x040006E5 RID: 1765
	public static Image[] imgViolet = new Image[7];

	// Token: 0x040006E6 RID: 1766
	public static MyHashTable danhHieu = new MyHashTable();

	// Token: 0x040006E7 RID: 1767
	public static MyVector messageServer = new MyVector(string.Empty);

	// Token: 0x040006E8 RID: 1768
	public static bool isPlaySound = false;

	// Token: 0x040006E9 RID: 1769
	private static int clearOldData;

	// Token: 0x040006EA RID: 1770
	public static int timeOpenKeyBoard;

	// Token: 0x040006EB RID: 1771
	public static bool isFocusPanel2;

	// Token: 0x040006EC RID: 1772
	public static int fps = 0;

	// Token: 0x040006ED RID: 1773
	public static int max;

	// Token: 0x040006EE RID: 1774
	public static int up;

	// Token: 0x040006EF RID: 1775
	public static int upmax;

	// Token: 0x040006F0 RID: 1776
	private long timefps = mSystem.currentTimeMillis() + 1000L;

	// Token: 0x040006F1 RID: 1777
	private long timeup = mSystem.currentTimeMillis() + 1000L;

	// Token: 0x040006F2 RID: 1778
	public static int isRequestMapID = -1;

	// Token: 0x040006F3 RID: 1779
	public static long waitingTimeChangeMap;

	// Token: 0x040006F4 RID: 1780
	private static int dir_ = -1;

	// Token: 0x040006F5 RID: 1781
	private int tickWaitThongBao;

	// Token: 0x040006F6 RID: 1782
	public bool isPaintCarret;

	// Token: 0x040006F7 RID: 1783
	public static MyVector debugUpdate;

	// Token: 0x040006F8 RID: 1784
	public static MyVector debugPaint;

	// Token: 0x040006F9 RID: 1785
	public static MyVector debugSession;

	// Token: 0x040006FA RID: 1786
	private static bool isShowErrorForm = false;

	// Token: 0x040006FB RID: 1787
	public static bool paintBG;

	// Token: 0x040006FC RID: 1788
	public static int gsskyHeight;

	// Token: 0x040006FD RID: 1789
	public static int gsgreenField1Y;

	// Token: 0x040006FE RID: 1790
	public static int gsgreenField2Y;

	// Token: 0x040006FF RID: 1791
	public static int gshouseY;

	// Token: 0x04000700 RID: 1792
	public static int gsmountainY;

	// Token: 0x04000701 RID: 1793
	public static int bgLayer0y;

	// Token: 0x04000702 RID: 1794
	public static int bgLayer1y;

	// Token: 0x04000703 RID: 1795
	public static Image imgCloud;

	// Token: 0x04000704 RID: 1796
	public static Image imgSun;

	// Token: 0x04000705 RID: 1797
	public static Image imgSun2;

	// Token: 0x04000706 RID: 1798
	public static Image imgClear;

	// Token: 0x04000707 RID: 1799
	public static Image[] imgBorder = new Image[3];

	// Token: 0x04000708 RID: 1800
	public static Image[] imgSunSpec = new Image[3];

	// Token: 0x04000709 RID: 1801
	public static int borderConnerW;

	// Token: 0x0400070A RID: 1802
	public static int borderConnerH;

	// Token: 0x0400070B RID: 1803
	public static int borderCenterW;

	// Token: 0x0400070C RID: 1804
	public static int borderCenterH;

	// Token: 0x0400070D RID: 1805
	public static int[] cloudX;

	// Token: 0x0400070E RID: 1806
	public static int[] cloudY;

	// Token: 0x0400070F RID: 1807
	public static int sunX;

	// Token: 0x04000710 RID: 1808
	public static int sunY;

	// Token: 0x04000711 RID: 1809
	public static int sunX2;

	// Token: 0x04000712 RID: 1810
	public static int sunY2;

	// Token: 0x04000713 RID: 1811
	public static int[] layerSpeed;

	// Token: 0x04000714 RID: 1812
	public static int[] moveX;

	// Token: 0x04000715 RID: 1813
	public static int[] moveXSpeed;

	// Token: 0x04000716 RID: 1814
	public static bool isBoltEff;

	// Token: 0x04000717 RID: 1815
	public static bool boltActive;

	// Token: 0x04000718 RID: 1816
	public static int tBolt;

	// Token: 0x04000719 RID: 1817
	public static Image imgBgIOS;

	// Token: 0x0400071A RID: 1818
	public static int typeBg = -1;

	// Token: 0x0400071B RID: 1819
	public static int transY;

	// Token: 0x0400071C RID: 1820
	public static int[] yb = new int[5];

	// Token: 0x0400071D RID: 1821
	public static int[] colorTop;

	// Token: 0x0400071E RID: 1822
	public static int[] colorBotton;

	// Token: 0x0400071F RID: 1823
	public static int yb1;

	// Token: 0x04000720 RID: 1824
	public static int yb2;

	// Token: 0x04000721 RID: 1825
	public static int yb3;

	// Token: 0x04000722 RID: 1826
	public static int nBg = 0;

	// Token: 0x04000723 RID: 1827
	public static int lastBg = -1;

	// Token: 0x04000724 RID: 1828
	public static int[] bgRain = new int[] { 1, 4, 11 };

	// Token: 0x04000725 RID: 1829
	public static int[] bgRainFont = new int[] { -1 };

	// Token: 0x04000726 RID: 1830
	public static Image imgCaycot;

	// Token: 0x04000727 RID: 1831
	public static Image tam;

	// Token: 0x04000728 RID: 1832
	public static int typeBackGround = -1;

	// Token: 0x04000729 RID: 1833
	public static int saveIDBg = -10;

	// Token: 0x0400072A RID: 1834
	public static bool isLoadBGok;

	// Token: 0x0400072B RID: 1835
	private static long lastTimePress = 0L;

	// Token: 0x0400072C RID: 1836
	public static int keyAsciiPress;

	// Token: 0x0400072D RID: 1837
	public static int pXYScrollMouse;

	// Token: 0x0400072E RID: 1838
	private static Image imgSignal;

	// Token: 0x0400072F RID: 1839
	public static MyVector flyTexts = new MyVector();

	// Token: 0x04000730 RID: 1840
	public int longTime;

	// Token: 0x04000731 RID: 1841
	public static long timeBreakLoading;

	// Token: 0x04000732 RID: 1842
	private static string thongBaoTest;

	// Token: 0x04000733 RID: 1843
	public static int xThongBaoTranslate = GameCanvas.w - 60;

	// Token: 0x04000734 RID: 1844
	public static bool isPointerJustDown = false;

	// Token: 0x04000735 RID: 1845
	private int count = 1;

	// Token: 0x04000736 RID: 1846
	public static bool csWait;

	// Token: 0x04000737 RID: 1847
	public static MyRandom r = new MyRandom();

	// Token: 0x04000738 RID: 1848
	public static bool isBlackScreen;

	// Token: 0x04000739 RID: 1849
	public static int[] bgSpeed;

	// Token: 0x0400073A RID: 1850
	public static int cmdBarX;

	// Token: 0x0400073B RID: 1851
	public static int cmdBarY;

	// Token: 0x0400073C RID: 1852
	public static int cmdBarW;

	// Token: 0x0400073D RID: 1853
	public static int cmdBarH;

	// Token: 0x0400073E RID: 1854
	public static int cmdBarLeftW;

	// Token: 0x0400073F RID: 1855
	public static int cmdBarRightW;

	// Token: 0x04000740 RID: 1856
	public static int cmdBarCenterW;

	// Token: 0x04000741 RID: 1857
	public static int hpBarX;

	// Token: 0x04000742 RID: 1858
	public static int hpBarY;

	// Token: 0x04000743 RID: 1859
	public static int hpBarW;

	// Token: 0x04000744 RID: 1860
	public static int expBarW;

	// Token: 0x04000745 RID: 1861
	public static int lvPosX;

	// Token: 0x04000746 RID: 1862
	public static int moneyPosX;

	// Token: 0x04000747 RID: 1863
	public static int hpBarH;

	// Token: 0x04000748 RID: 1864
	public static int girlHPBarY;

	// Token: 0x04000749 RID: 1865
	public int timeOut;

	// Token: 0x0400074A RID: 1866
	public int[] dustX;

	// Token: 0x0400074B RID: 1867
	public int[] dustY;

	// Token: 0x0400074C RID: 1868
	public int[] dustState;

	// Token: 0x0400074D RID: 1869
	public static int[] wsX;

	// Token: 0x0400074E RID: 1870
	public static int[] wsY;

	// Token: 0x0400074F RID: 1871
	public static int[] wsState;

	// Token: 0x04000750 RID: 1872
	public static int[] wsF;

	// Token: 0x04000751 RID: 1873
	public static Image[] imgWS;

	// Token: 0x04000752 RID: 1874
	public static Image imgShuriken;

	// Token: 0x04000753 RID: 1875
	public static Image[][] imgDust;

	// Token: 0x04000754 RID: 1876
	public static bool isResume;

	// Token: 0x04000755 RID: 1877
	public static ServerListScreen serverScreen;

	// Token: 0x04000756 RID: 1878
	public static ServerScr serverScr;

	// Token: 0x04000757 RID: 1879
	public static SelectCharScr _SelectCharScr;

	// Token: 0x04000758 RID: 1880
	public bool resetToLoginScr;

	// Token: 0x04000759 RID: 1881
	public static long TIMEOUT;

	// Token: 0x0400075A RID: 1882
	public static int timeLoading = 15;
}
