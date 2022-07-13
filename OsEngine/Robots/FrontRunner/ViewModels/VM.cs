using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Robots.FrontRunner.Entity;
using OsEngine.Robots.FrontRunner.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace OsEngine.Robots.FrontRunner.ViewModels
{
    public class VM : BaseVM
    {
        #region Fields ========================================================
        FrontRunnerBot bot;
        #endregion

        #region Constructors ==================================================
        public VM(FrontRunnerBot frontRunnerBot)
        {
            bot = frontRunnerBot;
            bot.PropertyChanged += bot_PropertyChanged;
        }
        #endregion

        #region Properties ====================================================
        public decimal BigVolume
        {
            get { return bot.BigVolume; }
            set
            {
                if (value < 2 || value / bot.CoefficientBigVolume <= Lot)
                    return;

                bot.BigVolume = value;
                OnPropertyChanged(nameof(BigVolume));
            }
        }

        public int Offset
        {
            get { return bot.Offset; }
            set
            {
                if (value < 1)
                    return;

                bot.Offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public int Take
        {
            get { return bot.Take; }
            set
            {
                if (value < 1)
                    return;
 
                bot.Take = value;
                OnPropertyChanged(nameof(Take));
            }
        }

        public decimal Lot
        {
            get { return bot.Lot; }
            set
            {
                if (value < 1 || value >= BigVolume / bot.CoefficientBigVolume)
                    return;

                bot.Lot = value;
                OnPropertyChanged(nameof(Lot));
            }
        }

        public Edit ButtonStopStart
        {
            get { return bot.ButtonStopStart; }
            set
            {
                bot.ButtonStopStart = value;
                OnPropertyChanged(nameof(ButtonStopStart));
            }
        }

        public PositionStateType PositionState
        {
            get { return bot.PositionState; }
            set { bot.PositionState = value; }
        }

        public string Code
        {
            get { return bot.Code; }
            set { bot.Code = value; }
        }

        public decimal PriceCode
        {
            get { return bot.PriceCode; }
            set { bot.PriceCode = value; }
        }

        public decimal VolumePosition
        {
            get { return bot.VolumePosition; }
            set { bot.VolumePosition = value; }
        }

        public decimal WaitVolumePosition
        {
            get { return bot.WaitVolumePosition; }
            set { bot.WaitVolumePosition = value; }
        }

        public decimal PricePosition
        {
            get { return bot.PricePosition; }
            set { bot.PricePosition = value; }
        }

        public decimal TakeProfit
        {
            get { return bot.TakeProfit; }
            set { bot.TakeProfit = value; }
        }

        public decimal ProfitPosition
        {
            get { return bot.ProfitPosition; }
            set { bot.ProfitPosition = value; }
        }

        public decimal HistoryPosition
        {
            get { return bot.HistoryPosition; }
            set { bot.HistoryPosition = value; }
        }

        public decimal ProfitHistory
        {
            get { return bot.ProfitHistory; }
            set { bot.ProfitHistory = value; }
        }

        public decimal AllProfit
        {
            get { return bot.AllProfit; }
            set { bot.AllProfit = value; }
        }
        #endregion

        #region Commands ======================================================
        DelegateCommand commandStart;
        public ICommand CommandStart
        {
            get
            {
                if (commandStart == null)
                    commandStart = new DelegateCommand(Start);

                return commandStart;
            }
        }
        #endregion

        #region Methods =======================================================
        private void Start(object obj)
        {
            if (ButtonStopStart == Edit.Start)
                ButtonStopStart = Edit.Stop;
            else
                ButtonStopStart = Edit.Start;
        }

        private void bot_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PositionState):
                    OnPropertyChanged(nameof(PositionState));
                    break;
                case nameof(Code):
                    OnPropertyChanged(nameof(Code));
                    break;
                case nameof(PriceCode):
                    OnPropertyChanged(nameof(PriceCode));
                    break;
                case nameof(VolumePosition):
                    OnPropertyChanged(nameof(VolumePosition));
                    break;
                case nameof(WaitVolumePosition):
                    OnPropertyChanged(nameof(WaitVolumePosition));
                    break;
                case nameof(PricePosition):
                    OnPropertyChanged(nameof(PricePosition));
                    break;
                case nameof(TakeProfit):
                    OnPropertyChanged(nameof(TakeProfit));
                    break;
                case nameof(ProfitPosition):
                    OnPropertyChanged(nameof(ProfitPosition));
                    break;
                case nameof(HistoryPosition):
                    OnPropertyChanged(nameof(HistoryPosition));
                    break;
                case nameof(ProfitHistory):
                    OnPropertyChanged(nameof(ProfitHistory));
                    break;
                case nameof(AllProfit):
                    OnPropertyChanged(nameof(AllProfit));
                    break;
            }
        }
        #endregion
    }
}
