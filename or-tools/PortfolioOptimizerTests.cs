using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioOptimizerTests
{
    [TestFixture]
    public class PortfolioOptimizerTests
    {
        [Test]
        public void OptimizePortfolio_CurrentPortfolioDoesNotMatchTargetModel_ReturnsOptimizedPortfolioWithoutAvoidedIsins()
        {
            // Arrange
            var currentPortfolio = new Dictionary<string, (double quantity, double price)>
            {
                {"ISIN1", (10, 100.0)},
                {"ISIN2", (20, 200.0)}
            };
            var targetModel = new Dictionary<string, double>
            {
                {"ISIN1", 15},
                {"ISIN2", 25},
                {"ISIN3", 35}
            };
            var isinsToKeep = new HashSet<string>();
            var isinsToAvoid = new HashSet<string> { "ISIN3" };
            var budget = 1000.0;

            // Act
            var optimizedPortfolio = PortfolioOptimizer.OptimizePortfolio(currentPortfolio, targetModel, isinsToKeep, isinsToAvoid, budget);

            // Assert
            Assert.That(optimizedPortfolio, Has.Count.EqualTo(2));
            Assert.That(optimizedPortfolio["ISIN1"].quantity, Is.EqualTo(15));
            //Assert.That(optimizedPortfolio["ISIN1"].price, Is.EqualTo(100m));
            Assert.That(optimizedPortfolio["ISIN2"].quantity, Is.EqualTo(25));
            //Assert.That(optimizedPortfolio["ISIN2"].price, Is.EqualTo(200m));
            Assert.That(optimizedPortfolio.ContainsKey("ISIN3"), Is.False);
        }
    }
}