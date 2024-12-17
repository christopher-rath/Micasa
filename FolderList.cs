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
using System.Linq;
using LiteDB;

namespace Micasa
{
    internal sealed partial class FolderList : IDisposable
    {
        private readonly FolderListTypes _theListType;
        private readonly LiteDatabase _db = new(Database.ConnectionString(Database.DBFilename));
        private readonly ILiteCollection<PhotosTbl> _PhotoCol;
        private readonly ILiteCollection<FoldersTbl> _FolderCol;
        private readonly string[] _WatchedFolders = WatchedLists.Instance.WatchedFolders;
        private long _WatchedFolderItem = 0;

        public FolderList(FolderListTypes ListType)
        {
            _PhotoCol = _db.GetCollection<PhotosTbl>(Constants.sMcPhotosColNm);
            _FolderCol = _db.GetCollection<FoldersTbl>(Constants.sMcFoldersColNm);
            _theListType = ListType;
        }

        public string NextFolder
        {
            get
            {
                if (FolderListTypes.ComprehensiveScan == _theListType)
                {
#pragma warning disable CA1829 // Use Length/Count property instead of Enumerable.Count method
                    if (_WatchedFolders.Count() > _WatchedFolderItem)
                    {
                        return _WatchedFolders[_WatchedFolderItem++];
                    }
                    else
                    {
                        return "";
                    }
#pragma warning restore CA1829 // Use Length/Count property instead of Enumerable.Count method
                }
                else if (FolderListTypes.ScanForAdd == _theListType)
                {
                    return "";
                }
                else
                {
                    return "";
                }
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }

    [Flags]
    public enum FolderListTypes
    {
        ScanForDelete,
        ScanForAdd,
        ComprehensiveScan
    }
}
