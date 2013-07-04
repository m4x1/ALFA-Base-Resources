﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using CLRScriptFramework;
using ALFA;
using NWScript;
using NWScript.ManagedInterfaceLayer.NWScriptManagedInterface;

using OEIShared.IO.GFF;

using NWEffect = NWScript.NWScriptEngineStructure0;
using NWEvent = NWScript.NWScriptEngineStructure1;
using NWLocation = NWScript.NWScriptEngineStructure2;
using NWTalent = NWScript.NWScriptEngineStructure3;
using NWItemProperty = NWScript.NWScriptEngineStructure4;

namespace ACR_Items
{
    public partial class Pricing : CLRScriptBase
    {
        #region Base Item Classifications
        static List<int> OOC = new List<int>
        {
            BASE_ITEM_CBLUDGWEAPON,
            BASE_ITEM_CPIERCWEAPON,
            BASE_ITEM_CREATUREITEM,
            BASE_ITEM_CSLASHWEAPON,
            BASE_ITEM_CSLSHPRCWEAP,
            BASE_ITEM_INVALID,
        };

        static List<int> Armor = new List<int>
        {
            BASE_ITEM_ARMOR,
            BASE_ITEM_SMALLSHIELD,
            BASE_ITEM_LARGESHIELD,
            BASE_ITEM_TOWERSHIELD,
        };
        static List<int> Ammunition = new List<int>
        {
            BASE_ITEM_ARROW,
            BASE_ITEM_BOLT,
            BASE_ITEM_BULLET,
            BASE_ITEM_DART,
            BASE_ITEM_SHURIKEN,
            BASE_ITEM_THROWINGAXE,
        };

        static List<int> LightWeapons = new List<int>
        {
            BASE_ITEM_SHORTSWORD,
            BASE_ITEM_LIGHTMACE,
            BASE_ITEM_DAGGER,
            BASE_ITEM_LIGHTHAMMER,
            BASE_ITEM_HANDAXE,
            BASE_ITEM_KUKRI,
            BASE_ITEM_SICKLE,
            BASE_ITEM_WHIP,
            BASE_ITEM_LIGHTFLAIL,
            BASE_ITEM_RAPIER,
        };

        static List<int> HeavyWeapons = new List<int>
        {
            BASE_ITEM_HALBERD,
            BASE_ITEM_GREATSWORD,
            BASE_ITEM_GREATAXE,
            BASE_ITEM_QUARTERSTAFF,
            BASE_ITEM_SCYTHE,
            BASE_ITEM_FALCHION,
            BASE_ITEM_SPEAR,
            BASE_ITEM_GREATCLUB,
            BASE_ITEM_WARMACE,
        };

        static List<int> Weapons = new List<int>
        {
                BASE_ITEM_ALLUSE_SWORD,
                BASE_ITEM_BASTARDSWORD,
                BASE_ITEM_BATTLEAXE,
                BASE_ITEM_CLUB,
                BASE_ITEM_DAGGER,
                BASE_ITEM_DWARVENWARAXE,
                BASE_ITEM_FALCHION,
                BASE_ITEM_FLAIL,
                BASE_ITEM_GREATAXE,
                BASE_ITEM_GREATCLUB,
                BASE_ITEM_GREATSWORD,
                BASE_ITEM_HALBERD,
                BASE_ITEM_HANDAXE,
                BASE_ITEM_HEAVYCROSSBOW,
                BASE_ITEM_KAMA,
                BASE_ITEM_KATANA,
                BASE_ITEM_KUKRI,
                BASE_ITEM_LIGHTCROSSBOW,
                BASE_ITEM_LIGHTFLAIL,
                BASE_ITEM_LIGHTHAMMER,
                BASE_ITEM_LIGHTMACE,
                BASE_ITEM_LONGBOW,
                BASE_ITEM_LONGSWORD,
                BASE_ITEM_MAGICSTAFF,
                BASE_ITEM_MORNINGSTAR,
                BASE_ITEM_QUARTERSTAFF,
                BASE_ITEM_RAPIER,
                BASE_ITEM_SCIMITAR,
                BASE_ITEM_SCYTHE,
                BASE_ITEM_SHORTBOW,
                BASE_ITEM_SHORTSPEAR,
                BASE_ITEM_SHORTSWORD,
                BASE_ITEM_SICKLE,
                BASE_ITEM_SLING,
                BASE_ITEM_SPEAR,
                BASE_ITEM_TRAINING_CLUB,
                BASE_ITEM_WARHAMMER,
                BASE_ITEM_WARMACE,
                BASE_ITEM_WHIP,
        };
        #endregion
        #region Base Item Values
        static Dictionary<int, int> BaseItemValues = new Dictionary<int, int>
        {
            {BASE_ITEM_ALFAPOTION, 0},
            {BASE_ITEM_ALLUSE_SWORD, 30},
            {BASE_ITEM_AMULET, 0},
            {BASE_ITEM_ARMOR, 0},
            {BASE_ITEM_ARROW, 1},
            {BASE_ITEM_BAG, 0},
            {BASE_ITEM_BALOR_FALCHION, 0},
            {BASE_ITEM_BALOR_SWORD, 0},
            {BASE_ITEM_BASTARDSWORD, 70},
            {BASE_ITEM_BATTLEAXE, 20},
            {BASE_ITEM_BELT, 0},
            {BASE_ITEM_BLANK_POTION, 0},
            {BASE_ITEM_BLANK_SCROLL, 0},
            {BASE_ITEM_BLANK_WAND, 0},
            {BASE_ITEM_BOLT, 1},
            {BASE_ITEM_BOOK, 0},
            {BASE_ITEM_BOOTS, 0},
            {BASE_ITEM_BOTTLE, 0},
            {BASE_ITEM_BOUNTYITEM, 0},
            {BASE_ITEM_BRACER, 0},
            {BASE_ITEM_BULLET, 1},
            {BASE_ITEM_CBLUDGWEAPON, 0},
            {BASE_ITEM_CGIANT_AXE, 0},
            {BASE_ITEM_CGIANT_SWORD, 0},
            {BASE_ITEM_CLOAK, 0},
            {BASE_ITEM_CLUB, 2},
            {BASE_ITEM_CPIERCWEAPON, 0},
            {BASE_ITEM_CRAFTBASE, 0},
            {BASE_ITEM_CRAFTMATERIALMED, 0},
            {BASE_ITEM_CRAFTMATERIALSML, 0},
            {BASE_ITEM_CREATUREITEM, 0},
            {BASE_ITEM_CSLASHWEAPON, 0},
            {BASE_ITEM_CSLSHPRCWEAP, 0},
            {BASE_ITEM_DAGGER, 4},
            {BASE_ITEM_DART, 1},
            {BASE_ITEM_DRUM, 5},
            {BASE_ITEM_DWARVENWARAXE, 60},
            {BASE_ITEM_ENCHANTED_POTION, 0},
            {BASE_ITEM_ENCHANTED_SCROLL, 0},
            {BASE_ITEM_ENCHANTED_WAND, 0},
            {BASE_ITEM_EQUIP_OFFHAND, 0},
            {BASE_ITEM_FALCHION, 150},
            {BASE_ITEM_FLAIL, 16},
            {BASE_ITEM_FLUTE, 5},
            {BASE_ITEM_GEM, 0},
            {BASE_ITEM_GLOVES, 0},
            {BASE_ITEM_GOLD, 0},
            {BASE_ITEM_GREATAXE, 40},
            {BASE_ITEM_GREATCLUB, 20},
            {BASE_ITEM_GREATSWORD, 100},
            {BASE_ITEM_GRENADE, 0},
            {BASE_ITEM_HALBERD, 20},
            {BASE_ITEM_HANDAXE, 12},
            {BASE_ITEM_HEALERSKIT, 0},
            {BASE_ITEM_HEAVYCROSSBOW, 100},
            {BASE_ITEM_HELMET, 0},
            {BASE_ITEM_INCANTATION, 0},
            {BASE_ITEM_INKWELL, 0},
            {BASE_ITEM_INVALID, 0},
            {BASE_ITEM_KAMA, 4},
            {BASE_ITEM_KATANA, 80},
            {BASE_ITEM_KEY, 0},
            {BASE_ITEM_KUKRI, 16},
            {BASE_ITEM_LARGEBOX, 0},
            {BASE_ITEM_LARGESHIELD, 50},
            {BASE_ITEM_LIGHTCROSSBOW, 70},
            {BASE_ITEM_LIGHTFLAIL, 16},
            {BASE_ITEM_LIGHTHAMMER, 2},
            {BASE_ITEM_LIGHTMACE, 10},
            {BASE_ITEM_LONGBOW, 150},
            {BASE_ITEM_LONGSWORD, 30},
            {BASE_ITEM_MAGICROD, 0},
            {BASE_ITEM_MAGICSTAFF, 2},
            {BASE_ITEM_MAGICWAND, 0},
            {BASE_ITEM_MANDOLIN, 5},
            {BASE_ITEM_MISCLARGE, 0},
            {BASE_ITEM_MISCMEDIUM, 0},
            {BASE_ITEM_MISCSMALL, 0},
            {BASE_ITEM_MISCSTACK, 0},
            {BASE_ITEM_MISCTHIN, 0},
            {BASE_ITEM_MORNINGSTAR, 16},
            {BASE_ITEM_PACK, 0},
            {BASE_ITEM_PAN, 0},
            {BASE_ITEM_POT, 0},
            {BASE_ITEM_POTIONS, 0},
            {BASE_ITEM_QUARTERSTAFF, 2},
            {BASE_ITEM_RAKE, 0},
            {BASE_ITEM_RAPIER, 40},
            {BASE_ITEM_RECIPE, 0},
            {BASE_ITEM_RING, 0},
            {BASE_ITEM_SCIMITAR, 30},
            {BASE_ITEM_SCYTHE, 36},
            {BASE_ITEM_SHORTBOW, 70},
            {BASE_ITEM_SHORTSPEAR, 20},
            {BASE_ITEM_SHORTSTAFF, 4},
            {BASE_ITEM_SHORTSWORD, 20},
            {BASE_ITEM_SHOVEL, 0},
            {BASE_ITEM_SHURIKEN, 1},
            {BASE_ITEM_SICKLE, 12},
            {BASE_ITEM_SLING, 2},
            {BASE_ITEM_SMALLSHIELD, 10},
            {BASE_ITEM_SMITHYHAMMER, 0},
            {BASE_ITEM_SPEAR, 4},
            {BASE_ITEM_SPELLSCROLL, 0},
            {BASE_ITEM_SPOON, 0},
            {BASE_ITEM_STEIN, 0},
            {BASE_ITEM_THIEVESTOOLS, 0},
            {BASE_ITEM_THROWINGAXE, 1},
            {BASE_ITEM_TORCH, 0},
            {BASE_ITEM_TOWERSHIELD, 100},
            {BASE_ITEM_TRADEGOODS, 0},
            {BASE_ITEM_TRAINING_CLUB, 2},
            {BASE_ITEM_TRAPKIT, 0},
            {BASE_ITEM_WARHAMMER, 24},
            {BASE_ITEM_WARMACE, 100},
            {BASE_ITEM_WHIP, 10},
        };

        static Dictionary<int, int> ArmorRulesTypeValues = new Dictionary<int, int>
        {
            {ARMOR_RULES_TYPE_BANDED, 250},
            {ARMOR_RULES_TYPE_BANDED_MASTERWORK, 400},
            {ARMOR_RULES_TYPE_BANDED_MITHRAL, 9250},
            {ARMOR_RULES_TYPE_BANDED_HEAVYMETAL, 400},
            {ARMOR_RULES_TYPE_BANDED_LIVING, 5400},
            {ARMOR_RULES_TYPE_BANDED_DRAGON, 11250},
            {ARMOR_RULES_TYPE_BREASTPLATE, 200},
            {ARMOR_RULES_TYPE_BREASTPLATE_MASTERWORK, 350},
            {ARMOR_RULES_TYPE_BREASTPLATE_MITHRAL, 4200},
            {ARMOR_RULES_TYPE_BREASTPLATE_DRAGON, 6200},
            {ARMOR_RULES_TYPE_BREASTPLATE_DUSKWOOD, 3200},
            {ARMOR_RULES_TYPE_BREASTPLATE_HEAVYMETAL, 350},
            {ARMOR_RULES_TYPE_BREASTPLATE_LIVING, 3700},
            {ARMOR_RULES_TYPE_CHAIN_SHIRT, 100},
            {ARMOR_RULES_TYPE_CHAIN_SHIRT_MASTERWORK, 250},
            {ARMOR_RULES_TYPE_CHAIN_SHIRT_MITHRAL, 1100},
            {ARMOR_RULES_TYPE_CHAIN_SHIRT_HEAVYMETAL, 250},
            {ARMOR_RULES_TYPE_CHAIN_SHIRT_LIVING, 2100},
            {ARMOR_RULES_TYPE_CHAINMAIL, 150},
            {ARMOR_RULES_TYPE_CHAINMAIL_MASTERWORK, 300},
            {ARMOR_RULES_TYPE_CHAINMAIL_MITHRAL, 4150},
            {ARMOR_RULES_TYPE_CHAINMAIL_HEAVYMETAL, 300},
            {ARMOR_RULES_TYPE_CHAINMAIL_LIVING, 3650},
            {ARMOR_RULES_TYPE_CLOTH, 0},
            {ARMOR_RULES_TYPE_FULL_PLATE, 1500},
            {ARMOR_RULES_TYPE_FULL_PLATE_MASTERWORK, 1650},
            {ARMOR_RULES_TYPE_FULL_PLATE_MITHRAL, 10500},
            {ARMOR_RULES_TYPE_FULL_PLATE_DRAGON, 12500},
            {ARMOR_RULES_TYPE_FULL_PLATE_HEAVYMETAL, 1650},
            {ARMOR_RULES_TYPE_FULL_PLATE_LIVING, 6500},
            {ARMOR_RULES_TYPE_HALF_PLATE, 600},
            {ARMOR_RULES_TYPE_HALF_PLATE_MASTERWORK, 750},
            {ARMOR_RULES_TYPE_HALF_PLATE_MITHRAL, 9600},
            {ARMOR_RULES_TYPE_HALF_PLATE_DRAGON, 11600},
            {ARMOR_RULES_TYPE_HALF_PLATE_HEAVYMETAL, 750},
            {ARMOR_RULES_TYPE_HALF_PLATE_LIVING, 6600},
            {ARMOR_RULES_TYPE_HIDE, 15},
            {ARMOR_RULES_TYPE_HIDE_MASTERWORK, 165},
            {ARMOR_RULES_TYPE_HIDE_DRAGON, 6015},
            {ARMOR_RULES_TYPE_LEATHER, 10},
            {ARMOR_RULES_TYPE_LEATHER_MASTERWORK, 160},
            {ARMOR_RULES_TYPE_LEATHER_DRAGON, 3010},
            {ARMOR_RULES_TYPE_PADDED, 5},
            {ARMOR_RULES_TYPE_PADDED_MASTERWORK, 155},
            {ARMOR_RULES_TYPE_PADDED_DRAGON, 3005},
            {ARMOR_RULES_TYPE_SCALE, 50},
            {ARMOR_RULES_TYPE_SCALE_MASTERWORK, 200},
            {ARMOR_RULES_TYPE_SCALE_MITHRAL, 4050},
            {ARMOR_RULES_TYPE_SCALE_DRAGON, 6050},
            {ARMOR_RULES_TYPE_SCALE_HEAVYMETAL, 200},
            {ARMOR_RULES_TYPE_SCALE_LIVING, 3550},
            {ARMOR_RULES_TYPE_SHIELD_HEAVY, 50},
            {ARMOR_RULES_TYPE_SHIELD_HEAVY_DARKWOOD, 230},
            {ARMOR_RULES_TYPE_SHIELD_HEAVY_MASTERWORK, 200},
            {ARMOR_RULES_TYPE_SHIELD_HEAVY_MITHRAL, 1050},
            {ARMOR_RULES_TYPE_SHIELD_HEAVY_DRAGON, 4050},
            {ARMOR_RULES_TYPE_SHIELD_LIGHT, 10},
            {ARMOR_RULES_TYPE_SHIELD_LIGHT_DARKWOOD, 300},
            {ARMOR_RULES_TYPE_SHIELD_LIGHT_MASTERWORK, 160},
            {ARMOR_RULES_TYPE_SHIELD_LIGHT_MITHRAL, 1010},
            {ARMOR_RULES_TYPE_SHIELD_LIGHT_DRAGON, 4010},
            {ARMOR_RULES_TYPE_SHIELD_TOWER, 100},
            {ARMOR_RULES_TYPE_SHIELD_TOWER_DARKWOOD, 700},
            {ARMOR_RULES_TYPE_SHIELD_TOWER_MASTERWORK, 250},
            {ARMOR_RULES_TYPE_SHIELD_TOWER_MITHRAL, 1100},
            {ARMOR_RULES_TYPE_SHIELD_TOWER_DRAGON, 4100},
            {ARMOR_RULES_TYPE_SPLINT, 250},
            {ARMOR_RULES_TYPE_SPLINT_MASTERWORK, 400},
            {ARMOR_RULES_TYPE_SPLINT_MITHREAL, 9250},
            {ARMOR_RULES_TYPE_SPLINT_DRAGON, 11250},
            {ARMOR_RULES_TYPE_SPLINT_HEAVYMETAL, 400},
            {ARMOR_RULES_TYPE_SPLINT_LIVING, 5250},
            {ARMOR_RULES_TYPE_STUDDED_LEATHER, 15},
            {ARMOR_RULES_TYPE_STUDDED_LEATHER_MASTERWORK, 165},
            {ARMOR_RULES_TYPE_LEATHER_STUDDED_DRAGON, 3015},
        };
        #endregion
    }
}
