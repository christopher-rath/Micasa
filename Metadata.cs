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
using System.Collections.Generic;
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

        public static string DecodeFlashBits(string flashValue)
        {
            ushort flashBits = (ushort)int.Parse(flashValue);

            var descriptions = new List<string>();

            if ((flashBits & 0x1) != 0)
            {
                descriptions.Add("Flash fired");
            }
            else
            {
                descriptions.Add("Flash did not fire");
            }

            if ((flashBits & 0x4) != 0)
            {
                descriptions.Add("Return light detected");
            }
            else if ((flashBits & 0x2) != 0)
            {
                descriptions.Add("Return light not detected");
            }

            if ((flashBits & 0x10) != 0)
            {
                descriptions.Add("Compulsory flash");
            }
            else if ((flashBits & 0x18) == 0x18)
            {
                descriptions.Add("Auto flash");
            }

            if ((flashBits & 0x20) != 0)
            {
                descriptions.Add("No flash function");
            }

            if ((flashBits & 0x40) != 0)
            {
                descriptions.Add("Red-eye reduction");
            }

            return string.Join(", ", descriptions);

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
            else if (!MiMdSupportedImg(_imgFl))
            {
                Debug.WriteLine($"GetMetadataTag: [tag {tagName}] '{_imgFl}' is not a supported image type for metadata retrieval.");
                return string.Empty;
            }
            else
            {
                try
                {
                    // Initialize the EXIF file environment variable.
                    var ExifFs = ImageFile.FromFile(_imgFl);

                    switch (tagName)
                    {
                        case Tagnames.CaptionTagNm:
                            return GetCaptionFromImage();
                        case Tagnames.PixelXDimensionNm:
                            object xDimValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=40962}"); // PixelXDimension
                            if (xDimValue == null)
                            {
                                return string.Empty;
                            }
                            else if (xDimValue is ExifUInt pixelXDimension)
                            {
                                // If the value is an ExifUInt, return it as a string.
                                return $"{pixelXDimension.Value}";
                            }
                            else
                            {
                                // If it's not an ExifUInt, return it as a string.
                                return $"{xDimValue}";
                            }
                        case Tagnames.PixelYDimensionNm:
                            object yDimValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=40963}"); // PixelYDimension
                            if (yDimValue == null)
                            {
                                return string.Empty;
                            }
                            else if (yDimValue is ExifUInt pixelYDimension)
                            {
                                // If the value is an ExifUInt, return it as a string.
                                return $"{pixelYDimension.Value}";
                            }
                            else
                            {
                                // If it's not an ExifUInt, return it as a string.
                                return $"{yDimValue}";
                            }
                        case Tagnames.MakeNm:
                            // We have to use EXIFLibrary to retrive the Make value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // Make tag.  The query .GetQuery("/app1/ifd/exif/{ushort=271}" should 
                            // return the Make field but instead it returns NULL.
                            //
                            // For each of the EXIF Library sourced fields, the same logic applies:
                            //   1. Check that the property exists in the EXIF dataset.
                            //   2. If it exists, check that the propety's value is not null.
                            //   3. If the value is not null then retrieve the value and use it.
                            // This overall approach is used each time, and won't be individually
                            // documented any further.
                            if (ExifFs.Properties.Contains(ExifTag.Make))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.Make) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ModelNm:
                            // We have to use EXIFLibrary to retrive the Make value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // Model tag.  The query .GetQuery("/app1/ifd/exif/{ushort=272}" should 
                            // return the Model field but instead it returns NULL.
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
                            object dateTimeDigitizedValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=36867}"); // DateTimeDigitized
                            if (dateTimeDigitizedValue == null)
                            {
                                return string.Empty;
                            }
                            else if (dateTimeDigitizedValue is ExifDateTime dateTimeDigitized)
                            {
                                // If the value is an ExifDateTime, return it as a string.
                                if (dateTimeDigitized.Value == DateTime.MinValue)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    return dateTimeDigitized.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                            }
                            else
                            {
                                // If it's not an ExifDateTime, return it as a string.
                                return $"{dateTimeDigitizedValue}";
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
                            object flashValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=37385}"); // Flash
                            if (flashValue == null)
                            {
                                return string.Empty;
                            }
                            else 
                            {
                                return DecodeFlashBits($"{flashValue}");
                            }
                        case Tagnames.LensMakerNm:
                            // Neither the EXIF Library nor the BitmapMetadata class return a value to
                            // this query.  For the time being, the EXIF Library code has been left intact
                            // and the BitmapMetadata class code left in the comment block, below.
                            object lensMakerValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=42034}"); // LensMake
                            if (lensMakerValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return $"{lensMakerValue}";
                            }
                            //if (ExifFs.Properties.Contains(ExifTag.LensMake))
                            //{
                            //    return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensMake) ?? string.Empty;
                            //}
                            //else
                            //{
                            //    return string.Empty;
                            //}
                        case Tagnames.LensModelNm:
                            // Neither the EXIF Library nor the BitmapMetadata class return a value to
                            // this query.  For the time being, the EXIF Library code has been left intact
                            // and the BitmapMetadata class code left in the comment block, below.
                            if (ExifFs.Properties.Contains(ExifTag.LensMake))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensMake) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                            //object lensModelValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=42036}"); // LensModel
                            //if (lensModelValue == null)
                            //{
                            //    return string.Empty;
                            //}
                            //else
                            //{
                            //    return $"{lensModelValue}";
                            //}
                        case Tagnames.FocalLengthNm:
                            // We have to use EXIFLibrary to retrive the FocalLength value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // FocalLength tag.  The query .GetQuery("/app1/ifd/exif/{ushort=37386}" should 
                            // return the FNumber but instead it returns a nonsensically large value 
                            // (e.g., 42949673610).
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
                            // We have to use EXIFLibrary to retrive the FocalLengthIn35mmFilm value 
                            // because as of August 2025 the BitmapMetadata class does not yet properly 
                            // support the FocalLengthIn35mmFilm tag.  The query 
                            // .GetQuery("/app1/ifd/exif/{ushort=42053}" should return the FNumber but 
                            // instead it returns null.
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
                            // We have to use EXIFLibrary to retrive the FocalLengthIn35mmFilm value 
                            // because as of August 2025 the BitmapMetadata class does not yet properly 
                            // support the FocalLengthIn35mmFilm tag.  The query 
                            // .GetQuery("/app1/ifd/exif/{ushort=33434}" should return the FNumber but 
                            // instead it returns null.
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
                            // We have to use EXIFLibrary to retrive the FocalLength value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // FocalLength tag.  The query .GetQuery("/app1/ifd/exif/{ushort=37378}" should 
                            // return the FNumber but instead it returns a nonsensically large value 
                            // (e.g., 429496729897085.
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
                                    // We have to use EXIFLibrary to retrive the FNumber value because as of 
                                    // August 2025 the BitmapMetadata class does not yet properly support the 
                                    // FNumber tag.  The query .GetQuery("/app1/ifd/exif/{ushort=33437}" should 
                                    // return the FNumber but instead it returns a nonsensically large value 
                                    // (e.g., 4294967295).
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
                                    object distanceValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=37383}"); // SubjectDistance
                                    if (distanceValue == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        // If the distance value is a rational number, format it.
                                        if (distanceValue is ExifURational subjectDistance)
                                        {
                                            return $"{(double)subjectDistance.Value.Numerator / subjectDistance.Value.Denominator:F2}";
                                        }
                                        else
                                        {
                                            // If it's not a rational number, return it as a string.
                                            return $"{distanceValue}";
                                        }
                                    }
                                // ISO -- How this is stored changed in EXIF v2.3; so, we first check
                                // for the new tags (PhotographicSensitivity & SensitivityType), if they
                                // don't exist then we look for the old tag (ISOSpeedRatings).
                                case Tagnames.ISONm:
                                    object isoValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34855}"); // PhotographicSensitivity (ISO)
                                    object sensitivityType = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34864}"); // SensitivityType

                                    if (isoValue == null && sensitivityType == null)
                                    {
                                        isoValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34867}"); // ISOSpeedRatings
                                        isoValue ??= string.Empty;
                                    }
                                    else
                                    {
                                        // If we have the new ISO tags, format them correctly.
                                        if (sensitivityType != null)
                                        {
                                            string sensitivityTypeDesc = GetSensitivityTypeDescription((ushort)sensitivityType);
                                            isoValue = $"{isoValue} ({sensitivityTypeDesc})";
                                        }
                                    }
                                    return $"{isoValue}";

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
