using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsEngine.Robots.PriceChannel
{
    public class PriceChannelFix : BotPanel
    {
        #region Fields ========================================================
        private BotTabSimple _tab;
        private Aindicator _pc;
        private StrategyParameterInt LengthUp;
        private StrategyParameterInt LengthDown;
        private StrategyParameterString Mode;
        private StrategyParameterInt Lot;
        private StrategyParameterDecimal Risk;
        #endregion

        #region Constructors ==================================================
        public PriceChannelFix(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            LengthUp = CreateParameter("Length Channel Up", 12, 5, 80, 2);
            LengthDown = CreateParameter("Length Channel Down", 12, 5, 80, 2);
            Mode = CreateParameter("Mode", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosePosition" });
            Lot = CreateParameter("Lot", 10, 5, 20, 1);
            Risk = CreateParameter("Risk", 1m, 0.2m, 3m, 0.1m);
            _pc = IndicatorsFactory.CreateIndicatorByName("PriceChannel", name + "PriceChannel", false);
            _pc.ParametersDigit[0].Value = LengthUp.ValueInt;
            _pc.ParametersDigit[1].Value = LengthDown.ValueInt;
            _pc = (Aindicator)_tab.CreateCandleIndicator(_pc, "Prime");
            _pc.Save();
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
        }
        #endregion

        #region Delegates =====================================================
        #endregion

        #region Events ========================================================
        #endregion

        #region Properties ====================================================
        #endregion

        #region Methods =======================================================
        public override string GetNameStrategyType()
        {
            return nameof(PriceChannelFix);
        }

        public override void ShowIndividualSettingsDialog()
        {
            throw new NotImplementedException();
        }

        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (Mode.ValueString == "Off")
                return;

            if (_pc.DataSeries[0].Values == null ||
                _pc.DataSeries[1].Values == null ||
                _pc.DataSeries[0].Values.Count < LengthUp.ValueInt + 1 ||
                _pc.DataSeries[1].Values.Count < LengthDown.ValueInt + 1)
                return;

            List<Position> positions = _tab.PositionsOpenAll;

            if (Mode.ValueString == "OnlyClosePosition")
            {
                PositionStateType stateType = PositionStateType.Done;

                foreach (var pos in positions)
                {
                    if (pos.State != PositionStateType.Done)
                    {
                        stateType = pos.State;
                        break;
                    }
                }

                if (stateType == PositionStateType.Done)
                    return;
            }

            Candle candle = candles[candles.Count - 1]; // последняя свеча
            decimal lastUp = _pc.DataSeries[0].Values[_pc.DataSeries[0].Values.Count - 2]; // предпоследняя свеча
            decimal lastDown = _pc.DataSeries[1].Values[_pc.DataSeries[1].Values.Count - 2]; // предпоследняя свеча

            if (positions.Count == 0)
            {
                decimal riskMany = _tab.Portfolio.ValueBegin * Risk.ValueDecimal / 100;
                decimal costPriceStep = _tab.Securiti.PriceStepCost;
                costPriceStep = 0.01m; //===

                decimal steps = (lastUp - lastDown) / _tab.Securiti.PriceStep;
                decimal lot = riskMany / (steps * costPriceStep);

                if (Mode.ValueString != "OnlyShort" &&
                    candle.Close > lastUp &&
                    candle.Open < lastUp)
                {
                    _tab.BuyAtMarket((int)lot);
                }

                if (Mode.ValueString != "OnlyLong" &&
                    candle.Close < lastDown &&
                    candle.Open > lastDown)
                {
                    _tab.SellAtMarket((int)lot);
                }
            }

            Trailing(positions);
        }

        private void Trailing(List<Position> positions)
        {
            if (positions.Count > 0)
            {
                decimal lastUp = _pc.DataSeries[0].Values.Last();
                decimal lastDown = _pc.DataSeries[1].Values.Last();

                foreach (var pos in positions)
                {
                    if (pos.State == PositionStateType.Open)
                    {
                        if (pos.Direction == Side.Buy)
                        {
                            _tab.CloseAtTrailingStop(pos, lastDown, lastDown - 100 * _tab.Securiti.PriceStep);
                        }

                        if (pos.Direction == Side.Sell)
                        {
                            _tab.CloseAtTrailingStop(pos, lastUp, lastUp + 100 * _tab.Securiti.PriceStep);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
