#region Copyright
// Micasa -- Your Photo Home -- A lightweight photo organiser & editor.
// Author: Christopher Rath <christopher@rath.ca>
// Archived at: http://rath.ca/
// Copyright 2021-2025 © Christopher Rath
// Distributed under the GNU Lesser General Public License v2.1
//     (see the About–→Terms menu item for the license text).
// Warranty: None, see the license.
#endregion
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ExifLibrary;
using LiteDB;

namespace Micasa
{
    /// <summary>
    /// All of the metadata-related methods for Miscasa are contained in this class.  Ideally,
    /// none of the rest of the code in Micasa needs to know how the metadata is stored or 
    /// retrieved.  That detail is masked by the methods in this class.
    /// 
    /// For the ExifLibrary documentation, see: https://oozcitak.github.io/exiflibrary/index.html
    /// </summary>
    internal class Metadata
    {
        private static string _imgFl;
        private static FileStream _imgFs;
        private static BitmapSource _img;
        private static BitmapMetadata _mdProp;
        private static FileStream _mdFs;
        private static BitmapDecoder _mdBmDc;
        private static BitmapMetadata _mdBmMd;

        public Metadata(string imgFl)
        {
            // Initialize the image file path.
            _imgFl = imgFl;

            // Initialize the metadata access variables.
            try
            {
                _imgFs = File.OpenRead(imgFl);
                _img = BitmapFrame.Create(_imgFs);
                _mdProp = (BitmapMetadata)_img.Metadata;
                _mdFs = new FileStream(_imgFl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _mdBmDc = BitmapDecoder.Create(_mdFs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                _mdBmMd = (BitmapMetadata)_mdBmDc.Frames[0].Metadata;
            }
            catch
            {
                MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Metadata: Error opening image file: {0}", imgFl),
                    "Micasa Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _mdProp = null;
            }
        }

        /// <summary>
        /// Examine the image filename to determine if it is an image type supported by the
        /// this Metadata class.  
        /// 
        /// As of August 2025, the WPF BitmapMetadata class has codecs for the following 
        /// image types; however, at this time this Metadata class only supports a subset 
        /// of them (those that support EXIF):
        ///  * JPEG (.jpg) – Supports EXIF, IPTC, and XMP metadata.
        ///  * TIFF (.tif) – Supports IFD, EXIF, IPTC, and XMP metadata.
        ///  * PNG (.png) – Supports tEXt(PNG textual data) and XMP metadata.
        ///  * GIF (.gif) – Limited metadata support, but some metadata can be accessed.
        ///  * BMP (.bmp) – Minimal metadata support; not commonly used for metadata.
        ///  * Windows Media Photo (.wdp) – Supports XMP metadata.
        ///  * ICO (.ico) – Typically does not support metadata querying.
        ///  
        /// TO DO: add the .wdp and .ico image types to the list of types supported by Micasa
        ///        (i.e., update the list in the Options panel).
        /// </summary>
        /// <param name="imgFilename">The filename of the image to be checked.</param>
        /// <returns></returns>        
        private static bool MiMdSupportedImg(string imgFilename)
        {
            if (string.IsNullOrEmpty(imgFilename))
            {
                return false;
            }
            else
            {
                bool _supportedImg 
                    = Path.GetExtension(imgFilename).ToLower(CultureInfo.InvariantCulture) switch
                {
                    // Test for the image types that the GetMetadataValue method has been coded to handle.
                    Constants.sMcFT_Jpg or Constants.sMcFT_JpgA or Constants.sMcFT_Tif 
                        or Constants.sMcFT_TifA => true,
                    _ => false,
                };
                return _supportedImg;
            }
        }

        public static string GetSensitivityTypeDescription(ushort sensitivityType)
        {
            return sensitivityType switch
            {
                0 => "Unknown",
                1 => "ISO Speed",
                2 => "Recommended Exposure Index (REI)",
                3 => "ISO Speed and Recommended Exposure Index (REI)",
                4 => "ISO Speed Latitude (TTL)",
                5 => "ISO Speed Latitude (Scene)",
                _ => "Undefined"
            };
        }

        /// <summary>
        /// Get the Caption from the image file.  This method is agnostic regarding the formatting
        /// of the caption string; that is, any RTF, HTML, or other markup is simply retrieved 
        /// from the image and returned by the method in its raw form.
        /// 
        /// If any error occurs, this method will silently return an empty string.
        /// 
        /// TO DO: get the caption from the .micasa file & reconcile the two sources based on timestamps; 
        /// then return the most current caption.
        /// </summary>
        /// <param name="imgFl">Fully-qualified filename.</param>
        /// <returns>A string.</returns>
        private static string GetCaptionFromImage()
        {
            string caption = "";

            if (MiMdSupportedImg(_imgFl))
            {
                try
                {
                    if (_mdProp != null)
                    {
                        caption = _mdProp.Title;
                        caption ??= ""; // ??= means if 'caption' is null then do the assignment.
                    }
                }
                catch
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                            "GetCaptionFromImage ({0}): Unknown exception; returning empty string.", _imgFl));
                }
            }
            return caption;
        }

        /// <summary>
        /// Return the value of the metadata tag specified by the tagName parameter.  See the 
        /// list of recognised tagnames in the Const class (at the bottom of this file).
        /// 
        /// This method returns tag data from the image's EXIF or BitmapMetadata information, 
        /// or from the .micasa file (or .picasa file, if applicable).
        /// 
        /// Note that although this method does not itself access instance variables, it calls
        /// private methods that do access instance variables.  Therefore, this method cannot
        /// be marked as static becuase it must always be involved from an instance (and not 
        /// the class itself).
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CA1822 // Mark members as static
        public string GetMetadataValue(string tagName)
#pragma warning restore CA1822 // Mark members as static
        {
            if (tagName == null)
            {
                throw new ArgumentNullException(nameof(tagName), "Tag name cannot be null.");
            }
            else
            {
                // Initialize the EXIF file environment variable.
                try
                {
                    var ExifFs = ImageFile.FromFile(_imgFl);

                    switch (tagName)
                    {
                        case Tagnames.CaptionTagNm:
                            return GetCaptionFromImage();
                        case Tagnames.PixelXDimensionNm:
                            // For each of the EXIF Library sourced fields, the same logic applies:
                            //   1. Check that the property exists in the EXIF dataset.
                            //   2. If it exists, check that the propety's value is not null.
                            //   3. If the value is not null then retrieve the value and use it.
                            // This overall approach is used each time, and won't be individually
                            // documented any further.
                            if (ExifFs.Properties.Contains(ExifTag.PixelXDimension))
                            {
                                if (ExifFs.Properties.Get<ExifUInt>(ExifTag.PixelXDimension) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    uint Xdim = ExifFs.Properties.Get<ExifUInt>(ExifTag.PixelXDimension);
                                    if (Xdim == 0)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{Xdim}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.PixelYDimensionNm:
                            if (ExifFs.Properties.Contains(ExifTag.PixelYDimension))
                            {
                                if (ExifFs.Properties.Get<ExifUInt>(ExifTag.PixelYDimension) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    uint Ydim = ExifFs.Properties.Get<ExifUInt>(ExifTag.PixelYDimension);
                                    if (Ydim == 0)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{Ydim}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.MakeNm:
                            if (ExifFs.Properties.Contains(ExifTag.Make))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.Make) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.Model))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.Model) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.DateTimeNm:
                            if (ExifFs.Properties.Contains(ExifTag.DateTime))
                            {
                                if (ExifFs.Properties.Get<ExifDateTime>(ExifTag.DateTime) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    DateTime dateTime = ExifFs.Properties.Get<ExifDateTime>(ExifTag.DateTime);
                                    if (dateTime == DateTime.MinValue)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.DateTimeDigitizedNm:
                            if (ExifFs.Properties.Contains(ExifTag.DateTimeDigitized))
                            {
                                if (ExifFs.Properties.Get<ExifDateTime>(ExifTag.DateTimeDigitized) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    DateTime dateTimeDigitized = ExifFs.Properties.Get<ExifDateTime>(ExifTag.DateTimeDigitized);
                                    if (dateTimeDigitized == DateTime.MinValue)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return dateTimeDigitized.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.OrientationNm:
                            if (ExifFs.Properties.Contains(ExifTag.Orientation))
                            {
                                if (ExifFs.Properties.Get<ExifEnumProperty<Orientation>>(ExifTag.Orientation) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var orientation = ExifFs.Properties.Get<ExifEnumProperty<Orientation>>(ExifTag.Orientation);
                                    if (orientation == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return orientation.Value.ToString();
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.FlashNm:
                            if (ExifFs.Properties.Contains(ExifTag.Flash))
                            {
                                if (ExifFs.Properties.Get<ExifEnumProperty<Flash>>(ExifTag.Flash) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var flash = ExifFs.Properties.Get<ExifEnumProperty<Flash>>(ExifTag.Flash);
                                    if (flash == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return flash.Value.ToString();
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.LensMakerNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensMake))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensMake) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.LensModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensModel))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensModel) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.FocalLengthNm:
                            if (ExifFs.Properties.Contains(ExifTag.FocalLength))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.FocalLength) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var focalLength = ExifFs.Properties.Get<ExifURational>(ExifTag.FocalLength);
                                    if (focalLength == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        if (focalLength.Value.Denominator == 1)
                                        {
                                            return $"{focalLength.Value.Numerator}";
                                        }
                                        else
                                        {
                                            return $"{focalLength.Value.Numerator}/{focalLength.Value.Denominator}";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.FocalLengthIn35mmFilmNm:
                            if (ExifFs.Properties.Contains(ExifTag.FocalLengthIn35mmFilm))
                            {
                                if (ExifFs.Properties.Get<ExifUInt>(ExifTag.FocalLengthIn35mmFilm) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    uint focalLength35mm = ExifFs.Properties.Get<ExifUInt>(ExifTag.FocalLengthIn35mmFilm);
                                    if (focalLength35mm == 0)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{focalLength35mm}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ExposureTimeNm:
                            if (ExifFs.Properties.Contains(ExifTag.ExposureTime))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.ExposureTime) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var exposureTime = ExifFs.Properties.Get<ExifURational>(ExifTag.ExposureTime);
                                    if (exposureTime == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        if (exposureTime.Value.Denominator == 1)
                                        {
                                            return $"{exposureTime.Value.Numerator}";
                                        }
                                        else
                                        {
                                            return $"{exposureTime.Value.Numerator}/{exposureTime.Value.Denominator}";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ApertureValueNm:
                            if (ExifFs.Properties.Contains(ExifTag.ApertureValue))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.ApertureValue) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var apertureValue = ExifFs.Properties.Get<ExifURational>(ExifTag.ApertureValue);
                                    if (apertureValue == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{(double)apertureValue.Value.Numerator / apertureValue.Value.Denominator:F2}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.FNumberNm:
                            if (ExifFs.Properties.Contains(ExifTag.FNumber))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.FNumber) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var fNumber = ExifFs.Properties.Get<ExifURational>(ExifTag.FNumber);
                                    if (fNumber == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{(double)fNumber.Value.Numerator / fNumber.Value.Denominator:F1}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.SubjectDistanceNm:
                            if (ExifFs.Properties.Contains(ExifTag.SubjectDistance))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.SubjectDistance) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var subjectDistance = ExifFs.Properties.Get<ExifURational>(ExifTag.SubjectDistance);
                                    if (subjectDistance == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{(double)subjectDistance.Value.Numerator / subjectDistance.Value.Denominator:F2}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        // ISO -- How this is stored changed in EXIF v2.3; so, we first check
                        // for the new tags (PhotographicSensitivity & SensitivityType), if they
                        // don't exist then we look for the old tag (ISOSpeedRatings).
                        case Tagnames.ISONm:
                            if (MiMdSupportedImg(_imgFl))
                            {
                                object isoValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34855}"); // PhotographicSensitivity (ISO)
                                object sensitivityType = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34864}"); // SensitivityType

                                if (isoValue == null && sensitivityType == null)
                                {
                                    isoValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34867}"); // ISOSpeedRatings
                                    isoValue ??= string.Empty;
                                }
                                else
                                {
                                    // If we have the new ISO tags, we can format them correctly.
                                    if (sensitivityType != null)
                                    {
                                        string sensitivityTypeDesc = GetSensitivityTypeDescription((ushort)sensitivityType);
                                        isoValue = $"{isoValue} ({sensitivityTypeDesc})";
                                    }
                                }
                                return $"{isoValue}";
                            }
                            else
                            {
                                Debug.WriteLine($"GetMetadataTag: [tag {tagName}] '{_imgFl}' is not a supported image type for metadata retrieval.");
                                return string.Empty;
                            }

                        // Add more cases for other metadata tags as needed.
                        default:
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "GetMetadataTag: Unknown tag name '{0}'", tagName));
                            return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "GetMetadataValue ({0}): Error opening EXIF image file: {1}", _imgFl, ex.Message));
                    return string.Empty;
                }
            }
        }
     
    public static class Tagnames
        {
            public const string CaptionTagNm = "Caption";
            public const string PixelXDimensionNm = "PixelXDimension";
            public const string PixelYDimensionNm = "PixelYDimension";
            public const string MakeNm = "Make";
            public const string ModelNm = "Model";
            public const string DateTimeNm = "DateTime";
            public const string DateTimeDigitizedNm = "DateTimeDigitized";
            public const string OrientationNm = "Orientation";
            public const string FlashNm = "Flash";
            public const string LensMakerNm = "LensMaker";
            public const string LensModelNm = "LensModel";
            public const string FocalLengthNm = "FocalLength";
            public const string FocalLengthIn35mmFilmNm = "FocalLengthIn35mmFilm";
            public const string ExposureTimeNm = "ExposureTime";
            public const string ApertureValueNm = "ApertureValue";
            public const string FNumberNm = "FNumber";
            public const string SubjectDistanceNm = "SubjectDistance";
            public const string ISONm = "ISO";
        }
    }
}
