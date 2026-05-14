using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyRogueProject.Models;
using StickyRogueProject.Services;
using StickyRogueProject.Views;
using StickyRogueProject.Views.PopUp;
using System.Text.Json;

namespace StickyRogueProject.ViewModels;

public partial class CombatViewModel : ObservableObject
{
    private readonly SaveService _saveService;
    private readonly HistoryService _historyService;
    private readonly SoundService _soundService;
    private readonly Random _rng = new();
    private readonly AiEnemyService _aiEnemyService;

    // ⚡ ตัวเก็บประวัติการต่อสู้ ⚡
    private readonly List<CombatTurn> _combatHistory = new();
    private string _lastPlayerAction = "Attack";

    public Func<Task>? OpenInventoryPopup { get; set; }
    public Func<Task>? OpenPlayerStatusPopup { get; set; }
    public Func<Task>? OpenEnemyStatusPopup { get; set; }
    public Func<string, string, Task>? ShowAlert { get; set; }
    public Func<string, string, string, string, Task<bool>>? ShowConfirm { get; set; }

    public Action? OnPlayerHitAnim { get; set; }
    public Action? OnEnemyHitAnim { get; set; }
    public Action? OnPlayerDodgeAnim { get; set; }
    public Action? OnEnemyDodgeAnim { get; set; }

    public Action? OnEnemyActionTriggered;

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

    [ObservableProperty] private string _enemyIntentIcon = "❓";
    [ObservableProperty] private string _enemyIntentText = "กำลังคิด...";

    [ObservableProperty] private string _characterName = string.Empty;
    [ObservableProperty] private int _characterLevel = 1;
    [ObservableProperty] private string _characterHpText = "0/0";
    [ObservableProperty] private double _characterHpProgress = 1.0;
    [ObservableProperty] private string _characterMpText = "0/0";
    [ObservableProperty] private double _characterMpProgress = 1.0;
    [ObservableProperty] private string _characterImageSource = string.Empty;

    [ObservableProperty] private string _xpDisplay = "0 / 100";
    [ObservableProperty] private double _characterXpProgress = 0.0;
    [ObservableProperty] private int _currentXp = 0;
    [ObservableProperty] private int _xpToNextLevel = 100;

    [ObservableProperty] private int _coins;
    [ObservableProperty] private int _hpPotionCount;
    [ObservableProperty] private int _mpPotionCount;

    [ObservableProperty] private string _combatLog = "Combat Start!";

    // ⚡ ตัวแปรสำหรับเปิด/ปิด และเก็บข้อความช่องแชท 2 ฝั่ง ⚡
    [ObservableProperty] private string _enemyTauntText = "";
    [ObservableProperty] private bool _isEnemyTauntVisible = false;

    [ObservableProperty] private string _playerTauntText = "";
    [ObservableProperty] private bool _isPlayerTauntVisible = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AttackCommand))]
    [NotifyCanExecuteChangedFor(nameof(DefendCommand))]
    [NotifyCanExecuteChangedFor(nameof(MagicCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(UseItemCommand))]
    private bool _isBusy;

    private bool _isDefending = false;

    public CombatViewModel(SaveService saveService, HistoryService historyService,
                        SoundService soundService, AiEnemyService aiEnemyService)
    {
        _saveService = saveService;
        _historyService = historyService;
        _soundService = soundService;
        _aiEnemyService = aiEnemyService;
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RefreshPlayerUi();
                SpawnEnemyForWave(CurrentWave);
                UpdateWaveLabel();
                AppendLog($"Wave {CurrentWave} — {EnemyName} ปรากฏตัว!");
            });
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

    // ==========================================
    // ⚔️ โหมดโจมตี (Attack)
    // ==========================================
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task AttackAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            _soundService.PlaySwordSlash();

            // ตัวละครพูดปลุกใจ
            string[] playerQuotes = { "ย๊ากกกก!", "รับดาบข้าไปซะ!", "เปิดช่องโหว่แล้ว!" };
            PlayerTauntText = $"💬 {CharacterName}: \"{playerQuotes[_rng.Next(playerQuotes.Length)]}\"";
            IsPlayerTauntVisible = true;

            if (_rng.NextDouble() < 0.10)
            {
                AppendLog("💨 คุณโจมตีพลาดเป้า!");
                OnEnemyDodgeAnim?.Invoke(); // ⚡ อนิเมชันมอนสเตอร์โยกหลบ

                _combatHistory.Add(new CombatTurn
                {
                    PlayerAction = "Attack",
                    PlayerDamageDealt = 0,
                    EnemyHpAfter = _enemy.CurrentHp,
                    PlayerHpAfter = _save.CurrentHp,
                    PlayerWasMissed = true
                });
            }
            else
            {
                double variation = 0.8 + _rng.NextDouble() * 0.4;
                int rawDmg = (int)(_save.Atk * variation);

                if (_enemy.ResistanceType == "Physical")
                {
                    rawDmg /= 2;
                    AppendLog($"🛡️ {_enemy.Name} หนังเหนียว! ต้านทานการโจมตีกายภาพ");
                }

                int finalDmg = _enemy.TakeDamage(rawDmg);
                AppendLog($"⚔️ คุณโจมตี {EnemyName} {finalDmg} ความเสียหาย!");

                OnEnemyHitAnim?.Invoke(); // ⚡ อนิเมชันมอนสเตอร์สั่น (โดนตี)
                RefreshEnemyUi();

                _combatHistory.Add(new CombatTurn
                {
                    PlayerAction = "Attack",
                    PlayerDamageDealt = finalDmg,
                    EnemyHpAfter = _enemy.CurrentHp,
                    PlayerHpAfter = _save.CurrentHp,
                    PlayerWasMissed = false
                });
            }

            if (_enemy.IsDefeated) { await OnEnemyDefeatedAsync(); return; }
            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
        _ = DetermineNextEnemyIntentAsync();
    }

    // ==========================================
    // 🛡️ โหมดป้องกัน (Defend)
    // ==========================================
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task DefendAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            _soundService.PlayDefend();
            _isDefending = true;

            PlayerTauntText = $"💬 {CharacterName}: \"เข้ามาเลย! ข้าพร้อมรับมือ!\"";
            IsPlayerTauntVisible = true;

            AppendLog("🛡️ คุณตั้งการ์ดเตรียมรับการโจมตี!");

            _combatHistory.Add(new CombatTurn
            {
                PlayerAction = "Defend",
                PlayerDamageDealt = 0,
                EnemyHpAfter = _enemy.CurrentHp,
                PlayerHpAfter = _save.CurrentHp,
                PlayerWasMissed = false
            });

            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
        _ = DetermineNextEnemyIntentAsync();
    }

    // ==========================================
    // ✨ โหมดเวทมนตร์ (Magic)
    // ==========================================
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

            _save.CurrentMp -= mpCost;
            _soundService.PlayMagicAtk();

            string[] magicQuotes = { "พลังเวทย์จงสถิตกับข้า!", "จงมอดไหม้!", "หายไปซะ!" };
            PlayerTauntText = $"💬 {CharacterName}: \"{magicQuotes[_rng.Next(magicQuotes.Length)]}\"";
            IsPlayerTauntVisible = true;

            if (_rng.NextDouble() < 0.10)
            {
                AppendLog("💨 ร่ายเวทย์ล้มเหลว พลาดเป้า!");
                OnEnemyDodgeAnim?.Invoke(); // ⚡ อนิเมชันมอนสเตอร์โยกหลบ

                _combatHistory.Add(new CombatTurn
                {
                    PlayerAction = "Magic",
                    PlayerDamageDealt = 0,
                    EnemyHpAfter = _enemy.CurrentHp,
                    PlayerHpAfter = _save.CurrentHp,
                    PlayerWasMissed = true
                });
            }
            else
            {
                double variation = 0.85 + _rng.NextDouble() * 0.3;
                int magicDmg = (int)(_save.Int * variation);

                if (_enemy.ResistanceType == "Magic")
                {
                    magicDmg /= 2;
                    AppendLog($"🛡️ {_enemy.Name} มีเกล็ดสะท้อนเวทย์! ต้านทานพลังเวทมนตร์");
                }

                _enemy.CurrentHp = Math.Max(0, _enemy.CurrentHp - magicDmg);
                AppendLog($"✨ เวทย์ถล่ม {EnemyName} {magicDmg} ความเสียหาย! (ทะลุ DEF)");

                OnEnemyHitAnim?.Invoke(); // ⚡ อนิเมชันมอนสเตอร์สั่น (โดนตี)
                RefreshPlayerUi();
                RefreshEnemyUi();

                _combatHistory.Add(new CombatTurn
                {
                    PlayerAction = "Magic",
                    PlayerDamageDealt = magicDmg,
                    EnemyHpAfter = _enemy.CurrentHp,
                    PlayerHpAfter = _save.CurrentHp,
                    PlayerWasMissed = false
                });
            }

            if (_enemy.IsDefeated) { await OnEnemyDefeatedAsync(); return; }
            await DoEnemyTurnAsync();
        }
        finally { IsBusy = false; }
        _ = DetermineNextEnemyIntentAsync();
    }

    // ==========================================
    // 🏃 โหมดหลบหนี (Run)
    // ==========================================
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task RunAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            bool escaped = _rng.NextDouble() < 0.25;

            PlayerTauntText = $"💬 {CharacterName}: \"ขืนอยู่ต่อตายแน่ ถอยดีกว่า!\"";
            IsPlayerTauntVisible = true;

            if (escaped)
            {
                _soundService.PlayEscaped();
                AppendLog("🏃 หลบหนีสำเร็จ!");
                await AdvanceToNextWaveAsync(skipReward: true);
                return;
            }

            AppendLog("❌ หลบหนีไม่สำเร็จ!");
            _combatHistory.Add(new CombatTurn
            {
                PlayerAction = "Run",
                PlayerDamageDealt = 0,
                EnemyHpAfter = _enemy.CurrentHp,
                PlayerHpAfter = _save.CurrentHp,
                PlayerWasMissed = false
            });
            await DoEnemyTurnAsync(runFailPenalty: true);
        }
        finally { IsBusy = false; }
        _ = DetermineNextEnemyIntentAsync();
    }

    // ==========================================
    // 💊 โหมดใช้ไอเทม (Use Item)
    // ==========================================
    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task UseItemAsync(string? potionType)
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;

            PlayerTauntText = $"💬 {CharacterName}: \"ขอดื่มยาฟื้นฟูพลังหน่อย!\"";
            IsPlayerTauntVisible = true;

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

            _combatHistory.Add(new CombatTurn
            {
                PlayerAction = potionType == "HP" ? "Use HP Potion" : "Use MP Potion",
                PlayerDamageDealt = 0,
                EnemyHpAfter = _enemy.CurrentHp,
                PlayerHpAfter = _save.CurrentHp,
                PlayerWasMissed = false
            });

            await DoEnemyTurnAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UseItem] Error: {ex.Message}");
        }
        finally { IsBusy = false; }
        _ = DetermineNextEnemyIntentAsync();
    }

    // ==========================================
    // ส่วนคำสั่งอื่นๆ (สถานะ, เทิร์นศัตรู, เคลียร์เวฟ ฯลฯ)
    // ==========================================
    [RelayCommand]
    private async Task OpenInventoryAsync()
    {
        if (_save == null) return;

        _soundService.PlayClickSound();

        // ⚡ สร้าง ViewModel และส่ง Save เข้าไป
        var invViewModel = new InventoryPopUpViewModel(_save, _saveService);

        // ⚡ เปิด Popup (ใช้ ShowPopupAsync ตามที่เราเปลี่ยน InventoryPopUpPage เป็น Popup)
        if (Application.Current.MainPage is Page mainPage)
        {
            await mainPage.ShowPopupAsync(new Views.PopUp.InventoryPopUpPage(invViewModel));
        }

        // ⚡ พอกระเป๋าปิดปุ๊บ บังคับรีเฟรชหน้าจอต่อสู้ทันที Stat จะได้อัปเดต!
        RefreshPlayerUi();
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

        await Task.Delay(600);
        OnEnemyActionTriggered?.Invoke();

        string intent = _enemy.NextIntent;
        bool isEnemyMiss = _rng.NextDouble() < 0.10;

        if (intent == "Heal")
        {
            EnemyTauntText = $"💬 {EnemyName}: \"แผลแค่นี้จิ๊บจ๊อยน่า...\"";
            IsEnemyTauntVisible = true;

            int healAmt = (int)(_enemy.MaxHp * 0.2);
            _enemy.CurrentHp = Math.Min(_enemy.MaxHp, _enemy.CurrentHp + healAmt);
            AppendLog($"💚 {EnemyName} ฟื้นฟูพลังชีวิตตัวเอง {healAmt} HP!");
            RefreshEnemyUi();
        }
        else
        {
            if (isEnemyMiss)
            {
                EnemyTauntText = $"💬 {EnemyName}: \"ชิ! แกหลบไวนักนะ!\"";
                IsEnemyTauntVisible = true;

                PlayerTauntText = $"💬 {CharacterName}: \"ฮ่าๆ ช้าไปนะ!\"";
                IsPlayerTauntVisible = true;

                AppendLog($"💨 {EnemyName} โจมตีพลาดเป้า! คุณรอดตัวไป");
                OnPlayerDodgeAnim?.Invoke(); // ⚡ อนิเมชันตัวละครเราโยกหลบ
            }
            else
            {
                string[] enemyAttackQuotes = { "ตายซะไอ้มนุษย์!", "รับนี่ไป!", "หึหึ... อ่อนหัด!" };
                EnemyTauntText = $"💬 {EnemyName}: \"{enemyAttackQuotes[_rng.Next(enemyAttackQuotes.Length)]}\"";
                IsEnemyTauntVisible = true;

                _soundService.PlayDaggerSlash();

                int rawDmg = 0;
                string atkLog = "โจมตี";

                if (intent == "Attack") { rawDmg = _enemy.CalculateAttack(); atkLog = "โจมตีปกติ"; }
                else if (intent == "Heavy") { rawDmg = (int)(_enemy.CalculateAttack() * 1.5); atkLog = "โจมตีอย่างหนัก!"; }
                else if (intent == "Magic") { rawDmg = (int)(_enemy.Int * (0.8 + _rng.NextDouble() * 0.4)); atkLog = "ร่ายเวทย์ใส่คุณ!"; }

                if (runFailPenalty) rawDmg = (int)(rawDmg * 1.15);

                int effectiveDef = _save.Def / 2;
                string guardLog = " (เกราะช่วยซับ 50%)";

                if (_isDefending)
                {
                    if (_rng.NextDouble() < 0.80)
                    {
                        effectiveDef = _save.Def;
                        guardLog = " 🛡️ (ตั้งการ์ดสำเร็จ! เกราะทำงาน 100%)";
                    }
                    else
                    {
                        effectiveDef = 0;
                        guardLog = " 💥 (การ์ดแตก! โดนดาเมจเต็มๆ)";
                    }
                }

                int finalDmg = Math.Max(1, rawDmg - effectiveDef);
                _save.CurrentHp = Math.Max(0, _save.CurrentHp - finalDmg);

                string penaltyNote = runFailPenalty ? " (+15% โทษหลบหนี)" : string.Empty;
                AppendLog($"💀 {EnemyName} {atkLog} {finalDmg} ดาเมจ!{penaltyNote}{guardLog}");

                // ⚡ ผู้เล่นร้องเจ็บปวด & เล่นอนิเมชันสั่น
                PlayerTauntText = $"💬 {CharacterName}: \"โอ๊ยยย! เจ็บนะ!\"";
                IsPlayerTauntVisible = true;
                OnPlayerHitAnim?.Invoke();
            }
        }

        _isDefending = false;
        RefreshPlayerUi();

        if (_save.CurrentHp <= 0)
        {
            await OnPlayerDefeatedAsync();
        }
    }

    private async Task DetermineNextEnemyIntentAsync()
    {
        if (_enemy is null || _save is null) return;

        EnemyIntentText = "กำลังคิด...";
        EnemyIntentIcon = "🧠";

        try
        {
            var decision = await _aiEnemyService.DecideActionAsync(
                _enemy, _save, CurrentWave, CurrentLoop,
                _combatHistory);

            _enemy.NextIntent = decision.Action;
            (_enemy.IntentIcon, EnemyIntentText) = decision.Action switch
            {
                "Heavy" => ("💥", "เตรียมทุบหนัก!"),
                "Magic" => ("✨", "กำลังร่ายเวทย์"),
                "Heal" => ("💚", "เตรียมฟื้นฟู"),
                "Defend" => ("🛡️", "ตั้งการ์ด"),
                _ => ("🗡️", "เตรียมโจมตี"),
            };
            EnemyIntentIcon = _enemy.IntentIcon;

            if (!string.IsNullOrEmpty(decision.Taunt))
            {
                EnemyTauntText = $"💬 {_enemy.Name}: \"{decision.Taunt}\"";
                IsEnemyTauntVisible = true;
            }

            // ⚡ ผู้เล่นตะโกนเตือนภัย (รอนิดนึงให้ข้อความบาดเจ็บหายไปก่อน)
            await Task.Delay(1000);
            WarnPlayerAboutIntent();
        }
        catch
        {
            await Task.Delay(1000); // ⚡ หน่วงเวลาให้ดูอนิเมชันตอนโดนตี
            FallbackDetermineIntent();
            WarnPlayerAboutIntent();
        }
    }

    // ⚡ ฟังก์ชันสำหรับให้ตัวละครแจ้งเตือนผู้เล่น (Breaking 4th Wall)
    private void WarnPlayerAboutIntent()
    {
        if (_enemy is null || _save is null) return;

        if (_enemy.NextIntent == "Heavy")
        {
            PlayerTauntText = $"💬 {CharacterName}: \"มันง้างมาแล้ว! ป้องกันเร็ว!\"";
            IsPlayerTauntVisible = true;
        }
        else if (_enemy.NextIntent == "Magic")
        {
            PlayerTauntText = $"💬 {CharacterName}: \"มันกำลังร่ายเวทย์! ระวัง!\"";
            IsPlayerTauntVisible = true;
        }
        else if (_enemy.NextIntent == "Heal")
        {
            PlayerTauntText = $"💬 {CharacterName}: \"มันกำลังจะฟื้นฟูเลือด ต้องรีบตี!\"";
            IsPlayerTauntVisible = true;
        }
    }

    private async Task OnEnemyDefeatedAsync()
    {
        if (_save is null || _enemy is null) return;

        int maxLoopCoins = 65;
        int coinsAvailableToEarn = maxLoopCoins - _save.LoopCoinsCollected;

        int actualCoinsEarned = Math.Min(_enemy.CoinReward, coinsAvailableToEarn);

        if (actualCoinsEarned > 0)
        {
            _save.Coins += actualCoinsEarned;
            _save.LoopCoinsCollected += actualCoinsEarned;
            AppendLog($"✅ ชนะ! ได้ {actualCoinsEarned} 🪙 (สะสมรอบนี้: {_save.LoopCoinsCollected}/{maxLoopCoins})");
        }
        else
        {
            AppendLog($"✅ ชนะ! (เหรียญดรอปครบเพดาน 65 เหรียญในรอบนี้แล้ว!)");
        }

        GainXp(_enemy.XpReward * 3);

        if (_rng.NextDouble() < 0.75)
        {
            if (_rng.NextDouble() < 0.70)
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
            AppendLog("🏪 กำจัดศัตรูตัวที่ 9! เข้าร้านค้า...");

            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(async () => {
                var popup = new Views.PopUp.GameMessagePopUpPage(
                    "คุณกำจัดศัตรูตัวที่ 9 ผ่านแล้ว!",
                    "แวะร้านค้าเพื่อเตรียมพร้อมก่อน Boss!",
                    tcs
                );
                await Shell.Current.Navigation.PushModalAsync(popup);
            });

            await tcs.Task;

            CurrentWave = 10;
            _save.CurrentWave = 10;
            await _saveService.UpdateSaveAsync(_save);
            await Shell.Current.GoToAsync("ShopPage");
            SpawnEnemyForWave(10);
            return;
        }

        if (CurrentWave == 10 && !skipReward)
        {
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

            AppendLog("👑 Boss พ่ายแพ้! ผ่าน Loop นี้แล้ว!");

            // เปลี่ยนจาก SafeAlert มาใช้ GameMessagePopUpPage 
            var tcsBoss = new TaskCompletionSource<bool>();
            MainThread.BeginInvokeOnMainThread(async () => {
                var popup = new Views.PopUp.GameMessagePopUpPage(
                    "👑 Boss Defeated!",
                    $"Loop {CurrentLoop} สำเร็จ!\nมุ่งหน้าสู่โบสถ์...",
                    tcsBoss
                );
                await Shell.Current.Navigation.PushModalAsync(popup);
            });
            await tcsBoss.Task;

            CurrentLoop++;
            CurrentWave = 1;
            _save!.CurrentLoop = CurrentLoop;
            _save.CurrentWave = CurrentWave;
            _save.LoopCoinsCollected = 0;
            _save.CurrentHp = _save.MaxHp;
            _save.CurrentMp = _save.MaxMp;

            await _saveService.UpdateSaveAsync(_save);

            await Shell.Current.GoToAsync("ChurchPage");
            return;
        }

        CurrentWave++;
        if (CurrentWave > 10) CurrentWave = 10;
        _save.CurrentWave = CurrentWave;
        await _saveService.UpdateSaveAsync(_save);

        SpawnEnemyForWave(CurrentWave);
        UpdateWaveLabel();
        AppendLog($"ศัตรูตัวที่ {CurrentWave} {EnemyName} ปรากฏตัว! ");
    }

    private async Task HandleWave5EventAsync()
    {
        double roll = _rng.NextDouble();

        if (roll < 0.15)
        {
            AppendLog("🎰 โชคชะตาพาไป Casino!");

            var casinoPopup = new Views.PopUp.CasinoEventPopUpPage();
            await App.Current.MainPage.ShowPopupAsync(casinoPopup);

            bool isBlackjack = _rng.NextDouble() < 0.5;
            string gameName = isBlackjack ? "BlackjackPage" : "HighLowPage";

            CurrentWave = 6;
            _save!.CurrentWave = 6;
            await _saveService.UpdateSaveAsync(_save);
            await Shell.Current.GoToAsync(gameName);
            SpawnEnemyForWave(6);
            UpdateWaveLabel();
        }
        else
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
        
        await Task.Delay(1500);

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

            _soundService.PlayGameOverSound();
        });
    }

    private void GainXp(int xp)
    {
        if (_save is null) return;

        CurrentXp += xp;
        bool leveledUp = false;

        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            _save.Level++;

            _save.Atk += 5;
            _save.Def += 6;
            _save.Int += 5;
            _save.MaxHp += 10;
            _save.MaxMp += 5;

            _save.CurrentHp = _save.MaxHp;
            _save.CurrentMp = _save.MaxMp;

            XpToNextLevel = CalculateXpThreshold(_save.Level);
            leveledUp = true;
        }

        if (leveledUp)
        {
            AppendLog($"🎉 Level Up! Lv.{_save.Level} คุณแข็งแกร่งขึ้น!");
        }

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
        _save.LoopCoinsCollected = 0;

        await _saveService.UpdateSaveAsync(_save);

        SpawnEnemyForWave(1);
        UpdateWaveLabel();
        RefreshPlayerUi();
        AppendLog($"🌟 รอบที่ {CurrentLoop} เริ่มต้น! ศัตรูแข็งแกร่งขึ้น!");
    }

    public async Task ReloadSaveDataAsync()
    {
        try
        {
            // บังคับโหลด Save ใหม่จากไฟล์จริงๆ
            var freshSave = await _saveService.LoadSaveAsync();

            if (freshSave is not null)
            {
                _save = freshSave;

                // ⚡ รีเฟรชค่าพื้นหลัง
                RefreshPlayerUi();

            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReloadSave] บัค: {ex.Message}");
        }
    }

    private void SpawnEnemyForWave(int wave)
    {
        _enemy = wave == 10
            ? EnemyFactory.CreateBossEnemy(CurrentLoop)
            : EnemyFactory.CreateNormalEnemy(wave, CurrentLoop);

        _combatHistory.Clear();

        // ⚡ รีเซ็ตบทพูดตอนเริ่ม Wave
        EnemyTauntText = $"💬 {_enemy.Name}: \"เข้ามาเลยไอ้หนู!\"";
        IsEnemyTauntVisible = true;

        PlayerTauntText = $"💬 {CharacterName}: \"ลุยกันเลย!\"";
        IsPlayerTauntVisible = true;

        FallbackDetermineIntent();
        WarnPlayerAboutIntent();

        RefreshEnemyUi();
        _ = DetermineNextEnemyIntentAsync();
    }

    private void FallbackDetermineIntent()
    {
        if (_enemy is null) return;

        double roll = _rng.NextDouble();
        string randomTaunt = "";

        if (CurrentWave == 10) // บทพูดของ Boss
        {
            if (roll < 0.35) { _enemy.NextIntent = "Heavy"; _enemy.IntentIcon = "💥"; EnemyIntentText = "เตรียมทุบหนัก!"; randomTaunt = "ข้าจะบดขยี้แกให้แหลก!"; }
            else if (roll < 0.65) { _enemy.NextIntent = "Magic"; _enemy.IntentIcon = "✨"; EnemyIntentText = "กำลังร่ายเวทย์"; randomTaunt = "จงหายไปในความมืดซะ!"; }
            else if (roll < 0.85) { _enemy.NextIntent = "Attack"; _enemy.IntentIcon = "🗡️"; EnemyIntentText = "เตรียมโจมตี"; randomTaunt = "รับดาบของข้าไป!"; }
            else { _enemy.NextIntent = "Heal"; _enemy.IntentIcon = "💚"; EnemyIntentText = "เตรียมฟื้นฟูเลือด"; randomTaunt = "พลังของข้าไร้ขีดจำกัด..."; }
        }
        else // บทพูดของมอนสเตอร์ลูกกระจ๊อก
        {
            if (roll < 0.60) { _enemy.NextIntent = "Attack"; _enemy.IntentIcon = "🗡️"; EnemyIntentText = "เตรียมโจมตี"; randomTaunt = "ย๊ากกกก เข้ามาเลย!"; }
            else if (roll < 0.80) { _enemy.NextIntent = "Heavy"; _enemy.IntentIcon = "💥"; EnemyIntentText = "เตรียมโจมตีหนัก!"; randomTaunt = "หลบให้พ้นล่ะไอ้หนู!"; }
            else if (roll < 0.90) { _enemy.NextIntent = "Magic"; _enemy.IntentIcon = "✨"; EnemyIntentText = "กำลังร่ายเวทย์"; randomTaunt = "ลิ้มรสพลังเวทย์ของข้า!"; }
            else { _enemy.NextIntent = "Heal"; _enemy.IntentIcon = "💚"; EnemyIntentText = "เตรียมฟื้นฟู"; randomTaunt = "แฮ่กๆ... ขอยาหน่อย!"; }
        }

        EnemyIntentIcon = _enemy.IntentIcon;

        // เปลี่ยนเป้าหมายจาก Log ไปลงกล่องแชท
        EnemyTauntText = $"💬 {_enemy.Name}: \"{randomTaunt}\"";
        IsEnemyTauntVisible = true;
    }

    public void RefreshPlayerUi()
    {
        if (_save is null) return;

        CharacterName = _save.ClassName;

        switch (_save.ClassName)
        {
            case "Warrior": CharacterImageSource = "fighter.png"; break;
            case "Mage": CharacterImageSource = "magecat.png"; break;
            case "Rogue": CharacterImageSource = "therogue.png"; break;
        }

        CharacterLevel = _save.Level;

        XpToNextLevel = CalculateXpThreshold(_save.Level);
        XpDisplay = $"{CurrentXp} / {XpToNextLevel}";
        CharacterXpProgress = XpToNextLevel > 0
            ? Math.Clamp((double)CurrentXp / XpToNextLevel, 0.0, 1.0)
            : 0.0;

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
            ? $"👑 BOSS ของรอบการต่อสู้ที่ {CurrentLoop}"
            : $"ศัตรูตัวที่ {CurrentWave} / 10  ·  ของรอบการต่อสู้ที่ {CurrentLoop}";
    }

    private void AppendLog(string message)
    {
        CombatLog = message;
        System.Diagnostics.Debug.WriteLine($"[Combat] {message}");
    }

    private bool CanAct() => !IsBusy;

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