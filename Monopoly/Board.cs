using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    abstract public class Board
    {
        private static Card[] ChanceCards = new Card[] {
            new GetOutOfJailFreeCard(),
            new GoToJailCard()
        };

        private Random rng = new Random();
        public int RollDice()
        {
            return RandomInt(1, 6);
        }

        public int RandomInt(int min, int max)
        {
            return rng.Next(min, max + 1);
        }

        public Card PickChanceCard()
        {
            return ChanceCards[rng.Next(ChanceCards.Length)];
        }
        public BoardSpace[] boardSpaces;
        protected List<Player> players;

        public IReadOnlyList<Player> Players => players;

        public int JailSpace = 10;

        public int CurrentPlayer = 0;
        public int Turn = 1;

        public Board()
        {

            players = new List<Player>();
            boardSpaces = new BoardSpace[]
            {
                null,
                new Property(this, "Old Kent Road", PropertyFamily.Brown, 60m, 30m, 50m, new decimal[] { 2m,  10m, 30m, 90m, 160m, 250m}),
                new CommunityChestSpace(),
                new Property(this,  "Whitechapel Road", PropertyFamily.Brown, 60m, 30m, 50m, new decimal[] { 4m, 20m,  60m, 180m, 360m, 450m}),
                new TaxSpace("Income Tax", 200m),
                new StationSpace("Kings Cross Station"),
                new Property(this, "The Angel Islington", PropertyFamily.LightBlue, 100m, 50m, 50m, new decimal[]{ 6m, 30m, 90m, 270m, 400m, 550m }),
                new ChanceSpace(),

                new Property(this, "Euston Road", PropertyFamily.LightBlue, 100m, 50m, 50m, new decimal[]{ 6m, 30m, 90m, 270m, 400m, 550m }),

                new Property(this, "Pentonville Road", PropertyFamily.LightBlue, 100m, 50m, 50m, new decimal[]{ 8m, 40m, 100m, 300m, 450m, 600m }),
                new JailSpace(),
                //...
                new GotoJailSpace(),
                //...
                new Property(this, "Park Lane", PropertyFamily.DarkBlue, 350m, 200m, 175m, new decimal[] { 35m, 175m, 500m, 1100m, 1300m, 1500m }),
                new Property(this, "Mayfair", PropertyFamily.DarkBlue, 400m, 200m, 200m, new decimal[] { 50m, 200m, 600m, 1400m, 1700m, 2000m }),
            };
        }

        public abstract void SetupPlayers();
        public abstract void ShowPlayerLanded(Player player, BoardSpace space);
        public virtual void PassedGo(Player player)
        {
            player.Gain(200m);
        }
        public abstract void BeforeTurn();
        public void DoTurn()
        {
            BeforeTurn();
            foreach (var player in players)
                player.Roll();
            Turn++;
        }

        public abstract void ShowPlayerPaidRent(Player player, Player owner, BoardSpace space, decimal amount);
        public abstract bool CheckPlayerBuy(Player player, BoardSpace property, decimal cost);
        public abstract bool CheckPlayerUseGetOutOfJailCard(Player player);
        public abstract bool CheckPlayerPayOutOfJail(Player player);
        public abstract void ShowTaxed(Player player, TaxSpace space);
        public abstract void ShowSentToJail(Player player);
        public abstract void ShowFreedFromJail(Player player, EscapeJailCause cause);
        public abstract void ShowStillInJail(Player player);
        public abstract void ShowPlayerRolled(Player player, int dice1, int dice2, bool doubles);
        public abstract void ShowPlayerMoney(Player player);
        public abstract void ShowPlayerPickedCard(Player player, Card card);
        public abstract void ShowPlayerAddedHouse(Player owner, Property prop, bool isHotel);
    }
    public class ConsoleBoard : Board
    {
        static T PromptMemberOf<T>(string prompt, IReadOnlyList<T> list)
        {
            Console.Write($"{prompt}: ");
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {list[i]}");
            }
            var index = Prompt<int>("Please select an index") - 1;
            return list[index];

        }
        static bool PromptBool(string promptMsg) {
            do
            {
                Console.Write($"{promptMsg}: ");
                var line = Console.ReadLine();
                if (line.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
                if (line.StartsWith("n", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                Console.WriteLine("Expected a boolean (y / n) input");
            } while (true);
        }
        static T Prompt<T>(string promptMsg) {
            bool wasValid = true;
            string line;
            T value = default(T);
            do {
                if(!wasValid)
                {
                    Console.WriteLine($"Expected {typeof(T)}");
                }
                wasValid = false;
                Console.Write($"{promptMsg}: ");
                line = Console.ReadLine();
                try {
                    value = (T)Convert.ChangeType(line, typeof(T));
                    wasValid = true;
                } catch(FormatException ex) 
                {
                    wasValid = false;
                }
            } while(!wasValid);
            return value;
        }

        private Player PromptPlayer(int index)
        {
            var name = Prompt<string>($"Enter name for player {index}");
            var isAuto = PromptBool("CPU control this player?");
            return new Player(this, name) { Auto = isAuto };
        }

        public override void PassedGo(Player player)
        {
            base.PassedGo(player);
            Console.WriteLine($"{player} passed go and collected 200");
        }

        public override void SetupPlayers()
        {
            var numPlayers = Prompt<int>("Enter number of players to create");
            for(int i = 0; i < numPlayers; i++)
            {
                players.Add(PromptPlayer(i + 1));
            }
        }

        public override void ShowPlayerLanded(Player player, BoardSpace space)
        {
            Console.WriteLine($"{player} moved by {player.LastSpacesMoved}: landed on {space.Name}!");
        }

        public override void ShowPlayerPaidRent(Player player, Player owner, BoardSpace space, decimal amount)
        {
            Console.WriteLine($"{player} gave {space.Name} owner {owner} {amount:C}!");
        }

        public override void ShowPlayerMoney(Player player)
        {
            Console.WriteLine($"{player} now has {player.Money:C}");
        }

        public override bool CheckPlayerBuy(Player player, BoardSpace property, decimal cost)
        {
            if(player.Auto)
            {
                return cost < player.Money;
            }
            var shouldBuy = PromptBool($"Does {player} ({player.Money:C}) want to buy property {property.Name} {cost:C}? (true / false)\n");
            if (shouldBuy)
            {
                var props = player.OwnedProperties.Count == 0 ? "no" : string.Join(", ", player.OwnedProperties.Select(p => p.Name));
                Console.WriteLine($"{player} bought {property.Name}. Now has {player.Money - cost:C} and owns {props}");
            }
            return shouldBuy;
        }
        public override void BeforeTurn()
        {
            Console.WriteLine($"Starting turn: {Turn}");
            foreach(var player in players)
            {
                if (player.OwnedProperties.Count == 0)
                {
                    Console.WriteLine($"{player} does not own any properties.");
                    continue;
                }

                var props = string.Join(", ", player.OwnedProperties.Select(p => p.Name));

                Console.WriteLine($"{player} has properties: {props}");
                if (player.Auto)
                    continue;
                var doAction = PromptBool($"Does {player} want to do an action on any owned property?");

                if (doAction)
                {
                    var prop = PromptMemberOf("Pick a property to do an action on", player.OwnedProperties);
                    ShowActions(player, prop);
                }
            }
        }

        private void ShowActions(Player player, BoardSpace space)
        {
            if (space is Property prop)
            {
                var options = new List<string>();
                var action = PromptMemberOf($"Enter action for {prop.Name}", new List<string>
                {
                    "None",
                    "Buy a house"
                });
                if (action == "Buy a house")
                {
                    if (prop.CanAddHouse())
                    {
                        player.Charge(prop.HouseCost);
                        prop.NumHouses++;
                    }
                    else
                    {
                        Console.WriteLine("Can't add a house!");
                    }
                }
            }
        }

        public override void ShowTaxed(Player player, TaxSpace space)
        {
            Console.WriteLine($"{player} charged by ${space.Tax:C} at ${space.Name}");
        }
        public override void ShowSentToJail(Player player)
        {
            Console.WriteLine($"{player} sent to jail");
        }

        public override void ShowFreedFromJail(Player player, EscapeJailCause cause)
        {
            switch (cause)
            {
                case EscapeJailCause.Doubles:
                    Console.WriteLine($"{player} rolled doubles to get out of jail");
                    break;
                case EscapeJailCause.GetOutOfJailCard:
                    Console.WriteLine($"{player} used a 'Get out of Jail free' card");
                    break;
                case EscapeJailCause.Paid:
                    Console.WriteLine($"{player} paid {50:C} to get out of jail");
                    break;
            }

        }

        public override void ShowStillInJail(Player player)
        {
            Console.WriteLine($"{player} is still in jail!");
        }

        public override void ShowPlayerRolled(Player player, int dice1, int dice2, bool doubles)
        {
            Console.WriteLine($"{player} rolled {dice1} and {dice2}. doubles: {doubles}");
        }

        public override bool CheckPlayerPayOutOfJail(Player player)
        {
            return PromptBool($"Will {player} pay {50:C} to get out of jail?");
        }

        public override bool CheckPlayerUseGetOutOfJailCard(Player player)
        {
            return PromptBool($"Will  {player} use their get out of jail free card?");
        }
        public override void ShowPlayerPickedCard(Player player, Card card)
        {
            Console.WriteLine($"{player} picked card {card}");
        }

        public override void ShowPlayerAddedHouse(Player owner, Property prop, bool isHotel)
        {
            Console.WriteLine($"{owner} added {(isHotel ? "hotel" : "house")} to {prop.Name}");
        }
    }
}
