using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Principal;

namespace SirenSettingInstaller
{
    public partial class MainWindow : Form
    {
        // Create filesToDelete and foldersToDelete array for cleanup
        string[] filesToDelete = new string[] { };
        string[] foldersToDelete = new string[] { };

        // Initialize required components
        private Label statusLabel;
        private ProgressBar mainProgressBar;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Checks if the current process is running with administrator privileges using the <c>WindowsIdentity</c> class.
        /// </summary>
        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Elevates the current process to administrator privileges. If the process is already running with administrator privileges, this method does nothing.
        /// </summary>
        private void RunWithAdminPrivileges()
        {
            // Create a new process info structure
            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            var processInfo = new ProcessStartInfo(currentExecutablePath)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            // Try to start the process
            try
            {
                Process.Start(processInfo);
                Application.Exit(); // Success, exit the current process
                return;
            }
            catch (Exception e)
            {
                string message = "Failed to start with administrator privileges. Please try again. Error: " + e.Message;
                Console.WriteLine(message);
                MessageBox.Show(message, "SirenSettingLimitAdjuster Installer: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        /// <summary>
        /// Shows a message box with the message and the "SirenSettingLimitAdjuster Installer: Error" title.
        /// </summary>
        /// <param name="message"></param> The message of the message box.
        private static void ShowApplicationError(string message)
        {
            MessageBox.Show(message, "SirenSettingLimitAdjuster Installer: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Downloads a file to a random folder in the windows temp directory. Returns the path to the downloaded file if successful, otherwise returns null.
        /// </summary>
        /// <param name="url"></param> The URL to download the file from.
        /// <param name="progress"></param> The progress reporter. See <see cref="IProgress{T}"/> for more information.
        /// <param name="progressBar"></param> The progress bar to update. Typeof <see cref="ProgressBar"/>.
        /// <param name="statusLabel"></param> The status label to update. Typeof <see cref="Label"/>.
        /// <returns></returns>
        private async Task<string> DownloadResourceToTempPath(string url, IProgress<(long bytesReceived, long? totalBytes)> progress, ProgressBar progressBar, Label statusLabel)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var tempFileName = Path.GetTempFileName();
                        var totalBytes = response.Content.Headers.ContentLength;

                        using (var fileStream = File.Create(tempFileName))
                        using (var httpStream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[8192];
                            var bytesRead = 0;
                            long totalBytesRead = 0;

                            while ((bytesRead = await httpStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                progress.Report((totalBytesRead, totalBytes));
                            }

                            return tempFileName; // Return the file path
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request exception: {ex.Message}");
                ShowApplicationError($"An error occurred while downloading the file. Please try again later. (DOWNLOAD_FAILED_HTTP)");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO exception: {ex.Message}");
                ShowApplicationError($"An error occurred while downloading the file. Please try again later. (DOWNLOAD_FAILED_IO)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                ShowApplicationError($"An error occurred while downloading the file. Please try again later. (DOWNLOAD_FAILED_UNKNOWN)");
            }

            return null; // Download failed
        }

        private async void MainWindow_Shown(object sender, EventArgs e)
        {
            // Set the variables
            statusLabel = stateLabel;
            mainProgressBar = mainPG;

            // Set the progress bar to the "indeterminate" state
            mainProgressBar.Style = ProgressBarStyle.Marquee;

            // Check for administrator privileges
            if (!IsAdministrator())
            {
                var confirmationResult = MessageBox.Show(
                    "This application requires administrative privileges to run. Do you want to continue?",
                    "SirenSettingLimitAdjuster Installer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmationResult == DialogResult.Yes)
                {
                    RunWithAdminPrivileges();
                }
                else
                {
                    Console.WriteLine("User cancelled.");
                    MessageBox.Show("Sorry, this application must be run as administrator.", "SirenSettingLimitAdjuster Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                return;
            }

            mainProgressBar.Style = ProgressBarStyle.Continuous;
            statusLabel.Text = "Starting download...";

            string downloadUrl = "https://github.com/SticksDev/SirenSettingInstaller/raw/main/data/SirenSetting_Limit_Adjuster_v2.zip";

            // Use async/await to keep the UI responsive
            var downloadTask = DownloadResourceToTempPath(downloadUrl,
                new Progress<(long bytesReceived, long? totalBytes)>(
                    (progress) =>
                    {
                        if (progress.totalBytes.HasValue)
                        {
                            mainPG.Maximum = (int)progress.totalBytes.Value;
                            mainPG.Value = (int)progress.bytesReceived;

                            stateLabel.Text =
                                $"Downloading... {progress.bytesReceived / 1024} KB / {progress.totalBytes.Value / 1024} KB";

                            // Update progress asynchronously on the UI thread
                            stateLabel.BeginInvoke((Action)(() => stateLabel.Text = stateLabel.Text));
                            mainPG.BeginInvoke((Action)(() => mainPG.Value = mainPG.Value));
                        }
                    }), mainPG, stateLabel);

            // Wait until the download is complete asynchronously
            await downloadTask;
            downloadTask.Dispose();

            if (downloadTask.Result == null)
            {
                Application.Exit();
                return;
            }

            // Set state to "Verifying..."
            statusLabel.Text = "Verifying...";
            mainProgressBar.Style = ProgressBarStyle.Marquee;

            // Verify the downloaded file
            string downloadedFilePath = downloadTask.Result;
            string fileHash = "74fec47012be517504fb515abc913ece559f0ad680d0f16058ae6fb2ea46a9aa"; // SHA256 hash of the file
            string downloadedFileHash = "";
            bool isFileVerified = false;

            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    using (FileStream stream = File.OpenRead(downloadedFilePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        downloadedFileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        isFileVerified = downloadedFileHash == fileHash;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while verifying the file: {ex.Message}");
                ShowApplicationError($"An error occurred while verifying the file. Please try again later. (VERIFY_FAILED)");

                Application.Exit();
                return;
            }

            if (!isFileVerified)
            {
                Console.WriteLine($"File verification failed. Expected hash: {fileHash}, actual hash: {downloadedFileHash}");
                ShowApplicationError($"File verification failed. Please try again later. (VERIFY_FAILED_HASH)");

                Application.Exit();
                return;
            }

            // Set state to "Installing..."
            statusLabel.Text = "Installing...";

            // Extract the downloaded file
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                ZipFile.ExtractToDirectory(downloadedFilePath, tempDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while extracting the file: {ex.Message}");
                ShowApplicationError($"An error occurred while extracting the file. Please try again later. (EXTRACT_FAILED)");

                Application.Exit();
                return;
            }

            // Add newly created files and folders to the cleanup list
            foldersToDelete = foldersToDelete.Append(tempDirectory).ToArray();

            // Attempt to find the FiveM installation directory
            // The default installation directory is C:\Users\<username>\AppData\Local\FiveM\FiveM.app
            string fivemDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app");

            if (!Directory.Exists(fivemDirectory))
            {
                Console.WriteLine($"FiveM installation directory not found. Expected path: {fivemDirectory}");

                // Show a information message box and ask the user to select the FiveM installation directory
                MessageBox.Show("FiveM installation directory not found. Please select the FiveM installation directory.", "SirenSettingLimitAdjuster Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Show the folder browser dialog
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Please select the FiveM installation directory.";
                folderBrowserDialog.ShowNewFolderButton = false;
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.LocalApplicationData;

                var dialogResult = folderBrowserDialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    fivemDirectory = folderBrowserDialog.SelectedPath;
                }
                else
                {
                    Console.WriteLine("User cancelled.");
                    ShowApplicationError("You must select a directory.");
                    Application.Exit();
                    return;
                }

                Application.Exit();
                return;
            }

            // Ensure they selected the correct directory. There should be a folder called "plugins" in the FiveM installation directory.
            if (!Directory.Exists(Path.Combine(fivemDirectory, "plugins")))
            {
                Console.WriteLine($"FiveM installation directory not found. Expected path: {fivemDirectory}");
                ShowApplicationError("The selected directory is not a valid FiveM installation directory. (MISSING_PLUGINS_FOLDER)");
                Application.Exit();
                return;
            }

            // In the zip file, there is a folder called "SirenSetting_Limit_Adjuster_v2" and in that folder there is a file called "SirenSetting_Limit_Adjuster.asi"
            // Copy that file to the FiveM installation directory
            string asiFilePath = Path.Combine(tempDirectory, "SirenSetting_Limit_Adjuster_v2", "SirenSetting_Limit_Adjuster.asi");
            string destinationFilePath = Path.Combine(fivemDirectory, "plugins", "SirenSetting_Limit_Adjuster.asi");

            try
            {
                // If the file already exists, delete it
                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                }

                // Copy the file
                File.Copy(asiFilePath, destinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while copying the file: {ex.Message}");
                ShowApplicationError($"An error occurred while copying the file. Please try again later. (COPY_FAILED)");

                Application.Exit();
                return;
            }

            // Set state to "Cleaning up..."
            statusLabel.Text = "Cleaning up...";
            mainProgressBar.Style = ProgressBarStyle.Marquee;

            // Delete the temporary files and folders
            bool isCleanupSuccessful = true;

            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the file: {ex.Message}");
                    isCleanupSuccessful = false;
                }
            }

            foreach (string folder in foldersToDelete)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the folder: {ex.Message}");
                    isCleanupSuccessful = false;
                }
            }

            if (!isCleanupSuccessful)
            {
                // Show a information message box saying that the installtion is complete but cleanup failed
                // After displaying the message box, open the directory containing the temp files
                MessageBox.Show("Installation is complete, but cleanup failed. Please delete the temporary files manually.", "SirenSettingLimitAdjuster Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start("explorer.exe", Path.GetTempPath());
                Application.Exit();
            }
            else
            {
                // Show a information message box saying that the installtion is complete
                MessageBox.Show("Installation is complete. Have fun!", "SirenSettingLimitAdjuster Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}