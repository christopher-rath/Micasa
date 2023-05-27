#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2023 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Micasa
{
    /// <summary>
    ///     This static class holds the application level options that are configured by the user 
    ///     from the Options panel (Tools-->Options).
    /// </summary>
    public sealed class Options
    {
        public static readonly string HomeFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly string iniFileNm = HomeFolder + Path.DirectorySeparatorChar + Constants. sAppIniFileNm;
        private bool _FileTypeAvi = false;
        private bool _FileTypeBmp = true;
        private bool _FileTypeGif = true;
        private bool _FileTypeJpg = true;
        private bool _FileTypeMov = false;
        private bool _FileTypeNef = false;
        private bool _FileTypePng = true;
        private bool _FileTypePsd = false;
        private bool _FileTypeTga = false;
        private bool _FileTypeTif = true;
        private bool _FileTypeWebp = false;       
        private AppMode _MyAppMode = AppMode.Migrate;
        private bool _UpdPhotoFiles = false;
        private IniFile iniFile = new(iniFileNm);
        public  AppMode DefaultAppMode = AppMode.Migrate;
        // The single instance of Options.
        public static Options Instance = new();

        /// <summary>
        /// This Options class uses private constructor to implement the Singleton design pattern.  
        /// The single instance of this class is referenced via Options.Instance.
        /// </summary>
        private Options() 
        {
            _FileTypeAvi = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Avi, _FileTypeAvi);
            _FileTypeBmp = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Bmp, _FileTypeBmp);
            _FileTypeGif = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Gif, _FileTypeGif);
            _FileTypeJpg = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Jpg, _FileTypeJpg);
            _FileTypeMov = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Mov, _FileTypeMov);
            _FileTypeNef = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Nef, _FileTypeNef);
            _FileTypePng = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Png, _FileTypePng);
            _FileTypePsd = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Psd, _FileTypePsd);
            _FileTypeTga = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Tga, _FileTypeTga);
            _FileTypeTif = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Tif, _FileTypeTif);
            _FileTypeWebp = iniFile.GetBool(Constants.sMcFT_Section, Constants.sMcFT_Webp, _FileTypeWebp);
            // If the conversion to AppMode fails then we use the defaut value.
            if(!Enum.TryParse<AppMode>(iniFile.GetString(Constants.sMcScMicasa, Constants.sMcOpAppMode, _MyAppMode.ToString()), true, out _MyAppMode))
            {
                _MyAppMode = DefaultAppMode;
            }
            _UpdPhotoFiles = iniFile.GetBool(Constants.sMcScMicasa, Constants.sMcUpdPhotoFiles, _UpdPhotoFiles);
        }

        #region GetterSetters
        public bool FileTypeAvi
        {
            get => _FileTypeAvi;
            set
            {
                _FileTypeAvi = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Avi, _FileTypeAvi);
            }
        }

        public bool FileTypeBmp
        {
            get => _FileTypeBmp; 
            set
            {
                _FileTypeBmp = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Bmp, _FileTypeBmp);
            }
        }

        public bool FileTypeGif
        {
            get => _FileTypeGif;
            set
            {
                _FileTypeGif = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Gif, _FileTypeGif);
            }
        }

        public bool FileTypeJpg
        {
            get => _FileTypeJpg;
            set
            {
                _FileTypeJpg = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Jpg, _FileTypeJpg);
            }
        }

        public bool FileTypeMov
        {
            get => _FileTypeMov;
            set
            {
                _FileTypeMov = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Mov, _FileTypeMov);
            }
        }

        public bool FileTypeNef
        {
            get => _FileTypeNef;
            set
            {
                _FileTypeNef = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Nef, _FileTypeNef);
            }
        }

        public bool FileTypePng
        {
            get => _FileTypePng;
            set
            {
                _FileTypePng = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Png, _FileTypePng);
            }
        }

        public bool FileTypePsd
        {
            get => _FileTypePsd;
            set
            {
                _FileTypePsd = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Psd, _FileTypePsd);
            }
        }

        public bool FileTypeTga
        {
            get => _FileTypeTga;
            set
            {
                _FileTypeTga = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Tga, _FileTypeTga);
            }
        }

        public bool FileTypeTif
        {
            get => _FileTypeTif;
            set
            {
                _FileTypeTif = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Tif, _FileTypeTif);
            }
        }

        public bool FileTypeWebp
        {
            get => _FileTypeWebp;
            set
            {
                _FileTypeWebp = value;
                iniFile.SetBool(Constants.sMcFT_Section, Constants.sMcFT_Webp, _FileTypeWebp);
            }
        }

        public AppMode MyAppMode
        {
            get => _MyAppMode;
            set
            {
                _MyAppMode = value;
                iniFile.SetString(Constants.sMcScMicasa, Constants.sMcOpAppMode, _MyAppMode.ToString());
            }
        }

        public bool UpdatePhotoFiles
        {
            get => _UpdPhotoFiles;
            set
            {
                _UpdPhotoFiles = value;
                iniFile.SetBool(Constants.sMcScMicasa, Constants.sMcUpdPhotoFiles, _UpdPhotoFiles);
            }
        }
        #endregion GetterSetters

        /// <summary>
        /// Examine a file extension and determine if the user has configured
        /// Micasa to scan this file type.  Note that we mask any errors by 
        /// simply returning a value of false when an error occurs (e.g., the
        /// caller passes in an empty string).
        /// </summary>
        /// <param name="filename">A filename, including the extension.</param>
        /// <returns>True if the file is to be scanned; otherwise False.</returns>
        public bool IsFileTypeToScan(string filename)
        {
            string Extension;
            try
            {
                Extension = Path.GetExtension(filename).ToLower();
                switch (Extension)
                {
                    case Constants.sMcFT_Avi:
                        return FileTypeAvi;
                    case Constants.sMcFT_Bmp:
                        return FileTypeBmp;
                    case Constants.sMcFT_Gif:
                        return FileTypeGif;
                    case Constants.sMcFT_Jpg:
                    case Constants.sMcFT_JpgA:
                        return FileTypeJpg;
                    case Constants.sMcFT_Mov:
                        return FileTypeMov;
                    case Constants.sMcFT_Nef:
                        return FileTypeNef;
                    case Constants.sMcFT_Png:
                        return FileTypePng;
                    case Constants.sMcFT_Psd:
                        return FileTypePsd;
                    case Constants.sMcFT_Tga:
                        return FileTypeTga;
                    case Constants.sMcFT_Tif:
                    case Constants.sMcFT_TifA:
                        return FileTypeTif;
                    case Constants.sMcFT_Webp:
                        return FileTypeWebp;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
