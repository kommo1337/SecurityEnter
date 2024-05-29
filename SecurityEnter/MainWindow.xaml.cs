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
                        // Retrieve the salt, encrypted hash, key, and IV from the database
                        byte[] salt = user.Salt;
                        byte[] storedEncryptedHash = user.PasswordHash;
                        byte[] storedKey = user.KeyHash;
                        byte[] storedIV = user.IV;

                        // Derive key and IV from the password using PBKDF2
                        using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000))
                        {
                            byte[] aesKey = rfc2898.GetBytes(32); // 256-bit key
                            byte[] aesIV = rfc2898.GetBytes(16); // 128-bit IV

                            if (!ConstantTimeComparison(aesKey, storedKey) || !ConstantTimeComparison(aesIV, storedIV))
                            {
                                MessageBox.Show("Неверный пароль");
                                return false;
                            }

                            // Hash the password
                            SHA256 sha256 = SHA256.Create();
                            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                            byte[] computedHash = sha256.ComputeHash(passwordBytes);

                            // Decrypt the stored password hash
                            using (AesManaged aes = new AesManaged())
                            {
                                aes.KeySize = 256;
                                aes.BlockSize = 128;
                                aes.Key = aesKey;
                                aes.IV = aesIV;

                                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                                byte[] decryptedHash = decryptor.TransformFinalBlock(storedEncryptedHash, 0, storedEncryptedHash.Length);

                                // Compare hashes using constant-time comparison
                                if (!ConstantTimeComparison(decryptedHash, computedHash))
                                {
                                    MessageBox.Show("Неверный пароль");
                                    return false;
                                }
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
