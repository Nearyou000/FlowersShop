using FlowersShop.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowersShop.Forms
{
    public partial class SalesForm : Form
    {
        private DatabaseHelper db;
        private Color primaryColor = Color.FromArgb(156, 39, 176);

        public SalesForm()
        {
            InitializeComponent();
            LoadSales();
        }

        private void InitializeComponent()
        {
            this.Text = "💰 История продаж";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "💰 История продаж",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            salesDataGridView = new DataGridView
            {
                Location = new Point(0, 50),
                Size = new Size(840, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Кнопки
            var buttonPanel = new Panel
            {
                Size = new Size(840, 50),
                Location = new Point(0, 460)
            };

            var viewDetailsButton = new Button
            {
                Text = "👁️ Просмотреть детали",
                Size = new Size(180, 40),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var refreshButton = new Button
            {
                Text = "🔄 Обновить",
                Size = new Size(150, 40),
                Location = new Point(190, 0),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var exportButton = new Button
            {
                Text = "📁 Экспорт в CSV",
                Size = new Size(150, 40),
                Location = new Point(350, 0),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            viewDetailsButton.FlatAppearance.BorderSize = 0;
            refreshButton.FlatAppearance.BorderSize = 0;
            exportButton.FlatAppearance.BorderSize = 0;

            viewDetailsButton.Click += ViewDetailsButton_Click;
            refreshButton.Click += (s, e) => LoadSales();
            exportButton.Click += ExportButton_Click;

            buttonPanel.Controls.Add(viewDetailsButton);
            buttonPanel.Controls.Add(refreshButton);
            buttonPanel.Controls.Add(exportButton);

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(salesDataGridView);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private DataGridView salesDataGridView;

        private void LoadSales()
        {
            db = new DatabaseHelper();
            var sales = db.GetAllSales();

            salesDataGridView.DataSource = null;
            salesDataGridView.Columns.Clear();

            salesDataGridView.DataSource = sales.Select(s => new
            {
                s.Id,
                Дата = s.SaleDate.ToString("dd.MM.yyyy HH:mm"),
                Количество_товаров = s.ItemCount,  // Исправлено: с пробелом
                Общая_сумма = $"{s.TotalAmount:C}"   // Исправлено: с пробелом
            }).ToList();

            // Переименовываем колонки для отображения с пробелами
            if (salesDataGridView.Columns["Количество_товаров"] != null)
                salesDataGridView.Columns["Количество_товаров"].HeaderText = "Количество товаров";

            if (salesDataGridView.Columns["Общая_сумма"] != null)
                salesDataGridView.Columns["Общая_сумма"].HeaderText = "Общая сумма";

            salesDataGridView.Columns["Id"].Visible = false;
            salesDataGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            salesDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            salesDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void ViewDetailsButton_Click(object sender, EventArgs e)
        {
            if (salesDataGridView.SelectedRows.Count > 0)
            {
                var saleId = (int)salesDataGridView.SelectedRows[0].Cells["Id"].Value;
                var detailForm = new SaleDetailForm(saleId);
                detailForm.ShowDialog();
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                saveDialog.FileName = $"Продажи_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        db.ExportSalesToCsv(saveDialog.FileName);
                        MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveDialog.FileName}",
                            "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}