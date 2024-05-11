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
                        // Получаем соль и хеш пароля из базы данных
                        byte[] salt = user.Salt;
                        byte[] storedHash = user.PasswordHash;

                        string saltedPassword = "0x" + BitConverter.ToString(salt).Replace("-", "") + password;

                        // Вычисляем хеш пароля
                        SHA512 sha512 = SHA512.Create();
                        byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);
                        byte[] computedHash = sha512.ComputeHash(bytes);

                        // Применяем Key Stretching
                        //int stretchFactor = 5000;
                        //for (int i = 0; i < stretchFactor; i++)
                        //{
                        //    computedHash = sha512.ComputeHash(computedHash);
                        //}

                        // Сравниваем хеши с использованием функции ConstantTimeComparison
                        if (!ConstantTimeComparison(storedHash, computedHash))
                        {
                            MessageBox.Show("Неверный пароль");
                            return false;
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

        public static bool ConstantTimeComparison(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }


    }
}
