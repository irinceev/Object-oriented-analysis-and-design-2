import javax.swing.*;
import java.awt.*;
import java.net.*;
import java.util.Properties;
import jakarta.mail.*;
import jakarta.mail.Authenticator;
import jakarta.mail.PasswordAuthentication;
import jakarta.mail.internet.InternetAddress;
import jakarta.mail.internet.MimeMessage;

// 1. Отделенный интерфейс
interface INotification {
    void send(String message);
}

// 2. Телега
class TelegramNotification implements INotification {
    private String botToken = "8356898265:AAE6_hADT3UnTdxObiAcCou1jD2WrczqSZE";
    private String chatId = "332931052";

    public void send(String message) {
        try {
            String urlStr = "https://api.telegram.org/bot"
                    + botToken
                    + "/sendMessage?chat_id="
                    + chatId
                    + "&text=" + URLEncoder.encode(message, "UTF-8");

            URL url = new URL(urlStr);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.getInputStream();

            System.out.println("Telegram sent");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}

// 3. EMAIL
class EmailNotification implements INotification {
    private String from = "mx3128@mail.ru";
    private String password = "bQziwI4mLSP1N1i8jNzb"; 
    private String to = "kromanyons@mail.ru";

    public void send(String message) {
        Properties props = new Properties();
        props.put("mail.smtp.host", "smtp.mail.ru");
        props.put("mail.smtp.port", "465");
        props.put("mail.smtp.auth", "true");
        props.put("mail.smtp.ssl.enable", "true");        
        props.put("mail.smtp.ssl.protocols", "TLSv1.2");  
        props.put("mail.smtp.ssl.trust", "smtp.mail.ru"); 

        Session session = Session.getInstance(props, new Authenticator() {
            @Override
            protected PasswordAuthentication getPasswordAuthentication() {
                return new PasswordAuthentication(from, password);
            }
        });

        try {
            Message msg = new MimeMessage(session);
            msg.setFrom(new InternetAddress(from));
            msg.setRecipients(Message.RecipientType.TO, InternetAddress.parse(to));
            msg.setSubject("Всё работает ДАААААААААААААААА");
            msg.setText(message);

            Transport.send(msg);
            System.out.println("Mail.ru email sent");
        } catch (Exception e) {
            System.err.println("Email error: " + e.getMessage());
        }
    }
}

// 4. Консоль
class SMSNotification implements INotification {
    public void send(String message) {
        System.out.println("[Console] " + message);
    }
}

// 5. Клиентская логика
class Notifier {
    public static void notify(INotification sender, String message) {
        sender.send(message);
    }
}
// =========================
// 5. FACTORY
// =========================
class NotificationFactory {
    public static INotification create(String type) {
        switch (type) {
            case "telegram": return new TelegramNotification();
            case "email":    return new EmailNotification();
            case "sms":      return new SMSNotification();
            default: throw new IllegalArgumentException("Unknown type: " + type);
        }
    }
} 

// 6. GUI
public class App extends JFrame {

    private JTextField messageField;
    private JRadioButton tgBtn, emailBtn, smsBtn;

    public App() {
        setTitle("Уведомлятор");
        setSize(400, 200);
        setDefaultCloseOperation(EXIT_ON_CLOSE);
        setLayout(new FlowLayout());

        add(new JLabel("Сообщение:"));
        messageField = new JTextField(25);
        add(messageField);

        tgBtn = new JRadioButton("Телеграм", true);
        emailBtn = new JRadioButton("Email");
        smsBtn = new JRadioButton("СМС");

        ButtonGroup group = new ButtonGroup();
        group.add(tgBtn);
        group.add(emailBtn);
        group.add(smsBtn);

        add(tgBtn);
        add(emailBtn);
        add(smsBtn);

        JButton sendBtn = new JButton("Отослать");
        add(sendBtn);

        sendBtn.addActionListener(e -> sendMessage());
    }

    private void sendMessage() {
        String message = messageField.getText();

        String type;
        if (tgBtn.isSelected())         type = "telegram";
        else if (emailBtn.isSelected()) type = "email";
        else                            type = "sms";

        INotification sender = NotificationFactory.create(type);
        Notifier.notify(sender, message);
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new App().setVisible(true));
    }
}