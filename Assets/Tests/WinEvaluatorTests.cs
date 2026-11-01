using System.Collections.Generic;
using NUnit.Framework;
using SlotMachine.Data;
using SlotMachine.Logic;
using UnityEngine;

namespace SlotMachine.Tests
{
    [TestFixture]
    public class WinEvaluatorTests
    {
        private PaytableConfig _config;
        private SymbolData _symbolSeven;
        private SymbolData _symbolBell;
        private SymbolData _symbolCherry;
        private SymbolData _symbolWild;
        private SymbolData _symbolScatter;

        [SetUp]
        public void Setup()
        {
            _config = ScriptableObject.CreateInstance<PaytableConfig>();
            _config.InitializeDefaultPaylines();

            _symbolSeven = ScriptableObject.CreateInstance<SymbolData>();
            SetSymbolFields(_symbolSeven, "lucky_seven", "Lucky Seven", SymbolType.Standard, 50f, 5f, 40);

            _symbolBell = ScriptableObject.CreateInstance<SymbolData>();
            SetSymbolFields(_symbolBell, "bell", "Golden Bell", SymbolType.Standard, 15f, 2f, 90);

            _symbolCherry = ScriptableObject.CreateInstance<SymbolData>();
            SetSymbolFields(_symbolCherry, "cherry", "Cherries", SymbolType.Standard, 8f, 1.5f, 120);

            _symbolWild = ScriptableObject.CreateInstance<SymbolData>();
            SetSymbolFields(_symbolWild, "wild", "Wild Joker", SymbolType.Wild, 100f, 10f, 20);

            _symbolScatter = ScriptableObject.CreateInstance<SymbolData>();
            SetSymbolFields(_symbolScatter, "scatter", "Star Scatter", SymbolType.Scatter, 0f, 0f, 25);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_symbolSeven);
            Object.DestroyImmediate(_symbolBell);
            Object.DestroyImmediate(_symbolCherry);
            Object.DestroyImmediate(_symbolWild);
            Object.DestroyImmediate(_symbolScatter);
        }

        private void SetSymbolFields(SymbolData sym, string id, string name, SymbolType type, float p3, float p2, int weight)
        {
            var typeObj = typeof(SymbolData);
            typeObj.GetField("symbolId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, id);
            typeObj.GetField("symbolName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, name);
            typeObj.GetField("symbolType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, type);
            typeObj.GetField("payout3Match", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, p3);
            typeObj.GetField("payout2Match", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, p2);
            typeObj.GetField("dropWeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sym, weight);
        }

        [Test]
        public void EvaluateGrid_ThreeSevensOnCenterLine_AwardsExactPayout()
        {
            // Center line is row 1
            SymbolData[,] grid = new SymbolData[3, 3]
            {
                { _symbolCherry, _symbolSeven, _symbolBell },
                { _symbolBell,   _symbolSeven, _symbolCherry },
                { _symbolCherry, _symbolSeven, _symbolBell }
            };

            int bet = 20;
            WinResult result = WinEvaluator.EvaluateGrid(grid, _config, bet);

            Assert.IsTrue(result.HasAnyWin);
            Assert.AreEqual(1, result.WinningPaylines.Count);
            Assert.AreEqual("Center Line", result.WinningPaylines[0].Payline.LineName);
            Assert.AreEqual(50f, result.WinningPaylines[0].Multiplier);
            Assert.AreEqual(1000, result.TotalWinAmount); // 50 * 20 = 1000
            Assert.IsTrue(result.IsJackpotWon);
        }

        [Test]
        public void EvaluateGrid_WildSubstitution_CompletesPayline()
        {
            // Center line: Seven, Wild, Seven
            SymbolData[,] grid = new SymbolData[3, 3]
            {
                { _symbolCherry, _symbolSeven, _symbolBell },
                { _symbolBell,   _symbolWild,  _symbolCherry },
                { _symbolCherry, _symbolSeven, _symbolBell }
            };

            int bet = 10;
            WinResult result = WinEvaluator.EvaluateGrid(grid, _config, bet);

            Assert.IsTrue(result.HasAnyWin);
            Assert.AreEqual(1, result.WinningPaylines.Count);
            Assert.AreEqual(_symbolSeven.SymbolId, result.WinningPaylines[0].WinningSymbol.SymbolId);
            Assert.AreEqual(3, result.WinningPaylines[0].MatchCount);
            Assert.AreEqual(500, result.TotalWinAmount); // 50 * 10 = 500
        }

        [Test]
        public void EvaluateGrid_ThreeScatters_TriggersFreeSpins()
        {
            SymbolData[,] grid = new SymbolData[3, 3]
            {
                { _symbolScatter, _symbolCherry,  _symbolBell },
                { _symbolBell,    _symbolScatter, _symbolCherry },
                { _symbolCherry,  _symbolBell,    _symbolScatter }
            };

            int bet = 20;
            WinResult result = WinEvaluator.EvaluateGrid(grid, _config, bet);

            Assert.AreEqual(3, result.ScatterCount);
            Assert.IsTrue(result.IsFreeSpinsTriggered);
            Assert.AreEqual(10, result.FreeSpinsAwarded);
        }

        [Test]
        public void EvaluateGrid_NoMatches_ReturnsZeroWin()
        {
            SymbolData[,] grid = new SymbolData[3, 3]
            {
                { _symbolSeven,  _symbolBell,   _symbolCherry },
                { _symbolBell,   _symbolCherry, _symbolSeven },
                { _symbolCherry, _symbolSeven,  _symbolBell }
            };

            int bet = 20;
            WinResult result = WinEvaluator.EvaluateGrid(grid, _config, bet);

            Assert.IsFalse(result.HasAnyWin);
            Assert.AreEqual(0, result.TotalWinAmount);
            Assert.AreEqual(0, result.WinningPaylines.Count);
        }

        [Test]
        public void RNGManager_SeededExecution_ProducesIdenticalSequences()
        {
            var rngA = new RNGManager(12345);
            var rngB = new RNGManager(12345);

            var symbols = new List<SymbolData> { _symbolSeven, _symbolBell, _symbolCherry, _symbolWild };

            for (int i = 0; i < 20; i++)
            {
                var symA = rngA.GetWeightedRandomSymbol(symbols);
                var symB = rngB.GetWeightedRandomSymbol(symbols);
                Assert.AreEqual(symA.SymbolId, symB.SymbolId);
            }
        }
    }
}

