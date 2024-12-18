using System;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;


public static class PortfolioOptimizer
{
    public  static Dictionary<string, (string action, int quantity)> OptimizePortfolio(
        Dictionary<string, (double quantity, double price)> currentPortfolio,
        Dictionary<string, double> targetModel,
        HashSet<string> isinsToKeep,
        HashSet<string> isinsToAvoid,
        double budget)
    {
        var result = new Dictionary<string, (string action, int quantity)>();
        // Create the solver instance (SCIP or another supported solver).
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
        {
            Console.WriteLine("Could not create solver.");
            return result;
        }

        // Variables: Adjustments for buying and selling
        var buyVars = new Dictionary<string, Variable>();
        var sellVars = new Dictionary<string, Variable>();

        foreach (var isin in targetModel.Keys)
        {
            if (isinsToKeep.Contains(isin))
            {
                // Fixed ISINs: No buying or selling
                buyVars[isin] = solver.MakeNumVar(0.0, 0.0, $"buy_{isin}");
                sellVars[isin] = solver.MakeNumVar(0.0, 0.0, $"sell_{isin}");
            }
            else
            {
                // Allow buying and selling for other ISINs
                buyVars[isin] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"buy_{isin}");
                sellVars[isin] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"sell_{isin}");
            }
        }

        // Constraint 1: Budget constraint
        Constraint budgetConstraint = solver.MakeConstraint(0.0, budget, "budget");
        foreach (var isin in targetModel.Keys)
        {
            if (currentPortfolio.TryGetValue(isin, out var info))
            {
                double price = info.price;
                budgetConstraint.SetCoefficient(buyVars[isin], price);
                budgetConstraint.SetCoefficient(sellVars[isin], -price); // Selling adds cash
            }
        }

        // Constraint 2: Target quantities
        foreach (var isin in targetModel.Keys)
        {
            if(isinsToAvoid.Contains(isin)) continue;
            double targetQuantity = targetModel[isin];
            double currentQuantity = currentPortfolio.ContainsKey(isin) ? currentPortfolio[isin].quantity : 0.0;

            // Adjust buy and sell to meet the target quantity
            Constraint targetConstraint = solver.MakeConstraint(targetQuantity, targetQuantity, $"target_{isin}");
            targetConstraint.SetCoefficient(buyVars[isin], 1.0);
            targetConstraint.SetCoefficient(sellVars[isin], -1.0);
            targetConstraint.SetBounds(targetQuantity - currentQuantity, targetQuantity - currentQuantity);
        }

        // Objective: Minimize the total transactions (buy + sell)
        Objective objective = solver.Objective();
        foreach (var isin in targetModel.Keys)
        {
            objective.SetCoefficient(buyVars[isin], 1.0);
            objective.SetCoefficient(sellVars[isin], 1.0);
        }
        objective.SetMinimization();

        // Solve the optimization problem
        Solver.ResultStatus resultStatus = solver.Solve();

        // Output the results
        if (resultStatus == Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("Optimal solution found:");
            foreach (var isin in targetModel.Keys)
            {
                double buy = buyVars[isin].SolutionValue();
                double sell = sellVars[isin].SolutionValue();
                string action = buy > 0 ? "Buy" : (sell > 0 ? "Sell" : "Keep");              
                Console.WriteLine($"ISIN: {isin}, Action: {action}, Quantity: {Math.Max(buy, sell)}");
                result.Add(isin, (action, (int)Math.Max(buy, sell))); 
            }
            Console.WriteLine($"Total Cost: {objective.Value()}");
        }
        else
        {
            Console.WriteLine("No optimal solution found.");
        }
        return result;
    }
}
