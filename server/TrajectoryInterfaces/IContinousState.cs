namespace TrajectoryInterfaces
{
    public interface IContinousState
    {
        double Alpha { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double GetDistance(IContinousState other);
    }
}