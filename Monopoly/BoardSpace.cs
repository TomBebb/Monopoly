using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    public class BoardSpace
    {
        public string Name { get; private set; }

        public BoardSpace(string name)
        {
            Name = name;
        }

        public virtual void OnPlayerLanded(Player player)
        {
            player.Board.ShowPlayerLanded(player, this);
        }
    }

    public class TaxSpace: BoardSpace
    {
        public decimal Tax;

        public TaxSpace(string name, decimal tax): base(name)
        {
            Tax = tax;
        }

        public override void OnPlayerLanded(Player player)
        {
            base.OnPlayerLanded(player);
            player.Charge(Tax);
            player.Board.ShowTaxed(player, this);
        }
    }

    public class GotoJailSpace : BoardSpace
    {
        public GotoJailSpace() : base("Go to Jail")
        { }
        public override void OnPlayerLanded(Player player)
        {
            base.OnPlayerLanded(player);
            player.SendToJail();
        }
    }
    public class JailSpace : BoardSpace
    {
        public JailSpace() : base("Jail")
        { }
    }

    public class StationSpace : BoardSpace
    {
        public Player Owner;
        public StationSpace(string name): base(name)
        {

        }

        public decimal Cost = 200m;
        public decimal Rent
        {
            get
            {
                if (Owner == null)
                    return 0m;
                switch (Owner.OwnedStations)
                {
                    case 1: return 25m;
                    case 2: return 50m;
                    case 3: return 100m;
                    default: return 200m;
                }
            }

        }

        public override void OnPlayerLanded(Player player)
        {
            base.OnPlayerLanded(player);
            if(player != Owner && Owner != null && !Owner.InJail)
            {
                var rent = Rent;
                player.Charge(Rent);
                Owner.Gain(Rent);
                player.Board.ShowPlayerPaidRent(player, Owner, this, rent);
            } else if(Owner == null)
            {
                if (player.Board.CheckPlayerBuy(player, this, Cost))
                {
                    Owner = player;
                    player.AddProperty(this);
                }
            }
        }
    }

    public enum PropertyFamily
    {
        Brown,
        LightBlue,
        DarkBlue
    }

    public class CommunityChestSpace : BoardSpace
    {
        public CommunityChestSpace() : base("Community Chest") { }
    }

    public class ChanceSpace : BoardSpace
    {
        public ChanceSpace() : base("Chance") { }
        public override void OnPlayerLanded(Player player)
        {
            base.OnPlayerLanded(player);

            var card = player.Board.PickChanceCard();
            player.Board.ShowPlayerPickedCard(player, card);
            card.DoCard(player);
        }
    }

    public class Property : BoardSpace
    {
        public Board Board;
        public decimal[] RentValues;
        public int NumHouses = 0;
        public Player Owner { get; private set; }
        public decimal Rent => RentValues[NumHouses];
        public decimal HouseCost { get; private set; }
        public decimal Cost { get; private set; }
        public decimal MortgageValue { get; private set; }
        public PropertyFamily Family { get; private set; }

        public Property(Board board, string name, PropertyFamily family, decimal cost, decimal houseCost, decimal mortgageValue, decimal[] rentValues): base(name)
        {
            Board = board;
            RentValues = rentValues;
            Family = family;
            Cost = cost;
            HouseCost = houseCost;
            MortgageValue = mortgageValue;
        }

        public void AddHouse()
        {
            if (!CanAddHouse())
                return;
            NumHouses++;
            Board.ShowPlayerAddedHouse(Owner, this, NumHouses == RentValues.Last());
        }

        public bool CanAddHouse()
        {
            return (Owner.Money >= HouseCost) && (NumHouses + 1 < RentValues.Length) && (Owner.HasAll(Family));
        }

        public override string ToString() => Name;

        public override void OnPlayerLanded(Player player)
        {
            base.OnPlayerLanded(player);

            if (Owner != player && Owner != null && !Owner.InJail)
            {
                var rent = Rent;
                player.Charge(Rent);
                Owner.Gain(Rent);
                player.Board.ShowPlayerPaidRent(player, Owner, this, rent);
            } else if(Owner == null)
            {
                if (player.Board.CheckPlayerBuy(player, this, Cost))
                {
                    Owner = player;
                    player.AddProperty(this);
                }
            }
        }
    }
}
