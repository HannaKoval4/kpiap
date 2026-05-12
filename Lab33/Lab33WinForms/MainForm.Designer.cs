namespace Lab33WinForms;

public partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private DataGridView dataGridView1;
    private TextBox textBoxSearch;
    private Button buttonSave;
    private Button buttonSearch;
    private Button buttonAuth;
    private Button buttonRegister;
    private Label labelSearch;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        dataGridView1 = new DataGridView();
        textBoxSearch = new TextBox();
        buttonSave = new Button();
        buttonSearch = new Button();
        buttonAuth = new Button();
        buttonRegister = new Button();
        labelSearch = new Label();
        ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
        SuspendLayout();

        labelSearch.AutoSize = true;
        labelSearch.Location = new Point(12, 15);
        labelSearch.Text = "Поиск (название / описание):";

        textBoxSearch.Location = new Point(12, 35);
        textBoxSearch.Size = new Size(420, 27);
        textBoxSearch.TabIndex = 0;
        textBoxSearch.TextChanged += textBoxSearch_TextChanged;

        buttonSearch.Location = new Point(450, 33);
        buttonSearch.Size = new Size(100, 29);
        buttonSearch.Text = "Найти";
        buttonSearch.UseVisualStyleBackColor = true;
        buttonSearch.Click += buttonSearch_Click;

        buttonSave.Location = new Point(560, 33);
        buttonSave.Size = new Size(110, 29);
        buttonSave.Text = "Сохранить";
        buttonSave.UseVisualStyleBackColor = true;
        buttonSave.Click += buttonSave_Click;

        buttonAuth.Location = new Point(690, 33);
        buttonAuth.Size = new Size(130, 29);
        buttonAuth.Text = "Авторизация";
        buttonAuth.UseVisualStyleBackColor = true;
        buttonAuth.Click += buttonAuth_Click;

        buttonRegister.Location = new Point(830, 33);
        buttonRegister.Size = new Size(130, 29);
        buttonRegister.Text = "Регистрация";
        buttonRegister.UseVisualStyleBackColor = true;
        buttonRegister.Click += buttonRegister_Click;

        dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridView1.Location = new Point(12, 75);
        dataGridView1.Size = new Size(948, 420);
        dataGridView1.TabIndex = 1;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridView1.AllowUserToAddRows = true;
        dataGridView1.AllowUserToDeleteRows = true;

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(972, 507);
        Controls.Add(dataGridView1);
        Controls.Add(buttonRegister);
        Controls.Add(buttonAuth);
        Controls.Add(buttonSave);
        Controls.Add(buttonSearch);
        Controls.Add(textBoxSearch);
        Controls.Add(labelSearch);
        MinimumSize = new Size(700, 400);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Лаб. 33 на основе лаб. 29 — записи и многопользовательский доступ";
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
