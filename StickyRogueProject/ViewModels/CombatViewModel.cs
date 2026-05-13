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

    // ⚡ ตัวแปรใหม่สำหรับโชว์หน้า UI ว่ามอนสเตอร์จะทำอะไร ⚡
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AttackCommand))]
    [NotifyCanExecuteChangedFor(nameof(DefendCommand))]
    [NotifyCanExecuteChangedFor(nameof(MagicCommand))]
    [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    [NotifyCanExecuteChangedFor(nameof(UseItemCommand))]
    private bool _isBusy;

    private bool _isDefending = false;

    public CombatViewModel(SaveService saveService, HistoryService historyService, SoundService soundService)
    {
        _saveService = saveService;
        _historyService = historyService;
        _soundService = soundService;
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

    [RelayCommand(CanExecute = nameof(CanAct))]
    private async Task AttackAsync()
    {
        if (_save is null || _enemy is null) return;
        try
        {
            IsBusy = true;
            if (_rng.NextDouble() < 0.10)
            {
                AppendLog("💨 คุณโจมตีพลาดเป้า!");
                OnEnemyDodgeAnim?.Invoke(); // ⚡ สั่งศัตรูโยกหลบ
                FireDialogue(false, EnemyFactory.GetRandomEnemyDodgeQuote());
                try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { } // สั่นเบาๆ
            }
            else
            {
                double variation = 0.8 + _rng.NextDouble() * 0.4;
                int rawDmg = (int)(_save.Atk * variation);
                if (_enemy.ResistanceType == "Physical") { rawDmg /= 2; AppendLog($"🛡️ {_enemy.Name} ต้านทานการโจมตีกายภาพ"); }

                int finalDmg = _enemy.TakeDamage(rawDmg);
                AppendLog($"⚔️ คุณโจมตี {EnemyName} {finalDmg} ความเสียหาย!");

                OnEnemyHitAnim?.Invoke(); // ⚡ สั่งศัตรูกระพริบแดง
                FireDialogue(true, EnemyFactory.GetRandomPlayerAttackQuote());
                try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { } // สั่นกระแทก
                RefreshEnemyUi();
            }

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
            _isDefending = true; // ⚡ เปิดโหมดป้องกันสำหรับการตั้งการ์ด
            AppendLog("🛡️ คุณตั้งการ์ดเตรียมรับการโจมตี!");
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

            _save.CurrentMp -= mpCost;

            // ⚡ โอกาสร่ายเวทย์พลาด 10%
            if (_rng.NextDouble() < 0.10)
            {
                AppendLog("💨 ร่ายเวทย์ล้มเหลว พลาดเป้า!");
            }
            else
            {
                double variation = 0.85 + _rng.NextDouble() * 0.3;
                int magicDmg = (int)(_save.Int * variation);

                // ⚡ เช็คระบบต้านทาน (Magic)
                if (_enemy.ResistanceType == "Magic")
                {
                    magicDmg /= 2;
                    AppendLog($"🛡️ {_enemy.Name} มีเกล็ดสะท้อนเวทย์! ต้านทานพลังเวทมนตร์");
                }

                _enemy.CurrentHp = Math.Max(0, _enemy.CurrentHp - magicDmg);
                AppendLog($"✨ เวทย์ถล่ม {EnemyName} {magicDmg} ความเสียหาย! (ทะลุ DEF)");
                RefreshPlayerUi();
                RefreshEnemyUi();
            }

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

            AppendLog("❌ หลบหนีไม่สำเร็จ!");
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

            // ใช้ไอเทมเสร็จ มอนสเตอร์ได้ตีต่อ
            await DoEnemyTurnAsync();
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

    // ⚡ ฟังก์ชันทำงานของมอนสเตอร์ (แก้ไขใหม่หมด)
    private async Task DoEnemyTurnAsync(bool runFailPenalty = false)
    {
        if (_save is null || _enemy is null) return;
        await Task.Delay(600);

        string intent = _enemy.NextIntent;
        bool isEnemyMiss = _rng.NextDouble() < 0.10;

        if (intent == "Heal")
        {
            int healAmt = (int)(_enemy.MaxHp * 0.2);
            _enemy.CurrentHp = Math.Min(_enemy.MaxHp, _enemy.CurrentHp + healAmt);
            AppendLog($"💚 {EnemyName} ฟื้นฟูพลังชีวิต {healAmt} HP!");
            RefreshEnemyUi();
        }
        else
        {
            if (isEnemyMiss)
            {
                AppendLog($"💨 {EnemyName} โจมตีพลาดเป้า!");
                OnPlayerDodgeAnim?.Invoke(); // ⚡ สั่งเราโยกหลบ
                FireDialogue(true, EnemyFactory.GetRandomPlayerDodgeQuote());
            }
            else
            {
                int rawDmg = 0; string atkLog = "โจมตี";
                if (intent == "Attack") { rawDmg = _enemy.CalculateAttack(); atkLog = "โจมตีปกติ"; }
                else if (intent == "Heavy") { rawDmg = (int)(_enemy.CalculateAttack() * 1.5); atkLog = "โจมตีอย่างหนัก!"; }
                else if (intent == "Magic") { rawDmg = (int)(_enemy.Int * (0.8 + _rng.NextDouble() * 0.4)); atkLog = "ร่ายเวทย์!"; }

                int effectiveDef = _save.Def / 2; string guardLog = " (เกราะซับ 50%)";
                if (_isDefending)
                {
                    if (_rng.NextDouble() < 0.80) { effectiveDef = _save.Def; guardLog = " 🛡️ (ป้องกัน 100%)"; }
                    else { effectiveDef = 0; guardLog = " 💥 (การ์ดแตก!)"; }
                }

                int finalDmg = Math.Max(1, rawDmg - effectiveDef);
                _save.CurrentHp = Math.Max(0, _save.CurrentHp - finalDmg);
                AppendLog($"💀 {EnemyName} {atkLog} {finalDmg} ดาเมจ!{guardLog}");

                OnPlayerHitAnim?.Invoke(); // ⚡ สั่งเรากระพริบแดง
                FireDialogue(false, EnemyFactory.GetRandomEnemyAttackQuote(CurrentWave == 10));

                // ⚡ สั่นสะเทือนมือถือเวลาโดนตี ⚡
                try
                {
                    if (_isDefending && effectiveDef == _save.Def)
                    {
                        HapticFeedback.Default.Perform(HapticFeedbackType.Click); // บล็อคได้ สั่นเบาๆ
                    }
                    else
                    {
                        // โดนตี สั่นแรง (ยิ่งเป็น Heavy ยิ่งสั่นนาน)
                        int vibrationTime = intent == "Heavy" ? 400 : 150;
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(vibrationTime));
                    }
                }
                catch { }
            }
        }

        _isDefending = false;
        RefreshPlayerUi();
        if (_save.CurrentHp <= 0) await OnPlayerDefeatedAsync();
        else DetermineNextEnemyIntent();
    }
    // ⚡ ฟังก์ชันสุ่มเจตนาของมอนสเตอร์ในเทิร์นถัดไป (ที่เผลอลบทิ้งไป) ⚡
    private void DetermineNextEnemyIntent()
    {
        if (_enemy is null) return;
        double roll = _rng.NextDouble();

        if (CurrentWave == 10) // บอสจะออกท่าโหดบ่อยกว่า
        {
            if (roll < 0.35) { _enemy.NextIntent = "Heavy"; _enemy.IntentIcon = "💥"; EnemyIntentText = "เตรียมทุบหนัก!"; }
            else if (roll < 0.65) { _enemy.NextIntent = "Magic"; _enemy.IntentIcon = "✨"; EnemyIntentText = "กำลังร่ายเวทย์"; }
            else if (roll < 0.85) { _enemy.NextIntent = "Attack"; _enemy.IntentIcon = "🗡️"; EnemyIntentText = "เตรียมโจมตี"; }
            else { _enemy.NextIntent = "Heal"; _enemy.IntentIcon = "💚"; EnemyIntentText = "เตรียมฟื้นฟูเลือด"; }
        }
        else // มอนสเตอร์ปกติ
        {
            if (roll < 0.60) { _enemy.NextIntent = "Attack"; _enemy.IntentIcon = "🗡️"; EnemyIntentText = "เตรียมโจมตี"; }
            else if (roll < 0.80) { _enemy.NextIntent = "Heavy"; _enemy.IntentIcon = "💥"; EnemyIntentText = "เตรียมโจมตีหนัก!"; }
            else if (roll < 0.90) { _enemy.NextIntent = "Magic"; _enemy.IntentIcon = "✨"; EnemyIntentText = "กำลังร่ายเวทย์"; }
            else { _enemy.NextIntent = "Heal"; _enemy.IntentIcon = "💚"; EnemyIntentText = "เตรียมฟื้นฟู"; }
        }

        // อัปเดต UI ให้ผู้เล่นเห็น
        EnemyIntentIcon = _enemy.IntentIcon;
    }

    private async Task OnEnemyDefeatedAsync()
    {
        if (_save is null || _enemy is null) return;

        // ⚡ ระบบเพดานเหรียญสูงสุด 65 เหรียญต่อ Loop ⚡
        int maxLoopCoins = 65;
        int coinsAvailableToEarn = maxLoopCoins - _save.LoopCoinsCollected;

        // ให้เหรียญเท่าที่ไม่เกินเพดานที่เหลืออยู่
        int actualCoinsEarned = Math.Min(_enemy.CoinReward, coinsAvailableToEarn);

        if (actualCoinsEarned > 0)
        {
            _save.Coins += actualCoinsEarned;
            _save.LoopCoinsCollected += actualCoinsEarned; // นับใส่โควต้า Loop
            AppendLog($"✅ ชนะ! ได้ {actualCoinsEarned} 🪙 (สะสมรอบนี้: {_save.LoopCoinsCollected}/{maxLoopCoins})");
        }
        else
        {
            AppendLog($"✅ ชนะ! (เหรียญดรอปครบเพดาน 65 เหรียญในรอบนี้แล้ว!)");
        }

        GainXp(_enemy.XpReward * 3);

        // โอกาส 75% ที่จะมี Potion ตก
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

            // ⚡ 1. สร้างตัวรับสัญญาณ
            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(async () => {
                var popup = new Views.PopUp.GameMessagePopUpPage(
                    "คุณกำจัดศัตรูตัวที่ 9 ผ่านแล้ว!",
                    "แวะร้านค้าเพื่อเตรียมพร้อมก่อน Boss!",
                    tcs // ⚡ ส่งตัวรับสัญญาณเข้าไปให้ PopUp
                );
                await Shell.Current.Navigation.PushModalAsync(popup);
            });

            // ⚡ 2. สั่งให้โค้ด "หยุดรอ" ตรงนี้จนกว่า PopUp จะส่งสัญญาณกลับมา (จนกว่าจะกดตกลง)
            await tcs.Task;

            // ⚡ 3. พอกดตกลงปุ๊บ ค่อยทำงานต่อ (เซฟเกมและเด้งไปร้านค้า)
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

            // ⚡ 1. สร้างตัวรับสัญญาณ เพื่อหยุดรอการกดปุ่ม
            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(async () => {
                // ⚡ 2. เรียกใช้ PopUp สวยๆ แทน SafeAlert
                var popup = new Views.PopUp.GameMessagePopUpPage(
                    "👑 Boss Defeated!",
                    $"Loop {CurrentLoop} สำเร็จ!\nมุ่งหน้าสู่ Church...",
                    tcs // ส่งตัวรับสัญญาณเข้าไป
                );
                await Shell.Current.Navigation.PushModalAsync(popup);
            });

            // ⚡ 3. สั่งให้โค้ด "หยุดรอ" ตรงนี้จนกว่าผู้เล่นจะกด "ตกลง" ใน PopUp
            await tcs.Task;

            // ⚡ 4. พอกดตกลงแล้ว ค่อยทำงานส่วนที่เหลือต่อ
            CurrentLoop++;
            CurrentWave = 1;
            _save!.CurrentLoop = CurrentLoop;
            _save.CurrentWave = CurrentWave;
            _save.LoopCoinsCollected = 0;
            _save.CurrentHp = _save.MaxHp;
            _save.CurrentMp = _save.MaxMp;

            await _saveService.UpdateSaveAsync(_save);

            // วาร์ปไปหน้า Church
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
                _save.Coins += ev.Value; // ⚡ รับเงินจาก Event ตรงๆ ทะลุเพดานได้เลย ไม่ผ่าน LoopCoinsCollected
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
        _soundService.PlayGameOverSound();
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
        _save.LoopCoinsCollected = 0; // ⚡ เคลียร์โควต้าเหรียญให้เริ่มใหม่ใน Loop ถัดไป

        await _saveService.UpdateSaveAsync(_save);

        SpawnEnemyForWave(1);
        UpdateWaveLabel();
        RefreshPlayerUi();
        AppendLog($"🌟 รอบที่ {CurrentLoop} เริ่มต้น! ศัตรูแข็งแกร่งขึ้น!");
    }

    public async Task ReloadSaveDataAsync()
    {
        if (_save is null) return;

        var freshSave = await _saveService.LoadSaveAsync();
        if (freshSave is not null)
        {
            _save = freshSave;
            RefreshPlayerUi();
        }
    }

    private void SpawnEnemyForWave(int wave)
    {
        _enemy = wave == 10
            ? EnemyFactory.CreateBossEnemy(CurrentLoop)
            : EnemyFactory.CreateNormalEnemy(wave, CurrentLoop);

        // ⚡ เมื่อสุ่มมอนสเตอร์เสร็จ ให้มันคิดท่าโจมตีแรกรอไว้เลย
        DetermineNextEnemyIntent();
        RefreshEnemyUi();
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
    // ⚡ ตัวแปรบทพูด ⚡
    [ObservableProperty] private string _playerDialogue = string.Empty;
    [ObservableProperty] private string _enemyDialogue = string.Empty;
    [ObservableProperty] private bool _isPlayerDialogueVisible = false;
    [ObservableProperty] private bool _isEnemyDialogueVisible = false;

    // ⚡ ตัวส่งสัญญาณไปหาหน้า XAML เพื่อเล่นภาพเคลื่อนไหว ⚡
    public Action? OnPlayerHitAnim { get; set; }
    public Action? OnEnemyHitAnim { get; set; }
    public Action? OnPlayerDodgeAnim { get; set; }
    public Action? OnEnemyDodgeAnim { get; set; }

    // ฟังก์ชันจัดการหลอดแชท (เปิดแล้วปิดเอง)
    private void FireDialogue(bool isPlayer, string text)
    {
        MainThread.BeginInvokeOnMainThread(async () => {
            if (isPlayer) { PlayerDialogue = text; IsPlayerDialogueVisible = true; }
            else { EnemyDialogue = text; IsEnemyDialogueVisible = true; }

            await Task.Delay(2500); // โชว์ข้อความ 2.5 วินาที

            if (isPlayer && PlayerDialogue == text) IsPlayerDialogueVisible = false;
            if (!isPlayer && EnemyDialogue == text) IsEnemyDialogueVisible = false;
        });
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