using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cung cấp dữ liệu game offline: map, monster, NPC, item template, skill.
/// Tạo các Message server→client đúng protocol để MessageHandler xử lý.
/// 
/// Dữ liệu đọc từ:
/// 1. OfflineData/*.json (extracted từ SQL dump)
/// 2. Hardcoded defaults cho items, skills, shops (chưa có trong SQL dump)
/// </summary>
public class OfflineGameDataProvider
{
    public static OfflineGameDataProvider instance = new OfflineGameDataProvider();

    // Data loaded từ JSON
    public List<MobData> mobs = new List<MobData>();
    public List<MapData> maps = new List<MapData>();

    // Default data (hardcoded)
    public Dictionary<short, ItemTemplate> itemTemplates = new Dictionary<short, ItemTemplate>();
    public List<SkillData> skills = new List<SkillData>();
    public List<NpcData> npcs = new List<NpcData>();
    public List<PotionData> potions = new List<PotionData>();

    public struct MobData
    {
        public short id;
        public string name;
        public sbyte type;
        public int hp;
        public short level;
    }

    public struct MapData
    {
        public short id;
        public string name;
        public int w;
        public int h;
        public short[] tiles;
        public List<MobSpawn> mobSpawns;
        public List<NpcSpawn> npcSpawns;
    }

    public struct MobSpawn
    {
        public short mobId;
        public short x;
        public short y;
    }

    public struct NpcSpawn
    {
        public short id;
        public short x;
        public short y;
    }

    public struct ItemTemplate
    {
        public short id;
        public string name;
        public sbyte type;
        public sbyte style;
        public sbyte he;
        public sbyte gender;
        public sbyte level;
        public short durable;
        public int price;
        public short idIcon;
        public sbyte colorItem;
        public short[] attributes;
    }

    public struct SkillData
    {
        public short id;
        public string name;
        public string description;
        public sbyte classChar;
        public int price;
    }

    public struct NpcData
    {
        public short id;
        public string name;
        public sbyte type;
    }

    public struct PotionData
    {
        public short id;
        public string name;
        public string name2;
        public sbyte idImage;
        public short delay;
        public bool isTrade;
        public short price;
        public short recovered;
    }

    // ========================= LOAD DATA =========================

    public static readonly string MOBS_JSON = "[{\"id\":1,\"name\":\"Nhím\",\"type\":-1,\"hp\":200,\"level\":-1},{\"id\":2,\"name\":\"Sâu\",\"type\":-1,\"hp\":300,\"level\":-1},{\"id\":3,\"name\":\"Giọt nước\",\"type\":-1,\"hp\":450,\"level\":-1},{\"id\":4,\"name\":\"Gà điên\",\"type\":-1,\"hp\":650,\"level\":-1},{\"id\":5,\"name\":\"Rắn lục\",\"type\":-1,\"hp\":900,\"level\":-1},{\"id\":6,\"name\":\"Ma trơi\",\"type\":-1,\"hp\":1200,\"level\":-1},{\"id\":7,\"name\":\"Nắp ấm\",\"type\":-1,\"hp\":1550,\"level\":-1},{\"id\":8,\"name\":\"Rệp quỷ\",\"type\":-1,\"hp\":1950,\"level\":-1},{\"id\":9,\"name\":\"Chuột cống\",\"type\":-1,\"hp\":2400,\"level\":-1},{\"id\":10,\"name\":\"Quỷ hoa\",\"type\":-1,\"hp\":3000,\"level\":-1},{\"id\":11,\"name\":\"Sâu róm\",\"type\":-1,\"hp\":3660,\"level\":-1},{\"id\":12,\"name\":\"Lửa ma\",\"type\":-1,\"hp\":4380,\"level\":-1},{\"id\":13,\"name\":\"Giọt nước lớn\",\"type\":-1,\"hp\":5160,\"level\":-1},{\"id\":14,\"name\":\"Gà con\",\"type\":-1,\"hp\":9960,\"level\":-1},{\"id\":15,\"name\":\"Heo mọi\",\"type\":-1,\"hp\":6000,\"level\":-1},{\"id\":16,\"name\":\"Bọ cạp\",\"type\":-1,\"hp\":6900,\"level\":-1},{\"id\":17,\"name\":\"Rùa đỏ\",\"type\":-1,\"hp\":5160,\"level\":-1},{\"id\":18,\"name\":\"Nhện tím\",\"type\":-1,\"hp\":18600,\"level\":-1},{\"id\":19,\"name\":\"Nấm ma\",\"type\":-1,\"hp\":7860,\"level\":-1},{\"id\":20,\"name\":\"Quỷ sinh hoa\",\"type\":-1,\"hp\":8880,\"level\":-1},{\"id\":21,\"name\":\"Rết\",\"type\":-1,\"hp\":6900,\"level\":-1},{\"id\":22,\"name\":\"Gà khổng lồ\",\"type\":-1,\"hp\":30150,\"level\":-1},{\"id\":23,\"name\":\"Rắn khổng lồ\",\"type\":-1,\"hp\":32630,\"level\":-1},{\"id\":24,\"name\":\"Bướm khổng lồ\",\"type\":-1,\"hp\":9960,\"level\":-1},{\"id\":25,\"name\":\"Bóng ma\",\"type\":-1,\"hp\":15310,\"level\":-1},{\"id\":26,\"name\":\"Thủy nhãn\",\"type\":-1,\"hp\":13770,\"level\":-1},{\"id\":27,\"name\":\"Skeleton\",\"type\":-1,\"hp\":18600,\"level\":-1},{\"id\":28,\"name\":\"Cọp khổng lồ\",\"type\":-1,\"hp\":12300,\"level\":-1},{\"id\":29,\"name\":\"Cá sấu\",\"type\":-1,\"hp\":22170,\"level\":-1},{\"id\":30,\"name\":\"ếch quỷ\",\"type\":-1,\"hp\":24060,\"level\":-1},{\"id\":31,\"name\":\"Dế khổng lồ\",\"type\":-1,\"hp\":37830,\"level\":-1},{\"id\":32,\"name\":\"Nấm tinh\",\"type\":-1,\"hp\":35190,\"level\":-1},{\"id\":33,\"name\":\"Trâu núi\",\"type\":-1,\"hp\":40550,\"level\":-1},{\"id\":34,\"name\":\"Sơn tặc\",\"type\":-1,\"hp\":43350,\"level\":-1},{\"id\":35,\"name\":\"Hải tặc\",\"type\":-1,\"hp\":46230,\"level\":-1},{\"id\":36,\"name\":\"Long Trụ\",\"type\":-1,\"hp\":50000000,\"level\":-1},{\"id\":37,\"name\":\"Long trụ phụ\",\"type\":-1,\"hp\":25000000,\"level\":-1},{\"id\":38,\"name\":\"Thuồng luồng\",\"type\":-1,\"hp\":500000,\"level\":-1},{\"id\":39,\"name\":\"Thằn lằn\",\"type\":-1,\"hp\":200000000,\"level\":-1},{\"id\":40,\"name\":\"Rết đỏ\",\"type\":-1,\"hp\":53678,\"level\":-1},{\"id\":41,\"name\":\"Rết tía\",\"type\":-1,\"hp\":106654,\"level\":-1},{\"id\":42,\"name\":\"Rết xanh\",\"type\":-1,\"hp\":75098,\"level\":-1},{\"id\":43,\"name\":\"Liên hoa trụ\",\"type\":-1,\"hp\":20000000,\"level\":-1},{\"id\":44,\"name\":\"Tử kê\",\"type\":-1,\"hp\":118730,\"level\":-1},{\"id\":45,\"name\":\"Hồng kê\",\"type\":-1,\"hp\":57634,\"level\":-1},{\"id\":46,\"name\":\"Tướng thủ thành\",\"type\":-1,\"hp\":80000000,\"level\":-1},{\"id\":47,\"name\":\"Rắn mang bành\",\"type\":-1,\"hp\":100918,\"level\":-1},{\"id\":48,\"name\":\"Sư tử\",\"type\":-1,\"hp\":193830,\"level\":-1},{\"id\":49,\"name\":\"ốc ma\",\"type\":-1,\"hp\":460530,\"level\":-1},{\"id\":50,\"name\":\"Bướm nâu\",\"type\":-1,\"hp\":79894,\"level\":-1},{\"id\":51,\"name\":\"Ma cây\",\"type\":-1,\"hp\":560530,\"level\":-1},{\"id\":52,\"name\":\"Huyết ma\",\"type\":-1,\"hp\":90030,\"level\":-1},{\"id\":53,\"name\":\"Cá Thòi lòi\",\"type\":-1,\"hp\":330530,\"level\":-1},{\"id\":54,\"name\":\"Cua càng to\",\"type\":-1,\"hp\":360530,\"level\":-1},{\"id\":55,\"name\":\"độc nhãn\",\"type\":-1,\"hp\":49878,\"level\":-1},{\"id\":56,\"name\":\"Cá ma\",\"type\":-1,\"hp\":680530,\"level\":-1},{\"id\":57,\"name\":\"Huyết nhãn\",\"type\":-1,\"hp\":84870,\"level\":-1},{\"id\":58,\"name\":\"Quỷ hoa\",\"type\":-1,\"hp\":800530,\"level\":-1},{\"id\":59,\"name\":\"Skeleton lam\",\"type\":-1,\"hp\":95378,\"level\":-1},{\"id\":60,\"name\":\"Tê giác\",\"type\":-1,\"hp\":240878,\"level\":-1},{\"id\":61,\"name\":\"Cọp tím\",\"type\":-1,\"hp\":160078,\"level\":-1},{\"id\":62,\"name\":\"Cọp xanh\",\"type\":-1,\"hp\":112590,\"level\":-1},{\"id\":63,\"name\":\"Cọp đỏ\",\"type\":-1,\"hp\":61750,\"level\":-1},{\"id\":64,\"name\":\"Cá sấu đỏ\",\"type\":-1,\"hp\":183830,\"level\":-1},{\"id\":65,\"name\":\"Cua đinh\",\"type\":-1,\"hp\":172230,\"level\":-1},{\"id\":66,\"name\":\"Cá sấu xanh\",\"type\":-1,\"hp\":200878,\"level\":-1},{\"id\":67,\"name\":\"Cóc lam\",\"type\":-1,\"hp\":209778,\"level\":-1},{\"id\":68,\"name\":\"Cóc lửa\",\"type\":-1,\"hp\":70478,\"level\":-1},{\"id\":69,\"name\":\"Cóc xanh\",\"type\":-1,\"hp\":228350,\"level\":-1},{\"id\":70,\"name\":\"Dế lửa\",\"type\":-1,\"hp\":238030,\"level\":-1},{\"id\":71,\"name\":\"Dế cam\",\"type\":-1,\"hp\":66030,\"level\":-1},{\"id\":72,\"name\":\"Dế lam\",\"type\":-1,\"hp\":258198,\"level\":-1},{\"id\":73,\"name\":\"trâu đỏ\",\"type\":-1,\"hp\":268694,\"level\":-1},{\"id\":74,\"name\":\"Người đá\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":75,\"name\":\"trâu xanh\",\"type\":-1,\"hp\":290530,\"level\":-1},{\"id\":76,\"name\":\"ốc sên\",\"type\":-1,\"hp\":152630,\"level\":-1},{\"id\":77,\"name\":\"Bọ hung\",\"type\":-1,\"hp\":268350,\"level\":-1},{\"id\":78,\"name\":\"Sơn tặc 3\",\"type\":-1,\"hp\":325454,\"level\":-1},{\"id\":79,\"name\":\"Hải tặc 1\",\"type\":-1,\"hp\":337690,\"level\":-1},{\"id\":80,\"name\":\"Nấm độc\",\"type\":-1,\"hp\":125078,\"level\":-1},{\"id\":81,\"name\":\"Cánh cam\",\"type\":-1,\"hp\":290530,\"level\":-1},{\"id\":82,\"name\":\"đại bàng\",\"type\":-1,\"hp\":200000000,\"level\":-1},{\"id\":83,\"name\":\"cổng thành\",\"type\":-1,\"hp\":100000000,\"level\":-1},{\"id\":84,\"name\":\"Cương thi\",\"type\":-1,\"hp\":5000,\"level\":-1},{\"id\":85,\"name\":\"Đá\",\"type\":-1,\"hp\":50,\"level\":-1},{\"id\":86,\"name\":\"Bông\",\"type\":-1,\"hp\":50,\"level\":-1},{\"id\":87,\"name\":\"Gỗ\",\"type\":-1,\"hp\":50,\"level\":-1},{\"id\":88,\"name\":\"Da\",\"type\":-1,\"hp\":50,\"level\":-1},{\"id\":89,\"name\":\"Sắt\",\"type\":-1,\"hp\":50,\"level\":-1},{\"id\":90,\"name\":\"Người tuyết\",\"type\":-1,\"hp\":150000000,\"level\":-1},{\"id\":91,\"name\":\"Rắn chị\",\"type\":-1,\"hp\":150000000,\"level\":-1},{\"id\":92,\"name\":\"Rắn em\",\"type\":-1,\"hp\":150000000,\"level\":-1},{\"id\":93,\"name\":\"Mắt quỷ\",\"type\":-1,\"hp\":100000000,\"level\":-1},{\"id\":94,\"name\":\"Bạch cốt tướng quân\",\"type\":-1,\"hp\":150000000,\"level\":-1},{\"id\":95,\"name\":\"Ngựa ca nhan trang\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":96,\"name\":\"Ngựa ca nhan xanh\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":97,\"name\":\"Ngựa ca nhan đỏ\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":98,\"name\":\"Ngựa ca nhan xanh la\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":99,\"name\":\"Ngựa ca nhan vàng\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":100,\"name\":\"Ngựa ca nhan tím\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":101,\"name\":\"Ngựa bang trắng\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":102,\"name\":\"Ngựa bang xanh\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":103,\"name\":\"Ngựa bang đỏ\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":104,\"name\":\"Ngựa bang xanh lá\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":105,\"name\":\"Ngựa bang vàng\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":106,\"name\":\"Ngựa bang tím\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":107,\"name\":\"Ngựa quốc gia trắng\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":108,\"name\":\"Ngựa quốc gia xanh\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":109,\"name\":\"Ngựa quốc gia đỏ\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":110,\"name\":\"Ngựa quốc gia xanh lá\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":111,\"name\":\"Ngựa quốc gia vàng\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":112,\"name\":\"Ngựa quốc gia tím\",\"type\":-1,\"hp\":158414,\"level\":-1},{\"id\":113,\"name\":\"Boss thỏ điên\",\"type\":-1,\"hp\":200000000,\"level\":-1},{\"id\":114,\"name\":\"Khô lâu\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":115,\"name\":\"Boss Gấu xám\",\"type\":-1,\"hp\":400000000,\"level\":-1},{\"id\":116,\"name\":\"Dracula\",\"type\":-1,\"hp\":500000000,\"level\":-1},{\"id\":117,\"name\":\"Bí ngô\",\"type\":-1,\"hp\":500000000,\"level\":-1},{\"id\":118,\"name\":\"Nhện ma\",\"type\":-1,\"hp\":900530,\"level\":-1},{\"id\":119,\"name\":\"cong thanh\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":120,\"name\":\"Trụ rồng\",\"type\":-1,\"hp\":5000000,\"level\":-1},{\"id\":121,\"name\":\"Rương ma quái\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":122,\"name\":\"Ngọc 1 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":123,\"name\":\"Ngọc 2 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":124,\"name\":\"Ngọc 3 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":125,\"name\":\"Ngọc 4 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":126,\"name\":\"Ngọc 5 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":127,\"name\":\"Ngọc 6 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":128,\"name\":\"Ngọc 7 sao\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":129,\"name\":\"Boss thuỷ tinh\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":130,\"name\":\"Boss sơn tinh\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":131,\"name\":\"Thùng gỗ\",\"type\":-1,\"hp\":1,\"level\":-1},{\"id\":132,\"name\":\"Cọp tím\",\"type\":-1,\"hp\":950000,\"level\":-1},{\"id\":133,\"name\":\"Cọp xanh\",\"type\":-1,\"hp\":1000000,\"level\":-1},{\"id\":134,\"name\":\"Cọp đỏ\",\"type\":-1,\"hp\":1050000,\"level\":-1}]";

    public void LoadAllData()
    {
        LoadMobs();
        LoadMaps();
        LoadDefaultItems();
        LoadDefaultSkills();
        LoadDefaultNpcs();
        LoadDefaultPotions();
        PopulateMonsterTemplates();
        PopulateItemTemplates();
        PopulateSkillData();
        Debug.Log($"[OfflineData] Loaded: {mobs.Count} mobs, {maps.Count} maps, {itemTemplates.Count} items, {skills.Count} skills, {npcs.Count} npcs, {potions.Count} potions");
    }

    private void LoadMobs()
    {
        try
        {
            mobs = ParseMobsJson(MOBS_JSON);
        }
        catch (Exception ex)
        {
            Debug.LogError("[OfflineData] LoadMobs error: " + ex.Message);
            LoadDefaultMobs();
        }
    }

    private void LoadMaps()
    {
        try
        {
            LoadDefaultMaps();
        }
        catch (Exception ex)
        {
            Debug.LogError("[OfflineData] LoadMaps error: " + ex.Message);
            LoadDefaultMaps();
        }
    }

    // ========================= DEFAULT DATA =========================

    private void LoadDefaultMobs()
    {
        // Minimal mob data
        mobs.Clear();
        mobs.Add(new MobData { id = 1, name = "Nhím", type = -1, hp = 200, level = 1 });
        mobs.Add(new MobData { id = 2, name = "Sâu", type = -1, hp = 300, level = 2 });
        mobs.Add(new MobData { id = 3, name = "Giọt nước", type = -1, hp = 450, level = 3 });
        mobs.Add(new MobData { id = 4, name = "Gà điên", type = -1, hp = 650, level = 4 });
        mobs.Add(new MobData { id = 5, name = "Rắn lục", type = -1, hp = 900, level = 5 });
        mobs.Add(new MobData { id = 6, name = "Ma trơi", type = -1, hp = 1200, level = 6 });
    }

    private void LoadDefaultMaps()
    {
        maps.Clear();
        // Tạo 1 map mặc định 40x30 tiles
        int w = 40, h = 30;
        short[] tiles = new short[w * h];
        System.Random rng = new System.Random(42);
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = (short)(rng.Next(0, 5));
        }
        MapData map = new MapData
        {
            id = 0,
            name = "Ngoài thành",
            w = w,
            h = h,
            tiles = tiles,
            mobSpawns = new List<MobSpawn>
            {
                new MobSpawn { mobId = 1, x = 100, y = 100 },
                new MobSpawn { mobId = 2, x = 200, y = 150 },
                new MobSpawn { mobId = 3, x = 300, y = 200 },
                new MobSpawn { mobId = 4, x = 150, y = 250 },
                new MobSpawn { mobId = 5, x = 250, y = 100 },
            },
            npcSpawns = new List<NpcSpawn>
            {
                new NpcSpawn { id = 1, x = 105, y = 184 },
            }
        };
        maps.Add(map);
    }

    private void LoadDefaultItems()
    {
        itemTemplates.Clear();
        // Weapons (type=3+class)
        AddItem(79, "Kiếm sắt", 3, 0, 0, 0, 1, 100, 100, 79, 0);
        AddItem(86, "Đại đao sắt", 3, 0, 0, 1, 1, 100, 100, 86, 0);
        AddItem(93, "Đũa thần", 3, 0, 0, 2, 1, 100, 100, 93, 0);
        AddItem(100, "Rìu sắt", 3, 0, 0, 3, 1, 100, 100, 100, 0);
        AddItem(107, "Cung sắt", 3, 0, 0, 4, 1, 100, 100, 107, 0);
        // Armor (type=0)
        AddItem(1, "Áo nữ", 0, 1, 0, 0, 1, 50, 50, 1, 0);
        AddItem(2, "Áo nam", 0, 2, 0, 0, 1, 50, 50, 2, 0);
        // Pants (type=1)
        AddItem(27, "Quần nữ", 1, 1, 0, 0, 1, 50, 50, 27, 0);
        AddItem(28, "Quần nam", 1, 2, 0, 0, 1, 50, 50, 28, 0);
    }

    private void AddItem(short id, string name, sbyte type, sbyte style, sbyte he, sbyte gender, sbyte level, short durable, int price, short idIcon, sbyte colorItem)
    {
        itemTemplates[id] = new ItemTemplate
        {
            id = id, name = name, type = type, style = style,
            he = he, gender = gender, level = level,
            durable = durable, price = price, idIcon = idIcon, colorItem = colorItem,
            attributes = new short[0]
        };
    }

    private void LoadDefaultSkills()
    {
        skills.Clear();
        string[][] skillNames = new string[][]
        {
            new string[] { "Chém", "Đánh lust", "Kíchitivity", "Phòng thủ", "Bão kiếm", "Kiếm pháp", "Lôi kiếm", "Huyết kiếm", "Thiên kiếm" },
            new string[] { "Chém", "Đánh", "Bổ", "Phòng thủ", "Trọng kích", "Liên hoàn", "Lốc xoáy", "Càn khôn", "Hỏa principle" },
            new string[] { "Tia lửa", "Quả cầu lửa", "Băng giá", "Phòng thủ", "Sấm sét", "Bão tố", "Thủy tinh", "Lửa địa ngục", "Hỏa tinh", "Băng tinh", "Sấm tinh" },
            new string[] { "Đánh", "Đá", "Quật", "Phòng thủ", "Kích", "Xung phong", "Bão đá", "Địa chấn", "Thổ tinh" },
            new string[] { "Bắn", "Mũi tên", "Xuyên tâm", "Phòng thủ", "Mưa tên", "Băng tiễn", "Lửa tiễn", "Thiên tiễn", "Mộc tinh" }
        };
        for (int c = 0; c < 5; c++)
        {
            for (int i = 0; i < skillNames[c].Length; i++)
            {
                skills.Add(new SkillData
                {
                    id = (short)(c * 20 + i),
                    name = skillNames[c][i],
                    description = skillNames[c][i],
                    classChar = (sbyte)c,
                    price = 1000
                });
            }
        }
    }

    private void LoadDefaultNpcs()
    {
        npcs.Clear();
        npcs.Add(new NpcData { id = 1, name = "Thương nhân", type = 0 });
        npcs.Add(new NpcData { id = 2, name = "Làngtrưởng", type = 1 });
    }

    private void LoadDefaultPotions()
    {
        potions.Clear();
        potions.Add(new PotionData { id = 1, name = "Bình máu nhỏ", name2 = "HP +50", idImage = 0, delay = 200, isTrade = true, price = 50, recovered = 50 });
        potions.Add(new PotionData { id = 4, name = "Bình MP nhỏ", name2 = "MP +30", idImage = 3, delay = 200, isTrade = true, price = 40, recovered = 30 });
    }

    private void PopulateMonsterTemplates()
    {
        try
        {
            if (mobs.Count == 0) return;
            Res.monsterTemplates = new MonsterTemplate[mobs.Count + 10];
            foreach (var mob in mobs)
            {
                MonsterTemplate mt = new MonsterTemplate();
                mt.type = mob.id;
                mt.name = mob.name;
                mt.moveType = 0;
                mt.speed = 2;
                mt.height = 24;
                mt.w = 32;
                mt.h = 32;
                mt.xCenter = 16;
                mt.yCenter = 16;
                mt.he = 0;
                mt.palate = 0;
                mt.spalate = (sbyte)(mob.id);
                mt.maxhp = mob.hp;
                Res.monsterTemplates[mob.id] = mt;
            }
            Debug.Log("[OfflineData] Populated " + mobs.Count + " MonsterTemplates into Res.monsterTemplates");
        }
        catch (Exception ex)
        {
            Debug.LogError("[OfflineData] PopulateMonsterTemplates ERROR: " + ex.Message);
        }
    }

    private void PopulateItemTemplates()
    {
        try
        {
            Res.itemTemplates = new mVector();
            ItemTemplate[] arr = new ItemTemplate[200];
            foreach (var kv in itemTemplates)
            {
                var t = kv.Value;
                ItemTemplate it = new ItemTemplate();
                it.id = t.id;
                it.name = t.name;
                it.type = t.type;
                it.style = t.style;
                it.he = t.he;
                it.gender = t.gender;
                it.level = t.level;
                it.durable = t.durable;
                it.price = t.price;
                it.idIcon = t.idIcon;
                it.colorItem = t.colorItem;
                it.attributes = new short[10];
                arr[t.id] = it;
            }
            Res.itemTemplates.addElement(arr);

            // Potion templates
            Res.potionTemplates = new PotionTemplate[20];
            Res.potionTemplates[0] = new PotionTemplate(100, 1);
            Res.potionTemplates[3] = new PotionTemplate(80, 2);

            Debug.Log("[OfflineData] Populated " + itemTemplates.Count + " ItemTemplates into Res");
        }
        catch (Exception ex)
        {
            Debug.LogError("[OfflineData] PopulateItemTemplates ERROR: " + ex.Message);
        }
    }

    private void PopulateSkillData()
    {
        try
        {
            SkillManager.SKILL_DAM_PERCENT = new short[1][][];
            SkillManager.SKILL_COOLDOWN = new int[1][][];
            SkillManager.SKILL_RANGE = new short[1][];
            SkillManager.SKILL_MP = new short[1][][];
            SkillManager.TIME_LIFE_BUFF_SKILL = new short[15][];
            SkillManager.LEVEL_ADD_SKILL = new sbyte[15][];
            SkillManager.SKILL_AEO = new sbyte[5][];

            for (int s = 0; s < 15; s++)
            {
                SkillManager.SKILL_DAM_PERCENT[0] = new short[15][];
                for (int i = 0; i < 15; i++)
                {
                    SkillManager.SKILL_DAM_PERCENT[0][i] = new short[11];
                    for (int l = 0; l < 11; l++)
                        SkillManager.SKILL_DAM_PERCENT[0][i][l] = (short)(10 + l * 5 + i * 2);
                }
            }

            for (int i = 0; i < 15; i++)
            {
                SkillManager.SKILL_COOLDOWN[0] = new int[15][];
                SkillManager.SKILL_COOLDOWN[0][i] = new int[11];
                for (int l = 0; l < 11; l++)
                    SkillManager.SKILL_COOLDOWN[0][i][l] = (500 + l * 100);
            }

            SkillManager.SKILL_RANGE[0] = new short[15];
            for (int i = 0; i < 15; i++)
                SkillManager.SKILL_RANGE[0][i] = 50;

            for (int i = 0; i < 15; i++)
            {
                SkillManager.SKILL_MP[0] = new short[15][];
                SkillManager.SKILL_MP[0][i] = new short[11];
                for (int l = 0; l < 11; l++)
                    SkillManager.SKILL_MP[0][i][l] = (sbyte)(5 + l * 2);
            }

            for (int i = 0; i < 15; i++)
            {
                SkillManager.TIME_LIFE_BUFF_SKILL[i] = new short[11];
                for (int l = 0; l < 11; l++)
                    SkillManager.TIME_LIFE_BUFF_SKILL[i][l] = (short)(3 + l);
            }

            for (int i = 0; i < 15; i++)
            {
                SkillManager.LEVEL_ADD_SKILL[i] = new sbyte[11];
                for (int l = 0; l < 11; l++)
                    SkillManager.LEVEL_ADD_SKILL[i][l] = (sbyte)(l + 1);
            }

            for (int c = 0; c < 5; c++)
                SkillManager.SKILL_AEO[c] = new sbyte[] { 4, 5, 6, 7, 8 };

            Debug.Log("[OfflineData] Populated SkillManager data");
        }
        catch (Exception ex)
        {
            Debug.LogError("[OfflineData] PopulateSkillData ERROR: " + ex.Message);
        }
    }

    // ========================= JSON PARSING (simple) =========================

    private List<MobData> ParseMobsJson(string json)
    {
        List<MobData> result = new List<MobData>();
        // Simple JSON array parsing - find { "id": ..., "name": ... } patterns
        int idx = 0;
        while (idx < json.Length)
        {
            int objStart = json.IndexOf('{', idx);
            if (objStart == -1) break;
            int objEnd = json.IndexOf('}', objStart);
            if (objEnd == -1) break;
            string obj = json.Substring(objStart, objEnd - objStart + 1);
            idx = objEnd + 1;

            MobData m = new MobData();
            m.id = GetJsonInt(obj, "id") != 0 ? (short)GetJsonInt(obj, "id") : (short)0;
            m.name = GetJsonString(obj, "name");
            m.type = (sbyte)GetJsonInt(obj, "type");
            m.hp = GetJsonInt(obj, "hp");
            m.level = (short)GetJsonInt(obj, "level");
            if (m.id > 0) result.Add(m);
        }
        return result;
    }

    private List<MapData> ParseMapsJson(string json)
    {
        List<MapData> result = new List<MapData>();
        int idx = 0;
        while (idx < json.Length)
        {
            int objStart = json.IndexOf('{', idx);
            if (objStart == -1) break;
            int objEnd = json.IndexOf('}', objStart);
            if (objEnd == -1) break;
            string obj = json.Substring(objStart, objEnd - objStart + 1);
            idx = objEnd + 1;

            MapData m = new MapData();
            m.id = (short)GetJsonInt(obj, "id");
            m.name = GetJsonString(obj, "name");
            m.w = 40;
            m.h = 30;
            m.tiles = new short[m.w * m.h];
            m.mobSpawns = new List<MobSpawn>();
            m.npcSpawns = new List<NpcSpawn>();
            if (m.id >= 0) result.Add(m);
        }
        return result;
    }

    private string GetJsonString(string json, string key)
    {
        string search = "\"" + key + "\"";
        int idx = json.IndexOf(search);
        if (idx == -1) return "";
        int colonIdx = json.IndexOf(':', idx + search.Length);
        if (colonIdx == -1) return "";
        int start = json.IndexOf('"', colonIdx + 1);
        if (start == -1) return "";
        int end = json.IndexOf('"', start + 1);
        if (end == -1) return "";
        return json.Substring(start + 1, end - start - 1);
    }

    private int GetJsonInt(string json, string key)
    {
        string search = "\"" + key + "\"";
        int idx = json.IndexOf(search);
        if (idx == -1) return 0;
        int colonIdx = json.IndexOf(':', idx + search.Length);
        if (colonIdx == -1) return 0;
        int start = colonIdx + 1;
        while (start < json.Length && (json[start] == ' ' || json[start] == '"')) start++;
        int end = start;
        while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && json[end] != '"') end++;
        string numStr = json.Substring(start, end - start).Trim();
        if (int.TryParse(numStr, out int val)) return val;
        return 0;
    }

    // ========================= SEND MESSAGES =========================

    /// <summary>Gửi item templates (CMD 25) – cần cho hiển thị trang bị.</summary>
    public void SendItemTemplates()
    {
        Message m = new Message(Cmd_message.ITEM_TEMPLATE);
        try
        {
            myWriter w = m.writer();
            w.writeShort((short)itemTemplates.Count);
            foreach (var kv in itemTemplates)
            {
                ItemTemplate t = kv.Value;
                w.writeShort(t.id);
                w.writeUTF(t.name ?? "");
                w.writeByte(t.type);
                w.writeByte(t.style);
                w.writeByte(t.he);
                w.writeByte(t.gender);
                w.writeByte(t.level);
                w.writeShort(t.durable);
                w.writeShort(t.idIcon);
                w.writeShort((short)t.price);
                w.writeByte(t.colorItem);
            }
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendItemTemplates ERROR: " + ex.StackTrace);
        }
    }

    /// <summary>Gửi monster templates (CMD 26) – cần cho spawn quái.</summary>
    public void SendMonsterTemplates()
    {
        Message m = new Message(Cmd_message.MONSTER_TEMPLATE);
        try
        {
            myWriter w = m.writer();
            w.writeShort((short)mobs.Count);
            foreach (var mob in mobs)
            {
                w.writeShort(mob.id);
                w.writeUTF(mob.name ?? "");
                w.writeByte(mob.type);
                w.writeShort(mob.level);
                w.writeInt(mob.hp);
            }
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMonsterTemplates ERROR: " + ex.StackTrace);
        }
    }

    /// <summary>Gửi skill info (CMD 35).</summary>
    public void SendSkillInfo()
    {
        Message m = new Message(Cmd_message.SKILL_INFO);
        try
        {
            myWriter w = m.writer();
            w.writeShort((short)skills.Count);
            foreach (var sk in skills)
            {
                w.writeShort(sk.id);
                w.writeUTF(sk.name ?? "");
                w.writeByte(sk.classChar);
                w.writeUTF(sk.description ?? "");
                w.writeInt(sk.price);
            }
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendSkillInfo ERROR: " + ex.StackTrace);
        }
    }
}
