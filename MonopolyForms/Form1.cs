using Monopoly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonopolyForms
{
    public partial class Form1 : Form, IPlayerInteracter
    {
        
        private Board board;
        private SolidBrush goSpaceBrush, brownBrush, lightBlueBrush, darkBlueBrush, pinkBrush, orangeBrush, redBrush, propNameBrush;

        public Form1()
        {
            InitializeComponent();

            goSpaceBrush = new SolidBrush(Color.Green);
            brownBrush = new SolidBrush(Color.Brown);
            lightBlueBrush = new SolidBrush(Color.LightBlue);
            darkBlueBrush = new SolidBrush(Color.DarkBlue);
            pinkBrush = new SolidBrush(Color.Pink);
            orangeBrush = new SolidBrush(Color.Orange);
            redBrush = new SolidBrush(Color.Red);
            propNameBrush = new SolidBrush(Color.Black);

            board = new Board(this);
            board.Setup();

            Click += Form1_OnClick;
            /*
        Brown,
        LightBlue,
        DarkBlue,
        Pink,
        Orange,
        Red*/
        }

        private void Form1_OnClick(object sender, EventArgs e)
        {
            board.DoTurn();
            Refresh();
        }

        private SolidBrush getFamilyBrush(PropertyFamily fam)
        {
            switch (fam)
            {
                case PropertyFamily.Brown:
                    return brownBrush;
                case PropertyFamily.LightBlue:
                    return lightBlueBrush;
                case PropertyFamily.DarkBlue:
                    return darkBlueBrush;
                case PropertyFamily.Pink:
                    return pinkBrush;
                case PropertyFamily.Orange:
                    return orangeBrush;
                case PropertyFamily.Red:
                    return redBrush;
                default:
                    return null;
            }
        }
        private void drawSpace(Graphics gfx, int spaceIndex, int maxSpaceWidth)
        {
            var spaceSize = Math.Min(Width / 10, Height / 10);
            var atSpace = board.boardSpaces[spaceIndex];

            var bounds = new Rectangle((spaceIndex % 10) * maxSpaceWidth, spaceSize * (spaceIndex / 10), maxSpaceWidth, spaceSize);
            var brush = atSpace is Property property ? getFamilyBrush(property.Family) : goSpaceBrush;
            gfx.FillRectangle(goSpaceBrush, bounds);
            gfx.DrawString(atSpace == null ? "Go" : atSpace.Name, Control.DefaultFont, propNameBrush, new Point(bounds.X, bounds.Y));

            var playersAtSquare = board.Players.Where(p => p.SpaceOnBoard == spaceIndex);
            foreach(var p in playersAtSquare)
            {
                gfx.DrawString(p.ToString(), Control.DefaultFont, propNameBrush, new Point(bounds.X, bounds.Y + spaceSize /2));
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var bufCtx = BufferedGraphicsManager.Current;
            var buf = bufCtx.Allocate(CreateGraphics(), DisplayRectangle);
            var maxSpaceWidth = board.boardSpaces.Where(s => s != null).Select(space => TextRenderer.MeasureText(space.Name, DefaultFont).Width).Max();

            for (int i = 0; i < board.boardSpaces.Length; i++)
                drawSpace(buf.Graphics, i, maxSpaceWidth);

            buf.Render();
            buf.Dispose();
        }

        public Board Board { get; set; }

        public void BeforeTurn() { }
        public bool CheckPlayerBuy(Player player, BoardSpace property, decimal cost) => false;
        public bool CheckPlayerPayOutOfJail(Player player) => false;
        public bool CheckPlayerUseGetOutOfJailCard(Player player) => false;
        public void PassedGo(Player player)
        {
        }
        public void Setup(Board board)
        {
            board.AddPlayer(new Player(board, "Tom")); 
        }
        public void ShowFreedFromJail(Player player, EscapeJailCause cause)
        {
            MessageBox.Show(this, $"{player} freed from jail due to {cause}", "Jail Freedom");
        }
        public void ShowPlayerAddedHouse(Player owner, Property prop, bool isHotel)
        {
            MessageBox.Show(this, $"{owner} added a {(isHotel ? "hotel" : "house")}", "Property Upgraded");
        }
        public void ShowPlayerLanded(Player player, BoardSpace space)
        {
            Refresh();
        }
        public void ShowPlayerMoney(Player player) { }
        public void ShowPlayerPaidRent(Player player, Player owner, BoardSpace space, decimal amount) =>
            MessageBox.Show(this, $"{player} paid {owner} {amount:C} at {space}");

        public void ShowPlayerPickedCard(Player player, Card card)
        {
            MessageBox.Show(this, $"{player} picked card {card}", "Card Picked");
        }
        public void ShowPlayerRolled(Player player, int dice1, int dice2, bool doubles)
        {
            MessageBox.Show(this, $"{player} rolled a {dice1} and a {dice2}");
        }
        public void ShowSentToJail(Player player) =>
             MessageBox.Show(this, $"{player} sent to jail");

        public void ShowStillInJail(Player player) =>
            MessageBox.Show(this, $"{player} still in jail");

        public void ShowTaxed(Player player, TaxSpace space) =>
            MessageBox.Show(this, $"{player} taxed at {space.Tax:C}");
    }
}
