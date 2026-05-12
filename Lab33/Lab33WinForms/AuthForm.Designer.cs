namespace Lab33WinForms;

public partial class AuthForm
{
    private System.ComponentModel.IContainer components = null!;

    private TextBox textBoxLogin;
    private TextBox textBoxPassword;
    private Button buttonOk;
    private Button buttonClear;
    private Label labelLogin;
    private Label labelPassword;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        textBoxLogin = new TextBox();
        textBoxPassword = new TextBox();
        buttonOk = new Button();
        buttonClear = new Button();
        labelLogin = new Label();
        labelPassword = new Label();
        SuspendLayout();

        labelLogin.AutoSize = true;
        labelLogin.Location = new Point(14, 18);
        labelLogin.Text = "Логин *:";

        textBoxLogin.Location = new Point(14, 40);
        textBoxLogin.MaxLength = 50;
        textBoxLogin.Size = new Size(320, 27);
        textBoxLogin.TabIndex = 0;

        labelPassword.AutoSize = true;
        labelPassword.Location = new Point(14, 78);
        labelPassword.Text = "Пароль *:";

        textBoxPassword.Location = new Point(14, 100);
        textBoxPassword.MaxLength = 50;
        textBoxPassword.Size = new Size(320, 27);
        textBoxPassword.TabIndex = 1;
        textBoxPassword.UseSystemPasswordChar = true;

        buttonOk.Location = new Point(14, 145);
        buttonOk.Size = new Size(150, 32);
        buttonOk.Text = "Войти";
        buttonOk.UseVisualStyleBackColor = true;
        buttonOk.Click += buttonOk_Click;

        buttonClear.Location = new Point(184, 145);
        buttonClear.Size = new Size(150, 32);
        buttonClear.Text = "Очистить";
        buttonClear.UseVisualStyleBackColor = true;
        buttonClear.Click += buttonClear_Click;

        AcceptButton = buttonOk;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(352, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Controls.Add(buttonClear);
        Controls.Add(buttonOk);
        Controls.Add(textBoxPassword);
        Controls.Add(labelPassword);
        Controls.Add(textBoxLogin);
        Controls.Add(labelLogin);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Авторизация";
        Load += AuthForm_Load;
        ResumeLayout(false);
        PerformLayout();
    }
}
