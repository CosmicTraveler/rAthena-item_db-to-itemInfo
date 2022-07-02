﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using EasyButtons;
using System;
using System.Text;

public class Converter : MonoBehaviour
{
    const string CREATOR_URL = "https://kanintemsrisukgames.wordpress.com/2019/04/05/support-kt-games/";
    const float ONE_SECOND = 1;

    bool _isFilesError;
    string _errorLog;

    /// <summary>
    /// Button to start convert
    /// </summary>
    [SerializeField] Button _btnConvert;
    /// <summary>
    /// Button to see creator
    /// </summary>
    [SerializeField] Button _btnCreator;
    /// <summary>
    /// Convert button gameObject
    /// </summary>
    [SerializeField] GameObject[] _objectsToHideWhenConverterStart;
    /// <summary>
    /// Progress gameObject
    /// </summary>
    [SerializeField] GameObject _objConvertProgression;
    /// <summary>
    /// Text convert progression
    /// </summary>
    [SerializeField] Text _txtConvertProgression;
    /// <summary>
    /// Hardcode item scripts description
    /// </summary>
    [SerializeField] HardcodeItemScripts _hardcodeItemScripts;
    /// <summary>
    /// Localization
    /// </summary>
    [SerializeField] Localization _localization;

    // Settings

    /// <summary>
    /// Is print out zero value? (Example: Attack: 0)
    /// </summary>
    [SerializeField] bool _isZeroValuePrintable;
    /// <summary>
    /// Only read text asset from 'item_db_test.txt'
    /// </summary>
    [SerializeField] bool _isOnlyUseTestTextAsset;
    /// <summary>
    /// Only read text asset from 'item_db_custom.txt'
    /// </summary>
    [SerializeField] bool _isOnlyUseCustomTextAsset;
    /// <summary>
    /// Is random resource name for all item?
    /// </summary>
    [SerializeField] bool _isRandomResourceName;
    /// <summary>
    /// Is only random resource name for custom item?
    /// </summary>
    [SerializeField] bool _isRandomResourceNameForCustomTextAssetOnly;

    // Containers

    /// <summary>
    /// Container for 1 item, use while parsing from item_db
    /// </summary>
    ItemContainer _itemContainer = new ItemContainer();
    /// <summary>
    /// Container that hold all item ids (Split by item type)
    /// </summary>
    ItemListContainer _itemListContainer = new ItemListContainer();
    /// <summary>
    /// Container that hold all resource names (Spilt by equipment type)
    /// </summary>
    ResourceContainer _resourceContainer = new ResourceContainer();
    /// <summary>
    /// Class numbers holder
    /// </summary>
    Dictionary<int, string> _classNumberDatabases = new Dictionary<int, string>();
    /// <summary>
    /// Combos holder
    /// </summary>
    List<ComboDatabase> _comboDatabases = new List<ComboDatabase>();
    /// <summary>
    /// Items holder
    /// </summary>
    Dictionary<int, ItemDatabase> _itemDatabases = new Dictionary<int, ItemDatabase>();
    /// <summary>
    /// Aegis name holder
    /// </summary>
    Dictionary<string, int> _aegisNameDatabases = new Dictionary<string, int>();
    /// <summary>
    /// Monsters holder
    /// </summary>
    Dictionary<int, MonsterDatabase> _monsterDatabases = new Dictionary<int, MonsterDatabase>();
    /// <summary>
    /// Resources holder
    /// </summary>
    Dictionary<int, string> _resourceDatabases = new Dictionary<int, string>();
    /// <summary>
    /// Skills holder
    /// </summary>
    Dictionary<int, SkillDatabase> _skillDatabases = new Dictionary<int, SkillDatabase>();
    /// <summary>
    /// Skill name holder
    /// </summary>
    Dictionary<string, int> _skillNameDatabases = new Dictionary<string, int>();

    void Start()
    {
        _btnConvert.onClick.AddListener(OnConvertButtonTap);
        _btnCreator.onClick.AddListener(OnCreatorButtonTap);

        _objConvertProgression.SetActive(false);
        for (int i = 0; i < _objectsToHideWhenConverterStart.Length; i++)
            _objectsToHideWhenConverterStart[i].SetActive(true);
    }
    /// <summary>
    /// Call when creator button has been tap
    /// </summary>
    void OnCreatorButtonTap()
    {
        Application.OpenURL(CREATOR_URL);
    }
    /// <summary>
    /// Call when convert button has been tap
    /// </summary>
    void OnConvertButtonTap()
    {
        for (int i = 0; i < _objectsToHideWhenConverterStart.Length; i++)
            _objectsToHideWhenConverterStart[i].SetActive(false);

        _objConvertProgression.SetActive(true);

        _isFilesError = false;

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_START) + "..";

        Debug.Log(DateTime.Now);

        Invoke("FetchingData", ONE_SECOND);
    }
    void FetchingData()
    {
        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_ITEM) + "..";

        FetchItem();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_RESOURCE_NAME) + "..";

        FetchResourceName();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_SKILL) + "..";

        FetchSkill();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_CLASS_NUMBER) + "..";

        FetchClassNumber();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_CLASS_MONSTER) + "..";

        FetchMonster();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_ITEM_COMBO) + "..";

        FetchCombo();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_FETCHING_RESOURCE_NAME_WITH_TYPE) + "..";

        FetchResourceNameWithType();

        if (_isFilesError)
        {
            _txtConvertProgression.text = "<color=red>" + _localization.GetTexts(Localization.ERROR) + "</color>: " + _errorLog;

            return;
        }

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_PLEASE_WAIT) + "..";

        Debug.Log(DateTime.Now);

        Invoke("Convert", ONE_SECOND);
    }
    // Exporting

    /// <summary>
    /// Export item lists that can be use in game
    /// </summary>
    void ExportItemLists()
    {
        StringBuilder builder = new StringBuilder();

        ExportingItemLists(builder, "weaponIds", _itemListContainer.weaponIds);
        ExportingItemLists(builder, "equipmentIds", _itemListContainer.equipmentIds);
        ExportingItemLists(builder, "costumeIds", _itemListContainer.costumeIds);
        ExportingItemLists(builder, "cardIds", _itemListContainer.cardIds);
        ExportingItemLists(builder, "enchantIds", _itemListContainer.enchantIds);

        File.WriteAllText("global_item_ids.txt", builder.ToString(), Encoding.UTF8);

        Debug.Log("'global_item_ids.txt' has been successfully created.");
    }
    /// <summary>
    /// Exporting item lists to StringBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="listName"></param>
    /// <param name="items"></param>
    void ExportingItemLists(StringBuilder builder, string listName, List<string> items)
    {
        builder.Append("setarray $" + listName + "[0],");

        items.RemoveAll((item) => string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item) || (item == null));

        foreach (var item in items)
            builder.Append(item + ",");

        builder.Remove(builder.Length - 1, 1);

        builder.Append(";\n");
    }
    /// <summary>
    /// Export all item to item mall (NPC) that can be use in game
    /// </summary>
    void ExportItemMall()
    {
        // All item id

        int shopNumber = 0;

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < _itemListContainer.allItemIds.Count; i++)
        {
            if ((i == 0)
                || ((i % 100) == 0))
            {
                if (!string.IsNullOrEmpty(builder.ToString()))
                    builder.Remove(builder.Length - 1, 1);

                shopNumber++;

                builder.Append("\n-	shop	ItemMall" + shopNumber + "	-1,no,");
            }

            builder.Append(_itemListContainer.allItemIds[i] + ":1000000000,");
        }

        // Pet eggs item id

        shopNumber = 0;

        for (int i = 0; i < _itemListContainer.petEggIds.Count; i++)
        {
            if ((i == 0)
                || (i % 100 == 0))
            {
                if (!string.IsNullOrEmpty(builder.ToString()))
                    builder.Remove(builder.Length - 1, 1);

                shopNumber++;

                builder.Append("\n-	shop	PetEgg" + shopNumber + "	-1,no,");
            }

            builder.Append(_itemListContainer.petEggIds[i] + ":33333,");
        }

        // Pet armors item id

        shopNumber = 0;

        for (int i = 0; i < _itemListContainer.petArmorIds.Count; i++)
        {
            if ((i == 0)
                || (i % 100 == 0))
            {
                if (!string.IsNullOrEmpty(builder.ToString()))
                    builder.Remove(builder.Length - 1, 1);

                shopNumber++;

                builder.Append("\n-	shop	PetArmor" + shopNumber + "	-1,no,");
            }

            builder.Append(_itemListContainer.petArmorIds[i] + ":10000000,");
        }

        // Fasion costumes item id

        shopNumber = 0;

        for (int i = 0; i < _itemListContainer.fashionCostumeIds.Count; i++)
        {
            if ((i == 0)
                || (i % 100 == 0))
            {
                if (!string.IsNullOrEmpty(builder.ToString()))
                    builder.Remove(builder.Length - 1, 1);

                shopNumber++;

                builder.Append("\n-	shop	FashionCostume" + shopNumber + "	-1,no,");
            }

            builder.Append(_itemListContainer.fashionCostumeIds[i] + ":50000000,");
        }

        // Buffs item id

        shopNumber = 0;

        for (int i = 0; i < _itemListContainer.buffItemIds.Count; i++)
        {
            if ((i == 0)
                || (i % 100 == 0))
            {
                if (!string.IsNullOrEmpty(builder.ToString()))
                    builder.Remove(builder.Length - 1, 1);

                shopNumber++;

                builder.Append("\n-	shop	BuffItem" + shopNumber + "	-1,no,");
            }

            builder.Append(_itemListContainer.buffItemIds[i] + ":90000,");
        }

        var builderDebug = builder.ToString();

        if (!string.IsNullOrEmpty(builderDebug)
            && (builderDebug[builderDebug.Length - 1] == ','))
            builder.Remove(builder.Length - 1, 1);

        File.WriteAllText("item_mall.txt", builder.ToString(), Encoding.UTF8);

        Debug.Log("'item_mall.txt' has been successfully created.");
    }

    // Parsing

    /// <summary>
    /// Fetch resource name from all equipment (Split into list by equipment type)
    /// </summary>
    void FetchResourceNameWithType()
    {
        var path = Application.dataPath + "/Assets/item_db_equip.yml";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            return;
        }

        var equipmentsFile = File.ReadAllText(path);

        var equipments = equipmentsFile.Split('\n');

        _resourceContainer = new ResourceContainer();

        string id = string.Empty;

        bool isArmor = false;

        for (int i = 0; i < equipments.Length; i++)
        {
            var text = equipments[i];

            text = CommentRemover.Fix(text);

            text = LineEndingsRemover.Fix(text);

            // Skip
            if (text.Contains("    Buy:")
                || text.Contains("    Sell:")
                || text.Contains("    Jobs:")
                || text.Contains("    Classes:")
                || text.Contains("    AliasName:")
                || text.Contains("    Flags:")
                || text.Contains("    BuyingStore:")
                || text.Contains("    DeadBranch:")
                || text.Contains("    Container:")
                || text.Contains("    UniqueId:")
                || text.Contains("    BindOnEquip:")
                || text.Contains("    DropAnnounce:")
                || text.Contains("    NoConsume:")
                || text.Contains("    DropEffect:")
                || text.Contains("    Delay:")
                || text.Contains("    Duration:")
                || text.Contains("    Status:")
                || text.Contains("    Stack:")
                || text.Contains("    Amount:")
                || text.Contains("    Inventory:")
                || text.Contains("    Cart:")
                || text.Contains("    Storage:")
                || text.Contains("    GuildStorage:")
                || text.Contains("    NoUse:")
                || text.Contains("    Override:")
                || text.Contains("    Sitting:")
                || text.Contains("    Trade:")
                || text.Contains("    NoDrop:")
                || text.Contains("    NoTrade:")
                || text.Contains("    TradePartner:")
                || text.Contains("    NoSell:")
                || text.Contains("    NoCart:")
                || text.Contains("    NoStorage:")
                || text.Contains("    NoGuildStorage:")
                || text.Contains("    NoMail:")
                || text.Contains("    NoAuction:")
                || text.Contains("    Script:")
                || text.Contains("    OnEquip_Script:")
                || text.Contains("    OnUnequip_Script:")
                || string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                text = string.Empty;

            // Id
            if (text.Contains("  - Id:"))
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                id = text.Replace("-Id:", string.Empty);
            }
            // Type
            else if (text.Contains("    Type:"))
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                text = text.Replace("Type:", string.Empty);

                if ((text.ToLower() == "armor")
                    || (text.ToLower() == "shadowgear"))
                    isArmor = true;
                else
                    isArmor = false;
            }
            // Locations
            else if (isArmor)
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                if (text.ToLower().Contains("head_top"))
                    _resourceContainer.topHeadgears.Add(id);
                else if (text.ToLower().Contains("head_mid"))
                    _resourceContainer.middleHeadgears.Add(id);
                else if (text.ToLower().Contains("head_low"))
                    _resourceContainer.lowerHeadgears.Add(id);
                else if (text.ToLower().Contains("garment"))
                    _resourceContainer.garments.Add(id);
                else if (text.ToLower().Contains("armor"))
                    _resourceContainer.armors.Add(id);
                else if (text.ToLower().Contains("shadow_weapon")
                    || text.ToLower().Contains("shadow_shield"))
                    _resourceContainer.shields.Add(id);
                else if (text.ToLower().Contains("shoes"))
                    _resourceContainer.shoes.Add(id);
                else if (text.ToLower().Contains("accessory"))
                    _resourceContainer.accessorys.Add(id);
            }
            else
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                if (text.ToLower().Contains("dagger"))
                    _resourceContainer.daggers.Add(id);
                else if (text.ToLower().Contains("1hsword"))
                    _resourceContainer.oneHandedSwords.Add(id);
                else if (text.ToLower().Contains("2hsword"))
                    _resourceContainer.twoHandedSwords.Add(id);
                else if (text.ToLower().Contains("1hspear"))
                    _resourceContainer.oneHandedSpears.Add(id);
                else if (text.ToLower().Contains("2hspear"))
                    _resourceContainer.twoHandedSpears.Add(id);
                else if (text.ToLower().Contains("1haxe"))
                    _resourceContainer.oneHandedAxes.Add(id);
                else if (text.ToLower().Contains("2haxe"))
                    _resourceContainer.twoHandedAxes.Add(id);
                else if (text.ToLower().Contains("2hmace"))
                    _resourceContainer.twoHandedMaces.Add(id);
                else if (text.ToLower().Contains("mace"))
                    _resourceContainer.oneHandedMaces.Add(id);
                else if (text.ToLower().Contains("2hstaff"))
                    _resourceContainer.twoHandedStaffs.Add(id);
                else if (text.ToLower().Contains("staff"))
                    _resourceContainer.oneHandedStaffs.Add(id);
                else if (text.ToLower().Contains("bow"))
                    _resourceContainer.bows.Add(id);
                else if (text.ToLower().Contains("knuckle"))
                    _resourceContainer.knuckles.Add(id);
                else if (text.ToLower().Contains("musical"))
                    _resourceContainer.musicals.Add(id);
                else if (text.ToLower().Contains("whip"))
                    _resourceContainer.whips.Add(id);
                else if (text.ToLower().Contains("book"))
                    _resourceContainer.books.Add(id);
                else if (text.ToLower().Contains("katar"))
                    _resourceContainer.katars.Add(id);
                else if (text.ToLower().Contains("revolver"))
                    _resourceContainer.revolvers.Add(id);
                else if (text.ToLower().Contains("rifle"))
                    _resourceContainer.rifles.Add(id);
                else if (text.ToLower().Contains("gatling"))
                    _resourceContainer.gatlings.Add(id);
                else if (text.ToLower().Contains("shotgun"))
                    _resourceContainer.shotguns.Add(id);
                else if (text.ToLower().Contains("grenade"))
                    _resourceContainer.grenades.Add(id);
                else if (text.ToLower().Contains("huuma"))
                    _resourceContainer.huumas.Add(id);
            }
        }
    }
    /// <summary>
    /// Parse monster database file into converter (Only store ID, Name)
    /// </summary>
    void FetchMonster()
    {
        var path = Application.dataPath + "/Assets/mob_db.yml";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var monsterDatabasesFile = File.ReadAllText(path);

        var monsterDatabases = monsterDatabasesFile.Split('\n');

        _monsterDatabases = new Dictionary<int, MonsterDatabase>();

        MonsterDatabase monsterDatabase = new MonsterDatabase();

        for (int i = 0; i < monsterDatabases.Length; i++)
        {
            var text = monsterDatabases[i];

            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                continue;

            text = CommentRemover.Fix(text);

            text = LineEndingsRemover.Fix(text);

            if (text.Contains("  - Id:"))
                monsterDatabase.id = int.Parse(SpacingRemover.Remove(text).Replace("-Id:", string.Empty));
            else if (text.Contains("    Name:"))
            {
                monsterDatabase.name = QuoteRemover.Remove(text.Replace("    Name: ", string.Empty));

                if (_monsterDatabases.ContainsKey(monsterDatabase.id))
                    Debug.LogWarning("Found duplicated monster ID: " + monsterDatabase.id + " Please tell rAthena about this.");
                else
                    _monsterDatabases.Add(monsterDatabase.id, monsterDatabase);

                monsterDatabase = new MonsterDatabase();
            }
        }

        Debug.Log("There are " + _monsterDatabases.Count + " monster database.");
    }
    /// <summary>
    /// Parse class number file into converter
    /// </summary>
    void FetchClassNumber()
    {
        var path = Application.dataPath + "/Assets/classNum.txt";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var classNumbersFile = File.ReadAllText(path);

        var classNumbers = classNumbersFile.Split('\n');

        _classNumberDatabases = new Dictionary<int, string>();

        var id = 0;
        var classNumber = string.Empty;

        for (int i = 0; i < classNumbers.Length; i++)
        {
            var text = classNumbers[i];

            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                continue;

            text = LineEndingsRemover.Fix(text);

            var texts = text.Split('=');

            id = int.Parse(texts[0]);
            classNumber = texts[1];

            if (!_classNumberDatabases.ContainsKey(id))
                _classNumberDatabases.Add(id, classNumber);
        }

        path = Application.dataPath + "/Assets/item_db_custom.txt";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        classNumbersFile = File.ReadAllText(path);

        classNumbers = classNumbersFile.Split('\n');

        for (int i = 0; i < classNumbers.Length; i++)
        {
            var text = classNumbers[i];

            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                continue;

            text = CommentRemover.Fix(text);

            text = LineEndingsRemover.Fix(text);

            if (text.Contains("- Id:"))
                id = int.Parse(SpacingRemover.Remove(text).Replace("-Id:", string.Empty));
            else if (text.Contains("    View:"))
            {
                classNumber = SpacingRemover.Remove(text).Replace("View:", string.Empty);

                if (!_classNumberDatabases.ContainsKey(id))
                    _classNumberDatabases.Add(id, classNumber);
            }
        }

        Debug.Log("There are " + _classNumberDatabases.Count + " class number database.");
    }
    /// <summary>
    /// Parse skill database file into converter (Only store ID, Name)
    /// </summary>
    void FetchSkill()
    {
        var path = Application.dataPath + "/Assets/skill_db.yml";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var skillDatabasesFile = File.ReadAllText(path);

        var skillDatabases = skillDatabasesFile.Split('\n');

        _skillDatabases = new Dictionary<int, SkillDatabase>();
        _skillNameDatabases = new Dictionary<string, int>();

        SkillDatabase skillDatabase = new SkillDatabase();

        for (int i = 0; i < skillDatabases.Length; i++)
        {
            var text = skillDatabases[i];

            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                continue;

            text = CommentRemover.Fix(text);

            text = LineEndingsRemover.Fix(text);

            if (text.Contains("  - Id:"))
                skillDatabase.id = int.Parse(SpacingRemover.Remove(text).Replace("-Id:", string.Empty));
            else if (text.Contains("    Name:"))
                skillDatabase.name = QuoteRemover.Remove(text.Replace("    Name: ", string.Empty));
            else if (text.Contains("    Description:"))
            {
                skillDatabase.description = QuoteRemover.Remove(text.Replace("    Description: ", string.Empty));

                skillDatabase.nameWithQuote = "\"" + skillDatabase.name + "\"";

                if (_skillDatabases.ContainsKey(skillDatabase.id))
                    Debug.LogWarning("Found duplicated skill ID: " + skillDatabase.id + " (Old: " + _skillDatabases[skillDatabase.id] + " vs New: " + skillDatabase.id + ")");
                else
                    _skillDatabases.Add(skillDatabase.id, skillDatabase);

                if (_skillNameDatabases.ContainsKey(skillDatabase.name))
                    Debug.LogWarning("Found duplicated skill name: " + skillDatabase.name + " (Old: " + _skillNameDatabases[skillDatabase.name] + " vs New: " + skillDatabase.name + ")");
                else
                    _skillNameDatabases.Add(skillDatabase.name, skillDatabase.id);

                skillDatabase = new SkillDatabase();
            }
        }

        Debug.Log("There are " + _skillDatabases.Count + " skill database.");
    }
    /// <summary>
    /// Parse resource name file into converter
    /// </summary>
    void FetchResourceName()
    {
        var path = Application.dataPath + "/Assets/resourceName" + (_localization.IsUsingANSI ? "ANSI" : string.Empty) + ".txt";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var resourceNamesFile = File.ReadAllText(path, _localization.IsUsingANSI ? Encoding.GetEncoding(51949) : Encoding.UTF8);

        var resourceNames = resourceNamesFile.Split('\n');

        _resourceDatabases = new Dictionary<int, string>();

        for (int i = 0; i < resourceNames.Length; i++)
        {
            var text = resourceNames[i];

            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                continue;

            text = LineEndingsRemover.Fix(text);

            var texts = text.Split('=');

            var id = int.Parse(texts[0]);
            var name = texts[1];

            if (_resourceDatabases.ContainsKey(id))
                Debug.LogWarning("Found duplicated resource name ID: " + id + " (Old: " + _resourceDatabases[id] + " vs New: " + name + ")");
            else
                _resourceDatabases.Add(id, name);
        }

        Debug.Log("There are " + _resourceDatabases.Count + " resource name database.");
    }
    /// <summary>
    /// Parse combo database file into converter
    /// </summary>
    void FetchCombo()
    {
        var path = Application.dataPath + "/Assets/item_combos.yml";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var comboDatabasesFile = File.ReadAllText(path);

        var comboDatabases = comboDatabasesFile.Split('\n');

        _comboDatabases = new List<ComboDatabase>();

        ComboDatabase comboDatabase = new ComboDatabase();

        bool isScript = false;

        string script = string.Empty;

        // Comment remover

        // Nowaday rAthena use YAML for combo database, but it still had /* and */
        // Then just keep these for unexpected error

        for (int i = 0; i < comboDatabases.Length; i++)
        {
            var text = CommentRemover.FixCommentSeperateLine(comboDatabases, i);

            if (text.Contains("- Combos:"))
            {
                comboDatabase = new ComboDatabase();

                _comboDatabases.Add(comboDatabase);

                isScript = false;

                script = string.Empty;
            }
            else if (text.Contains("- Combo:"))
                comboDatabase.sameComboDatas.Add(new ComboDatabase.SameComboData());
            else if (text.Contains("          - "))
                comboDatabase.sameComboDatas[comboDatabase.sameComboDatas.Count - 1].aegisNames.Add(SpacingRemover.Remove(QuoteRemover.Remove(text.Replace("          - ", string.Empty))));
            else if (text.Contains("Script: |"))
                isScript = true;
            else if (isScript)
            {
                var comboScript = ConvertItemScripts(text);

                if (!string.IsNullOrEmpty(comboScript))
                    script += "			\"" + comboScript + "\",\n";

                // Is reach last line or next line is new combo database?
                // Cutoff and add combo scripts now
                if ((i + 1) >= comboDatabases.Length
                    || comboDatabases[i + 1].Contains("- Combos:"))
                    comboDatabase.descriptions.Add(script);
            }
        }

        Debug.Log("There are " + _comboDatabases.Count + " combo database.");
    }
    /// <summary>
    /// Parse item database file into converter (Only store ID, Name), also parse into item list
    /// </summary>
    void FetchItem()
    {
        var path = Application.dataPath + "/Assets/item_db_equip.yml";
        var path2 = Application.dataPath + "/Assets/item_db_usable.yml";
        var path3 = Application.dataPath + "/Assets/item_db_etc.yml";
        var path4 = Application.dataPath + "/Assets/item_db_custom.txt";

        // Is file exists?
        if (!File.Exists(path))
        {
            _errorLog = path + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }
        else if (!File.Exists(path2))
        {
            _errorLog = path2 + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }
        else if (!File.Exists(path3))
        {
            _errorLog = path3 + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }
        else if (!File.Exists(path4))
        {
            _errorLog = path4 + " " + _localization.GetTexts(Localization.NOT_FOUND);

            Debug.Log(_errorLog);

            _isFilesError = true;

            return;
        }

        var itemDatabasesFile = File.ReadAllText(path) + "\n"
            + File.ReadAllText(path2) + "\n"
            + File.ReadAllText(path3) + "\n"
            + File.ReadAllText(path4);

        var itemDatabases = itemDatabasesFile.Split('\n');

        _itemDatabases = new Dictionary<int, ItemDatabase>();
        _aegisNameDatabases = new Dictionary<string, int>();

        _itemListContainer = new ItemListContainer();

        ItemDatabase itemDatabase = new ItemDatabase();

        string _id = string.Empty;

        string _name = string.Empty;

        bool isArmor = false;

        for (int i = 0; i < itemDatabases.Length; i++)
        {
            var text = itemDatabases[i];

            text = CommentRemover.Fix(text);

            text = LineEndingsRemover.Fix(text);

            // Skip
            if (text.Contains("    Buy:")
                || text.Contains("    Sell:")
                || text.Contains("    Jobs:")
                || text.Contains("    Classes:")
                || text.Contains("    AliasName:")
                || text.Contains("    Flags:")
                || text.Contains("    BuyingStore:")
                || text.Contains("    DeadBranch:")
                || text.Contains("    Container:")
                || text.Contains("    UniqueId:")
                || text.Contains("    BindOnEquip:")
                || text.Contains("    DropAnnounce:")
                || text.Contains("    NoConsume:")
                || text.Contains("    DropEffect:")
                || text.Contains("    Delay:")
                || text.Contains("    Duration:")
                || text.Contains("    Status:")
                || text.Contains("    Stack:")
                || text.Contains("    Amount:")
                || text.Contains("    Inventory:")
                || text.Contains("    Cart:")
                || text.Contains("    Storage:")
                || text.Contains("    GuildStorage:")
                || text.Contains("    NoUse:")
                || text.Contains("    Override:")
                || text.Contains("    Sitting:")
                || text.Contains("    Trade:")
                || text.Contains("    NoDrop:")
                || text.Contains("    NoTrade:")
                || text.Contains("    TradePartner:")
                || text.Contains("    NoSell:")
                || text.Contains("    NoCart:")
                || text.Contains("    NoStorage:")
                || text.Contains("    NoGuildStorage:")
                || text.Contains("    NoMail:")
                || text.Contains("    NoAuction:")
                || text.Contains("    Script:")
                || text.Contains("    OnEquip_Script:")
                || text.Contains("    OnUnequip_Script:"))
                text = string.Empty;

            if (text.Contains("  - Id:"))
            {
                text = SpacingRemover.Remove(text);

                _id = text.Replace("-Id:", string.Empty);

                itemDatabase.id = int.Parse(_id);

                _itemListContainer.allItemIds.Add(_id);
            }
            else if (text.Contains("    AegisName:"))
            {
                text = QuoteRemover.Remove(text);

                _name = text.Replace("    AegisName: ", string.Empty);

                // To prevent some aegis name that had double spacebar format
                _name = SpacingRemover.Remove(_name);

                itemDatabase.aegisName = _name;
            }
            else if (text.Contains("    Name:"))
            {
                text = QuoteRemover.Remove(text);

                _name = text.Replace("    Name: ", string.Empty);

                itemDatabase.name = _name;

                if (_itemDatabases.ContainsKey(itemDatabase.id))
                    Debug.LogWarning("Found duplicated item ID: " + itemDatabase.id + " Please tell rAthena about this.");
                else
                    _itemDatabases.Add(itemDatabase.id, itemDatabase);

                if (_aegisNameDatabases.ContainsKey(itemDatabase.aegisName))
                    Debug.LogWarning("Found duplicated item aegis name: " + itemDatabase.aegisName + " Please tell rAthena about this.");
                else
                    _aegisNameDatabases.Add(itemDatabase.aegisName, itemDatabase.id);

                itemDatabase = new ItemDatabase();
            }
            else if (text.Contains("    Type:"))
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                text = text.Replace("Type:", string.Empty);

                isArmor = false;

                if (text.ToLower() == "weapon")
                    _itemListContainer.weaponIds.Add(_id);
                else if (text.ToLower() == "armor")
                    isArmor = true;
                else if ((text.ToLower() == "card")
                    && _name.ToLower().Contains(" card"))
                    _itemListContainer.cardIds.Add(_id);
                else if ((text.ToLower() == "card")
                    && !_name.ToLower().Contains(" card"))
                    _itemListContainer.enchantIds.Add(_id);
            }
            // Locations
            else if (isArmor)
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                if (text.ToLower().Contains("costume_head_top")
                    || text.ToLower().Contains("costume_head_mid")
                    || text.ToLower().Contains("costume_head_low")
                    || text.ToLower().Contains("costume_garment")
                    || text.ToLower().Contains("shadow_armor")
                    || text.ToLower().Contains("shadow_weapon")
                    || text.ToLower().Contains("shadow_shield")
                    || text.ToLower().Contains("shadow_shoes")
                    || text.ToLower().Contains("shadow_right_accessory")
                    || text.ToLower().Contains("shadow_left_accessory"))
                {
                    _itemListContainer.costumeIds.Add(_id);

                    // Always clear isArmor
                    isArmor = false;
                }
                else if (text.ToLower().Contains("head_top")
                    || text.ToLower().Contains("head_mid")
                    || text.ToLower().Contains("head_low")
                    || text.ToLower().Contains("armor")
                    || text.ToLower().Contains("left_hand")
                    || text.ToLower().Contains("garment")
                    || text.ToLower().Contains("shoes")
                    || text.ToLower().Contains("right_accessory")
                    || text.ToLower().Contains("left_accessory")
                    || text.ToLower().Contains("both_accessory"))
                {
                    _itemListContainer.equipmentIds.Add(_id);

                    // Always clear isArmor
                    isArmor = false;
                }
            }
            // Locations
            else
            {
                text = QuoteRemover.Remove(text);

                text = SpacingRemover.Remove(text);

                if (text.ToLower().Contains("costume_head_top")
                    || text.ToLower().Contains("costume_head_mid")
                    || text.ToLower().Contains("costume_head_low")
                    || text.ToLower().Contains("costume_garment")
                    || text.ToLower().Contains("shadow_armor")
                    || text.ToLower().Contains("shadow_weapon")
                    || text.ToLower().Contains("shadow_shield")
                    || text.ToLower().Contains("shadow_shoes")
                    || text.ToLower().Contains("shadow_right_accessory")
                    || text.ToLower().Contains("shadow_left_accessory"))
                    _itemListContainer.costumeIds.Add(_id);
            }
        }

        Debug.Log("There are " + _itemDatabases.Count + " item database.");
        Debug.Log("There are " + _itemListContainer.weaponIds.Count + " weapon database.");
        Debug.Log("There are " + _itemListContainer.equipmentIds.Count + " equipment database.");
        Debug.Log("There are " + _itemListContainer.costumeIds.Count + " costume database.");
        Debug.Log("There are " + _itemListContainer.cardIds.Count + " card database.");
        Debug.Log("There are " + _itemListContainer.enchantIds.Count + " enchant database.");
    }

    // Converting

    /// <summary>
    /// Start converting process
    /// </summary>
    void Convert()
    {
        if (File.Exists("itemInfo_Sak.lub"))
            File.Delete("itemInfo_Sak.lub");

        var path = Application.dataPath + "/Assets/item_db_equip.yml";
        var path2 = Application.dataPath + "/Assets/item_db_usable.yml";
        var path3 = Application.dataPath + "/Assets/item_db_etc.yml";
        var path4 = Application.dataPath + "/Assets/item_db_custom.txt";
        var path5 = Application.dataPath + "/Assets/item_db_test.txt";

        var itemDatabasesFile = File.ReadAllText(path) + "\n"
            + File.ReadAllText(path2) + "\n"
            + File.ReadAllText(path3) + "\n"
            + File.ReadAllText(path4);

        if (_isOnlyUseTestTextAsset
            && File.Exists(path5))
            itemDatabasesFile = File.ReadAllText(path5);

        if (_isOnlyUseCustomTextAsset)
            itemDatabasesFile = File.ReadAllText(path4);

        var itemDatabases = itemDatabasesFile.Split('\n');

        _itemContainer = new ItemContainer();

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < itemDatabases.Length; i++)
        {
            var text = CommentRemover.FixCommentSeperateLine(itemDatabases, i);

            var nextText = ((i + 1) < itemDatabases.Length) ? itemDatabases[i + 1] : string.Empty;

            var nextNextText = ((i + 2) < itemDatabases.Length) ? itemDatabases[i + 2] : string.Empty;

            // Skip
            if (text.Contains("    AegisName:")
                || text.Contains("    Sell:")
                || text.Contains("    Jobs:")
                || text.Contains("    Classes:")
                || text.Contains("    Locations:")
                || text.Contains("    AliasName:")
                || text.Contains("    Flags:")
                || text.Contains("    BuyingStore:")
                || text.Contains("    DeadBranch:")
                || text.Contains("    Container:")
                || text.Contains("    UniqueId:")
                || text.Contains("    BindOnEquip:")
                || text.Contains("    DropAnnounce:")
                || text.Contains("    NoConsume:")
                || text.Contains("    DropEffect:")
                || text.Contains("    Delay:")
                || text.Contains("    Duration:")
                || text.Contains("    Status:")
                || text.Contains("    Stack:")
                || text.Contains("    Amount:")
                || text.Contains("    Inventory:")
                || text.Contains("    Cart:")
                || text.Contains("    Storage:")
                || text.Contains("    GuildStorage:")
                || text.Contains("    NoUse:")
                || text.Contains("    Override:")
                || text.Contains("    Sitting:")
                || text.Contains("    Trade:")
                || text.Contains("    NoDrop:")
                || text.Contains("    NoTrade:")
                || text.Contains("    TradePartner:")
                || text.Contains("    NoSell:")
                || text.Contains("    NoCart:")
                || text.Contains("    NoStorage:")
                || text.Contains("    NoGuildStorage:")
                || text.Contains("    NoMail:")
                || text.Contains("    NoAuction:")
                || text.Contains("    Script:")
                || text.Contains("    OnEquip_Script:")
                || text.Contains("    OnUnequip_Script:"))
            {
                if (text.Contains("    Jobs:"))
                {
                    _itemContainer.isJob = true;
                    _itemContainer.isClass = false;
                    _itemContainer.isScript = false;
                    _itemContainer.isEquipScript = false;
                    _itemContainer.isUnequipScript = false;
                }
                else if (text.Contains("    Classes:"))
                {
                    _itemContainer.isJob = false;
                    _itemContainer.isClass = true;
                    _itemContainer.isScript = false;
                    _itemContainer.isEquipScript = false;
                    _itemContainer.isUnequipScript = false;
                }
                else if (text.Contains("    Script:"))
                {
                    _itemContainer.isJob = false;
                    _itemContainer.isClass = false;
                    _itemContainer.isScript = true;
                    _itemContainer.isEquipScript = false;
                    _itemContainer.isUnequipScript = false;
                }
                else if (text.Contains("    OnEquip_Script:"))
                {
                    _itemContainer.isJob = false;
                    _itemContainer.isClass = false;
                    _itemContainer.isScript = false;
                    _itemContainer.isEquipScript = true;
                    _itemContainer.isUnequipScript = false;
                }
                else if (text.Contains("    OnUnequip_Script:"))
                {
                    _itemContainer.isJob = false;
                    _itemContainer.isClass = false;
                    _itemContainer.isScript = false;
                    _itemContainer.isEquipScript = false;
                    _itemContainer.isUnequipScript = true;
                }

                text = string.Empty;
            }

            // Id
            if (text.Contains("  - Id:"))
            {
                text = SpacingRemover.Remove(text);

                _itemContainer.id = text.Replace("-Id:", string.Empty);
            }
            // Name
            else if (text.Contains("    Name:"))
            {
                _itemContainer.name = text.Replace("    Name: ", string.Empty);

                _itemContainer.name = QuoteRemover.Remove(_itemContainer.name);

                // Hotfix for →
                _itemContainer.name = _itemContainer.name.Replace("→", " to ");
            }
            // Type
            else if (text.Contains("    Type:"))
            {
                _itemContainer.type = text.Replace("    Type: ", string.Empty);

                if (!string.IsNullOrEmpty(_itemContainer.type))
                {
                    if ((_itemContainer.type.ToLower() == "petegg")
                        && !_itemListContainer.petEggIds.Contains(_itemContainer.id))
                        _itemListContainer.petEggIds.Add(_itemContainer.id);
                    else if ((_itemContainer.type.ToLower() == "petarmor")
                        && !_itemListContainer.petArmorIds.Contains(_itemContainer.id))
                        _itemListContainer.petArmorIds.Add(_itemContainer.id);
                }
            }
            // SubType
            else if (text.Contains("    SubType:"))
                _itemContainer.subType = text.Replace("    SubType: ", string.Empty);
            // Buy
            else if (text.Contains("    Buy:"))
            {
                text = text.Replace("    Buy: ", string.Empty);

                int buy = 0;

                if (int.TryParse(text, out buy))
                    _itemContainer.buy = buy.ToString("n0");
                else
                    _itemContainer.buy = TryParseInt(text);
            }
            // Weight
            else if (text.Contains("    Weight:"))
            {
                text = text.Replace("    Weight: ", string.Empty);

                _itemContainer.weight = TryParseInt(text, 10);
            }
            // Attack
            else if (text.Contains("    Attack:"))
                _itemContainer.attack = text.Replace("    Attack: ", string.Empty);
            // Magic Attack
            else if (text.Contains("    MagicAttack:"))
                _itemContainer.magicAttack = text.Replace("    MagicAttack: ", string.Empty);
            // Defense
            else if (text.Contains("    Defense:"))
                _itemContainer.defense = text.Replace("    Defense: ", string.Empty);
            // Range
            else if (text.Contains("    Range:"))
                _itemContainer.range = text.Replace("    Range: ", string.Empty);
            // Slots
            else if (text.Contains("    Slots:"))
                _itemContainer.slots = text.Replace("    Slots: ", string.Empty);
            // Jobs
            else if (_itemContainer.isJob && text.Contains("      All: true"))
                _itemContainer.jobs += _localization.GetTexts(Localization.JOBS_ALL_JOB) + ", ";
            else if (_itemContainer.isJob && text.Contains("      All: false"))
                _itemContainer.jobs += _localization.GetTexts(Localization.JOBS_ALL_JOB) + " [x], ";
            else if (_itemContainer.isJob && text.Contains("      Acolyte: true"))
                _itemContainer.jobs += "Acolyte, ";
            else if (_itemContainer.isJob && text.Contains("      Acolyte: false"))
                _itemContainer.jobs += "Acolyte [x], ";
            else if (_itemContainer.isJob && text.Contains("      Alchemist: true"))
                _itemContainer.jobs += "Alchemist, ";
            else if (_itemContainer.isJob && text.Contains("      Alchemist: false"))
                _itemContainer.jobs += "Alchemist [x], ";
            else if (_itemContainer.isJob && text.Contains("      Archer: true"))
                _itemContainer.jobs += "Archer, ";
            else if (_itemContainer.isJob && text.Contains("      Archer: false"))
                _itemContainer.jobs += "Archer [x], ";
            else if (_itemContainer.isJob && text.Contains("      Assassin: true"))
                _itemContainer.jobs += "Assassin, ";
            else if (_itemContainer.isJob && text.Contains("      Assassin: false"))
                _itemContainer.jobs += "Assassin [x], ";
            else if (_itemContainer.isJob && text.Contains("      BardDancer: true"))
                _itemContainer.jobs += "Bard & Dancer, ";
            else if (_itemContainer.isJob && text.Contains("      BardDancer: false"))
                _itemContainer.jobs += "Bard & Dancer [x], ";
            else if (_itemContainer.isJob && text.Contains("      Blacksmith: true"))
                _itemContainer.jobs += "Blacksmith, ";
            else if (_itemContainer.isJob && text.Contains("      Blacksmith: false"))
                _itemContainer.jobs += "Blacksmith [x], ";
            else if (_itemContainer.isJob && text.Contains("      Crusader: true"))
                _itemContainer.jobs += "Crusader, ";
            else if (_itemContainer.isJob && text.Contains("      Crusader: false"))
                _itemContainer.jobs += "Crusader [x], ";
            else if (_itemContainer.isJob && text.Contains("      Gunslinger: true"))
                _itemContainer.jobs += "Gunslinger, ";
            else if (_itemContainer.isJob && text.Contains("      Gunslinger: false"))
                _itemContainer.jobs += "Gunslinger [x], ";
            else if (_itemContainer.isJob && text.Contains("      Hunter: true"))
                _itemContainer.jobs += "Hunter, ";
            else if (_itemContainer.isJob && text.Contains("      Hunter: false"))
                _itemContainer.jobs += "Hunter [x], ";
            else if (_itemContainer.isJob && text.Contains("      KagerouOboro: true"))
                _itemContainer.jobs += "Kagerou & Oboro, ";
            else if (_itemContainer.isJob && text.Contains("      KagerouOboro: false"))
                _itemContainer.jobs += "Kagerou & Oboro [x], ";
            else if (_itemContainer.isJob && text.Contains("      Shinkiro: true"))
                _itemContainer.jobs += "Shinkiro, ";
            else if (_itemContainer.isJob && text.Contains("      Shinkiro: false"))
                _itemContainer.jobs += "Shinkiro [x], ";
            else if (_itemContainer.isJob && text.Contains("      Shiranui: true"))
                _itemContainer.jobs += "Shiranui, ";
            else if (_itemContainer.isJob && text.Contains("      Shiranui: false"))
                _itemContainer.jobs += "Shiranui [x], ";
            else if (_itemContainer.isJob && text.Contains("      Knight: true"))
                _itemContainer.jobs += "Knight, ";
            else if (_itemContainer.isJob && text.Contains("      Knight: false"))
                _itemContainer.jobs += "Knight [x], ";
            else if (_itemContainer.isJob && text.Contains("      Mage: true"))
                _itemContainer.jobs += "Mage, ";
            else if (_itemContainer.isJob && text.Contains("      Mage: false"))
                _itemContainer.jobs += "Mage [x], ";
            else if (_itemContainer.isJob && text.Contains("      Merchant: true"))
                _itemContainer.jobs += "Merchant, ";
            else if (_itemContainer.isJob && text.Contains("      Merchant: false"))
                _itemContainer.jobs += "Merchant [x], ";
            else if (_itemContainer.isJob && text.Contains("      Monk: true"))
                _itemContainer.jobs += "Monk, ";
            else if (_itemContainer.isJob && text.Contains("      Monk: false"))
                _itemContainer.jobs += "Monk [x], ";
            else if (_itemContainer.isJob && text.Contains("      Ninja: true"))
                _itemContainer.jobs += "Ninja, ";
            else if (_itemContainer.isJob && text.Contains("      Ninja: false"))
                _itemContainer.jobs += "Ninja [x], ";
            else if (_itemContainer.isJob && text.Contains("      Novice: true"))
                _itemContainer.jobs += "Novice, ";
            else if (_itemContainer.isJob && text.Contains("      Novice: false"))
                _itemContainer.jobs += "Novice [x], ";
            else if (_itemContainer.isJob && text.Contains("      Priest: true"))
                _itemContainer.jobs += "Priest, ";
            else if (_itemContainer.isJob && text.Contains("      Priest: false"))
                _itemContainer.jobs += "Priest [x], ";
            else if (_itemContainer.isJob && text.Contains("      Rebellion: true"))
                _itemContainer.jobs += "Rebellion, ";
            else if (_itemContainer.isJob && text.Contains("      Rebellion: false"))
                _itemContainer.jobs += "Rebellion [x], ";
            else if (_itemContainer.isJob && text.Contains("      Night_Watch: true"))
                _itemContainer.jobs += "Night Watch, ";
            else if (_itemContainer.isJob && text.Contains("      Night_Watch: false"))
                _itemContainer.jobs += "Night Watch [x], ";
            else if (_itemContainer.isJob && text.Contains("      Rogue: true"))
                _itemContainer.jobs += "Rogue, ";
            else if (_itemContainer.isJob && text.Contains("      Rogue: false"))
                _itemContainer.jobs += "Rogue [x], ";
            else if (_itemContainer.isJob && text.Contains("      Sage: true"))
                _itemContainer.jobs += "Sage, ";
            else if (_itemContainer.isJob && text.Contains("      Sage: false"))
                _itemContainer.jobs += "Sage [x], ";
            else if (_itemContainer.isJob && text.Contains("      SoulLinker: true"))
                _itemContainer.jobs += "Soul Linker, ";
            else if (_itemContainer.isJob && text.Contains("      SoulLinker: false"))
                _itemContainer.jobs += "Soul Linker [x], ";
            else if (_itemContainer.isJob && text.Contains("      Soul_Ascetic: true"))
                _itemContainer.jobs += "Soul Ascetic, ";
            else if (_itemContainer.isJob && text.Contains("      Soul_Ascetic: false"))
                _itemContainer.jobs += "Soul Ascetic [x], ";
            else if (_itemContainer.isJob && text.Contains("      StarGladiator: true"))
                _itemContainer.jobs += "Star Gladiator, ";
            else if (_itemContainer.isJob && text.Contains("      StarGladiator: false"))
                _itemContainer.jobs += "Star Gladiator [x], ";
            else if (_itemContainer.isJob && text.Contains("      Sky_Emperor: true"))
                _itemContainer.jobs += "Sky Emperor, ";
            else if (_itemContainer.isJob && text.Contains("      Sky_Emperor: false"))
                _itemContainer.jobs += "Sky Emperor [x], ";
            else if (_itemContainer.isJob && text.Contains("      Summoner: true"))
                _itemContainer.jobs += "Summoner, ";
            else if (_itemContainer.isJob && text.Contains("      Summoner: false"))
                _itemContainer.jobs += "Summoner [x], ";
            else if (_itemContainer.isJob && text.Contains("      Spirit_Handler: true"))
                _itemContainer.jobs += "Spirit Handler, ";
            else if (_itemContainer.isJob && text.Contains("      Spirit_Handler: false"))
                _itemContainer.jobs += "Spirit Handler [x], ";
            else if (_itemContainer.isJob && text.Contains("      SuperNovice: true"))
                _itemContainer.jobs += "Super Novice, ";
            else if (_itemContainer.isJob && text.Contains("      SuperNovice: false"))
                _itemContainer.jobs += "Super Novice [x], ";
            else if (_itemContainer.isJob && text.Contains("      Hyper_Novice: true"))
                _itemContainer.jobs += "Hyper Novice, ";
            else if (_itemContainer.isJob && text.Contains("      Hyper_Novice: false"))
                _itemContainer.jobs += "Hyper Novice [x], ";
            else if (_itemContainer.isJob && text.Contains("      Swordman: true"))
                _itemContainer.jobs += "Swordman, ";
            else if (_itemContainer.isJob && text.Contains("      Swordman: false"))
                _itemContainer.jobs += "Swordman [x], ";
            else if (_itemContainer.isJob && text.Contains("      Taekwon: true"))
                _itemContainer.jobs += "Taekwon, ";
            else if (_itemContainer.isJob && text.Contains("      Taekwon: false"))
                _itemContainer.jobs += "Taekwon [x], ";
            else if (_itemContainer.isJob && text.Contains("      Thief: true"))
                _itemContainer.jobs += "Thief, ";
            else if (_itemContainer.isJob && text.Contains("      Thief: false"))
                _itemContainer.jobs += "Thief [x], ";
            else if (_itemContainer.isJob && text.Contains("      Wizard: true"))
                _itemContainer.jobs += "Wizard, ";
            else if (_itemContainer.isJob && text.Contains("      Wizard: false"))
                _itemContainer.jobs += "Wizard [x], ";
            // Classes
            else if (_itemContainer.isClass && text.Contains("      All: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASSES_ALL_CLASS) + ", ";
            else if (_itemContainer.isClass && text.Contains("      All: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASSES_ALL_CLASS) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      Normal: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 1, ";
            else if (_itemContainer.isClass && text.Contains("      Normal: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 1 [x], ";
            else if (_itemContainer.isClass && text.Contains("      Upper: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 2, ";
            else if (_itemContainer.isClass && text.Contains("      Upper: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 2 [x], ";
            else if (_itemContainer.isClass && text.Contains("      Baby: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 1 " + _localization.GetTexts(Localization.OR) + " 2 " + _localization.GetTexts(Localization.CLASSES_BABY) + ", ";
            else if (_itemContainer.isClass && text.Contains("      Baby: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 1 " + _localization.GetTexts(Localization.OR) + " 2 " + _localization.GetTexts(Localization.CLASSES_BABY) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      Third: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3, ";
            else if (_itemContainer.isClass && text.Contains("      Third: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 [x], ";
            else if (_itemContainer.isClass && text.Contains("      Third_Upper: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_TRANS) + ", ";
            else if (_itemContainer.isClass && text.Contains("      Third_Upper: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_TRANS) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      Third_Baby: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_BABY) + ", ";
            else if (_itemContainer.isClass && text.Contains("      Third_Baby: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_BABY) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      All_Upper: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 2 " + _localization.GetTexts(Localization.OR) + _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_TRANS) + ", ";
            else if (_itemContainer.isClass && text.Contains("      All_Upper: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 2 " + _localization.GetTexts(Localization.OR) + _localization.GetTexts(Localization.CLASS) + " 3 " + _localization.GetTexts(Localization.CLASSES_TRANS) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      All_Baby: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " " + _localization.GetTexts(Localization.CLASSES_BABY) + ", ";
            else if (_itemContainer.isClass && text.Contains("      All_Baby: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " " + _localization.GetTexts(Localization.CLASSES_BABY) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      All_Third: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3, ";
            else if (_itemContainer.isClass && text.Contains("      All_Third: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 3 [x], ";
            else if (_itemContainer.isClass && text.Contains("      Fourth: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4, ";
            else if (_itemContainer.isClass && text.Contains("      Fourth: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4 [x], ";
            else if (_itemContainer.isClass && text.Contains("      Fourth_Baby: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4 " + _localization.GetTexts(Localization.CLASSES_BABY) + ", ";
            else if (_itemContainer.isClass && text.Contains("      Fourth_Baby: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4 " + _localization.GetTexts(Localization.CLASSES_BABY) + " [x], ";
            else if (_itemContainer.isClass && text.Contains("      All_Fourth: true"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4, ";
            else if (_itemContainer.isClass && text.Contains("      All_Fourth: false"))
                _itemContainer.classes += _localization.GetTexts(Localization.CLASS) + " 4 [x], ";
            // Gender
            else if (text.Contains("      Female: true"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_FEMALE) + ", ";
            else if (text.Contains("      Female: false"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_FEMALE) + " [x], ";
            else if (text.Contains("      Male: true"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_MALE) + ", ";
            else if (text.Contains("      Male: false"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_MALE) + " [x], ";
            else if (text.Contains("      Both: true"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_ALL) + ", ";
            else if (text.Contains("      Both: false"))
                _itemContainer.gender += _localization.GetTexts(Localization.GENDER_ALL) + " [x], ";
            // Location
            else if (text.Contains("      Head_Top: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_TOP) + ", ";
            else if (text.Contains("      Head_Top: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_TOP) + " [x], ";
            else if (text.Contains("      Head_Mid: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_MID) + ", ";
            else if (text.Contains("      Head_Mid: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_MID) + " [x], ";
            else if (text.Contains("      Head_Low: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_LOW) + ", ";
            else if (text.Contains("      Head_Low: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_HEAD_LOW) + " [x], ";
            else if (text.Contains("      Armor: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_ARMOR) + ", ";
            else if (text.Contains("      Armor: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_ARMOR) + " [x], ";
            else if (text.Contains("      Right_Hand: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_RIGHT_HAND) + ", ";
            else if (text.Contains("      Right_Hand: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_RIGHT_HAND) + " [x], ";
            else if (text.Contains("      Left_Hand: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_LEFT_HAND) + ", ";
            else if (text.Contains("      Left_Hand: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_LEFT_HAND) + " [x], ";
            else if (text.Contains("      Garment: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_GARMENT) + ", ";
            else if (text.Contains("      Garment: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_GARMENT) + " [x], ";
            else if (text.Contains("      Shoes: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHOES) + ", ";
            else if (text.Contains("      Shoes: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHOES) + " [x], ";
            else if (text.Contains("      Right_Accessory: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_RIGHT_ACCESSORY) + ", ";
            else if (text.Contains("      Right_Accessory: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_RIGHT_ACCESSORY) + " [x], ";
            else if (text.Contains("      Left_Accessory: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_LEFT_ACCESSORY) + ", ";
            else if (text.Contains("      Left_Accessory: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_LEFT_ACCESSORY) + " [x], ";
            else if (text.Contains("      Costume_Head_Top: true"))
            {
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_TOP) + ", ";

                if (!_itemListContainer.fashionCostumeIds.Contains(_itemContainer.id))
                    _itemListContainer.fashionCostumeIds.Add(_itemContainer.id);
            }
            else if (text.Contains("      Costume_Head_Top: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_TOP) + " [x], ";
            else if (text.Contains("      Costume_Head_Mid: true"))
            {
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_MID) + ", ";

                if (!_itemListContainer.fashionCostumeIds.Contains(_itemContainer.id))
                    _itemListContainer.fashionCostumeIds.Add(_itemContainer.id);
            }
            else if (text.Contains("      Costume_Head_Mid: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_MID) + " [x], ";
            else if (text.Contains("      Costume_Head_Low: true"))
            {
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_LOW) + ", ";

                if (!_itemListContainer.fashionCostumeIds.Contains(_itemContainer.id))
                    _itemListContainer.fashionCostumeIds.Add(_itemContainer.id);
            }
            else if (text.Contains("      Costume_Head_Low: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_LOW) + " [x], ";
            else if (text.Contains("      Costume_Garment: true"))
            {
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_GARMENT) + ", ";

                if (!_itemListContainer.fashionCostumeIds.Contains(_itemContainer.id))
                    _itemListContainer.fashionCostumeIds.Add(_itemContainer.id);
            }
            else if (text.Contains("      Costume_Garment: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_COSTUME_GARMENT) + " [x], ";
            else if (text.Contains("      Ammo: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_AMMO) + ", ";
            else if (text.Contains("      Ammo: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_AMMO) + " [x], ";
            else if (text.Contains("      Shadow_Armor: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_ARMOR) + ", ";
            else if (text.Contains("      Shadow_Armor: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_ARMOR) + " [x], ";
            else if (text.Contains("      Shadow_Weapon: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_WEAPON) + ", ";
            else if (text.Contains("      Shadow_Weapon: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_WEAPON) + " [x], ";
            else if (text.Contains("      Shadow_Shield: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_SHIELD) + ", ";
            else if (text.Contains("      Shadow_Shield: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_SHIELD) + " [x], ";
            else if (text.Contains("      Shadow_Shoes: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_SHOES) + ", ";
            else if (text.Contains("      Shadow_Shoes: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_SHOES) + " [x], ";
            else if (text.Contains("      Shadow_Right_Accessory: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_RIGHT_ACCESSORY) + ", ";
            else if (text.Contains("      Shadow_Right_Accessory: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_RIGHT_ACCESSORY) + " [x], ";
            else if (text.Contains("      Shadow_Left_Accessory: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_LEFT_ACCESSORY) + ", ";
            else if (text.Contains("      Shadow_Left_Accessory: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_SHADOW_LEFT_ACCESSORY) + " [x], ";
            else if (text.Contains("      Both_Hand: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_BOTH_HAND) + ", ";
            else if (text.Contains("      Both_Hand: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_BOTH_HAND) + " [x], ";
            else if (text.Contains("      Both_Accessory: true"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_BOTH_ACCESSORY) + ", ";
            else if (text.Contains("      Both_Accessory: false"))
                _itemContainer.locations += _localization.GetTexts(Localization.LOCATION_BOTH_ACCESSORY) + " [x], ";
            // Weapon Level
            else if (text.Contains("    WeaponLevel:"))
                _itemContainer.weaponLevel = text.Replace("    WeaponLevel: ", string.Empty);
            // Armor Level
            else if (text.Contains("    ArmorLevel:"))
                _itemContainer.armorLevel = text.Replace("    ArmorLevel: ", string.Empty);
            // Equip Level Min
            else if (text.Contains("    EquipLevelMin:"))
                _itemContainer.equipLevelMinimum = text.Replace("    EquipLevelMin: ", string.Empty);
            // Equip Level Max
            else if (text.Contains("    EquipLevelMax:"))
                _itemContainer.equipLevelMaximum = text.Replace("    EquipLevelMax: ", string.Empty);
            // Refineable
            else if (text.Contains("    Refineable: true"))
                _itemContainer.refinable = _localization.GetTexts(Localization.CAN);
            else if (text.Contains("    Refineable: false"))
                _itemContainer.refinable = _localization.GetTexts(Localization.CANNOT);
            // View
            else if (text.Contains("    View:"))
            {
                _itemContainer.view = text.Replace("    View: ", string.Empty);

                if (!_classNumberDatabases.ContainsKey(int.Parse(_itemContainer.id)))
                    _classNumberDatabases.Add(int.Parse(_itemContainer.id), _itemContainer.view);
                else
                    _classNumberDatabases[int.Parse(_itemContainer.id)] = _itemContainer.view;
            }
            // Script
            else if (_itemContainer.isScript)
            {
                var script = ConvertItemScripts(text);

                if (!string.IsNullOrEmpty(script))
                    _itemContainer.script += "			\"" + script + "\",\n";
            }
            // Equip Script
            else if (_itemContainer.isEquipScript)
            {
                var equipScript = ConvertItemScripts(text);

                if (!string.IsNullOrEmpty(equipScript))
                    _itemContainer.equipScript += "			\"" + equipScript + "\",\n";
            }
            // Unequip Script
            else if (_itemContainer.isUnequipScript)
            {
                var unequipScript = ConvertItemScripts(text);

                if (!string.IsNullOrEmpty(unequipScript))
                    _itemContainer.unequipScript += "			\"" + unequipScript + "\",\n";
            }

            // Write builder now
            if (nextText.Contains("- Id:")
                && !string.IsNullOrEmpty(_itemContainer.id)
                && !string.IsNullOrWhiteSpace(_itemContainer.id)
                || ((i + 1) >= itemDatabases.Length))
            {
                var resourceName = GetResourceNameFromId(int.Parse(_itemContainer.id)
                    , _itemContainer.type
                    , _itemContainer.subType
                    , !string.IsNullOrEmpty(_itemContainer.locations) ? _itemContainer.locations.Substring(0, _itemContainer.locations.Length - 2) : string.Empty);

                // Id
                builder.Append("	[" + _itemContainer.id + "] = {\n");
                // Unidentified display name
                builder.Append("		unidentifiedDisplayName = \"" + _itemContainer.name
                    +
                    (((_itemContainer.type.ToLower() == "weapon")
                    || (_itemContainer.type.ToLower() == "armor")
                    || (_itemContainer.type.ToLower() == "shadowgear"))
                    ? " [" + (!string.IsNullOrEmpty(_itemContainer.slots) ? _itemContainer.slots : "0") + "]"
                    : string.Empty) + "\",\n");
                // Unidentified resource name
                builder.Append("		unidentifiedResourceName = " + resourceName + ",\n");
                // Unidentified description
                builder.Append("		unidentifiedDescriptionName = {\n");
                builder.Append("			\"\"\n");
                builder.Append("		},\n");
                // Identified display name
                builder.Append("		identifiedDisplayName = \"" + _itemContainer.name + "\",\n");
                // Identified resource name
                builder.Append("		identifiedResourceName = " + resourceName + ",\n");
                // Identified description
                builder.Append("		identifiedDescriptionName = {\n");
                // Description
                var comboBonuses = GetCombo(GetItemDatabase(int.Parse(_itemContainer.id)).aegisName);

                string hardcodeBonuses = _hardcodeItemScripts.GetHardcodeItemScript(int.Parse(_itemContainer.id));

                var bonuses = !string.IsNullOrEmpty(hardcodeBonuses)
                    ? hardcodeBonuses
                    : !string.IsNullOrEmpty(_itemContainer.script)
                    ? _itemContainer.script
                    : string.Empty;

                var equipBonuses = !string.IsNullOrEmpty(_itemContainer.equipScript)
                    ? "			\"^666478[" + _localization.GetTexts(Localization.WHEN_EQUIP) + "]^000000\",\n"
                    + _itemContainer.equipScript
                    : string.Empty;

                var unequipBonuses = !string.IsNullOrEmpty(_itemContainer.unequipScript)
                    ? "			\"^666478[" + _localization.GetTexts(Localization.WHEN_UNEQUIP) + "]^000000\",\n"
                    + _itemContainer.unequipScript
                    : string.Empty;

                var description = "			\"^3F28FFID:^000000 " + _itemContainer.id + "\",\n"
                    + "			\"^3F28FF" + _localization.GetTexts(Localization.TYPE) + ":^000000 " + _itemContainer.type + "\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.subType))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.SUB_TYPE) + ":^000000 " + _itemContainer.subType + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.SUB_TYPE) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.locations))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.LOCATION) + ":^000000 " + _itemContainer.locations.Substring(0, _itemContainer.locations.Length - 2) + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.LOCATION) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.jobs))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.JOB) + ":^000000 " + _itemContainer.jobs.Substring(0, _itemContainer.jobs.Length - 2) + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.JOB) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.classes))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.CLASS) + ":^000000 " + _itemContainer.classes.Substring(0, _itemContainer.classes.Length - 2) + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.CLASS) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.gender))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.GENDER) + ":^000000 " + _itemContainer.gender.Substring(0, _itemContainer.gender.Length - 2) + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.GENDER) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.attack))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.ATTACK) + ":^000000 " + _itemContainer.attack + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.ATTACK) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.magicAttack))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MAGIC_ATTACK) + ":^000000 " + _itemContainer.magicAttack + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MAGIC_ATTACK) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.defense))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.DEFENSE) + ":^000000 " + _itemContainer.defense + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.DEFENSE) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.range))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.RANGE) + ":^000000 " + _itemContainer.range + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.RANGE) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.weaponLevel))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.WEAPON_LEVEL) + ":^000000 " + _itemContainer.weaponLevel + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.WEAPON_LEVEL) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.armorLevel))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.ARMOR_LEVEL) + ":^000000 " + _itemContainer.armorLevel + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.ARMOR_LEVEL) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.equipLevelMinimum))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MINIMUM_EQUIP_LEVEL) + ":^000000 " + _itemContainer.equipLevelMinimum + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MINIMUM_EQUIP_LEVEL) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.equipLevelMaximum))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MAXIMUM_EQUIP_LEVEL) + ":^000000 " + _itemContainer.equipLevelMaximum + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.MAXIMUM_EQUIP_LEVEL) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.refinable))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.REFINABLE) + ":^000000 " + _itemContainer.refinable + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.REFINABLE) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.weight))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.WEIGHT) + ":^000000 " + _itemContainer.weight + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.WEIGHT) + ":^000000 -\",\n";

                if (!string.IsNullOrEmpty(_itemContainer.buy))
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.PRICE) + ":^000000 " + _itemContainer.buy + "\",\n";
                else if (_isZeroValuePrintable)
                    description += "			\"^3F28FF" + _localization.GetTexts(Localization.PRICE) + ":^000000 -\",\n";

                builder.Append(bonuses);

                if (!string.IsNullOrEmpty(bonuses)
                    && !string.IsNullOrWhiteSpace(bonuses))
                    builder.Append("			\"————————————\",\n");

                builder.Append(comboBonuses);

                builder.Append(equipBonuses);

                builder.Append(unequipBonuses);

                builder.Append(description);

                builder.Append("			\"\"\n");

                builder.Append("		},\n");

                // Slot Count
                if (!string.IsNullOrEmpty(_itemContainer.slots))
                    builder.Append("		slotCount = " + _itemContainer.slots + ",\n");
                else
                    builder.Append("		slotCount = 0,\n");

                // View / Class Number
                builder.Append("		ClassNum = " + GetClassNumFromId(_itemContainer) + ",\n");

                // Costume
                builder.Append("		costume = " + IsCostumeFromId(_itemContainer) + "\n");

                if (string.IsNullOrEmpty(nextNextText)
                    || string.IsNullOrWhiteSpace(nextNextText))
                    builder.Append("	}\n");
                else
                    builder.Append("	},\n");

                _itemContainer = new ItemContainer();
            }
        }

        var prefix = "tbl = {\n";
        var postfix = "}\n\n" +
            "-- Function #0\n" +
            "main = function()\n" +
            "	for ItemID, DESC in pairs(tbl) do\n" +
            "		result, msg = AddItem(ItemID, DESC.unidentifiedDisplayName, DESC.unidentifiedResourceName, DESC.identifiedDisplayName, DESC.identifiedResourceName, DESC.slotCount, DESC.ClassNum)\n" +
            "		if not result == true then\n" +
            "			return false, msg\n" +
            "		end\n" +
            "		for k, v in pairs(DESC.unidentifiedDescriptionName) do\n" +
            "			result, msg = AddItemUnidentifiedDesc(ItemID, v)\n" +
            "			if not result == true then\n" +
            "				return false, msg\n" +
            "			end\n" +
            "		end\n" +
            "		for k, v in pairs(DESC.identifiedDescriptionName) do\n" +
            "			result, msg = AddItemIdentifiedDesc(ItemID, v)\n" +
            "			if not result == true then\n" +
            "				return false, msg\n" +
            "			end\n" +
            "		end\n" +
            "		if nil ~= DESC.EffectID then\n" +
            "			result, msg = AddItemEffectInfo(ItemID, DESC.EffectID)\n" +
            "			if not result == true then\n" +
            "				return false, msg\n" +
            "			end\n" +
            "		end\n" +
            "		if nil ~= DESC.costume then\n" +
            "			result, msg = AddItemIsCostume(ItemID, DESC.costume)\n" +
            "			if not result == true then\n" +
            "				return false, msg\n" +
            "			end\n" +
            "		end\n" +
            "		k = DESC.unidentifiedResourceName\n" +
            "		v = DESC.identifiedDisplayName\n" +
            "	end\n" +
            "	return true, \"good\"\n" +
            "end\n" +
            "\n" +
            "-- Function #1\n" +
            "main_server = function()\n" +
            "	for ItemID, DESC in pairs(tbl) do\n" +
            "		result, msg = AddItem(ItemID, DESC.identifiedDisplayName, DESC.slotCount)\n" +
            "		if not result == true then\n" +
            "			return false, msg\n" +
            "		end\n" +
            "	end\n" +
            "	return true, \"good\"\n" +
            "end\n";

        string finalize = prefix + builder.ToString() + postfix;

        // New line
        finalize = finalize.Replace("[NEW_LINE]", "\",\n			\"");

        // TODO: Fix it properly
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " 11)", _localization.GetTexts(Localization.TYPE) + ")");
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " 11 )", _localization.GetTexts(Localization.TYPE) + ")");
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " II_VIEW)", _localization.GetTexts(Localization.TYPE) + ")");
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " II_VIEW )", _localization.GetTexts(Localization.TYPE) + ")");
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " ITEMINFO_VIEW)", _localization.GetTexts(Localization.TYPE) + ")");
        finalize = finalize.Replace(_localization.GetTexts(Localization.WITH) + " ITEMINFO_VIEW )", _localization.GetTexts(Localization.TYPE) + ")");

        // Spacing fix
        finalize = finalize.Replace("     ๐", "๐");
        finalize = finalize.Replace("    ๐", "๐");
        finalize = finalize.Replace("   ๐", "๐");
        finalize = finalize.Replace("  ๐", "๐");
        finalize = finalize.Replace(" ๐", "๐");

        if (_localization.IsUsingANSI)
            finalize = finalize.Replace("๐", "^5D2799-^000000");
        if (!_localization.IsUsingUTF8)
            finalize = finalize.Replace("—", "-");

        // Write it out
        File.WriteAllText("itemInfo_Sak.lub", finalize, _localization.GetCurrentEncoding);

        Debug.Log("'itemInfo_Sak.lub' has been successfully created.");

        ExportItemLists();

        ExportItemMall();

        Debug.Log(DateTime.Now);

        _txtConvertProgression.text = _localization.GetTexts(Localization.CONVERT_PROGRESSION_DONE) + "!!";
    }

    /// <summary>
    /// Convert item scripts
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    string ConvertItemScripts(string text)
    {
        // Comment fix
        int commentFixRetry = 300;
        while (text.Contains("/*"))
        {
            var copier = text;
            if (!copier.Contains("*/"))
                text = copier.Substring(0, copier.IndexOf("/*"));
            else
                text = copier.Substring(0, copier.IndexOf("/*")) + copier.Substring(copier.IndexOf("*/") + 2);

            commentFixRetry--;

            if (commentFixRetry <= 0)
                break;
        }

        commentFixRetry = 30;
        while (text.Contains("*/"))
        {
            text = text.Replace("*/", string.Empty);

            commentFixRetry--;

            if (commentFixRetry <= 0)
                break;
        }

        // Wrong wording fix
        text = text.Replace("bPAtk", "bPatk");
        text = text.Replace("Ele_dark", "Ele_Dark");
        text = text.Replace("bonus2 bIgnoreMDefRaceRate", "bonus2 bIgnoreMdefRaceRate");
        text = text.Replace("bVariableCastRate", "bVariableCastrate");
        text = text.Replace("bMaxHPRate", "bMaxHPrate");
        text = text.Replace("bMaxSPRate", "bMaxSPrate");
        text = text.Replace("bHPRecovRate", "bHPrecovRate");
        text = text.Replace("Baselevel", "BaseLevel");
        text = text.Replace("buseSPRate", "bUseSPrate");
        // End wrong wording fix

        // Comma fix
        int commaFixRetry = 300;
        while (text.Contains("(") && text.Contains(")") && commaFixRetry > 0)
        {
            commaFixRetry--;

            int commaOpenCount = 0;
            int commaCloseCount = 0;
            int currentCommaFixerStartIndex = text.IndexOf('(');

            //Debug.Log("text#1:" + text);

            for (int i = currentCommaFixerStartIndex; i < text.Length; i++)
            {
                if (text[i] == '(')
                    commaOpenCount++;
                else if (text[i] == ')')
                {
                    commaCloseCount++;

                    if (commaCloseCount == commaOpenCount)
                    {
                        var testCommaFixer = text.Substring(currentCommaFixerStartIndex, i - currentCommaFixerStartIndex + 1);
                        var testCommaFixer2 = text.Substring(currentCommaFixerStartIndex, i - currentCommaFixerStartIndex + 1);
                        testCommaFixer2 = testCommaFixer2.Replace('(', '†');
                        testCommaFixer2 = testCommaFixer2.Replace(')', '‡');
                        testCommaFixer2 = testCommaFixer2.Replace(",", " " + _localization.GetTexts(Localization.WITH) + " ");
                        text = text.Replace(testCommaFixer, testCommaFixer2);

                        break;
                    }
                }
            }

            //Debug.Log("text#2:" + text);
        }

        text = text.Replace('†', '(');
        text = text.Replace('‡', ')');

        //Debug.Log("text#3:" + text);

        // End comma fix

        text = text.Replace("      ", string.Empty);

        // autobonus3
        if (text.Contains("autobonus3 \"{"))
        {
            string duplicate = string.Empty;

            var temp = text.Replace("autobonus3 \"{", string.Empty);
            if (temp.IndexOf("}\"") > 0)
            {
                duplicate = temp.Substring(temp.IndexOf("}\""));

                temp = temp.Substring(0, temp.IndexOf("}\""));

                text = text.Replace(temp + "}\"", string.Empty);
            }
            var duplicates = duplicate.Split(',');
            var bonuses = string.Empty;
            var allBonus = temp.Split(';');
            for (int i = 0; i < allBonus.Length; i++)
            {
                if (!string.IsNullOrEmpty(allBonus[i]) && !string.IsNullOrWhiteSpace(allBonus[i]))
                {
                    text = text.Replace(allBonus[i] + ";", string.Empty);

                    bonuses += ConvertItemScripts(allBonus[i]);
                }
            }

            // Find first "{ for other script
            if (duplicate.IndexOf("\"{") > 0)
            {
                duplicate = duplicate.Substring(duplicate.IndexOf("\"{") + 2);
                temp = duplicate.Substring(0, duplicate.IndexOf("}\""));

                while (!string.IsNullOrEmpty(temp))
                {
                    if (temp.IndexOf(';') > 0)
                    {
                        var sumBonus = temp.Substring(0, temp.IndexOf(';'));
                        bonuses += ConvertItemScripts(sumBonus);
                        if (temp.Length > temp.IndexOf(';') + 1)
                            temp = temp.Substring(temp.IndexOf(';') + 1);
                        else
                            temp = string.Empty;
                    }
                    else
                        temp = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(bonuses) || !string.IsNullOrWhiteSpace(bonuses))
            {
                bonuses = bonuses.Replace("๐", "[NEW_LINE]๐");
                bonuses = bonuses.Replace("^FF2525", "[NEW_LINE]^FF2525");

                int number = 1;
                while (bonuses.Contains("๐"))
                {
                    bonuses = ReplaceOneTime.ReplaceNow(bonuses, "๐", number.ToString("f0") + ".)");

                    number++;
                }

                var skillName = QuoteRemover.Remove(duplicates.Length >= 4 ? duplicates[3] : string.Empty);
                var duration = QuoteRemover.Remove(duplicates.Length >= 3 ? duplicates[2] : string.Empty);
                var rate = QuoteRemover.Remove(duplicates.Length >= 2 ? duplicates[1] : string.Empty);
                text = string.Format(_localization.GetTexts(Localization.AUTO_BONUS_3), bonuses, GetSkillName(skillName), TryParseInt(rate, 10), TryParseInt(duration, 1000));
            }
            else
                text = string.Empty;
        }
        // autobonus2
        if (text.Contains("autobonus2 \"{"))
        {
            var duplicate = string.Empty;

            var temp = text.Replace("autobonus2 \"{", string.Empty);
            var flag = GetAllAtf(temp);
            if (temp.IndexOf("}\"") > 0)
            {
                duplicate = temp.Substring(temp.IndexOf("}\"") + 2);

                temp = temp.Substring(0, temp.IndexOf("}\""));

                text = text.Replace(temp + "}\"", string.Empty);
            }
            var duplicates = duplicate.Split(',');
            var bonuses = string.Empty;
            var allBonus = temp.Split(';');
            for (int i = 0; i < allBonus.Length; i++)
            {
                if (!string.IsNullOrEmpty(allBonus[i]) && !string.IsNullOrWhiteSpace(allBonus[i]))
                {
                    text = text.Replace(allBonus[i] + ";", string.Empty);

                    bonuses += ConvertItemScripts(allBonus[i]);
                }
            }

            // Find first "{ for other script
            if (duplicate.IndexOf("\"{") > 0)
            {
                duplicate = duplicate.Substring(duplicate.IndexOf("\"{") + 2);
                temp = duplicate.Substring(0, duplicate.IndexOf("}\""));

                while (!string.IsNullOrEmpty(temp))
                {
                    if (temp.IndexOf(';') > 0)
                    {
                        var sumBonus = temp.Substring(0, temp.IndexOf(';'));
                        bonuses += ConvertItemScripts(sumBonus);
                        if (temp.Length > temp.IndexOf(';') + 1)
                            temp = temp.Substring(temp.IndexOf(';') + 1);
                        else
                            temp = string.Empty;
                    }
                    else
                        temp = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(bonuses) || !string.IsNullOrWhiteSpace(bonuses))
            {
                bonuses = bonuses.Replace("๐", "[NEW_LINE]๐");
                bonuses = bonuses.Replace("^FF2525", "[NEW_LINE]^FF2525");

                int number = 1;
                while (bonuses.Contains("๐"))
                {
                    bonuses = ReplaceOneTime.ReplaceNow(bonuses, "๐", number.ToString("f0") + ".)");

                    number++;
                }

                var duration = QuoteRemover.Remove(duplicates.Length >= 3 ? duplicates[2] : string.Empty);
                var rate = QuoteRemover.Remove(duplicates.Length >= 2 ? duplicates[1] : string.Empty);
                text = string.Format(_localization.GetTexts(Localization.AUTO_BONUS_2), bonuses, flag, TryParseInt(rate, 10), TryParseInt(duration, 1000));
            }
            else
                text = string.Empty;
        }
        // autobonus
        if (text.Contains("autobonus \"{"))
        {
            var duplicate = string.Empty;

            var temp = text.Replace("autobonus \"{", string.Empty);
            var flag = GetAllAtf(temp);
            if (temp.IndexOf("}\"") > 0)
            {
                duplicate = temp.Substring(temp.IndexOf("}\"") + 2);

                temp = temp.Substring(0, temp.IndexOf("}\""));

                text = text.Replace(temp + "}\"", string.Empty);
            }
            var duplicates = duplicate.Split(',');
            var bonuses = string.Empty;
            var allBonus = temp.Split(';');
            for (int i = 0; i < allBonus.Length; i++)
            {
                if (!string.IsNullOrEmpty(allBonus[i]) && !string.IsNullOrWhiteSpace(allBonus[i]))
                {
                    text = text.Replace(allBonus[i] + ";", string.Empty);

                    bonuses += ConvertItemScripts(allBonus[i]);
                }
            }

            // Find first "{ for other script
            if (duplicate.IndexOf("\"{") > 0)
            {
                duplicate = duplicate.Substring(duplicate.IndexOf("\"{") + 2);
                temp = duplicate.Substring(0, duplicate.IndexOf("}\""));

                while (!string.IsNullOrEmpty(temp))
                {
                    if (temp.IndexOf(';') > 0)
                    {
                        var sumBonus = temp.Substring(0, temp.IndexOf(';'));
                        bonuses += ConvertItemScripts(sumBonus);
                        if (temp.Length > temp.IndexOf(';') + 1)
                            temp = temp.Substring(temp.IndexOf(';') + 1);
                        else
                            temp = string.Empty;
                    }
                    else
                        temp = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(bonuses) || !string.IsNullOrWhiteSpace(bonuses))
            {
                bonuses = bonuses.Replace("๐", "[NEW_LINE]๐");
                bonuses = bonuses.Replace("^FF2525", "[NEW_LINE]^FF2525");

                int number = 1;
                while (bonuses.Contains("๐"))
                {
                    bonuses = ReplaceOneTime.ReplaceNow(bonuses, "๐", number.ToString("f0") + ".)");

                    number++;
                }

                var duration = QuoteRemover.Remove(duplicates.Length >= 3 ? duplicates[2] : string.Empty);
                var rate = QuoteRemover.Remove(duplicates.Length >= 2 ? duplicates[1] : string.Empty);
                text = string.Format(_localization.GetTexts(Localization.AUTO_BONUS_1), bonuses, flag, TryParseInt(rate, 10), TryParseInt(duration, 1000));
            }
            else
                text = string.Empty;
        }
        // bonus_script
        if (text.Contains("bonus_script \"{"))
        {
            var duplicate = string.Empty;

            var temp = text.Replace("bonus_script \"{", string.Empty);
            if (temp.IndexOf("}\"") > 0)
            {
                duplicate = temp.Substring(temp.IndexOf("}\"") + 2);

                temp = temp.Substring(0, temp.IndexOf("}\""));

                text = text.Replace(temp + "}\"", string.Empty);
            }
            var duplicates = duplicate.Split(',');
            var bonuses = string.Empty;
            var allBonus = temp.Split(';');
            for (int i = 0; i < allBonus.Length; i++)
            {
                if (!string.IsNullOrEmpty(allBonus[i]) && !string.IsNullOrWhiteSpace(allBonus[i]))
                {
                    text = text.Replace(allBonus[i] + ";", string.Empty);

                    bonuses += ConvertItemScripts(allBonus[i]);
                }
            }

            if (!string.IsNullOrEmpty(bonuses) || !string.IsNullOrWhiteSpace(bonuses))
            {
                bonuses = bonuses.Replace("๐", "[NEW_LINE]๐");
                bonuses = bonuses.Replace("^FF2525", "[NEW_LINE]^FF2525");

                int number = 1;
                while (bonuses.Contains("๐"))
                {
                    bonuses = ReplaceOneTime.ReplaceNow(bonuses, "๐", number.ToString("f0") + ".)");

                    number++;
                }

                var duration = QuoteRemover.Remove(duplicates.Length >= 2 ? duplicates[1] : string.Empty);
                text = string.Format(_localization.GetTexts(Localization.BONUS_SCRIPT), bonuses, TryParseInt(duration));
            }
            else
                text = string.Empty;

            ParseStatusChangeStartIntoItemId();
        }

        text = text.Replace(";", string.Empty);

        text = text.Replace("UnEquipScript: |", "^666478[" + _localization.GetTexts(Localization.WHEN_UNEQUIP) + "]^000000");
        text = text.Replace("EquipScript: |", "^666478[" + _localization.GetTexts(Localization.WHEN_EQUIP) + "]^000000");

        text = text.Replace("bonus bStr,", "๐ Str +");
        text = text.Replace("bonus bAgi,", "๐ Agi +");
        text = text.Replace("bonus bVit,", "๐ Vit +");
        text = text.Replace("bonus bInt,", "๐ Int +");
        text = text.Replace("bonus bDex,", "๐ Dex +");
        text = text.Replace("bonus bLuk,", "๐ Luk +");
        text = text.Replace("bonus bAllStats,", "๐ All Status +");
        text = text.Replace("bonus bAgiVit,", "๐ Agi, Vit +");
        text = text.Replace("bonus bAgiDexStr,", "๐ Agi, Dex, Str +");

        text = text.Replace("bonus bPow,", "๐ Pow +");
        text = text.Replace("bonus bSta,", "๐ Sta +");
        text = text.Replace("bonus bWis,", "๐ Wis +");
        text = text.Replace("bonus bSpl,", "๐ Spl +");
        text = text.Replace("bonus bCon,", "๐ Con +");
        text = text.Replace("bonus bCrt,", "๐ Crt +");
        text = text.Replace("bonus bAllTraitStats,", "๐ All Trait +");

        text = text.Replace("bonus bMaxHP,", "๐ MaxHP +");
        if (text.Contains("bonus bMaxHPrate,"))
        {
            var temp = text.Replace("bonus bMaxHPrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ MaxHP +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMaxSP,", "๐ MaxSP +");
        if (text.Contains("bonus bMaxSPrate,"))
        {
            var temp = text.Replace("bonus bMaxSPrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ MaxSP +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMaxAP,", "๐ MaxAP +");
        if (text.Contains("bonus bMaxAPrate,"))
        {
            var temp = text.Replace("bonus bMaxAPrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ MaxAP +{0}%", TryParseInt(temps[0]));
        }

        text = text.Replace("bonus bBaseAtk,", _localization.GetTexts(Localization.BONUS_BASE_ATK));
        text = text.Replace("bonus bAtk,", "๐ Atk +");
        text = text.Replace("bonus bAtk2,", "๐ Atk +");
        if (text.Contains("bonus bAtkRate,"))
        {
            var temp = text.Replace("bonus bAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Atk +{0}%", TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bWeaponAtkRate,"))
        {
            var temp = text.Replace("bonus bWeaponAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_WEAPON_ATK_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMatk,", "๐ MAtk +");
        if (text.Contains("bonus bMatkRate,"))
        {
            var temp = text.Replace("bonus bMatkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ MAtk +{0}%", TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bWeaponMatkRate,"))
        {
            var temp = text.Replace("bonus bWeaponMatkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_WEAPON_MATK_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bDef,", "๐ Def +");
        if (text.Contains("bonus bDefRate,"))
        {
            var temp = text.Replace("bonus bDefRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Def +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bDef2,", _localization.GetTexts(Localization.BONUS_DEF2));
        if (text.Contains("bonus bDef2Rate,"))
        {
            var temp = text.Replace("bonus bDef2Rate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DEF2_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMdef,", "๐ MDef +");
        if (text.Contains("bonus bMdefRate,"))
        {
            var temp = text.Replace("bonus bMdefRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ MDef +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMdef2,", _localization.GetTexts(Localization.BONUS_MDEF2));
        if (text.Contains("bonus bMdef2Rate,"))
        {
            var temp = text.Replace("bonus bMdef2Rate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_MDEF2_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bHit,", "๐ Hit +");
        if (text.Contains("bonus bHitRate,"))
        {
            var temp = text.Replace("bonus bHitRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Hit +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bCritical,", "๐ Critical +");
        text = text.Replace("bonus bCriticalLong,", _localization.GetTexts(Localization.BONUS_CRITICAL_LONG));
        if (text.Contains("bonus2 bCriticalAddRace,"))
        {
            var temp = text.Replace("bonus2 bCriticalAddRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_CRITICAL_ADD_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bCriticalRate,"))
        {
            var temp = text.Replace("bonus bCriticalRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Critical +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bFlee,", "๐ Flee +");
        if (text.Contains("bonus bFleeRate,"))
        {
            var temp = text.Replace("bonus bFleeRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Flee +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bFlee2,", "๐ Perfect Dodge +");
        if (text.Contains("bonus bFlee2Rate,"))
        {
            var temp = text.Replace("bonus bFlee2Rate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Perfect Dodge +{0}%", TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bPerfectHitRate,"))
        {
            var temp = text.Replace("bonus bPerfectHitRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_PERFECT_HIT_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bPerfectHitAddRate,"))
        {
            var temp = text.Replace("bonus bPerfectHitAddRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Perfect Hit +{0}%", TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bSpeedRate,"))
        {
            var temp = text.Replace("bonus bSpeedRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_SPEED_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bSpeedAddRate,"))
        {
            var temp = text.Replace("bonus bSpeedAddRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_SPEED_ADD_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bAspd,", "๐ ASPD +");
        if (text.Contains("bonus bAspdRate,"))
        {
            var temp = text.Replace("bonus bAspdRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ ASPD +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bAtkRange,", _localization.GetTexts(Localization.BONUS_ATK_RANGE));
        if (text.Contains("bonus bAddMaxWeight,"))
        {
            var temp = text.Replace("bonus bAddMaxWeight,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ADD_MAX_WEIGHT), TryParseInt(temps[0], 10));
        }

        text = text.Replace("bonus bPatk,", "๐ P.Atk +");
        if (text.Contains("bonus bPAtkRate,"))
        {
            var temp = text.Replace("bonus bPAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ P.Atk +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bSmatk,", "๐ S.MAtk +");
        if (text.Contains("bonus bSMatkRate,"))
        {
            var temp = text.Replace("bonus bSMatkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ S.MAtk +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bRes,", "๐ Res +");
        if (text.Contains("bonus bResRate,"))
        {
            var temp = text.Replace("bonus bResRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ Res +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bMres,", "๐ M.Res +");
        if (text.Contains("bonus bMResRate,"))
        {
            var temp = text.Replace("bonus bMResRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ M.Res +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bHplus,", "๐ H.Plus +");
        if (text.Contains("bonus bHPlusRate,"))
        {
            var temp = text.Replace("bonus bHPlusRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ H.Plus +{0}%", TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bCrate,", "๐ C.Rate +");
        if (text.Contains("bonus bCRateRate,"))
        {
            var temp = text.Replace("bonus bCRateRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format("๐ C.Rate +{0}%", TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bHPrecovRate,"))
        {
            var temp = text.Replace("bonus bHPrecovRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_HP_RECOV_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bSPrecovRate,"))
        {
            var temp = text.Replace("bonus bSPrecovRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_SP_RECOV_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bHPRegenRate,"))
        {
            var temp = text.Replace("bonus2 bHPRegenRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_REGEN_RATE), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bHPLossRate,"))
        {
            var temp = text.Replace("bonus2 bHPLossRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_LOSS_RATE), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bSPRegenRate,"))
        {
            var temp = text.Replace("bonus2 bSPRegenRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_REGEN_RATE), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bSPLossRate,"))
        {
            var temp = text.Replace("bonus2 bSPLossRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_LOSS_RATE), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bRegenPercentHP,"))
        {
            var temp = text.Replace("bonus2 bRegenPercentHP,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_REGEN_PERCENT_HP), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bRegenPercentSP,"))
        {
            var temp = text.Replace("bonus2 bRegenPercentSP,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_REGEN_PERCENT_SP), TryParseInt(temps[0]), TryParseInt(temps[1], 1000));
        }
        text = text.Replace("bonus bNoRegen,1", _localization.GetTexts(Localization.BONUS_STOP_HP_REGEN));
        text = text.Replace("bonus bNoRegen,2", _localization.GetTexts(Localization.BONUS_STOP_SP_REGEN));
        if (text.Contains("bonus bUseSPrate,"))
        {
            var temp = text.Replace("bonus bUseSPrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_USE_SP_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bSkillUseSP,"))
        {
            var temp = text.Replace("bonus2 bSkillUseSP,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_USE_SP), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
            text = text.Replace("--", "+");
        }
        if (text.Contains("bonus2 bSkillUseSPrate,"))
        {
            var temp = text.Replace("bonus2 bSkillUseSPrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_USE_SP_RATE), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
            text = text.Replace("--", "+");
        }
        if (text.Contains("bonus2 bSkillAtk,"))
        {
            var temp = text.Replace("bonus2 bSkillAtk,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_ATK), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bShortAtkRate,"))
        {
            var temp = text.Replace("bonus bShortAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_SHORT_ATK_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bLongAtkRate,"))
        {
            var temp = text.Replace("bonus bLongAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_LONG_ATK_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bCritAtkRate,"))
        {
            var temp = text.Replace("bonus bCritAtkRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_CRIT_ATK_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bCritDefRate,"))
        {
            var temp = text.Replace("bonus bCritDefRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_CRIT_DEF_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bCriticalDef,"))
        {
            var temp = text.Replace("bonus bCriticalDef,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_CRITICAL_DEF), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bWeaponAtk,"))
        {
            var temp = text.Replace("bonus2 bWeaponAtk,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_ATK), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bWeaponDamageRate,"))
        {
            var temp = text.Replace("bonus2 bWeaponDamageRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_DAMAGE_RATE), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bNearAtkDef,"))
        {
            var temp = text.Replace("bonus bNearAtkDef,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_NEAR_ATK_DEF), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bLongAtkDef,"))
        {
            var temp = text.Replace("bonus bLongAtkDef,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_LONG_ATK_DEF), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bMagicAtkDef,"))
        {
            var temp = text.Replace("bonus bMagicAtkDef,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_MAGIC_ATK_DEF), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bMiscAtkDef,"))
        {
            var temp = text.Replace("bonus bMiscAtkDef,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_MISC_ATK_DEF), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bNoWeaponDamage,"))
        {
            var temp = text.Replace("bonus bNoWeaponDamage,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_NO_WEAPON_DAMAGE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bNoMagicDamage,"))
        {
            var temp = text.Replace("bonus bNoMagicDamage,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_NO_MAGIC_DAMAGE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bNoMiscDamage,"))
        {
            var temp = text.Replace("bonus bNoMiscDamage,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_NO_MISC_DAMAGE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bHealPower,"))
        {
            var temp = text.Replace("bonus bHealPower,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_HEAL_POWER), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bHealPower2,"))
        {
            var temp = text.Replace("bonus bHealPower2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_HEAL_POWER_2), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bSkillHeal,"))
        {
            var temp = text.Replace("bonus2 bSkillHeal,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_HEAL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSkillHeal2,"))
        {
            var temp = text.Replace("bonus2 bSkillHeal2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_HEAL_2), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bAddItemHealRate,"))
        {
            var temp = text.Replace("bonus bAddItemHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ADD_ITEM_HEAL_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bAddItemHealRate,"))
        {
            var temp = text.Replace("bonus2 bAddItemHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_ITEM_HEAL_RATE), GetItemName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddItemGroupHealRate,"))
        {
            var temp = text.Replace("bonus2 bAddItemGroupHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_ITEM_GROUP_HEAL_RATE), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bAddItemSPHealRate,"))
        {
            var temp = text.Replace("bonus bAddItemSPHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ADD_ITEM_SP_HEAL_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bAddItemSPHealRate,"))
        {
            var temp = text.Replace("bonus2 bAddItemSPHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_ITEM_SP_HEAL_RATE), GetItemName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddItemGroupSPHealRate,"))
        {
            var temp = text.Replace("bonus2 bAddItemGroupSPHealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_ITEM_GROUP_SP_HEAL_RATE), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bCastrate,"))
        {
            var temp = text.Replace("bonus bCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_CAST_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bCastrate,"))
        {
            var temp = text.Replace("bonus2 bCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_CAST_RATE), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bFixedCastrate,"))
        {
            var temp = text.Replace("bonus bFixedCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_FIXED_CAST_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bFixedCastrate,"))
        {
            var temp = text.Replace("bonus2 bFixedCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_FIXED_CAST_RATE), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bVariableCastrate,"))
        {
            var temp = text.Replace("bonus bVariableCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_VARIABLE_CAST_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bVariableCastrate,"))
        {
            var temp = text.Replace("bonus2 bVariableCastrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_VARIABLE_CAST_RATE), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bFixedCast,"))
        {
            var temp = text.Replace("bonus bFixedCast,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_FIXED_CAST), TryParseInt(temps[0], 1000));
        }
        if (text.Contains("bonus2 bSkillFixedCast,"))
        {
            var temp = text.Replace("bonus2 bSkillFixedCast,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_FIXED_CAST), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus bVariableCast,"))
        {
            var temp = text.Replace("bonus bVariableCast,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_VARIABLE_CAST), TryParseInt(temps[0], 1000));
        }
        if (text.Contains("bonus2 bSkillVariableCast,"))
        {
            var temp = text.Replace("bonus2 bSkillVariableCast,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_VARIABLE_CAST), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1], 1000));
        }
        text = text.Replace("bonus bNoCastCancel2", _localization.GetTexts(Localization.BONUS_NO_CAST_CANCEL_2));
        text = text.Replace("bonus bNoCastCancel", _localization.GetTexts(Localization.BONUS_NO_CAST_CANCEL));
        if (text.Contains("bonus bDelayrate,"))
        {
            var temp = text.Replace("bonus bDelayrate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DELAY_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bDelayRate,"))
        {
            var temp = text.Replace("bonus bDelayRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DELAY_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus2 bSkillDelay,"))
        {
            var temp = text.Replace("bonus2 bSkillDelay,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_DELAY), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bSkillCooldown,"))
        {
            var temp = text.Replace("bonus2 bSkillCooldown,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SKILL_COOLDOWN), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1], 1000));
        }
        if (text.Contains("bonus2 bAddEle,"))
        {
            var temp = text.Replace("bonus2 bAddEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bAddEle,"))
        {
            var temp = text.Replace("bonus3 bAddEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_ELE), ParseElement(temps[0]), TryParseInt(temps[1]), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus2 bMagicAddEle,"))
        {
            var temp = text.Replace("bonus2 bMagicAddEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ADD_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubEle,"))
        {
            var temp = text.Replace("bonus2 bSubEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bSubEle,"))
        {
            var temp = text.Replace("bonus3 bSubEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_SUB_ELE), ParseElement(temps[0]), TryParseInt(temps[1]), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus2 bSubDefEle,"))
        {
            var temp = text.Replace("bonus2 bSubDefEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_DEF_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicSubDefEle,"))
        {
            var temp = text.Replace("bonus2 bMagicSubDefEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_SUB_DEF_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddRace,"))
        {
            var temp = text.Replace("bonus2 bAddRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicAddRace,"))
        {
            var temp = text.Replace("bonus2 bMagicAddRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ADD_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubRace,"))
        {
            var temp = text.Replace("bonus2 bSubRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bSubRace,"))
        {
            var temp = text.Replace("bonus3 bSubRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_SUB_RACE), ParseRace(temps[0]), TryParseInt(temps[1]), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus2 bAddClass,"))
        {
            var temp = text.Replace("bonus2 bAddClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicAddClass,"))
        {
            var temp = text.Replace("bonus2 bMagicAddClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ADD_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubClass,"))
        {
            var temp = text.Replace("bonus2 bSubClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddSize,"))
        {
            var temp = text.Replace("bonus2 bAddSize,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_SIZE), ParseSize(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicAddSize,"))
        {
            var temp = text.Replace("bonus2 bMagicAddSize,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ADD_SIZE), ParseSize(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubSize,"))
        {
            var temp = text.Replace("bonus2 bSubSize,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_SIZE), ParseSize(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bWeaponSubSize,"))
        {
            var temp = text.Replace("bonus2 bWeaponSubSize,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_SUB_SIZE), ParseSize(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicSubSize,"))
        {
            var temp = text.Replace("bonus2 bMagicSubSize,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_SUB_SIZE), ParseSize(temps[0]), TryParseInt(temps[1]));
        }
        text = text.Replace("bonus bNoSizeFix", _localization.GetTexts(Localization.BONUS_NO_SIZE_FIX));
        if (text.Contains("bonus2 bAddDamageClass,"))
        {
            var temp = text.Replace("bonus2 bAddDamageClass,", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(TryParseInt(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_DAMAGE_CLASS), (monsterDatabase != null) ? "^FF0000" + monsterDatabase.name + "^000000" : temps[0], TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddMagicDamageClass,"))
        {
            var temp = text.Replace("bonus2 bAddMagicDamageClass,", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(TryParseInt(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_MAGIC_DAMAGE_CLASS), (monsterDatabase != null) ? "^FF0000" + monsterDatabase.name + "^000000" : temps[0], TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddDefMonster,"))
        {
            var temp = text.Replace("bonus2 bAddDefMonster,", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(TryParseInt(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_DEF_MONSTER), (monsterDatabase != null) ? "^FF0000" + monsterDatabase.name + "^000000" : temps[0], TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddMDefMonster,"))
        {
            var temp = text.Replace("bonus2 bAddMDefMonster,", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(TryParseInt(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_MDEF_MONSTER), (monsterDatabase != null) ? "^FF0000" + monsterDatabase.name + "^000000" : temps[0], TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddRace2,"))
        {
            var temp = text.Replace("bonus2 bAddRace2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_RACE_2), ParseRace2(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubRace2,"))
        {
            var temp = text.Replace("bonus2 bSubRace2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_RACE_2), ParseRace2(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bMagicAddRace2,"))
        {
            var temp = text.Replace("bonus2 bMagicAddRace2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ADD_RACE_2), ParseRace2(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSubSkill,"))
        {
            var temp = text.Replace("bonus2 bSubSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SUB_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bAbsorbDmgMaxHP,"))
        {
            var temp = text.Replace("bonus bAbsorbDmgMaxHP,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ABSORB_DMG_MAX_HP), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bAbsorbDmgMaxHP2,"))
        {
            var temp = text.Replace("bonus bAbsorbDmgMaxHP2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ABSORB_DMG_MAX_HP_2), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bAtkEle,"))
        {
            var temp = text.Replace("bonus bAtkEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ATK_ELE), ParseElement(temps[0]));
        }
        if (text.Contains("bonus bDefEle,"))
        {
            var temp = text.Replace("bonus bDefEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DEF_ELE), ParseElement(temps[0]));
        }
        if (text.Contains("bonus2 bMagicAtkEle,"))
        {
            var temp = text.Replace("bonus2 bMagicAtkEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_MAGIC_ATK_ELE), ParseElement(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bDefRatioAtkRace,"))
        {
            var temp = text.Replace("bonus bDefRatioAtkRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DEF_RATIO_ATK_RACE), ParseRace(temps[0]));
        }
        if (text.Contains("bonus bDefRatioAtkEle,"))
        {
            var temp = text.Replace("bonus bDefRatioAtkEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DEF_RATIO_ATK_ELE), ParseElement(temps[0]));
        }
        if (text.Contains("bonus bDefRatioAtkClass,"))
        {
            var temp = text.Replace("bonus bDefRatioAtkClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DEF_RATIO_ATK_CLASS), ParseClass(temps[0]));
        }
        if (text.Contains("bonus4 bSetDefRace,"))
        {
            var temp = text.Replace("bonus4 bSetDefRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_SET_DEF_RACE), ParseRace(temps[0]), TryParseInt(temps[1]), TryParseInt(temps[2], 1000), TryParseInt(temps[3]));
        }
        if (text.Contains("bonus4 bSetMDefRace,"))
        {
            var temp = text.Replace("bonus4 bSetMDefRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_SET_MDEF_RACE), ParseRace(temps[0]), TryParseInt(temps[1]), TryParseInt(temps[2], 1000), TryParseInt(temps[3]));
        }
        if (text.Contains("bonus bIgnoreDefEle,"))
        {
            var temp = text.Replace("bonus bIgnoreDefEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_IGNORE_DEF_ELE), ParseElement(temps[0]));
        }
        if (text.Contains("bonus bIgnoreDefRace,"))
        {
            var temp = text.Replace("bonus bIgnoreDefRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_IGNORE_DEF_RACE), ParseRace(temps[0]));
        }
        if (text.Contains("bonus bIgnoreDefClass,"))
        {
            var temp = text.Replace("bonus bIgnoreDefClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_IGNORE_DEF_CLASS), ParseClass(temps[0]));
        }
        if (text.Contains("bonus bIgnoreMDefRace,"))
        {
            var temp = text.Replace("bonus bIgnoreMDefRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_IGNORE_MDEF_RACE), ParseRace(temps[0]));
        }
        if (text.Contains("bonus2 bIgnoreDefRaceRate,"))
        {
            var temp = text.Replace("bonus2 bIgnoreDefRaceRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_IGNORE_DEF_RACE_RATE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bIgnoreMdefRaceRate,"))
        {
            var temp = text.Replace("bonus2 bIgnoreMdefRaceRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_IGNORE_MDEF_RACE_RATE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bIgnoreMdefRace2Rate,"))
        {
            var temp = text.Replace("bonus2 bIgnoreMdefRace2Rate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_IGNORE_MDEF_RACE_2_RATE), ParseRace2(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bIgnoreMDefEle,"))
        {
            var temp = text.Replace("bonus bIgnoreMDefEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_IGNORE_MDEF_ELE), ParseElement(temps[0]));
        }
        if (text.Contains("bonus2 bIgnoreDefClassRate,"))
        {
            var temp = text.Replace("bonus2 bIgnoreDefClassRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_IGNORE_DEF_CLASS_RATE), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bIgnoreMdefClassRate,"))
        {
            var temp = text.Replace("bonus2 bIgnoreMdefClassRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_IGNORE_MDEF_CLASS_RATE), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bExpAddRace,"))
        {
            var temp = text.Replace("bonus2 bExpAddRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_EXP_ADD_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bExpAddClass,"))
        {
            var temp = text.Replace("bonus2 bExpAddClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_EXP_ADD_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddEff,"))
        {
            var temp = text.Replace("bonus2 bAddEff,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_EFF), ParseEffect(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bAddEff2,"))
        {
            var temp = text.Replace("bonus2 bAddEff2,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_EFF_2), ParseEffect(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bAddEffWhenHit,"))
        {
            var temp = text.Replace("bonus2 bAddEffWhenHit,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_EFF_WHEN_HIT), ParseEffect(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bResEff,"))
        {
            var temp = text.Replace("bonus2 bResEff,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_RES_EFF), ParseEffect(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus3 bAddEff,"))
        {
            var temp = text.Replace("bonus3 bAddEff,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_EFF), ParseEffect(temps[0]), TryParseInt(temps[1], 100), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus4 bAddEff,"))
        {
            var temp = text.Replace("bonus4 bAddEff,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_ADD_EFF), ParseEffect(temps[0]), TryParseInt(temps[1], 100), ParseAtf(temps[2]), TryParseInt(temps[3], 1000));
        }
        if (text.Contains("bonus3 bAddEffWhenHit,"))
        {
            var temp = text.Replace("bonus3 bAddEffWhenHit,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_EFF_WHEN_HIT), ParseEffect(temps[0]), TryParseInt(temps[1], 100), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus4 bAddEffWhenHit,"))
        {
            var temp = text.Replace("bonus4 bAddEff,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_ADD_EFF_WHEN_HIT), ParseEffect(temps[0]), TryParseInt(temps[1], 100), ParseAtf(temps[2]), TryParseInt(temps[3], 1000));
        }
        if (text.Contains("bonus3 bAddEffOnSkill,"))
        {
            var temp = text.Replace("bonus3 bAddEffOnSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_EFF_ON_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), ParseEffect(temps[1]), TryParseInt(temps[2], 100));
        }
        if (text.Contains("bonus4 bAddEffOnSkill,"))
        {
            var temp = text.Replace("bonus4 bAddEffOnSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_ADD_EFF_ON_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), ParseEffect(temps[1]), TryParseInt(temps[2], 100), ParseAtf(temps[3]));
        }
        if (text.Contains("bonus5 bAddEffOnSkill,"))
        {
            var temp = text.Replace("bonus5 bAddEffOnSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS5_ADD_EFF_ON_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), ParseEffect(temps[1]), TryParseInt(temps[2], 100), ParseAtf(temps[3]), TryParseInt(temps[4], 1000));
        }
        if (text.Contains("bonus2 bComaClass,"))
        {
            var temp = text.Replace("bonus2 bComaClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_COMA_CLASS), ParseClass(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bComaRace,"))
        {
            var temp = text.Replace("bonus2 bComaRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_COMA_RACE), ParseRace(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bWeaponComaEle,"))
        {
            var temp = text.Replace("bonus2 bWeaponComaEle,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_COMA_ELE), ParseElement(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bWeaponComaClass,"))
        {
            var temp = text.Replace("bonus2 bWeaponComaClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_COMA_CLASS), ParseClass(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus2 bWeaponComaRace,"))
        {
            var temp = text.Replace("bonus2 bWeaponComaRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_WEAPON_COMA_RACE), ParseRace(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus3 bAutoSpell,"))
        {
            var temp = text.Replace("bonus3 bAutoSpell,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_AUTO_SPELL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10));
        }
        if (text.Contains("bonus3 bAutoSpellWhenHit,"))
        {
            var temp = text.Replace("bonus3 bAutoSpellWhenHit,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_AUTO_SPELL_WHEN_HIT), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10));
        }
        if (text.Contains("bonus4 bAutoSpell,"))
        {
            var temp = text.Replace("bonus4 bAutoSpell,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_AUTO_SPELL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10), ParseI(temps[3]));
        }
        if (text.Contains("bonus5 bAutoSpell,"))
        {
            var temp = text.Replace("bonus5 bAutoSpell,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS5_AUTO_SPELL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10), ParseAtf(temps[3]), ParseI(temps[4]));
        }
        if (text.Contains("bonus4 bAutoSpellWhenHit,"))
        {
            var temp = text.Replace("bonus4 bAutoSpellWhenHit,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_AUTO_SPELL_WHEN_HIT), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10), ParseI(temps[3]));
        }
        if (text.Contains("bonus5 bAutoSpellWhenHit,"))
        {
            var temp = text.Replace("bonus5 bAutoSpellWhenHit,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS5_AUTO_SPELL_WHEN_HIT), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]), TryParseInt(temps[2], 10), ParseAtf(temps[3]), ParseI(temps[4]));
        }
        if (text.Contains("bonus4 bAutoSpellOnSkill,"))
        {
            var temp = text.Replace("bonus4 bAutoSpellOnSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS4_AUTO_SPELL_ON_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), GetSkillName(QuoteRemover.Remove(temps[1])), TryParseInt(temps[2]), TryParseInt(temps[3], 10));
        }
        if (text.Contains("bonus5 bAutoSpellOnSkill,"))
        {
            var temp = text.Replace("bonus5 bAutoSpellOnSkill,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS5_AUTO_SPELL_ON_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), GetSkillName(QuoteRemover.Remove(temps[1])), TryParseInt(temps[2]), TryParseInt(temps[3], 10), ParseI(temps[4]));
        }
        text = text.Replace("bonus bHPDrainValue,", _localization.GetTexts(Localization.BONUS_HP_DRAIN_VALUE));
        if (text.Contains("bonus2 bHPDrainValueRace,"))
        {
            var temp = text.Replace("bonus2 bHPDrainValueRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_DRAIN_VALUE_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bHpDrainValueClass,"))
        {
            var temp = text.Replace("bonus2 bHpDrainValueClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_DRAIN_VALUE_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        text = text.Replace("bonus bSPDrainValue,", _localization.GetTexts(Localization.BONUS_SP_DRAIN_VALUE));
        if (text.Contains("bonus2 bSPDrainValueRace,"))
        {
            var temp = text.Replace("bonus2 bSPDrainValueRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_DRAIN_VALUE_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSpDrainValueClass,"))
        {
            var temp = text.Replace("bonus2 bSpDrainValueClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_DRAIN_VALUE_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bHPDrainRate,"))
        {
            var temp = text.Replace("bonus2 bHPDrainRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_DRAIN_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bSPDrainRate,"))
        {
            var temp = text.Replace("bonus2 bSPDrainRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_DRAIN_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bHPVanishRate,"))
        {
            var temp = text.Replace("bonus2 bHPVanishRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_HP_VANISH_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bHPVanishRaceRate,"))
        {
            var temp = text.Replace("bonus3 bHPVanishRaceRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_HP_VANISH_RACE_RATE), ParseRace(temps[0]), TryParseInt(temps[1], 10), TryParseInt(temps[2]));
        }
        if (text.Contains("bonus3 bHPVanishRate,"))
        {
            var temp = text.Replace("bonus3 bHPVanishRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_HP_VANISH_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus2 bSPVanishRate,"))
        {
            var temp = text.Replace("bonus2 bSPVanishRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_VANISH_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bSPVanishRaceRate,"))
        {
            var temp = text.Replace("bonus3 bSPVanishRaceRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_SP_VANISH_RACE_RATE), ParseRace(temps[0]), TryParseInt(temps[1], 10), TryParseInt(temps[2]));
        }
        if (text.Contains("bonus3 bSPVanishRate,"))
        {
            var temp = text.Replace("bonus3 bSPVanishRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_SP_VANISH_RATE), TryParseInt(temps[0], 10), TryParseInt(temps[1]), ParseAtf(temps[2]));
        }
        if (text.Contains("bonus3 bStateNoRecoverRace,"))
        {
            var temp = text.Replace("bonus3 bStateNoRecoverRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_STATE_NO_RECOVER_RACE), ParseRace(temps[0]), TryParseInt(temps[1], 100), TryParseInt(temps[2], 1000));
        }
        text = text.Replace("bonus bHPGainValue,", _localization.GetTexts(Localization.BONUS_HP_GAIN_VALUE));
        text = text.Replace("bonus bSPGainValue,", _localization.GetTexts(Localization.BONUS_SP_GAIN_VALUE));
        if (text.Contains("bonus2 bSPGainRace,"))
        {
            var temp = text.Replace("bonus2 bSPGainRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_SP_GAIN_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        text = text.Replace("bonus bLongHPGainValue,", _localization.GetTexts(Localization.BONUS_LONG_HP_GAIN_VALUE));
        text = text.Replace("bonus bLongSPGainValue,", _localization.GetTexts(Localization.BONUS_LONG_SP_GAIN_VALUE));
        text = text.Replace("bonus bMagicHPGainValue,", _localization.GetTexts(Localization.BONUS_MAGIC_HP_GAIN_VALUE));
        text = text.Replace("bonus bMagicSPGainValue,", _localization.GetTexts(Localization.BONUS_MAGIC_SP_GAIN_VALUE));
        if (text.Contains("bonus bShortWeaponDamageReturn,"))
        {
            var temp = text.Replace("bonus bShortWeaponDamageReturn,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_SHORT_WEAPON_DAMAGE_RETURN), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bLongWeaponDamageReturn,"))
        {
            var temp = text.Replace("bonus bLongWeaponDamageReturn,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_LONG_WEAPON_DAMAGE_RETURN), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bMagicDamageReturn,"))
        {
            var temp = text.Replace("bonus bMagicDamageReturn,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_MAGIC_DAMAGE_RETURN), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bReduceDamageReturn,"))
        {
            var temp = text.Replace("bonus bReduceDamageReturn,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_REDUCE_DAMAGE_RETURN), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bUnstripableWeapon", _localization.GetTexts(Localization.BONUS_UNSTRIPABLE_WEAPON));
        text = text.Replace("bonus bUnstripableArmor", _localization.GetTexts(Localization.BONUS_UNSTRIPABLE_ARMOR));
        text = text.Replace("bonus bUnstripableHelm", _localization.GetTexts(Localization.BONUS_UNSTRIPABLE_HELM));
        text = text.Replace("bonus bUnstripableShield", _localization.GetTexts(Localization.BONUS_UNSTRIPABLE_SHIELD));
        text = text.Replace("bonus bUnstripable", _localization.GetTexts(Localization.BONUS_UNSTRIPABLE));
        text = text.Replace("bonus bUnbreakableGarment", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_GARMENT));
        text = text.Replace("bonus bUnbreakableWeapon", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_WEAPON));
        text = text.Replace("bonus bUnbreakableArmor", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_ARMOR));
        text = text.Replace("bonus bUnbreakableHelm", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_HELM));
        text = text.Replace("bonus bUnbreakableShield", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_SHIELD));
        text = text.Replace("bonus bUnbreakableShoes", _localization.GetTexts(Localization.BONUS_UNBREAKABLE_SHOES));
        if (text.Contains("bonus bUnbreakable,"))
        {
            var temp = text.Replace("bonus bUnbreakable,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_UNBREAKABLE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bBreakWeaponRate,"))
        {
            var temp = text.Replace("bonus bBreakWeaponRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_BREAK_WEAPON_RATE), TryParseInt(temps[0], 100));
        }
        if (text.Contains("bonus bBreakArmorRate,"))
        {
            var temp = text.Replace("bonus bBreakArmorRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_BREAK_ARMOR_RATE), TryParseInt(temps[0], 100));
        }
        if (text.Contains("bonus2 bDropAddRace,"))
        {
            var temp = text.Replace("bonus2 bDropAddRace,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_DROP_ADD_RACE), ParseRace(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bDropAddClass,"))
        {
            var temp = text.Replace("bonus2 bDropAddClass,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_DROP_ADD_CLASS), ParseClass(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus3 bAddMonsterIdDropItem,"))
        {
            var temp = text.Replace("bonus3 bAddMonsterIdDropItem,", string.Empty);
            var temps = temp.Split(',');
            var itemId = QuoteRemover.Remove(temps[0]);
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_MONSTER_ID_DROP_ITEM), GetItemName(itemId), TryParseInt(temps[1]), TryParseInt(temps[2], 100), itemId);
        }
        if (text.Contains("bonus2 bAddMonsterDropItem,"))
        {
            var temp = text.Replace("bonus2 bAddMonsterDropItem,", string.Empty);
            var temps = temp.Split(',');
            var itemId = QuoteRemover.Remove(temps[0]);
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_MONSTER_DROP_ITEM), GetItemName(itemId), TryParseInt(temps[1], 100), itemId);
        }
        if (text.Contains("bonus3 bAddMonsterDropItem,"))
        {
            var temp = text.Replace("bonus3 bAddMonsterDropItem,", string.Empty);
            var temps = temp.Split(',');
            var itemId = QuoteRemover.Remove(temps[0]);
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_MONSTER_DROP_ITEM), GetItemName(itemId), ParseRace(temps[1]), TryParseInt(temps[2], 100), itemId);
        }
        if (text.Contains("bonus3 bAddClassDropItem,"))
        {
            var temp = text.Replace("bonus3 bAddClassDropItem,", string.Empty);
            var temps = temp.Split(',');
            var itemId = QuoteRemover.Remove(temps[0]);
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_CLASS_DROP_ITEM), GetItemName(itemId), ParseClass(temps[1]), TryParseInt(temps[2], 100), itemId);
        }
        if (text.Contains("bonus2 bAddMonsterDropItemGroup,"))
        {
            var temp = text.Replace("bonus2 bAddMonsterDropItemGroup,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_MONSTER_DROP_ITEM_GROUP), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1], 100));
        }
        if (text.Contains("bonus3 bAddMonsterDropItemGroup,"))
        {
            var temp = text.Replace("bonus3 bAddMonsterDropItemGroup,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_MONSTER_DROP_ITEM_GROUP), QuoteRemover.Remove(temps[0]), ParseRace(temps[1]), TryParseInt(temps[2], 100));
        }
        if (text.Contains("bonus3 bAddClassDropItemGroup,"))
        {
            var temp = text.Replace("bonus3 bAddClassDropItemGroup,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS3_ADD_CLASS_DROP_ITEM_GROUP), QuoteRemover.Remove(temps[0]), ParseClass(temps[1]), TryParseInt(temps[2], 100));
        }
        if (text.Contains("bonus2 bGetZenyNum,"))
        {
            var temp = text.Replace("bonus2 bGetZenyNum,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_GET_ZENY_NUM), TryParseInt(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus2 bAddGetZenyNum,"))
        {
            var temp = text.Replace("bonus2 bAddGetZenyNum,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_GET_ZENY_NUM), TryParseInt(temps[0]), TryParseInt(temps[1]));
        }
        if (text.Contains("bonus bDoubleRate,"))
        {
            var temp = text.Replace("bonus bDoubleRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DOUBLE_RATE), TryParseInt(temps[0]));
        }
        if (text.Contains("bonus bDoubleAddRate,"))
        {
            var temp = text.Replace("bonus bDoubleAddRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_DOUBLE_ADD_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bSplashRange,", _localization.GetTexts(Localization.BONUS_SPLASH_RANGE));
        text = text.Replace("bonus bSplashAddRange,", _localization.GetTexts(Localization.BONUS_SPLASH_ADD_RANGE));
        if (text.Contains("bonus2 bAddSkillBlow,"))
        {
            var temp = text.Replace("bonus2 bAddSkillBlow,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS2_ADD_SKILL_BLOW), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        text = text.Replace("bonus bNoKnockback", _localization.GetTexts(Localization.BONUS_NO_KNOCKBACK));
        text = text.Replace("bonus bNoGemStone", _localization.GetTexts(Localization.BONUS_NO_GEM_STONE));
        text = text.Replace("bonus bIntravision", _localization.GetTexts(Localization.BONUS_INTRAVISION));
        text = text.Replace("bonus bPerfectHide", _localization.GetTexts(Localization.BONUS_PERFECT_HIDE));
        text = text.Replace("bonus bRestartFullRecover", _localization.GetTexts(Localization.BONUS_RESTART_FULL_RECOVER));
        if (text.Contains("bonus bClassChange,"))
        {
            var temp = text.Replace("bonus bClassChange,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_CLASS_CHANGE), TryParseInt(temps[0], 100));
        }
        if (text.Contains("bonus bAddStealRate,"))
        {
            var temp = text.Replace("bonus bAddStealRate,", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.BONUS_ADD_STEAL_RATE), TryParseInt(temps[0]));
        }
        text = text.Replace("bonus bNoMadoFuel", _localization.GetTexts(Localization.BONUS_NO_MADO_FUEL));
        text = text.Replace("bonus bNoWalkDelay", _localization.GetTexts(Localization.BONUS_NO_WALK_DELAY));
        text = text.Replace("specialeffect2", _localization.GetTexts(Localization.SPECIAL_EFFECT_2));
        text = text.Replace("specialeffect", _localization.GetTexts(Localization.SPECIAL_EFFECT));
        // Unit Skill Use Id
        if (text.Contains("unitskilluseid "))
        {
            var temp = text.Replace("unitskilluseid ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.UNIT_SKILL_USE_ID), QuoteRemover.Remove(temps[0]), GetSkillName(QuoteRemover.Remove(temps[1])), TryParseInt(temps[2]));
        }
        // Item Skill
        if (text.Contains("itemskill "))
        {
            var temp = text.Replace("itemskill ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.ITEM_SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        // Skill
        if (text.Contains("skill "))
        {
            var temp = text.Replace("skill ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.SKILL), GetSkillName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        // itemheal
        if (text.Contains("percentheal "))
        {
            var temp = text.Replace("percentheal ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.PERCENT_HEAL), TryParseInt(temps[0]), TryParseInt(temps[1]));
        }
        // itemheal
        if (text.Contains("itemheal "))
        {
            var temp = text.Replace("itemheal ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.ITEM_HEAL), TryParseInt(temps[0]), TryParseInt(temps[1]));
        }
        // heal
        if (((!string.IsNullOrEmpty(text) && (text[0] == 'h')) || text.Contains(" h")) && text.Contains("heal "))
        {
            var temp = text.Replace("heal ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.HEAL), TryParseInt(temps[0]), TryParseInt(temps[1]));
        }
        // sc_start4
        if (text.Contains("sc_start4 "))
        {
            var temp = text.Replace("sc_start4 ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.SC_START_4), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1], 1000));

            ParseStatusChangeStartIntoItemId();
        }
        // sc_start2
        if (text.Contains("sc_start2 "))
        {
            var temp = text.Replace("sc_start2 ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.SC_START_2), QuoteRemover.Remove(temps[0]), TryParseInt(temps[1], 1000));

            ParseStatusChangeStartIntoItemId();
        }
        // sc_start
        if (text.Contains("sc_start "))
        {
            var temp = text.Replace("sc_start ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.SC_START), QuoteRemover.Remove(temps[0]), (temps.Length > 1) ? TryParseInt(temps[1], 1000) : "0");

            ParseStatusChangeStartIntoItemId();
        }
        // sc_end
        if (text.Contains("sc_end "))
        {
            var temp = text.Replace("sc_end ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.SC_END), QuoteRemover.Remove(temps[0]));
        }
        // active_transform
        if (text.Contains("active_transform "))
        {
            var temp = text.Replace("active_transform ", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(QuoteRemover.Remove(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.ACTIVE_TRANSFORM), (monsterDatabase != null) ? monsterDatabase.name : QuoteRemover.Remove(temps[0]), TryParseInt(temps[1], 1000));
        }
        // transform
        if (text.Contains("transform "))
        {
            var temp = text.Replace("transform ", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(QuoteRemover.Remove(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.ACTIVE_TRANSFORM), (monsterDatabase != null) ? monsterDatabase.name : QuoteRemover.Remove(temps[0]), TryParseInt(temps[1], 1000));
        }
        // getitem
        if (text.Contains("getitem "))
        {
            var temp = text.Replace("getitem ", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.GET_ITEM), GetItemName(QuoteRemover.Remove(temps[0])), TryParseInt(temps[1]));
        }
        // getgroupitem
        if (text.Contains("getgroupitem"))
        {
            var temp = text.Replace("getgroupitem", string.Empty);
            var temps = temp.Split(',');
            text = string.Format(_localization.GetTexts(Localization.GET_GROUP_ITEM), QuoteRemover.Remove(temps[0]).Replace("IG_", string.Empty), TryParseInt((temps.Length > 1) ? temps[1] : null, 1, 1));
        }
        // pet
        if (text.Contains("pet "))
        {
            var temp = text.Replace("pet ", string.Empty);
            var temps = temp.Split(',');
            var monsterDatabase = GetMonsterDatabase(QuoteRemover.Remove(temps[0]));
            text = string.Format(_localization.GetTexts(Localization.PET), (monsterDatabase != null) ? "^FF0000" + monsterDatabase.name + "^000000" : temps[0]);
        }
        text = text.Replace("sc_end_class", _localization.GetTexts(Localization.SC_END_CLASS));
        text = text.Replace("setmounting()", _localization.GetTexts(Localization.SET_MOUNTING));
        text = text.Replace("laphine_upgrade()", _localization.GetTexts(Localization.LAPHINE_UPGRADE));
        text = text.Replace("laphine_synthesis()", _localization.GetTexts(Localization.LAPHINE_SYNTHESIS));
        text = text.Replace("openstylist()", _localization.GetTexts(Localization.OPEN_STYLIST));
        text = text.Replace("refineui()", _localization.GetTexts(Localization.REFINE_UI));

        // All in one parse...
        text = AllInOneParse(text);

        // Negative value
        text = text.Replace("+-", "-");
        // Positive value
        text = text.Replace(" ++", " +");
        // autobonus string fix
        text = text.Replace("+%", "%");
        text = text.Replace("⁞", ":");

        // Whitespace
        text = text.Replace("    ", " ");
        text = text.Replace("   ", " ");
        text = text.Replace("  ", " ");
        text = text.Replace("\\", string.Empty);

        // Spacing fix
        text = text.Replace("     ๐", "๐");
        text = text.Replace("    ๐", "๐");
        text = text.Replace("   ๐", "๐");
        text = text.Replace("  ๐", "๐");
        text = text.Replace(" ๐", "๐");

        return text;
    }

    string ParseRace(string text)
    {
        text = text.Replace("RC_Angel", "^AC6523(Angel)^000000");
        text = text.Replace("RC_Brute", "^AC6523(Brute)^000000");
        text = text.Replace("RC_DemiHuman", "^AC6523(Demi-Human)^000000");
        text = text.Replace("RC_Demon", "^AC6523(Demon)^000000");
        text = text.Replace("RC_Dragon", "^AC6523(Dragon)^000000");
        text = text.Replace("RC_Fish", "^AC6523(Fish)^000000");
        text = text.Replace("RC_Formless", "^AC6523(Formless)^000000");
        text = text.Replace("RC_Insect", "^AC6523(Insect)^000000");
        text = text.Replace("RC_Plant", "^AC6523(Plant)^000000");
        text = text.Replace("RC_Player_Human", "^AC6523(Human)^000000");
        text = text.Replace("RC_Player_Doram", "^AC6523(Doram)^000000");
        text = text.Replace("RC_Undead", "^AC6523(Undead)^000000");
        text = text.Replace("RC_All", "^AC6523(" + _localization.GetTexts(Localization.ALL_RACE) + ")^000000");
        return text;
    }

    string ParseRace2(string text)
    {
        text = text.Replace("RC2_Goblin", "^AC6523(Goblin)^000000");
        text = text.Replace("RC2_Kobold", "^AC6523(Kobold)^000000");
        text = text.Replace("RC2_Orc", "^AC6523(Orc)^000000");
        text = text.Replace("RC2_Golem", "^AC6523(Golem)^000000");
        text = text.Replace("RC2_Guardian", "^AC6523(Guardian)^000000");
        text = text.Replace("RC2_Ninja", "^AC6523(Ninja)^000000");
        text = text.Replace("RC2_BioLab", "^AC6523(Biolab)^000000");
        text = text.Replace("RC2_SCARABA", "^AC6523(Scaraba)^000000");
        text = text.Replace("RC2_FACEWORM", "^AC6523(Faceworm)^000000");
        text = text.Replace("RC2_THANATOS", "^AC6523(Thanatos)^000000");
        text = text.Replace("RC2_CLOCKTOWER", "^AC6523(Clocktower)^000000");
        text = text.Replace("RC2_ROCKRIDGE", "^AC6523(Rockridge)^000000");
        return text;
    }

    string ParseClass(string text)
    {
        text = text.Replace("Class_Normal", "^0040B6(Normal)^000000");
        text = text.Replace("Class_Boss", "^0040B6(Boss)^000000");
        text = text.Replace("Class_Guardian", "^0040B6(Guardian)^000000");
        text = text.Replace("Class_All", "^0040B6(" + _localization.GetTexts(Localization.ALL_CLASS) + ")^000000");
        return text;
    }

    string ParseSize(string text)
    {
        text = text.Replace("Size_Small", "^FF26F5(" + _localization.GetTexts(Localization.SIZE_SMALL) + ")^000000");
        text = text.Replace("Size_Medium", "^FF26F5(" + _localization.GetTexts(Localization.SIZE_MEDIUM) + ")^000000");
        text = text.Replace("Size_Large", "^FF26F5(" + _localization.GetTexts(Localization.SIZE_LARGE) + ")^000000");
        text = text.Replace("Size_All", "^FF26F5(" + _localization.GetTexts(Localization.ALL_SIZE) + ")^000000");
        return text;
    }

    string ParseElement(string text)
    {
        text = text.Replace("Ele_Dark", "^C426FF(Dark)^000000");
        text = text.Replace("Ele_Earth", "^C426FF(Earth)^000000");
        text = text.Replace("Ele_Fire", "^C426FF(Fire)^000000");
        text = text.Replace("Ele_Ghost", "^C426FF(Ghost)^000000");
        text = text.Replace("Ele_Holy", "^C426FF(Holy)^000000");
        text = text.Replace("Ele_Neutral", "^C426FF(Neutral)^000000");
        text = text.Replace("Ele_Poison", "^C426FF(Poison)^000000");
        text = text.Replace("Ele_Undead", "^C426FF(Undead)^000000");
        text = text.Replace("Ele_Water", "^C426FF(Water)^000000");
        text = text.Replace("Ele_Wind", "^C426FF(Wind)^000000");
        text = text.Replace("Ele_All", "^C426FF(" + _localization.GetTexts(Localization.ALL_ELEMENT) + ")^000000");
        return text;
    }

    string ParseEffect(string text)
    {
        text = text.Replace("Eff_Bleeding", "^EC1B3ABleeding^000000");
        text = text.Replace("Eff_Blind", "^EC1B3ABlind^000000");
        text = text.Replace("Eff_Burning", "^EC1B3ABurning^000000");
        text = text.Replace("Eff_Confusion", "^EC1B3AConfusion^000000");
        text = text.Replace("Eff_Crystalize", "^EC1B3ACrystalize^000000");
        text = text.Replace("Eff_Curse", "^EC1B3ACurse^000000");
        text = text.Replace("Eff_DPoison", "^EC1B3ADeadly Poison^000000");
        text = text.Replace("Eff_Fear", "^EC1B3AFear^000000");
        text = text.Replace("Eff_Freeze", "^EC1B3AFreeze^000000");
        text = text.Replace("Eff_Freezing", "^EC1B3AFreezing^000000");
        text = text.Replace("Eff_Poison", "^EC1B3APoison^000000");
        text = text.Replace("Eff_Silence", "^EC1B3ASilence^000000");
        text = text.Replace("Eff_Sleep", "^EC1B3ASleep^000000");
        text = text.Replace("Eff_Stone", "^EC1B3AStone^000000");
        text = text.Replace("Eff_Stun", "^EC1B3AStun^000000");
        return text;
    }

    string ParseAtf(string text)
    {
        text = text.Replace("ATF_SELF", _localization.GetTexts(Localization.ATF_SELF));
        text = text.Replace("ATF_TARGET", _localization.GetTexts(Localization.ATF_TARGET));
        text = text.Replace("ATF_SHORT", _localization.GetTexts(Localization.ATF_SHORT));
        text = text.Replace("BF_SHORT", _localization.GetTexts(Localization.BF_SHORT));
        text = text.Replace("ATF_LONG", _localization.GetTexts(Localization.ATF_LONG));
        text = text.Replace("BF_LONG", _localization.GetTexts(Localization.BF_LONG));
        text = text.Replace("ATF_SKILL", _localization.GetTexts(Localization.ATF_SKILL));
        text = text.Replace("ATF_WEAPON", _localization.GetTexts(Localization.ATF_WEAPON));
        text = text.Replace("BF_WEAPON", _localization.GetTexts(Localization.BF_WEAPON));
        text = text.Replace("ATF_MAGIC", _localization.GetTexts(Localization.ATF_MAGIC));
        text = text.Replace("BF_MAGIC", _localization.GetTexts(Localization.BF_MAGIC));
        text = text.Replace("BF_SKILL", _localization.GetTexts(Localization.BF_SKILL));
        text = text.Replace("ATF_MISC", _localization.GetTexts(Localization.ATF_MISC));
        text = text.Replace("BF_MISC", _localization.GetTexts(Localization.BF_MISC));
        text = text.Replace("BF_NORMAL", _localization.GetTexts(Localization.BF_NORMAL));
        return text;
    }

    string GetAllAtf(string text)
    {
        string atf = string.Empty;

        if (text.Contains("ATF_SELF"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_SELF) + ", ";
        if (text.Contains("ATF_TARGET"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_TARGET) + ", ";
        if (text.Contains("ATF_SHORT"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_SHORT) + ", ";
        if (text.Contains("BF_SHORT"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_SHORT) + ", ";
        if (text.Contains("ATF_LONG"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_LONG) + ", ";
        if (text.Contains("BF_LONG"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_LONG) + ", ";
        if (text.Contains("ATF_SKILL"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_SKILL) + ", ";
        if (text.Contains("ATF_WEAPON"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_WEAPON) + ", ";
        if (text.Contains("BF_WEAPON"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_WEAPON) + ", ";
        if (text.Contains("ATF_MAGIC"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_MAGIC) + ", ";
        if (text.Contains("BF_MAGIC"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_MAGIC) + ", ";
        if (text.Contains("BF_SKILL"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_SKILL) + ", ";
        if (text.Contains("ATF_MISC"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_ATF_MISC) + ", ";
        if (text.Contains("BF_MISC"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_MISC) + ", ";
        if (text.Contains("BF_NORMAL"))
            atf += _localization.GetTexts(Localization.AUTO_BONUS_BF_NORMAL) + ", ";

        // Remove leftover ,
        if (!string.IsNullOrEmpty(atf))
            atf = atf.Substring(0, atf.Length - 2);
        else
            atf = _localization.GetTexts(Localization.PHYSICAL_DAMAGE);

        return atf;
    }

    string ParseI(string text)
    {
        text = text.Replace("0", _localization.GetTexts(Localization.AUTOSPELL_I_SELF));
        text = text.Replace("1", _localization.GetTexts(Localization.AUTOSPELL_I_TARGET));
        text = text.Replace("2", _localization.GetTexts(Localization.AUTOSPELL_I_SKILL));
        text = text.Replace("3", _localization.GetTexts(Localization.AUTOSPELL_I_SKILL_TO_TARGET));
        return text;
    }

    string ParseEQI(string text)
    {
        text = text.Replace("EQI_COMPOUND_ON", _localization.GetTexts(Localization.EQUIP_COMPOUND_ON));
        text = text.Replace("EQI_ACC_L", _localization.GetTexts(Localization.LOCATION_LEFT_ACCESSORY));
        text = text.Replace("EQI_ACC_R", _localization.GetTexts(Localization.LOCATION_RIGHT_ACCESSORY));
        text = text.Replace("EQI_SHOES", _localization.GetTexts(Localization.LOCATION_SHOES));
        text = text.Replace("EQI_GARMENT", _localization.GetTexts(Localization.LOCATION_GARMENT));
        text = text.Replace("EQI_HEAD_LOW", _localization.GetTexts(Localization.LOCATION_HEAD_LOW));
        text = text.Replace("EQI_HEAD_MID", _localization.GetTexts(Localization.LOCATION_HEAD_MID));
        text = text.Replace("EQI_HEAD_TOP", _localization.GetTexts(Localization.LOCATION_HEAD_TOP));
        text = text.Replace("EQI_ARMOR", _localization.GetTexts(Localization.LOCATION_ARMOR));
        text = text.Replace("EQI_HAND_L", _localization.GetTexts(Localization.LOCATION_LEFT_HAND));
        text = text.Replace("EQI_HAND_R", _localization.GetTexts(Localization.LOCATION_RIGHT_HAND));
        text = text.Replace("EQI_COSTUME_HEAD_TOP", _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_TOP));
        text = text.Replace("EQI_COSTUME_HEAD_MID", _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_MID));
        text = text.Replace("EQI_COSTUME_HEAD_LOW", _localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_LOW));
        text = text.Replace("EQI_COSTUME_GARMENT", _localization.GetTexts(Localization.LOCATION_COSTUME_GARMENT));
        text = text.Replace("EQI_AMMO", _localization.GetTexts(Localization.LOCATION_AMMO));
        text = text.Replace("EQI_SHADOW_ARMOR", _localization.GetTexts(Localization.LOCATION_SHADOW_ARMOR));
        text = text.Replace("EQI_SHADOW_WEAPON", _localization.GetTexts(Localization.LOCATION_SHADOW_WEAPON));
        text = text.Replace("EQI_SHADOW_SHIELD", _localization.GetTexts(Localization.LOCATION_SHADOW_SHIELD));
        text = text.Replace("EQI_SHADOW_SHOES", _localization.GetTexts(Localization.LOCATION_SHADOW_SHOES));
        text = text.Replace("EQI_SHADOW_ACC_R", _localization.GetTexts(Localization.LOCATION_SHADOW_RIGHT_ACCESSORY));
        text = text.Replace("EQI_SHADOW_ACC_L", _localization.GetTexts(Localization.LOCATION_SHADOW_LEFT_ACCESSORY));
        return text;
    }

    string ParseWeaponType(string text)
    {
        text = text.Replace("W_FIST", _localization.GetTexts(Localization.FIST));
        text = text.Replace("W_DAGGER", "Dagger");
        text = text.Replace("W_1HSWORD", "One-handed Sword");
        text = text.Replace("W_2HSWORD", "Two-handed Sword");
        text = text.Replace("W_1HSPEAR", "One-handed Spear");
        text = text.Replace("W_2HSPEAR", "Two-handed Spear");
        text = text.Replace("W_1HAXE", "One-handed Axe");
        text = text.Replace("W_2HAXE", "Two-handed Axe");
        text = text.Replace("W_MACE", "Mace");
        text = text.Replace("W_2HMACE", "Two-handed Mace");
        text = text.Replace("W_STAFF", "Staff");
        text = text.Replace("W_BOW", "Bow");
        text = text.Replace("W_KNUCKLE", "Knuckle");
        text = text.Replace("W_MUSICAL", "Musical");
        text = text.Replace("W_WHIP", "Whip");
        text = text.Replace("W_BOOK", "Book");
        text = text.Replace("W_KATAR", "Katar");
        text = text.Replace("W_REVOLVER", "Revolver");
        text = text.Replace("W_RIFLE", "Rifle");
        text = text.Replace("W_GATLING", "Gatling");
        text = text.Replace("W_SHOTGUN", "Shotgun");
        text = text.Replace("W_GRENADE", "Grenade Launcher");
        text = text.Replace("W_HUUMA", "Huuma");
        text = text.Replace("W_2HSTAFF", "Two-handed Staff");
        text = text.Replace("W_DOUBLE_DD,", "2 Daggers");
        text = text.Replace("W_DOUBLE_SS,", "2 Swords");
        text = text.Replace("W_DOUBLE_AA,", "2 Axes");
        text = text.Replace("W_DOUBLE_DS,", "Dagger + Sword");
        text = text.Replace("W_DOUBLE_DA,", "Dagger + Axe");
        text = text.Replace("W_DOUBLE_SA,", "Sword + Axe");
        text = text.Replace("W_SHIELD", _localization.GetTexts(Localization.SHIELD));
        return text;
    }

    string AllInOneParse(string text)
    {
        text = text.Replace("else if (", "^FF2525" + _localization.GetTexts(Localization.CONDITION_NOT_MET) + "^000000(");
        text = text.Replace("else if(", "^FF2525" + _localization.GetTexts(Localization.CONDITION_NOT_MET) + "^000000(");
        text = text.Replace("if (", "^FF2525" + _localization.GetTexts(Localization.IF) + "^000000(");
        text = text.Replace("else (", "^FF2525" + _localization.GetTexts(Localization.CONDITION_NOT_MET) + "^000000(");
        text = text.Replace("if(", "^FF2525" + _localization.GetTexts(Localization.IF) + "^000000(");
        text = text.Replace("else(", "^FF2525" + _localization.GetTexts(Localization.CONDITION_NOT_MET) + "^000000(");
        text = text.Replace("||", _localization.GetTexts(Localization.OR));
        text = text.Replace("&&", _localization.GetTexts(Localization.AND));
        text = text.Replace("&", " " + _localization.GetTexts(Localization.WAS) + " ");
        text = text.Replace("BaseLevel", "Level");
        text = text.Replace("BaseJob", _localization.GetTexts(Localization.BASE_JOB));
        text = text.Replace("BaseClass", _localization.GetTexts(Localization.BASE_CLASS));
        text = text.Replace("JobLevel", "Job Level");
        text = text.Replace("readparam", _localization.GetTexts(Localization.READ_PARAM));
        text = text.Replace("bSTR", "STR");
        text = text.Replace("bStr", "STR");
        text = text.Replace("bAGI", "AGI");
        text = text.Replace("bAgi", "AGI");
        text = text.Replace("bVIT", "VIT");
        text = text.Replace("bVit", "VIT");
        text = text.Replace("bINT", "INT");
        text = text.Replace("bInt", "INT");
        text = text.Replace("bDEX", "DEX");
        text = text.Replace("bDex", "DEX");
        text = text.Replace("bLUK", "LUK");
        text = text.Replace("bLuk", "LUK");
        text = text.Replace("eaclass()", "Class");
        text = text.Replace("EAJL_2_1", _localization.GetTexts(Localization.CLASS) + " 2-1");
        text = text.Replace("EAJL_2_2", _localization.GetTexts(Localization.CLASS) + " 2-2");
        text = text.Replace("EAJL_2", _localization.GetTexts(Localization.CLASS) + " 2");
        text = text.Replace("EAJL_UPPER", _localization.GetTexts(Localization.HI_CLASS));
        text = text.Replace("EAJL_BABY", _localization.GetTexts(Localization.CLASS) + " Baby");
        text = text.Replace("EAJL_THIRD", _localization.GetTexts(Localization.CLASS) + " 3");
        text = text.Replace("EAJ_BASEMASK", _localization.GetTexts(Localization.CLASS) + " 1");
        text = text.Replace("EAJ_UPPERMASK", _localization.GetTexts(Localization.HI_CLASS));
        text = text.Replace("EAJ_THIRDMASK", _localization.GetTexts(Localization.CLASS) + " 3");
        text = text.Replace("PETINFO_ID", "Pet");
        text = text.Replace("PETINFO_INTIMATE", _localization.GetTexts(Localization.PET_INFO_INTIMATE));
        text = text.Replace("PET_INTIMATE_LOYAL", _localization.GetTexts(Localization.PET_INTIMATE_LOYAL));
        text = text.Replace("getpetinfo(", "(");
        text = text.Replace("getiteminfo(", "(");
        text = text.Replace("getequipid(", " Item ");
        text = text.Replace("getequiprefinerycnt", _localization.GetTexts(Localization.REFINE_COUNT));
        text = text.Replace("getenchantgrade()", _localization.GetTexts(Localization.GRADE_COUNT));
        text = text.Replace("getenchantgrade", _localization.GetTexts(Localization.GRADE_COUNT));
        text = text.Replace("getrefine()", _localization.GetTexts(Localization.REFINE_COUNT));
        text = text.Replace("getequipweaponlv()", _localization.GetTexts(Localization.GET_WEAPON_LEVEL));
        text = text.Replace("getequipweaponlv", _localization.GetTexts(Localization.GET_WEAPON_LEVEL));
        text = text.Replace("getequiparmorlv()", _localization.GetTexts(Localization.GET_EQUIPMENT_LEVEL));
        text = text.Replace("getequiparmorlv", _localization.GetTexts(Localization.GET_EQUIPMENT_LEVEL));
        text = text.Replace("ismounting()", _localization.GetTexts(Localization.IF_MOUNTING));
        text = text.Replace("getskilllv", "Lv. Skill");
        text = text.Replace("pow (", _localization.GetTexts(Localization.POW) + "(");
        text = text.Replace("pow(", _localization.GetTexts(Localization.POW) + "(");
        text = text.Replace("min (", _localization.GetTexts(Localization.MIN) + "(");
        text = text.Replace("min(", _localization.GetTexts(Localization.MIN) + "(");
        text = text.Replace("max (", _localization.GetTexts(Localization.MAX) + "(");
        text = text.Replace("max(", _localization.GetTexts(Localization.MAX) + "(");
        text = text.Replace("rand (", _localization.GetTexts(Localization.RAND) + "(");
        text = text.Replace("rand(", _localization.GetTexts(Localization.RAND) + "(");
        text = text.Replace("==", _localization.GetTexts(Localization.EQUAL));
        text = text.Replace("!=", " " + _localization.GetTexts(Localization.NOT_EQUAL) + " ");
        text = text.Replace("JOB_", string.Empty);
        text = text.Replace("Job_", string.Empty);
        text = text.Replace("job_", string.Empty);
        text = text.Replace("EAJ_", string.Empty);
        text = text.Replace("eaj_", string.Empty);
        text = text.Replace("SC_", string.Empty);
        text = text.Replace("sc_", string.Empty);
        text = text.Replace("else", "^FF2525" + _localization.GetTexts(Localization.CONDITION_NOT_MET) + "^000000");
        text = text.Replace(" ? ", " " + _localization.GetTexts(Localization.WILL_BE) + " ");
        text = text.Replace("?", " " + _localization.GetTexts(Localization.WILL_BE) + " ");
        text = text.Replace(" : ", " " + _localization.GetTexts(Localization.IF_NOT) + " ");
        text = text.Replace(":", " " + _localization.GetTexts(Localization.IF_NOT) + " ");

        text = ParseWeaponType(text);
        text = ParseEQI(text);

        // Some normal function contains skill name, let's try parse them
        while (text.Contains("\""))
        {
            // Try to find first "
            var firstQuoteIndex = text.IndexOf("\"");

            if ((firstQuoteIndex > 0)
                && (text.Length > (firstQuoteIndex + 1)))
            {
                // Substract text
                var subText = text.Substring(firstQuoteIndex + 1);

                // Try to find second "
                var secondQuoteIndex = subText.IndexOf("\"");

                // Good! Found 2 quote
                if (secondQuoteIndex > 0)
                {
                    var finalSubText = text.Substring(firstQuoteIndex + 1, secondQuoteIndex);

                    var skillName = GetSkillName(finalSubText, true, true);

                    text = text.Replace("\"" + finalSubText + "\"", skillName);
                }
                // Unexpected error (Had 1 quote)
                else
                    break;
            }
            // Unexpected error (Maybe " stay on last index of string and had 1 quote?)
            else
                break;
        }

        text = QuoteRemover.Remove(text);

        return text;
    }

    string TryParseInt(string text, float divider = 1, int defaultValue = 0)
    {
        if (text == "INFINITE_TICK")
            return _localization.GetTexts(Localization.INFINITE);

        float sum = -1;

        if (float.TryParse(text, out sum))
            sum = float.Parse(text);
        else if (divider == 1)
        {
            if (string.IsNullOrEmpty(text)
                || string.IsNullOrWhiteSpace(text))
                return defaultValue.ToString("f0");
            else
                return text;
        }
        else
            return text + " ^FF0000" + _localization.GetTexts(Localization.DIVIDE) + " " + divider.ToString("f0") + "^000000";

        return ParseNumberDecimal(sum, divider);
    }

    string ParseNumberDecimal(float number, float divider)
    {
        if ((number / divider) == 0)
            return (number / divider).ToString("f0");
        else if (((number / divider) > -0.1f)
            && ((number / divider) < 0.1f))
            return (number / divider).ToString("f2");
        else if (((number / divider) % 1) != 0)
            return (number / divider).ToString("f1");
        else
            return (number / divider).ToString("f0");
    }

    string GetCombo(string aegisName)
    {
        StringBuilder builder = new StringBuilder();

        // Loop all combo data
        for (int i = 0; i < _comboDatabases.Count; i++)
        {
            var currentComboData = _comboDatabases[i];

            // Found
            if (currentComboData.IsAegisNameContain(aegisName))
            {
                StringBuilder sum = new StringBuilder();

                bool isFoundNow = false;

                for (int j = 0; j < currentComboData.sameComboDatas.Count; j++)
                {
                    var currentSameComboData = currentComboData.sameComboDatas[j];

                    // Add item name
                    for (int k = 0; k < currentSameComboData.aegisNames.Count; k++)
                    {
                        if (currentSameComboData.aegisNames[k] == aegisName)
                            isFoundNow = true;
                    }

                    if (isFoundNow)
                    {
                        // Declare header
                        var same_set_name_list = "			\"^666478" + _localization.GetTexts(Localization.EQUIP_WITH);

                        // Add item name
                        for (int k = 0; k < currentSameComboData.aegisNames.Count; k++)
                        {
                            var currentAegisName = currentSameComboData.aegisNames[k];

                            // Should not add base item name
                            if (currentAegisName == aegisName)
                                continue;
                            else
                            {
                                var itemId = GetItemIdFromAegisName(currentAegisName);

                                same_set_name_list += "[NEW_LINE]+ " + GetItemName(itemId.ToString("f0"));
                                same_set_name_list += "[NEW_LINE]+ (ID:" + itemId + ")";
                            }
                        }

                        // End
                        same_set_name_list += "^000000\",\n";

                        sum.Append(same_set_name_list);

                        // Add combo bonus description
                        for (int l = 0; l < currentComboData.descriptions.Count; l++)
                        {
                            if (l >= currentComboData.descriptions.Count - 1)
                                sum.Append(currentComboData.descriptions[l]);
                            else
                                sum.Append(currentComboData.descriptions[l] + "\n");
                        }

                        // End
                        sum.Append("			\"————————————\",\n");

                        isFoundNow = false;
                    }
                }

                // Finalize this combo data
                builder.Append(sum);
            }
        }

        return builder.ToString();
    }

    string GetItemName(string text)
    {
        int _int = 0;

        if (int.TryParse(text, out _int))
        {
            _int = int.Parse(text);

            if (_itemDatabases.ContainsKey(_int))
                return _itemDatabases[_int].name;
        }
        else
        {
            var itemId = 0;
            if (_aegisNameDatabases.ContainsKey(text))
            {
                itemId = _aegisNameDatabases[text];

                if (_itemDatabases.ContainsKey(itemId))
                    return _itemDatabases[itemId].name;
            }
        }

        return text;
    }

    int GetItemIdFromAegisName(string text)
    {
        text = SpacingRemover.Remove(text);

        if (_aegisNameDatabases.ContainsKey(text))
            return _aegisNameDatabases[text];
        else
        {
            Debug.LogWarning(text + " not found in aegisNameDatabases");

            return 0;
        }
    }

    string GetResourceNameFromId(int id, string type, string subType, string location)
    {
        if (_isRandomResourceNameForCustomTextAssetOnly)
        {
            if (_isRandomResourceName && id >= ItemGenerator.START_ID)
            {
                if (subType.ToLower().Contains("dagger"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.daggers[UnityEngine.Random.Range(0, _resourceContainer.daggers.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.daggers[UnityEngine.Random.Range(0, _resourceContainer.daggers.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("1hsword"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedSwords[UnityEngine.Random.Range(0, _resourceContainer.oneHandedSwords.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedSwords[UnityEngine.Random.Range(0, _resourceContainer.oneHandedSwords.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("2hsword"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedSwords[UnityEngine.Random.Range(0, _resourceContainer.twoHandedSwords.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedSwords[UnityEngine.Random.Range(0, _resourceContainer.twoHandedSwords.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("1hspear"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedSpears[UnityEngine.Random.Range(0, _resourceContainer.oneHandedSpears.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedSpears[UnityEngine.Random.Range(0, _resourceContainer.oneHandedSpears.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("2hspear"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedSpears[UnityEngine.Random.Range(0, _resourceContainer.twoHandedSpears.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedSpears[UnityEngine.Random.Range(0, _resourceContainer.twoHandedSpears.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("1haxe"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedAxes[UnityEngine.Random.Range(0, _resourceContainer.oneHandedAxes.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedAxes[UnityEngine.Random.Range(0, _resourceContainer.oneHandedAxes.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("2haxe"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedAxes[UnityEngine.Random.Range(0, _resourceContainer.twoHandedAxes.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedAxes[UnityEngine.Random.Range(0, _resourceContainer.twoHandedAxes.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("mace"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedMaces[UnityEngine.Random.Range(0, _resourceContainer.oneHandedMaces.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedMaces[UnityEngine.Random.Range(0, _resourceContainer.oneHandedMaces.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("2hmace"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedMaces[UnityEngine.Random.Range(0, _resourceContainer.twoHandedMaces.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedMaces[UnityEngine.Random.Range(0, _resourceContainer.twoHandedMaces.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("staff"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedStaffs[UnityEngine.Random.Range(0, _resourceContainer.oneHandedStaffs.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.oneHandedStaffs[UnityEngine.Random.Range(0, _resourceContainer.oneHandedStaffs.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("2hstaff"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedStaffs[UnityEngine.Random.Range(0, _resourceContainer.twoHandedStaffs.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.twoHandedStaffs[UnityEngine.Random.Range(0, _resourceContainer.twoHandedStaffs.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("bow"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.bows[UnityEngine.Random.Range(0, _resourceContainer.bows.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.bows[UnityEngine.Random.Range(0, _resourceContainer.bows.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("knuckle"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.knuckles[UnityEngine.Random.Range(0, _resourceContainer.knuckles.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.knuckles[UnityEngine.Random.Range(0, _resourceContainer.knuckles.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("musical"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.musicals[UnityEngine.Random.Range(0, _resourceContainer.musicals.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.musicals[UnityEngine.Random.Range(0, _resourceContainer.musicals.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("whip"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.whips[UnityEngine.Random.Range(0, _resourceContainer.whips.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.whips[UnityEngine.Random.Range(0, _resourceContainer.whips.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("book"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.books[UnityEngine.Random.Range(0, _resourceContainer.books.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.books[UnityEngine.Random.Range(0, _resourceContainer.books.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("katar"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.katars[UnityEngine.Random.Range(0, _resourceContainer.katars.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.katars[UnityEngine.Random.Range(0, _resourceContainer.katars.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("revolver"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.revolvers[UnityEngine.Random.Range(0, _resourceContainer.revolvers.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.revolvers[UnityEngine.Random.Range(0, _resourceContainer.revolvers.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("rifle"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.rifles[UnityEngine.Random.Range(0, _resourceContainer.rifles.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.rifles[UnityEngine.Random.Range(0, _resourceContainer.rifles.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("gatling"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.gatlings[UnityEngine.Random.Range(0, _resourceContainer.gatlings.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.gatlings[UnityEngine.Random.Range(0, _resourceContainer.gatlings.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("shotgun"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.shotguns[UnityEngine.Random.Range(0, _resourceContainer.shotguns.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.shotguns[UnityEngine.Random.Range(0, _resourceContainer.shotguns.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("grenade"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.grenades[UnityEngine.Random.Range(0, _resourceContainer.grenades.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.grenades[UnityEngine.Random.Range(0, _resourceContainer.grenades.Count)]), null, null, null);
                    return s;
                }
                else if (subType.ToLower().Contains("huuma"))
                {
                    var s = GetResourceNameFromId(int.Parse(_resourceContainer.huumas[UnityEngine.Random.Range(0, _resourceContainer.huumas.Count)]), null, null, null);
                    while (s == "\"Bio_Reseearch_Docu\"")
                        s = GetResourceNameFromId(int.Parse(_resourceContainer.huumas[UnityEngine.Random.Range(0, _resourceContainer.huumas.Count)]), null, null, null);
                    return s;
                }

                if (type.ToLower() == "armor")
                {
                    if (location == _localization.GetTexts(Localization.LOCATION_HEAD_TOP))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.topHeadgears[UnityEngine.Random.Range(0, _resourceContainer.topHeadgears.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.topHeadgears[UnityEngine.Random.Range(0, _resourceContainer.topHeadgears.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_HEAD_MID))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.middleHeadgears[UnityEngine.Random.Range(0, _resourceContainer.middleHeadgears.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.middleHeadgears[UnityEngine.Random.Range(0, _resourceContainer.middleHeadgears.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_HEAD_LOW))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.lowerHeadgears[UnityEngine.Random.Range(0, _resourceContainer.lowerHeadgears.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.lowerHeadgears[UnityEngine.Random.Range(0, _resourceContainer.lowerHeadgears.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_ARMOR))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.armors[UnityEngine.Random.Range(0, _resourceContainer.armors.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.armors[UnityEngine.Random.Range(0, _resourceContainer.armors.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_GARMENT))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.garments[UnityEngine.Random.Range(0, _resourceContainer.garments.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.garments[UnityEngine.Random.Range(0, _resourceContainer.garments.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_SHOES))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.shoes[UnityEngine.Random.Range(0, _resourceContainer.shoes.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.shoes[UnityEngine.Random.Range(0, _resourceContainer.shoes.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_LEFT_ACCESSORY) || location == _localization.GetTexts(Localization.LOCATION_RIGHT_ACCESSORY) || location == _localization.GetTexts(Localization.LOCATION_BOTH_ACCESSORY))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.accessorys[UnityEngine.Random.Range(0, _resourceContainer.accessorys.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.accessorys[UnityEngine.Random.Range(0, _resourceContainer.accessorys.Count)]), null, null, null);
                        return s;
                    }
                    else if (location == _localization.GetTexts(Localization.LOCATION_LEFT_HAND))
                    {
                        var s = GetResourceNameFromId(int.Parse(_resourceContainer.shields[UnityEngine.Random.Range(0, _resourceContainer.shields.Count)]), null, null, null);
                        while (s == "\"Bio_Reseearch_Docu\"")
                            s = GetResourceNameFromId(int.Parse(_resourceContainer.shields[UnityEngine.Random.Range(0, _resourceContainer.shields.Count)]), null, null, null);
                        return s;
                    }
                }
                List<int> keys = new List<int>(_resourceDatabases.Keys);
                return _resourceDatabases[keys[UnityEngine.Random.Range(0, keys.Count)]];
            }
        }
        else
        {
            if (_isRandomResourceName)
            {
                List<int> keys = new List<int>(_resourceDatabases.Keys);
                return _resourceDatabases[keys[UnityEngine.Random.Range(0, keys.Count)]];
            }
        }

        if (_resourceDatabases.ContainsKey(id))
            return _resourceDatabases[id];

        return "\"Bio_Reseearch_Docu\"";
    }

    string GetSkillName(string text, bool isKeepSpacebar = false, bool isOnlyString = false)
    {
        int _int = 0;

        if (int.TryParse(text, out _int)
            && !isOnlyString)
        {
            _int = int.Parse(text);

            if (_skillDatabases.ContainsKey(_int))
                return "^990B0B" + _skillDatabases[_int].description + "^000000";
        }
        else
        {
            if (!isKeepSpacebar)
                text = SpacingRemover.Remove(text);

            text = text.Replace(";", string.Empty);

            if (_skillNameDatabases.ContainsKey(text))
            {
                if (_skillDatabases.ContainsKey(_skillNameDatabases[text]))
                    return "^990B0B" + _skillDatabases[_skillNameDatabases[text]].description + "^000000";
            }
        }

        return text;
    }

    string GetClassNumFromId(ItemContainer itemContainer)
    {
        if (_classNumberDatabases.ContainsKey(int.Parse(itemContainer.id)))
            return _classNumberDatabases[int.Parse(itemContainer.id)];
        // Default view for weapons
        else if (!string.IsNullOrEmpty(itemContainer.subType))
        {
            if (itemContainer.subType == "Fist")
                return "0";
            else if (itemContainer.subType == "Dagger")
                return "1";
            else if (itemContainer.subType == "1hSword")
                return "2";
            else if (itemContainer.subType == "2hSword")
                return "3";
            else if (itemContainer.subType == "1hSpear")
                return "4";
            else if (itemContainer.subType == "2hSpear")
                return "5";
            else if (itemContainer.subType == "1hAxe")
                return "6";
            else if (itemContainer.subType == "2hAxe")
                return "7";
            else if (itemContainer.subType == "Mace")
                return "8";
            else if (itemContainer.subType == "Staff")
                return "10";
            else if (itemContainer.subType == "Bow")
                return "11";
            else if (itemContainer.subType == "Knuckle")
                return "12";
            else if (itemContainer.subType == "Musical")
                return "13";
            else if (itemContainer.subType == "Whip")
                return "14";
            else if (itemContainer.subType == "Book")
                return "15";
            else if (itemContainer.subType == "Katar")
                return "16";
            else if (itemContainer.subType == "Revolver")
                return "17";
            else if (itemContainer.subType == "Rifle")
                return "18";
            else if (itemContainer.subType == "Gatling")
                return "19";
            else if (itemContainer.subType == "Shotgun")
                return "20";
            else if (itemContainer.subType == "Grenade")
                return "21";
            else if (itemContainer.subType == "Huuma")
                return "22";
            else if (itemContainer.subType == "2hStaff")
                return "23";
            else
                return "0";
        }
        // Default view for shields
        else if (!string.IsNullOrEmpty(itemContainer.type)
            && string.IsNullOrEmpty(itemContainer.locations)
            && (itemContainer.locations == _localization.GetTexts(Localization.LOCATION_LEFT_HAND)))
            return "1";
        else
            return "0";
    }

    string IsCostumeFromId(ItemContainer itemContainer)
    {
        if (itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_TOP))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_MID))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_COSTUME_HEAD_LOW))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_ARMOR))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_WEAPON))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_SHIELD))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_COSTUME_GARMENT))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_SHOES))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_LEFT_ACCESSORY))
            || itemContainer.locations.Contains(_localization.GetTexts(Localization.LOCATION_SHADOW_RIGHT_ACCESSORY)))
            return "true";
        else
            return "false";
    }

    /// <summary>
    /// Get monster database by ID
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    MonsterDatabase GetMonsterDatabase(string text)
    {
        int id = 0;

        if (int.TryParse(text, out id))
        {
            id = int.Parse(text);

            if (_monsterDatabases.ContainsKey(id))
                return _monsterDatabases[id];
        }

        return null;
    }

    /// <summary>
    /// Get item {name, aegis name} database by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ItemDatabase GetItemDatabase(int id)
    {
        if (_itemDatabases.ContainsKey(id))
            return _itemDatabases[id];
        else
            return null;
    }

    /// <summary>
    /// Parse usable items that contains sc_start bonuses into item list
    /// </summary>
    void ParseStatusChangeStartIntoItemId()
    {
        if (!string.IsNullOrEmpty(_itemContainer.id)
            && !string.IsNullOrEmpty(_itemContainer.type))
        {
            if (!_itemListContainer.buffItemIds.Contains(_itemContainer.id)
                && ((_itemContainer.type.ToLower() == "healing")
                || (_itemContainer.type.ToLower() == "usable")
                || (_itemContainer.type.ToLower() == "cash")))
                _itemListContainer.buffItemIds.Add(_itemContainer.id);
        }
    }
}
