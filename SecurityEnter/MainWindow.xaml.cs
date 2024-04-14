using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace SecurityEnter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private int failedLoginAttempts = 0;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool loginSuccessful = Login(LoginTextBox.Text, PasswordBox.Password);

            if (!loginSuccessful)
            {
                failedLoginAttempts++;

                if (failedLoginAttempts >= 3)
                {
                    
                    EnterBTN.IsEnabled = false;
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    EnterBTN.IsEnabled = true;

                    
                    failedLoginAttempts = 0;
                }
            }
            else
            {
                
                failedLoginAttempts = 0;
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Reg().Show();
            Close();
        }

        public static bool Login(string login, string password)
        {
            try
            {
                using (var context = new DBEntities())
                {
                    
                    var user = context.User.FirstOrDefault(u => u.Login == login);

                    if (user != null)
                    {
                        
                        byte[] hashBytes = user.PasswordHash;
                        byte[] salt = new byte[16];
                        Array.Copy(hashBytes, 0, salt, 0, 16);

                        
                        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
                        byte[] hash = pbkdf2.GetBytes(20);

                        
                        for (int i = 0; i < 20; i++)
                        {
                            if (hashBytes[i + 16] != hash[i])
                            {
                                MessageBox.Show("Неверный пароль");
                                return false;
                            }
                        }

                        
                        Osnova osnovaForm = new Osnova();
                        osnovaForm.Show();

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

    }
}
