using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu/load dữ liệu nhân vật offline vào PlayerPrefs (Unity local storage).
/// Dùng myWriter/myReader để serialize binary, rồi lưu hex string vào PlayerPrefs.
/// </summary>
public class OfflineSave
{
    private const string PREFIX = "kpah_off_";

    private static string KeyAccounts()
    {
        return PREFIX + "accounts";
    }

    private static string KeyChars(string user)
    {
        return PREFIX + "chars_" + user;
    }

    private static string KeyPlayer(int id)
    {
        return PREFIX + "p_" + id;
    }

    private static string KeyNextId()
    {
        return PREFIX + "next_id";
    }

    // ---- Tài khoản ----

    public static bool AccountExists(string username)
    {
        return PlayerPrefs.HasKey(KeyAccounts()) && PlayerPrefs.GetString(KeyAccounts()).Contains(username);
    }

    public static void SavePassword(string username, string password)
    {
        PlayerPrefs.SetString(PREFIX + "pwd_" + username, password);
    }

    public static bool CheckPassword(string username, string password)
    {
        string saved = PlayerPrefs.GetString(PREFIX + "pwd_" + username, "");
        return saved == password;
    }

    public static void AddAccount(string username)
    {
        string all = PlayerPrefs.GetString(KeyAccounts(), "");
        if (all.Length > 0)
            all += ",";
        all += username;
        PlayerPrefs.SetString(KeyAccounts(), all);
    }

    // ---- Danh sách nhân vật ----

    public static List<int> LoadCharIds(string username)
    {
        string raw = PlayerPrefs.GetString(KeyChars(username), "");
        if (string.IsNullOrEmpty(raw))
            return new List<int>();
        string[] parts = raw.Split(',');
        List<int> ids = new List<int>();
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int v))
                ids.Add(v);
        }
        return ids;
    }

    public static void SaveCharIds(string username, List<int> ids)
    {
        string s = "";
        for (int i = 0; i < ids.Count; i++)
        {
            if (i > 0) s += ",";
            s += ids[i].ToString();
        }
        PlayerPrefs.SetString(KeyChars(username), s);
    }

    // ---- ID tự tăng ----

    public static int NextPlayerId()
    {
        int id = PlayerPrefs.GetInt(KeyNextId(), 1000);
        PlayerPrefs.SetInt(KeyNextId(), id + 1);
        return id;
    }

    // ---- Lưu nhân vật ----

    public static void SavePlayer(OfflinePlayer p)
    {
        myWriter w = new myWriter();
        w.writeInt(p.idDatabase);
        w.writeShort(p.idPlayer);
        w.writeUTF(p.name ?? "");
        w.writeByte((sbyte)p.clazz);
        w.writeByte((sbyte)p.head);
        w.writeByte((sbyte)p.gender);
        w.writeByte((sbyte)p.idNation);
        w.writeByte((sbyte)p.he);
        w.writeByte((sbyte)p.level);

        w.writeInt(p.hp);
        w.writeInt(p.hpMax);
        w.writeInt(p.mp);
        w.writeInt(p.mpMax);
        w.writeInt(p.attack);
        w.writeInt(p.defend);
        w.writeInt(p.defendMagic);
        w.writeShort(p.accurate);
        w.writeShort(p.dodge);
        w.writeShort(p.critical);
        w.writeShort(p.strength);
        w.writeShort(p.agility);
        w.writeShort(p.spirit);
        w.writeShort(p.health);
        w.writeShort(p.luck);
        w.writeShort(p.basePoint);
        w.writeShort(p.skillPoint);
        w.writeInt(p.dedicationPoint);
        w.writeShort(p.baokich);
        w.writeInt(p.exp);
        w.writeShort(p.speed);

        byte[] sl = p.skillLevels != null ? p.skillLevels : new byte[0];
        w.writeByte((sbyte)sl.Length);
        for (int i = 0; i < sl.Length; i++)
            w.writeByte((sbyte)sl[i]);

        w.writeLong(p.xu);
        w.writeLong(p.luong);
        w.writeLong(p.luongKhoa);
        w.writeByte((sbyte)p.limItemBag);

        w.writeUTF(p.itemBody ?? "[]");
        w.writeUTF(p.itemBag ?? "[]");
        w.writeUTF(p.itemBox ?? "[]");
        w.writeUTF(p.itemPotion ?? "[]");
        w.writeUTF(p.itemQuest ?? "[]");
        w.writeUTF(p.itemGem ?? "[]");
        w.writeUTF(p.itemGemLock ?? "[]");
        w.writeUTF(p.itemSold ?? "[]");
        w.writeUTF(p.itemAnimal ?? "[]");
        w.writeUTF(p.itemAnimalExpiry ?? "[]");
        w.writeUTF(p.horse ?? "[]");

        w.writeShort(p.zone);
        w.writeShort(p.x);
        w.writeShort(p.y);

        w.writeLong(p.lastTimeLogout);
        w.writeLong(p.lastTimeEndDelete);

        sbyte[] raw = w.getData();
        string hex = ByteArrayToHex(convertSbyteToByte(raw));
        PlayerPrefs.SetString(KeyPlayer(p.idDatabase), hex);
    }

    // ---- Load nhân vật ----

    public static OfflinePlayer LoadPlayer(int id)
    {
        string hex = PlayerPrefs.GetString(KeyPlayer(id), "");
        if (string.IsNullOrEmpty(hex))
            return null;
        try
        {
            byte[] bytes = HexToByteArray(hex);
            sbyte[] raw = ByteToSbyte(bytes);
            myReader r = new myReader(raw);
            OfflinePlayer p = new OfflinePlayer();
            p.idDatabase = r.readInt();
            p.idPlayer = r.readShort();
            p.name = r.readUTF();
            p.clazz = (byte)r.readByte();
            p.head = (byte)r.readByte();
            p.gender = (byte)r.readByte();
            p.idNation = (byte)r.readByte();
            p.he = (byte)r.readByte();
            p.level = (byte)r.readByte();

            p.hp = r.readInt();
            p.hpMax = r.readInt();
            p.mp = r.readInt();
            p.mpMax = r.readInt();
            p.attack = r.readInt();
            p.defend = r.readInt();
            p.defendMagic = r.readInt();
            p.accurate = r.readShort();
            p.dodge = r.readShort();
            p.critical = r.readShort();
            p.strength = r.readShort();
            p.agility = r.readShort();
            p.spirit = r.readShort();
            p.health = r.readShort();
            p.luck = r.readShort();
            p.basePoint = r.readShort();
            p.skillPoint = r.readShort();
            p.dedicationPoint = r.readInt();
            p.baokich = r.readShort();
            p.exp = r.readInt();
            p.speed = r.readShort();

            int slen = r.readByte();
            p.skillLevels = new byte[slen];
            for (int i = 0; i < slen; i++)
                p.skillLevels[i] = (byte)r.readByte();

            p.xu = r.readLong();
            p.luong = r.readLong();
            p.luongKhoa = r.readLong();
            p.limItemBag = (byte)r.readByte();

            p.itemBody = r.readUTF();
            p.itemBag = r.readUTF();
            p.itemBox = r.readUTF();
            p.itemPotion = r.readUTF();
            p.itemQuest = r.readUTF();
            p.itemGem = r.readUTF();
            p.itemGemLock = r.readUTF();
            p.itemSold = r.readUTF();
            p.itemAnimal = r.readUTF();
            p.itemAnimalExpiry = r.readUTF();
            p.horse = r.readUTF();

            p.zone = r.readShort();
            p.x = r.readShort();
            p.y = r.readShort();

            p.lastTimeLogout = r.readLong();
            p.lastTimeEndDelete = r.readLong();

            return p;
        }
        catch (Exception ex)
        {
            Debug.LogError("OfflineSave.LoadPlayer ERROR: " + ex.Message);
            return null;
        }
    }

    public static void DeletePlayer(int id)
    {
        PlayerPrefs.DeleteKey(KeyPlayer(id));
    }

    // ---- Hex helpers ----

    private static byte[] convertSbyteToByte(sbyte[] s)
    {
        byte[] b = new byte[s.Length];
        for (int i = 0; i < s.Length; i++)
            b[i] = (s[i] > 0) ? (byte)s[i] : (byte)(s[i] + 256);
        return b;
    }

    private static sbyte[] ByteToSbyte(byte[] b)
    {
        sbyte[] s = new sbyte[b.Length];
        for (int i = 0; i < b.Length; i++)
            s[i] = (sbyte)b[i];
        return s;
    }

    private static string ByteArrayToHex(byte[] b)
    {
        StringBuilder sb = new StringBuilder(b.Length * 2);
        for (int i = 0; i < b.Length; i++)
            sb.Append(b[i].ToString("X2"));
        return sb.ToString();
    }

    private static byte[] HexToByteArray(string hex)
    {
        if (hex.Length % 2 != 0)
            hex = "0" + hex;
        byte[] b = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
            b[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return b;
    }
}
