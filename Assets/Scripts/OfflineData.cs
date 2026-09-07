using UnityEngine;

/// <summary>
/// Dữ liệu tĩnh cho offline mode: stat mặc định theo class, item template IDs,
/// potion names, v.v... matching server-side Manager/Const values.
/// </summary>
public class OfflineData
{
    // Const class IDs (match server Const.java)
    public const byte KIEM_KHACH = 0;
    public const byte CHIEN_BINH = 1;
    public const byte PHAP_SU = 2;
    public const byte DAU_SI = 3;
    public const byte CUNG_THU = 4;

    // Potion names
    public static readonly string[] potionNames = new string[]
    {
        "Bình máu nhỏ", "Bình máu trung", "Bình máu lớn",
        "Bình MP nhỏ", "Bình MP trung", "Bình MP lớn",
        "Thuốc giải", "Thuốc hồi sinh", "Thuốc tăng lực", "Thuốc bỏ qua"
    };

    /// <summary>Trả về五行 (he) theo class – match server Util.getHe()</summary>
    public static byte GetHe(byte clazz)
    {
        switch (clazz)
        {
            case KIEM_KHACH: return 0; // Kim
            case CHIEN_BINH: return 1; // Hỏa
            case PHAP_SU: return 2;    // Thủy
            case DAU_SI: return 3;     // Thổ
            case CUNG_THU: return 4;   // Mộc
        }
        return 0;
    }

    /// <summary>Set default stats theo class – match server PlayerDAO.getDefaultPoint()</summary>
    public static void SetDefaultStats(OfflinePlayer p)
    {
        int hpBase = 100, mpBase = 50;
        int atkBase = 10, defBase = 5;
        int str = 0, agi = 0, spi = 0, hp = 0, lck = 0;

        switch (p.clazz)
        {
            case KIEM_KHACH:
                str = 25; agi = 20; spi = 10; hp = 25; lck = 10;
                atkBase = 12; defBase = 8;
                break;
            case CHIEN_BINH:
                str = 30; agi = 20; spi = 10; hp = 20; lck = 10;
                atkBase = 15; defBase = 10;
                break;
            case PHAP_SU:
                str = 10; agi = 25; spi = 35; hp = 10; lck = 10;
                atkBase = 8; defBase = 5;
                mpBase = 120;
                break;
            case DAU_SI:
                str = 20; agi = 30; spi = 10; hp = 20; lck = 10;
                atkBase = 13; defBase = 12;
                break;
            case CUNG_THU:
                str = 20; agi = 30; spi = 15; hp = 15; lck = 10;
                atkBase = 11; defBase = 7;
                break;
        }

        p.hp = hpBase;
        p.hpMax = hpBase;
        p.mp = mpBase;
        p.mpMax = mpBase;
        p.attack = atkBase;
        p.defend = defBase;
        p.defendMagic = defBase;
        p.accurate = 10;
        p.dodge = 10;
        p.critical = 5;
        p.strength = (short)str;
        p.agility = (short)agi;
        p.spirit = (short)spi;
        p.health = (short)hp;
        p.luck = (short)lck;
        p.basePoint = 0;
        p.skillPoint = 0;
        p.dedicationPoint = 0;
        p.baokich = 0;
        p.exp = 0;
        p.speed = 5;

        // Skill levels: 14 skills, first skill = 1
        p.skillLevels = new byte[14];
        p.skillLevels[0] = 1;
    }

    /// <summary>Set default items – match server PlayerDAO.getDefaultItemWeapon/getDefaultItemPotion</summary>
    public static void SetDefaultItems(OfflinePlayer p, byte clazz, byte gender)
    {
        // ItemBody: ao + quan + weapon – format: [[id,type,style,...], ...]
        // Server format: "[[idItem, templateId, style, ...], ...]"
        // Simplified: each item as "[idItem,type,style,he,gender,level,durable,price,attb,clazz,plus,colorItem]"
        string aoId = (gender == 0) ? "-32767" : "-32767";
        string aoStyle = (gender == 0) ? "2" : "1";
        string quanId = "-32766";
        string quanStyle = (gender == 0) ? "2" : "1";

        // Weapon ID and style per class
        string wpnId = "-32768";
        string wpnStyle = "0";
        switch (clazz)
        {
            case KIEM_KHACH: wpnStyle = "79"; break;
            case CHIEN_BINH: wpnStyle = "86"; break;
            case PHAP_SU: wpnStyle = "93"; break;
            case DAU_SI: wpnStyle = "100"; break;
            case CUNG_THU: wpnStyle = "107"; break;
        }

        // Format: [idItem,templateId,style,he,gender,level,durable,price,attack,clazz,plus,colorItem,allAttribute]
        p.itemBody = "["
            + "[" + aoId + ",0," + aoStyle + "," + GetHe(clazz) + "," + gender + ",1,100,0,0,0,0,0,\"\"]"
            + ",[" + quanId + ",1," + quanStyle + "," + GetHe(clazz) + "," + gender + ",1,100,0,0,0,0,0,\"\"]"
            + ",[" + wpnId + ",3," + wpnStyle + "," + GetHe(clazz) + "," + gender + ",1,100,10,0,0,0,0,\"\"]"
            + "]";

        // Potion: HP + MP
        p.itemPotion = "[[1,10],[4,10]]";

        // Empty containers
        p.itemBag = "[0," + p.luong + "," + p.luongKhoa + "," + p.xu + ",5," + (sbyte.MinValue + 2) + "]";
        p.itemBox = "[]";
        p.itemQuest = "[]";
        p.itemGem = "[]";
        p.itemGemLock = "[]";
        p.itemSold = "[]";
        p.itemAnimal = "[]";
        p.itemAnimalExpiry = "[]";
        p.horse = "[]";
    }
}
