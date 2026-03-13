namespace JWTUAuthLogin.Shared.Utilities
{
    public class SecureConfigurationHelper
    {
            private readonly IConfiguration _configuration;
            private readonly string _encryptionKey;

            public SecureConfigurationHelper(IConfiguration configuration)
            {
                _configuration = configuration;

                // Get encryption key from environment variable (set on EC2 instance)
                // For development, you can set it in your user environment variables
                // Try Process first (works best in IIS), then fallback to User and Machine
                _encryptionKey = Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.Process)
                              ?? Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY")
                              ?? Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.User)
                              ?? Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.Machine);

                // Enhanced logging for debugging
                Console.WriteLine("=== SecureConfigurationHelper Initialization (Mobile API) ===");
                var processVar = Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.Process);
                var userVar = Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.User);
                var machineVar = Environment.GetEnvironmentVariable("SMARTHR_ENCRYPTION_KEY", EnvironmentVariableTarget.Machine);

                Console.WriteLine($"Process Env Var: {(processVar != null ? processVar.Substring(0, Math.Min(10, processVar.Length)) + "..." : "NOT SET")}");
                Console.WriteLine($"User Env Var: {(userVar != null ? userVar.Substring(0, Math.Min(10, userVar.Length)) + "..." : "NOT SET")}");
                Console.WriteLine($"Machine Env Var: {(machineVar != null ? machineVar.Substring(0, Math.Min(10, machineVar.Length)) + "..." : "NOT SET")}");
                Console.WriteLine($"Encryption Key Available: {!string.IsNullOrEmpty(_encryptionKey)}");
                Console.WriteLine("=============================================================");

                if (string.IsNullOrEmpty(_encryptionKey))
                {
                    Console.WriteLine("WARNING: SMARTHR_ENCRYPTION_KEY environment variable not set!");
                    Console.WriteLine("Encrypted values will NOT be decrypted.");
                    Console.WriteLine("JWT authentication will FAIL!");
                    Console.WriteLine("Set the environment variable and RESTART the application pool/service.");
                    Console.WriteLine("For IIS: Set in web.config <environmentVariables> or Application Pool environment");
                    Console.WriteLine("For Windows Service: Set as Machine environment variable and restart service");
                }
                else
                {
                    Console.WriteLine($"Encryption key loaded successfully (length: {_encryptionKey.Length} characters)");
                }
            }

            /// <summary>
            /// Gets a configuration value and automatically decrypts it if encrypted
            /// </summary>
            public string GetValue(string key)
            {
                string value = _configuration[key];

                if (string.IsNullOrEmpty(value))
                    return value;

                // If the value is encrypted and we have a key, decrypt it
                if (ConfigurationEncryption.IsEncrypted(value) && !string.IsNullOrEmpty(_encryptionKey))
                {
                    try
                    {
                        return ConfigurationEncryption.Decrypt(value, _encryptionKey);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to decrypt configuration value for key '{key}'. " +
                            $"Ensure SMARTHR_ENCRYPTION_KEY environment variable is set correctly. " +
                            $"Error: {ex.Message}",
                            ex);
                    }
                }

                return value;
            }

            /// <summary>
            /// Gets a connection string and automatically decrypts it if encrypted
            /// </summary>
            public string GetConnectionString(string name)
            {
                string connectionString = _configuration.GetConnectionString(name);

                if (string.IsNullOrEmpty(connectionString))
                    return connectionString;

                // If encrypted, decrypt it
                if (ConfigurationEncryption.IsEncrypted(connectionString) && !string.IsNullOrEmpty(_encryptionKey))
                {
                    try
                    {
                        return ConfigurationEncryption.Decrypt(connectionString, _encryptionKey);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to decrypt connection string '{name}'. " +
                            $"Ensure SMARTHR_ENCRYPTION_KEY environment variable is set correctly. " +
                            $"Error: {ex.Message}",
                            ex);
                    }
                }

                return connectionString;
            }

            /// <summary>
            /// Gets a section value and automatically decrypts it if encrypted
            /// </summary>
            public string GetSectionValue(string section, string key)
            {
                return GetValue($"{section}:{key}");
            }

            /// <summary>
            /// Checks if encryption key is available
            /// </summary>
            public bool IsEncryptionKeyAvailable()
            {
                return !string.IsNullOrEmpty(_encryptionKey);
            }

            /// <summary>
            /// Gets the encryption key (use with caution, mainly for setup/testing)
            /// </summary>
            public string GetEncryptionKey()
            {
                return _encryptionKey;
            }
        }
    }
