namespace Miner_WPF.Models.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private double computeAbility;

        public double ComputeAbility { get => computeAbility; set => SetProperty(ref computeAbility, value); }
    }
}
