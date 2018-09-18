using System;
using Android.Graphics;
using Newtonsoft.Json;
using Steepshot.Core.Models.Common;
using Steepshot.CustomViews;

namespace Steepshot.Utils
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GalleryMediaModel
    {
        private bool _selected;

        public Action ModelChanged;


        public long Id { get; set; }

        public string Path { get; set; }

        public string Bucket { get; set; }

        public int Orientation { get; set; }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                ModelChanged?.Invoke();
            }
        }

        private int _selectionPosition;

        public int SelectionPosition
        {
            get => _selectionPosition;
            set
            {
                _selectionPosition = value;
                ModelChanged?.Invoke();
            }
        }

        [JsonProperty]
        public ImageParameters Parameters { get; set; }

        public Bitmap PreparedBitmap { get; set; }

        public bool MultySelect { get; set; }
        
        [JsonProperty]
        public UploadState UploadState { get; set; }
        
        [JsonProperty]
        public string TempPath { get; set; }
        
        [JsonProperty]
        public UUIDModel UploadMediaUuid { get; set; }
    }


    public enum UploadState
    {
        None,

        Prepare,

        ReadyToSave,
        Saved,

        UploadStart,
        UploadEnd,
        UploadError,

        UploadVerified,
        Ready,
    }
}
