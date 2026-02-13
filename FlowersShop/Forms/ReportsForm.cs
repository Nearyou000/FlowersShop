using FlowersShop.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowersShop.Forms
{
    public partial class ReportsForm : Form
    {
        private DatabaseHelper db;
        private Color primaryColor = Color.FromArgb(156, 39, 176);
        private Color secondaryColor = Color.FromArgb(233, 30, 99);

        public ReportsForm()
        {
            InitializeComponent();
            LoadReports();
        }

        private void InitializeComponent()
        {
            this.Text = "📊 Отчеты и аналитика";
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
                Text = "📊 Отчеты и аналитика",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Панель статистики
            var statsPanel = new Panel
            {
                Size = new Size(840, 150),
                Location = new Point(0, 50),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            revenueLabel = new Label
            {
                Font = new Font("Segoe UI", 14),
                Location = new Point(20, 20),
                AutoSize = true
            };

            totalSalesLabel = new Label
            {
                Font = new Font("Segoe UI", 14),
                Location = new Point(20, 50),
                AutoSize = true
            };

            avgSaleLabel = new Label
            {
                Font = new Font("Segoe UI", 14),
                Location = new Point(20, 80),
                AutoSize = true
            };

            statsPanel.Controls.Add(revenueLabel);
            statsPanel.Controls.Add(totalSalesLabel);
            statsPanel.Controls.Add(avgSaleLabel);

            // Таблица товаров с малым запасом
            var lowStockLabel = new Label
            {
                Text = "⚠️ Товары с малым запасом",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(0, 220),
                AutoSize = true
            };

            lowStockDataGridView = new DataGridView
            {
                Location = new Point(0, 250),
                Size = new Size(840, 200),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var refreshButton = new Button
            {
                Text = "🔄 Обновить отчеты",
                Size = new Size(200, 40),
                Location = new Point(0, 460),
                BackColor = secondaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var exportButton = new Button
            {
                Text = "📁 Экспорт всех данных",
                Size = new Size(200, 40),
                Location = new Point(210, 460),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            refreshButton.FlatAppearance.BorderSize = 0;
            exportButton.FlatAppearance.BorderSize = 0;

            refreshButton.Click += (s, e) => LoadReports();
            exportButton.Click += ExportButton_Click;

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(statsPanel);
            mainPanel.Controls.Add(lowStockLabel);
            mainPanel.Controls.Add(lowStockDataGridView);
            mainPanel.Controls.Add(refreshButton);
            mainPanel.Controls.Add(exportButton);

            this.Controls.Add(mainPanel);
        }

        private Label revenueLabel;
        private Label totalSalesLabel;
        private Label avgSaleLabel;
        private DataGridView lowStockDataGridView;

        private void LoadReports()
        {
            db = new DatabaseHelper();

            // Общая статистика
            var totalRevenue = db.GetTotalRevenue();
            var allSales = db.GetAllSales();
            var totalSales = allSales.Count;
            var avgSale = totalSales > 0 ? totalRevenue / totalSales : 0;

            revenueLabel.Text = $"💰 Общая выручка: {totalRevenue:C}";
            totalSalesLabel.Text = $"📈 Всего продаж: {totalSales}";
            avgSaleLabel.Text = $"📊 Средний чек: {avgSale:C}";

            // Товары с малым запасом
            var lowStockProducts = db.GetLowStockProducts(10);

            lowStockDataGridView.DataSource = null;
            lowStockDataGridView.Columns.Clear();

            lowStockDataGridView.DataSource = lowStockProducts.Select(p => new
            {
                Название = p.Name,
                Категория = p.Category,
                На_складе = p.StockQuantity,  // Исправлено
                Минимальный_запас = 10,        // Исправлено
                Статус = p.StockQuantity <= 5 ? "КРИТИЧЕСКИ" : "НИЗКИЙ"
            }).ToList();

            // Переименовываем колонки
            if (lowStockDataGridView.Columns["На_складе"] != null)
                lowStockDataGridView.Columns["На_складе"].HeaderText = "На складе";

            if (lowStockDataGridView.Columns["Минимальный_запас"] != null)
                lowStockDataGridView.Columns["Минимальный_запас"].HeaderText = "Минимальный запас";

            // Настройка стилей
            lowStockDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.OrangeRed;
            lowStockDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            lowStockDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Подсветка критически низких остатков
            lowStockDataGridView.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == lowStockDataGridView.Columns["Статус"].Index && e.Value != null)
                {
                    if (e.Value.ToString() == "КРИТИЧЕСКИ")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(lowStockDataGridView.Font, FontStyle.Bold);
                    }
                }
            };
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv";
                saveDialog.FileName = $"Отчет_цветочный_магазин_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Экспорт продаж
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