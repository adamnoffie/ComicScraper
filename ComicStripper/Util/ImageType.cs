using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public static class ImageType
    {
        public static string GetExtension(string contentType)
        {
            switch (contentType.Trim().ToLowerInvariant())
            {
                case "image/bmp": return "bmp";
                case "image/cgm": return "cgm";
                case "image/gif": return "gif";
                case "application/octet-stream": return "gif";
                case "image/ief": return "ief";
                case "image/jpeg": return "jpg";
                case "image/png": return "png";
                case "image/svg+xml": return "svg";
                case "image/tiff": return "tif";
                default: return string.Empty;
            }
        }
    }
}
