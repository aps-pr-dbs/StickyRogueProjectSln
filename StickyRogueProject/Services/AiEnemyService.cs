using StickyRogueProject.Models;
using System.Net.Http.Json;
using System.Text.Json;

public class AiEnemyService
{
    private readonly HttpClient _http;

    public AiEnemyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<EnemyDecision> DecideActionAsync(
    Enemy enemy, ActiveSave player, int wave, int loop,
    List<CombatTurn> history) // ⚡ เพิ่ม parameter
    {
        // ⚡ สร้างสรุป History 3 เทิร์นล่าสุด
        string historyText = "No turns yet.";
        if (history.Count > 0)
        {
            var recent = history.TakeLast(3).ToList();
            var lines = recent.Select((t, i) =>
                $"Turn -{recent.Count - i}: " +
                $"Player used [{t.PlayerAction}] " +
                $"dealt {t.PlayerDamageDealt} dmg, " +
                $"EnemyHP after={t.EnemyHpAfter}, " +
                $"PlayerHP after={t.PlayerHpAfter}" +
                (t.PlayerWasMissed ? " (missed)" : "")
            );
            historyText = string.Join("\n", lines);
        }

        string jsonExample = "{\"action\": \"Attack\", \"taunt\": \"รับดาเมจนี้ไปเลย!\", \"reason\": \"short reason\"}";

        string prompt = $@"
                You are controlling a monster in a turn-based RPG. 
                Analyze the battle history and choose a SMART action that counters the player's pattern.

                === BATTLE HISTORY (last 3 turns) ===
                {historyText}

                === CURRENT STATE ===
                ENEMY: {enemy.Name} Lv.{enemy.Level}
                HP: {enemy.CurrentHp}/{enemy.MaxHp} ({(double)enemy.CurrentHp / enemy.MaxHp:P0})
                ATK: {enemy.Atk}, INT: {enemy.Int}
                Resistance: {enemy.ResistanceType}

                PLAYER: {player.ClassName} Lv.{player.Level}
                HP: {player.CurrentHp}/{player.MaxHp}
                MP: {player.CurrentMp}/{player.MaxMp}
                ATK: {player.Atk}, DEF: {player.Def}, INT: {player.Int}
                Wave: {wave}/10, Loop: {loop}

                === STRATEGY GUIDE ===
                - If player keeps using Attack -> use Defend or Heavy to punish
                - If player keeps using Magic -> use Defend (reduce magic dmg)
                - If player used Defend last turn -> use Heavy (they may drop guard)
                - If enemy HP below 40% -> consider Heal
                - If player HP is low -> go aggressive with Heavy or Magic

                === AVAILABLE ACTIONS ===
                - Attack: normal physical damage
                - Heavy: 1.5x damage, 15% miss chance
                - Magic: INT-based damage, ignores player DEF
                - Heal: restore 20% HP (use only if HP below 40%)
                - Defend: reduce next incoming damage by 50%

                Reply ONLY as JSON with no markdown:
                {jsonExample}";

        var response = await _http.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages",
            new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 200,
                messages = new[] {
                    new { role = "user", content = prompt }
                }
            });

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        string raw = data
            .GetProperty("content")[0]
            .GetProperty("text").GetString() ?? "{}";

        // Parse JSON กลับมา
        var doc = JsonDocument.Parse(raw);
        return new EnemyDecision
        {
            Action = doc.RootElement.GetProperty("action").GetString() ?? "Attack",
            Taunt = doc.RootElement.GetProperty("taunt").GetString() ?? "...",
        };
    }
}

public record EnemyDecision
{
    public string Action { get; init; } = "Attack";
    public string Taunt { get; init; } = "";
}