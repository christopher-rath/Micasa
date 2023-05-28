using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Micasa
{
    sealed class FolderList : IDisposable
    {
        private readonly FolderListTypes _theListType;
        private readonly LiteDatabase _db = new(Database.ConnectionString(Database.DBFilename));
        private ILiteCollection<PhotosTbl> _PhotoCol;
        private ILiteCollection<FoldersTbl> _FolderCol;
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
#pragma warning disable CA1829
                    if (_WatchedFolders.Count() > _WatchedFolderItem)
                    {
                        return _WatchedFolders[_WatchedFolderItem++];
                    }
                    else
                    {
                        return "";
                    }
#pragma warning restore CA1829
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    [Flags] public enum FolderListTypes
    {
        ScanForDelete,
        ScanForAdd,
        ComprehensiveScan
    }
}
