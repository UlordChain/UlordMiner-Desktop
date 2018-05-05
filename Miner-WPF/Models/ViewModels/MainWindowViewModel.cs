namespace Miner_WPF.Models.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private double computeAbility;
        private bool isUP;

        public double ComputeAbility { get => computeAbility; set => SetProperty(ref computeAbility, value); }
        public bool IsUP { get => isUP; set => SetProperty(ref isUP, value); }
    }
}
