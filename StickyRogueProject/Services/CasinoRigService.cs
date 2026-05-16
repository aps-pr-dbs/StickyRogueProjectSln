// CasinoRigService — ศูนย์กลาง Logic "คาสิโนโกง" ทั้งหมด
// ใช้ร่วมกันระหว่าง BlackjackViewModel และ HighLowViewModel
//
// อัปเดตให้รองรับ ActiveSave โครงสร้างใหม่:
//   - save.Inventory คือ List<InventoryItem> ([Ignore] in SQLite, persisted via InventoryJson)
//   - save.HpPotionCount / save.MpPotionCount ยังมีอยู่ แต่ Item Theft ตอนนี้ดึงจาก Inventory ก่อน
//   - save.Coins คือ int ปกติ (ไม่เปลี่ยนแปลง)
//
// ลำดับความสำคัญของการขโมย Item:
//   1. Consumable ใน Inventory (Potion ฯลฯ)
//   2. Material ใน Inventory
//   3. HpPotionCount (Shortcut Potion)
//   4. MpPotionCount (Shortcut Potion)
//   5. ขโมยเหรียญ 10% แทน (fallback สุดท้าย)

using StickyRogueProject.Models;

namespace StickyRogueProject.Services;

// ── ผลลัพธ์ Win ──────────────────────────────────────────────
public enum CasinoWinResult
{
    NormalWin,   // ชนะปกติ — คืนเงินพนัน x1
    DoubleWin,   // Jackpot — คืนเงินพนัน x2
    Cheated,     // โกง — เสียเงิน + Debuff
}

// ── ผลลัพธ์ Loss ─────────────────────────────────────────────
public enum CasinoLossResult
{
    NormalLoss,   // แพ้ปกติ — เสียเงินพนันอย่างเดียว
    ItemStolen,   // โดนขโมย Item จาก Inventory ด้วย
}

// ── Debuff เมื่อถูกโกง ───────────────────────────────────────
public enum CasinoDebuff
{
    LoseAtk,
    LoseDef,
    LoseInt,
    LoseHp,
    LoseMp,
}

public static class CasinoRigService
{
    private static readonly Random _rng = new();

    // ── บทสนทนา NPC Dealer ───────────────────────────────────
    public static readonly string[] OpeningTaunts =
    {
        "อีกคนที่มาส่งเงินให้ข้า... น่าสงสาร",
        "ฮ่าฮ่า! แกกล้ามาเล่นในถิ่นของฉันเหรอ? ไม่มีทางที่แกจะเอาเหรียญพวกนี้กลับบ้านไปได้หรอก!",
        "นักผจญภัยหน้าโง่อีกราย ที่เอาเงินมาประเคนให้ข้า!",
        "เข้ามาแล้วก็อย่าคิดจะออกไปพร้อมเงิน",
        "กฎในที่นี้มีข้าเป็นคนกำหนด ไม่มีใครชนะข้า",
        "โชคของเจ้าหมดแล้ว ตั้งแต่วันที่เดินเข้ามา",
    };

    public static readonly string[] WinTaunts =
    {
        "...ข้าปล่อยให้ชนะครั้งนี้ เพื่อให้เจ้าติดใจ",
        "โชคดีเกินไปแล้ว อย่าฝันว่าจะชนะอีก",
        "เอาไปเถอะ แต่เงินนั้นจะกลับมาหาข้าเอง",
        "ชนะครั้งเดียวแล้วคิดว่าเก่งงั้นหรือ?",
    };

    public static readonly string[] CheatTaunts =
    {
        "คิดว่าชนะแล้วใช่ไหม? ข้าเป็นผู้กำหนดกฎ!",
        "แกคิดว่าแกชนะแล้วงั้นเหรอ? กฎของที่นี่ฉันเป็นคนกำหนด!",
        "ในคาสิโนของข้า... ไม่มีคำว่า 'ชนะ' สำหรับผู้มาเยือน",
        "HAHAHA! ไพ่ที่เจ้าได้นั้น... ข้าจัดการมาเองทั้งนั้น",
        "เจ้าจะรู้ว่าคาสิโนนี้ไม่เคยแพ้ใคร",
    };

    public static readonly string[] LossTaunts =
    {
        "แพ้แล้ว... ตามคาด เข้ามาอีกสิ ข้ายินดีรับเสมอ",
        "เงินเจ้าอยู่ในมือข้าแล้ว ขอบคุณ",
        "อ่อนแอเกินไป ฝึกฝนมาให้ดีกว่านี้แล้วค่อยมาแก้แค้น",
        "เสร็จไปอีกหนึ่ง~",
    };

    public static readonly string[] ItemStolenTaunts =
    {
        "โอ้? ของชิ้นนี้ดูมีค่านะ... ข้าขอเป็น 'ค่าธรรมเนียม' แล้วกัน",
        "อุ้ยยย! ดูเหมือนกระเป๋าเจ้าเบาลงนิดนึง ฮ่าฮ่า",
        "ในคาสิโนของข้า ทุกอย่างสามารถเป็นเดิมพันได้",
    };

    // ── ResolveWin ───────────────────────────────────────────
    // 80% → ชนะจริง (ใน 80% มี 20% โอกาส Double Payout)
    // 20% → โกง! เสียเงิน + Debuff
    public static (CasinoWinResult Result, int CoinDelta, string DealerLine, CasinoDebuff? Debuff)
        ResolveWin(int betAmount)
    {
        double roll = _rng.NextDouble();

        if (roll < 0.20) // 20% โกง
        {
            var debuff = (CasinoDebuff)_rng.Next(0, 5);
            string taunt = CheatTaunts[_rng.Next(CheatTaunts.Length)];
            // coinDelta เป็นลบ — เงินพนันถูกหักล่วงหน้าแล้ว ไม่คืน
            return (CasinoWinResult.Cheated, -betAmount, taunt, debuff);
        }

        // 80% ชนะ — ตรวจ Jackpot
        if (_rng.NextDouble() < 0.20) // 20% ใน 80% = Double
        {
            string jackpotTaunt = WinTaunts[_rng.Next(WinTaunts.Length)];
            return (CasinoWinResult.DoubleWin, betAmount * 3, jackpotTaunt, null);
        }

        string normalTaunt = WinTaunts[_rng.Next(WinTaunts.Length)];
        return (CasinoWinResult.NormalWin, betAmount * 2, normalTaunt, null);
    }

    // ── ResolveLoss ──────────────────────────────────────────
    // 40% โดนขโมย Item ด้วย
    public static (CasinoLossResult Result, string DealerLine)
        ResolveLoss()
    {
        double roll = _rng.NextDouble();

        if (roll < 0.40) // 40% โดนขโมย
        {
            string taunt = ItemStolenTaunts[_rng.Next(ItemStolenTaunts.Length)];
            return (CasinoLossResult.ItemStolen, taunt);
        }

        string normalTaunt = LossTaunts[_rng.Next(LossTaunts.Length)];
        return (CasinoLossResult.NormalLoss, normalTaunt);
    }

    // ── ApplyDebuff ──────────────────────────────────────────
    // ใช้ Debuff กับ ActiveSave เมื่อถูกโกง
    // คืน: ข้อความอธิบาย Debuff สำหรับ CasinoDialogPage
    public static string ApplyDebuff(ActiveSave save, CasinoDebuff debuff)
    {
        switch (debuff)
        {
            case CasinoDebuff.LoseAtk:
                int atkLoss = Math.Max(1, save.Atk / 5);
                save.Atk = Math.Max(0, save.Atk - atkLoss);
                return $"ATK -{atkLoss}";

            case CasinoDebuff.LoseDef:
                int defLoss = Math.Max(1, save.Def / 5);
                save.Def = Math.Max(0, save.Def - defLoss);
                return $"DEF -{defLoss}";

            case CasinoDebuff.LoseInt:
                int intLoss = Math.Max(1, save.Int / 5);
                save.Int = Math.Max(0, save.Int - intLoss);
                return $"INT -{intLoss}";

            case CasinoDebuff.LoseHp:
                int hpLoss = Math.Max(10, save.MaxHp / 10);
                save.CurrentHp = Math.Max(1, save.CurrentHp - hpLoss);
                return $"HP -{hpLoss}";

            case CasinoDebuff.LoseMp:
                int mpLoss = Math.Max(5, save.MaxMp / 10);
                save.CurrentMp = Math.Max(0, save.CurrentMp - mpLoss);
                return $"MP -{mpLoss}";

            default:
                return "???";
        }
    }

    // ── StealItem ────────────────────────────────────────────
    // ขโมย Item จากผู้เล่น 1 ชิ้น ตามลำดับความสำคัญ:
    //   1. Consumable (Potion) ใน save.Inventory
    //   2. Material (ของ Drop) ใน save.Inventory
    //   3. HpPotionCount (Shortcut Potion)
    //   4. MpPotionCount (Shortcut Potion)
    //   5. Fallback: ขโมยเหรียญ 10%
    //
    // คืน: ข้อความอธิบายว่าขโมยอะไรไป สำหรับ CasinoDialogPage
    // สำคัญ: ต้องเรียก SaveService.UpdateSaveAsync() หลังจาก Method นี้เสมอ
    public static string StealItem(ActiveSave save)
    {
        // ── ลำดับ 1: ขโมย Consumable จาก Inventory ──────────
        var consumable = save.Inventory
            .FirstOrDefault(i => i.Type == ItemType.Consumable);

        if (consumable is not null)
        {
            save.Inventory.Remove(consumable);
            return $"{consumable.Icon} {consumable.Name} ถูกขโมยไป";
        }

        // ── ลำดับ 2: ขโมย Material จาก Inventory ───────────
        var material = save.Inventory
            .FirstOrDefault(i => i.Type == ItemType.Material);

        if (material is not null)
        {
            save.Inventory.Remove(material);
            return $"{material.Icon} {material.Name} ถูกขโมยไป";
        }

        // ── ลำดับ 3: HpPotionCount (Shortcut Potion) ────────
        // HpPotionCount เป็น int ที่แยกจาก Inventory
        // ใช้ถ้า Inventory ว่างแต่ยังมี Shortcut Potion เหลือ
        if (save.HpPotionCount > 0)
        {
            save.HpPotionCount--;
            return "🧪 HP Potion ×1 ถูกขโมยไป";
        }

        // ── ลำดับ 4: MpPotionCount ───────────────────────────
        if (save.MpPotionCount > 0)
        {
            save.MpPotionCount--;
            return "💧 MP Potion ×1 ถูกขโมยไป";
        }

        // ── ลำดับ 5: Fallback — ขโมยเหรียญ 10% ─────────────
        // ไม่มีอะไรให้ขโมยแล้ว → ขโมยเหรียญแทน
        int stolen = Math.Max(5, save.Coins / 10);
        save.Coins = Math.Max(0, save.Coins - stolen);
        return $"🪙 ขโมยเหรียญเพิ่ม {stolen} เหรียญ (ไม่มี Item เหลือ)";
    }

    // ── Helper ───────────────────────────────────────────────
    public static string RandomOpeningTaunt() =>
        OpeningTaunts[new Random().Next(OpeningTaunts.Length)];
}