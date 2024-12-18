
var currentPortfolio = new Dictionary<string, (double quantity, double price)>
{
    { "ISIN1", (100, 10.0) },
    { "ISIN2", (50, 20.0) },
    { "ISIN3", (200, 15.0) }
};

var targetModel = new Dictionary<string, double>
{
    { "ISIN1", 150 },
    { "ISIN2", 30 },
    { "ISIN3", 200 },
    { "ISIN4", 600 }
};

var isinsToKeep = new HashSet<string> { "ISIN3" };
var isinsToAvoid = new HashSet<string> { "ISIN4" };
double budget = 500;

PortfolioOptimizer.OptimizePortfolio(currentPortfolio, targetModel, isinsToKeep,isinsToAvoid, budget);