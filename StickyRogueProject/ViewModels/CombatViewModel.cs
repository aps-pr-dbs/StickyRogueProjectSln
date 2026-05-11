using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using StickyRogueProject.Views;
using System.Text.Json;

namespace StickyRogueProject.ViewModels;

public partial class CombatViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly HistoryService _historyService;
    private readonly Random _rng = new();

    public Func<Task>? OpenInventoryPopup { get; set; }
    public Func<Task>? OpenPlayerStatusPopup { get; set; }
    public Func<Task>? OpenEnemyStatusPopup { get; set; }
    public Func<string, string, Task>? ShowAlert { get; set; }
    public Func<string, string, string, string, Task<bool>>? ShowConfirm { get; set; }

    private ActiveSave? _save;
    private Enemy? _enemy;

    [ObservableProperty] private int _currentWave = 1;
    [ObservableProperty] private int _currentLoop = 1;
    [ObservableProperty] private string _waveLabel = "Enemy 1 / 10";

    [ObservableProperty] private string _enemyName = string.Empty;
    [ObservableProperty] private int _enemyLevel = 1;
    [ObservableProperty] private string _enemyHpText = "0/0";
    [ObservableProperty] private double _enemyHpProgress = 1.0;
    [ObservableProperty] private string _enemyImageSource = string.Empty;

    [ObservableProperty] private string _characterName = string.Empty;
    [ObservableProperty] private int _characterLevel = 1;
    [ObservableProperty] private string _characterHpText = "0/0";
    [ObservableProperty] private double _characterHpProgress = 1.0;
    [ObservableProperty] private string _characterMpText = "0/0";
    [ObservableProperty] private double _characterMpProgress = 1.0;
    [ObservableProperty] private string _characterImageSource = "player.png";

    [ObservableProperty] private string _xpDisplay = "0 / 100";
    [ObservableProperty] private double _characterXpProgress = 0.0;
    [ObservableProperty] private int _currentXp = 0;
    [ObservableProperty] private int _xpToNextLevel = 100;

    [ObservableProperty] private int _coins;
    [ObservableProperty] private int _hpPotionCount;
    [ObservableProperty] private int _mpPotionCount;

    [ObservableProperty] private string _combatLog = "Combat Start!";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AttackCommand))]
    [NotifyCanExecuteChangedFor(nameof(DefendCommand))]
    [NotifyCanExecuteChangedFor(nameof(MagicCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(UseItemCommand))]
    private bool _isBusy;

    private bool _isDefending = false;

    public CombatViewModel(SaveService saveService, HistoryService historyService)
    {
        _saveService = saveService;
        _historyService = historyService;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            _save = await _saveService.LoadSaveAsync();
            if (_save is null)
            {
                await SafeAlert("ข้อผิดพลาด", "ไม่พบ Save กรุณาเริ่มเกมใหม่");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new AppShell();
                });
                return;
            }

            CurrentWave = _save.CurrentWave;
            CurrentLoop = _save.CurrentLoop;

            RefreshPlayerUi();
            SpawnEnemyForWave(CurrentWave);
            UpdateWaveLabel();
            AppendLog($"Wave {CurrentWave} — {EnemyName} ปรากฏตัว!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CombatViewModel] Init ล้มเหลว: {ex.Message}");
            await SafeAlert("ข้อผิดพลาด", "โหลดข้อมูลไม่สำเร็จ");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task AttackAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            double variation = 0.8 + _rng.NextDouble() * 0.4;
            int rawDmg = (int)(_save.Atk * variation);
            int finalDmg = _enemy.TakeDamage(rawDmg);

            AppendLog($"⚔️ คุณโจมตี {EnemyName} {finalDmg} ความเสียหาย!");
            RefreshEnemyUi();

            if (_enemy.IsDefeated) { await OnEnemyDefeatedAsync(); return; }
            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task DefendAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            _isDefending = true;
            AppendLog("🛡️ คุณรับท่าไว้! ความเสียหายรอบนี้ลดลงครึ่งหนึ่ง");
            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task MagicAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            const int mpCost = 10;
            if (_save.CurrentMp < mpCost)
            {
                AppendLog("✨ MP ไม่พอ! ต้องการ 10 MP");
                await DoEnemyTurnAsync();
                return;
            }

            _save.CurrentMp = Math.Max(0, _save.CurrentMp - mpCost);
            double variation = 0.85 + _rng.NextDouble() * 0.3;
            int magicDmg = (int)(_save.Int * variation);
            _enemy.CurrentHp = Math.Max(0, _enemy.CurrentHp - magicDmg);

            AppendLog($"✨ เวทย์ถล่ม {EnemyName} {magicDmg} ความเสียหาย! (ทะลุ DEF)");
            RefreshPlayerUi();
            RefreshEnemyUi();

            if (_enemy.IsDefeated) { await OnEnemyDefeatedAsync(); return; }
            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task RunAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            bool escaped = _rng.NextDouble() < 0.25;

            if (escaped)
            {
                AppendLog("🏃 หลบหนีสำเร็จ!");
                await AdvanceToNextWaveAsync(skipReward: true);
                return;
            }

            AppendLog("❌ หลบหนีไม่ได้!");
            await DoEnemyTurnAsync(runFailPenalty: true);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task UseItemAsync(string? potionType)
    {
        if (_save is null) return;
        try
        {
            IsBusy = true;
            if (potionType == "HP")
            {
                if (_save.HpPotionCount <= 0)
                {
                    AppendLog("💊 ไม่มี HP Potion เหลือ!");
                    return;
                }
                int heal = (int)(_save.MaxHp * 0.5);
                _save.HpPotionCount--;
                _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp + heal);
                AppendLog($"💊 ใช้ HP Potion! ฮีล {heal} HP");
            }
            else if (potionType == "MP")
            {
                if (_save.MpPotionCount <= 0)
                {
                    AppendLog("🧪 ไม่มี MP Potion เหลือ!");
                    return;
                }
                int restore = (int)(_save.MaxMp * 0.5);
                _save.MpPotionCount--;
                _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp + restore);
                AppendLog($"🧪 ใช้ MP Potion! เติม {restore} MP");
            }

            RefreshPlayerUi();
            await _saveService.UpdateSaveAsync(_save);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UseItem] Error: {ex.Message}");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task OpenInventoryAsync()
    {
        if (OpenInventoryPopup is not null)
            await OpenInventoryPopup();
    }

    [RelayCommand]
    private async Task ShowPlayerStatusAsync()
    {
        if (OpenPlayerStatusPopup is not null)
            await OpenPlayerStatusPopup();
    }

    [RelayCommand]
    private async Task ShowEnemyStatusAsync()
    {
        if (OpenEnemyStatusPopup is not null)
            await OpenEnemyStatusPopup();
    }

    private async Task DoEnemyTurnAsync(bool runFailPenalty = false)
    {
        if (_save is null || _enemy is null) return;

        await Task.Delay(300);

        int rawDmg = _enemy.CalculateAttack();
        if (runFailPenalty) rawDmg = (int)(rawDmg * 1.15);

        int afterDef = Math.Max(1, rawDmg - _save.Def);

        bool wasDefending = _isDefending;
        _isDefending = false;
        int finalDmg = wasDefending
            ? (int)Math.Ceiling(afterDef / 2.0)
            : afterDef;

        _save.CurrentHp = Math.Max(0, _save.CurrentHp - finalDmg);

        string penaltyNote = runFailPenalty ? " (+15% โทษหลบหนี)" : string.Empty;
        string defendNote = wasDefending ? " (ลดครึ่งจาก Defend)" : string.Empty;
        AppendLog($"💀 {EnemyName} โจมตี {finalDmg} ความเสียหาย!{penaltyNote}{defendNote}");

        RefreshPlayerUi();

        if (_save.CurrentHp <= 0)
            await OnPlayerDefeatedAsync();
    }

    private async Task OnEnemyDefeatedAsync()
    {
        if (_save is null || _enemy is null) return;

        _save.Coins += _enemy.CoinReward;
        GainXp(_enemy.XpReward * 3);

        AppendLog($"✅ ชนะ! +{_enemy.XpReward} XP  +{_enemy.CoinReward * 0.5} เหรียญ");

        if (_rng.NextDouble() < 0.50)
        {
            if (_rng.NextDouble() < 0.50)
            {
                _save.HpPotionCount++;
                AppendLog("💊 ได้รับ HP Potion!");
            }
            else
            {
                _save.MpPotionCount++;
                AppendLog("🧪 ได้รับ MP Potion!");
            }
        }

        // 10% chance to drop ArtifactItem
        if (_rng.NextDouble() < 0.10)
        {
            var artifact = GetRandomArtifact();
            if (artifact != null)
            {
                _save.Artifacts.Add(artifact);
                AppendLog($"✨ ได้รับ Artifact: {artifact.Name}!");
            }
        }

        _save.CurrentWave = CurrentWave;
        _save.CurrentLoop = CurrentLoop;
        await _saveService.UpdateSaveAsync(_save);

        RefreshPlayerUi();

        await AdvanceToNextWaveAsync(skipReward: false);
    }

    private async Task AdvanceToNextWaveAsync(bool skipReward)
    {
        if (_save is null) return;

        if (CurrentWave == 5 && !skipReward)
        {
            await HandleWave5EventAsync();
            return;
        }

        if (CurrentWave == 9 && !skipReward)
        {
            AppendLog("🏪 ทะลุ Wave 9! เข้าร้านค้า...");
            await SafeAlert("Wave 9 ผ่านแล้ว!", "แวะร้านค้าเพื่อเตรียมพร้อมก่อน Boss!");
            CurrentWave = 10;
            _save.CurrentWave = 10;
            await _saveService.UpdateSaveAsync(_save);
            await Shell.Current.GoToAsync("ShopPage");
            SpawnEnemyForWave(10);
            return;
        }

        if (CurrentWave == 10 && !skipReward)
        {
            // ── ด่านที่ 100 = Loop 10, Wave 10 → Game Clear ──
            if (CurrentLoop >= 10)
            {
                AppendLog("🏆 ราชาปีศาจพ่ายแพ้! คุณพิชิตทุกด่านแล้ว!");
                await Task.Delay(800);

                try
                {
                    _save!.CurrentWave = 10;
                    _save.CurrentLoop = 10;
                    await _saveService.UpdateSaveAsync(_save);
                    await _historyService.SaveRunHistoryAsync(_save, causeOfDeath: "GAME CLEAR!");
                    await _saveService.DeleteSaveAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GameClear] Save error: {ex.Message}");
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var vm = new GameClearViewModel(_save!.ClassName, _save.Level, _save.Coins);
                    var page = new GameClearPage(vm);
                    Application.Current.MainPage = page;
                });
                return;
            }

            // Loop ปกติ → ไป Church
            AppendLog("👑 Boss พ่ายแพ้! ผ่าน Loop นี้แล้ว!");
            await SafeAlert("👑 Boss Defeated!", $"Loop {CurrentLoop} สำเร็จ!\nมุ่งหน้าสู่ Church...");
            await Shell.Current.GoToAsync("ChurchPage");
            return;
        }

        CurrentWave++;
        if (CurrentWave > 10) CurrentWave = 10;
        _save.CurrentWave = CurrentWave;
        await _saveService.UpdateSaveAsync(_save);

        SpawnEnemyForWave(CurrentWave);
        UpdateWaveLabel();
        AppendLog($"── Wave {CurrentWave} เริ่มต้น! {EnemyName} ปรากฏตัว! ──");
    }

    // ── Wave 5 Event Handler (อัปเดต 50/50 Casino) ──────────
    private async Task HandleWave5EventAsync()
    {
        double roll = _rng.NextDouble();

        if (roll < 0.15) // 15% Casino
        {
            AppendLog("🎰 โชคชะตาพาไป Casino!");

            // ⚡ เปลี่ยนมาเรียกใช้ Popup สวยๆ แทน Alert เดิม ⚡
            var casinoPopup = new Views.PopUp.CasinoEventPopUpPage();
            await App.Current.MainPage.ShowPopupAsync(casinoPopup);

            CurrentWave = 6;
            _save!.CurrentWave = 6;
            await _saveService.UpdateSaveAsync(_save);
            await Shell.Current.GoToAsync("CasinoMenu");
            SpawnEnemyForWave(6);
            UpdateWaveLabel();
        }
        else // 50% Random Event Card// 50% Random Event Card // 50% Random Event Card
        {
            var gameEvent = EventPool.GetRandomEvent();
            AppendLog($"📜 เหตุการณ์บังคับ: {gameEvent.Title}");

            var popup = new Views.PopUp.RandomEventPopUpPage(gameEvent);
            await App.Current.MainPage.ShowPopupAsync(popup);

            string action = popup.ResultAction;

            if (action == "Escaped")
            {
                AppendLog("🏃 หนีรอดจากพ่อค้าเถื่อนสำเร็จ!");
            }
            else if (action == "DealerWin")
            {
                _save!.Atk += 20; _save.Def += 20; _save.Int += 20;
                _save.MaxHp += 50; _save.MaxMp += 50;
                _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp + 50);
                _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp + 50);
                AppendLog("💊 ATK, DEF, INT +20 และ MAX HP, MAX MP +50!");
            }
            else if (action == "DealerLose")
            {
                _save!.Atk = Math.Max(1, _save.Atk - 10);
                _save.Def = Math.Max(0, _save.Def - 10);
                _save.Int = Math.Max(1, _save.Int - 10);
                _save.MaxHp = Math.Max(10, _save.MaxHp - 20);
                _save.MaxMp = Math.Max(0, _save.MaxMp - 20);
                _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp);
                _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp);
                AppendLog("💀 โดนหลอก! ATK, DEF, INT -10 และ MAX HP, MAX MP -20!");
            }
            else if (action == "Normal")
            {
                ApplyNormalEvent(gameEvent);
            }

            CurrentWave = 6;
            _save!.CurrentWave = 6;
            await _saveService.UpdateSaveAsync(_save);
            SpawnEnemyForWave(6);
            UpdateWaveLabel();
            RefreshPlayerUi();
            AppendLog($"── Wave 6 เริ่มต้น! {EnemyName} ปรากฏตัว! ──");
        }
    }

    private void ApplyNormalEvent(GameEvent ev)
    {
        if (_save is null) return;

        switch (ev.EffectType)
        {
            case EventEffectType.None:
                AppendLog("💨 โชคไม่ดีเลย ไม่มีอะไรเกิดขึ้น...");
                break;
            case EventEffectType.GainCoins:
                _save.Coins += ev.Value;
                AppendLog($"💰 ได้รับ {ev.Value} เหรียญ");
                break;
            case EventEffectType.LoseCoins:
                _save.Coins = Math.Max(0, _save.Coins - ev.Value);
                AppendLog($"💸 โดนขโมยไป {ev.Value} เหรียญ");
                break;
            case EventEffectType.HealHalfHp:
                int heal = (int)(_save.MaxHp * 0.5);
                _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp + heal);

                int healMp = (int)(_save.MaxMp * 0.5);
                _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp + healMp);

                AppendLog($"❤️ แช่ออนเซ็น ฟื้นฟู {heal} HP และ {healMp} MP");
                break;
            case EventEffectType.LoseHp:
                _save.CurrentHp -= ev.Value;
                AppendLog($"💥 โดนกับดัก เสีย {ev.Value} HP");
                if (_save.CurrentHp <= 0) _save.CurrentHp = 1;
                break;
            case EventEffectType.GainStat:
                ApplyStatChange(ev.StatType, ev.Value);
                AppendLog($"✨ พลัง {ev.StatType} +{ev.Value}");
                break;
            case EventEffectType.LoseStat:
                ApplyStatChange(ev.StatType, -ev.Value);
                AppendLog($"💀 พลัง {ev.StatType} -{ev.Value}");
                break;
        }
    }

    private void ApplyStatChange(string statType, int amount)
    {
        switch (statType)
        {
            case "ATK": _save!.Atk = Math.Max(1, _save.Atk + amount); break;
            case "INT": _save!.Int = Math.Max(1, _save.Int + amount); break;
            case "MAX HP":
                _save!.MaxHp = Math.Max(10, _save.MaxHp + amount);
                _save.CurrentHp = Math.Min(_save.MaxHp, _save.CurrentHp);
                break;
            case "MAX MP":
                _save!.MaxMp = Math.Max(0, _save.MaxMp + amount);
                _save.CurrentMp = Math.Min(_save.MaxMp, _save.CurrentMp);
                break;
        }
    }

    private async Task OnPlayerDefeatedAsync()
    {
        if (_save is null) return;

        AppendLog("💀 คุณพ่ายแพ้...");
        await Task.Delay(1000);

        try
        {
            await _historyService.SaveRunHistoryAsync(_save, causeOfDeath: $"ถูก {EnemyName} สังหาร");
            await _saveService.DeleteSaveAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Permadeath] Error: {ex.Message}");
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var vm = new GameOverViewModel(EnemyName, CurrentLoop, CurrentWave);
            var page = new GameOverPage(vm);
            Application.Current.MainPage = page;

        });
    }

    private void GainXp(int xp)
    {
        if (_save is null) return;

        CurrentXp += xp;
        XpToNextLevel = CalculateXpThreshold(_save.Level);

        if (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            _save.Level++;
            _save.Atk += 8;
            _save.Def += 5;
            _save.Int += 8;
            _save.MaxHp += 10;
            _save.CurrentHp = _save.MaxHp;
            _save.MaxMp += 20;
            _save.CurrentMp = _save.MaxMp;

            AppendLog($"🎉 Level Up! Lv.{_save.Level}  Stats เพิ่ม + ฮีลเต็ม!");
        }

        XpToNextLevel = CalculateXpThreshold(_save.Level);
        XpDisplay = $"{CurrentXp} / {XpToNextLevel}";
        CharacterXpProgress = XpToNextLevel > 0
            ? Math.Clamp((double)CurrentXp / XpToNextLevel, 0.0, 1.0)
            : 0.0;

        CharacterLevel = _save.Level;
    }

    private int CalculateXpThreshold(int level) => level * 80 + 20;

    public async Task StartNextLoopAsync()
    {
        if (_save is null) return;

        CurrentLoop++;
        CurrentWave = 1;

        _save.CurrentLoop = CurrentLoop;
        _save.CurrentWave = 1;
        _save.CurrentHp = _save.MaxHp;
        _save.CurrentMp = _save.MaxMp;

        await _saveService.UpdateSaveAsync(_save);

        SpawnEnemyForWave(1);
        UpdateWaveLabel();
        RefreshPlayerUi();
        AppendLog($"🌟 Loop {CurrentLoop} เริ่มต้น! ศัตรูแข็งแกร่งขึ้น!");
    }

    private void SpawnEnemyForWave(int wave)
    {
        _enemy = wave == 10
            ? EnemyFactory.CreateBossEnemy(CurrentLoop)
            : EnemyFactory.CreateNormalEnemy(wave, CurrentLoop);

        RefreshEnemyUi();
    }

    private void RefreshPlayerUi()
    {
        if (_save is null) return;

        CharacterName = _save.ClassName;

        switch (_save.ClassName)
        {
            case "Warrior": CharacterImageSource = "fighter.png"; break;
            case "Mage": CharacterImageSource = "magecat.png"; break;
            case "Rogue": CharacterImageSource = "therogue.png"; break;
            default: CharacterImageSource = "player.png"; break;
        }

        CharacterLevel = _save.Level;
        CharacterHpText = $"{_save.CurrentHp}/{_save.MaxHp}";
        CharacterHpProgress = _save.MaxHp > 0
            ? Math.Clamp((double)_save.CurrentHp / _save.MaxHp, 0.0, 1.0)
            : 0.0;
        CharacterMpText = $"{_save.CurrentMp}/{_save.MaxMp}";
        CharacterMpProgress = _save.MaxMp > 0
            ? Math.Clamp((double)_save.CurrentMp / _save.MaxMp, 0.0, 1.0)
            : 0.0;
        Coins = _save.Coins;
        HpPotionCount = _save.HpPotionCount;
        MpPotionCount = _save.MpPotionCount;
    }

    private void RefreshEnemyUi()
    {
        if (_enemy is null) return;

        EnemyName = _enemy.Name;
        EnemyLevel = _enemy.Level;
        EnemyHpText = _enemy.HpText;
        EnemyHpProgress = _enemy.HpProgress;
        EnemyImageSource = _enemy.ImageSource;
    }

    private void UpdateWaveLabel()
    {
        WaveLabel = CurrentWave == 10
            ? $"👑 BOSS  (Loop {CurrentLoop})"
            : $"Wave {CurrentWave} / 10  ·  Loop {CurrentLoop}";
    }

    private void AppendLog(string message)
    {
        CombatLog = message;
        System.Diagnostics.Debug.WriteLine($"[Combat] {message}");
    }

    private bool CanAct() => !IsBusy;

    private InventoryItem? GetRandomArtifact()
    {
        // List of available artifacts to drop
        var availableArtifacts = new[]
        {
            new InventoryItem
            {
                Name = "Sword of Power",
                Icon = "⚔️",
                Type = ItemType.Weapon,
                BonusAtk = 15,
                Description = "+15 ATK",
            },
            new InventoryItem
            {
                Name = "Guardian's Shield",
                Icon = "🛡️",
                Type = ItemType.Armor,
                BonusDef = 12,
                BonusMaxHp = 30,
                Description = "+12 DEF, +30 Max HP",
            },
            new InventoryItem
            {
                Name = "Mage's Orb",
                Icon = "🔮",
                Type = ItemType.Armor,
                BonusMagic = 18,
                BonusMaxMp = 40,
                Description = "+18 Magic, +40 Max MP",
            },
            new InventoryItem
            {
                Name = "Dragon's Fang",
                Icon = "⚡",
                Type = ItemType.Weapon,
                BonusAtk = 20,
                BonusMagic = 5,
                Description = "+20 ATK, +5 Magic",
            },
            new InventoryItem
            {
                Name = "Stone of Resilience",
                Icon = "🪨",
                Type = ItemType.Armor,
                BonusDef = 15,
                Description = "+15 DEF",
            },
        };

        return availableArtifacts[_rng.Next(availableArtifacts.Length)];
    }

    private async Task SafeAlert(string title, string message)
    {
        if (ShowAlert is not null)
            await ShowAlert(title, message);
        else
            await Shell.Current.DisplayAlertAsync(title, message, "ตกลง");
    }

    private async Task<bool> SafeConfirm(string title, string message, string accept, string cancel)
    {
        if (ShowConfirm is not null)
            return await ShowConfirm(title, message, accept, cancel);
        return await Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
    }

    public ActiveSave? CurrentSave => _save;
    public Enemy? CurrentEnemy => _enemy;
}