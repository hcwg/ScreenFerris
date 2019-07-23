using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfDemo.Models;

namespace WpfDemo.UserControls
{
    /// <summary>
    /// Interaction logic for ShortcutEdit.xaml
    /// </summary>
    public partial class ShortcutEdit : UserControl
    {
        ShortcutBinding binding;
        HashSet<Key> keys;
        public ShortcutEdit()
        {
            InitializeComponent();
            binding = DataContext as ShortcutBinding;
            keys = new HashSet<Key>();
        }
       
   
       
        private void TxtShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            keys.Add(e.Key);
            txtShortcut.Text = TransformSetToText(keys);
        }

        private void TxtShortcut_KeyUp(object sender, KeyEventArgs e)
        {
            //keys.Remove(e.Key);
            //txtShortcut.Text = TransformSetToText(keys);
        }

        static public string TransformSetToText(HashSet<Key> keys)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var key in keys)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" + ");
                }
                string keyString = key.ToString();
                if (keyString.StartsWith("Left"))
                {
                    keyString = keyString.Replace("Left", "");
                } else if (keyString.StartsWith("Right"))
                {
                    keyString = keyString.Replace("Left", "");
                }
                stringBuilder.Append(keyString);
            }
            return stringBuilder.ToString();
        }

        private void TxtBlockShortcut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtBlockShortcut.Visibility = Visibility.Collapsed;
            txtShortcut.Text = txtBlockShortcut.Content as string ;
            keys.Clear();
            txtShortcut.Visibility = Visibility.Visible;
            txtShortcut.Focus();
            
        }
        private void TxtShortcut_LostFocus(object sender, RoutedEventArgs e)
        {
            txtShortcut.Visibility = Visibility.Collapsed;
            txtBlockShortcut.Visibility = Visibility.Visible;
            txtBlockShortcut.Content = txtShortcut.Text;
        }

    }
}
