using BeachSAAPI.Models;
using System.Globalization;
using System.Text;

namespace BeachSAAPI.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        private const double PageWidth = 612;
        private const double PageHeight = 792;
        private const double Margin = 50;

        private static readonly double[] Primary = { 0.043, 0.165, 0.447 };
        private static readonly double[] Secondary = { 0.984, 0.686, 0.090 };
        private static readonly double[] TextColor = { 0.239, 0.310, 0.451 };
        private static readonly double[] LineColor = { 0.780, 0.839, 0.949 };

        private static readonly int[] HelveticaWidths =
        {
            278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,
            1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,
            667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,
            333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
            556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584
        };

        private readonly StringBuilder content = new StringBuilder();
        private readonly CultureInfo inv = CultureInfo.InvariantCulture;

        public byte[] Generate(Invoice invoice, Reservation reservation, User user, Package package)
        {
            this.content.Clear();

            double downPayment = invoice.totalUsd * package.PercentageTotalAmount / 100;
            double monthlyPayment = package.RemainingMonthlyInstallments > 0
                ? (invoice.totalUsd - downPayment) / package.RemainingMonthlyInstallments
                : 0;

            double y = PageHeight - Margin;

            this.Rect(0, PageHeight - 110, PageWidth, 110, Primary);
            this.Text(Margin, PageHeight - 60, "Beach.SA", 24, true, new double[] { 1, 1, 1 });
            this.Text(Margin, PageHeight - 82, "Hoteles & Resort", 11, false, new double[] { 1, 1, 1 });
            this.TextRight(PageWidth - Margin, PageHeight - 60, $"Factura N° {invoice.InvoiceNumber:D6}", 16, true, Secondary);
            this.TextRight(PageWidth - Margin, PageHeight - 82, $"Fecha: {invoice.InvoiceDate:dd/MM/yyyy}", 11, false, new double[] { 1, 1, 1 });

            y = PageHeight - 145;

            y = this.Section(y, "Cliente");
            y = this.Row(y, "Nombre", user.FullName);
            y = this.Row(y, "Cédula", $"{user.ID} ({user.IdType})");
            y = this.Row(y, "Correo", user.Email);
            if (user.PhoneNumber > 0)
            {
                y = this.Row(y, "Teléfono", user.PhoneNumber.ToString());
            }

            y -= 10;
            y = this.Section(y, "Reservación");
            y = this.Row(y, "Paquete", package.PackageType);
            y = this.Row(y, "Entrada", reservation.CheckInDate.ToString("dd/MM/yyyy"));
            y = this.Row(y, "Salida", reservation.CheckOutDate.ToString("dd/MM/yyyy"));
            y = this.Row(y, "Noches", reservation.AccommodationDays.ToString());
            y = this.Row(y, "Huéspedes", reservation.GuestQuantity.ToString());
            y = this.Row(y, "Forma de pago", reservation.PaymentMethod);
            if (!string.IsNullOrEmpty(reservation.CheckNumber))
            {
                y = this.Row(y, "Cheque", $"N° {reservation.CheckNumber} · {reservation.Bank}");
            }

            y -= 10;
            y = this.Section(y, "Detalle del pago");
            y = this.Row(y, "Subtotal", this.Usd(invoice.Subtotal));
            if (invoice.DiscountAmount > 0)
            {
                y = this.Row(y, $"Descuento ({(invoice.Discount * 100).ToString("N0", inv)} %)", "-" + this.Usd(invoice.DiscountAmount));
            }
            y = this.Row(y, $"IVA ({(invoice.IVA * 100).ToString("N0", inv)} %)", this.Usd(invoice.IvaAmount));
            y = this.Row(y, "Tipo de cambio BCCR", "CRC " + invoice.ExchangeRate.ToString("N2", inv));
            y = this.Row(y, $"Prima ({package.PercentageTotalAmount.ToString("N0", inv)} %)", this.Usd(downPayment));
            y = this.Row(y, $"{package.RemainingMonthlyInstallments} mensualidades de", this.Usd(monthlyPayment));

            y -= 20;
            this.Rect(Margin, y - 50, PageWidth - Margin * 2, 60, Primary);
            this.Text(Margin + 15, y - 8, "Total USD", 11, false, new double[] { 1, 1, 1 });
            this.Text(Margin + 15, y - 32, this.Usd(invoice.totalUsd), 18, true, Secondary);
            this.TextRight(PageWidth - Margin - 15, y - 8, "Total CRC", 11, false, new double[] { 1, 1, 1 });
            this.TextRight(PageWidth - Margin - 15, y - 32, "CRC " + invoice.totalColones.ToString("N2", inv), 18, true, Secondary);

            this.TextCenter(PageWidth / 2, 40, "Gracias por elegir Beach.SA · info@beachsa.com", 9, false, TextColor);

            return this.BuildDocument();
        }

        private string Usd(double value)
        {
            return "$" + value.ToString("N2", inv);
        }

        private double Section(double y, string title)
        {
            this.Text(Margin, y, title, 13, true, Primary);
            this.Line(Margin, y - 6, PageWidth - Margin, y - 6, LineColor);
            return y - 24;
        }

        private double Row(double y, string label, string value)
        {
            this.Text(Margin, y, label, 11, false, TextColor);
            this.TextRight(PageWidth - Margin, y, value ?? "", 11, false, TextColor);
            return y - 18;
        }

        private void Rect(double x, double y, double w, double h, double[] color)
        {
            this.content.AppendLine($"{this.Color(color)} rg {this.N(x)} {this.N(y)} {this.N(w)} {this.N(h)} re f");
        }

        private void Line(double x1, double y1, double x2, double y2, double[] color)
        {
            this.content.AppendLine($"{this.Color(color)} RG 1 w {this.N(x1)} {this.N(y1)} m {this.N(x2)} {this.N(y2)} l S");
        }

        private void Text(double x, double y, string text, double size, bool bold, double[] color)
        {
            string font = bold ? "F2" : "F1";
            this.content.AppendLine($"BT {this.Color(color)} rg /{font} {this.N(size)} Tf {this.N(x)} {this.N(y)} Td ({this.Escape(text)}) Tj ET");
        }

        private void TextRight(double right, double y, string text, double size, bool bold, double[] color)
        {
            this.Text(right - this.Width(text, size, bold), y, text, size, bold, color);
        }

        private void TextCenter(double center, double y, string text, double size, bool bold, double[] color)
        {
            this.Text(center - this.Width(text, size, bold) / 2, y, text, size, bold, color);
        }

        private double Width(string text, double size, bool bold)
        {
            double total = 0;

            foreach (char c in this.Clean(text))
            {
                total += c >= 32 && c <= 126 ? HelveticaWidths[c - 32] : 556;
            }

            return total * size / 1000 * (bold ? 1.06 : 1);
        }

        private string Clean(string text)
        {
            return (text ?? "").Replace("₡", "CRC ");
        }

        private string Escape(string text)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in this.Clean(text))
            {
                if (c == '(' || c == ')' || c == '\\')
                {
                    sb.Append('\\').Append(c);
                }
                else if (c < 128)
                {
                    sb.Append(c);
                }
                else if (c < 256)
                {
                    sb.Append('\\').Append(Convert.ToString(c, 8).PadLeft(3, '0'));
                }
                else
                {
                    sb.Append('?');
                }
            }

            return sb.ToString();
        }

        private string Color(double[] color)
        {
            return $"{this.N(color[0])} {this.N(color[1])} {this.N(color[2])}";
        }

        private string N(double value)
        {
            return value.ToString("0.###", inv);
        }

        private byte[] BuildDocument()
        {
            byte[] stream = Encoding.ASCII.GetBytes(this.content.ToString());

            List<string> objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {this.N(PageWidth)} {this.N(PageHeight)}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"
            };

            using MemoryStream output = new MemoryStream();
            List<long> offsets = new List<long>();

            void Write(string value)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value);
                output.Write(bytes, 0, bytes.Length);
            }

            Write("%PDF-1.4\n");

            for (int i = 0; i < objects.Count; i++)
            {
                offsets.Add(output.Position);
                Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }

            offsets.Add(output.Position);
            Write($"6 0 obj\n<< /Length {stream.Length} >>\nstream\n");
            output.Write(stream, 0, stream.Length);
            Write("\nendstream\nendobj\n");

            long xref = output.Position;
            Write($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");

            foreach (long offset in offsets)
            {
                Write($"{offset:D10} 00000 n \n");
            }

            Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");

            return output.ToArray();
        }
    }
}
