using VRPMS.VRPCD.Helpers;
using VRPMS.VRPCD.Methods.BasicSolutionMethods;
using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Methods.SolutionSearchMethods;
using VRPMS.VRPCD.Models;

namespace VRPMS.VRPCD;

public class Solver
{
    public Solver(Problem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException(nameof(problem), ErrorMessages.ProblemCannotBeNull);
        }

        BestSolutionSolver = new TabuSearchMethod();
        BasicSolver = new NearestNeighborMethod(problem);
    }

    public BasicSolutionMethodBase BasicSolver { get; private set; }

    public SolutionSearchMethodBase BestSolutionSolver { get; private set; }

    public Solution Solve()
    {
        Solution initialSolution = BasicSolver.Solve();
        BestSolutionSolver.BasicSolutionData = BasicSolver;
        return BestSolutionSolver.Search();
    }
}
