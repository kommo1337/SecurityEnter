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
                    byte[] salt = new byte[64];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(salt);
                    }

                    // Derive key and IV from the password using PBKDF2
                    using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000))
                    {
                        byte[] aesKey = rfc2898.GetBytes(32); // 256-bit key
                        byte[] aesIV = rfc2898.GetBytes(16); // 128-bit IV

                        // Hash the password
                        SHA256 sha256 = SHA256.Create();
                        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                        byte[] passwordHash = sha256.ComputeHash(passwordBytes);

                        // Encrypt the password hash
                        using (AesManaged aes = new AesManaged())
                        {
                            aes.KeySize = 256;
                            aes.BlockSize = 128;
                            aes.Key = aesKey;
                            aes.IV = aesIV;

                            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                            byte[] encryptedPasswordHash = encryptor.TransformFinalBlock(passwordHash, 0, passwordHash.Length);

                            var user = new User
                            {
                                Login = login,
                                PasswordHash = encryptedPasswordHash, // Store encrypted hash
                                Salt = salt,
                                KeyHash = aesKey, // Store the AES key
                                IV = aesIV,       // Store the AES IV
                                Info = i
                            };

                            context.User.Add(user);
                            context.SaveChanges();

                            MessageBox.Show("Успешно");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



    }
}
