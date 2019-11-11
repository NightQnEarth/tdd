﻿using System;
using System.Drawing;
using System.Linq;
using TagsCloudVisualization.CloudLayouters;
using TagsCloudVisualization.Extensions;
using TagsCloudVisualization.TagClouds;

namespace TagsCloudVisualization
{
    internal static class Program
    {
        private const string WebCloudFilename = "WebTagCloud.bmp";
        private const string CommonWordsCloudFilename = "CommonWordsTagCloud.bmp";
        private static readonly Size imagesSize = new Size(800, 600);
        private static readonly Point imageCenter = new Point(imagesSize.Width / 2, imagesSize.Height / 2);

        private static readonly TagCloudContext[] cloudContexts =
        {
            new TagCloudContext(WebCloudFilename, imagesSize, TagCloudContent.WebCloudStrings, new WebCloud(),
                                new CircularCloudLayouter(imageCenter)),
            new TagCloudContext(CommonWordsCloudFilename, new Size(800, 600), TagCloudContent.CommonWordsCloudStrings,
                                new CommonWordsCloud(), new CircularCloudLayouter(imageCenter))
        };

        private static void Main()
        {
            foreach (var cloudContext in cloudContexts)
                CreateTagCloudImage(cloudContext);
        }

        private static void CreateTagCloudImage(TagCloudContext cloudContext)
        {
            var shuffledContentStrings = cloudContext.TagCloudContent.Take(1)
                                                     .Concat(cloudContext.TagCloudContent.Skip(1)
                                                                         .SequenceShuffle(new Random()))
                                                     .ToArray();

            using var bitmap = cloudContext.Cloud.CreateBitmap(shuffledContentStrings, cloudContext.CloudLayouter,
                                                               cloudContext.ImageSize);

            bitmap.Save(cloudContext.ImageName);
        }
    }
}