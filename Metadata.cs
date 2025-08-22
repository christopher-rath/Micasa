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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
    /// For the Exif Library documentation, see: https://oozcitak.github.io/exiflibrary/index.html
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
            ushort flashBits = (ushort)int.Parse(flashValue, CultureInfo.InvariantCulture);

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

        public static string DecodeWhiteBalance(string whiteBalanceValue)
        {
            ushort whiteBalance = (ushort)int.Parse(whiteBalanceValue, CultureInfo.InvariantCulture);

            return whiteBalance switch
            {
                0 => "Auto",
                1 => "Manual",
                _ => $"Unknown ({whiteBalanceValue})"
            };
        }

        public static string DecodeMeteringMode(string meteringModeValue)
        {
            ushort meteringMode = (ushort)int.Parse(meteringModeValue, CultureInfo.InvariantCulture);

            return meteringMode switch
            {
                0 => "Unknown",
                1 => "Average",
                2 => "Center Weighted Average",
                3 => "Spot",
                4 => "MultiSpot",
                5 => "Pattern",
                6 => "Partial",
                255 => "Other",
                _ => $"Undefined ({meteringModeValue})"
            };
        }

        public static string DecodeExposureBias(string exposureBiasValue)
        {
            // The exposure bias is stored as a signed rational number.  The EXIFLibrary
            // returns it as a string in the form "numerator/denominator".
            if (string.IsNullOrEmpty(exposureBiasValue))
            {
                return "0";
            }
            else if (!exposureBiasValue.Contains('/'))
            {                 
                // If the value is not in the expected format, return it as-is.
                return exposureBiasValue;
            }
            else
            {
                string[] parts = exposureBiasValue.Split('/');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int numerator) || !int.TryParse(parts[1], out int denominator) || denominator == 0)
                {
                    return "0";
                }
                else
                {
                    double exposureBias = (double)numerator / denominator;
                    return exposureBias.ToString("F1", CultureInfo.InvariantCulture);
                }
            }
        }

        public static string DecodeExposureProgram(string exposureProgramValue)
        {
            ushort exposureProgram = (ushort)int.Parse(exposureProgramValue, CultureInfo.InvariantCulture);

            return exposureProgram switch
            {
                0 => "Not defined",
                1 => "Manual",
                2 => "Normal program",
                3 => "Aperture priority",
                4 => "Shutter priority",
                5 => "Creative program (biased towards depth of field)",
                6 => "Action program (biased towards fast shutter speed)",
                7 => "Portrait mode (for closeup photos with the background out of focus)",
                8 => "Landscape mode (for landscape photos with the background in focus)",
                _ => $"Unknown ({exposureProgramValue})"
            };
        }

        public static string FormatGPSVersionID(byte[] gpsVersionID)
        {
            if (gpsVersionID == null || gpsVersionID.Length != 4)
            {
                return "Unknown";
            }
            else
            {
                return string.Join(".", gpsVersionID);
            }
        }

        public static string FormatEXIFVersion(byte[] exifVersion)
        {
            if (exifVersion == null || exifVersion.Length != 4)
            {
                return "Unknown";
            }
            else
            {
                return $"{(char)exifVersion[0]}.{(char)exifVersion[1]}.{(char)exifVersion[2]}.{(char)exifVersion[3]}";
            }
        }

        public static string FormatDimensions(string xDimension, string yDimension, string units)
        {
            if (string.IsNullOrEmpty(xDimension) && string.IsNullOrEmpty(yDimension))
            {
                return string.Empty;
            }
            else
            {
                return $"{xDimension} x {yDimension} {units}";
            }
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
        /// 
        /// TO DO: When a BitmapMetadata or EXIF Library qurey don't return a value, try the
        ///        other supported query methods.
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
                        case Tagnames.WhiteBalanceNm:
                            object whiteBalanceValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=41987}"); // WhiteBalance
                            if (whiteBalanceValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return DecodeWhiteBalance($"{whiteBalanceValue}");
                            }
                        case Tagnames.MeteringModeNm:
                            object meteringModeValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=37383}"); // MeteringMode
                            if (meteringModeValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return DecodeMeteringMode($"{meteringModeValue}");
                            }
                        case Tagnames.ExposureProgramNm:
                            object exposureProgramValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=34850}"); // ExposureProgram
                            if (exposureProgramValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return DecodeExposureProgram($"{exposureProgramValue}");
                            }

                        case Tagnames.ColorSpaceNm:
                            // We have to use EXIFLibrary to retrive the ColorSpace value 
                            // because as of August 2025 the BitmapMetadata class does not yet properly 
                            // support the ColorSpace tag.  The query 
                            // .GetQuery("/app1/ifd/exif/{ushort=40961}" should return the ColorSpace but 
                            // instead it generates an invalide query exception.
                            if (ExifFs.Properties.Contains(ExifTag.ColorSpace))
                            {
                                if (ExifFs.Properties.Get<ExifURational>(ExifTag.ColorSpace) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var colorSpace = ExifFs.Properties.Get(ExifTag.ColorSpace);
                                    if (colorSpace == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{colorSpace.ToString}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.XResolutionNm:
                            // We have to use EXIFLibrary to retrive the XResolution value 
                            // because as of August 2025 the BitmapMetadata class does not yet properly 
                            // support the XResolution tag.  The query 
                            // .GetQuery("/app1/ifd/exif/{ushort=0x011A}" should return the XResolution but 
                            // instead it generates an invalide query exception.
                            if (ExifFs.Properties.Contains(ExifTag.XResolution))
                            {
                                if (ExifFs.Properties.Get<ExifAscii>(ExifTag.XResolution) == null)
                                { 
                                    return string.Empty; 
                                }
                                else
                                {
                                    var xResolution = ExifFs.Properties.Get(ExifTag.XResolution);
                                    if (xResolution == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{xResolution}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.YResolutionNm:
                            // We have to use EXIFLibrary to retrive the YResolution value 
                            // because as of August 2025 the BitmapMetadata class does not yet properly 
                            // support the YResolution tag.  The query 
                            // .GetQuery("/app1/ifd/exif/{ushort=0x011A}" should return the YResolution but 
                            // instead it generates an invalide query exception.
                            if (ExifFs.Properties.Contains(ExifTag.YResolution))
                            {
                                if (ExifFs.Properties.Get<ExifAscii>(ExifTag.YResolution) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var yResolution = ExifFs.Properties.Get(ExifTag.YResolution);
                                    if (yResolution == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{yResolution}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ResolutionUnitNm:
                            // We have to use EXIFLibrary to retrive the ResolutionUnit value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // ResolutionUnit tag.  The query .GetQuery("/app1/ifd/exif/{ushort=0x0128}" should 
                            // return the ResolutionUnit but instead it returns an invalide query error.
                            if (ExifFs.Properties.Contains(ExifTag.ResolutionUnit))
                            {
                                if (ExifFs.Properties[ExifTag.ResolutionUnit] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var resolutionUnit = ExifFs.Properties.Get<ExifEnumProperty<ResolutionUnit>>(ExifTag.ResolutionUnit);
                                    if (resolutionUnit == null)
                                    {
                                        return string.Empty;    
                                    }
                                    else
                                    {
                                        return $"{resolutionUnit}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.SoftwareNm:
                            // We have to use EXIFLibrary to retrive the Software value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // Software tag.  The query .GetQuery("/app1/ifd/exif/{ushort=0x0131}" should 
                            // return the Software but instead it returns an invalide query error.
                            if (ExifFs.Properties.Contains(ExifTag.Software))
                            {
                                if (ExifFs.Properties[ExifTag.Software] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var software = ExifFs.Properties.Get<ExifAscii>(ExifTag.Software);
                                    if (software == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{software}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ArtistNm:
                            // We have to use EXIFLibrary to retrive the Artist value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // Artist tag.  The query .GetQuery("/app1/ifd/exif/{ushort=315}" should 
                            // return the Artist but instead it returns null value.
                            if (ExifFs.Properties.Contains(ExifTag.Artist))
                            {
                                if (ExifFs.Properties[ExifTag.Artist] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var artist = ExifFs.Properties.Get<ExifAscii>(ExifTag.Artist);
                                    if (artist == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{artist}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.CopyrightNm:
                            // We have to use EXIFLibrary to retrive the Copyright value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // Copyright tag.  The query .GetQuery("/app1/ifd/exif/{ushort=33432}" should 
                            // return the Copyright but instead it returns null value.
                            if (ExifFs.Properties.Contains(ExifTag.Copyright))
                            {
                              if (ExifFs.Properties[ExifTag.Copyright] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var copyright = ExifFs.Properties.Get<ExifAscii>(ExifTag.Copyright);
                                    if (copyright == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{copyright}";
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ShutterSpeedValueNm:
                            // We have to use EXIFLibrary to retrive the ShutterSpeed value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // ShutterSpeed tag.  The query .GetQuery("/app1/ifd/exif/{ushort=37377}" should 
                            // return the ShutterSpeed but instead it returns a nonsensically large value
                            // (e.g., 18014398522064896).
                            if (ExifFs.Properties.Contains(ExifTag.ShutterSpeedValue))
                            {
                                if (ExifFs.Properties[ExifTag.ShutterSpeedValue] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var shutterSpeed = ExifFs.Properties.Get<ExifSRational>(ExifTag.ShutterSpeedValue);
                                    if (shutterSpeed == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{(double)shutterSpeed.Value.Numerator / shutterSpeed.Value.Denominator:F1}";
                                    }

                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.ExposureBiasValueNm:
                            // We have to use EXIFLibrary to retrive the ExposureBias value because as of 
                            // August 2025 the BitmapMetadata class does not yet properly support the 
                            // ExposureBias tag.  The query .GetQuery("/app1/ifd/exif/{ushort=37377}" should 
                            // return the ExposureBias but instead it returns an error.
                            if (ExifFs.Properties.Contains(ExifTag.ExposureBiasValue))
                            {
                                if (ExifFs.Properties[ExifTag.ExposureBiasValue] == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var exposureBias = ExifFs.Properties.Get<ExifSRational>(ExifTag.ExposureBiasValue);
                                    if (exposureBias == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return DecodeExposureBias($"{exposureBias}");
                                    }
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Tagnames.MakerNoteNm:
                            object makerNoteValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=37500}"); // MakerNote
                            if (makerNoteValue == null)
                            { 
                                return string.Empty; 
                            }
                            else
                            {
                                return $"{makerNoteValue}";
                            }
                        case Tagnames.UserCommentNm:
                            object userCommentValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=37510}"); // UserComment
                            if (userCommentValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return $"{userCommentValue}";
                            }
                        case Tagnames.GPSVersionIDNm:
                            object gpsVersionValue = _mdBmMd.GetQuery("/app1/ifd/gps/{ushort=0}"); // GPSVersionID
                            if (gpsVersionValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return FormatGPSVersionID((byte[])gpsVersionValue);
                            }
                        case Tagnames.EXIFVersionNm:
                            object exifVersionValue = _mdBmMd.GetQuery("/app1/ifd/exif/{ushort=36864}"); // EXIFVersion
                            if (exifVersionValue == null)
                            {
                                return string.Empty;
                            }
                            else
                            {
                                return FormatEXIFVersion((byte[])exifVersionValue);
                            }


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
            public const string WhiteBalanceNm = "WhiteBalance";
            public const string MeteringModeNm = "MeteringMode";
            public const string ExposureProgramNm = "ExposureProgram";
            public const string ColorSpaceNm = "ColorSpace";
            public const string XResolutionNm = "XResolution";
            public const string YResolutionNm = "YResolution";
            public const string ResolutionUnitNm = "ResolutionUnit";
            public const string SoftwareNm = "Software";
            public const string ArtistNm = "Artist";
            public const string CopyrightNm = "Copyright";
            public const string ShutterSpeedValueNm = "ShutterSpeedValue";
            public const string ExposureBiasValueNm = "ExposureBiasValue";
            public const string MakerNoteNm = "MakerNote";
            public const string UserCommentNm = "UserComment";
            public const string GPSVersionIDNm = "GPSVersionID";
            public const string EXIFVersionNm = "EXIFVersion";
        }
    }
}
