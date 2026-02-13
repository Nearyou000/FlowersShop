using FlowersShop.Data;
using FlowersShop.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FlowersShop.Forms
{
    public partial class ProductsForm : Form
    {
        private DatabaseHelper db;
        private List<Product> products;
        private Color primaryColor = Color.FromArgb(156, 39, 176);
        private Color secondaryColor = Color.FromArgb(233, 30, 99);
        private Color backgroundColor = Color.FromArgb(250, 250, 250);
        private DataGridView dataGridView;

        public ProductsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Каталог товаров";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = backgroundColor;

            // Инициализация DataGridView
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            // Панель поиска
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var searchBox = new TextBox
            {
                Text = "🔍 Поиск товаров...",
                Size = new Size(300, 35),
                Location = new Point(20, 17),
                Font = new Font("Segoe UI", 11),
                ForeColor = SystemColors.GrayText
            };

            // Обработчики для имитации placeholder
            searchBox.Enter += (s, e) =>
            {
                if (searchBox.Text == "🔍 Поиск товаров...")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = SystemColors.WindowText;
                }
            };

            searchBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "🔍 Поиск товаров...";
                    searchBox.ForeColor = SystemColors.GrayText;
                }
            };

            var searchButton = new Button
            {
                Text = "Найти",
                Size = new Size(100, 35),
                Location = new Point(330, 17),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += (s, e) => SearchProducts(GetSearchText(searchBox));

            searchBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    SearchProducts(GetSearchText(searchBox));
                }
            };

            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(searchButton);

            // Кнопки действий
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White
            };

            var addButton = new Button
            {
                Text = "➕ Добавить товар",
                Size = new Size(150, 40),
                Location = new Point(20, 10),
                BackColor = secondaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var editButton = new Button
            {
                Text = "✏️ Редактировать",
                Size = new Size(150, 40),
                Location = new Point(180, 10),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var deleteButton = new Button
            {
                Text = "🗑️ Удалить",
                Size = new Size(150, 40),
                Location = new Point(340, 10),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var refreshButton = new Button
            {
                Text = "🔄 Обновить",
                Size = new Size(150, 40),
                Location = new Point(500, 10),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            addButton.FlatAppearance.BorderSize = 0;
            editButton.FlatAppearance.BorderSize = 0;
            deleteButton.FlatAppearance.BorderSize = 0;
            refreshButton.FlatAppearance.BorderSize = 0;

            addButton.Click += (s, e) => AddProduct();
            editButton.Click += (s, e) => EditProduct();
            deleteButton.Click += (s, e) => DeleteProduct();
            refreshButton.Click += (s, e) => LoadProducts();

            buttonPanel.Controls.Add(addButton);
            buttonPanel.Controls.Add(editButton);
            buttonPanel.Controls.Add(deleteButton);
            buttonPanel.Controls.Add(refreshButton);

            // Добавление элементов на форму
            this.Controls.Add(dataGridView);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(searchPanel);

            // Загружаем данные
            LoadProducts();
        }

        private string GetSearchText(TextBox searchBox)
        {
            return searchBox.Text == "🔍 Поиск товаров..." ? "" : searchBox.Text;
        }

        private void LoadProducts()
        {
            try
            {
                db = new DatabaseHelper();
                products = db.GetAllProducts();

                dataGridView.DataSource = null;
                dataGridView.Columns.Clear();

                if (products.Count == 0)
                {
                    // Если товаров нет, выводим сообщение
                    dataGridView.DataSource = null;
                    return;
                }

                var displayList = products.Select(p => new
                {
                    p.Id,
                    Название = p.Name,
                    Категория = p.Category,
                    Цена = $"{p.Price:C}",
                    Количество = p.StockQuantity,           // Исправлено: НаСкладе -> Количество
                    Дата_добавления = p.CreatedDate.ToString("dd.MM.yyyy")  // Исправлено: ДатаДобавления -> Дата_добавления
                }).ToList();

                dataGridView.DataSource = displayList;

                // Переименовываем колонки для отображения с пробелами
                if (dataGridView.Columns.Contains("Количество"))
                    dataGridView.Columns["Количество"].HeaderText = "На складе";

                if (dataGridView.Columns.Contains("Дата_добавления"))
                    dataGridView.Columns["Дата_добавления"].HeaderText = "Дата добавления";

                if (dataGridView.Columns.Contains("Название"))
                    dataGridView.Columns["Название"].Width = 200;

                if (dataGridView.Columns.Contains("Id"))
                    dataGridView.Columns["Id"].Visible = false;

                dataGridView.RowHeadersVisible = false;
                dataGridView.EnableHeadersVisualStyles = false;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LoadProducts();
                return;
            }

            var filteredProducts = db.SearchProducts(searchTerm);

            dataGridView.DataSource = null;
            dataGridView.Columns.Clear();

            var displayList = filteredProducts.Select(p => new
            {
                p.Id,
                Название = p.Name,
                Категория = p.Category,
                Цена = $"{p.Price:C}",
                Количество = p.StockQuantity,           // Исправлено
                Дата_добавления = p.CreatedDate.ToString("dd.MM.yyyy")  // Исправлено
            }).ToList();

            dataGridView.DataSource = displayList;

            // Переименовываем колонки
            if (dataGridView.Columns.Contains("Количество"))
                dataGridView.Columns["Количество"].HeaderText = "На складе";

            if (dataGridView.Columns.Contains("Дата_добавления"))
                dataGridView.Columns["Дата_добавления"].HeaderText = "Дата добавления";

            if (dataGridView.Columns.Contains("Id"))
                dataGridView.Columns["Id"].Visible = false;
        }

        private void AddProduct()
        {
            var productForm = new ProductForm();
            if (productForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void EditProduct()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
                var productForm = new ProductForm(id);
                if (productForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteProduct()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var id = (int)dataGridView.SelectedRows[0].Cells["Id"].Value;
                var productName = dataGridView.SelectedRows[0].Cells["Название"].Value.ToString();

                var result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{productName}'?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    db.DeleteProduct(id);
                    LoadProducts();
                    MessageBox.Show("Товар успешно удален", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}