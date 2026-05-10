namespace StickyRogueProject.Models;

public enum EventEffectType
{
    None, GainCoins, LoseCoins, HealHalfHp, LoseHp, GainStat, LoseStat, DrugDealer
}

public class GameEvent
{
    public string Title { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public EventEffectType EffectType { get; set; }
    public int Value { get; set; }
    public string StatType { get; set; } = string.Empty;

    public bool IsDrugDealer => EffectType == EventEffectType.DrugDealer;
    public bool IsNormalEvent => !IsDrugDealer;
}

public static class EventPool
{
    private static readonly Random _rng = new();
    private static readonly string[] _statTypes = { "ATK", "INT", "MAX HP", "MAX MP" };

    public static GameEvent GetRandomEvent()
    {
        int roll = _rng.Next(1, 10);
        var ev = new GameEvent();
        int xxx = 0;

        switch (roll)
        {
            case 1:
                ev.Title = "You found a lottery ticket on the road";
                ev.Story = "A bad day! maybe next time you’ll hit the jackpot!";
                ev.ImagePath = "event_poor.png";
                ev.EffectType = EventEffectType.None;
                break;
            case 2:
                xxx = _rng.Next(1, 1001);
                ev.Title = "You found a lottery ticket on the road";
                ev.Story = $"A lucky day! it's the jackpot, You get {xxx} coin!";
                ev.ImagePath = "event_lottery.png";
                ev.EffectType = EventEffectType.GainCoins;
                ev.Value = xxx;
                break;
            case 3:
                ev.Title = "You found an onsen in the forest";
                ev.Story = "Time to relax and recover your 50% HP.";
                ev.ImagePath = "event_onsen.png";
                ev.EffectType = EventEffectType.HealHalfHp;
                break;
            case 4:
                xxx = _rng.Next(1, 101);
                ev.Title = "A rat bit through your wallet";
                ev.Story = $"You lost {xxx} coins";
                ev.ImagePath = "event_rat.png";
                ev.EffectType = EventEffectType.LoseCoins;
                ev.Value = xxx;
                break;
            case 5:
                xxx = _rng.Next(1, 101);
                ev.Title = "Stepped on a trap";
                ev.Story = $"Ouch! You lost {xxx} HP.";
                ev.ImagePath = "event_trap.png";
                ev.EffectType = EventEffectType.LoseHp;
                ev.Value = xxx;
                break;
            case 6:
                ev.StatType = _statTypes[_rng.Next(_statTypes.Length)];
                ev.Title = "You found a sacred tome";
                ev.Story = $"You gained 3 {ev.StatType}.";
                ev.ImagePath = "event_tome.png";
                ev.EffectType = EventEffectType.GainStat;
                ev.Value = 3;
                break;
            case 7:
                ev.StatType = _statTypes[_rng.Next(_statTypes.Length)];
                ev.Title = "You found a lose tome";
                ev.Story = $"You lose 3 {ev.StatType}.";
                ev.ImagePath = "event_dtome.png";
                ev.EffectType = EventEffectType.LoseStat;
                ev.Value = 3;
                break;
            case 8:
                xxx = _rng.Next(1, 1001);
                ev.Title = "Old lady gives money";
                ev.Story = $"She said you look like her grandson. She give you {xxx} coin";
                ev.ImagePath = "event_olady.png";
                ev.EffectType = EventEffectType.GainCoins;
                ev.Value = xxx;
                break;
            case 9:
                ev.Title = "Found an illegal drug dealer";
                ev.Story = "Hey you, interested in some good stuff?";
                ev.ImagePath = "event_dealer2.png";
                ev.EffectType = EventEffectType.DrugDealer;
                break;
        }
        return ev;
    }
}