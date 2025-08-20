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
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ExifLibrary;
using Windows.Networking.BackgroundTransfer;

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
        private static BitmapMetadata _md;

        public Metadata(string imgFl)
        {
            // Initialize the image file path.
            _imgFl = imgFl;

            // Initialize the metadata object.
            try
            {
                _imgFs = File.OpenRead(imgFl);
                _img = BitmapFrame.Create(_imgFs);
                _md = (BitmapMetadata)_img.Metadata;
            }
            catch
            {
                MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Metadata: Error opening image file: {0}", imgFl),
                    "Micasa Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _md = null;
            }
        }


        /// <summary>
        /// Get the Caption from the image file.  This method is agnostic regarding the formatting
        /// of the caption string; that is, any RTF, HTML, or other markup is simply retrieved 
        /// from the image and returned by the method in its raw form.
        /// 
        /// The BitmapMetadata class supports GIF, JPEG, PNG, and TIFF image formats.
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
            bool supportedImg = false;

            supportedImg = Path.GetExtension(_imgFl).ToLower(CultureInfo.InvariantCulture) switch
            {
                Constants.sMcFT_Gif or Constants.sMcFT_Jpg or Constants.sMcFT_JpgA or Constants.sMcFT_Png
                    or Constants.sMcFT_Tif or Constants.sMcFT_TifA => true,
                _ => false,
            };
            if (supportedImg)
            {
                try
                {
                    if (_md != null)
                    {
                        caption = _md.Title;
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
                        case Const.CaptionTagNm:
                            return GetCaptionFromImage();
                        case Const.PixelXDimensionNm:
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
                        case Const.PixelYDimensionNm:
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
                        case Const.MakeNm:
                            if (ExifFs.Properties.Contains(ExifTag.Make))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.Make) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.ModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.Model))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.Model) ?? string.Empty;
                            }
                            else
                            {
                                 return string.Empty;
                            }
                        case Const.DateTimeNm:
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
                        case Const.DateTimeDigitizedNm:
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
                        case Const.OrientationNm:
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
                        case Const.FlashNm:
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
                        case Const.LensMakerNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensMake))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensMake) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.LensModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensModel))
                            {
                                return ExifFs.Properties.Get<ExifAscii>(ExifTag.LensModel) ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.FocalLengthNm:
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
                        case Const.FocalLengthIn35mmFilmNm:
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
                        case Const.ExposureTimeNm:
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
                        case Const.ApertureValueNm:
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
                        case Const.FNumberNm:
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
                        case Const.SubjectDistanceNm:
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
                        case Const.ISONm:
                            // ExifLibary doesn't support the Exif v2.3 tags, so we will have to use the 
                            // BitmapMetadata class to retrieve the ISO speed.
                            //
                            //if (ExifFs.Properties.Contains(ExifTag.PhotographicSensitivity) &&
                            //    ExifFs.Properties.Contains(ExifTag.SensitivityType))
                            //{
                            //    if (ExifFs.Properties.Get<ExifUInt>(ExifTag.PhotographicSensitivity) == null ||
                            //        ExifFs.Properties.Get<ExifEnumProperty<SensitivityType>>(ExifTag.SensitivityType) == null)
                            //    {
                            //        return string.Empty;
                            //    }
                            //    else
                            //    {
                            //        uint isoSpeed = ExifFs.Properties.Get<ExifUInt>(ExifTag.PhotographicSensitivity);
                            //        if (isoSpeed == 0)
                            //        {
                            //            return string.Empty;
                            //        }
                            //        else
                            //        {
                            //            return $"{isoSpeed}";
                            //        }
                            //    }
                            //}
                            //else 
                            if (ExifFs.Properties.Contains(ExifTag.ISOSpeedRatings))
                            {
                                if (ExifFs.Properties.Get<ExifUInt>(ExifTag.ISOSpeedRatings) == null)
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    uint isoSpeed = ExifFs.Properties.Get<ExifUInt>(ExifTag.ISOSpeedRatings);
                                    if (isoSpeed == 0)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return $"{isoSpeed}";
                                    }
                                }
                            }
                            else
                            {
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

        public static class Const
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
