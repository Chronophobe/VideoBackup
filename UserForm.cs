using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace VideoBackup
{
    class UserForm : Form
    {
        FolderBrowserDialog fileDirDialog = new FolderBrowserDialog();
        FolderBrowserDialog backupDirDialog = new FolderBrowserDialog();

        Label fileDirLabel = new Label();
        Label backupDirLabel = new Label();

        TextBox fileDirBox = new TextBox();
        TextBox backupDirBox = new TextBox();

        Button fileDirBrowse = new Button();
        Button backupDirBrowse = new Button();

        Label fromTypeLabel = new Label();
        Label toTypeLabel = new Label();

        ComboBox fromTypeList = new ComboBox();
        ComboBox toTypeList = new ComboBox();

        CheckBox renameCheck = new CheckBox();

        Button previewButton = new Button();
        Label numberLabel = new Label();
        ListBox previewBox = new ListBox();
        ListBox resultBox = new ListBox();

        Button backupButton = new Button();
        Button pauseButton = new Button();
        ProgressBar progressBar = new ProgressBar();

        StatusBar status = new StatusBar();

        BackgroundWorker Peon = new BackgroundWorker();

        public UserForm()
        {
            this.Text = "Video Backup";
            this.ClientSize = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources.iconmonstr_video_camera_4_icon;
            setupControls();
            status.Text = "Ready";
        }

        private void setupControls()
        {
            MainMenu menu = new MainMenu();
            MenuItem helpButton = new MenuItem("Help");
            menu.MenuItems.Add(helpButton);

            fileDirLabel.Location = new Point(10, 10);
            fileDirLabel.Width = 230;
            fileDirLabel.Text = "Directory to backup from: ";

            fileDirBox.Location = new Point(fileDirLabel.Location.X, fileDirLabel.Location.Y + fileDirLabel.Height);
            fileDirBox.Width = fileDirLabel.Width;

            fileDirBrowse.Location = new Point(fileDirBox.Location.X + fileDirBox.Width + 10, fileDirBox.Location.Y - 1);
            fileDirBrowse.Text = "Browse...";

            backupDirLabel.Location = new Point(fileDirBox.Location.X, fileDirBox.Location.Y + fileDirBox.Height + 20);
            backupDirLabel.Width = fileDirLabel.Width;
            backupDirLabel.Text = "Directory to backup to: ";

            backupDirBox.Location = new Point(backupDirLabel.Location.X, backupDirLabel.Location.Y + backupDirLabel.Height);
            backupDirBox.Width = fileDirLabel.Width;

            backupDirBrowse.Location = new Point(backupDirBox.Location.X + backupDirBox.Width + 10, backupDirBox.Location.Y - 1);
            backupDirBrowse.Text = "Browse...";

            fromTypeLabel.Location = new Point(fileDirBrowse.Location.X + fileDirBrowse.Width + 40, fileDirLabel.Location.Y);
            fromTypeLabel.Text = "From file type: ";

            fromTypeList.DropDownStyle = ComboBoxStyle.DropDown;
            fromTypeList.Items.AddRange(new string[] { ".mod", ".tod" });
            fromTypeList.Location = new Point(fileDirBrowse.Location.X + fileDirBrowse.Width + 40, fileDirBrowse.Location.Y + 1);
            fromTypeList.Height = 50;
            fromTypeList.SelectedIndex = 0;

            toTypeLabel.Location = new Point(backupDirBrowse.Location.X + backupDirBrowse.Width + 40, backupDirLabel.Location.Y);
            toTypeLabel.Text = "To file type: ";

            toTypeList.DropDownStyle = fromTypeList.DropDownStyle;
            toTypeList.Items.AddRange(new string[] { ".mpg" });
            toTypeList.Location = new Point(backupDirBrowse.Location.X + backupDirBrowse.Width + 40, backupDirBrowse.Location.Y + 1);
            toTypeList.Height = fromTypeList.Height;
            toTypeList.SelectedIndex = 0;

            renameCheck.Location = new Point(backupDirBox.Location.X, backupDirBox.Location.Y + backupDirBox.Height + 10);
            renameCheck.Text = "Rename files to {date} {time}";
            renameCheck.Width = 230;
            renameCheck.Checked = true;

            previewButton.Location = new Point(renameCheck.Location.X, renameCheck.Location.Y + renameCheck.Height + 20);
            previewButton.Text = "Preview";

            numberLabel.Location = new Point(this.ClientSize.Width - numberLabel.Width - 10, previewButton.Location.Y);
            numberLabel.Text = "Files: 0";
            numberLabel.RightToLeft = RightToLeft.Yes;

            previewBox.Location = new Point(previewButton.Location.X, previewButton.Location.Y + previewButton.Height + 10);
            previewBox.Width = this.ClientSize.Width / 2 - 15;
            previewBox.Height = 200;
            previewBox.HorizontalScrollbar = true;

            resultBox.Location = new Point(previewBox.Location.X + previewBox.Width + 10, previewBox.Location.Y);
            resultBox.Width = previewBox.Width;
            resultBox.Height = previewBox.Height;
            resultBox.HorizontalScrollbar = true;

            backupButton.Location = new Point(previewBox.Location.X, previewBox.Location.Y + previewBox.Height + 10);
            backupButton.Text = "Backup";

            pauseButton.Location = new Point(backupButton.Location.X + backupButton.Width + 10, backupButton.Location.Y);
            pauseButton.Text = "Pause";
            pauseButton.Enabled = false;

            progressBar.Location = new Point(backupButton.Location.X, backupButton.Location.Y + backupButton.Height + 10);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.ForeColor = Color.FromArgb(118, 182, 0);
            progressBar.Width = this.ClientSize.Width - 20;

            this.Menu = menu;
            this.Controls.AddRange(new Control[]{

                fileDirLabel,
                fileDirBox, 
                fileDirBrowse,
                fromTypeLabel,
                fromTypeList,

                backupDirLabel,
                backupDirBox,
                backupDirBrowse,
                toTypeLabel,
                toTypeList,
                
                renameCheck,

                previewButton,
                numberLabel,
                previewBox,
                resultBox,

                backupButton,
                pauseButton,
                progressBar,
                status
            });

            helpButton.Click += displayHelp;
            
            fileDirBrowse.Click += browseFile;
            backupDirBrowse.Click += browseFile;

            previewButton.Click += preview;

            backupButton.Click += startBackup;

            previewBox.SelectedIndexChanged += (sender, ea) => {
                resultBox.SelectedIndex = previewBox.SelectedIndex;
            };

            resultBox.SelectedIndexChanged += (sender, ea) => {
                previewBox.SelectedIndex = resultBox.SelectedIndex;
            };

            pauseButton.Click += (s, ea) =>
            {
                Button sender = ((Button)s);
                if (sender.Text == "Pause")
                {
                    paused = true;
                    sender.Text = "Resume";
                    status.Text = progressBar.Value + "% - [Paused]";
                    this.Text = "Video Backup - Paused";
                    this.Cursor = Cursors.Default;
                }
                else
                {
                    paused = false;
                    sender.Text = "Pause";
                    status.Text = progressBar.Value + "%";
                    this.Text = "Video Backup - " + progressBar.Value + "%";
                    this.Cursor = Cursors.AppStarting;
                }
            };

            Peon.DoWork += this.backup;
            Peon.ProgressChanged += this.updateProgress;
            Peon.WorkerReportsProgress = true;
            Peon.WorkerSupportsCancellation = true;
            Peon.RunWorkerCompleted += (o, e) =>
            {
                this.Text = "Video Backup";
                enableForm(true);
            };
        }

        private void displayHelp(object sender, EventArgs ea)
        {
            MessageBox.Show("1. Browse to the directory of the video files.\n2. Browse to a directory to save the video files.\n(3.) Choose the extension of the files.\n(4.) Choose the extension to change to.\n(5.) Press preview to see the changes you are about to make.\n6. Press backup to start the process.", "Help");
        }

        private void browseFile(object sender, EventArgs ea)
        {
            FolderBrowserDialog dialog;
            TextBox input;
            if (sender == fileDirBrowse)
            {
                dialog = fileDirDialog;
                input = fileDirBox;
            }
            else
            {
                dialog = backupDirDialog;
                input = backupDirBox;
            }
            dialog.ShowDialog();
            input.Text = dialog.SelectedPath;
        }

        private void preview(object sender, EventArgs ea)
        {
            previewBox.Items.Clear();
            resultBox.Items.Clear();
            this.Cursor = Cursors.WaitCursor;
            if (!directoryExists(fileDirBox.Text) || !directoryExists(backupDirBox.Text))
            {
                return;
            }
            string from = fileDirBox.Text;
            string to = backupDirBox.Text;
            string fromType = fromTypeList.Text;
            string toType = toTypeList.Text;

            string[] files = this.getFiles(from, fromType);
            string[] results = new string[files.Length];
            for (int i = 0; i < results.Length; i++)
            {
                string name = Path.ChangeExtension(to + "\\" + Path.GetFileName(files[i]), toType);
                if (renameCheck.Checked)
                {
                    name = to + "\\" + this.buildName(File.GetLastWriteTime(files[i])) + toType;
                }
                results[i] = name;
            }
            previewBox.Items.AddRange(files);
            resultBox.Items.AddRange(results);
            progressBar.Value = 0;
            this.Cursor = Cursors.Default;
            this.progressBar.ForeColor = Color.FromArgb(118, 182, 0);
            this.status.Text = "Preview loaded";
            this.numberLabel.Text = "Files: " + files.Length;
        }

        private void updateProgress(object sender, ProgressChangedEventArgs ea)
        {
            this.progressBar.Value = ea.ProgressPercentage;
            this.status.Text = ea.ProgressPercentage + "%" + (paused ? " - [Paused]" : "");
            this.Text = "Video Backup - " + (paused ? "Paused" : ea.ProgressPercentage + "%");
            if (ea.ProgressPercentage == 100)
            {
                enableForm(true);
            }
        }

        private void startBackup(object sender, EventArgs ea)
        {
            if (((Button)sender).Text == "Backup")
            {
                enableForm(false);
                
                string from = fileDirBox.Text;
                string to = backupDirBox.Text;
                string fromType = fromTypeList.Text;
                string toType = toTypeList.Text;
                string[] files = getFiles(from, fromType);
                object[] data = new object[] { from, to, fromType, toType, files };

                progressBar.Value = 0;
                progressBar.Maximum = 100;

                Peon.RunWorkerAsync(data);
                this.progressBar.ForeColor = Color.FromArgb(118, 182, 0);
            }
            else if (((Button)sender).Text == "Stop")
            {
                Peon.CancelAsync();
                enableForm(true);
                status.Text = "Backup stopped. Not all files have been copied.";
                this.progressBar.ForeColor = Color.FromArgb(182, 0, 0);
            }
        }

        bool paused = false;
        private void backup(object sender, DoWorkEventArgs ea)
        {
            object[] data = (object[]) ea.Argument;
            if (!directoryExists(fileDirBox.Text) || !directoryExists(backupDirBox.Text))
            {
                return;
            }

            //params
            string from = (string)((object[]) data)[0];
            string to = (string)((object[])data)[1];
            string fromType = (string)((object[])data)[2];
            string toType = (string)((object[])data)[3];
            string[] files = (string[])((object[])data)[4];

            int duplicate = 0;
            int counter = 0;
            foreach (string file in files)
            {
                while (paused)
                {
                    if (Peon.CancellationPending)
                    {
                        return;
                    }
                }
                counter++;
                if (renameCheck.Checked)
                {
                    DateTime attrib = File.GetLastWriteTime(file);
                    string name = this.buildName(attrib);
                    try
                    {
                        File.Copy(file, Path.ChangeExtension(to + "\\" + name + Path.GetExtension(file), toType), false);
                        duplicate = 0;
                    }
                    catch (IOException e)
                    {
                        duplicate++;
                        File.Copy(file, Path.ChangeExtension(to + "\\" + name + "[" + duplicate + "]" + Path.GetExtension(file), toType), false);
                    }
                }
                else
                {
                    try
                    {
                        File.Copy(file, Path.ChangeExtension(to + "\\" + Path.GetFileName(file), toType));
                        duplicate = 0;
                    }
                    catch (IOException e)
                    {
                        duplicate++;
                        try
                        {
                            File.Copy(file, Path.ChangeExtension(to + "\\" + Path.GetFileNameWithoutExtension(file) + "[" + duplicate + "]" + Path.GetExtension(file), toType), false);
                        }
                        catch (IOException)
                        {
                            
                        }
                    }
                }
                if (Peon.CancellationPending)
                {
                    return;
                }
                else
                {
                    Peon.ReportProgress(100 * counter / files.Length);
                }
            }
        }

        private void enableForm(bool state)
        {
            if (state)
            {
                this.progressBar.Cursor = Cursors.Default;
                this.Cursor = Cursors.Default;
                this.backupButton.Text = "Backup";
                this.pauseButton.Text = "Pause";
            }
            else
            {
                this.Cursor = Cursors.AppStarting;
                this.progressBar.Cursor = Cursors.WaitCursor;
                this.backupButton.Text = "Stop";
            }
            this.previewButton.Enabled = state;
            this.pauseButton.Enabled = !state;
        }

        private bool directoryExists(string dir)
        {
            bool existance = Directory.Exists(dir);
            if (existance)
            {
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(dir))
                {
                    status.Text = "Directory not specified.";
                }
                else
                {
                    status.Text = "Directory does not exist: " + dir + ".";
                }
                return false;
            }
        }

        private string[] getFiles(string from, string ext)
        {
            try
            {
                List<string> files = new List<string>(Directory.GetFiles(from, "?*" + ext, SearchOption.AllDirectories));
                files.Sort(filenameCompare);
                return files.ToArray();
            }
            catch (UnauthorizedAccessException e)
            {
                status.Text = e.Message;
                return new string[0];
                
            }
        }

        private string buildName(DateTime d)
        {
            string name = "";
            name += d.Year + "-";
            name += d.Month + "-";
            name += d.Day + " ";
            name += d.Hour + "h";
            name += d.Minute + "m";
            name += d.Second + "s";
            return name;
        }

        private int filenameCompare(string a, string b)
        {
            string[] namepartsA = a.Split('\\');
            string[] namepartsB = b.Split('\\');
            string filenameA = namepartsA[namepartsA.Length - 1];
            string filenameB = namepartsB[namepartsB.Length - 1];

            return filenameA.CompareTo(filenameB);
        }
    }
}
