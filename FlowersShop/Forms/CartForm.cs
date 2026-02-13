using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlowersShop.Data;
using FlowersShop.Data.Models;

namespace FlowersShop.Forms
{
    public partial class CartForm : Form
    {
        private DatabaseHelper db;
        private List<CartItem> cartItems;
        private Color primaryColor = Color.FromArgb(156, 39, 176);
        private Color secondaryColor = Color.FromArgb(233, 30, 99);

        public class CartItem
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice => Product.Price * Quantity;
        }

        public CartForm()
        {
            db = new DatabaseHelper();
            cartItems = new List<CartItem>();
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "🛒 Корзина покупок";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Разделение на две панели
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 600
            };

            // Левая панель - список товаров
            var productsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var productsLabel = new Label
            {
                Text = "📦 Доступные товары",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            productsDataGridView = new DataGridView
            {
                Location = new Point(0, 40),
                Size = new Size(560, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var addToCartButton = new Button
            {
                Text = "➕ Добавить в корзину",
                Size = new Size(200, 40),
                Location = new Point(0, 450),
                BackColor = secondaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            addToCartButton.FlatAppearance.BorderSize = 0;
            addToCartButton.Click += AddToCartButton_Click;

            productsPanel.Controls.Add(productsLabel);
            productsPanel.Controls.Add(productsDataGridView);
            productsPanel.Controls.Add(addToCartButton);

            // Правая панель - корзина
            var cartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(20)
            };

            var cartLabel = new Label
            {
                Text = "🛒 Корзина",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            cartDataGridView = new DataGridView
            {
                Location = new Point(0, 40),
                Size = new Size(340, 300),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            totalLabel = new Label
            {
                Text = "Итого: 0 руб.",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = primaryColor,
                AutoSize = true,
                Location = new Point(0, 350)
            };

            var removeButton = new Button
            {
                Text = "➖ Удалить из корзины",
                Size = new Size(200, 40),
                Location = new Point(0, 390),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var checkoutButton = new Button
            {
                Text = "💰 Оформить продажу",
                Size = new Size(200, 40),
                Location = new Point(0, 440),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            removeButton.FlatAppearance.BorderSize = 0;
            checkoutButton.FlatAppearance.BorderSize = 0;

            removeButton.Click += RemoveButton_Click;
            checkoutButton.Click += CheckoutButton_Click;

            cartPanel.Controls.Add(cartLabel);
            cartPanel.Controls.Add(cartDataGridView);
            cartPanel.Controls.Add(totalLabel);
            cartPanel.Controls.Add(removeButton);
            cartPanel.Controls.Add(checkoutButton);

            splitContainer.Panel1.Controls.Add(productsPanel);
            splitContainer.Panel2.Controls.Add(cartPanel);

            this.Controls.Add(splitContainer);
        }

        private DataGridView productsDataGridView;
        private DataGridView cartDataGridView;
        private Label totalLabel;

        private void LoadProducts()
        {
            var products = db.GetAllProducts();

            productsDataGridView.DataSource = null;
            productsDataGridView.Columns.Clear();

            productsDataGridView.DataSource = products.Select(p => new
            {
                p.Id,
                Название = p.Name,
                Категория = p.Category,
                Цена = $"{p.Price:C}",
                На_складе = p.StockQuantity  // Исправлено
            }).ToList();

            // Переименовываем колонку
            if (productsDataGridView.Columns["На_складе"] != null)
                productsDataGridView.Columns["На_складе"].HeaderText = "На складе";

            productsDataGridView.Columns["Id"].Visible = false;
            productsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            productsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            productsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void AddToCartButton_Click(object sender, EventArgs e)
        {
            if (productsDataGridView.SelectedRows.Count > 0)
            {
                var id = (int)productsDataGridView.SelectedRows[0].Cells["Id"].Value;
                var product = db.GetProductById(id);

                if (product.StockQuantity <= 0)
                {
                    MessageBox.Show("Товара нет в наличии", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Запрос количества
                using (var quantityForm = new Form())
                {
                    quantityForm.Text = "Выберите количество";
                    quantityForm.Size = new Size(300, 150);
                    quantityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    quantityForm.StartPosition = FormStartPosition.CenterParent;

                    var numeric = new NumericUpDown
                    {
                        Minimum = 1,
                        Maximum = product.StockQuantity,
                        Value = 1,
                        Size = new Size(100, 30),
                        Location = new Point(100, 30),
                        Font = new Font("Segoe UI", 12)
                    };

                    var okButton = new Button
                    {
                        Text = "OK",
                        DialogResult = DialogResult.OK,
                        Size = new Size(80, 30),
                        Location = new Point(110, 70)
                    };

                    quantityForm.Controls.Add(new Label
                    {
                        Text = "Количество:",
                        Location = new Point(20, 35),
                        Size = new Size(80, 30)
                    });
                    quantityForm.Controls.Add(numeric);
                    quantityForm.Controls.Add(okButton);

                    if (quantityForm.ShowDialog() == DialogResult.OK)
                    {
                        var existingItem = cartItems.FirstOrDefault(item => item.Product.Id == id);
                        if (existingItem != null)
                        {
                            existingItem.Quantity += (int)numeric.Value;
                        }
                        else
                        {
                            cartItems.Add(new CartItem
                            {
                                Product = product,
                                Quantity = (int)numeric.Value
                            });
                        }

                        UpdateCartDisplay();
                    }
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (cartDataGridView.SelectedRows.Count > 0)
            {
                var productName = cartDataGridView.SelectedRows[0].Cells["Товар"].Value.ToString();
                var item = cartItems.FirstOrDefault(i => i.Product.Name == productName);

                if (item != null)
                {
                    cartItems.Remove(item);
                    UpdateCartDisplay();
                }
            }
        }

        private void CheckoutButton_Click(object sender, EventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Оформить продажу на сумму {cartItems.Sum(i => i.TotalPrice):C}?",
                "Подтверждение продажи", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var sale = new Sale
                    {
                        SaleDate = DateTime.Now,
                        TotalAmount = cartItems.Sum(i => i.TotalPrice),
                        ItemCount = cartItems.Sum(i => i.Quantity)
                    };

                    var saleItems = cartItems.Select(i => new SaleItem
                    {
                        ProductId = i.Product.Id,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.Price,
                        TotalPrice = i.TotalPrice
                    }).ToList();

                    db.CreateSale(sale, saleItems);

                    MessageBox.Show($"Продажа оформлена успешно!\nНомер чека: {sale.Id}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    cartItems.Clear();
                    UpdateCartDisplay();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при оформлении продажи: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateCartDisplay()
        {
            cartDataGridView.DataSource = null;
            cartDataGridView.Columns.Clear();

            cartDataGridView.DataSource = cartItems.Select(item => new
            {
                Товар = item.Product.Name,
                Количество = item.Quantity,
                Цена = $"{item.Product.Price:C}",
                Сумма = $"{item.TotalPrice:C}"
            }).ToList();

            decimal total = cartItems.Sum(item => item.TotalPrice);
            totalLabel.Text = $"Итого: {total:C}";

            cartDataGridView.ColumnHeadersDefaultCellStyle.BackColor = secondaryColor;
            cartDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            cartDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }
    }
}