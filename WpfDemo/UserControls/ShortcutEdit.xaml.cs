namespace WpfDemo.UserControls
{
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using WpfDemo.Models;

    /// <summary>
    /// Interaction logic for ShortcutEdit.xaml.
    /// </summary>
    public partial class ShortcutEdit : UserControl
    {
        private readonly HashSet<Key> keys; 
        private ShortcutBinding binding;

        public ShortcutEdit()
        {
            this.InitializeComponent();
            this.binding = this.DataContext as ShortcutBinding;
            this.keys = new HashSet<Key>();
        }

        public static string TransformSetToText(HashSet<Key> keys)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var key in keys)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" + ");
                }

                string keyString = key.ToString();
                if (keyString.StartsWith("Left"))
                {
                    keyString = keyString.Replace("Left", string.Empty);
                }
                else if (keyString.StartsWith("Right"))
                {
                    keyString = keyString.Replace("Left", string.Empty);
                }

                stringBuilder.Append(keyString);
            }

            return stringBuilder.ToString();
        }



        private void TxtShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            this.keys.Add(e.Key);
            this.txtShortcut.Text = TransformSetToText(this.keys);
        }

        private void TxtShortcut_KeyUp(object sender, KeyEventArgs e)
        {
            // keys.Remove(e.Key);
            // txtShortcut.Text = TransformSetToText(keys);
        }

        private void TxtBlockShortcut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.txtBlockShortcut.Visibility = Visibility.Collapsed;
            this.txtShortcut.Text = this.txtBlockShortcut.Content as string;
            this.keys.Clear();
            this.txtShortcut.Visibility = Visibility.Visible;
            this.txtShortcut.Focus();

        }

        private void TxtShortcut_LostFocus(object sender, RoutedEventArgs e)
        {
            this.txtShortcut.Visibility = Visibility.Collapsed;
            this.txtBlockShortcut.Visibility = Visibility.Visible;
            this.txtBlockShortcut.Content = this.txtShortcut.Text;
        }

    }
}
