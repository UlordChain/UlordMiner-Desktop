using Contract;

namespace Ulord
{
    public class Performance : IPerformance
    {
        private int sum;
        private int accept;
        private double hashrate;
        private double difficulty;

        public int Sum { get => sum; internal set => sum = value; }

        public int Accept { get => accept; internal set => accept = value; }

        public double Hashrate { get => hashrate; internal set => hashrate = value; }

        public double Difficulty { get => difficulty; internal set => difficulty = value; }

        public void SetPerformance(int sum, int accept, double hashrate, double difficulty)
        {
            Sum = sum;
            Accept = accept;
            Hashrate = hashrate;
            Difficulty = difficulty;
        }
    }
}
