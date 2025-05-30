using VRPMS.VRPCD.Methods.SolutionMethods;
using VRPMS.VRPCD.Models;

namespace VRPMS.VRPCD;

public class Solver
{
    public Solver(SolutionMethodBase basicSolver)
    {
        BasicSolver = basicSolver;
    }

    public SolutionMethodBase BasicSolver { get; set; }

    public Solution Solve(Problem problem)
    {
        return BasicSolver.Solve(problem);
    }
}
