using Contract;

namespace Miner_WPF.Models.ViewModels
{
    public class ConfigWindowViewModel : BindableBase
    {
        private bool isAlive;
        private double computeAbility;
        private double accept;
        private double reject;
        public Config Config { set; get; }
        public bool IsAlive { get => isAlive; set => SetProperty(ref isAlive, value); }
        public double ComputeAbility { get => computeAbility; set => SetProperty(ref computeAbility, value); }
        public double Accept { get => accept; set => SetProperty(ref accept, value); }
        public double Reject { get => reject; set => SetProperty(ref reject, value); }
        public void SetPerformance(IPerformance perfmon)
        {
            ComputeAbility = perfmon.Hashrate;
            Accept = perfmon.Accept;
            Reject = perfmon.Sum - perfmon.Accept;
        }
    }
}
