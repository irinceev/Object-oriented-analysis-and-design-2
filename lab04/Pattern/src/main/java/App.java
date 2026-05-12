import interfaces.INotification;
import factory.NotificationFactory;
import client.Notifier;

import javax.swing.*;
import java.awt.*;

public class App extends JFrame {

    private JTextField messageField;
    private JRadioButton tgBtn, emailBtn, consoleBtn;

    public App() {
        setTitle("Notification System");
        setSize(400, 200);
        setDefaultCloseOperation(EXIT_ON_CLOSE);
        setLayout(new FlowLayout());

        add(new JLabel("Message:"));
        messageField = new JTextField(25);
        add(messageField);

        tgBtn = new JRadioButton("Telegram", true);
        emailBtn = new JRadioButton("Email");
        consoleBtn = new JRadioButton("Console");

        ButtonGroup group = new ButtonGroup();
        group.add(tgBtn);
        group.add(emailBtn);
        group.add(consoleBtn);

        add(tgBtn);
        add(emailBtn);
        add(consoleBtn);

        JButton sendBtn = new JButton("Send");
        add(sendBtn);
        sendBtn.addActionListener(e -> sendMessage());
    }

    private void sendMessage() {
        String message = messageField.getText();

        String type;
        if (tgBtn.isSelected())         type = "telegram";
        else if (emailBtn.isSelected()) type = "email";
        else                            type = "console";

        INotification sender = NotificationFactory.create(type);
        Notifier.notify(sender, message);
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new App().setVisible(true));
    }
}