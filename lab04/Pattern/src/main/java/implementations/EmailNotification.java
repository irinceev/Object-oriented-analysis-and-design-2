package implementations;

import interfaces.INotification;
import jakarta.mail.*;
import jakarta.mail.Authenticator;
import jakarta.mail.PasswordAuthentication;
import jakarta.mail.internet.InternetAddress;
import jakarta.mail.internet.MimeMessage;
import java.util.Properties;

public class EmailNotification implements INotification {
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
            msg.setSubject("Lab 4 Notification");
            msg.setText(message);
            Transport.send(msg);
            System.out.println("Mail.ru email sent");
        } catch (Exception e) {
            System.err.println("Email error: " + e.getMessage());
        }
    }
}