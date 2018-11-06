using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;

namespace Steepshot.iOS.Helpers
{
    public class VideoHelper
    {
        public async Task<NSUrl> CropAssetToSquareInCenter(AVUrlAsset urlAsset, float renderSide, Tuple<int, int> rangeToTrim)
        {
            var videoTrack = urlAsset.TracksWithMediaType(AVMediaType.Video).FirstOrDefault();

            if (videoTrack == null)
                return null;

            var assetTimeRange = new CMTimeRange()
            {
                Start = CMTime.FromSeconds(rangeToTrim.Item1, (int)videoTrack.NominalFrameRate),
                Duration = CMTime.FromSeconds(rangeToTrim.Item2, (int)videoTrack.NominalFrameRate)
            };

            var transformer = new AVMutableVideoCompositionLayerInstruction
            {
                TrackID = videoTrack.TrackID
            };

            var videoTransform = videoTrack.PreferredTransform;

            var rotateInRadians = Math.Atan2(videoTransform.xx * 1.0, videoTransform.xy * 1.0);

            if (rotateInRadians == Math.PI || rotateInRadians == 0)
                videoTransform.Translate(0, -(videoTrack.NaturalSize.Height - renderSide) / 2);
            else
                videoTransform.Translate(-(videoTrack.NaturalSize.Height - renderSide) / 2, 0);

            var naturalSide = videoTrack.NaturalSize.Height > videoTrack.NaturalSize.Width ? videoTrack.NaturalSize.Width : videoTrack.NaturalSize.Height;
            videoTransform.Scale(renderSide / naturalSide, renderSide / naturalSide);

            transformer.SetTransform(videoTransform, CMTime.Zero);

            var instruction = new AVMutableVideoCompositionInstruction
            {
                TimeRange = assetTimeRange,
                LayerInstructions = new[] { transformer }
            };

            var videoComposition = new AVMutableVideoComposition
            {
                FrameDuration = new CMTime(1, (int)videoTrack.NominalFrameRate),
                Instructions = new AVVideoCompositionInstruction[] { instruction },
                RenderSize = new CGSize(renderSide, renderSide)
            };

            var outputFileName = new NSUuid().AsString();
            var documentsPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true).First();
            var outputFilePath = Path.Combine(documentsPath, Path.ChangeExtension(outputFileName, "mp4"));
            var _exportLocation = NSUrl.CreateFileUrl(outputFilePath, false, null);

            using (var exportSession = new AVAssetExportSession(urlAsset, AVAssetExportSession.PresetHighestQuality)
            {
                OutputUrl = _exportLocation,
                OutputFileType = AVFileType.Mpeg4,
                VideoComposition = videoComposition,
                ShouldOptimizeForNetworkUse = true,
                TimeRange = assetTimeRange
            })
            {
                await exportSession.ExportTaskAsync();

                if (exportSession.Status == AVAssetExportSessionStatus.Completed)
                    return _exportLocation;
                return null;
            }
        }
    }
}
