using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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
using System.Windows.Shapes;

namespace SecurityEnter
{
    /// <summary>
    /// Логика взаимодействия для Reg.xaml
    /// </summary>
    public partial class Reg : Window
    {
        public Reg()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RegisterNewUser(LoginTextBox.Text, PasswordBox.Password);
        }

        public static void RegisterNewUser(string login, string password, string i = null)
        {
            try
            {
                using (var context = new DBEntities())
                {

                    byte[] salt = new byte[16];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(salt);
                    }
                     
                    
                    string saltedPassword = "0x" + BitConverter.ToString(salt).Replace("-", "") + password;

                   
                    SHA512 sha512 = SHA512.Create();
                    byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);
                    byte[] hash = sha512.ComputeHash(bytes);


                    var user = new User
                    {
                        Login = login,
                        PasswordHash = hash,
                        Salt = salt,
                        Info = i
                    };

                    context.User.Add(user);
                    context.SaveChanges();

                    MessageBox.Show("Успешно");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
