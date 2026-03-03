using System;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;

public class User
{
    public string Login { get; private set; }
    public string Password { get; private set; }
    public bool IsAdmin { get; private set; }

    public User(string login, string password, bool isAdmin = false)
    {
        Login = login;
        Password = password;
        IsAdmin = isAdmin;
    }
}
public class Singleton
{
    private static Singleton _instance;
    private readonly string _dbPath = "users.db";

    public User CurrentUser { get; private set; }

    public static Singleton GetInstance()
    {
        if (_instance == null)
            _instance = new Singleton();
        return _instance;
    }

    private Singleton()
    {
        InitDatabase();
    }

    private void InitDatabase()
    {
        if (!File.Exists(_dbPath))
            SQLiteConnection.CreateFile(_dbPath);

        using (var conn = new SQLiteConnection($"Data Source={_dbPath}"))
        {
            conn.Open();
            string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    IsAdmin BOOLEAN NOT NULL
                )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();
        }
    }

    public bool ValidateLogin(string login, string password, bool isAdmin)
    {
        using (var conn = new SQLiteConnection($"Data Source={_dbPath}"))
        {
            conn.Open();
            var cmd = new SQLiteCommand(@"
                SELECT COUNT(*) FROM Users 
                WHERE Login = @login AND Password = @pass AND IsAdmin = @admin", conn);

            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@pass", password);
            cmd.Parameters.AddWithValue("@admin", isAdmin);

            bool success = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

            if (success)
                CurrentUser = new User(login, password, isAdmin);

            return success;
        }
    }

    public bool RegisterUser(string login, string password)
        => Register(login, password, false);

    public bool RegisterAdmin(string login, string password)
        => Register(login, password, true);

    private bool Register(string login, string password, bool isAdmin)
    {
        using (var conn = new SQLiteConnection($"Data Source={_dbPath}"))
        {
            conn.Open();

            var check = new SQLiteCommand(
                "SELECT COUNT(*) FROM Users WHERE Login=@login", conn);
            check.Parameters.AddWithValue("@login", login);

            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                return false;

            var insert = new SQLiteCommand(
                "INSERT INTO Users (Login, Password, IsAdmin) VALUES (@login,@pass,@admin)", conn);

            insert.Parameters.AddWithValue("@login", login);
            insert.Parameters.AddWithValue("@pass", password);
            insert.Parameters.AddWithValue("@admin", isAdmin);

            insert.ExecuteNonQuery();
            return true;
        }
    }

    public bool DeleteCurrentUser()
    {
        if (CurrentUser == null)
            return false;

        using (var conn = new SQLiteConnection($"Data Source={_dbPath}"))
        {
            conn.Open();
            var cmd = new SQLiteCommand(
                "DELETE FROM Users WHERE Login=@login", conn);

            cmd.Parameters.AddWithValue("@login", CurrentUser.Login);
            bool deleted = cmd.ExecuteNonQuery() > 0;

            if (deleted)
                CurrentUser = null;

            return deleted;
        }
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
public class MainView : Form
{
    private static readonly Random _random = new Random();
    private static readonly string[] _roles =
    {
        "Блатные", "Мужики", "Барыги", "Козлы",
        "Черти", "Фуфлыжники", "Петухи", "Крысы"
    };

    private Label lblRole;
    private Label lblStatus;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Главное меню";
        this.Size = new System.Drawing.Size(350, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        var currentUser = Singleton.GetInstance().CurrentUser;

        var lblWelcome = new Label
        {
            Text = $"Привет, {currentUser.Login}!",
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(300, 30),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        var btnRole = new Button
        {
            Text = "Ваша роль",
            Location = new System.Drawing.Point(100, 70),
            Size = new System.Drawing.Size(140, 35)
        };

        btnRole.Click += (s, e) =>
        {
            lblRole.Text = _roles[_random.Next(_roles.Length)];
            lblStatus.Text = "Роль обновлена!";
        };

        lblRole = new Label
        {
            Location = new System.Drawing.Point(20, 120),
            Size = new System.Drawing.Size(300, 30),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Text = "???"
        };

        var btnDelete = new Button
        {
            Text = "Удалить аккаунт",
            Location = new System.Drawing.Point(100, 170),
            Size = new System.Drawing.Size(140, 35)
        };

        var btnChangeAccount = new Button
        {
            Text = "Сменить аккаунт",
            Location = new System.Drawing.Point(100, 210),
            Size = new System.Drawing.Size(140, 35),
            BackColor = System.Drawing.Color.Orange
        };

        btnChangeAccount.Click += (s, e) =>
        {
            Singleton.GetInstance().Logout(); 
            this.Close();                      
        };

        btnDelete.Click += (s, e) =>
        {
            bool deleted = Singleton.GetInstance().DeleteCurrentUser();
            if (deleted)
            {
                MessageBox.Show("Аккаунт удален");
                this.Close();
            }
        };

        lblStatus = new Label
        {
            Location = new System.Drawing.Point(20, 220),
            Size = new System.Drawing.Size(300, 30),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        this.Controls.AddRange(new Control[]
        {
            lblWelcome,
            btnRole,
            lblRole,
            btnDelete,
            btnChangeAccount,  
            lblStatus
        });

        this.FormClosed += (s, e) => LoginView.Instance?.Show();
    }
}
public partial class LoginView : Form
{
    public static LoginView Instance { get; private set; }

    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;
    private Button btnRegister;
    private Label lblStatus;
    private CheckBox chkAdmin;

    public LoginView()
    {
        Instance = this;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Авторизация (Singleton)";
        this.Size = new System.Drawing.Size(300, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        txtLogin = new TextBox 
        { 
            Location = new System.Drawing.Point(20, 20), 
            Width = 240 
        };

        txtPassword = new TextBox 
        { 
            Location = new System.Drawing.Point(20, 50), 
            Width = 240, 
            UseSystemPasswordChar = true 
        };

        chkAdmin = new CheckBox 
        { 
            Location = new System.Drawing.Point(20, 80), 
            Text = "Админ", 
            Width = 100 
        };

        btnLogin = new Button 
        { 
            Location = new System.Drawing.Point(20, 110), 
            Text = "Войти", 
            Width = 100 
        };

        btnRegister = new Button 
        { 
            Location = new System.Drawing.Point(130, 110), 
            Text = "Регистрация", 
            Width = 100 
        };

        lblStatus = new Label
        {
            Location = new System.Drawing.Point(20, 150),
            Size = new System.Drawing.Size(240, 60),
            ForeColor = System.Drawing.Color.Red
        };

        btnLogin.Click += BtnLogin_Click;
        btnRegister.Click += BtnRegister_Click;

        this.Controls.AddRange(new Control[] 
        { 
            txtLogin, 
            txtPassword, 
            chkAdmin, 
            btnLogin, 
            btnRegister, 
            lblStatus 
        });
    }

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        bool success = Singleton.GetInstance()
            .ValidateLogin(txtLogin.Text, txtPassword.Text, chkAdmin.Checked);

        if (success)
        {
            lblStatus.Text = "Успешный вход!";
            lblStatus.ForeColor = System.Drawing.Color.Green;

            this.Hide();
            var mainView = new MainView();
            mainView.Show();
            mainView.FormClosed += (s, args) => this.Show();
        }
        else
        {
            lblStatus.Text = "Ошибка авторизации";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }
    }

    private void BtnRegister_Click(object sender, EventArgs e)
    {
        bool success = chkAdmin.Checked
            ? Singleton.GetInstance().RegisterAdmin(txtLogin.Text, txtPassword.Text)
            : Singleton.GetInstance().RegisterUser(txtLogin.Text, txtPassword.Text);

        lblStatus.Text = success 
            ? "Регистрация успешна!" 
            : "Пользователь уже существует!";

        lblStatus.ForeColor = success 
            ? System.Drawing.Color.Green 
            : System.Drawing.Color.Red;
    }
}
public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LoginView());
    }
}
