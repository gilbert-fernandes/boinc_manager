# boinc_manager
A tool to keep BOINC running and delay-start its launch on a huge number of machines

This tool was written for a German user, so while most comments in code are in english, the fields
inside the .INI file are in German, as are error messages displayed and the interface itself.

What this program tries to solve ?

The user that uses BOINC has a set of machines. When they boot up (being woken by ethernet wake packets)
they all start BOINC which tries to fetch stuff from the network in order to start up and work properly.
Problem is network is saturated when a lot of BOINC instances are being launched at the same time.
As network fails to give data back, those BOINC instances fail to launch...

This program has two uses :
- introduce a delay in start of a program
- if asked, keep that program launched.

When you launch the program, it first checks if a config file has been created in the folder it is
launched from. If not, it shows a window and asks for configuration. This can also be provoked
by launching the program with the -config argument at launch.

First parameter you can set (Nach dem Start warten) is the number of minutes to wait once launched
before doing anything. By setting value to 0 to a few machines, and 2 to another set, and so on,
you can create groups of machines where BOINC will be loaded (BOINC or any other program) after
a time delay has elapsed. This is very basic but simple to use.

You give 5 machines a 0 second delay, another 5 a 2 minute delay and so on. You boot them all
up, boinc_manager is loaded at start of Windows and after the time has elapsed, program is
launched. Since config is stored in a simple text .INI file, you can thru network deposit new
config files.

The two options that follow in the interface are "dann starten" to select the program to start,
and "mit Optionen" are the launching arguments. Nothing special here, you can start whatever you
want.

The interface then offers 3 options :
- Starten Start-Programm, wenn es stoppt (es sei denn, Windows wird heruntergefahren)
- Erstellen Sie eine Protokolldatei
- Begrenzen Sie die Protokollgröße 

The first one asks boinc_manager to keep the program alive. If not checked, it will wait for
the time asked to wait for, launch the program and if it's running, it will quit. If the program
failed to launch, it waits a minute and tries again, forever, until it launches.

If boinc_manager was asked to keep the program alive, it stays in memory and sleeps. Every
minute it wakes up, and checks if the PID of the launched program is still running. If so,
sleeps again. Otherwise, it start the program again (looping by 1 minute increments until it
really starts).

Second of those options asks for the creation of a log file, boinc_manager.log, that will
store what is happening after the configuration is done. Here is a sample :

--
2/28/2016 3:58 AM config-Datei gefunden, minimiert Anlaß
2/28/2016 3:58 AM Abschuss C:\Program Files\Notepad++\notepad++.exe 
2/28/2016 3:59 AM Prozess-ID für Prozess ist 2248

2/28/2016 4:01 AM config-Datei gefunden, minimiert Anlaß
2/28/2016 4:01 AM Abschuss C:\Program Files\Notepad++\notepad++.exe 
2/28/2016 4:01 AM Prozess-ID für Prozess ist 2552
2/28/2016 4:02 AM Prozess wurde beendet! Neubelebung Prozess in einer Minute
2/28/2016 4:02 AM Abschuss C:\Program Files\Notepad++\notepad++.exe 
--

Each blank line represents a separate start of the program. Date followed by time at the
beginning of each line, and it tells it start in background, which program it is trying
to launch with which options, and the Process ID of the program once it started.
If it finds the program has closed and it was asked to keep it alive, you get the message
"Prozess wurde beendet!" and it tries to restart it after 1 minute.

The last option is used to keep the log under a fixed size. It is expressed in MiB
and to have code be very simple, once the log reaches the maximum allowed size
(checked each time we put a line in the log) it removes the first half of the log,
and keeps the later half. Much faster than just reducing for the amount required,
and be forced to do it again and again on each log commit : cut the older half,
and do next commits with less work for some time.

The .INI file has the following items :

--
[boinc_manager]
warten_in_minuten=0
programm_zu_starten=C:\Program Files\Notepad++\notepad++.exe
programmoptionen=
neu_starten_wenn_es_aufhört=true
erstellen_protokolldatei=true
protokollgröße_begrenzen=true
maximale_größe_für_log=100
--

If you do not understand German, the Config.cs file contains the internal fields used
which are in english.

Config.cs contains both constants I use (referenced as Config.XXX in form_main.cs)
and the program parameters which are loaded while the Form loads, before it is displayed.
Object creation gives those parameters default values. When checking for a parameter
I use the object created through my_config.XXX (my_config being a Config object).

No method, besises the Constructor that puts default values to parameters, and no
getter/setters, just simple public free to edit internal variables (KISS).

INIFile.cs contains an import of two functions from the kernel32.dll :
- static extern long WritePrivateProfileString
- static extern int GetPrivateProfileString

It lets you read, write or delete items from the INI file which gives users a easy
way to check or configure the program, and more easily modified than XML files.

form_main.cs contains all the code :
onLoad -> check if launched with -config, check for config presence otherwise
either display config window, load the configuration and let execution pass to formShown.

formShown -> this is the core of the program.
The status of the window (minimized or not) acts as a flag to let me know if we are
asking for configuration (form not minimized) or working with a config (minimized).
I use an infinite loop (based on a bool i set to false to break it rather than use
a Goto) and program does either a single launch and quits, or keeps the running program
active.

Next versions might evolve with a more server-client approach with a service running
on the background on the client machines, so it start as windows does, and a server
side to control the client machines and check their status.

Lack of time made me start with a local, manually set, program that is very basic
and bare-bones.

This is BSD "new" license code.
