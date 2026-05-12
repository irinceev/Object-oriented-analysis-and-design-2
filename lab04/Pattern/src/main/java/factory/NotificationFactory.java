package factory;

import interfaces.INotification;
import implementations.*;

public class NotificationFactory {
    public static INotification create(String type) {
        switch (type) {
            case "telegram": return new TelegramNotification();
            case "email":    return new EmailNotification();
            case "console":      return new ConsoleNotification();
            default: throw new IllegalArgumentException("Unknown type: " + type);
        }
    }
}