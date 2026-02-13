using FlowersShop.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowersShop.Forms
{
    public partial class SaleDetailForm : Form
    {
        private DatabaseHelper db;
        private int saleId;
        private Color primaryColor = Color.FromArgb(156, 39, 176);

        public SaleDetailForm(int saleId)
        {
            this.saleId = saleId;
            db = new DatabaseHelper();
            InitializeComponent();
            LoadSaleDetails();
        }

        private void InitializeComponent()
        {
            this.Text = $"Детали продажи #{saleId}";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = $"🧾 Детали продажи #{saleId}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            itemsDataGridView = new DataGridView
            {
                Location = new Point(0, 50),
                Size = new Size(540, 300),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var totalLabel = new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 360)
            };

            var closeButton = new Button
            {
                Text = "Закрыть",
                Size = new Size(100, 40),
                Location = new Point(440, 360),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(itemsDataGridView);
            mainPanel.Controls.Add(totalLabel);
            mainPanel.Controls.Add(closeButton);

            this.Controls.Add(mainPanel);
            this.totalLabel = totalLabel;
        }

        private DataGridView itemsDataGridView;
        private Label totalLabel;

        private void LoadSaleDetails()
        {
            var items = db.GetSaleItems(saleId);
            var sale = db.GetAllSales().Find(s => s.Id == saleId);

            if (sale != null)
            {
                this.Text = $"Детали продажи #{saleId} от {sale.SaleDate:dd.MM.yyyy HH:mm}";
            }

            itemsDataGridView.DataSource = null;
            itemsDataGridView.Columns.Clear();

            itemsDataGridView.DataSource = items.Select(i => new
            {
                Товар = i.ProductName,
                Количество = i.Quantity,
                Цена_за_ед = $"{i.UnitPrice:C}",  // Исправлено
                Сумма = $"{i.TotalPrice:C}"
            }).ToList();

            // Переименовываем колонки
            if (itemsDataGridView.Columns["Цена_за_ед"] != null)
                itemsDataGridView.Columns["Цена_за_ед"].HeaderText = "Цена за ед.";

            decimal total = items.Sum(i => i.TotalPrice);
            totalLabel.Text = $"Итого: {total:C}";

            itemsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            itemsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            itemsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }
    }
}