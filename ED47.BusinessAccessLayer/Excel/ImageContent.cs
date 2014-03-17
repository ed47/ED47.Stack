using System.Drawing;

namespace ED47.BusinessAccessLayer.Excel
{
    public class ImageContent   
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public Image Image { get; set; }
        public int LeftOffset { get; set; }
        public int TopOffset { get; set; }
    }
}