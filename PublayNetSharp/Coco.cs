namespace PublayNetSharp
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class CocoEntry
    {
        public CocoEntry(int id, string fileName, float height, float width)
        {
            Id = id;
            FileName = fileName;
            Height = height;
            Width = width;
            Annotations = new List<CocoAnnotation>();
        }

        public int Id { get; set; }

        public string FileName { get; set; }

        public List<CocoAnnotation> Annotations { get; set; }

        public float Height { get; set; }

        public float Width { get; set; }
    }


    public class CocoFile
    {
        public List<CocoImage> images { get; set; }
        public List<CocoAnnotation> annotations { get; set; }
        public List<CocoCategory> categories { get; set; }
    }

    public class CocoImage
    {
        public int id { get; set; }
        public string file_name { get; set; }
        public float height { get; set; }
        public float width { get; set; }
    }

    public class CocoCategory
    {
        public int id { get; set; }
        public string supercategory { get; set; }
        public string name { get; set; }
    }

    public class CocoAnnotation
    {
        public float[][] segmentation { get; set; }

        public PointF[][] GetSegmentationPoints()
        {
            PointF[][] pointsArray = new PointF[segmentation.Length][];
            for (int s = 0; s < segmentation.Length; s++)
            {
                List<PointF> points = new List<PointF>();
                for (int i = 0; i < segmentation[s].Length; i += 2)
                {
                    points.Add(new PointF(segmentation[s][i], segmentation[s][i + 1]));
                }
                pointsArray[s] = points.ToArray();
            }
            return pointsArray;
        }

        public RectangleF GetBBoxRectangle()
        {
            if (bbox == null || bbox.Count() != 4)
            {
                return new RectangleF();
            }
            return new RectangleF(bbox[0], bbox[1], bbox[2], bbox[3]);
        }

        /// <summary>
        /// 
        /// </summary>
        public double area { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int iscrowd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int image_id { get; set; }

        /// <summary>
        /// [x,y,width,height]
        /// </summary>
        public float[] bbox { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int category_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
    }

}
