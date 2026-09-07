using System;
using System.Collections.Generic;
using UnityEngine;

public class OfflineManager
{
    public static OfflineManager instance = new OfflineManager();
    public bool isActive;
    public string currentUser;
    public OfflinePlayer currentPlayer;

    // Monster tracking: id -> [hp, maxhp, type, x, y, level]
    private Dictionary<short, int[]> monsterHP = new Dictionary<short, int[]>();

    // Drop tracking: id -> [catagory, templateID, x, y]
    private Dictionary<short, sbyte[]> dropItems = new Dictionary<short, sbyte[]>();

    // Random for damage calc
    private System.Random rng = new System.Random();

    private OfflineManager()
    {
        isActive = false;
    }

    public void ActivateOffline(string username)
    {
        currentUser = username;
        isActive = true;
        Session_ME.isOfflineMode = true;
        OfflineGameDataProvider.instance.LoadAllData();
        // Ensure Tilemap.typeOfTile is initialized for offline mode
        if (Tilemap.typeOfTile == null || Tilemap.typeOfTile.Length == 0)
        {
            Tilemap.typeOfTile = new int[256];
            for (int i = 0; i < 256; i++) Tilemap.typeOfTile[i] = 0;
        }
    }

    // ========================= CLIENT MESSAGE PROCESSING =========================

    public void ProcessMessage(Message msg)
    {
        try
        {
            switch (msg.command)
            {
                case Cmd_message.LOGIN:
                    HandleLogin(msg);
                    break;
                case Cmd_message.CHARLIST:
                    HandleCharList(msg);
                    break;
                case Cmd_message.CREATE_CHAR:
                    HandleCreateChar(msg);
                    break;
                case Cmd_message.MOVE_CHAR:
                    HandleMoveChar(msg);
                    break;
                case Cmd_message.PLAYER_ATTACK_MONSTER:
                    HandleAttackMonster(msg);
                    break;
                case 106:
                    HandleAttackMultiMonster(msg);
                    break;
                case Cmd_message.USE_POTION:
                    HandleUsePotion(msg);
                    break;
                case Cmd_message.TALK_WITH_NPC:
                    HandleTalkWithNPC(msg);
                    break;
                case Cmd_message.BUY_ITEM_SHOP:
                    HandleBuyItemShop(msg);
                    break;
                case Cmd_message.GET_ITEM_FROM_GROUND:
                    HandleGetItemFromGround(msg);
                    break;
                case Cmd_message.GET_POTION_FROM_GROUND:
                    HandleGetPotionFromGround(msg);
                    break;
                case Cmd_message.CHAT:
                case Cmd_message.PING:
                    break;
                default:
                    Debug.Log("[Offline] Unhandled cmd=" + msg.command);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("OfflineManager.ProcessMessage ERROR cmd=" + msg.command + ": " + ex.StackTrace);
        }
    }

    // ========================= LOGIN =========================

    private void HandleLogin(Message msg)
    {
        myReader r = new myReader(msg.getData());
        string username = r.readUTF();
        string pass = r.readUTF();
        if (!OfflineSave.AccountExists(username))
        {
            OfflineSave.AddAccount(username);
            OfflineSave.SavePassword(username, pass);
        }
        currentUser = username;
        isActive = true;
        SendLoginResponse();
        SendCharListResponse();
    }

    // ========================= CHARLIST =========================

    private void HandleCharList(Message msg)
    {
        myReader r = new myReader(msg.getData());
        sbyte type = r.readByte();
        int selectId = r.readInt();
        // Save any existing player before switching
        SaveCurrentPlayer();
        if (type == 1)
        {
            OfflinePlayer p = OfflineSave.LoadPlayer(selectId);
            if (p == null)
            {
                Debug.LogError("OfflineManager: player not found id=" + selectId);
                return;
            }
            currentPlayer = p;
            SendMainCharInfo(p);
            SendChangeMap(p);
        }
    }

    // ========================= CREATE CHAR =========================

    private void HandleCreateChar(Message msg)
    {
        myReader r = new myReader(msg.getData());
        string name = r.readUTF();
        byte clazz = (byte)r.readByte();
        byte head = (byte)r.readByte();
        byte gender = (byte)r.readByte();
        byte idNation = (byte)r.readByte();
        int newId = OfflineSave.NextPlayerId();
        OfflinePlayer p = new OfflinePlayer();
        p.idDatabase = newId;
        p.idPlayer = (short)(newId - 10000);
        p.name = name;
        p.clazz = clazz;
        p.head = head;
        p.gender = gender;
        p.idNation = idNation;
        p.he = OfflineData.GetHe(clazz);
        p.level = 1;
        p.speed = 5;
        p.limItemBag = 100;
        OfflineData.SetDefaultStats(p);
        OfflineData.SetDefaultItems(p, clazz, gender);
        p.zone = 280;
        p.x = 105;
        p.y = 184;
        OfflineSave.SavePlayer(p);
        List<int> ids = OfflineSave.LoadCharIds(currentUser);
        ids.Add(newId);
        OfflineSave.SaveCharIds(currentUser, ids);
        SendCharListResponse();
    }

    // ========================= MOVEMENT =========================

    private void HandleMoveChar(Message msg)
    {
        if (currentPlayer == null) return;
        myReader r = new myReader(msg.getData());
        short x = r.readShort();
        short y = r.readShort();
        currentPlayer.x = x;
        currentPlayer.y = y;
    }

    // ========================= COMBAT =========================

    private void HandleAttackMonster(Message msg)
    {
        myReader r = new myReader(msg.getData());
        short monsterID = r.readShort();
        sbyte skillType = r.readByte();
        if (!monsterHP.ContainsKey(monsterID)) return;

        int[] data = monsterHP[monsterID];
        int hp = data[0];
        int maxhp = data[1];

        // Calculate damage
        int baseDamage = currentPlayer.attack + rng.Next(5, 20);
        int critChance = rng.Next(0, 100);
        sbyte effect = 0;
        if (critChance < currentPlayer.critical)
        {
            baseDamage = (int)(baseDamage * 1.5);
            effect = AttackResult.EFF_CRITICAL;
        }
        int monsterDefend = 2 + data[5] * 2;  // data[5]=level
        int damage = System.Math.Max(1, baseDamage - monsterDefend);
        int hpLeft = System.Math.Max(0, hp - damage);

        data[0] = hpLeft;
        monsterHP[monsterID] = data;

        // Send attack response (CMD 9)
        SendAttackMonsterResponse(currentPlayer.idPlayer, monsterID, skillType, damage, hpLeft, effect);

        // Monster dies
        if (hpLeft <= 0)
        {
            SendMonsterDieResponse(currentPlayer.idPlayer, monsterID, skillType, damage, effect, data);
            // Award EXP
            AwardEXP(data[5]);
        }
    }

    private void SendAttackMonsterResponse(short attacker, short monsterID, sbyte skillType, int damage, int hpLeft, sbyte effect)
    {
        Message m = new Message(Cmd_message.PLAYER_ATTACK_MONSTER);
        try
        {
            myWriter w = m.writer();
            w.writeShort(attacker);
            w.writeShort(monsterID);
            w.writeByte(skillType);
            w.writeInt(damage);
            w.writeInt(hpLeft);
            w.writeByte(effect);
            w.writeByte(1);
            w.writeByte(-1);
            w.writeByte((sbyte)currentPlayer.level);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendAttackMonsterResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendMonsterDieResponse(short attacker, short monsterID, sbyte skillType, int damage, sbyte effect, int[] data)
    {
        Message m = new Message(Cmd_message.MONSTER_DIE);
        try
        {
            myWriter w = m.writer();
            w.writeShort(attacker);
            w.writeShort(monsterID);
            w.writeByte(skillType);
            w.writeInt(damage);
            w.writeByte(effect);

            // Generate drops
            List<ItemDropInfo> drops = GenerateDrops(data[2], data[5]);
            w.writeByte((sbyte)drops.Count);
            foreach (var drop in drops)
            {
                w.writeByte(drop.itemCatagory);
                w.writeShort(drop.itemTemplateID);
                w.writeShort(drop.itemID);
                w.writeShort(drop.x);
                w.writeShort(drop.y);
            }
            w.writeByte(1);
            w.writeByte(-1);
            w.writeByte((sbyte)currentPlayer.level);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMonsterDieResponse ERROR: " + ex.StackTrace);
        }
    }

    // ========================= EXP + LEVEL =========================

    private int currentEXP = 0;
    private int expToNextLevel = 100;

    private void AwardEXP(int monsterLevel)
    {
        int expGain = 10 + monsterLevel * 5 + rng.Next(5, 15);
        currentEXP += expGain;
        SendSetXPResponse(currentPlayer.idPlayer, expGain);

        if (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel;
            currentPlayer.level++;
            expToNextLevel = 100 * currentPlayer.level;
            currentPlayer.hpMax += 20;
            currentPlayer.mpMax += 10;
            currentPlayer.hp = currentPlayer.hpMax;
            currentPlayer.mp = currentPlayer.mpMax;
            currentPlayer.attack += 3;
            currentPlayer.defend += 2;
            currentPlayer.strength += 1;
            currentPlayer.agility += 1;
            currentPlayer.skillPoint++;
            SendLevelUpResponse(currentPlayer.idPlayer, (sbyte)currentPlayer.level, currentPlayer.hpMax, currentPlayer.mpMax);
            SendMainCharInfo(currentPlayer);
        }
        else
        {
            short percent = (short)((currentEXP * 100) / expToNextLevel);
            SendSetXPPercentResponse(currentPlayer.idPlayer, percent);
        }
    }

    private void SendSetXPResponse(short whoSetXP, int dxp)
    {
        Message m = new Message(Cmd_message.SET_XP);
        try
        {
            myWriter w = m.writer();
            w.writeShort(whoSetXP);
            w.writeShort(0);
            w.writeInt(dxp);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendSetXPResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendSetXPPercentResponse(short whoSetXP, short percent)
    {
        Message m = new Message(Cmd_message.SET_XP);
        try
        {
            myWriter w = m.writer();
            w.writeShort(whoSetXP);
            w.writeShort(percent);
            w.writeInt(0);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendSetXPPercentResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendLevelUpResponse(short id, sbyte level, int maxHP, int maxMP)
    {
        Message m = new Message(Cmd_message.LEVEL_UP);
        try
        {
            myWriter w = m.writer();
            w.writeShort(id);
            w.writeByte(level);
            w.writeInt(maxHP);
            w.writeInt(maxMP);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendLevelUpResponse ERROR: " + ex.StackTrace);
        }
    }

    private void HandleAttackMultiMonster(Message msg)
    {
        myReader r = new myReader(msg.getData());
        sbyte skillType = r.readByte();
        sbyte count = r.readByte();
        short[] monsterIDs = new short[count];
        for (int i = 0; i < count; i++)
            monsterIDs[i] = r.readShort();

        List<short> killedMonsters = new List<short>();
        int totalDamage = 0;
        foreach (short monsterID in monsterIDs)
        {
            if (!monsterHP.ContainsKey(monsterID)) continue;
            int[] data = monsterHP[monsterID];
            int hp = data[0];
            int baseDamage = currentPlayer.attack + rng.Next(5, 20);
            int monsterDefend = 2 + data[5] * 2;  // data[5]=level
            int damage = System.Math.Max(1, baseDamage - monsterDefend);
            int hpLeft = System.Math.Max(0, hp - damage);
            data[0] = hpLeft;
            monsterHP[monsterID] = data;
            totalDamage += damage;
            if (hpLeft <= 0)
                killedMonsters.Add(monsterID);
        }

        // Send multi-target response (CMD 106)
        Message m = new Message(106);
        try
        {
            myWriter w = m.writer();
            w.writeShort(currentPlayer.idPlayer);
            w.writeByte(skillType);
            w.writeInt(totalDamage > 0 ? totalDamage / count : 0);
            w.writeByte(0);
            w.writeByte((sbyte)currentPlayer.level);
            w.writeByte(-1);
            w.writeByte((sbyte)count);
            foreach (short monsterID in monsterIDs)
            {
                w.writeShort(monsterID);
                w.writeInt(monsterHP.ContainsKey(monsterID) ? monsterHP[monsterID][0] : 0);
            }
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("HandleAttackMultiMonster response ERROR: " + ex.StackTrace);
        }

        // Handle deaths and EXP
        foreach (short monsterID in killedMonsters)
        {
            if (monsterHP.ContainsKey(monsterID))
            {
                int[] data = monsterHP[monsterID];
                SendMonsterDieResponse(currentPlayer.idPlayer, monsterID, skillType, 0, 0, data);
                AwardEXP(data[5]);
            }
        }
    }

    // ========================= DROP ITEMS =========================

    private short dropIDCounter = 20001;

    private List<ItemDropInfo> GenerateDrops(int monsterType, int monsterLevel)
    {
        List<ItemDropInfo> drops = new List<ItemDropInfo>();
        int goldDrop = rng.Next(10, 50) * (1 + monsterLevel);
        currentPlayer.xu += goldDrop;
        int dropChance = rng.Next(0, 100);
        if (dropChance < 30)
        {
            short itemID = dropIDCounter++;
            short templateID = (short)(rng.Next(0, 8) * 7 + rng.Next(0, 7));
            short x = (short)(currentPlayer.x + rng.Next(-30, 30));
            short y = (short)(currentPlayer.y + rng.Next(-10, 10));
            ItemDropInfo drop = new ItemDropInfo();
            drop.itemCatagory = 3;
            drop.itemTemplateID = templateID;
            drop.itemID = itemID;
            drop.x = x;
            drop.y = y;
            drops.Add(drop);
            dropItems[itemID] = new sbyte[] { 3, (sbyte)((templateID >> 8) & 0xFF), (sbyte)(templateID & 0xFF), (sbyte)((x >> 8) & 0xFF), (sbyte)(x & 0xFF), (sbyte)((y >> 8) & 0xFF), (sbyte)(y & 0xFF) };
        }
        if (dropChance < 15)
        {
            short potionID = dropIDCounter++;
            short x = (short)(currentPlayer.x + rng.Next(-30, 30));
            short y = (short)(currentPlayer.y + rng.Next(-10, 10));
            ItemDropInfo drop = new ItemDropInfo();
            drop.itemCatagory = 4;
            drop.itemTemplateID = 0;
            drop.itemID = potionID;
            drop.x = x;
            drop.y = y;
            drops.Add(drop);
        }
        return drops;
    }

    // ========================= ITEM PICKUP =========================

    private void HandleGetItemFromGround(Message msg)
    {
        myReader r = new myReader(msg.getData());
        short itemID = r.readShort();
        SendGetItemFromGroundResponse(currentPlayer.idPlayer, itemID, 0, currentPlayer.x, currentPlayer.y);
    }

    private void HandleGetPotionFromGround(Message msg)
    {
        myReader r = new myReader(msg.getData());
        short itemID = r.readShort();
        SendGetPotionFromGroundResponse(currentPlayer.idPlayer, itemID, 0, 1);
    }

    private void SendGetItemFromGroundResponse(short whoGet, short itemID, sbyte clazz, short x, short y)
    {
        Message m = new Message(Cmd_message.GET_ITEM_FROM_GROUND);
        try
        {
            myWriter w = m.writer();
            w.writeShort(whoGet);
            w.writeByte(clazz);
            w.writeShort(itemID);
            w.writeShort(itemID);
            w.writeShort(0);
            w.writeByte(0);
            w.writeByte(1);
            w.writeShort(100);
            w.writeShort(100);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendGetItemFromGroundResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendGetPotionFromGroundResponse(short whoGet, short potionID, short potionType, short quantity)
    {
        Message m = new Message(Cmd_message.GET_POTION_FROM_GROUND);
        try
        {
            myWriter w = m.writer();
            w.writeShort(whoGet);
            w.writeShort(potionID);
            w.writeByte((sbyte)potionType);
            w.writeShort(quantity);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendGetPotionFromGroundResponse ERROR: " + ex.StackTrace);
        }
    }

    // ========================= POTION USE =========================

    private void HandleUsePotion(Message msg)
    {
        myReader r = new myReader(msg.getData());
        sbyte potionType = r.readByte();
        short valueAdd = 0;
        int valueNew = 0;
        int isHP = 0;

        if (potionType == 0 || potionType == 1 || potionType == 2)
        {
            valueAdd = 50;
            currentPlayer.hp = System.Math.Min(currentPlayer.hpMax, currentPlayer.hp + 50);
            valueNew = currentPlayer.hp;
            isHP = 1;
        }
        else if (potionType == 3 || potionType == 4 || potionType == 5)
        {
            valueAdd = 30;
            currentPlayer.mp = System.Math.Min(currentPlayer.mpMax, currentPlayer.mp + 30);
            valueNew = currentPlayer.mp;
            isHP = 0;
        }
        SendUsePotionResponse(currentPlayer.idPlayer, potionType, valueAdd, valueNew, isHP);
    }

    private void SendUsePotionResponse(short userId, sbyte potionType, short valueAdd, int valueNew, int isHP)
    {
        Message m = new Message(Cmd_message.USE_POTION);
        try
        {
            myWriter w = m.writer();
            w.writeShort(userId);
            w.writeByte(potionType);
            w.writeShort(valueAdd);
            w.writeInt(valueNew);
            w.writeByte((sbyte)isHP);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendUsePotionResponse ERROR: " + ex.StackTrace);
        }
    }

    // ========================= NPC =========================

    private void HandleTalkWithNPC(Message msg)
    {
        myReader r = new myReader(msg.getData());
        short npcID = r.readShort();
        sbyte menuID = r.readByte();
        SendNPCInfoResponse(npcID);
    }

    private void SendNPCInfoResponse(short npcID)
    {
        Message m = new Message(Cmd_message.NPC_INFO);
        try
        {
            myWriter w = m.writer();
            w.writeByte(0);
            w.writeShort(npcID);
            w.writeUTF("Thương nhân");
            w.writeUTF("Chào bạn! Mua gì không?");
            w.writeByte(2);
            w.writeUTF("Mua thuốc");
            w.writeUTF("Bán đồ");
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendNPCInfoResponse ERROR: " + ex.StackTrace);
        }
    }

    // ========================= SHOP =========================

    private void HandleBuyItemShop(Message msg)
    {
        myReader r = new myReader(msg.getData());
        sbyte shopID = r.readByte();
        short itemID = r.readShort();
        short quantity = r.readShort();
        currentPlayer.xu -= 500;
        SendBuyItemShopResponse(shopID, itemID, quantity);
    }

    private void SendBuyItemShopResponse(sbyte shopID, short itemID, short quantity)
    {
        Message m = new Message(Cmd_message.BUY_ITEM_SHOP);
        try
        {
            myWriter w = m.writer();
            w.writeByte(shopID);
            w.writeShort(itemID);
            w.writeShort(quantity);
            w.writeShort((short)currentPlayer.xu);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendBuyItemShopResponse ERROR: " + ex.StackTrace);
        }
    }

    // ========================= SEND RESPONSES =========================

    private void SendLoginResponse()
    {
        Message m = new Message(1);
        try
        {
            myWriter w = m.writer();
            w.writeByte(5);
            w.writeShort(0);
            byte pc = (byte)OfflineData.potionNames.Length;
            w.writeByte((sbyte)pc);
            for (int i = 0; i < pc; i++)
            {
                w.writeByte((sbyte)i);
                w.writeUTF(OfflineData.potionNames[i]);
                w.writeUTF(OfflineData.potionNames[i]);
                w.writeShort(200);
                w.writeBoolean(true);
            }
            for (int cl = 0; cl < 5; cl++)
                w.writeByte(14);
            for (int cl = 0; cl < 5; cl++)
                w.writeByte(0);
            w.writeShort(10);
            w.writeByte(0);
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    w.writeByte(0);
                    w.writeByte(50);
                    w.writeByte(0);
                    w.writeByte(0);
                }
            w.writeByte(10);
            w.writeByte(10);
            w.writeByte(100);
            w.writeByte(100);
            w.writeByte(0);
            w.writeUTF("19001530");
            w.writeByte(0);
            w.writeByte(0);
            w.writeUTF("http://localhost");
            w.writeByte(0);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendLoginResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendCharListResponse()
    {
        List<int> ids = OfflineSave.LoadCharIds(currentUser);
        Message m = new Message(13);
        try
        {
            myWriter w = m.writer();
            w.writeByte(0);
            w.writeShort((short)ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                OfflinePlayer p = OfflineSave.LoadPlayer(ids[i]);
                if (p == null) continue;
                w.writeShort((short)p.idDatabase);
                w.writeUTF(p.name);
                w.writeByte((sbyte)p.head);
                w.writeByte(3);
                w.writeByte(0);
                w.writeByte((sbyte)(p.gender == 0 ? 2 : 1));
                w.writeByte(1);
                w.writeByte((sbyte)(p.gender == 0 ? 2 : 1));
                w.writeByte((sbyte)(3 + p.clazz));
                w.writeByte(0);
                w.writeShort(p.level);
                w.writeByte(1);
                w.writeByte((sbyte)p.idNation);
                w.writeShort(0);
                w.writeByte(0);
            }
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendCharListResponse ERROR: " + ex.StackTrace);
        }
    }

    private void SendMainCharInfo(OfflinePlayer p)
    {
        Message m = new Message(3);
        try
        {
            myWriter w = m.writer();
            w.writeShort(p.idPlayer);
            w.writeUTF(p.name);
            w.writeInt(p.hp);
            w.writeInt(p.hpMax);
            w.writeInt(p.mp);
            w.writeInt(p.mpMax);
            w.writeByte((sbyte)p.head);
            w.writeByte((sbyte)p.clazz);
            w.writeInt(p.attack);
            w.writeInt(p.defend);
            w.writeInt(p.defendMagic);
            w.writeShort(p.accurate);
            w.writeShort(p.dodge);
            w.writeShort(p.critical);
            w.writeByte((sbyte)p.he);
            w.writeByte((sbyte)p.level);
            w.writeShort(0);
            w.writeShort(p.strength);
            w.writeShort(p.agility);
            w.writeShort(p.spirit);
            w.writeShort(p.health);
            w.writeShort(p.luck);
            w.writeShort(p.basePoint);
            w.writeShort(p.skillPoint);
            w.writeInt(p.dedicationPoint);
            w.writeShort(p.baokich);
            byte[] sl = p.skillLevels != null ? p.skillLevels : new byte[0];
            w.writeByte((sbyte)sl.Length);
            for (int i = 0; i < sl.Length; i++)
                w.writeByte((sbyte)sl[i]);
            w.writeShort(0);
            w.writeByte((sbyte)p.gender);
            w.writeByte(0);
            w.writeByte(0);
            w.writeByte(0);
            w.writeByte(5);
            w.writeShort(-1);
            w.writeBoolean(false);
            w.writeByte((sbyte)p.limItemBag);
            w.writeByte((sbyte)p.idNation);
            w.writeByte((sbyte)p.idNation);
            w.writeShort(0);
            w.writeInt(0);
            w.writeShort(0);
            w.writeInt(0);
            w.writeUTF("");
            w.writeBoolean(false);
            w.writeShort(-1);
            w.writeByte(0);
            w.writeShort(-1);
            w.writeByte(0);
            w.writeShort(-1);
            w.writeShort(-1);
            w.writeShort(-1);
            w.writeShort(-1);
            w.writeByte(0);
            w.writeUTF("");
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMainCharInfo ERROR: " + ex.StackTrace);
        }
    }

    private void SendChangeMap(OfflinePlayer p)
    {
        try
        {
            // Generate tile data for offline map
            sbyte[] tileData = GenerateOfflineTileData(40, 30);
            Message m = new Message(12);
            myWriter w = m.writer();
            w.writeShort(p.zone);
            w.writeShort(p.x);
            w.writeShort(p.y);
            w.writeByte(-1);
            w.writeShort(p.zone);
            w.writeUTF("Ngoài thành");
            w.writeBoolean(true);
            for (int i = 0; i < tileData.Length; i++)
                w.writeByte(tileData[i]);
            Session_ME.gI().onReceiveMessage(m);
            SetBorderCollision();
            SpawnMonsters();
            Canvas.endDlg();
        }
        catch (Exception ex)
        {
            Debug.LogError("SendChangeMap ERROR: " + ex.StackTrace);
        }
    }

    private sbyte[] GenerateOfflineTileData(int w, int h)
    {
        List<byte> data = new List<byte>();
        data.Add((byte)w);
        data.Add((byte)h);
        for (int i = 0; i < w * h; i++)
        {
            int x = i % w;
            int y = i / w;
            if (x <= 0 || y <= 0 || x >= w - 1 || y >= h - 1)
                data.Add(3);
            else
                data.Add(0);
        }
        data.Add(255);
        data.Add(255);
        data.Add(254);
        data.Add(1);
        sbyte[] result = new sbyte[data.Count];
        for (int i = 0; i < data.Count; i++)
            result[i] = (sbyte)data[i];
        return result;
    }


    private void SetBorderCollision()
    {
        try
        {
            if (Tilemap.type == null) return;
            int w = Tilemap.w;
            int h = Tilemap.h;
            for (int i = 0; i < w; i++)
            {
                Tilemap.type[i] |= 2;             // top
                Tilemap.type[(h - 1) * w + i] |= 2; // bottom
            }
            for (int j = 0; j < h; j++)
            {
                Tilemap.type[j * w] |= 2;           // left
                Tilemap.type[j * w + (w - 1)] |= 2; // right
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SetBorderCollision ERROR: " + ex.StackTrace);
        }
    }

    private void SpawnMonsters()
    {
        try
        {
            int mapW = Tilemap.w;
            int mapH = Tilemap.h;
            if (mapW <= 0 || mapH <= 0) return;
            int count = 5 + rng.Next(0, 4);
            for (int i = 0; i < count; i++)
            {
                short monsterID = (short)(10000 + i);
                short mx = (short)(rng.Next(3, mapW - 3) * 16 + 8);
                short my = (short)(rng.Next(3, mapH - 3) * 16 + 8);
                short monsterType = (short)(rng.Next(1, 6));
                int hp = 200 + monsterType * 200;
                int maxhp = hp;
                monsterHP[monsterID] = new int[] { hp, maxhp, monsterType, mx, my, 1 };
                SendMonsterInfo(monsterID, monsterType, mx, my, hp, 1, 0, maxhp);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SpawnMonsters ERROR: " + ex.StackTrace);
        }
    }

    private void SendMonsterInfo(short id, short type, short x, short y, int hp, sbyte lv, sbyte he, int maxhp)
    {
        Message m = new Message(Cmd_message.MONSTER_INFO);
        try
        {
            myWriter w = m.writer();
            w.writeShort(id);
            w.writeByte((sbyte)type);
            w.writeShort(x);
            w.writeShort(y);
            w.writeInt(hp);
            w.writeByte(lv);
            w.writeByte(he);
            w.writeInt(maxhp);
            w.writeInt(30000);
            Session_ME.gI().onReceiveMessage(m);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMonsterInfo ERROR: " + ex.StackTrace);
        }
    }

    // ========================= SAVE =========================

    public void SaveCurrentPlayer()
    {
        if (currentPlayer != null)
        {
            // Sync position from game mainChar before saving
            try
            {
                if (Canvas.gameScr != null && Canvas.gameScr.mainChar != null)
                {
                    currentPlayer.x = Canvas.gameScr.mainChar.x;
                    currentPlayer.y = Canvas.gameScr.mainChar.y;
                    currentPlayer.hp = Canvas.gameScr.mainChar.hp;
                    currentPlayer.hpMax = Canvas.gameScr.mainChar.maxhp;
                    currentPlayer.mp = Canvas.gameScr.mainChar.mp;
                    currentPlayer.mpMax = Canvas.gameScr.mainChar.maxmp;
                    currentPlayer.level = (byte)Canvas.gameScr.mainChar.level;
                    currentPlayer.exp = currentEXP;
                }
            }
            catch (Exception) { }
            currentPlayer.lastTimeLogout = mSystem.getCurrentTimeMillis();
            OfflineSave.SavePlayer(currentPlayer);
        }
    }

    public void Deactivate()
    {
        SaveCurrentPlayer();
        isActive = false;
        currentPlayer = null;
        currentUser = null;
    }
}
