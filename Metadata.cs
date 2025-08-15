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
        /// TO DO: get any caption from the .micasa file & reconcile the two sources based on timestamps; 
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
                            if (ExifFs.Properties.Contains(ExifTag.PixelXDimension))
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
                            else
                            {
                                return string.Empty;
                            }
                        case Const.PixelYDimensionNm:
                            if (ExifFs.Properties.Contains(ExifTag.PixelYDimension))
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
                            else
                            {
                                return string.Empty;
                            }
                        case Const.MakeNm:
                            if (ExifFs.Properties.Contains(ExifTag.Make))
                            {
                                string make = ExifFs.Properties.Get<ExifAscii>(ExifTag.Make);
                                return make ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.ModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.Model))
                            {
                                string model = ExifFs.Properties.Get<ExifAscii>(ExifTag.Model);
                                return model ?? string.Empty;
                            }
                            else
                            {
                                 return string.Empty;
                            }
                        case Const.DateTimeNm:
                            if (ExifFs.Properties.Contains(ExifTag.DateTime))
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
                            else
                            {
                                 return string.Empty;
                            }
                        case Const.DateTimeDigitizedNm:
                            if (ExifFs.Properties.Contains(ExifTag.DateTimeDigitized))
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
                            else
                            {
                                return string.Empty;
                            }
                        case Const.OrientationNm:
                            if (ExifFs.Properties.Contains(ExifTag.Orientation))
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
                            else
                            {
                                return string.Empty;
                            }
                        case Const.FlashNm:
                            if (ExifFs.Properties.Contains(ExifTag.Flash))
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
                            else
                            {
                                return string.Empty;
                            }
                        case Const.LensMakerNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensMake))
                            {
                                string lensMaker = ExifFs.Properties.Get<ExifAscii>(ExifTag.LensMake);
                                return lensMaker ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.LensModelNm:
                            if (ExifFs.Properties.Contains(ExifTag.LensModel))
                            {
                                string lensModel = ExifFs.Properties.Get<ExifAscii>(ExifTag.LensModel);
                                return lensModel ?? string.Empty;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case Const.FocalLengthNm:
                            if (ExifFs.Properties.Contains(ExifTag.FocalLength))
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
        }
    }
}
