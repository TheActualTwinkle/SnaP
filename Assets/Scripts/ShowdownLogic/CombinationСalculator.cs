// ReSharper disable IdentifierTypo
/// <summary>
/// Source https://github.com/ccqi/TexasHoldem
/// </summary>
public static class CombinationСalculator

{
    public static Hand GetBestHand(Hand hand)
    {
        if (hand.Count() < 5)
        {
            hand.Clear();
            return hand;
        }
        if (IsRoyalFlush(hand))
            return GetRoyalFlush(hand);
        if (IsStraightFlush(hand))
            return GetStraightFlush(hand);
        if (IsFourOfAKind(hand))
            return GetFourOfAKind(hand);
        if (IsFullHouse(hand))
            return GetFullHouse(hand);
        if (IsFlush(hand))
            return GetFlush(hand);
        if (IsStraight(hand))
            return GetStraight(hand);
        if (IsThreeOfAKind(hand))
            return GetThreeOfAKind(hand);
        if (IsTwoPair(hand))
            return GetTwoPair(hand);
        if (IsOnePair(hand))
            return GetOnePair(hand);
        return GetHighCard(hand);
    }
    
    //get best class without running isRoyalFlush, since straightflush covers the royal flush
    public static Hand GetBestHandEfficiently(Hand hand)
    {
        if (hand.Count() < 5)
        {
            hand.Clear();
            return hand;
        }
        if (IsStraightFlush(hand))
            return GetStraightFlush(hand);
        if (IsFourOfAKind(hand))
            return GetFourOfAKind(hand);
        if (IsFullHouse(hand))
            return GetFullHouse(hand);
        if (IsFlush(hand))
            return GetFlush(hand);
        if (IsStraight(hand))
            return GetStraight(hand);
        if (IsThreeOfAKind(hand))
            return GetThreeOfAKind(hand);
        if (IsTwoPair(hand))
            return GetTwoPair(hand);
        if (IsOnePair(hand))
            return GetOnePair(hand);
        return GetHighCard(hand);
    }
    
    //look for royal flush, removing pair using recursion
    private static bool IsRoyalFlush(Hand hand)
    {
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand[i] != hand[i + 1])
            {
                continue;
            }

            Hand simplifiedhand1 = new(hand);
            simplifiedhand1.RemoveAt(i);
            Hand simplifiedhand2 = new(hand);
            simplifiedhand2.RemoveAt(i + 1);
            if (IsRoyalFlush(simplifiedhand1))
                return true;
            if (IsRoyalFlush(simplifiedhand2))
                return true;
        }
        
        Suit currentsuit = hand.GetCard(0).Suit;
        if (hand.GetCard(0).GetRank() == 14 && hand.GetCard(1).GetRank() == 13 && hand.GetCard(2).GetRank() == 12 && hand.GetCard(3).GetRank() == 11 && hand.GetCard(4).GetRank() == 10 && hand.GetCard(1).Suit == currentsuit && hand.GetCard(2).Suit == currentsuit && hand.GetCard(3).Suit == currentsuit && hand.GetCard(4).Suit == currentsuit)
            return true;
        return false;
    }
    
    //get royal flush using recursion
    private static Hand GetRoyalFlush(Hand hand)
    {
        hand.SortByRank();
        Hand straightflush = new(GetStraightFlush(hand));
        straightflush.AddCombinationId(10);
        if (straightflush.GetCard(0).GetRank() == 14)
            return straightflush;
        straightflush.Clear();
        return straightflush;
    }
    
    //use recursion to get rid of pairs, then evaluate straight flush
    private static bool IsStraightFlush(Hand hand)
    {
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1))
            {
                continue;
            }

            Hand simplifiedhand1 = new(hand);
            simplifiedhand1.RemoveAt(i);
            Hand simplifiedhand2 = new(hand);
            simplifiedhand2.RemoveAt(i + 1);
            if (IsStraightFlush(simplifiedhand1))
                return true;
            if (IsStraightFlush(simplifiedhand2))
                return true;
        }
        for (var i = 0; i <= hand.Count() - 5; i++)
        {
            int currentrank = hand.GetCard(i).GetRank();
            Suit currentsuit = hand.GetCard(i).Suit;
            if (currentrank == hand.GetCard(i + 1).GetRank() + 1 && currentrank == hand.GetCard(i + 2).GetRank() + 2 && currentrank == hand.GetCard(i + 3).GetRank() + 3 && currentrank == hand.GetCard(i + 4).GetRank() + 4 && currentsuit == hand.GetCard(i + 1).Suit && currentsuit == hand.GetCard(i + 2).Suit && currentsuit == hand.GetCard(i + 3).Suit && currentsuit == hand.GetCard(i + 4).Suit)
                return true;
            
        }
        for (var i = 0; i <= hand.Count() - 4; i++)
        {
            int currentrank = hand.GetCard(i).GetRank();
            Suit currentsuit = hand.GetCard(i).Suit;
            
            if (currentrank == 5 && hand.GetCard(i + 1).GetRank() == 4 && hand.GetCard(i + 2).GetRank() == 3 && hand.GetCard(i + 3).GetRank() == 2 && hand.GetCard(0).GetRank() == 14 && currentsuit == hand.GetCard(i + 1).Suit && currentsuit == hand.GetCard(i + 2).Suit && currentsuit == hand.GetCard(i + 3).Suit && currentsuit == hand.GetCard(0).Suit)
                return true;
        }
        return false;
    }
    
    //get straight flush using two pointer variable and taking care of all cases
    private static Hand GetStraightFlush(Hand hand)
    {
        hand.SortByRank();
        Hand straightflush = new();
        straightflush.AddCombinationId(9);
        if (hand.GetCard(0).GetRank() == 14)
            hand.Add(new CardObject( hand.GetCard(0).Suit, Value.Ace));

        straightflush.Add(hand.GetCard(0));
        int ptr1=0, ptr2=1;
        while (ptr1 < hand.Count() - 2 || ptr2 < hand.Count())
        {
            if (straightflush.Count() >= 5)
                break;
            int rank1=hand.GetCard(ptr1).GetRank(), rank2=hand.GetCard(ptr2).GetRank();
            Suit suit1 = hand.GetCard(ptr1).Suit, suit2=hand.GetCard(ptr2).Suit;
            if (rank1 - rank2 == 1 && suit1 == suit2)
            {
                straightflush.Add(hand.GetCard(ptr2));
                ptr1 = ptr2;
                ptr2++;
            }
            else if(rank1==2&&rank2==14&&suit1==suit2)
            {
                straightflush.Add(hand.GetCard(ptr2));
                ptr1 = ptr2;
                ptr2++;
            }
            else
            {
                if (rank1 - rank2 <= 1)
                    ptr2++;
                else
                {
                    straightflush.Clear();
                    straightflush.AddCombinationId(9);
                    ptr1++;
                    ptr2=ptr1+1;
                    straightflush.Add(hand.GetCard(ptr1));
                }
            }    
        }
        if (hand.GetCard(0).GetRank() == 14)
            hand.RemoveAt(hand.Count() - 1);
        straightflush.AddCombinationId(straightflush.GetCard(0).GetRank());
        if (straightflush.Count() < 5)
            straightflush.Clear();
        return straightflush;
    }
    
    //easy algorithm to understand, just loop through the array and check for a certain amount of pairs
    //same for 3 of a kind, full house, 2 pair and 1 pair
    private static bool IsFourOfAKind(Hand hand)
    {
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 4; i++)
        {
            if (hand.GetCard(i) == hand.GetCard(i + 1) && hand.GetCard(i) == hand.GetCard(i + 2) && hand.GetCard(i) == hand.GetCard(i + 3))
                return true;
        }
        return false;
    }
    
    //same as above except return the cards themselves
    private static Hand GetFourOfAKind(Hand hand)
    {
        Hand fourofakind = new();
        fourofakind.AddCombinationId(8);
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 4; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i) != hand.GetCard(i + 2) || hand.GetCard(i) != hand.GetCard(i + 3))
            {
                continue;
            }

            fourofakind.Add(hand.GetCard(i));
            fourofakind.Add(hand.GetCard(i + 1));
            fourofakind.Add(hand.GetCard(i + 2));
            fourofakind.Add(hand.GetCard(i + 3));
            fourofakind.AddCombinationId(hand.GetCard(i).GetRank());
            break;
        }
        return GetKickers(hand,fourofakind);
    }

    private static bool IsFullHouse(Hand hand)
    {
        hand.SortByRank();
        bool threeofakind = false, pair = false;
        var threeofakindRank = 0;
        
        for (var i = 0; i <= hand.Count() - 3; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i) != hand.GetCard(i + 2))
            {
                continue;
            }

            threeofakind = true;
            threeofakindRank = hand.GetCard(i).GetRank();
            break;
        }
        
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i).GetRank() == threeofakindRank)
            {
                continue;
            }

            pair = true;
            break;
        }
        
        return threeofakind == true && pair == true;
    }

    private static Hand GetFullHouse(Hand hand)
    {
        hand.SortByRank();
        Hand fullhouse = new();
        fullhouse.AddCombinationId(7);
        bool threeofakind = false, pair = false;
        var threeofakindRank = 0;
        
        for (var i = 0; i <= hand.Count() - 3; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i) != hand.GetCard(i + 2))
            {
                continue;
            }

            threeofakind = true;
            threeofakindRank = hand.GetCard(i).GetRank();
            fullhouse.Add(hand.GetCard(i));
            fullhouse.Add(hand.GetCard(i + 1));
            fullhouse.Add(hand.GetCard(i + 2));
            fullhouse.AddCombinationId(hand.GetCard(i).GetRank());
            break;
        }
        
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i).GetRank() == threeofakindRank)
            {
                continue;
            }

            pair = true;
            fullhouse.Add(hand.GetCard(i));
            fullhouse.Add(hand.GetCard(i + 1));
            fullhouse.AddCombinationId(hand.GetCard(i).GetRank());
            break;
        }
        
        if (threeofakind == true && pair == true)
            return fullhouse;
        
        fullhouse.Clear();
        return fullhouse;
    }
    
    //use a counter, if a counter reaches five, a flush is detected
    private static bool IsFlush(Hand hand)
    {
        hand.SortByRank();
        int diamondCount = 0, clubCount = 0, heartCount = 0, spadeCount = 0;
        for (var i = 0; i < hand.Count(); i++)
        {
            if (hand.GetCard(i).Suit == Suit.Diamonds)
                diamondCount++;
            else if (hand.GetCard(i).Suit == Suit.Clubs)
                clubCount++;
            else if (hand.GetCard(i).Suit == Suit.Hearts)
                heartCount++;
            else if (hand.GetCard(i).Suit == Suit.Spades)
                spadeCount++;
        }
        
        if (diamondCount >= 5)
            return true;
        if (clubCount >= 5)
            return true;
        if (heartCount >= 5)
            return true;
        if (spadeCount >= 5)
            return true;

        return false;
    }
    
    private static Hand GetFlush(Hand hand)
    {
        hand.SortByRank();
        Hand flush = new();
        flush.AddCombinationId(6);
        int diamondCount = 0, clubCount = 0, heartCount = 0, spadeCount = 0;
        for (var i = 0; i < hand.Count(); i++)
        {
            if (hand.GetCard(i).Suit == Suit.Diamonds)
                diamondCount++;
            else if (hand.GetCard(i).Suit == Suit.Clubs)
                clubCount++;
            else if (hand.GetCard(i).Suit == Suit.Hearts)
                heartCount++;
            else if (hand.GetCard(i).Suit == Suit.Spades)
                spadeCount++;
        }

        Suit suit = 0;
        if (diamondCount >= 5)
        {
            suit = Suit.Diamonds;
        }
        else if (clubCount >= 5)
        {
            suit = Suit.Clubs;

        }
        else if (heartCount >= 5)
        {
            suit = Suit.Hearts;

        }
        else if (spadeCount >= 5)
        {
            suit = Suit.Spades;
        }
        
        for (var i = 0; i <= hand.Count(); i++)
        {
            if (hand.GetCard(i).Suit == suit)
            {
                flush.Add(hand.GetCard(i));
                flush.AddCombinationId(hand.GetCard(i).GetRank());
            }
            if (flush.Count() == 5)
                break;
        }
        
        return flush;
    }

    private static bool IsStraight(Hand hand)
    {
        hand.SortByRank();
        if(hand.GetCard(0).GetRank()==14)
            hand.Add(new CardObject(hand.GetCard(0).Suit, Value.Ace));
        var straightCount=1;
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            //if 5 cards are found to be straights, break out of the loop
            if (straightCount == 5)
                break;
            int currentrank = hand.GetCard(i).GetRank();
            //if cards suit differ by 1, increment straight
            if (currentrank - hand.GetCard(i + 1).GetRank() == 1)
                straightCount++;
            //specific condition for 2-A
            else if (currentrank == 2 && hand.GetCard(i + 1).GetRank() == 14)
                straightCount++;
            //if cards suit differ by more than 1, reset straight to 1
            else if (currentrank - hand.GetCard(i + 1).GetRank() > 1)
                straightCount = 1;
            //if card suits does not differ, do nothing
        }
        if (hand.GetCard(0).GetRank() == 14)
            hand.RemoveAt(hand.Count() - 1);
        //depending on the straight count, return true or false
        return straightCount == 5;
    }

    private static Hand GetStraight(Hand hand)
    {
        hand.SortByRank();
        Hand straight = new();
        straight.AddCombinationId(5);
        if (hand.GetCard(0).GetRank() == 14)
            hand.Add(new CardObject(hand.GetCard(0).Suit, Value.Ace));
        var straightCount = 1;
        straight.Add(hand.GetCard(0));
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            //if 5 cards are found to be straights, break out of the loop
            if (straightCount == 5)
                break;
            int currentrank = hand.GetCard(i).GetRank();
            //if cards suit differ by 1, increment straight
            if (currentrank - hand.GetCard(i + 1).GetRank() == 1)
            {
                straightCount++;
                straight.Add(hand.GetCard(i+1));
            }
            //specific condition for 2-A
            else if (currentrank == 2 && hand.GetCard(i + 1).GetRank() == 14)
            {
                straightCount++;
                straight.Add(hand.GetCard(i+1));
            }
            //if cards suit differ by more than 1, reset straight to 1
            else if (currentrank - hand.GetCard(i + 1).GetRank() > 1)
            {
                straightCount = 1;
                straight.Clear();
                straight.AddCombinationId(5);
                straight.Add(hand.GetCard(i+1));
            }
            //if card suits does not differ, do nothing
        }
        //depending on the straight count, return true or false
        if (hand.GetCard(0).GetRank() == 14)
            hand.RemoveAt(hand.Count() - 1);
        if (straightCount != 5)
            straight.Clear();
        straight.AddCombinationId(straight.GetCard(0).GetRank());
        return straight;
    }

    private static bool IsThreeOfAKind(Hand hand)
    {
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 3; i++)
        {
            if (hand.GetCard(i) == hand.GetCard(i + 1) && hand.GetCard(i) == hand.GetCard(i + 2))
                return true;
        }
        return false;
    }

    private static Hand GetThreeOfAKind(Hand hand)
    {
        hand.SortByRank();
        Hand threeofakind = new();
        threeofakind.AddCombinationId(4);
        for (var i = 0; i <= hand.Count() - 3; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1) || hand.GetCard(i) != hand.GetCard(i + 2))
            {
                continue;
            }

            threeofakind.AddCombinationId(hand.GetCard(i).GetRank());
            threeofakind.Add(hand.GetCard(i));
            threeofakind.Add(hand.GetCard(i + 1));
            threeofakind.Add(hand.GetCard(i + 2));
            break;
        }
        return GetKickers(hand, threeofakind);
    }

    private static bool IsTwoPair(Hand hand)
    {
        hand.SortByRank();
        var pairCount = 0;
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1))
            {
                continue;
            }

            pairCount++;
            i++;
        }
        return pairCount >= 2;
    }

    private static Hand GetTwoPair(Hand hand)
    {
        hand.SortByRank();
        Hand twopair = new();
        twopair.AddCombinationId(3);
        var pairCount = 0;
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) != hand.GetCard(i + 1))
            {
                continue;
            }

            twopair.AddCombinationId(hand.GetCard(i).GetRank());
            twopair.Add(hand.GetCard(i));
            twopair.Add(hand.GetCard(i+1));
            pairCount++;
            if (pairCount == 2)
                break;
            i++;
        }
        if (pairCount == 2)
            return GetKickers(hand,twopair);
        
        twopair.Clear();
        return twopair;
    }

    private static bool IsOnePair(Hand hand)
    {
        hand.SortByRank();
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) == hand.GetCard(i + 1))
                return true;
        }
        return false;
    }

    private static Hand GetOnePair(Hand hand)
    {
        hand.SortByRank();
        Hand onepair = new();
        onepair.AddCombinationId(2);
        for (var i = 0; i <= hand.Count() - 2; i++)
        {
            if (hand.GetCard(i) == hand.GetCard(i + 1))
            {
                onepair.AddCombinationId(hand.GetCard(i).GetRank());
                onepair.Add(hand.GetCard(i));
                onepair.Add(hand.GetCard(i + 1));
                break;
            }
        }
        return GetKickers(hand, onepair);
    }

    private static Hand GetHighCard(Hand hand)
    {
        hand.SortByRank();
        Hand highcard = new();
        highcard.AddCombinationId(1);
        highcard.Add(hand.GetCard(0));
        highcard.AddCombinationId(hand.GetCard(0).GetRank());
        return GetKickers(hand, highcard);
    }

    private static Hand GetKickers(Hand hand, Hand specialCards)
    {
        if (specialCards.Count() == 0)
            return specialCards;
        for (var i = 0; i < specialCards.Count(); i++)
        {
            hand.Remove(specialCards.GetCard(i));
        }
        for (var i = 0; i < hand.Count();i++)
        {
            if (specialCards.Count() >= 5)
                break;
            specialCards.Add(hand.GetCard(i));
            specialCards.AddCombinationId(hand.GetCard(i).GetRank());
        }
        return specialCards;
    }
}