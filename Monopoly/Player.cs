using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{

    public enum EscapeJailCause
    {
        GetOutOfJailCard,
        Paid,
        Doubles
    }
    public class Player
    {
        public Board Board { get; private set; }
        private const decimal StartingMoney = 1500;

        public decimal Money { get; private set; }  = StartingMoney;

        public int SpaceOnBoard = 0;
        public int LastSpacesMoved = 0;
        public int NumGetOutOfJailFreeCards = 0;
        public string Name;
        private List<BoardSpace> ownedProperties = new List<BoardSpace>();
        public int OwnedStations => ownedProperties.Where(p => p is StationSpace).Count();
        public bool InJail = false;
        public bool RolledDouble = false;
        public bool Auto = false;

        public IReadOnlyList<BoardSpace> OwnedProperties => ownedProperties;

        public Player(Board board, string name)
        {
            Board = board;
            Name = name;
        }

        internal void AddProperty(BoardSpace space)
        {
            ownedProperties.Add(space);
        }

        public void SendToJail()
        {
            InJail = true;
            SpaceOnBoard = Board.JailSpace;

            Board.ShowSentToJail(this);
        }

        public void FreeFromJail(EscapeJailCause cause)
        {
            Board.ShowFreedFromJail(this, cause);
            InJail = false;
        }

        public bool Charge(decimal amount)
        {
            if (amount > Money)
                return false;
            Money -= amount;
            Board.ShowPlayerMoney(this);
            return true;
        }

        public void Gain(decimal amount)
        {
            Money += amount;
            Board.ShowPlayerMoney(this);
        }

        public override string ToString()
        {
            return $"Player {Name}";
        }

        public void Roll()
        {
            if (InJail && Money >= 50m)
            {
                if (Auto || Board.CheckPlayerPayOutOfJail(this))
                {
                    FreeFromJail(EscapeJailCause.Paid);
                    Charge(50m);
                }
            }
            if (InJail && NumGetOutOfJailFreeCards > 0)
            {
                if (Auto || Board.CheckPlayerUseGetOutOfJailCard(this))
                {
                    NumGetOutOfJailFreeCards--;
                    FreeFromJail(EscapeJailCause.GetOutOfJailCard);
                }
            }
            var propsCanAddHouseTo = OwnedProperties.Where(p => p is Property pr && pr.CanAddHouse());
            var first = propsCanAddHouseTo.LastOrDefault();
            if (Auto && first != null && first is Property prop)
            {
                prop.AddHouse();

            }

            int die1, die2;
            do
            {
                die1 = Board.RollDice();
                die2 = Board.RollDice();
                RolledDouble = die1 == die2;
                Board.ShowPlayerRolled(this, die1, die2, RolledDouble);
            } while (RolledDouble && !InJail);


            Advance(die1, die2);
        }

        public void Advance(int die1, int die2)
        {
            if (InJail)
            {
                if (RolledDouble)
                {
                    FreeFromJail(EscapeJailCause.Doubles);
                } else
                {
                    Board.ShowStillInJail(this);
                }
                return;
            }
            LastSpacesMoved = die1 + die2;
            SpaceOnBoard += LastSpacesMoved;

            if (SpaceOnBoard >= Board.boardSpaces.Length)
            {
                SpaceOnBoard = 0;
                Board.PassedGo(this);
            }
            
            var atSpace = Board.boardSpaces[SpaceOnBoard];
            if(atSpace != null)
                atSpace.OnPlayerLanded(this);
        }

        public bool HasAll(PropertyFamily family)
        {
            var numOwned = ownedProperties.Where(prop => prop is Property p && p.Family == family).Count();
            var total = Board.boardSpaces.Where(prop => prop != null && prop is Property p && p.Family == family).Count();
            return total == numOwned;
        }
    }
}
