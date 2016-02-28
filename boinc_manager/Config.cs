// boinc_manager, config.cs

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

namespace boinc_manager
{
    public class Config
    {
        // ---- READ ONLY CONSTANTS ---------------------------------------------------------------

        public const string config_file              = "boinc_manager.ini";
        public const string log_file                 = "boinc_manager.log";
        public const string log_file_truncating      = "boinc_manager.log.old";

        // log size is expressed in mega-bytes
        public const long   default_maximum_log_size = 100;
        public const int    default_wait_for_minutes = 1;

        // name of the fields inside the .INI file
        public const string ini_wait_for_in_minutes  = "warten_in_minuten";
        public const string ini_binary               = "programm_zu_starten";
        public const string ini_binary_options       = "programmoptionen";
        public const string ini_restart_program      = "neu_starten_wenn_es_aufhört";
        public const string ini_create_log           = "erstellen_protokolldatei";
        public const string ini_limit_log_size       = "protokollgröße_begrenzen";
        public const string ini_maximum_log_size     = "maximale_größe_für_log";

        // ---- Internal properties ---------------------------------------------------------------

        public int          wait_for_minutes;
        public string       binary_path;
        public string       binary_options;
        public bool         restart_program_if_stops;
        public bool         log_actived;
        public bool         log_is_limited;
        public long         log_maximum_size;

        // ---- Constructor -----------------------------------------------------------------------

        public Config()
        {
            wait_for_minutes         = default_wait_for_minutes;

            binary_path              = string.Empty;
            binary_options           = string.Empty;

            restart_program_if_stops = true;
            log_actived              = false;
            log_is_limited           = true;

            log_maximum_size         = default_maximum_log_size;  // expressed in mega-bytes
        }
    }
}
