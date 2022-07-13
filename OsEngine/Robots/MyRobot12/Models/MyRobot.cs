using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Robots.MyRobot12.Models
{
    public class MyRobot1 : BotPanel
    {
        public MyRobot1(string name, StartProgram startProgram) : base(name, startProgram)
        {
            this.TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            this.CreateParameter("Mode", "Edit", new[] { "Edit", "Trade" });
            CreateParameter("Lot", 2, 1, 100, 1);
            CreateParameter("Stop", 38, 1, 100, 1);
            CreateParameter("Take", 90, 1, 100, 1);

            CreateParameter("Lot1", 2, 1, 100, 1);
            CreateParameter("Stop1", 65, 1, 100, 1);
            CreateParameter("Take1", 100, 1, 100, 1);
        }

        #region Fields ========================================================
        BotTabSimple _tab;
        #endregion

        public override string GetNameStrategyType()
        {
            return nameof(MyRobot1);
        }

        public override void ShowIndividualSettingsDialog()
        {
            WindowMyRobot window = new WindowMyRobot(this);

            StrategyParameterString paramString = (StrategyParameterString)Parameters[0];

            StrategyParameterInt paramInt = (StrategyParameterInt)Parameters[1];
            window.TextRobot.Text = "Lot = " + paramInt.ValueInt;

            StrategyParameterInt paramStop = (StrategyParameterInt)Parameters[2];
            window.Stop.Text = "Stop = " + paramStop.ValueInt;

            StrategyParameterInt paramTake = (StrategyParameterInt)Parameters[3];
            window.Take.Text = "Take = " + paramTake.ValueInt;

            window.ShowDialog();
        }
    }
}
