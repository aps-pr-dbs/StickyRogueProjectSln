using System;
using System.Collections.Generic;
using System.Text;

namespace StickyRogueProject.Models;

public record CombatTurn
{
    public string PlayerAction { get; init; } = "";   // "Attack", "Magic", "Defend", "Run", "Item"
    public int PlayerDamageDealt { get; init; }        // ดาเมจที่ผู้เล่นทำ
    public int EnemyHpAfter { get; init; }             // HP ศัตรูหลังโดน
    public int PlayerHpAfter { get; init; }            // HP ผู้เล่นหลังโดนตีกลับ
    public bool PlayerWasMissed { get; init; }         // ผู้เล่นตีพลาดไหม
}