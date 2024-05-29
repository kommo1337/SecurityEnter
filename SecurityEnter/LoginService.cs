using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityEnter
{
    public class LoginService
    {
        private static readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> loginAttempts = new Dictionary<string, (int, DateTime)>();

        public bool Login(string username, string password)
        {
            if (IsAccountLocked(username))
            {
                // Account is locked, return false
                return false;
            }

            bool loginSuccessful = ValidateCredentials(username, password);
            if (!loginSuccessful)
            {
                RecordFailedAttempt(username);
                return false;
            }

            ResetFailedAttempts(username);
            return true;
        }

        private void RecordFailedAttempt(string username)
        {
            if (!loginAttempts.ContainsKey(username))
            {
                loginAttempts[username] = (0, DateTime.Now);
            }

            loginAttempts[username] = (loginAttempts[username].Attempts + 1, DateTime.Now);

            if (loginAttempts[username].Attempts >= 3)
            {
                // Lock account for 30 seconds
                loginAttempts[username] = (loginAttempts[username].Attempts, DateTime.Now.AddSeconds(30));
            }
        }

        private bool IsAccountLocked(string username)
        {
            if (!loginAttempts.ContainsKey(username))
            {
                return false;
            }

            var attemptsInfo = loginAttempts[username];
            return attemptsInfo.Attempts >= 3 && attemptsInfo.LastAttempt > DateTime.Now;
        }

        private void ResetFailedAttempts(string username)
        {
            if (loginAttempts.ContainsKey(username))
            {
                loginAttempts.Remove(username);
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            // Your actual credential validation logic here
            return false; // Assume failure for demonstration
        }
    }

}
