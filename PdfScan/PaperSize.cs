using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfScan
{
    public class PaperSize
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public string Description { get; private set; }

        public PaperSize(float width, float height, string description)
        {
            this.Width = width;
            this.Height = height;
            this.Description = description;
        }

        public static List<PaperSize> GetSizes(out PaperSize defaultSize)
        {
            defaultSize = new PaperSize(8.5F, 11F, "Letter");

            List<PaperSize> sizes = new List<PaperSize>();
            
            sizes.Add(new PaperSize(5.5F, 8.5F, "Statement"));
            sizes.Add(new PaperSize(7.5F, 10F, "Executive"));
            sizes.Add(defaultSize);
            sizes.Add(new PaperSize(8.5F, 11F, "Note"));
            sizes.Add(new PaperSize(8.5F, 13F, "Folio"));
            sizes.Add(new PaperSize(8.5F, 14F, "Legal"));
            sizes.Add(new PaperSize(11F, 17F, "Tabloid"));
            sizes.Add(new PaperSize(17F, 11F, "Ledger"));
            sizes.Add(new PaperSize(5.83F, 8.27F, "A5"));
            sizes.Add(new PaperSize(8.27F, 11.69F, "A4"));
            sizes.Add(new PaperSize(11.69F, 16.54F, "A3"));
            sizes.Add(new PaperSize(7.17F, 10.12F, "B5"));
            sizes.Add(new PaperSize(9.84F, 13.94F, "B4"));
            sizes.Add(new PaperSize(8.46F, 10.83F, "Quarto"));

            return sizes;
        }
    }
}
