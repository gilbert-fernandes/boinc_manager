// boinc_manager, form_main.cs

/*
 * boinc_manager
 * 
 * Copyright (c) 2016 Gilbert Fernandes <gilbert.fernandes@orange.fr>
 * All rights reserved
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY GILBERT FERNANDES AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR CONTRIBUTORS
 * BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 */

// ---- Using -------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace boinc_manager
{
    public partial class form_main : Form
    {
        // ---- Properties ------------------------------------------------------------------------

        Config       my_config;
        Process      p                  = null;
        StreamWriter logfile_writer     = null;

        // ---- Constructor -----------------------------------------------------------------------

        public form_main()
        {
            InitializeComponent();

            my_config = new Config();
        }

        // ---- When program loads ----------------------------------------------------------------

        /*
         * First, we check if we have been launched with the -config option.
         * 
         * The window will appear and ask for settings if :
         * 
         *    if -config option is present
         * or if no config file is present
         * 
         */

        private void form_main_Load(object sender, EventArgs e)
        {
            string[] args          = Environment.GetCommandLineArgs();
            string config_location = Path.Combine(Application.StartupPath, Config.config_file);

            // -config asked ?

            foreach (string arg in args)
                if (arg.Contains("-config"))
                {
                    this.WindowState = FormWindowState.Normal;
                    return;
                }

            // config file present ?

            if (!File.Exists(config_location))
            {
                this.WindowState = FormWindowState.Normal; // ask for config
            }
            else // yes -> read contents
            {
                IniFile ini           = null;
                string read_parameter = string.Empty;
                int some_int          = 1;
                long some_long        = 0L;

                try
                {
                    ini = new IniFile(config_location);
                }
                catch (Exception excep)
                {
                    MessageBox.Show("Der folgende Fehler passiert, wenn sie versuchen, die Konfigurationsdatei zu lesen:\n\n"
                        + excep.Message,
                        "Fehler beim Lesen der Datei",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Application.Exit();
                }

                // ---- Read INI file settings ----------------------------------------------------

                // ---- wait for X minutes after launch

                try
                {
                    read_parameter = ini.Read(Config.ini_wait_for_in_minutes);
                }
                catch
                {
                }

                if (!int.TryParse(read_parameter, out some_int))
                    my_config.wait_for_minutes = Config.default_wait_for_minutes;
                else
                    my_config.wait_for_minutes = some_int;

                // ---- binary to launch

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_binary);
                }
                catch
                {
                }

                my_config.binary_path = read_parameter;

                // ---- options to use

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_binary_options);
                }
                catch
                {
                }

                my_config.binary_options = read_parameter;

                // ---- restart program if it stops ?

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_restart_program);
                }
                catch
                {
                }

                read_parameter = read_parameter.ToLower();

                switch (read_parameter)
                {
                    case "true" :
                        my_config.restart_program_if_stops = true;
                        break;

                    case "false" :
                        my_config.restart_program_if_stops = false;
                        break;

                    default :
                        my_config.restart_program_if_stops = true;
                        break;
                }

                // ---- create log ?

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_create_log);
                }
                catch
                {
                }

                read_parameter = read_parameter.ToLower();

                switch (read_parameter)
                {
                    case "true":
                        my_config.log_actived = true;
                        break;

                    case "false":
                        my_config.log_actived = false;
                        break;

                    default:
                        my_config.log_actived = false; // no log by default
                        break;
                }

                // ---- limit log size ?

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_limit_log_size);
                }
                catch
                {
                }

                read_parameter = read_parameter.ToLower();

                switch (read_parameter)
                {
                    case "true":
                        my_config.log_is_limited = true;
                        break;

                    case "false":
                        my_config.log_is_limited = false;
                        break;

                    default:
                        my_config.log_is_limited = true;
                        break;
                }

                // ---- if log is limited, to what size ?

                read_parameter = string.Empty;

                try
                {
                    read_parameter = ini.Read(Config.ini_maximum_log_size);
                }
                catch
                {
                }

                if (!long.TryParse(read_parameter, out some_long))
                    my_config.log_maximum_size = Config.default_maximum_log_size;
                else
                    my_config.log_maximum_size = some_long;
            }
        }

        // ---- Program init is done and we are ready to start ------------------------------------

        private void form_main_Shown(object sender, EventArgs e)
        {
            // if asked to log, either open or create log file

            if (my_config.log_actived == true)
            {
                string log_location = Path.Combine(Application.StartupPath, Config.log_file);

                // we open the log file to write into. if it does not exist, the file is created
                // if it does exist, it is opened with write access
                // if this fails, we do not log anything even if asked for

                try
                {
                    // we create a new file if it does not exist, or append to it

                    FileStream fs  = File.Open(log_location, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fs.Close();

                    logfile_writer = new StreamWriter(log_location, true);
                }
                catch
                {
                    logfile_writer = null;
                }
            }

            // output an empty line to log as marker between launches

            log("");

            if (WindowState == FormWindowState.Minimized)
            {
                log("config-Datei gefunden, minimiert Anlaß");

                int  processus_id = -1;   // -1 means not running
                bool looping      = true; // used to break the infinite loop

                // if we have to wait, let's sleep on value

                if (my_config.wait_for_minutes != 0)
                    Thread.Sleep(my_config.wait_for_minutes * 60 * 1000); // wait for X minutes

                // we now loop : every minute, we check for the process

                do
                {
                    // we start the process, the first time
                    // if we are looping, we only try if we failed the first/previous time

                    log("Abschuss " + my_config.binary_path + " " + my_config.binary_options);

                    if (processus_id == -1)
                        processus_id = launch_the_program(my_config.binary_path, my_config.binary_options);

                    // 1 minute sleep unless no keep alive asked

                    if(my_config.restart_program_if_stops == true)
                        Thread.Sleep(60 * 1000);

                    // if the process is not running and we were asked to keep it alive,
                    // we loop

                    if (processus_id == -1)
                        if (my_config.restart_program_if_stops == true)
                        {
                            log("Start fehlgeschlagen ist, wieder in einer Minute versuchen");
                            continue;
                        }

                    // here, the processus was launched. are we asked to keep it alive ?
                    // if not, we quit

                    if (my_config.restart_program_if_stops == false)
                    {
                        looping = false;
                        log("starten Erfolg, nicht am Leben erhalten gefragt -> Verlassen");
                        Application.Exit();
                    }
                    else
                    {
                        // here, the processus is active and we were asked to keep it alive
                        // so we refresh the process and check if it's still running

                        if (p != null) // sanity check
                        {
                            p.Refresh();

                            try
                            {
                                if (p.HasExited)
                                {
                                    // the processus has exited : we loop, setting processus_id to -1
                                    // so we try to start it again

                                    log("Prozess wurde beendet! Neubelebung Prozess in einer Minute");
                                    processus_id = -1;
                                    continue;
                                }
                            }
                            catch
                            {
                                // if any exception occurs trying to find the status of the processus,
                                // we try to launch it again by looping

                                log("nicht in der Lage Prozessstatus zu lesen: wieder in einer Minute versuchen");
                                processus_id = -1;
                                continue;
                            }
                        }
                    }
                }
                while (looping == true);
            }
        }

        // ---- Log supplied line to log, if active -----------------------------------------------

        void log(string line)
        {
            // no log file set or it has been invalidated after an error

            if (logfile_writer == null)
                return;

            try
            {
                if(String.IsNullOrEmpty(line))
                    logfile_writer.WriteLine(); // empty line without date/time
                else
                    logfile_writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + line);

                logfile_writer.Flush(); // make sure line gets in file as soon we have to log it
            }
            catch
            {
                // on any error, invalidate the log file
                logfile_writer = null;
                return;
            }

            // have we reached the maximum log file size ?

            // TODO : move this code to an exiting routine to call it less often than each log write ?

            FileInfo log_fileinfo = null;

            try
            {
                log_fileinfo = new FileInfo(Path.Combine(Application.StartupPath, Config.log_file));
            }
            catch
            {
                return;
            }

            long log_size = log_fileinfo.Length; // find file size in bytes

            if (log_size > (my_config.log_maximum_size * 1024 * 1024))
            {
                string some_line;

                logfile_writer.Close();
                logfile_writer = null; // from now on, if we catch an exception, we return
                                       // and the log file is invalidated

                string previous_location = Path.Combine(Application.StartupPath, Config.log_file_truncating);
                string current_location  = Path.Combine(Application.StartupPath, Config.log_file);

                try
                {
                    File.Move(current_location, previous_location);
                }
                catch
                {
                    return;
                }

                // calculate lines in previous log file divided by 2

                long half_line_count;

                try
                {
                    half_line_count = (File.ReadLines(previous_location).Count()) / 2; // God bless LINQ !
                }
                catch
                {
                    return;
                }

                // we open the previous file, seek to half of it
                // and copy contents to new file

                StreamReader log_reader = null;
                StreamWriter log_writer = null;

                try
                {
                    log_reader = new StreamReader(previous_location);
                    log_writer = new StreamWriter(current_location);
                }
                catch
                {
                    return;
                }

                // we skip half of the previous file lines

                try
                {
                    for (long skip = 0; skip < half_line_count; skip++)
                        log_reader.ReadLine();
                }
                catch
                {
                    return;
                }

                // and we copy the rest to the new file

                try
                {
                    while ((some_line = log_reader.ReadLine()) != null)
                    {
                        some_line = log_reader.ReadLine();
                        log_writer.WriteLine(some_line);
                    }
                }
                catch
                {
                    return;
                }

                try
                {
                    log_reader.Close();
                    log_writer.Close();
                }
                catch
                {
                    // the OS will close them if we cant now
                }

                // and we set the handle to the log file again

                try
                {
                    logfile_writer = new StreamWriter(current_location, true, System.Text.Encoding.UTF8);
                }
                catch
                {
                    logfile_writer = null;
                }
            }
        }

        // ---- Launch specified program with arguments, send back PID if any or -1 ---------------

        private int launch_the_program(string path, string arguments)
        {
            int process_id;

            p = new Process();

            p.StartInfo.FileName  = my_config.binary_path;
            p.StartInfo.Arguments = my_config.binary_options;

            p.Start();

            // we wait 5 seconds for the process to start unless no keep alive asked

            if(my_config.restart_program_if_stops == true)
                Thread.Sleep(5 * 1000);

            // has program started ?

            try
            {
                process_id = p.Id;
                log("Prozess-ID für Prozess ist " + process_id);
            }
            catch
            {
                return -1;
            }

            return process_id;
        }

        // --- Cancel button ----------------------------------------------------------------------

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ---- Save button -----------------------------------------------------------------------

        private void button4_Click(object sender, EventArgs e)
        {
            // grab config from window

            my_config.wait_for_minutes = Decimal.ToInt16(numericUpDown1.Value);
            my_config.binary_path      = textBox1.Text;
            my_config.binary_options   = textBox2.Text;

            if (checkBox1.Checked == true)                           // restart program ?
                my_config.restart_program_if_stops = true;
            else
                my_config.restart_program_if_stops = false;

            if (checkBox2.Checked == true)                           // create log ?
                my_config.log_actived = true;
            else
                my_config.log_actived = false;

            if (checkBox3.Checked == true)                           // log is size limited ?
                my_config.log_is_limited = true;
            else
                my_config.log_is_limited = false;

            my_config.log_maximum_size = Decimal.ToInt16(numericUpDown2.Value);

            // create a .ini file

            IniFile ini            = null;
            string config_location = Path.Combine(Application.StartupPath, Config.config_file);

            try
            {
                ini = new IniFile(config_location);
            }
            catch (Exception excep)
            {
                MessageBox.Show("Folgender Fehler ist aufgetreten versuchen, die Konfigurationsdatei zu erstellen:\n\n"
                    + excep.Message,
                    "Fehler beim Speichern der Konfiguration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                Application.Exit();
            }

            // save settings to .ini file

            try
            {
                ini.Write(Config.ini_wait_for_in_minutes, my_config.wait_for_minutes.ToString());
                ini.Write(Config.ini_binary,              my_config.binary_path);
                ini.Write(Config.ini_binary_options,      my_config.binary_options);
                ini.Write(Config.ini_restart_program,     my_config.restart_program_if_stops.ToString().ToLower());
                ini.Write(Config.ini_create_log,          my_config.log_actived.ToString().ToLower());
                ini.Write(Config.ini_limit_log_size,      my_config.log_is_limited.ToString().ToLower());
                ini.Write(Config.ini_maximum_log_size,    my_config.log_maximum_size.ToString());
            }
            catch (Exception excep)
            {
                MessageBox.Show("der folgende Fehler aufgetreten ist, die Parameter zu speichern:\n\n"
                    + excep.Message,
                    "Fehler beim Speichern der Parameter",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                Application.Exit();
            }

            Application.Exit();
        }

        // ---- Ask for binary file to execute ----------------------------------------------------

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result;
            OpenFileDialog open_file  = new OpenFileDialog();

            open_file.CheckFileExists = true;
            open_file.CheckPathExists = true;
            open_file.Multiselect     = false;
            open_file.Filter          = "Ausführbare Datei (*.exe,com)|*.exe;*.com|Alle Dateien (*.*)|*.*";

            result = open_file.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text         = open_file.FileName;
                my_config.binary_path = open_file.FileName;
            }
        }

        // ---- Program is exiting ----------------------------------------------------------------

        private void form_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (logfile_writer != null)
                logfile_writer.Close();
        }
    }
}

// ------------------------------------------------------------------------------------------------
