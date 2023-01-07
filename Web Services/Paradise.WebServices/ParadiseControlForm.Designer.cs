namespace Paradise.WebServices {
	partial class ParadiseControlForm {
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.trayMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.fileServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.startHttpServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stopHttpServerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.serviceListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.startServicesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stopServicesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restartAllServicesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.databaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.databaseOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.databaseCloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.openLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openConsoleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.trayMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenuStrip = this.trayMenuStrip;
			this.notifyIcon.Text = "Paradise Web Services";
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnTrayIconClicked);
			// 
			// trayMenuStrip
			// 
			this.trayMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileServerMenuItem,
            this.serviceListMenuItem,
            this.databaseMenuItem,
            this.toolStripSeparator2,
            this.openLogMenuItem,
            this.openConsoleMenuItem,
            this.toolStripSeparator1,
            this.quitMenuItem});
			this.trayMenuStrip.Name = "trayMenuStrip";
			this.trayMenuStrip.Size = new System.Drawing.Size(150, 148);
			// 
			// fileServerMenuItem
			// 
			this.fileServerMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startHttpServerMenuItem,
            this.stopHttpServerMenuItem});
			this.fileServerMenuItem.Name = "fileServerMenuItem";
			this.fileServerMenuItem.Size = new System.Drawing.Size(149, 22);
			this.fileServerMenuItem.Text = "HTTP Server";
			// 
			// startHttpServerMenuItem
			// 
			this.startHttpServerMenuItem.Name = "startHttpServerMenuItem";
			this.startHttpServerMenuItem.Size = new System.Drawing.Size(180, 22);
			this.startHttpServerMenuItem.Text = "Start HTTP Server";
			this.startHttpServerMenuItem.Click += new System.EventHandler(this.OnStartHttpServerMenuItemClicked);
			// 
			// stopHttpServerMenuItem
			// 
			this.stopHttpServerMenuItem.Enabled = false;
			this.stopHttpServerMenuItem.Name = "stopHttpServerMenuItem";
			this.stopHttpServerMenuItem.Size = new System.Drawing.Size(180, 22);
			this.stopHttpServerMenuItem.Text = "Stop HTTP Server";
			this.stopHttpServerMenuItem.Click += new System.EventHandler(this.OnStopHttpServerMenuItemClicked);
			// 
			// serviceListMenuItem
			// 
			this.serviceListMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startServicesMenuItem,
            this.stopServicesMenuItem,
            this.restartAllServicesMenuItem,
            this.toolStripSeparator4});
			this.serviceListMenuItem.Name = "serviceListMenuItem";
			this.serviceListMenuItem.Size = new System.Drawing.Size(149, 22);
			this.serviceListMenuItem.Text = "Services";
			// 
			// startServicesMenuItem
			// 
			this.startServicesMenuItem.Name = "startServicesMenuItem";
			this.startServicesMenuItem.Size = new System.Drawing.Size(155, 22);
			this.startServicesMenuItem.Text = "Start Services";
			this.startServicesMenuItem.Click += new System.EventHandler(this.OnStartAllServicesMenuItemClicked);
			// 
			// stopServicesMenuItem
			// 
			this.stopServicesMenuItem.Enabled = false;
			this.stopServicesMenuItem.Name = "stopServicesMenuItem";
			this.stopServicesMenuItem.Size = new System.Drawing.Size(155, 22);
			this.stopServicesMenuItem.Text = "Stop Services";
			this.stopServicesMenuItem.Click += new System.EventHandler(this.OnStopAllServicesMenuItemClicked);
			// 
			// restartAllServicesMenuItem
			// 
			this.restartAllServicesMenuItem.Enabled = false;
			this.restartAllServicesMenuItem.Name = "restartAllServicesMenuItem";
			this.restartAllServicesMenuItem.Size = new System.Drawing.Size(155, 22);
			this.restartAllServicesMenuItem.Text = "Restart Services";
			this.restartAllServicesMenuItem.Click += new System.EventHandler(this.OnRestartAllServicesMenuItemClicked);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(152, 6);
			// 
			// databaseMenuItem
			// 
			this.databaseMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.databaseOpenMenuItem,
            this.databaseCloseMenuItem});
			this.databaseMenuItem.Name = "databaseMenuItem";
			this.databaseMenuItem.Size = new System.Drawing.Size(149, 22);
			this.databaseMenuItem.Text = "Database";
			// 
			// databaseOpenMenuItem
			// 
			this.databaseOpenMenuItem.Name = "databaseOpenMenuItem";
			this.databaseOpenMenuItem.Size = new System.Drawing.Size(212, 22);
			this.databaseOpenMenuItem.Text = "Connect to database";
			this.databaseOpenMenuItem.Click += new System.EventHandler(this.DatabaseOpenMenuItemClicked);
			// 
			// databaseCloseMenuItem
			// 
			this.databaseCloseMenuItem.Enabled = false;
			this.databaseCloseMenuItem.Name = "databaseCloseMenuItem";
			this.databaseCloseMenuItem.Size = new System.Drawing.Size(212, 22);
			this.databaseCloseMenuItem.Text = "Disconnect from database";
			this.databaseCloseMenuItem.Click += new System.EventHandler(this.DatabaseCloseMenuItemClicked);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
			// 
			// openLogMenuItem
			// 
			this.openLogMenuItem.Name = "openLogMenuItem";
			this.openLogMenuItem.Size = new System.Drawing.Size(149, 22);
			this.openLogMenuItem.Text = "Open Log";
			this.openLogMenuItem.Click += new System.EventHandler(this.OpenLogMenuItemClicked);
			// 
			// openConsoleMenuItem
			// 
			this.openConsoleMenuItem.Name = "openConsoleMenuItem";
			this.openConsoleMenuItem.Size = new System.Drawing.Size(149, 22);
			this.openConsoleMenuItem.Text = "Open Console";
			this.openConsoleMenuItem.Click += new System.EventHandler(this.OpenConsoleMenuItemClicked);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
			// 
			// quitMenuItem
			// 
			this.quitMenuItem.Name = "quitMenuItem";
			this.quitMenuItem.Size = new System.Drawing.Size(149, 22);
			this.quitMenuItem.Text = "Quit Paradise";
			this.quitMenuItem.Click += new System.EventHandler(this.QuitMenuItemClicked);
			// 
			// label1
			// 
			this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(776, 432);
			this.label1.TabIndex = 0;
			this.label1.Text = "This should not be visible.\r\nPlease contact your friendly neighbourhood developer" +
    ".";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ParadiseControlForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.label1);
			this.Name = "ParadiseControlForm";
			this.Text = "Form1";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.trayMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ContextMenuStrip trayMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem quitMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem openLogMenuItem;
		private System.Windows.Forms.ToolStripMenuItem serviceListMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startServicesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopServicesMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem fileServerMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startHttpServerMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopHttpServerMenuItem;
		private System.Windows.Forms.ToolStripMenuItem restartAllServicesMenuItem;
		private System.Windows.Forms.ToolStripMenuItem databaseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem databaseCloseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem databaseOpenMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem openConsoleMenuItem;
	}
}

