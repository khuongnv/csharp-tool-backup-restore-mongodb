using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDbTools.Properties;

namespace MongoDbTools
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private TabControl tabControl;
        private TabPage tabBackup, tabRestore;
        
        // Backup controls
        private TextBox txtBackupConnectionString, txtBackupDatabaseName, txtBackupFolder;
        private Button btnSelectBackupFolder, btnBackup, btnTestBackupConnection;
        private ProgressBar progressBarBackup;
        private TextBox txtLogBackup;
        private Label lblStatusBackup;
        
        // Restore controls  
        private TextBox txtRestoreConnectionString, txtRestoreDatabaseName, txtRestoreFolder;
        private Button btnSelectRestoreFolder, btnRestore, btnTestRestoreConnection;
        private ProgressBar progressBarRestore;
        private TextBox txtLogRestore;
        private Label lblStatusRestore;
        private CheckBox chkDropExisting;

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
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            // Add custom title bar
            CreateTitleBar();

            // Tab Control with modern styling
            tabControl = new TabControl
            {
                Location = new System.Drawing.Point(10, 42),
                Size = new System.Drawing.Size(980, 678),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.FromArgb(255, 255, 255),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new System.Drawing.Size(200, 32),
                SizeMode = TabSizeMode.Fixed
            };
            tabControl.DrawItem += TabControl_DrawItem;

            // Create tabs
            CreateBackupTab();
            CreateRestoreTab();
            
            this.Controls.Add(tabControl);
        }

        private void CreateTitleBar()
        {
            var titleBar = new Panel
            {
                Location = new System.Drawing.Point(0, 0),
                Size = new System.Drawing.Size(1000, 32),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
            };

            var titleLabel = new Label
            {
                Text = "MongoDB Tools",
                Location = new System.Drawing.Point(12, 8),
                Size = new System.Drawing.Size(400, 16),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.Transparent
            };

            var closeButton = new Button
            {
                Text = "✕",
                Location = new System.Drawing.Point(960, 4),
                Size = new System.Drawing.Size(24, 24),
                BackColor = System.Drawing.Color.FromArgb(232, 17, 35),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            var minimizeButton = new Button
            {
                Text = "—",
                Location = new System.Drawing.Point(936, 4),
                Size = new System.Drawing.Size(24, 24),
                BackColor = System.Drawing.Color.FromArgb(60, 60, 65),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Bold)
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            titleBar.Controls.AddRange(new Control[] { titleLabel, minimizeButton, closeButton });
            this.Controls.Add(titleBar);

            // Make title bar draggable
            titleBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0x112, 0xf012, 0);
                }
            };
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            var tabPage = tabControl.TabPages[e.Index];
            var tabRect = tabControl.GetTabRect(e.Index);

            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = isSelected ? System.Drawing.Color.FromArgb(0, 120, 215) : System.Drawing.Color.FromArgb(240, 240, 240);
            var textColor = isSelected ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(64, 64, 64);

            using (var brush = new System.Drawing.SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            var textRect = new System.Drawing.Rectangle(tabRect.X + 5, tabRect.Y + 8, tabRect.Width - 10, tabRect.Height - 16);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, textRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void CreateBackupTab()
        {
            tabBackup = new TabPage("Backup Database");
            tabBackup.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            
            // Header Panel
            var headerPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(950, 28),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
            };
            
            var headerLabel = new Label
            {
                Text = "Backup MongoDB Database",
                Location = new System.Drawing.Point(12, 6),
                Size = new System.Drawing.Size(400, 16),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.Transparent
            };
            headerPanel.Controls.Add(headerLabel);
            
            // Connection String
            var lbl1 = new Label 
            { 
                Text = "Connection String:", 
                Location = new System.Drawing.Point(15, 55), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtBackupConnectionString = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 75), 
                Size = new System.Drawing.Size(720, 20), 
                Multiline = true, 
                Height = 40,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Consolas", 8)
            };
            
            // Add tooltip for connection string format
            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtBackupConnectionString, 
                "MongoDB Connection String Examples:\n" +
                "• Local: mongodb://localhost:27017\n" +
                "• With Auth: mongodb://username:password@host:port/database\n" +
                "• Atlas: mongodb+srv://username:password@cluster.mongodb.net/database\n" +
                "• With Options: mongodb://host:port/database?retryWrites=true&w=majority");
            
            // Database Name
            var lbl2 = new Label 
            { 
                Text = "Database Name:", 
                Location = new System.Drawing.Point(15, 130), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtBackupDatabaseName = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 150), 
                Size = new System.Drawing.Size(200, 22),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            
            // Add tooltip for database name
            toolTip.SetToolTip(txtBackupDatabaseName, 
                "Database Name Examples:\n" +
                "• myapp_db\n" +
                "• production_data\n" +
                "• user_management\n" +
                "• jobpxa");
            
            // Backup Folder
            var lbl3 = new Label 
            { 
                Text = "Backup Folder:", 
                Location = new System.Drawing.Point(15, 185), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtBackupFolder = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 205), 
                Size = new System.Drawing.Size(620, 22), 
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnSelectBackupFolder = new Button 
            { 
                Text = "Browse", 
                Location = new System.Drawing.Point(645, 203), 
                Size = new System.Drawing.Size(70, 26),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Regular)
            };
            btnSelectBackupFolder.FlatAppearance.BorderSize = 0;
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
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            btnTestBackupConnection.FlatAppearance.BorderSize = 0;
            btnTestBackupConnection.Click += BtnTestBackupConnection_Click;
            
            // Backup Button
            btnBackup = new Button 
            { 
                Text = "Start Backup", 
                Location = new System.Drawing.Point(145, 245), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.Color.FromArgb(40, 167, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            btnBackup.FlatAppearance.BorderSize = 0;
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
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            
            // Log
            var lbl4 = new Label 
            { 
                Text = "Activity Log:", 
                Location = new System.Drawing.Point(15, 345), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtLogBackup = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 365), 
                Size = new System.Drawing.Size(720, 250), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical, 
                ReadOnly = true, 
                Font = new System.Drawing.Font("Consolas", 8),
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Add to tab
            tabBackup.Controls.AddRange(new Control[] { headerPanel, lbl1, txtBackupConnectionString, lbl2, txtBackupDatabaseName, lbl3, txtBackupFolder, btnSelectBackupFolder, btnTestBackupConnection, btnBackup, progressBarBackup, lblStatusBackup, lbl4, txtLogBackup });
            tabControl.TabPages.Add(tabBackup);
        }

        private void CreateRestoreTab()
        {
            tabRestore = new TabPage("Restore Database");
            tabRestore.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            
            // Header Panel
            var headerPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(950, 28),
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69)
            };
            
            var headerLabel = new Label
            {
                Text = "Restore MongoDB Database",
                Location = new System.Drawing.Point(12, 6),
                Size = new System.Drawing.Size(400, 16),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.Transparent
            };
            headerPanel.Controls.Add(headerLabel);
            
            // Connection String
            var lbl1 = new Label 
            { 
                Text = "Connection String:", 
                Location = new System.Drawing.Point(15, 55), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtRestoreConnectionString = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 75), 
                Size = new System.Drawing.Size(720, 20), 
                Multiline = true, 
                Height = 40,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Consolas", 8)
            };
            
            // Add tooltip for connection string format
            var toolTipRestore = new ToolTip();
            toolTipRestore.SetToolTip(txtRestoreConnectionString, 
                "MongoDB Connection String Examples:\n" +
                "• Local: mongodb://localhost:27017\n" +
                "• With Auth: mongodb://username:password@host:port/database\n" +
                "• Atlas: mongodb+srv://username:password@cluster.mongodb.net/database\n" +
                "• With Options: mongodb://host:port/database?retryWrites=true&w=majority");
            
            // Database Name
            var lbl2 = new Label 
            { 
                Text = "Database Name:", 
                Location = new System.Drawing.Point(15, 130), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtRestoreDatabaseName = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 150), 
                Size = new System.Drawing.Size(200, 22),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            
            // Add tooltip for database name
            toolTipRestore.SetToolTip(txtRestoreDatabaseName, 
                "Database Name Examples:\n" +
                "• myapp_db\n" +
                "• production_data\n" +
                "• user_management\n" +
                "• jobpxa");
            
            // Restore Folder
            var lbl3 = new Label 
            { 
                Text = "Backup Folder:", 
                Location = new System.Drawing.Point(15, 185), 
                Size = new System.Drawing.Size(120, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtRestoreFolder = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 205), 
                Size = new System.Drawing.Size(620, 22), 
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnSelectRestoreFolder = new Button 
            { 
                Text = "Browse", 
                Location = new System.Drawing.Point(645, 203), 
                Size = new System.Drawing.Size(70, 26),
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8, System.Drawing.FontStyle.Regular)
            };
            btnSelectRestoreFolder.FlatAppearance.BorderSize = 0;
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
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular),
                BackColor = System.Drawing.Color.Transparent
            };
            
            // Test Connection Button
            btnTestRestoreConnection = new Button 
            { 
                Text = "Test Connection", 
                Location = new System.Drawing.Point(15, 270), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            btnTestRestoreConnection.FlatAppearance.BorderSize = 0;
            btnTestRestoreConnection.Click += BtnTestRestoreConnection_Click;
            
            // Restore Button
            btnRestore = new Button 
            { 
                Text = "Start Restore", 
                Location = new System.Drawing.Point(145, 270), 
                Size = new System.Drawing.Size(120, 30), 
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            btnRestore.FlatAppearance.BorderSize = 0;
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
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            
            // Log
            var lbl4 = new Label 
            { 
                Text = "Activity Log:", 
                Location = new System.Drawing.Point(15, 370), 
                Size = new System.Drawing.Size(100, 16),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular)
            };
            txtLogRestore = new TextBox 
            { 
                Location = new System.Drawing.Point(15, 390), 
                Size = new System.Drawing.Size(720, 250), 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical, 
                ReadOnly = true, 
                Font = new System.Drawing.Font("Consolas", 8),
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Add to tab
            tabRestore.Controls.AddRange(new Control[] { headerPanel, lbl1, txtRestoreConnectionString, lbl2, txtRestoreDatabaseName, lbl3, txtRestoreFolder, btnSelectRestoreFolder, chkDropExisting, btnTestRestoreConnection, btnRestore, progressBarRestore, lblStatusRestore, lbl4, txtLogRestore });
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
                var client = new MongoClient(txtBackupConnectionString.Text.Trim());
                await client.ListDatabaseNamesAsync();
                
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusBackup.Text = "✅ Connection successful!";
                LogMessageBackup("Connection test successful");
                
                // Save connection string on successful test
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var client = new MongoClient(txtRestoreConnectionString.Text.Trim());
                await client.ListDatabaseNamesAsync();
                
                MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatusRestore.Text = "✅ Connection successful!";
                LogMessageRestore("Connection test successful");
                
                // Save connection string on successful test
                SaveConnectionStrings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var client = new MongoClient(txtBackupConnectionString.Text.Trim());
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
            var client = new MongoClient(txtRestoreConnectionString.Text.Trim());
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
