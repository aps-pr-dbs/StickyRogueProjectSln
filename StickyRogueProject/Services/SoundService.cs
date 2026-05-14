using Plugin.Maui.Audio;

namespace StickyRogueProject.Services;

public class SoundService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer _clickSoundPlayer;
    private IAudioPlayer _bgmPlayer;
    private IAudioPlayer _selectPlayer;
    private IAudioPlayer _manapotionPlayer;
    private IAudioPlayer _hppotionPlayer;
    private IAudioPlayer _playerattackPlayer;
    private IAudioPlayer _gameOverPlayer;

    private IAudioPlayer _swordSlashPlayer;
    private IAudioPlayer _defendPlayer;
    private IAudioPlayer _magicAtkPlayer;
    private IAudioPlayer _escapedPlayer;
    private IAudioPlayer _daggerSlashPlayer;

    private MemoryStream _clickMemoryStream;
    private MemoryStream _bgmMemoryStream;
    private MemoryStream _selectMemoryStream;
    private MemoryStream _manapotionMemoryStream;
    private MemoryStream _hppotionMemoryStream;
    private MemoryStream _playerattackMemoryStream;
    private MemoryStream _gameOverMemoryStream;

    private MemoryStream _swordSlashMemoryStream;
    private MemoryStream _defendMemoryStream;
    private MemoryStream _magicAtkMemoryStream;
    private MemoryStream _escapedMemoryStream;
    private MemoryStream _daggerSlashMemoryStream;

    // ⚡ ระบบแยกเสียงใหม่ ⚡
    public bool IsBgmMuted { get; private set; } = false;
    public bool IsSfxMuted { get; private set; } = false;
    public double BgmVolume { get; private set; } = 0.5; // ระดับเสียงเริ่มต้น 50%

    // ใช้เพื่อให้ปุ่มลำโพงที่หน้าเมนูหลักยังทำงานได้เหมือนเดิม
    public bool IsMuted => IsBgmMuted;

    public SoundService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        LoadSoundsAsync();
    }

    private async void LoadSoundsAsync()
    {
        try
        {
            using var clickAssetStream = await FileSystem.OpenAppPackageFileAsync("click.mp3");
            if (clickAssetStream != null)
            {
                _clickMemoryStream = new MemoryStream();
                await clickAssetStream.CopyToAsync(_clickMemoryStream);
                _clickMemoryStream.Position = 0;
                _clickSoundPlayer = _audioManager.CreatePlayer(_clickMemoryStream);
            }

            using var bgmAssetStream = await FileSystem.OpenAppPackageFileAsync("music1.mp3");
            if (bgmAssetStream != null)
            {
                _bgmMemoryStream = new MemoryStream();
                await bgmAssetStream.CopyToAsync(_bgmMemoryStream);
                _bgmMemoryStream.Position = 0;
                _bgmPlayer = _audioManager.CreatePlayer(_bgmMemoryStream);
                _bgmPlayer.Loop = true;
                _bgmPlayer.Volume = BgmVolume; // ⚡ กำหนดความดังตอนเริ่ม
                PlayBgm();
            }

            using var selectAssetStream = await FileSystem.OpenAppPackageFileAsync("select.mp3");
            if (selectAssetStream != null)
            {
                _selectMemoryStream = new MemoryStream();
                await selectAssetStream.CopyToAsync(_selectMemoryStream);
                _selectMemoryStream.Position = 0;
                _selectPlayer = _audioManager.CreatePlayer(_selectMemoryStream);
            }

            using var hppotionAssetStream = await FileSystem.OpenAppPackageFileAsync("hppotion.mp3");
            if (hppotionAssetStream != null)
            {
                _hppotionMemoryStream = new MemoryStream();
                await hppotionAssetStream.CopyToAsync(_hppotionMemoryStream);
                _hppotionMemoryStream.Position = 0;
                _hppotionPlayer = _audioManager.CreatePlayer(_hppotionMemoryStream);
            }

            using var manapotionAssetStream = await FileSystem.OpenAppPackageFileAsync("manapotion.mp3");
            if (manapotionAssetStream != null)
            {
                _manapotionMemoryStream = new MemoryStream();
                await manapotionAssetStream.CopyToAsync(_manapotionMemoryStream);
                _manapotionMemoryStream.Position = 0;
                _manapotionPlayer = _audioManager.CreatePlayer(_manapotionMemoryStream);
            }

            using var playerattackAssetStream = await FileSystem.OpenAppPackageFileAsync("playerattack.mp3");
            if (playerattackAssetStream != null)
            {
                _playerattackMemoryStream = new MemoryStream();
                await playerattackAssetStream.CopyToAsync(_playerattackMemoryStream);
                _playerattackMemoryStream.Position = 0;
                _playerattackPlayer = _audioManager.CreatePlayer(_playerattackMemoryStream);
            }

            using var gameOverAssetStream = await FileSystem.OpenAppPackageFileAsync("died.mp3");
            if (gameOverAssetStream != null)
            {
                _gameOverMemoryStream = new MemoryStream();
                await gameOverAssetStream.CopyToAsync(_gameOverMemoryStream);
                _gameOverMemoryStream.Position = 0;
                _gameOverPlayer = _audioManager.CreatePlayer(_gameOverMemoryStream);
            }

            using var swordSlashAssetStream = await FileSystem.OpenAppPackageFileAsync("swordslash.mp3");
            if (swordSlashAssetStream != null)
            {
                _swordSlashMemoryStream = new MemoryStream();
                await swordSlashAssetStream.CopyToAsync(_swordSlashMemoryStream);
                _swordSlashMemoryStream.Position = 0;
                _swordSlashPlayer = _audioManager.CreatePlayer(_swordSlashMemoryStream);
            }

            using var defendAssetStream = await FileSystem.OpenAppPackageFileAsync("defend.mp3");
            if (defendAssetStream != null)
            {
                _defendMemoryStream = new MemoryStream();
                await defendAssetStream.CopyToAsync(_defendMemoryStream);
                _defendMemoryStream.Position = 0;
                _defendPlayer = _audioManager.CreatePlayer(_defendMemoryStream);
            }

            using var magicAtkAssetStream = await FileSystem.OpenAppPackageFileAsync("magicatk.mp3");
            if (magicAtkAssetStream != null)
            {
                _magicAtkMemoryStream = new MemoryStream();
                await magicAtkAssetStream.CopyToAsync(_magicAtkMemoryStream);
                _magicAtkMemoryStream.Position = 0;
                _magicAtkPlayer = _audioManager.CreatePlayer(_magicAtkMemoryStream);
            }

            using var escapedAssetStream = await FileSystem.OpenAppPackageFileAsync("escaped.mp3");
            if (escapedAssetStream != null)
            {
                _escapedMemoryStream = new MemoryStream();
                await escapedAssetStream.CopyToAsync(_escapedMemoryStream);
                _escapedMemoryStream.Position = 0;
                _escapedPlayer = _audioManager.CreatePlayer(_escapedMemoryStream);
            }

            using var daggerSlashAssetStream = await FileSystem.OpenAppPackageFileAsync("daggerslash.mp3");
            if (daggerSlashAssetStream != null)
            {
                _daggerSlashMemoryStream = new MemoryStream();
                await daggerSlashAssetStream.CopyToAsync(_daggerSlashMemoryStream);
                _daggerSlashMemoryStream.Position = 0;
                _daggerSlashPlayer = _audioManager.CreatePlayer(_daggerSlashMemoryStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SoundService] Error loading sound: {ex.Message}");
        }
    }

    // ⚡ คำสั่งจัดการเสียงใหม่ ⚡
    public void SetBgmVolume(double volume)
    {
        BgmVolume = volume;
        if (_bgmPlayer != null)
        {
            _bgmPlayer.Volume = volume;
        }
    }

    public void ToggleBgm()
    {
        IsBgmMuted = !IsBgmMuted;
        if (IsBgmMuted) PauseBgm();
        else PlayBgm();
    }

    public void ToggleSfx()
    {
        IsSfxMuted = !IsSfxMuted;
    }

    public void ToggleMute()
    {
        ToggleBgm(); // ป้องกันปุ่มหน้าแรกพัง
    }

    public void PlayBgm()
    {
        if (!IsBgmMuted && _bgmPlayer != null && !_bgmPlayer.IsPlaying)
        {
            _bgmPlayer.Play();
        }
    }

    public void PauseBgm()
    {
        if (_bgmPlayer != null && _bgmPlayer.IsPlaying)
        {
            _bgmPlayer.Pause();
        }
    }

    // --- เล่นเสียงเอฟเฟกต์ (เช็ค IsSfxMuted แทน IsMuted) ---
    public void PlayGameOverSound()
    {
        PauseBgm();
        if (!IsSfxMuted && _gameOverPlayer != null)
        {
            _gameOverPlayer.Seek(0);
            _gameOverPlayer.Play();
        }
    }

    public void PlaySelectSound() { if (!IsSfxMuted && _selectPlayer != null) { _selectPlayer.Seek(0); _selectPlayer.Play(); } }
    public void PlayPlayerAttackSound() { if (!IsSfxMuted && _playerattackPlayer != null) { _playerattackPlayer.Seek(0); _playerattackPlayer.Play(); } }
    public void PlayHPPotionSound() { if (!IsSfxMuted && _hppotionPlayer != null) { _hppotionPlayer.Seek(0); _hppotionPlayer.Play(); } }
    public void PlayManaSound() { if (!IsSfxMuted && _manapotionPlayer != null) { _manapotionPlayer.Seek(0); _manapotionPlayer.Play(); } }
    public void PlayClickSound() { if (!IsSfxMuted && _clickSoundPlayer != null) { _clickSoundPlayer.Seek(0); _clickSoundPlayer.Play(); } }
    public void PlaySwordSlash() { if (!IsSfxMuted && _swordSlashPlayer != null) { _swordSlashPlayer.Seek(0); _swordSlashPlayer.Play(); } }
    public void PlayDefend() { if (!IsSfxMuted && _defendPlayer != null) { _defendPlayer.Seek(0); _defendPlayer.Play(); } }
    public void PlayMagicAtk() { if (!IsSfxMuted && _magicAtkPlayer != null) { _magicAtkPlayer.Seek(0); _magicAtkPlayer.Play(); } }
    public void PlayEscaped() { if (!IsSfxMuted && _escapedPlayer != null) { _escapedPlayer.Seek(0); _escapedPlayer.Play(); } }
    public void PlayDaggerSlash() { if (!IsSfxMuted && _daggerSlashPlayer != null) { _daggerSlashPlayer.Seek(0); _daggerSlashPlayer.Play(); } }
}