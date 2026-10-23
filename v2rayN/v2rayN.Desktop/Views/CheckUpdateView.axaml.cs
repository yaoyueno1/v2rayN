using DialogHostAvalonia;

namespace v2rayN.Desktop.Views;

public partial class CheckUpdateView : ReactiveUserControl<CheckUpdateViewModel>
{
    public CheckUpdateView()
    {
        InitializeComponent();
        btnClose.Click += (_, _) => DialogHost.Close(null);

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.CheckUpdateModels, v => v.lstCheckUpdates.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.EnableUpdateViaProxy, v => v.togEnableUpdateViaProxy.IsChecked).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CheckOnlyCmd, v => v.btnCheckOnly).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CheckUpdateCmd, v => v.btnCheckUpdate).DisposeWith(disposables);
        });
    }

    public void PreviewSample()
    {
        if (ViewModel == null)
        {
            return;
        }

        foreach (var item in ViewModel.CheckUpdateModels)
        {
            item.IsSelected = true;
            item.Remarks = item.CoreTypeForStorage switch
            {
                "v2rayN" => "v2rayN v7.25.6 已是最新版本。",
                "Xray" => "Xray 有新版本可用：v26.10.21",
                "mihomo" => "mihomo 有新版本可用：v1.19.33",
                "sing_box" => "sing_box v1.14.3 已是最新版本。",
                _ => ResUI.menuCheckUpdate,
            };
            item.IsCheckPreRelease = item.CoreType == ECoreType.v2rayN;
        }
    }

    public void SelectLastRow()
    {
        if (lstCheckUpdates.ItemCount > 0)
        {
            lstCheckUpdates.SelectedIndex = lstCheckUpdates.ItemCount - 1;
        }
    }
}
