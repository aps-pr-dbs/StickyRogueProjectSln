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

    // ⚡ 1. สร้างตัวแปร Stream ไว้ระดับ Class เพื่อกันระบบแอบลบข้อมูลทิ้ง
    private MemoryStream _clickMemoryStream;
    private MemoryStream _bgmMemoryStream;
    private MemoryStream _selectMemoryStream;
    private MemoryStream _manapotionMemoryStream;
    private MemoryStream _hppotionMemoryStream;
    private MemoryStream _playerattackMemoryStream;
    private MemoryStream _gameOverMemoryStream;

    public bool IsMuted { get; private set; } = false;

    public SoundService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        LoadSoundsAsync();
    }

    private async void LoadSoundsAsync()
    {
        try
        {
            // โหลดเสียงคลิก
            using var clickAssetStream = await FileSystem.OpenAppPackageFileAsync("click.mp3");
            if (clickAssetStream != null)
            {
                _clickMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await clickAssetStream.CopyToAsync(_clickMemoryStream);
                _clickMemoryStream.Position = 0;

                _clickSoundPlayer = _audioManager.CreatePlayer(_clickMemoryStream);
            }

            // โหลดเพลง BGM
            using var bgmAssetStream = await FileSystem.OpenAppPackageFileAsync("music1.mp3");
            if (bgmAssetStream != null)
            {
                _bgmMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await bgmAssetStream.CopyToAsync(_bgmMemoryStream);
                _bgmMemoryStream.Position = 0;

                _bgmPlayer = _audioManager.CreatePlayer(_bgmMemoryStream);
                _bgmPlayer.Loop = true;

                PlayBgm();
            }
            // SELECT SOUND
            using var selectAssetStream = await FileSystem.OpenAppPackageFileAsync("select.mp3");
            if (selectAssetStream != null)
            {
                _selectMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await selectAssetStream.CopyToAsync(_selectMemoryStream);
                _selectMemoryStream.Position = 0;

                _selectPlayer = _audioManager.CreatePlayer(_selectMemoryStream);
            }
            // HP SOUND
            using var hppotionAssetStream = await FileSystem.OpenAppPackageFileAsync("hppotion.mp3");
            if (hppotionAssetStream != null)
            {
                _hppotionMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await hppotionAssetStream.CopyToAsync(_hppotionMemoryStream);
                _hppotionMemoryStream.Position = 0;

                _hppotionPlayer = _audioManager.CreatePlayer(_hppotionMemoryStream);
            }
            // MANA SOUND
            using var manapotionAssetStream = await FileSystem.OpenAppPackageFileAsync("manapotion.mp3");
            if (manapotionAssetStream != null)
            {
                _manapotionMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await manapotionAssetStream.CopyToAsync(_manapotionMemoryStream);
                _manapotionMemoryStream.Position = 0;

                _manapotionPlayer = _audioManager.CreatePlayer(_manapotionMemoryStream);
            }
            // Player Attack Sound
            using var playerattackAssetStream = await FileSystem.OpenAppPackageFileAsync("playerattack.mp3");
            if (playerattackAssetStream != null)
            {
                _playerattackMemoryStream = new MemoryStream(); // ⚡ ย้ายมาใช้ตัวแปร Class
                await playerattackAssetStream.CopyToAsync(_playerattackMemoryStream);
                _playerattackMemoryStream.Position = 0;

                _playerattackPlayer = _audioManager.CreatePlayer(_playerattackMemoryStream);
            }
           

            // โหลดเสียง Game Over
            using var gameOverAssetStream = await FileSystem.OpenAppPackageFileAsync("died.mp3");
            if (gameOverAssetStream != null)
            {
                _gameOverMemoryStream = new MemoryStream();
                await gameOverAssetStream.CopyToAsync(_gameOverMemoryStream);
                _gameOverMemoryStream.Position = 0;

                _gameOverPlayer = _audioManager.CreatePlayer(_gameOverMemoryStream);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SoundService] Error loading sound: {ex.Message}");
        }
    }
    //ฟังก์ชันสำหรับเรียกใช้ตอนตาย
    public void PlayGameOverSound()
    {
        if (!IsMuted && _gameOverPlayer != null)
        {
            PauseBgm(); // สั่งปิดเพลงต่อสู้ (BGM) ก่อน
            _gameOverPlayer.Seek(0); // ย้อนเสียงกลับไปเริ่มใหม่
            _gameOverPlayer.Play(); // เล่นเสียงตาย!
        }
    }

    public void PlaySelectSound()
    {
        if (!IsMuted && _selectPlayer != null)
        {
            // ⚡ 2. สั่งย้อนไฟล์เสียงกลับไปวินาทีที่ 0 ก่อนกดเล่น ไม่งั้นกดรัวๆ เสียงจะไม่ดังครับ
            _selectPlayer.Seek(0);
            _selectPlayer.Play();
        }
    }
    public void PlayPlayerAttackSound()
    {
        if (!IsMuted && _playerattackPlayer != null)
        {
            _playerattackPlayer.Seek(0);
            _playerattackPlayer.Play();
        }
    }

    public void PlayHPPotionSound()
    {
        if (!IsMuted && _hppotionPlayer != null)
        {
            _hppotionPlayer.Seek(0);
            _hppotionPlayer.Play();
        }
    }

    public void PlayManaSound()
    {
        if (!IsMuted && _manapotionPlayer != null)
        {
            _manapotionPlayer.Seek(0);
            _manapotionPlayer.Play();
        }
    }

    public void PlayClickSound()
    {
        if (!IsMuted && _clickSoundPlayer != null)
        {
            // ⚡ 2. สั่งย้อนไฟล์เสียงกลับไปวินาทีที่ 0 ก่อนกดเล่น ไม่งั้นกดรัวๆ เสียงจะไม่ดังครับ
            _clickSoundPlayer.Seek(0);
            _clickSoundPlayer.Play();
        }
    }

    public void PlayBgm()
    {
        if (!IsMuted && _bgmPlayer != null && !_bgmPlayer.IsPlaying)
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

    public void ToggleMute()
    {
        IsMuted = !IsMuted;

        if (IsMuted)
        {
            PauseBgm();
        }
        else
        {
            PlayBgm();
        }
    }
}