using System;
using System.Windows;

namespace HoverText
{
  /// <summary>
  /// First-launch screen that walks the user through how Hover Text works.
  /// Shown once; dismissed with "Got it" (or by closing).
  /// </summary>
  public partial class OnboardingWindow : Window
  {
    public event Action? Completed;

    public OnboardingWindow(Settings settings)
    {
      InitializeComponent();
      TriggerKeyRun.Text = settings.TriggerKeyDisplayName;
    }

    private void GotIt_Click(object sender, RoutedEventArgs e)
    {
      Completed?.Invoke();
      Close();
    }
  }
}
