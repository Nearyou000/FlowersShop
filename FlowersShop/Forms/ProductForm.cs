using FlowersShop.Data;
using FlowersShop.Data.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlowersShop.Forms
{
    public partial class ProductForm : Form
    {
        private DatabaseHelper db;
        private Product product;
        private bool isEditMode;
        private Color primaryColor = Color.FromArgb(156, 39, 176);
        private ComboBox categoryComboBox; // Добавляем поле для ComboBox

        public ProductForm(int productId = 0)
        {
            db = new DatabaseHelper();
            isEditMode = productId > 0;

            if (isEditMode)
            {
                product = db.GetProductById(productId);
                if (product == null)
                {
                    MessageBox.Show("Товар не найден", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
            }
            else
            {
                product = new Product();
            }

            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            // Устанавливаем размер и свойства формы
            this.ClientSize = new System.Drawing.Size(500, 420);
            this.Name = "ProductForm";
            this.Text = isEditMode ? "Редактирование товара" : "Новый товар";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем панель для содержимого
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Создаем элементы управления формы
            CreateFormControls(mainPanel);

            // Добавляем панель на форму
            this.Controls.Add(mainPanel);

            this.Load += new System.EventHandler(this.ProductForm_Load);
        }

        private void CreateFormControls(Panel mainPanel)
        {
            int yPos = 20;

            // Заголовок
            var titleLabel = new Label
            {
                Text = isEditMode ? "✏️ Редактирование товара" : "➕ Новый товар",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                Location = new Point(0, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(titleLabel);
            yPos += 40;

            // Название
            var nameLabel = new Label
            {
                Text = "Название:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(0, yPos),
                Size = new Size(100, 30)
            };

            var nameTextBox = new TextBox
            {
                Location = new Point(120, yPos - 3),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11),
                Text = isEditMode ? product.Name : ""
            };
            mainPanel.Controls.Add(nameLabel);
            mainPanel.Controls.Add(nameTextBox);
            yPos += 40;

            // Категория
            var categoryLabel = new Label
            {
                Text = "Категория:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(0, yPos),
                Size = new Size(100, 30)
            };

            categoryComboBox = new ComboBox
            {
                Location = new Point(120, yPos - 3),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(categoryLabel);
            mainPanel.Controls.Add(categoryComboBox);
            yPos += 40;

            // Цена
            var priceLabel = new Label
            {
                Text = "Цена:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(0, yPos),
                Size = new Size(100, 30)
            };

            var priceNumeric = new NumericUpDown
            {
                Location = new Point(120, yPos - 3),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11),
                Minimum = 0,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Value = isEditMode ? product.Price : 0
            };
            mainPanel.Controls.Add(priceLabel);
            mainPanel.Controls.Add(priceNumeric);
            yPos += 40;

            // Количество
            var quantityLabel = new Label
            {
                Text = "Количество:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(0, yPos),
                Size = new Size(100, 30)
            };

            var quantityNumeric = new NumericUpDown
            {
                Location = new Point(120, yPos - 3),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11),
                Minimum = 0,
                Maximum = 10000,
                Value = isEditMode ? product.StockQuantity : 0
            };
            mainPanel.Controls.Add(quantityLabel);
            mainPanel.Controls.Add(quantityNumeric);
            yPos += 40;

            // Описание
            var descriptionLabel = new Label
            {
                Text = "Описание:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(0, yPos),
                Size = new Size(100, 30)
            };

            var descriptionTextBox = new TextBox
            {
                Location = new Point(120, yPos - 3),
                Size = new Size(300, 100),
                Font = new Font("Segoe UI", 11),
                Multiline = true,
                Text = isEditMode ? product.Description : ""
            };
            mainPanel.Controls.Add(descriptionLabel);
            mainPanel.Controls.Add(descriptionTextBox);
            yPos += 120;

            // Кнопки
            var saveButton = new Button
            {
                Text = "💾 Сохранить",
                Size = new Size(120, 35),
                Location = new Point(150, yPos),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var cancelButton = new Button
            {
                Text = "❌ Отмена",
                Size = new Size(120, 35),
                Location = new Point(280, yPos),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            saveButton.FlatAppearance.BorderSize = 0;
            cancelButton.FlatAppearance.BorderSize = 0;

            // Сохраняем ссылки на элементы в Tag для обработки
            saveButton.Tag = new { nameTextBox, categoryComboBox, priceNumeric, quantityNumeric, descriptionTextBox };
            saveButton.Click += SaveButton_Click;

            cancelButton.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(saveButton);
            mainPanel.Controls.Add(cancelButton);
        }

        private void LoadCategories()
        {
            // Простая загрузка категорий в ComboBox
            string[] categories = { "Букеты", "Горшечные растения", "Срезанные цветы", "Подарочные наборы", "Семена", "Инвентарь" };

            if (categoryComboBox != null)
            {
                categoryComboBox.Items.AddRange(categories);

                // Устанавливаем значение, если редактируем существующий товар
                if (isEditMode && !string.IsNullOrEmpty(product.Category))
                {
                    categoryComboBox.Text = product.Category;
                }
                else if (categoryComboBox.Items.Count > 0)
                {
                    categoryComboBox.SelectedIndex = 0;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var controls = button.Tag as dynamic;

            // Получаем значения из полей
            product.Name = controls.nameTextBox.Text.Trim();
            product.Category = controls.categoryComboBox.Text;
            product.Price = controls.priceNumeric.Value;
            product.StockQuantity = (int)controls.quantityNumeric.Value;
            product.Description = controls.descriptionTextBox.Text.Trim();

            // Валидация
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                MessageBox.Show("Выберите категорию товара", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    db.UpdateProduct(product);
                    MessageBox.Show("Товар успешно обновлен", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    db.AddProduct(product);
                    MessageBox.Show("Товар успешно добавлен", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProductForm_Load(object sender, EventArgs e)
        {
            // Метод загрузки формы
        }
    }
}