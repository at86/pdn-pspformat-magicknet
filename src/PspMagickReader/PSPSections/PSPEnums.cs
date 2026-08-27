namespace PspMagickReader.PSPSections
{
    internal enum PSPBlockID : ushort
    {
        ImageAttributes = 0,
        Creator,
        ColorPalette,
        LayerStart,
        Layer,
        Channel,
        Selection,
        AlphaBank,
        AlphaChannel,
        CompositeImage,
        ExtendedData,
        PictureTube,
        AdjustmentLayerExtension,
        VectorLayerExtension,
        VectorShape,
        PaintStyle,
        CompositeImageBank,
        CompositeImageAttributes,
        JPEGImage,
        LineStyle,
        TableBank,
        Table,
        Paper,
        Pattern,
        GroupLayerExtension,
        MaskLayerExtension,
        BrushData,
    }

    internal enum PSPCompression : ushort
    {
        None = 0,
        RLE = 1,
        LZ77 = 2,
        JPEG = 3,
    }
}
