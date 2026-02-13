using FlowersShop.Data;
using System;
using System.Drawing;
using System.Windows.Forms;
using FlowersShop.Forms;

namespace FlowersShop.Forms
{
    public partial class MainForm : Form
    {
        private Color primaryColor = Color.FromArgb(156, 39, 176); // Фиолетовый
        private Color secondaryColor = Color.FromArgb(233, 30, 99); // Розовый
        private Color accentColor = Color.FromArgb(103, 58, 183); // Темно-фиолетовый
        private Color backgroundColor = Color.FromArgb(250, 250, 250); // Светло-серый
        private Color textColor = Color.FromArgb(33, 33, 33); // Темно-серый

        // Определяем класс для кнопок меню
        private class MenuButtonInfo
        {
            public string Text { get; set; }
            public Type FormType { get; set; }
            public bool IsExitButton { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            ApplyColors();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройки формы
            this.Text = "Цветочный магазин - Главная";
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = backgroundColor;
            this.Name = "MainForm";

            // Создаем заголовок
            var titleLabel = new Label
            {
                Text = "Цветочный магазин",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(30, 20),
                Size = new Size(400, 50),
                ForeColor = primaryColor
            };

            // Создаем панель с кнопками меню
            var menuPanel = CreateMenuPanel();

            // Создаем панель со статистикой
            var statsPanel = CreateStatsPanel();

            // Добавляем элементы на форму
            this.Controls.Add(titleLabel);
            this.Controls.Add(menuPanel);
            this.Controls.Add(statsPanel);

            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }

        private Panel CreateMenuPanel()
        {
            var panel = new Panel
            {
                Size = new Size(200, 400),
                Location = new Point(770, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var menuItems = new[]
            {
                new MenuButtonInfo { Text = "Товары", FormType = typeof(ProductsForm) },
                new MenuButtonInfo { Text = "Добавить товар", FormType = typeof(ProductForm) },
                new MenuButtonInfo { Text = "Корзина", FormType = typeof(CartForm) },
                new MenuButtonInfo { Text = "Продажи", FormType = typeof(SalesForm) },
                new MenuButtonInfo { Text = "Отчеты", FormType = typeof(ReportsForm) },
                new MenuButtonInfo { Text = "Выход", IsExitButton = true }
            };

            int yPos = 20;
            foreach (var item in menuItems)
            {
                var button = new Button
                {
                    Text = item.Text,
                    Size = new Size(160, 40),
                    Location = new Point(20, yPos),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = item.IsExitButton ? secondaryColor : primaryColor,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Tag = item,
                    Cursor = Cursors.Hand
                };

                button.FlatAppearance.BorderSize = 0;

                if (item.IsExitButton)
                {
                    button.Click += (s, e) => Application.Exit();
                }
                else
                {
                    button.Click += MenuButton_Click;
                }

                panel.Controls.Add(button);
                yPos += 50;
            }

            return panel;
        }

        private Panel CreateStatsPanel()
        {
            var panel = new Panel
            {
                Size = new Size(720, 400),
                Location = new Point(30, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            try
            {
                var db = new DatabaseHelper();

                // Получаем данные
                var totalProducts = db.GetAllProducts().Count;
                var totalRevenue = db.GetTotalRevenue();
                var lowStockCount = db.GetLowStockProducts(5).Count;

                var statsData = new[]
                {
                    new { Title = "Всего товаров", Value = totalProducts.ToString(), Icon = "📦", Color = primaryColor },
                    new { Title = "Общая выручка", Value = $"{totalRevenue:C}", Icon = "💰", Color = secondaryColor },
                    new { Title = "Мало на складе", Value = lowStockCount.ToString(), Icon = "⚠️", Color = Color.Orange }
                };

                int yPos = 30;
                foreach (var stat in statsData)
                {
                    var statPanel = new Panel
                    {
                        Size = new Size(650, 100),
                        Location = new Point(35, yPos),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var iconLabel = new Label
                    {
                        Text = stat.Icon,
                        Font = new Font("Segoe UI", 32),
                        Location = new Point(20, 20),
                        Size = new Size(60, 60),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var titleLabel = new Label
                    {
                        Text = stat.Title,
                        Font = new Font("Segoe UI", 14),
                        Location = new Point(100, 20),
                        Size = new Size(200, 30),
                        ForeColor = textColor
                    };

                    var valueLabel = new Label
                    {
                        Text = stat.Value,
                        Font = new Font("Segoe UI", 24, FontStyle.Bold),
                        Location = new Point(100, 50),
                        Size = new Size(300, 40),
                        ForeColor = stat.Color
                    };

                    statPanel.Controls.Add(iconLabel);
                    statPanel.Controls.Add(titleLabel);
                    statPanel.Controls.Add(valueLabel);
                    panel.Controls.Add(statPanel);

                    yPos += 120;
                }
            }
            catch (Exception ex)
            {
                // Если не удалось загрузить статистику, показываем сообщение
                var errorLabel = new Label
                {
                    Text = $"Не удалось загрузить статистику: {ex.Message}",
                    Location = new Point(35, 30),
                    Size = new Size(650, 100),
                    ForeColor = Color.Red,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panel.Controls.Add(errorLabel);
            }

            return panel;
        }

        private void ApplyColors()
        {
            this.ForeColor = textColor;
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var menuItem = (MenuButtonInfo)button.Tag;

            if (menuItem.FormType != null)
            {
                try
                {
                    Form form = null;

                    // Для каждой формы свой конструктор
                    if (menuItem.FormType == typeof(ProductsForm))
                    {
                        form = new ProductsForm();
                    }
                    else if (menuItem.FormType == typeof(ProductForm))
                    {
                        form = new ProductForm(); // Без параметров для добавления
                    }
                    else if (menuItem.FormType == typeof(CartForm))
                    {
                        form = new CartForm();
                    }
                    else if (menuItem.FormType == typeof(SalesForm))
                    {
                        form = new SalesForm();
                    }
                    else if (menuItem.FormType == typeof(ReportsForm))
                    {
                        form = new ReportsForm();
                    }

                    if (form != null)
                    {
                        form.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Здесь можно добавить код, который должен выполниться при загрузке формы
        }
    }
}