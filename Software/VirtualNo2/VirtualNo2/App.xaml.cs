/* App.xaml.cs - Virtual No2 (C) motion phantom application.
 * Copyright (C) 2019 by Stefan Grimm
 *
 * This is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the VirtualNo2 software.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZeroMQ;

namespace VirtualNo2 {

  public partial class App : Application {

    public readonly string COMMAND_QUEUE_NAME_SENDER = "inproc://commands";   // e.g. "tcp://127.0.0.1:5558";
    public readonly string COMMAND_QUEUE_NAME_RECEIVER = "inproc://commands"; // e.g. "tcp://*:5558";
    private readonly string VIEWMODEL_NOTIFY_NAME = "inproc://notifyviewmodel";

    private CancellationTokenSource _cancellationTokenSource;
    private Model.EventMediator _eventMediator;
    private UI.ViewModel _viewModel;
    private ZContext _mqContext;

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      _cancellationTokenSource = new CancellationTokenSource();
      _mqContext = new ZContext();

      _eventMediator = new Model.EventMediator();
      _viewModel = new UI.ViewModel();

      using (var vmWH = new AutoResetEvent(false))
      using (var emWH = new AutoResetEvent(false)) {
        Task tVm = _viewModel.StartBindIncomingAsync(_cancellationTokenSource.Token, _mqContext, VIEWMODEL_NOTIFY_NAME, vmWH);
        Task tEv = _eventMediator.StartBindIncomingAsync(_cancellationTokenSource.Token, _mqContext, COMMAND_QUEUE_NAME_RECEIVER, emWH);
        vmWH.WaitOne();
        emWH.WaitOne();
      }
      _viewModel.ConnectOutgoing(_mqContext, COMMAND_QUEUE_NAME_SENDER);
      _eventMediator.ConnectViewModel(_mqContext, VIEWMODEL_NOTIFY_NAME);
      
      var app = new UI.MainWindow();
      app.DataContext = _viewModel;
      app.Closing += _viewModel.OnClosing;
      app.Show();

    }
    protected override void OnExit(ExitEventArgs e) {
      base.OnExit(e);

      _cancellationTokenSource.Cancel();

      _eventMediator.Dispose();
      _viewModel.Dispose();

      _mqContext.Dispose();
    }
  }
}
