namespace Variogram
{
    public interface IVariogram
    {
        double Nugget { get; }
        double Range { get; }
        double Sill { get; }

        double GetValue(double distace);
    }
}