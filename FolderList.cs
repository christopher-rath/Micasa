using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Micasa
{
    class FolderList
    {
        private FolderListTypes _theListType;
        private LiteDatabase _db = new(Database.ConnectionString(Database.DBFilename));
        private ILiteCollection<PhotosTbl> _PhotoCol;
        private ILiteCollection<FoldersTbl> _FolderCol;
        private string[] _WatchedFolders = WatchedLists.Instance.WatchedFolders;
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
                    if (_WatchedFolders.Count() > _WatchedFolderItem)
                    {
                        return _WatchedFolders[_WatchedFolderItem++];
                    }
                    else
                    {
                        return "";
                    }
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
    }

    [Flags] public enum FolderListTypes
    {
        ScanForDelete,
        ScanForAdd,
        ComprehensiveScan
    }
}
