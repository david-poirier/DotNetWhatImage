using WhatImage;

internal interface IImageSniffer
{
    bool CanSniff(byte[] imageData);
    ImageMetadata SniffMetadata(byte[] imageData);
}