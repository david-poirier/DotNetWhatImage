
using System.Collections.Generic;

namespace WhatImage
{
    public class MetaDetector
    {
        private readonly List<IImageSniffer> _sniffers;

        public MetaDetector()
        {
            _sniffers = new List<IImageSniffer>
            {
                new JpegSniffer(),
                new PngSniffer(),
                new GifSniffer(),
                new WebpSniffer()
            };
        }

        public ImageMetadata SniffMetadata(byte[] imageData)
        {
            var sniffer = GetSniffer(imageData);
            return sniffer?.SniffMetadata(imageData);
        }

        private IImageSniffer GetSniffer(byte[] imageData)
        {
            foreach (var sniffer in _sniffers)
            {
                if (sniffer.CanSniff(imageData))
                    return sniffer;
            }
            return null;
        }
    }
}