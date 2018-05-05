namespace Contract
{
    public interface IPerformance
    {
        int Sum { get; }
        int Accept { get; }
        double Hashrate { get; }
        double Difficulty { get; }
        void SetPerformance(int sum, int accept, double hashrate, double difficulty);
    }
}
