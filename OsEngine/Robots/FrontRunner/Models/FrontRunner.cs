using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.FrontRunner.Entity;
using OsEngine.Robots.FrontRunner.Views;
using System.ComponentModel;

namespace OsEngine.Robots.FrontRunner.Models
{
    public class FrontRunnerBot : BotPanel, INotifyPropertyChanged
    {
        #region Fields ========================================================
        public decimal BigVolume = 100;
        public int CoefficientBigVolume = 2;
        public int Offset = 1;
        public int Take = 28;
        public decimal Lot = 2;

        public BotTabSimple tab;
        public Position position;

        private decimal VolumeForTake;
        #endregion

        #region Constructors ==================================================
        public FrontRunnerBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            tab = TabsSimple[0];
            position = tab.PositionsLast;
            tab.MarketDepthUpdateEvent += tab_MarketDepthUpdateEvent;
        }
        #endregion

        #region Delegates =====================================================
        #endregion

        #region Events ========================================================
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties ====================================================
        private Edit buttonStopStart = Edit.Stop;
        public Edit ButtonStopStart
        {
            get { return buttonStopStart; }
            set
            {
                buttonStopStart = value;

                if (buttonStopStart == Edit.Stop)
                {
                    TakeProfit = 0;
                    tab.CloseAllOrderInSystem();
                }
            }
        }

        private PositionStateType positionState;
        public PositionStateType PositionState
        {
            get { return positionState; }
            set
            {
                if (value != positionState)
                {
                    positionState = value;
                    OnPropertyChanged(nameof(PositionState));
                }
            }
        }

        private string code;
        public string Code
        {
            get { return code; }
            set
            {
                if (value != code)
                {
                    code = value;
                    OnPropertyChanged(nameof(Code));
                }
            }
        }

        private decimal priceCode;
        public decimal PriceCode
        {
            get { return priceCode; }
            set
            {
                if (value != priceCode)
                {
                    priceCode = value;

                    if (position.Direction == Side.Sell)
                        priceCode = tab.PriceBestAsk;

                    OnPropertyChanged(nameof(PriceCode));
                }
            }
        }

        private decimal volumePosition;
        public decimal VolumePosition
        {
            get { return volumePosition; }
            set
            {
                if (value != volumePosition)
                {
                    volumePosition = value;

                    if (position.Direction == Side.Sell)
                        volumePosition = -volumePosition;

                    OnPropertyChanged(nameof(VolumePosition));
                }
            }
        }

        private decimal waitVolumePosition;
        public decimal WaitVolumePosition
        {
            get { return waitVolumePosition; }
            set
            {
                if (value != waitVolumePosition)
                {
                    waitVolumePosition = value;

                    if (position.Direction == Side.Sell)
                        waitVolumePosition = -waitVolumePosition;

                    OnPropertyChanged(nameof(WaitVolumePosition));
                }
            }
        }

        private decimal pricePosition;
        public decimal PricePosition
        {
            get { return pricePosition; }
            set
            {
                if (VolumePosition == 0 && WaitVolumePosition == 0)
                {
                    pricePosition = 0;
                    OnPropertyChanged(nameof(PricePosition));
                }
                else if (value != pricePosition)
                {
                    pricePosition = value;
                    OnPropertyChanged(nameof(PricePosition));
                }
            }
        }

        private decimal takeProfit;
        public decimal TakeProfit
        {
            get { return takeProfit; }
            set
            {
                if (value != takeProfit)
                {
                    takeProfit = value;
                    OnPropertyChanged(nameof(TakeProfit));
                }
            }
        }

        private decimal profitPosition;
        public decimal ProfitPosition
        {
            get { return profitPosition; }
            set
            {
                if (VolumePosition == 0)
                {
                    profitPosition = 0;
                    OnPropertyChanged(nameof(ProfitPosition));
                }
                else if (value != profitPosition)
                {
                    profitPosition = value;
                    OnPropertyChanged(nameof(ProfitPosition));
                }
            }
        }

        private decimal historyPosition;
        public decimal HistoryPosition
        {
            get { return historyPosition; }
            set
            {
                if (value != historyPosition)
                {
                    historyPosition = value;
                    OnPropertyChanged(nameof(HistoryPosition));
                }
            }
        }

        private decimal profitHistory;
        public decimal ProfitHistory
        {
            get { return profitHistory; }
            set
            {
                if (value != profitHistory)
                {
                    profitHistory = value;
                    OnPropertyChanged(nameof(ProfitHistory));
                }
            }
        }

        private decimal allProfit;
        public decimal AllProfit
        {
            get { return allProfit; }
            set
            {
                if (value != allProfit)
                {
                    allProfit = value;
                    OnPropertyChanged(nameof(AllProfit));
                }
            }
        }
        #endregion

        #region Methods =======================================================
        public override string GetNameStrategyType()
        {
            return "FrontRunnerBot";
        }

        public override void ShowIndividualSettingsDialog()
        {
            FrontRunnerUi frontRunnerUi = new FrontRunnerUi(this);
            frontRunnerUi.Show();
        }

        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void tab_MarketDepthUpdateEvent(MarketDepth marketDepth)
        {
            if (ButtonStopStart == Edit.Stop)
            {
                OutputInformation();
                return;
            }

            PositionClosed();
            RemoveClosePosition(marketDepth);
            OpenPosition(marketDepth);
            LimitTakeProfit();
            OutputInformation();
        }

        private void PositionClosed()
        {
            if (position != null && position.State == PositionStateType.Done &&
                TakeProfit != 0)
            {
                VolumeForTake = 0;
                TakeProfit = 0;
                HistoryPosition = position.ProfitPortfolioPunkt;
                ProfitHistory += HistoryPosition;
            }
        }

        private void RemoveClosePosition(MarketDepth marketDepth)
        {
            if (position != null)
            {
                decimal positionEntryPrice;
                decimal offset = Offset * tab.Securiti.PriceStep;
                bool priceInGlassForPositionEntryPrice = false;

                if (position.Direction == Side.Buy)
                {
                    positionEntryPrice = position.EntryPrice - offset;

                    for (int i = 0; i < marketDepth.Bids.Count; i++)
                    {
                        if (marketDepth.Bids[i].Price > positionEntryPrice)
                        {
                            if (marketDepth.Bids[i].Bid >= BigVolume &&
                                position.OpenVolume == 0 && position.OpenActiv)
                            {
                                tab.CloseAllOrderInSystem();
                                return;
                            }
                        }
                        else
                        {
                            if (marketDepth.Bids[i].Price == positionEntryPrice)
                                priceInGlassForPositionEntryPrice = true;

                            if ((marketDepth.Bids[i].Price == positionEntryPrice &&
                                 marketDepth.Bids[i].Bid <= BigVolume / CoefficientBigVolume) ||

                                (marketDepth.Bids[i].Price < positionEntryPrice &&
                                 !priceInGlassForPositionEntryPrice))
                            {
                                if (position.OpenActiv || position.CloseActiv)
                                {
                                    tab.CloseAllOrderInSystem();
                                    return;
                                }

                                if (position.OpenVolume > 0)
                                {
                                    tab.CloseAtMarket(position, position.OpenVolume);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (position.Direction == Side.Sell)
                {
                    positionEntryPrice = position.EntryPrice + offset;

                    for (int i = 0; i < marketDepth.Asks.Count; i++)
                    {
                        if (marketDepth.Asks[i].Price < positionEntryPrice)
                        {
                            if (marketDepth.Asks[i].Ask >= BigVolume &&
                                position.OpenVolume == 0 && position.OpenActiv)
                            {
                                tab.CloseAllOrderInSystem();
                                return;
                            }
                        }
                        else
                        {
                            if (marketDepth.Asks[i].Price == positionEntryPrice)
                                priceInGlassForPositionEntryPrice = true;

                            if ((marketDepth.Asks[i].Price == positionEntryPrice &&
                                    marketDepth.Asks[i].Ask <= BigVolume / CoefficientBigVolume) ||

                                (marketDepth.Asks[i].Price > positionEntryPrice &&
                                    !priceInGlassForPositionEntryPrice))
                            {
                                if (position.OpenActiv || position.CloseActiv)
                                {
                                    tab.CloseAllOrderInSystem();
                                    return;
                                }

                                if (position.OpenVolume > 0)
                                {
                                    tab.CloseAtMarket(position, position.OpenVolume);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OpenPosition(MarketDepth marketDepth)
        {
            if (position == null || position.OpenVolume == 0)
            {
                decimal priceLong = 0;
                decimal distanceLong = 0;
                decimal priceShort = 0;
                decimal distanceShort = 0;
                decimal offset = Offset * tab.Securiti.PriceStep;

                for (int i = 0; i < marketDepth.Bids.Count; i++)
                {
                    if (marketDepth.Bids[i].Bid >= BigVolume)
                    {
                        priceLong = marketDepth.Bids[i].Price;
                        distanceLong = marketDepth.Bids[0].Price - priceLong;
                        break;
                    }
                }

                for (int i = 0; i < marketDepth.Asks.Count; i++)
                {
                    if (marketDepth.Asks[i].Ask >= BigVolume)
                    {
                        priceShort = marketDepth.Asks[i].Price;
                        distanceShort = priceShort - marketDepth.Asks[0].Price;
                        break;
                    }
                }

                if ((priceLong > 0 && priceShort == 0) ||
                    (priceLong > 0 && priceShort > 0 && distanceLong < distanceShort))
                {
                    if (position == null || !position.OpenActiv)
                    {
                        priceLong += offset;
                        position = tab.BuyAtLimit(Lot, priceLong);
                        return;
                    }
                    else if (position.Direction == Side.Sell)
                    {
                        tab.CloseAllOrderInSystem();
                        return;
                    }
                }

                if ((priceLong == 0 && priceShort > 0) ||
                    (priceLong > 0 && priceShort > 0 && distanceShort < distanceLong))
                {
                    if (position == null || !position.OpenActiv)
                    {
                        priceShort -= offset;
                        position = tab.SellAtLimit(Lot, priceShort);
                        return;
                    }
                    else if (position.Direction == Side.Buy)
                    {
                        tab.CloseAllOrderInSystem();
                        return;
                    }
                }
            }
        }

        private void LimitTakeProfit()
        {
            if (position != null && position.OpenVolume > 0)
            {
                decimal take = Take * tab.Securiti.PriceStep;

                if (!position.CloseActiv)
                {
                    if (position.Direction == Side.Buy)
                        TakeProfit = position.EntryPrice + take;

                    if (position.Direction == Side.Sell)
                        TakeProfit = position.EntryPrice - take;

                    VolumeForTake = position.OpenVolume;

                    tab.CloseAtLimit(position, TakeProfit, VolumeForTake);
                }
                else
                {
                    if (position.OpenVolume < VolumeForTake)
                        VolumeForTake = position.OpenVolume;

                    if (position.OpenVolume > VolumeForTake)
                        tab.CloseAllOrderInSystem();
                }
            }
        }

        private void OutputInformation()
        {
            if (position != null)
            {
                PositionState = position.State;
                Code = position.SecurityName;
                PriceCode = tab.PriceBestBid;
                VolumePosition = position.OpenVolume;
                WaitVolumePosition = position.WaitVolume;
                PricePosition = position.EntryPrice;
                TakeProfit = TakeProfit;
                ProfitPosition = position.ProfitPortfolioPunkt;
                HistoryPosition = HistoryPosition;
                ProfitHistory = ProfitHistory;
                AllProfit = ProfitPosition + ProfitHistory;
            }
        }
        #endregion
    }
}
