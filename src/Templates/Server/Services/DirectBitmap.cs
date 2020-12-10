using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Templates.Blazor.Server.Services
{
    public class DirectBitmap : IDisposable
    {
        private GCHandle _gcHandle;
        public Bitmap Bitmap { get; }
        public Image<Bgra32> Image { get; }
        public Bgra32[] Buffer { get; }
        public int Height { get; }
        public int Width { get; }

        public DirectBitmap(int width, int height)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            Buffer = GC.AllocateUninitializedArray<Bgra32>(width * height, true);
            _gcHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, _gcHandle.AddrOfPinnedObject());
            Image = SixLabors.ImageSharp.Image.WrapMemory(Buffer.AsMemory(), width, height);
        }

        public void Dispose()
        {
            Bitmap.Dispose();
            Image.Dispose();
            _gcHandle.Free();
        }
    }
}
