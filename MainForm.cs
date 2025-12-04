using System;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsCleaner
{
    public class MainForm : Form
    {
        private MenuStrip menu = new MenuStrip();
        private ToolStripMenuItem fileMenu = null!;
        private ToolStripMenuItem exportLogsMenuItem = null!;
        private ToolStripMenuItem exitMenuItem = null!;

        private Button btnDryRun = null!;
        private Button btnClean = null!;
        private Button btnCancel = null!;

        private CheckBox chkRecycle = null!;
        private CheckBox chkSystemTemp = null!;
        private CheckBox chkChrome = null!;
        private CheckBox chkEdge = null!;
        private CheckBox chkWindowsUpdate = null!;
        private CheckBox chkThumbnails = null!;
        private CheckBox chkPrefetch = null!;
        private CheckBox chkFlushDns = null!;
        private CheckBox chkVerbose = null!;
        private CheckBox chkAdvanced = null!;

        private ListView lvLogs = null!;
        private ColoredProgressBar progressBar = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        private CancellationTokenSource? _cts;
        private Color _accentColor = Color.FromArgb(0, 120, 215);
        private bool _isDark = false;

#pragma warning disable CS8774
        public MainForm()
        {
            Text = "Windows Cleaner";
            Width = 900;
            Height = 600;

            InitializeComponents();
            Logger.Init();
            Logger.OnLog += Logger_OnLog;
        }

        [MemberNotNull(nameof(menu), nameof(fileMenu), nameof(exportLogsMenuItem), nameof(exitMenuItem),
            nameof(btnDryRun), nameof(btnClean), nameof(btnCancel), nameof(chkRecycle), nameof(chkSystemTemp),
            nameof(chkChrome), nameof(chkEdge), nameof(chkWindowsUpdate), nameof(chkThumbnails), nameof(chkPrefetch),
            nameof(chkFlushDns), nameof(chkVerbose), nameof(lvLogs), nameof(progressBar), nameof(statusStrip), nameof(statusLabel))]
        private void InitializeComponents()
        {
            menu = new MenuStrip();
            fileMenu = new ToolStripMenuItem("Fichier");
            var clearLogsMenuItem = new ToolStripMenuItem("Effacer les logs");
            exportLogsMenuItem = new ToolStripMenuItem("Exporter les logs");
            exitMenuItem = new ToolStripMenuItem("Quitter");
            var helpMenu = new ToolStripMenuItem("Aide");
            var aboutMenuItem = new ToolStripMenuItem("À propos");
            aboutMenuItem.Click += AboutMenuItem_Click;
            clearLogsMenuItem.Click += ClearLogsMenuItem_Click;
            exportLogsMenuItem.Click += ExportLogsMenuItem_Click;
            exitMenuItem.Click += (s, e) => Close();
            fileMenu.DropDownItems.Add(clearLogsMenuItem);
            fileMenu.DropDownItems.Add(exportLogsMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);
            menu.Items.Add(fileMenu);
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menu.Items.Add(helpMenu);

            // Affichage / thèmes
            var viewMenu = new ToolStripMenuItem("Affichage");
            var themeLight = new ToolStripMenuItem("Thème Clair");
            var themeDark = new ToolStripMenuItem("Thème Sombre");
            var accentBlue = new ToolStripMenuItem("Accent Bleu");
            var accentGreen = new ToolStripMenuItem("Accent Vert");
            var accentOrange = new ToolStripMenuItem("Accent Orange");
            themeLight.Click += (s, e) => ApplyTheme(isDark: false, accent: Color.FromArgb(0, 120, 215));
            themeDark.Click += (s, e) => ApplyTheme(isDark: true, accent: Color.FromArgb(0, 120, 215));
            accentBlue.Click += (s, e) => ApplyAccent(Color.FromArgb(0, 120, 215));
            accentGreen.Click += (s, e) => ApplyAccent(Color.FromArgb(0, 153, 51));
            accentOrange.Click += (s, e) => ApplyAccent(Color.FromArgb(255, 140, 0));
            viewMenu.DropDownItems.Add(themeLight);
            viewMenu.DropDownItems.Add(themeDark);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(accentBlue);
            viewMenu.DropDownItems.Add(accentGreen);
            viewMenu.DropDownItems.Add(accentOrange);
            menu.Items.Add(viewMenu);
            Controls.Add(menu);

            btnDryRun = new Button() { Text = "Dry Run", Left = 10, Top = 30, Width = 100 };
            btnClean = new Button() { Text = "Nettoyer", Left = 120, Top = 30, Width = 100 };
            btnCancel = new Button() { Text = "Annuler", Left = 230, Top = 30, Width = 100, Enabled = false };

            chkRecycle = new CheckBox() { Text = "Vider la Corbeille", Left = 350, Top = 34, Width = 150 };
            chkSystemTemp = new CheckBox() { Text = "Inclure Temp système", Left = 510, Top = 34, Width = 170 };

            chkChrome = new CheckBox() { Text = "Nettoyer cache Chrome", Left = 10, Top = 65, Width = 200 };
            chkEdge = new CheckBox() { Text = "Nettoyer cache Edge", Left = 220, Top = 65, Width = 180 };
            chkWindowsUpdate = new CheckBox() { Text = "Nettoyer Windows Update", Left = 410, Top = 65, Width = 200 };
            chkThumbnails = new CheckBox() { Text = "Nettoyer vignettes (thumbcache)", Left = 10, Top = 95, Width = 260 };
            chkPrefetch = new CheckBox() { Text = "Nettoyer Prefetch", Left = 280, Top = 95, Width = 140 };
            chkFlushDns = new CheckBox() { Text = "Flush DNS", Left = 430, Top = 95, Width = 100 };
            chkVerbose = new CheckBox() { Text = "Verbose", Left = 540, Top = 95, Width = 100 };
            chkAdvanced = new CheckBox() { Text = "Nettoyage avancé (rapport avant exécution)", Left = 10, Top = 110, Width = 350 };

            lvLogs = new ListView() { Left = 10, Top = 130, Width = 860, Height = 380, View = View.Details, FullRowSelect = true };
            lvLogs.Columns.Add("Heure", 160);
            lvLogs.Columns.Add("Niveau", 100);
            lvLogs.Columns.Add("Message", 580);

            // Improved ListView appearance: owner-draw, alternating rows, double-buffered
            lvLogs.OwnerDraw = true;
            lvLogs.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvLogs.DrawColumnHeader += LvLogs_DrawColumnHeader;
            lvLogs.DrawItem += LvLogs_DrawItem;
            lvLogs.DrawSubItem += LvLogs_DrawSubItem;
            // enable double buffering to reduce flicker
            try { typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(lvLogs, true, null); } catch { }
            // increase row height via a tiny ImageList hack
            try
            {
                var img = new ImageList();
                img.ImageSize = new Size(1, 22); // height of rows
                lvLogs.SmallImageList = img;
                lvLogs.Font = new Font("Segoe UI", 9);
            }
            catch { }

            progressBar = new ColoredProgressBar() { Left = 10, Top = 520, Width = 700, Height = 20, BackColor = menu != null ? menu.BackColor : SystemColors.Control, ForeColor = this.ForeColor };
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.BarColor = _accentColor;
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Prêt");
            statusStrip.Items.Add(statusLabel);

            Controls.Add(btnDryRun);
            Controls.Add(btnClean);
            Controls.Add(btnCancel);
            Controls.Add(chkRecycle);
            Controls.Add(chkSystemTemp);
            Controls.Add(chkChrome);
            Controls.Add(chkEdge);
            Controls.Add(chkWindowsUpdate);
            Controls.Add(chkThumbnails);
            Controls.Add(chkPrefetch);
            Controls.Add(chkFlushDns);
            Controls.Add(chkVerbose);
            Controls.Add(chkAdvanced);
            Controls.Add(lvLogs);
            Controls.Add(progressBar);
            Controls.Add(statusStrip);

            btnDryRun.Click += async (s, e) => await StartCleanerAsync(dryRun: true);
            btnClean.Click += async (s, e) => await StartCleanerAsync(dryRun: false);
            btnCancel.Click += (s, e) => Cancel();

            // button visual polish
            foreach (var b in new[] { btnDryRun, btnClean, btnCancel })
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.Padding = new Padding(6);
                b.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            }

            MainMenuStrip = menu;

            // apply default theme
            ApplyTheme(isDark: false, accent: _accentColor);
        }
#pragma warning restore CS8774

        private void ApplyAccent(Color accent)
        {
            _accentColor = accent;
            ApplyTheme(_isDark, _accentColor);
        }

        private void ApplyTheme(bool isDark, Color accent)
        {
            _isDark = isDark;
            _accentColor = accent;

            Color baseBack = isDark ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Color controlBack = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color baseFore = isDark ? Color.WhiteSmoke : Color.Black;

            try
            {
                this.BackColor = baseBack;
                this.ForeColor = baseFore;

                if (menu != null)
                {
                    menu.BackColor = controlBack;
                    menu.ForeColor = baseFore;
                }

                if (statusStrip != null)
                {
                    statusStrip.BackColor = controlBack;
                    statusStrip.ForeColor = baseFore;
                }

                if (lvLogs != null)
                {
                    lvLogs.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                    lvLogs.ForeColor = baseFore;
                }

                if (progressBar != null)
                {
                    try { progressBar.BackColor = controlBack; } catch { }
                    try { progressBar.ForeColor = accent; } catch { }
                }

                foreach (Control c in this.Controls)
                {
                    try
                    {
                        if (c is Button)
                        {
                            c.BackColor = accent;
                            c.ForeColor = Color.White;
                        }
                        else if (c is ListView)
                        {
                            // already set
                        }
                        else if (c is StatusStrip)
                        {
                            // already set
                        }
                        else
                        {
                            c.BackColor = controlBack;
                            c.ForeColor = baseFore;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void LvLogs_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (e == null || e.Header == null) return;
            try
            {
                using var b = new SolidBrush(menu?.BackColor ?? SystemColors.Control);
                e.Graphics.FillRectangle(b, e.Bounds);
                using var f = new SolidBrush(this.ForeColor);
                e.Graphics.DrawString(e.Header.Text, this.Font, f, e.Bounds);
            }
            catch { }
        }

        private void LvLogs_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // handled in DrawSubItem for details view
        }

        private void LvLogs_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e == null || e.Item == null || e.SubItem == null) return;
            try
            {
                var g = e.Graphics;
                int idx = e.Item.Index;
                bool selected = e.Item.Selected;

                Color bg;
                if (selected) bg = _accentColor;
                else if (idx % 2 == 0) bg = _isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                else bg = _isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(250, 250, 250);

                using (var sb = new SolidBrush(bg)) g.FillRectangle(sb, e.Bounds);

                Color textCol;
                if (selected) textCol = Color.White;
                else if (e.SubItem.ForeColor != Color.Empty) textCol = e.SubItem.ForeColor;
                else textCol = _isDark ? Color.WhiteSmoke : Color.Black;

                using var brush = new SolidBrush(textCol);
                var r = e.Bounds;
                r.Inflate(-4, 0);
                var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
                g.DrawString(e.SubItem.Text, this.Font, brush, r, sf);
            }
            catch { }
        }

        private void Logger_OnLog(DateTime ts, LogLevel level, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendLog(ts, level, message)));
            }
            else
            {
                AppendLog(ts, level, message);
            }
        }

        private void AppendLog(DateTime ts, LogLevel level, string message)
        {
            var lvi = new ListViewItem(ts.ToString("yyyy-MM-dd HH:mm:ss"));
            lvi.SubItems.Add(level.ToString());
            lvi.SubItems.Add(message);
            // color by level
            switch (level)
            {
                case LogLevel.Debug:
                    lvi.ForeColor = Color.Gray;
                    break;
                case LogLevel.Info:
                    lvi.ForeColor = Color.Black;
                    break;
                case LogLevel.Warning:
                    lvi.ForeColor = Color.Orange;
                    break;
                case LogLevel.Error:
                    lvi.ForeColor = Color.Red;
                    break;
            }
            lvLogs.Items.Add(lvi);
            // keep bottom visible
            if (lvLogs.Items.Count > 0) lvLogs.EnsureVisible(lvLogs.Items.Count - 1);
            statusLabel.Text = $"Logs: {lvLogs.Items.Count}";
        }

        private void ClearLogsMenuItem_Click(object? sender, EventArgs e)
        {
            var res = MessageBox.Show("Effacer les logs sur disque et dans l'interface ?", "Confirmer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;
            try
            {
                Logger.Clear();
                lvLogs.Items.Clear();
                statusLabel.Text = "Logs effacés";
                MessageBox.Show("Logs effacés.", "Effacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'effacer les logs: " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportLogsMenuItem_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "Fichier log|*.log|Texte|*.txt|Tous|*.*";
            sfd.FileName = "windows-cleaner.log";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var dest = Logger.Export(sfd.FileName);
                if (dest != null)
                    MessageBox.Show($"Logs exportés vers {dest}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Échec de l'export des logs", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutMenuItem_Click(object? sender, EventArgs e)
        {
            // Author and MIT license (full text)
            var author = "Auteur : c.lecomte";
            var licenseTitle = "Licence : MIT";
            var licenseText = @"MIT License

Copyright (c) 2025 c.lecomte

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.";

            var msg = $"Windows Cleaner\n\n{author}\n\n{licenseTitle}\n\n{licenseText}\n\nVersion: 0.1";
            MessageBox.Show(msg, "À propos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Cancel()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                statusLabel.Text = "Annulation en cours...";
            }
        }

        private async Task StartCleanerAsync(bool dryRun)
        {
            btnClean.Enabled = false;
            btnDryRun.Enabled = false;
            btnCancel.Enabled = true;
            lvLogs.Items.Clear();
            progressBar.Value = 0;
            statusLabel.Text = dryRun ? "Dry run en cours..." : "Nettoyage en cours...";

            var options = new CleanerOptions
            {
                DryRun = dryRun,
                EmptyRecycleBin = chkRecycle.Checked,
                IncludeSystemTemp = chkSystemTemp.Checked,
                CleanChrome = chkChrome.Checked,
                CleanEdge = chkEdge.Checked,
                CleanWindowsUpdate = chkWindowsUpdate.Checked,
                CleanThumbnails = chkThumbnails.Checked,
                CleanPrefetch = chkPrefetch.Checked,
                FlushDns = chkFlushDns.Checked,
                Verbose = chkVerbose.Checked,
                // advanced option
            };

            // advanced mode: generate and show report before executing (unless dry-run)
            if (chkAdvanced.Checked && !dryRun)
            {
                statusLabel.Text = "Génération du rapport avancé...";
                var report = Cleaner.GenerateReport(options, s => Logger.Log(LogLevel.Debug, s));
                var summary = $"Éléments candidats: {report.Count}\nEspace estimé: {report.TotalBytes} octets";
                var details = new System.Text.StringBuilder();
                details.AppendLine(summary);
                details.AppendLine();
                foreach (var it in report.Items.Take(200)) // limit preview size
                {
                    details.AppendLine($"{(it.IsDirectory ? "[D]" : "[F]")} {it.Path} ({it.Size} octets)");
                }
                if (report.Count > 200) details.AppendLine($"... ({report.Count - 200} autres éléments non affichés)");

                var dlg = new Form() { Text = "Rapport avancé - aperçu", Width = 1000, Height = 700 };
                var panelTop = new Panel() { Dock = DockStyle.Top, Height = 34 };
                var lblFilter = new Label() { Text = "Filtre:", Left = 6, Top = 8, AutoSize = true };
                var txtFilter = new TextBox() { Left = 56, Top = 4, Width = 480 };
                panelTop.Controls.Add(lblFilter);
                panelTop.Controls.Add(txtFilter);

                var dgv = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
                var colType = new DataGridViewTextBoxColumn() { Name = "Type", HeaderText = "Type", DataPropertyName = "Type", Width = 50 };
                var colPath = new DataGridViewTextBoxColumn() { Name = "Path", HeaderText = "Path", DataPropertyName = "Path", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
                var colSize = new DataGridViewTextBoxColumn() { Name = "Size", HeaderText = "Size", DataPropertyName = "Size", Width = 120 };
                dgv.Columns.Add(colType);
                dgv.Columns.Add(colPath);
                dgv.Columns.Add(colSize);

                // Build DataTable for sortable view
                var dt = new System.Data.DataTable();
                dt.Columns.Add("Type", typeof(string));
                dt.Columns.Add("Path", typeof(string));
                dt.Columns.Add("Size", typeof(long));
                foreach (var it in report.Items)
                {
                    var row = dt.NewRow();
                    row["Type"] = it.IsDirectory ? "D" : "F";
                    row["Path"] = it.Path;
                    row["Size"] = it.Size;
                    dt.Rows.Add(row);
                }

                var dv = dt.DefaultView;

                // apply persisted sort if present
                var sett = SettingsManager.Load();
                if (!string.IsNullOrEmpty(sett.ReportSortColumn) && (sett.ReportSortDirection == "ASC" || sett.ReportSortDirection == "DESC"))
                {
                    dv.Sort = sett.ReportSortColumn + " " + (sett.ReportSortDirection == "ASC" ? "ASC" : "DESC");
                }

                var bs = new BindingSource();
                bs.DataSource = dv;
                dgv.DataSource = bs;

                txtFilter.TextChanged += (s, e) =>
                {
                    var f = txtFilter.Text ?? string.Empty;
                    dv.RowFilter = string.IsNullOrWhiteSpace(f) ? string.Empty : $"Path LIKE '%" + f.Replace("'", "''") + "%'";
                };

                var btnPanel = new Panel() { Dock = DockStyle.Bottom, Height = 44 };
                var btnProceed = new Button() { Text = "Continuer et exécuter", Left = 10, Width = 180, Top = 6, DialogResult = DialogResult.OK }; btnProceed.FlatStyle = FlatStyle.Flat;
                var btnCancelReport = new Button() { Text = "Annuler", Left = 200, Width = 100, Top = 6, DialogResult = DialogResult.Cancel }; btnCancelReport.FlatStyle = FlatStyle.Flat;
                var btnSave = new Button() { Text = "Enregistrer le rapport complet", Left = 310, Width = 220, Top = 6 }; btnSave.FlatStyle = FlatStyle.Flat;
                btnSave.Click += (s, e) =>
                {
                    using var sfd = new SaveFileDialog();
                    sfd.Filter = "CSV|*.csv|Texte|*.txt|Tous|*.*";
                    sfd.FileName = "rapport_avance.csv";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using var sw = new System.IO.StreamWriter(sfd.FileName);
                        sw.WriteLine("Type,Path,Size");
                        foreach (var it in report.Items)
                            sw.WriteLine($"{(it.IsDirectory ? "D" : "F")},\"{it.Path.Replace("\"", "\"\"")}\",{it.Size}");
                    }
                };
                btnPanel.Controls.Add(btnProceed); btnPanel.Controls.Add(btnCancelReport); btnPanel.Controls.Add(btnSave);
                dlg.Controls.Add(panelTop); dlg.Controls.Add(dgv); dlg.Controls.Add(btnPanel);
                var res = dlg.ShowDialog();
                // persist current sort
                try
                {
                    if (dgv.SortedColumn != null)
                    {
                        var col = dgv.SortedColumn.Name;
                        var dir = dgv.SortOrder == SortOrder.Ascending ? "ASC" : "DESC";
                        var s = SettingsManager.Load();
                        s.ReportSortColumn = col;
                        s.ReportSortDirection = dir;
                        SettingsManager.Save(s);
                    }
                }
                catch { }

                // double-click to open
                dgv.CellDoubleClick += (ss, ee) =>
                {
                    try
                    {
                        if (ee.RowIndex < 0) return;
                        var row = ((System.Data.DataRowView)((BindingSource)dgv.DataSource).Current)?.Row;
                        // safer approach: get value directly from dgv
                        var val = dgv.Rows[ee.RowIndex].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        if (System.IO.Directory.Exists(val))
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe", '"' + val + '"') { UseShellExecute = true });
                            Logger.Log(LogLevel.Info, $"Ouverture dossier: {val}");
                        }
                        else if (System.IO.File.Exists(val))
                        {
                            // select the file
                            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + val + "\"") { UseShellExecute = true });
                            Logger.Log(LogLevel.Info, $"Ouverture fichier: {val}");
                        }
                        else
                        {
                            MessageBox.Show($"Chemin introuvable: {val}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Impossible d'ouvrir l'élément: " + ex.Message);
                    }
                };

                // Context menu: Open / Copy path / Ignore
                var cms = new ContextMenuStrip();
                var openItem = new ToolStripMenuItem("Ouvrir dans l'Explorateur");
                var copyItem = new ToolStripMenuItem("Copier le chemin");
                var ignoreItem = new ToolStripMenuItem("Ignorer cet élément");
                cms.Items.Add(openItem);
                cms.Items.Add(copyItem);
                cms.Items.Add(new ToolStripSeparator());
                cms.Items.Add(ignoreItem);
                dgv.ContextMenuStrip = cms;

                // Ensure right-click selects the row
                dgv.CellMouseDown += (ss, ee) =>
                {
                    if (ee.Button == MouseButtons.Right && ee.RowIndex >= 0)
                    {
                        dgv.ClearSelection();
                        dgv.Rows[ee.RowIndex].Selected = true;
                        dgv.CurrentCell = dgv.Rows[ee.RowIndex].Cells[0];
                    }
                };

                openItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        if (System.IO.Directory.Exists(val))
                            Process.Start(new ProcessStartInfo("explorer.exe", '"' + val + '"') { UseShellExecute = true });
                        else if (System.IO.File.Exists(val))
                            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + val + "\"") { UseShellExecute = true });
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Ouvrir élément: " + ex.Message); }
                };

                copyItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        Clipboard.SetText(val);
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Copier chemin: " + ex.Message); }
                };

                ignoreItem.Click += (s3, e3) =>
                {
                    try
                    {
                        if (dgv.SelectedRows.Count == 0) return;
                        var val = dgv.SelectedRows[0].Cells["Path"].Value as string;
                        if (string.IsNullOrWhiteSpace(val)) return;
                        // remove from DataTable
                        var rows = dt.Select("Path = '" + val.Replace("'", "''") + "'");
                        foreach (var r in rows) r.Delete();
                        dt.AcceptChanges();
                        // remove from report items
                        report.Items.RemoveAll(it => string.Equals(it.Path, val, StringComparison.OrdinalIgnoreCase));
                    }
                    catch (Exception ex) { Logger.Log(LogLevel.Error, "Ignorer élément: " + ex.Message); }
                };
                // persist current sort
                try
                {
                    if (dgv.SortedColumn != null)
                    {
                        var col = dgv.SortedColumn.Name;
                        var dir = dgv.SortOrder == SortOrder.Ascending ? "ASC" : "DESC";
                        var s = SettingsManager.Load();
                        s.ReportSortColumn = col;
                        s.ReportSortDirection = dir;
                        SettingsManager.Save(s);
                    }
                }
                catch { }
                if (res != DialogResult.OK)
                {
                    Logger.Log(LogLevel.Warning, "Opération avancée annulée par l'utilisateur (rapport). ");
                    statusLabel.Text = "Annulé (rapport)";
                    btnClean.Enabled = true;
                    btnDryRun.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }
                // otherwise continue
                statusLabel.Text = "Exécution après rapport avancé...";
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                // If performing real cleaning, confirm dangerous options
                if (!dryRun)
                {
                    var dangers = new System.Text.StringBuilder();
                    if (options.IncludeSystemTemp) dangers.AppendLine("- Inclure Temp système");
                    if (options.CleanWindowsUpdate) dangers.AppendLine("- Nettoyer Windows Update (SoftwareDistribution\\Download)");
                    if (options.CleanPrefetch) dangers.AppendLine("- Nettoyer Prefetch");
                    if (options.EmptyRecycleBin) dangers.AppendLine("- Vider la Corbeille");

                    if (dangers.Length > 0)
                    {
                        var msg = "Vous êtes sur le point d'exécuter des opérations potentiellement dangereuses:\n\n" + dangers.ToString() + "\nContinuer ?";
                        var confirm = MessageBox.Show(msg, "Confirmer les opérations", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm != DialogResult.Yes)
                        {
                            Logger.Log(LogLevel.Warning, "Opération annulée par l'utilisateur (confirmation refusée)");
                            statusLabel.Text = "Annulé (confirmation)";
                            btnClean.Enabled = true;
                            btnDryRun.Enabled = true;
                            btnCancel.Enabled = false;
                            return;
                        }
                    }
                }

                Logger.Log(LogLevel.Info, $"Début du nettoyage ({(dryRun ? "dry-run" : "exécution")})...");

                // Run the cleaner in a Task and periodically update progress
                var task = Task.Run(() => Cleaner.RunCleanup(options, s => Logger.Log(LogLevel.Info, s)), token);

                while (!task.IsCompleted)
                {
                    if (token.IsCancellationRequested) break;
                    progressBar.Value = Math.Min(progressBar.Value + 5, 95);
                    await Task.Delay(200, token).ContinueWith(_ => { });
                }

                var result = await task;
                Logger.Log(LogLevel.Info, $"Terminé. Fichiers supprimés: {result.FilesDeleted}, Octets libérés: {result.BytesFreed}");
                statusLabel.Text = "Terminé";
                progressBar.Value = 100;
            }
            catch (OperationCanceledException)
            {
                Logger.Log(LogLevel.Warning, "Opération annulée par l'utilisateur.");
                statusLabel.Text = "Annulé";
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Erreur: " + ex.Message);
                statusLabel.Text = "Erreur";
            }
            finally
            {
                btnClean.Enabled = true;
                btnDryRun.Enabled = true;
                btnCancel.Enabled = false;
                _cts = null;
            }
        }
    }
}
