using System.Windows;
using System.Windows.Controls;

using MeetMindAI.WPF.ViewModels.Authentication;

namespace MeetMindAI.WPF.Views.Authentication;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel &&
            sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void TogglePasswordButton_OnClick(
    object sender,
    RoutedEventArgs e)
    {
        if (PasswordBox.Visibility == Visibility.Visible)
        {
            VisiblePasswordBox.Text = PasswordBox.Password;

            PasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;

            TogglePasswordButton.Content = "Hide";

            VisiblePasswordBox.Focus();
            VisiblePasswordBox.CaretIndex =
                VisiblePasswordBox.Text.Length;
        }
        else
        {
            PasswordBox.Password = VisiblePasswordBox.Text;

            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;

            TogglePasswordButton.Content = "Show";

            PasswordBox.Focus();
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
