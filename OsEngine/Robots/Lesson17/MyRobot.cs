using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System.Collections.Generic;
using System.IO;

namespace OsEngine.Robots.Lesson17
{
    public class MyRobot : BotPanel
    {
        public MyRobot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            this.TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            this.CreateParameter("Mode", "Edit", new[] { "Edit", "Trade" });
            _risk = CreateParameter("Risk %", 1m, 0.1m, 10m, 0.1m);
            _profitKoef = CreateParameter("Koef Profit", 1m, 0.1m, 10m, 0.1m);
            _countDownCandles = CreateParameter("Count Down Candles", 1, 1, 5, 1);
            _koefVolume = CreateParameter("Koef Volume", 2m, 2m, 10m, 0.5m);
            _countCandles = CreateParameter("Count Candles", 10, 5, 50, 1);

            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
            _tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent;
            _tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent;
        }

        #region Fields ========================================================
        private BotTabSimple _tab;

        /// <summary>
        /// Риск на сделку
        /// </summary>
        private StrategyParameterDecimal _risk;

        /// <summary>
        /// Во сколько раз тейк больше риска
        /// </summary>
        private StrategyParameterDecimal _profitKoef;

        /// <summary>
        /// Количество падающих свечей перед объемным разворотом
        /// </summary>
        private StrategyParameterInt _countDownCandles;

        /// <summary>
        /// Во сколько раз объем превышает средний
        /// </summary>
        private StrategyParameterDecimal _koefVolume;

        /// <summary>
        /// Средний объем
        /// </summary>
        private decimal _averageVolume;

        /// <summary>
        /// Количество пунктов до стоп лосса
        /// </summary>
        private decimal _punkts = 0;

        private decimal _lowCandle = 0;

        /// <summary>
        /// Количество свечей для вычисления среднего объема
        /// </summary>
        private StrategyParameterInt _countCandles;
        #endregion

        #region Methods =======================================================
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            Candle candle = candles[candles.Count - 1];
            
            List<Position> positions = _tab.PositionOpenLong;
            if (positions.Count > 0)
            {
                //if (positions[0].StopOrderIsActiv &&
                //    positions[0].StopOrderPrice < positions[0].EntryPrice &&
                //    candle.Close >= positions[0].EntryPrice + (positions[0].EntryPrice - positions[0].StopOrderPrice))
                //{
                //    _tab.CloseAtStop(positions[0], positions[0].EntryPrice, positions[0].EntryPrice - 100 * _tab.Securiti.PriceStep);
                //}

                return;
            }

            if (candles.Count < _countDownCandles.ValueInt + 1 ||
                candles.Count < _countCandles.ValueInt + 1)
            {
                return;
            }

            _averageVolume = 0;
            for (int i = candles.Count - 2; i > candles.Count - 2 - _countCandles.ValueInt; i--)
            {
                _averageVolume += candles[i].Volume;
            }
            _averageVolume /= _countCandles.ValueInt;

            if (candle.Close < (candle.High + candle.Low) / 2 ||
                candle.Volume < _averageVolume * _koefVolume.ValueDecimal)
            {
                return;
            }

            for (int i = candles.Count - 2; i > candles.Count - 2 - _countDownCandles.ValueInt; i--)
            {
                if (candles[i].Close > candles[i].Open)
                {
                    return;
                }
            }

            _punkts = candle.Close - candle.Low;
            if (_punkts < 5 * _tab.Securiti.PriceStep)
            {
                return;
            }

            decimal amountStop = _punkts / _tab.Securiti.PriceStep * _tab.Securiti.PriceStepCost;
            decimal amountRisk = _tab.Portfolio.ValueBegin * _risk.ValueDecimal / 100;
            decimal volume = amountRisk / amountStop;
            decimal go = 10000;
            if (_tab.Securiti.Go > 1)
            {
                go = _tab.Securiti.Go;
            }
            decimal maxLot = _tab.Portfolio.ValueBegin / go;
            if (volume >= maxLot)
            {
                volume = maxLot;
            }
            volume /= 2;

            _lowCandle = candle.Low;
            _tab.BuyAtMarket(volume);
            _tab.BuyAtMarket(volume);
        }

        private void _tab_PositionOpeningSuccesEvent(Position pos)
        {
            List<Position> positions = _tab.PositionOpenLong;

            decimal _koef = 1;
            for (int i = 0; i < positions.Count; i++)
            {
                if (i == 1)
                {
                    _koef = _profitKoef.ValueDecimal;
                }
                
                decimal take = positions[i].EntryPrice + ((positions[i].EntryPrice - _lowCandle) * _koef);
                _tab.CloseAtProfit(positions[i], take, take);

                _tab.CloseAtStop(positions[i], _lowCandle, _lowCandle - 100 * _tab.Securiti.PriceStep);
            }
        }

        private void _tab_PositionClosingSuccesEvent(Position pos)
        {
            SaveCSV(pos);
        }

        private void SaveCSV(Position pos)
        {
            if (!File.Exists(@"Engine\trades.csv")) // файл не существует
            {
                string header = ";Позиция;Символ;Лоты;Изменение/Максимум" +
                                " Лотов;Исполнение входа;Сигнал входа;Бар" +
                                " входа;Дата входа;Время входа;Цена входа;" +
                                "Комиссия входа;Исполнение выхода;Сигнал" +
                                " выхода;Бар выхода;Дата выхода;Время выхода;" +
                                "Цена выхода;Комиссия выхода;Средневзвешенная" +
                                " цена входа;П/У;П/У сделки;П/У с одного лота;" +
                                "Зафиксированная П/У;Открытая П/У;Продолж." +
                                " (баров);Доход/Бар;Общий П/У;% изменения;MAE;" +
                                "MAE %;MFE;MFE %";

                using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", false))
                {
                    writer.WriteLine(header);
                    writer.Close();
                }
            }

            using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", true))
            {
                string str = ";;;;;;;;" + pos.TimeOpen.ToShortDateString();
                str += ";" + pos.TimeOpen.TimeOfDay;
                str += ";;;;;;;;;;;;;;" + pos.ProfitPortfolioPunkt + ";;;;;;;;;";

                writer.WriteLine(str);
                writer.Close();
            }
        }

        public override string GetNameStrategyType()
        {
            return nameof(MyRobot);
        }

        public override void ShowIndividualSettingsDialog()
        {
            WindowMyRobot window = new WindowMyRobot(this);
            window.ShowDialog();
        }
        #endregion
    }
}
