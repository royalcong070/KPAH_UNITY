using System.Collections.Generic;

/// <summary>
/// Mô hình dữ liệu nhân vật offline.
/// Lưu đầy đủ state theo đúng schema server (bảng `players`) nhưng chạy local.
/// </summary>
public class OfflinePlayer
{
    public int idDatabase;          // id
    public short idPlayer;          // idPlayer
    public string name;
    public byte clazz;              // classPlayer (0 Kiếm,1 Chiến,2 Pháp,3 Đấu,4 Cung)
    public byte head;
    public byte gender;
    public byte idNation;
    public byte he;
    public byte level = 1;

    // point: [hp, mp, attack, defend, defendMagic, accurate, dodge, critical, ...]
    public int hp;
    public int hpMax;
    public int mp;
    public int mpMax;
    public int attack;
    public int defend;
    public int defendMagic;
    public short accurate;
    public short dodge;
    public short critical;
    public short strength;
    public short agility;
    public short spirit;
    public short health;
    public short luck;
    public short basePoint;
    public short skillPoint;
    public int dedicationPoint;
    public short baokich;
    public int exp;
    public short speed;

    public byte[] skillLevels = new byte[0];

    // inventory: xu, luong, luong khoa...
    public long xu;
    public long luong;
    public long luongKhoa;
    public byte limItemBag = 100;

    // Các list item dạng string JSON (để dễ serialize, giữ consistent với server)
    public string itemBody;     // trang bị đang mặc (JSON array)
    public string itemBag;      // hành trang
    public string itemBox;      // rương
    public string itemPotion;   // bình thuốc
    public string itemQuest;    // vật phẩm nhiệm vụ
    public string itemGem;      // ngọc
    public string itemGemLock;  // ngọc khóa
    public string itemSold;     // đã bán
    public string itemAnimal;   // thú cưỡi
    public string itemAnimalExpiry;
    public string horse;

    // location: [zone, x, y, nation]
    public short zone;
    public short x;
    public short y;

    // meta
    public long lastTimeLogout;
    public long lastTimeEndDelete;

    public OfflinePlayer()
    {
    }

    public string GetName()
    {
        return name ?? string.Empty;
    }
}
