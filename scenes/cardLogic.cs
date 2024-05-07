using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public partial class battle : Node3D
{
    List<ItemData> BuffedCardsThisTurn = new();

    int BuffedCardId = 0;
    int DefenceBuffEachTurnPlayer = 0; // applied and taken off each turn, each turn is not required specification
    int ManaBuffPlayer = 0; // mana buff , check at start of turn
    int AtkBuffPlayerPerma = 0; // permanent atk buff, can be from items ? relics.
    public void PlayCard(ItemData usedCard, int enemyslot)
    {
    EnemyBase enemy = GetEnemyByID(enemyslot);

    // determine if has second effect 
    bool secondEffect = usedCard.SecondaryType == "" ? false : true;
    System.Collections.Generic.Dictionary<string, int> effects = new System.Collections.Generic.Dictionary<string, int> ();
    effects.Add(usedCard.Type, usedCard.Value);
    if (secondEffect)
    {
        effects.Add(usedCard.SecondaryType, usedCard.SecondaryValue);
    }


    foreach (KeyValuePair<string, int> effect in effects)
        {
            string key = effect.Key;
            int value = effect.Value;

            switch (key)
            {
                case "atk":
                    Attack(enemy, value);
                    break;
                case "def":
                    Defence(value);
                    break;
                case "draw":
                    DrawEffect(value);
                    break;
                case "atk all":
                    DamageAll(value);
                    break;
                case "draw until":
                    DrawUntil(value);
                    break;
                case "raiseAtk":
                    EnableBuff(key, value);
                    break;
                case "mana next turn":
                    RaiseManaForNextTurn(value);
                    break;
                case "break shield":
                    BreakShield(enemy);
                    break;
                case "mana":
                    RaiseManaPlayer(value);
                    break;
                case "draw random":
                    DrawRandomCard(value); 
                    break;
                case "raise self atk":
                    RaiseCardAtk(usedCard, value);
                    break;
                case "debuff hand mana":
                    DebuffHandMana(value);
                    break;
            }
        }

        EnemyLifeCheck();
        setGeneratedEnemyVisibility();
        CheckWinState();
    }

    public void DrawUntil(int value)
    {
        // this draws until hand full, lower cost -1 . its card 5
        draw(5 - playerHand.Count, "lowerCost", value);
    }

    public void RaiseCardAtk(ItemData item, int value)
    {
        if (IsStatAtkRelated(item.Type)){
            item.Value += value;
        }
    }
    public static void BreakShield(EnemyBase enemy)
    {
        enemy.CDef = 0;
    }

    public void RaiseManaPlayer(int value)
    {
        playerCurrentMana += value;
    }
    public void DebuffHandMana(int value)
    {
        foreach (ItemData item in playerHand)
        {
            buffCard(item, "lowerCost", value); /// -1 actually raises
        }
    }
    public void EnableBuff(string buff, int value)
    {
        if (buff == "raiseAtk")
        {
            atkbuffthisturn = true;
        }
    }

    public void DrawRandomCard(int value)
    {
        // value here is for reducing drawn card cost
		var random = new RandomNumberGenerator();
		random.Randomize();
		int randomSelection = random.RandiRange(1, GameManager.AllItems.Count);
		ItemData randomCard = GameManager.GenerateCard(randomSelection);
        buffCard(randomCard, "lowerCost", value);
        playerHand.Add(randomCard);
    }
    public void RaiseManaForNextTurn(int value)
    {
        ManaBuffPlayer += value;
    }

    bool atkbuffthisturn = false;
    public void DisableAllThisTurnBuffs()
    {
        atkbuffthisturn = false;
    }
    public void CheckForAfterCardPlayBuffs()
    {   
        if (atkbuffthisturn){
            foreach (ItemData item in playerHand)
            {
                if (IsStatAtkRelated(item.Type)){
                    item.Value += 1;
                }
                item.hasBeenBuffedThisTurn = true;
            }
        }
    }

    public bool IsStatAtkRelated(string buff)
    {
        if (buff == "atk" || buff == "atk all"){
            return true;
        }
        return false;
    }
    public ItemData buffCard(ItemData card, string buff, int amount)
    {   
        card.hasBeenBuffedThisTurn = true;
        switch (buff){
            case "lowerCost":
                if (card.Type == "item" || card.Cost <= 0){break;}
                card.Cost -= amount;
                break;
            case "permanent atk":
                if (IsStatAtkRelated(card.Type))
                {
                    card.Value += amount;
                }
                break;
        }
        return card;
    }

    public void Attack(EnemyBase enemy, int value)
    {
         DealDamageToEnemy(enemy, value);
    }

    public void Defence(int value)
    {
        playerDefence += value;
    }

    public void DrawEffect(int value)
    {
        draw(value);
    }

    public void DamageAll(int value)
    {
        foreach (EnemyBase enemy in enemyBoard)
        {
            DealDamageToEnemy(enemy, value);
        }
    }

    public void UnHighLightOtherCards(int HighlightedCardID)
    {   
        foreach (ColorRect card in cSlots)
        {
            int slot = (int)card.Get("slotID");
            if (slot != HighlightedCardID)
            {
                card.Call("UnHighlight");
            }
        }
    }


}
