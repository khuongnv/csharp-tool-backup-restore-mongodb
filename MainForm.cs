using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDbTools.Properties;

namespace MongoDbTools
{
    public partial class MainForm : Form
    {

        private TabControl tabControl;
        private TabPage tabBackup, tabRestore;
        
        // Backup controls
        private TextBox txtBackupConnectionString, txtBackupDatabaseName, txtBackupFolder;
        private Button btnSelectBackupFolder, btnBackup, btnTestBackupConnection;
        private ProgressBar progressBarBackup;
        private TextBox txtLogBackup;
        private Label lblStatusBackup;
        private ComboBox cmbBackupAuthMechanism;
        
        // Restore controls  
        private TextBox txtRestoreConnectionString, txtRestoreDatabaseName, txtRestoreFolder;
        private Button btnSelectRestoreFolder, btnRestore, btnTestRestoreConnection;
        private ProgressBar progressBarRestore;
        private TextBox txtLogRestore;
        private Label lblStatusRestore;
        private CheckBox chkDropExisting;
        private ComboBox cmbRestoreAuthMechanism;

        public MainForm()
        {
            InitializeComponent();
            InitializeDefaultValues();
        }

        private void InitializeComponent()
        {
            this.Text = "MongoDB Tools";
            this.Size = new System.Drawing.Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Icon = SystemIcons.Application;

            // Tab Control with classic styling
            tabControl = new TabControl
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(980, 710),
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                DrawMode = TabDrawMode.Normal,
                ItemSize = new System.Drawing.Size(200, 21),
                SizeMode = TabSizeMode.Fixed
            };

            // Create tabs
            CreateBackupTab();
            CreateRestoreTab();
            
            this.Controls.Add(tabControl);
        }


        private void CreateBackupTab()
        {
            tabBackup = new TabPage("Backup Database");
            tabBackup.BackColor = System.Drawing.SystemColors.Control;
            
            // Header Panel
            var headerPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(950, 28),
                BackColor = System.Drawing.SystemColors.ActiveCaption,
                BorderStyle = BorderStyle.Fixed3D
            };
            
            var headerLabel = new Label
            {
                Text = "Backup MongoDB Database",
                Location = new System.Drawing.Point(12, 6),
                Size = new System.Drawing.Size(400, 16),
                ForeColor = System.Drawing.SystemColors.ActiveCaptionText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.Transparent
            };
            headerPanel.Controls.Add(headerLabel);
            
            // Connection String
            var lbl1 = new Label 
            { 
                Text = "Connection String:", 
                Location = new System.Drawing.Point(15, 55), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtBackupConnectionString = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 75), 
                Size = new System.Drawing.Size(720, 20), 
                Multiline = true, 
                Height = 40,
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Courier New", 8.25F)
            };
            
            // Add tooltip for connection string format
            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtBackupConnectionString, 
                "MongoDB Connection String Examples:\n" +
                "• Local: mongodb://localhost:27017\n" +
                "• With Auth: mongodb://username:password@host:port/database\n" +
                "• Atlas: mongodb+srv://username:password@cluster.mongodb.net/database\n" +
                "• With Options: mongodb://host:port/database?retryWrites=true&w=majority\n" +
                "• With Auth Mechanism: mongodb://user:pass@host:port/db?authMechanism=SCRAM-SHA-256\n\n" +
                "💡 If you get SCRAM-SHA-1 errors, try SCRAM-SHA-256 or MONGODB-CR");
            
            // Database Name
            var lbl2 = new Label 
            { 
                Text = "Database Name:", 
                Location = new System.Drawing.Point(15, 130), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtBackupDatabaseName = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 150), 
                Size = new System.Drawing.Size(200, 22),
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            
            // Add tooltip for database name
            toolTip.SetToolTip(txtBackupDatabaseName, 
                "Database Name Examples:\n" +
                "• myapp_db\n" +
                "• production_data\n" +
                "• user_management\n" +
                "• jobpxa");
            
            // Authentication Mechanism
            var lblAuth = new Label 
            { 
                Text = "Auth Mechanism:", 
                Location = new System.Drawing.Point(230, 130), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            cmbBackupAuthMechanism = new ComboBox 
            { 
                Location = new System.Drawing.Point(230, 150), 
                Size = new System.Drawing.Size(150, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            cmbBackupAuthMechanism.Items.AddRange(new string[] { "Auto", "SCRAM-SHA-1", "SCRAM-SHA-256", "MONGODB-CR", "PLAIN", "GSSAPI", "MONGODB-X509" });
            cmbBackupAuthMechanism.SelectedIndex = 0; // Auto
            
            // Add tooltip for auth mechanism
            toolTip.SetToolTip(cmbBackupAuthMechanism, 
                "Authentication Mechanism:\n" +
                "• Auto: Let MongoDB driver choose\n" +
                "• SCRAM-SHA-1: Default for MongoDB 3.0+\n" +
                "• SCRAM-SHA-256: More secure, MongoDB 4.0+\n" +
                "• MONGODB-CR: Legacy authentication\n" +
                "• PLAIN: LDAP authentication\n" +
                "• GSSAPI: Kerberos authentication\n" +
                "• MONGODB-X509: Certificate authentication");
            
            // Backup Folder
            var lbl3 = new Label 
            { 
                Text = "Backup Folder:", 
                Location = new System.Drawing.Point(15, 185), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtBackupFolder = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 205), 
                Size = new System.Drawing.Size(620, 22), 
                ReadOnly = true,
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            btnSelectBackupFolder = new Button 
            { 
                Text = "Browse...", 
                Location = new System.Drawing.Point(645, 203), 
                Size = new System.Drawing.Size(70, 26),
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnSelectBackupFolder.Click += (s, e) => {
                using (var dlg = new FolderBrowserDialog())
                    if (dlg.ShowDialog() == DialogResult.OK) txtBackupFolder.Text = dlg.SelectedPath;
            };
            
            // Test Connection Button
            btnTestBackupConnection = new Button 
            { 
                Text = "Test Connection", 
                Location = new System.Drawing.Point(15, 245), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnTestBackupConnection.Click += BtnTestBackupConnection_Click;
            
            // Backup Button
            btnBackup = new Button 
            { 
                Text = "Start Backup", 
                Location = new System.Drawing.Point(145, 245), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnBackup.Click += BtnBackup_Click;
            
            // Progress & Status
            progressBarBackup = new ProgressBar 
            { 
                Location = new System.Drawing.Point(15, 290), 
                Size = new System.Drawing.Size(720, 18),
                Style = ProgressBarStyle.Continuous
            };
            lblStatusBackup = new Label 
            { 
                Text = "Ready to backup", 
                Location = new System.Drawing.Point(15, 315), 
                Size = new System.Drawing.Size(720, 18),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            
            // Log
            var lbl4 = new Label 
            { 
                Text = "Activity Log:", 
                Location = new System.Drawing.Point(15, 345), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtLogBackup = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 365), 
                Size = new System.Drawing.Size(720, 250), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical, 
                ReadOnly = true, 
                Font = new System.Drawing.Font("Courier New", 8.25F),
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                BorderStyle = BorderStyle.Fixed3D
            };
            
            // Add to tab
            tabBackup.Controls.AddRange(new Control[] { headerPanel, lbl1, txtBackupConnectionString, lbl2, txtBackupDatabaseName, lblAuth, cmbBackupAuthMechanism, lbl3, txtBackupFolder, btnSelectBackupFolder, btnTestBackupConnection, btnBackup, progressBarBackup, lblStatusBackup, lbl4, txtLogBackup });
            tabControl.TabPages.Add(tabBackup);
        }

        private void CreateRestoreTab()
        {
            tabRestore = new TabPage("Restore Database");
            tabRestore.BackColor = System.Drawing.SystemColors.Control;
            
            // Header Panel
            var headerPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(950, 28),
                BackColor = System.Drawing.SystemColors.ActiveCaption,
                BorderStyle = BorderStyle.Fixed3D
            };
            
            var headerLabel = new Label
            {
                Text = "Restore MongoDB Database",
                Location = new System.Drawing.Point(12, 6),
                Size = new System.Drawing.Size(400, 16),
                ForeColor = System.Drawing.SystemColors.ActiveCaptionText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.Transparent
            };
            headerPanel.Controls.Add(headerLabel);
            
            // Connection String
            var lbl1 = new Label 
            { 
                Text = "Connection String:", 
                Location = new System.Drawing.Point(15, 55), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtRestoreConnectionString = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 75), 
                Size = new System.Drawing.Size(720, 20), 
                Multiline = true, 
                Height = 40,
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Courier New", 8.25F)
            };
            
            // Add tooltip for connection string format
            var toolTipRestore = new ToolTip();
            toolTipRestore.SetToolTip(txtRestoreConnectionString, 
                "MongoDB Connection String Examples:\n" +
                "• Local: mongodb://localhost:27017\n" +
                "• With Auth: mongodb://username:password@host:port/database\n" +
                "• Atlas: mongodb+srv://username:password@cluster.mongodb.net/database\n" +
                "• With Options: mongodb://host:port/database?retryWrites=true&w=majority\n" +
                "• With Auth Mechanism: mongodb://user:pass@host:port/db?authMechanism=SCRAM-SHA-256\n\n" +
                "💡 If you get SCRAM-SHA-1 errors, try SCRAM-SHA-256 or MONGODB-CR");
            
            // Database Name
            var lbl2 = new Label 
            { 
                Text = "Database Name:", 
                Location = new System.Drawing.Point(15, 130), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtRestoreDatabaseName = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 150), 
                Size = new System.Drawing.Size(200, 22),
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            
            // Add tooltip for database name
            toolTipRestore.SetToolTip(txtRestoreDatabaseName, 
                "Database Name Examples:\n" +
                "• myapp_db\n" +
                "• production_data\n" +
                "• user_management\n" +
                "• jobpxa");
            
            // Authentication Mechanism
            var lblAuthRestore = new Label 
            { 
                Text = "Auth Mechanism:", 
                Location = new System.Drawing.Point(230, 130), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            cmbRestoreAuthMechanism = new ComboBox 
            { 
                Location = new System.Drawing.Point(230, 150), 
                Size = new System.Drawing.Size(150, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            cmbRestoreAuthMechanism.Items.AddRange(new string[] { "Auto", "SCRAM-SHA-1", "SCRAM-SHA-256", "MONGODB-CR", "PLAIN", "GSSAPI", "MONGODB-X509" });
            cmbRestoreAuthMechanism.SelectedIndex = 0; // Auto
            
            // Add tooltip for auth mechanism
            toolTipRestore.SetToolTip(cmbRestoreAuthMechanism, 
                "Authentication Mechanism:\n" +
                "• Auto: Let MongoDB driver choose\n" +
                "• SCRAM-SHA-1: Default for MongoDB 3.0+\n" +
                "• SCRAM-SHA-256: More secure, MongoDB 4.0+\n" +
                "• MONGODB-CR: Legacy authentication\n" +
                "• PLAIN: LDAP authentication\n" +
                "• GSSAPI: Kerberos authentication\n" +
                "• MONGODB-X509: Certificate authentication");
            
            // Restore Folder
            var lbl3 = new Label 
            { 
                Text = "Backup Folder:", 
                Location = new System.Drawing.Point(15, 185), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtRestoreFolder = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 205), 
                Size = new System.Drawing.Size(620, 22), 
                ReadOnly = true,
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            btnSelectRestoreFolder = new Button 
            { 
                Text = "Browse...", 
                Location = new System.Drawing.Point(645, 203), 
                Size = new System.Drawing.Size(70, 26),
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnSelectRestoreFolder.Click += (s, e) => {
                using (var dlg = new FolderBrowserDialog())
                    if (dlg.ShowDialog() == DialogResult.OK) txtRestoreFolder.Text = dlg.SelectedPath;
            };
            
            // Drop Existing CheckBox
            chkDropExisting = new CheckBox 
            { 
                Text = "Drop existing collections before restore", 
                Location = new System.Drawing.Point(15, 240), 
                Size = new System.Drawing.Size(350, 20), 
                Checked = false,
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.Transparent
            };
            
            // Test Connection Button
            btnTestRestoreConnection = new Button 
            { 
                Text = "Test Connection", 
                Location = new System.Drawing.Point(15, 270), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnTestRestoreConnection.Click += BtnTestRestoreConnection_Click;
            
            // Restore Button
            btnRestore = new Button 
            { 
                Text = "Start Restore", 
                Location = new System.Drawing.Point(145, 270), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                FlatStyle = FlatStyle.Standard,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            btnRestore.Click += BtnRestore_Click;
            
            // Progress & Status
            progressBarRestore = new ProgressBar 
            { 
                Location = new System.Drawing.Point(15, 315), 
                Size = new System.Drawing.Size(720, 18),
                Style = ProgressBarStyle.Continuous
            };
            lblStatusRestore = new Label 
            { 
                Text = "Ready to restore", 
                Location = new System.Drawing.Point(15, 340), 
                Size = new System.Drawing.Size(720, 18),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F)
            };
            
            // Log
            var lbl4 = new Label 
            { 
                Text = "Activity Log:", 
                Location = new System.Drawing.Point(15, 370), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.SystemColors.ControlText,
                Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular)
            };
            txtLogRestore = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 390), 
                Size = new System.Drawing.Size(720, 250), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical, 
                ReadOnly = true, 
                Font = new System.Drawing.Font("Courier New", 8.25F),
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.SystemColors.ControlText,
                BorderStyle = BorderStyle.Fixed3D
            };
            
            // Add to tab
            tabRestore.Controls.AddRange(new Control[] { headerPanel, lbl1, txtRestoreConnectionString, lbl2, txtRestoreDatabaseName, lblAuthRestore, cmbRestoreAuthMechanism, lbl3, txtRestoreFolder, btnSelectRestoreFolder, chkDropExisting, btnTestRestoreConnection, btnRestore, progressBarRestore, lblStatusRestore, lbl4, txtLogRestore });
            tabControl.TabPages.Add(tabRestore);
        }

        private void InitializeDefaultValues()
        {
            // Load saved connection strings and database names
            LoadConnectionStrings();
        }

        private void LoadConnectionStrings()
        {
            try
            {
                var backupConnStr = Properties.Settings.Default.BackupConnectionString;
                var backupDbName = Properties.Settings.Default.BackupDatabaseName;
                var restoreConnStr = Properties.Settings.Default.RestoreConnectionString;
                var restoreDbName = Properties.Settings.Default.RestoreDatabaseName;
                
                if (!string.IsNullOrEmpty(backupConnStr))
                    txtBackupConnectionString.Text = backupConnStr;
                else
                    txtBackupConnectionString.Text = "mongodb://username:password@host:port/database?options";
                
                if (!string.IsNullOrEmpty(backupDbName))
                    txtBackupDatabaseName.Text = backupDbName;
                else
                    txtBackupDatabaseName.Text = "database_name";
                
                if (!string.IsNullOrEmpty(restoreConnStr))
                    txtRestoreConnectionString.Text = restoreConnStr;
                else
                    txtRestoreConnectionString.Text = "mongodb://username:password@host:port/database?options";
                
                if (!string.IsNullOrEmpty(restoreDbName))
                    txtRestoreDatabaseName.Text = restoreDbName;
                else
                    txtRestoreDatabaseName.Text = "database_name";
            }
            catch
            {
                // If settings don't exist, use default template
                txtBackupConnectionString.Text = "mongodb://username:password@host:port/database?options";
                txtBackupDatabaseName.Text = "database_name";
                txtRestoreConnectionString.Text = "mongodb://username:password@host:port/database?options";
                txtRestoreDatabaseName.Text = "database_name";
            }
        }

        private void SaveConnectionStrings()
        {
            try
            {
                Properties.Settings.Default.BackupConnectionString = txtBackupConnectionString.Text;
                Properties.Settings.Default.BackupDatabaseName = txtBackupDatabaseName.Text;
                Properties.Settings.Default.RestoreConnectionString = txtRestoreConnectionString.Text;
                Properties.Settings.Default.RestoreDatabaseName = txtRestoreDatabaseName.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                LogMessageBackup($"Warning: Could not save settings: {ex.Message}");
            }
        }

        private string BuildConnectionStringWithAuth(string baseConnectionString, ComboBox authComboBox)
        {
            if (authComboBox.SelectedIndex == 0) // Auto
                return baseConnectionString;

            var authMechanism = authComboBox.SelectedItem.ToString();
            var separator = baseConnectionString.Contains("?") ? "&" : "?";
            return $"{baseConnectionString}{separator}authMechanism={authMechanism}";
        }

        private async void BtnTestBackupConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBackupConnectionString.Text))
            {
                MessageBox.Show("Please enter connection string!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnTestBackupConnection.Enabled = false;
            btnTestBackupConnection.Text = "Testing...";
            lblStatusBackup.Text = "🔄 Testing connection...";

            try
            {
                var connectionString = BuildConnectionStringWithAuth(txtBackupConnectionString.Text.Trim(), cmbBackupAuthMechanism);
                var client = new MongoClient(connectionString);
                await client.ListDatabaseNamesAsync();
                
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusBackup.Text = "✅ Connection successful!";
                LogMessageBackup($"Connection test successful using auth mechanism: {cmbBackupAuthMechanism.SelectedItem}");
                
                // Save connection string on successful test
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (errorMessage.Contains("SCRAM-SHA-1"))
                {
                    errorMessage += "\n\n💡 Try using 'SCRAM-SHA-256' or 'MONGODB-CR' authentication mechanism instead.";
                }
                MessageBox.Show($"Connection failed: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBackup.Text = "❌ Connection failed!";
                LogMessageBackup($"Connection test failed: {ex.Message}");
            }
            finally
            {
                btnTestBackupConnection.Enabled = true;
                btnTestBackupConnection.Text = "Test Connection";
            }
        }

        private async void BtnBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBackupConnectionString.Text) || string.IsNullOrWhiteSpace(txtBackupDatabaseName.Text) || string.IsNullOrWhiteSpace(txtBackupFolder.Text))
            {
                MessageBox.Show("Please fill all required fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnBackup.Enabled = false;
            progressBarBackup.Value = 0;
            progressBarBackup.Maximum = 100;
            txtLogBackup.Clear();
            lblStatusBackup.Text = "🔄 Starting backup...";

            try
            {
                await PerformBackup();
                MessageBox.Show("Backup completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusBackup.Text = "✅ Backup completed successfully!";
                
                // Save connection string on successful backup
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                LogMessageBackup($"Error: {ex.Message}");
                MessageBox.Show($"Backup failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBackup.Text = "❌ Backup failed!";
            }
            finally
            {
                btnBackup.Enabled = true;
                progressBarBackup.Value = progressBarBackup.Maximum;
            }
        }

        private async void BtnTestRestoreConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRestoreConnectionString.Text))
            {
                MessageBox.Show("Please enter connection string!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnTestRestoreConnection.Enabled = false;
            btnTestRestoreConnection.Text = "Testing...";
            lblStatusRestore.Text = "🔄 Testing connection...";

            try
            {
                var connectionString = BuildConnectionStringWithAuth(txtRestoreConnectionString.Text.Trim(), cmbRestoreAuthMechanism);
                var client = new MongoClient(connectionString);
                await client.ListDatabaseNamesAsync();
                
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusRestore.Text = "✅ Connection successful!";
                LogMessageRestore($"Connection test successful using auth mechanism: {cmbRestoreAuthMechanism.SelectedItem}");
                
                // Save connection string on successful test
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (errorMessage.Contains("SCRAM-SHA-1"))
                {
                    errorMessage += "\n\n💡 Try using 'SCRAM-SHA-256' or 'MONGODB-CR' authentication mechanism instead.";
                }
                MessageBox.Show($"Connection failed: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusRestore.Text = "❌ Connection failed!";
                LogMessageRestore($"Connection test failed: {ex.Message}");
            }
            finally
            {
                btnTestRestoreConnection.Enabled = true;
                btnTestRestoreConnection.Text = "Test Connection";
            }
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRestoreConnectionString.Text) || string.IsNullOrWhiteSpace(txtRestoreDatabaseName.Text) || string.IsNullOrWhiteSpace(txtRestoreFolder.Text))
            {
                MessageBox.Show("Please fill all required fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(txtRestoreFolder.Text))
            {
                MessageBox.Show("Backup folder does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnRestore.Enabled = false;
            progressBarRestore.Value = 0;
            progressBarRestore.Maximum = 100;
            txtLogRestore.Clear();
            lblStatusRestore.Text = "🔄 Starting restore...";

            try
            {
                await PerformRestore();
                MessageBox.Show("Restore completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusRestore.Text = "✅ Restore completed successfully!";
                
                // Save connection string on successful restore
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                LogMessageRestore($"Error: {ex.Message}");
                MessageBox.Show($"Restore failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusRestore.Text = "❌ Restore failed!";
            }
            finally
            {
                btnRestore.Enabled = true;
                progressBarRestore.Value = progressBarRestore.Maximum;
            }
        }

        private async Task PerformBackup()
        {
            var connectionString = BuildConnectionStringWithAuth(txtBackupConnectionString.Text.Trim(), cmbBackupAuthMechanism);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(txtBackupDatabaseName.Text.Trim());
            var backupFolder = txtBackupFolder.Text.Trim();

            LogMessageBackup("Connected to MongoDB successfully");

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupFolder, $"{txtBackupDatabaseName.Text.Trim()}_backup_{timestamp}");
            Directory.CreateDirectory(backupPath);

            var collections = await database.ListCollectionNamesAsync();
            var collectionNames = await collections.ToListAsync();

            LogMessageBackup($"Found {collectionNames.Count} collections");
            progressBarBackup.Maximum = collectionNames.Count;

            for (int i = 0; i < collectionNames.Count; i++)
            {
                var collectionName = collectionNames[i];
                LogMessageBackup($"Backing up collection: {collectionName}");

                var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
                var documents = await collection.Find(_ => true).ToListAsync();

                var jsonArray = documents.Select(doc => doc.ToDictionary()).ToList();
                var json = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                var filePath = Path.Combine(backupPath, $"{collectionName}.json");
                await File.WriteAllTextAsync(filePath, json);

                LogMessageBackup($"Saved {collectionName}.json with {documents.Count} documents");
                progressBarBackup.Value = i + 1;
                lblStatusBackup.Text = $"🔄 Backing up... {i + 1}/{collectionNames.Count}";
                Application.DoEvents();
            }

            LogMessageBackup($"Backup completed! Saved to: {backupPath}");
        }

        private async Task PerformRestore()
        {
            var connectionString = BuildConnectionStringWithAuth(txtRestoreConnectionString.Text.Trim(), cmbRestoreAuthMechanism);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(txtRestoreDatabaseName.Text.Trim());
            var backupFolder = txtRestoreFolder.Text.Trim();

            LogMessageRestore("Connected to MongoDB successfully");

            var jsonFiles = Directory.GetFiles(backupFolder, "*.json")
                .Where(f => !Path.GetFileName(f).Equals("backup_info.json", StringComparison.OrdinalIgnoreCase))
                .ToList();

            LogMessageRestore($"Found {jsonFiles.Count} collection files to restore");
            progressBarRestore.Maximum = jsonFiles.Count;

            for (int i = 0; i < jsonFiles.Count; i++)
            {
                var jsonFile = jsonFiles[i];
                var collectionName = Path.GetFileNameWithoutExtension(jsonFile);
                
                LogMessageRestore($"Restoring collection: {collectionName}");

                var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);

                if (chkDropExisting.Checked)
                {
                    await database.DropCollectionAsync(collectionName);
                    LogMessageRestore($"Dropped existing collection: {collectionName}");
                }

                var jsonContent = await File.ReadAllTextAsync(jsonFile);
                var documents = JsonConvert.DeserializeObject<List<object>>(jsonContent);

                if (documents != null && documents.Count > 0)
                {
                    var bsonDocuments = documents.Select(doc => MongoDB.Bson.BsonDocument.Parse(JsonConvert.SerializeObject(doc))).ToList();
                    await collection.InsertManyAsync(bsonDocuments);
                    LogMessageRestore($"Restored {bsonDocuments.Count} documents to {collectionName}");
                }

                progressBarRestore.Value = i + 1;
                lblStatusRestore.Text = $"🔄 Restoring... {i + 1}/{jsonFiles.Count}";
                Application.DoEvents();
            }

            LogMessageRestore("Restore completed successfully!");
        }

        private void LogMessageBackup(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";
            txtLogBackup.AppendText(logMessage + Environment.NewLine);
            txtLogBackup.SelectionStart = txtLogBackup.Text.Length;
            txtLogBackup.ScrollToCaret();
        }

        private void LogMessageRestore(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";
            txtLogRestore.AppendText(logMessage + Environment.NewLine);
            txtLogRestore.SelectionStart = txtLogRestore.Text.Length;
            txtLogRestore.ScrollToCaret();
        }
    }
}
