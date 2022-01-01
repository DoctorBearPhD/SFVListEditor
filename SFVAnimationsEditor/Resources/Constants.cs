using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVAnimationsEditor.Resources
{
    // TODO: Move all viable constants to the Constants file

    /// <summary>
    /// Resource file for storing public constants. Not all constants may be stored here, as this was added later in the project.
    /// </summary>
    public static class Constants
    {
        public const string REQUEST_DIALOG = "REQ_DIA";
        
        public const string REQUESTTYPE_FOLDER = "REQT_F";

        public const string RESPONSETYPE_FOLDERSELECTION = "REST_FS";

        public const string DISPLAY_MESSAGE = "DISP_MSG";
        public const string DISPLAY_ERROR   = "DISP_ERR";

        public const string TITLE_FOLDERSELECTION = "Select a folder...";


        public const string WARNING_SAVE_ALL =
            @"ONLY USE THIS IF YOU KNOW WHAT YOU ARE DOING!

This will copy the COMMON_OBJECT entries to EVERY character in the directory of your choosing.

A backup folder will be created alongside the chosen directory. If the backup folder already exists, it will be overwritten!";

    }
}
