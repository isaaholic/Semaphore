using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Source;
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public int _numValue = 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<Thread> Created { get; set; }
    public ObservableCollection<Thread> Waiting { get; set; }
    public ObservableCollection<Thread> Working { get; set; }

    private Semaphore semaphore;

    public int NumValue
    {
        get { return _numValue; }
        set
        {
            _numValue = value;
            PropertyChanged?.Invoke(
               this, new PropertyChangedEventArgs(nameof(NumValue)));
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        semaphore = new(NumValue, 3, "Semaphore");

        Created = new();
        Waiting = new();
        Working = new();
    }
    private void btnDec_Click(object sender, RoutedEventArgs e)
    {
        if (NumValue > 0)
            NumValue--;
    }

    private void btnInc_Click(object sender, RoutedEventArgs e)
    {
        if (NumValue < 10)
            NumValue++;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var thread = new Thread(ThreadWork);
        thread.Name = $"Thread {thread.ManagedThreadId}";
        Created.Add(thread);
    }

    private void ThreadWork(object? semaphore)
    {
        if (semaphore is Semaphore sem)
        {
            if (sem.WaitOne())
            {
                try
                {
                    var t = Thread.CurrentThread;
                    Dispatcher.Invoke(() => t.Name = $"Thread {t.ManagedThreadId}");
                    if (Working.Count >= NumValue) return;
                    Dispatcher.Invoke(() => Waiting.Remove(t));
                    Dispatcher.Invoke(() => Working.Add(t));
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(() => Working.Remove(t));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    sem.Release();
                }

            }
        }
    }

    private void CreatedLBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CreatedLBox.SelectedItem is Thread t)
        {
            Created.Remove(t);
            btndec.IsEnabled = false;
            btninc.IsEnabled = false;
            Waiting.Add(t);
            t.Start(semaphore);
        }
    }

}
